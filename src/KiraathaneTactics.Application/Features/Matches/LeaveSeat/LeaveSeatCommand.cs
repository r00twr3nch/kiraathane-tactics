using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Features.Matches.LeaveSeat;

/// <summary>Online koltuk kapıyı çekti. Kalan taraf hükmen kazanır.</summary>
public sealed record LeaveSeatCommand(Guid MatchId, string SeatToken) : ICommand<MatchStateDto>;
