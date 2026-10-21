using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.UsePerk;

/// <summary>Taş koymadan joker. Yalnızca Laf Ebeliği geçerlidir ve saati kısar.</summary>
public sealed record UsePerkCommand : ICommand<MatchStateDto>
{
    public Guid MatchId { get; init; }
    public string? SeatToken { get; init; }
    public PlayerSide Side { get; init; }
    public PerkType Perk { get; init; }
}
