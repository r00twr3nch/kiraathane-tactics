using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Features.Matches.UsePerk;

public sealed class UsePerkCommandHandler(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IMatchNotifier notifier)
    : ICommandHandler<UsePerkCommand, MatchStateDto>
{
    public async Task<Result<MatchStateDto>> Handle(UsePerkCommand request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetWithBoardAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result.Failure<MatchStateDto>(MatchErrors.NotFound(request.MatchId));

        var expected = request.Side == Domain.Enums.PlayerSide.Light ? match.LightSeatToken : match.DarkSeatToken;
        if (!string.IsNullOrWhiteSpace(request.SeatToken) && request.SeatToken != expected)
            return Result.Failure<MatchStateDto>(Error.Forbidden("Match.SeatForbidden", "Bu koltuk sana ait değil."));

        var outcome = MatchRules.Guard(() =>
        {
            match.ApplyClock(clock.UtcNow);
            match.UsePerk(request.Side, request.Perk);
        });

        if (outcome.IsFailure)
            return Result.Failure<MatchStateDto>(outcome.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var state = match.ToDto(clock.UtcNow);
        await notifier.PublishAsync(state, cancellationToken);
        return state;
    }
}
