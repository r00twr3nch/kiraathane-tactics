using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Matches.PlaceTile;

/// <summary>Sırası gelen tarafın bir katalog taşını boş kareye koyması. Joker isteğe bağlıdır.</summary>
public sealed record PlaceTileCommand : ICommand<MatchStateDto>
{
    public Guid MatchId { get; init; }
    public string? SeatToken { get; init; }
    public PlayerSide Side { get; init; }
    public int Row { get; init; }
    public int Column { get; init; }
    public Guid TileDefinitionId { get; init; }
    public PerkType? Perk { get; init; }
}
