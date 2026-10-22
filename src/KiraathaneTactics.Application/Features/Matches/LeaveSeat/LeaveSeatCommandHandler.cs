using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.LeaveSeat;

public sealed class LeaveSeatCommandHandler(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock,
    IMatchNotifier notifier)
    : ICommandHandler<LeaveSeatCommand, MatchStateDto>
{
    public async Task<Result<MatchStateDto>> Handle(LeaveSeatCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SeatToken))
            return Result.Failure<MatchStateDto>(Error.Forbidden("Match.SeatForbidden", "Bu koltuk sana ait değil."));

        var match = await matchRepository.GetWithBoardAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result.Failure<MatchStateDto>(MatchErrors.NotFound(request.MatchId));

        if (match.Status is MatchStatus.Completed or MatchStatus.Draw)
            return match.ToDto(clock.UtcNow);

        PlayerSide? winner = request.SeatToken switch
        {
            var token when token == match.LightSeatToken => PlayerSide.Dark,
            var token when token == match.DarkSeatToken => PlayerSide.Light,
            _ => null
        };

        if (winner is null)
            return Result.Failure<MatchStateDto>(Error.Forbidden("Match.SeatForbidden", "Bu koltuk sana ait değil."));

        // Masa henüz kurulmadıysa ya da karşı koltuk boşsa hükmen galibiyet yok.
        if (match.Status == MatchStatus.Waiting || match.DarkSeatToken is null)
            return match.ToDto(clock.UtcNow);

        var outcome = MatchRules.Guard(() => match.AwardWalkover(winner.Value));
        if (outcome.IsFailure)
            return Result.Failure<MatchStateDto>(outcome.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var state = match.ToDto(clock.UtcNow);
        await notifier.PublishAsync(state, cancellationToken);
        return state;
    }
}
