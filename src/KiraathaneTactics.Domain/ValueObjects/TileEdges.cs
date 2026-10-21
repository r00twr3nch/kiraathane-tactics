using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.Exceptions;

namespace KiraathaneTactics.Domain.ValueObjects;

/// <summary>
/// Dört kenarın gücü. 0 o kenarda ok olmadığı anlamına gelir ve ele geçiremez.
/// Karşı kenar: kuzeyin karşısı güneydir.
/// </summary>
public readonly record struct TileEdges(int North, int East, int South, int West)
{
    public static TileEdges Create(int north, int east, int south, int west)
    {
        Validate(north);
        Validate(east);
        Validate(south);
        Validate(west);
        return new TileEdges(north, east, south, west);
    }

    public int Strength(Direction direction) => direction switch
    {
        Direction.North => North,
        Direction.East => East,
        Direction.South => South,
        Direction.West => West,
        _ => throw new DomainException("Bilinmeyen yön.", "Tile.UnknownDirection")
    };

    public static Direction Opposite(Direction direction) => direction switch
    {
        Direction.North => Direction.South,
        Direction.East => Direction.West,
        Direction.South => Direction.North,
        Direction.West => Direction.East,
        _ => throw new DomainException("Bilinmeyen yön.", "Tile.UnknownDirection")
    };

    private static void Validate(int value)
    {
        if (value is < 0 or > 9)
            throw new DomainException("Kenar gücü 0 ile 9 arasında olmalı.", "Tile.EdgeOutOfRange");
    }
}
