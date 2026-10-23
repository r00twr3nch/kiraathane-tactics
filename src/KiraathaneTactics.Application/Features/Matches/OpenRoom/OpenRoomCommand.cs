using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.OpenRoom;

public sealed record OpenRoomCommand(PlayerSide? OpeningSide = null, int? MatchLengthSeconds = null, bool HostPlaysDark = false) : ICommand<SeatDto>;
