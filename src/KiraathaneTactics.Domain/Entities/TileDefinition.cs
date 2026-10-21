using KiraathaneTactics.Domain.Common;
using KiraathaneTactics.Domain.Exceptions;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>Katalog kartı. Maç içindeki kopyalar bu tanıma bakar; tahta durumu burada tutulmaz.</summary>
public class TileDefinition : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public int Value { get; private set; }
    public int North { get; private set; }
    public int East { get; private set; }
    public int South { get; private set; }
    public int West { get; private set; }

    private TileDefinition() { }

    public TileDefinition(string name, int value, TileEdges edges)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Taş adı boş olamaz.", "Tile.NameRequired");

        if (value is < 1 or > 9)
            throw new DomainException("Taş değeri 1 ile 9 arasında olmalı.", "Tile.ValueOutOfRange");

        Name = name.Trim();
        Value = value;
        North = edges.North;
        East = edges.East;
        South = edges.South;
        West = edges.West;
    }

    public TileEdges Edges => new(North, East, South, West);

    /// <summary>Fincan ağırlığı. Kenarlar aynı kalır; yalnız beraberlikte kullanılan sayı değişir.</summary>
    public void SetWeight(int value)
    {
        if (value is < 1 or > 9)
            throw new DomainException("Taş değeri 1 ile 9 arasında olmalı.", "Tile.ValueOutOfRange");

        Value = value;
    }
}
