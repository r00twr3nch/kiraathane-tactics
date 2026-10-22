using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.Events;

namespace KiraathaneTactics.Application.Features.Matches.Common;

public sealed record CellDto(
    int Row,
    int Column,
    PlayerSide? Owner,
    Guid? TileDefinitionId,
    int Value,
    int North,
    int East,
    int South,
    int West,
    bool IsLocked,
    bool IsFaceDown);

public sealed record MatchEventDto(string Kind, string Detail);

public sealed record SeatDto(Guid MatchId, string RoomCode, PlayerSide Side, string SeatToken);

public sealed record TileStockDto(Guid TileDefinitionId, int LightLeft, int DarkLeft);

public sealed record MatchStateDto(
    Guid Id,
    MatchStatus Status,
    PlayerSide CurrentTurn,
    PlayerSide? Winner,
    int LightScore,
    int DarkScore,
    int LightPerkCharges,
    int DarkPerkCharges,
        string RoomCode,
        int LightSecondsRemaining,
        int DarkSecondsRemaining,
        DateTimeOffset? TurnDeadlineUtc,
        IReadOnlyList<CellDto> Cells,
        IReadOnlyList<MatchEventDto> Events,
        IReadOnlyList<TileStockDto> Stock);

public static class MatchMapping
{
    public static MatchStateDto ToDto(this Match match, DateTimeOffset? utcNow = null)
    {
        var now = utcNow ?? DateTimeOffset.UtcNow;
        return new(
        match.Id,
        match.Status,
        match.CurrentTurn,
        match.Winner,
        match.Score(PlayerSide.Light),
        match.Score(PlayerSide.Dark),
        match.LightPerkCharges,
        match.DarkPerkCharges,
                match.RoomCode,
                match.SecondsRemaining(PlayerSide.Light, now),
                match.SecondsRemaining(PlayerSide.Dark, now),
                match.TurnDeadlineUtc,
                match.Cells
            .OrderBy(c => c.Row)
            .ThenBy(c => c.Column)
            .Select(c => new CellDto(
                c.Row,
                c.Column,
                c.Owner,
                c.TileDefinitionId,
                c.Value,
                c.North,
                c.East,
                c.South,
                c.West,
                c.IsLocked,
                c.IsFaceDown))
            .ToList(),
        match.DomainEvents.Select(ToEventDto).ToList(),
        match.Cells
            .Where(c => c.TileDefinitionId is not null && c.PlacedBy is not null)
            .Select(c => c.TileDefinitionId!.Value)
            .Distinct()
            .Select(id => new TileStockDto(
                id,
                match.CopiesLeft(PlayerSide.Light, id),
                match.CopiesLeft(PlayerSide.Dark, id)))
            .ToList());
    }

    private static MatchEventDto ToEventDto(MatchEvent domainEvent) => domainEvent switch
    {
        TilePlaced placed => new MatchEventDto(
            "TilePlaced",
            $"{placed.Side} ({placed.Position.Row},{placed.Position.Column}) faceDown={placed.FaceDown} locked={placed.Locked}"),
        TilesFlipped flipped => new MatchEventDto(
            "TilesFlipped",
            $"{flipped.NewOwner} {string.Join(" ", flipped.Positions.Select(p => $"({p.Row},{p.Column})"))}"),
        PerkSpent spent => new MatchEventDto(
            "PerkSpent",
            $"{spent.Side} {spent.Perk} remaining={spent.ChargesRemaining}"),
                ClockWound wound => new MatchEventDto(
                    "ClockWound",
                    $"{wound.Side} penalty={wound.PenaltySeconds}"),
                TileRevealed revealed => new MatchEventDto(
                    "TileRevealed",
                    $"{revealed.Side} ({revealed.Position.Row},{revealed.Position.Column})"),
                _ => new MatchEventDto(domainEvent.GetType().Name, string.Empty)
    };
}
