using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.OpenRoom;

public sealed class OpenRoomCommandHandler(
    IMatchRepository matches,
    IUnitOfWork unitOfWork)
    : ICommandHandler<OpenRoomCommand, SeatDto>
{
    public async Task<Result<SeatDto>> Handle(OpenRoomCommand request, CancellationToken cancellationToken)
    {
        var match = Match.OpenWaitingRoom(
            RoomCodeFactory.Create(),
            RoomCodeFactory.SeatToken(),
            request.OpeningSide,
            request.MatchLengthSeconds,
            request.HostPlaysDark);
        await matches.AddAsync(match, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var side = request.HostPlaysDark ? PlayerSide.Dark : PlayerSide.Light;
        var token = request.HostPlaysDark ? match.DarkSeatToken! : match.LightSeatToken!;
        return new SeatDto(match.Id, match.RoomCode, side, token);
    }
}
