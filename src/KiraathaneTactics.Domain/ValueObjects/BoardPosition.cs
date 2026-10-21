using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.Exceptions;

namespace KiraathaneTactics.Domain.ValueObjects;

/// <summary>5×5 tahtada bir kare. Satır ve sütun 0..4, kuzey satır 0'dır.</summary>
public readonly record struct BoardPosition(int Row, int Column)
{
    public const int Size = 5;

    public static BoardPosition Create(int row, int column)
    {
        if (row is < 0 or >= Size || column is < 0 or >= Size)
            throw new DomainException("Taş tahtanın dışına konamaz.", "Match.OutOfBounds");

        return new BoardPosition(row, column);
    }

    public BoardPosition? Neighbor(Direction direction) => direction switch
    {
        Direction.North when Row > 0 => new BoardPosition(Row - 1, Column),
        Direction.East when Column < Size - 1 => new BoardPosition(Row, Column + 1),
        Direction.South when Row < Size - 1 => new BoardPosition(Row + 1, Column),
        Direction.West when Column > 0 => new BoardPosition(Row, Column - 1),
        _ => null
    };

    public override string ToString() => $"({Row},{Column})";
}
