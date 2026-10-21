using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Features.Matches.JoinRoom;

public sealed record JoinRoomCommand(string RoomCode) : ICommand<SeatDto>;
