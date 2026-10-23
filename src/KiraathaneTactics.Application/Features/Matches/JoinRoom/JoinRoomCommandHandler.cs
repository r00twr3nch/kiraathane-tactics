using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.JoinRoom;

public sealed class JoinRoomCommandHandler(
    IMatchRepository matches,
    IUnitOfWork unitOfWork,
    IMatchNotifier notifier)
    : ICommandHandler<JoinRoomCommand, SeatDto>
{
    public async Task<Result<SeatDto>> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var code = request.RoomCode.Trim().ToUpperInvariant();
        var match = await matches.GetByRoomCodeAsync(code, cancellationToken);
        if (match is null)
            return Result.Failure<SeatDto>(MatchErrors.RoomNotFound(code));

        var token = RoomCodeFactory.SeatToken();
        var outcome = MatchRules.Guard(() => match.SeatDark(token));
        if (outcome.IsFailure)
            return Result.Failure<SeatDto>(outcome.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notifier.PublishAsync(match.ToDto(), cancellationToken);
        var guest = match.HostPlaysDark ? PlayerSide.Light : PlayerSide.Dark;
        return new SeatDto(match.Id, match.RoomCode, guest, token);
    }
}
