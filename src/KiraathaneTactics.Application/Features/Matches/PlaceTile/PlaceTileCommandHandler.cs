using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Application.Features.Matches.PlaceTile;

public sealed class PlaceTileCommandHandler(
    IMatchRepository matchRepository,
    IGenericRepository<TileDefinition> tileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IMatchNotifier notifier)
    : ICommandHandler<PlaceTileCommand, MatchStateDto>
{
    public async Task<Result<MatchStateDto>> Handle(PlaceTileCommand request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetWithBoardAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result.Failure<MatchStateDto>(MatchErrors.NotFound(request.MatchId));

        if (!OwnsSeat(match, request.Side, request.SeatToken))
            return Result.Failure<MatchStateDto>(Error.Forbidden("Match.SeatForbidden", "Bu koltuk sana ait değil."));

        var tile = await tileRepository.GetByIdAsync(request.TileDefinitionId, cancellationToken);
        if (tile is null)
            return Result.Failure<MatchStateDto>(MatchErrors.TileNotFound(request.TileDefinitionId));

        var outcome = MatchRules.Guard(() =>
        {
            match.ApplyClock(clock.UtcNow);
            match.PlaceTile(request.Side, BoardPosition.Create(request.Row, request.Column), tile, request.Perk);
        });

        if (outcome.IsFailure)
            return Result.Failure<MatchStateDto>(outcome.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var state = match.ToDto(clock.UtcNow);
        await notifier.PublishAsync(state, cancellationToken);
        return state;
    }

    private static bool OwnsSeat(Domain.Entities.Match match, Domain.Enums.PlayerSide side, string? token) =>
        string.IsNullOrWhiteSpace(token)
        || token == (side == Domain.Enums.PlayerSide.Light ? match.LightSeatToken : match.DarkSeatToken);
}
