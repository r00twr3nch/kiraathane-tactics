using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Features.Matches.GetState;

public sealed class GetMatchStateQueryHandler(
    IMatchRepository matchRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IMatchNotifier notifier)
    : IQueryHandler<GetMatchStateQuery, MatchStateDto>
{
    public async Task<Result<MatchStateDto>> Handle(GetMatchStateQuery request, CancellationToken cancellationToken)
    {
        var match = await matchRepository.GetWithBoardAsync(request.MatchId, cancellationToken);
        if (match is null)
            return MatchErrors.NotFound(request.MatchId);

        var before = match.Status;
        match.ApplyClock(clock.UtcNow);
        if (match.Status != before)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await notifier.PublishAsync(match.ToDto(clock.UtcNow), cancellationToken);
        }

        return match.ToDto(clock.UtcNow);
    }
}
