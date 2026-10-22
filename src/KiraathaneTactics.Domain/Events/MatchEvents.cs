using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Domain.Events;

/// <summary>Phase 2 ses ve animasyon kancalarının okuyacağı düz kayıtlar. Altyapıya bağlı değildir.</summary>
public abstract record MatchEvent;

public sealed record TilePlaced(
    Guid MatchId,
    BoardPosition Position,
    PlayerSide Side,
    bool FaceDown,
    bool Locked) : MatchEvent;

public sealed record TilesFlipped(
    Guid MatchId,
    PlayerSide NewOwner,
    IReadOnlyList<BoardPosition> Positions) : MatchEvent;

public sealed record PerkSpent(
    Guid MatchId,
    PlayerSide Side,
    PerkType Perk,
    int ChargesRemaining) : MatchEvent;

public sealed record ClockWound(
    Guid MatchId,
    PlayerSide Side,
    int PenaltySeconds) : MatchEvent;

public sealed record TileRevealed(
    Guid MatchId,
    BoardPosition Position,
    PlayerSide Side) : MatchEvent;

public sealed record RivalLeft(
    Guid MatchId,
    PlayerSide Winner) : MatchEvent;
