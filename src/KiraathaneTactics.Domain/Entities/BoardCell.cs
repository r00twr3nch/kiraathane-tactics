using KiraathaneTactics.Domain.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>Tahtadaki tek kare. Boş karede Owner ve TileDefinitionId yoktur.</summary>
public class BoardCell : BaseEntity
{
    public Guid MatchId { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }
    public PlayerSide? Owner { get; private set; }
    public Guid? TileDefinitionId { get; private set; }
    public int Value { get; private set; }
    public int North { get; private set; }
    public int East { get; private set; }
    public int South { get; private set; }
    public int West { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsFaceDown { get; private set; }

    private BoardCell() { }

    internal BoardCell(Guid matchId, int row, int column)
    {
        MatchId = matchId;
        Row = row;
        Column = column;
    }

    public bool IsEmpty => Owner is null;

    internal void Occupy(
        PlayerSide owner,
        Guid tileDefinitionId,
        int value,
        int north,
        int east,
        int south,
        int west,
        bool locked,
        bool faceDown)
    {
        Owner = owner;
        TileDefinitionId = tileDefinitionId;
        Value = value;
        North = north;
        East = east;
        South = south;
        West = west;
        IsLocked = locked;
        IsFaceDown = faceDown;
    }

    internal void FlipTo(PlayerSide newOwner)
    {
        Owner = newOwner;
    }

    internal void Reveal() => IsFaceDown = false;

    internal int StrengthToward(Direction direction) => direction switch
    {
        Direction.North => North,
        Direction.East => East,
        Direction.South => South,
        Direction.West => West,
        _ => 0
    };
}
