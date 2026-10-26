using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Application.Features.Matches.GetState;
using Microsoft.EntityFrameworkCore;
using KiraathaneTactics.Application.Features.Matches.JoinRoom;
using KiraathaneTactics.Application.Features.Matches.PlaceTile;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.API.Services;

/// <summary>
/// Boş masa aramasını uzatmamak için kuyruğa oturan kalfa.
/// Hamle seçimi çaycıyla aynıdır; yalnız bu botun koltuk anahtarı sunucuda durur.
/// </summary>
public sealed class QueueBotPlayer(
    IServiceScopeFactory scopes,
    ILogger<QueueBotPlayer> logger)
{
    public async Task SitAndPlayAsync(string roomCode, CancellationToken cancellationToken)
    {
        // Her hamle kendi kapsamındadır. Tek kapsam bütün maçı izlerse,
        // rakibin taşı veritabanına yazılsa bile bot eski sırayı görmeye devam eder.
        var joined = await SendCommand(new JoinRoomCommand(roomCode), cancellationToken);
        if (joined.IsFailure)
        {
            logger.LogInformation("Kuyruk botu masaya oturamadı: {Reason}", joined.Error.Description);
            return;
        }

        var seat = joined.Value;
        var catalog = await LoadCatalog(cancellationToken);
        logger.LogInformation(
            "Kuyruk botu {Room} masasında {Side} olarak oturdu.",
            seat.RoomCode, seat.Side);

        while (!cancellationToken.IsCancellationRequested)
        {
            var state = await SendQuery(new GetMatchStateQuery(seat.MatchId), cancellationToken);
            if (state.IsFailure)
            {
                logger.LogInformation("Kuyruk botu masayı göremedi: {Reason}", state.Error.Description);
                return;
            }

            var match = state.Value;
            if (match.Status is not MatchStatus.InProgress)
            {
                logger.LogInformation("Kuyruk botu masadan kalktı: {Status}", match.Status);
                return;
            }

            if (match.CurrentTurn != seat.Side)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken);
                continue;
            }

            var choice = Choose(match, catalog, seat.Side);
            if (choice is null)
            {
                logger.LogInformation("Kuyruk botunun koyacak taşı kalmadı.");
                await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken);
                continue;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(700), cancellationToken);
            var placed = await SendCommand(new PlaceTileCommand
            {
                MatchId = seat.MatchId,
                SeatToken = seat.SeatToken,
                Side = seat.Side,
                Row = choice.Value.Row,
                Column = choice.Value.Column,
                TileDefinitionId = choice.Value.TileId
            }, cancellationToken);

            if (placed.IsFailure)
            {
                logger.LogInformation("Kuyruk botu taşı koyamadı: {Reason}", placed.Error.Description);
                await Task.Delay(TimeSpan.FromMilliseconds(400), cancellationToken);
                continue;
            }

            logger.LogInformation(
                "Kuyruk botu taş koydu ({Row},{Column}). Sıra {Turn}.",
                choice.Value.Row, choice.Value.Column, placed.Value.CurrentTurn);
        }
    }

    private async Task<Result<T>> SendCommand<T>(ICommand<T> command, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(command, cancellationToken);
    }

    private async Task<Result<T>> SendQuery<T>(IQuery<T> query, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(query, cancellationToken);
    }

    private async Task<List<TileDefinition>> LoadCatalog(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var tiles = scope.ServiceProvider.GetRequiredService<IGenericRepository<TileDefinition>>();
        return await tiles.Query().ToListAsync(cancellationToken);
    }

    private static (Guid TileId, int Row, int Column)? Choose(
        MatchStateDto match,
        IReadOnlyList<TileDefinition> catalog,
        PlayerSide side)
    {
        var empty = match.Cells.Where(cell => cell.Owner is null).ToList();
        var hand = catalog.Where(tile => CopiesLeft(match, side, tile.Id) > 0).ToList();
        if (empty.Count == 0 || hand.Count == 0)
            return null;

        var best = hand
            .SelectMany(tile => empty.Select(spot => new
            {
                tile.Id,
                spot.Row,
                spot.Column,
                Captured = Captures(match, tile, spot.Row, spot.Column, side)
            }))
            .GroupBy(move => move.Captured)
            .OrderByDescending(group => group.Key)
            .First()
            .ToList();

        var pick = best[Random.Shared.Next(best.Count)];
        return (pick.Id, pick.Row, pick.Column);
    }

    private static int CopiesLeft(
        MatchStateDto match,
        PlayerSide side,
        Guid tileId)
    {
        var stock = match.Stock.FirstOrDefault(row => row.TileDefinitionId == tileId);
        if (stock is null)
            return Match.CopiesPerTile;
        return side == PlayerSide.Light ? stock.LightLeft : stock.DarkLeft;
    }

    private static int Captures(
        MatchStateDto match,
        TileDefinition tile,
        int row,
        int column,
        PlayerSide side)
    {
        return Neighbors(match, row, column).Count(neighbor =>
        {
            var cell = neighbor.Cell;
            if (cell?.Owner is null || cell.Owner == side || cell.IsLocked || cell.IsFaceDown)
                return false;

            var attack = Edge(tile, neighbor.Attack);
            var defense = Edge(cell, neighbor.Defense);
            return attack > defense || (attack == defense && attack > 0 && tile.Value > cell.Value);
        });
    }

    private static int Edge(TileDefinition tile, Direction direction) => direction switch
    {
        Direction.North => tile.North,
        Direction.East => tile.East,
        Direction.South => tile.South,
        Direction.West => tile.West,
        _ => 0
    };

    private static int Edge(CellDto cell, Direction direction) => direction switch
    {
        Direction.North => cell.North,
        Direction.East => cell.East,
        Direction.South => cell.South,
        Direction.West => cell.West,
        _ => 0
    };

    private static IEnumerable<(CellDto? Cell, Direction Attack, Direction Defense)> Neighbors(
        MatchStateDto match,
        int row,
        int column)
    {
        (Direction Attack, Direction Defense, int Row, int Column)[] around =
        [
            (Direction.North, Direction.South, row - 1, column),
            (Direction.East, Direction.West, row, column + 1),
            (Direction.South, Direction.North, row + 1, column),
            (Direction.West, Direction.East, row, column - 1)
        ];

        foreach (var spot in around)
        {
            var cell = spot.Row is < 0 or > 4 || spot.Column is < 0 or > 4
                ? null
                : match.Cells.FirstOrDefault(item => item.Row == spot.Row && item.Column == spot.Column);
            yield return (cell, spot.Attack, spot.Defense);
        }
    }
}
