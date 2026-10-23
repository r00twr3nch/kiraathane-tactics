using KiraathaneTactics.Domain.Common;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>
/// İnsan maçından duvara yazılan bir isim. Çaycı masaları buraya girmez.
/// Aynı maçtan her koltuk en fazla bir kez yazılır.
/// </summary>
public sealed class WallEntry : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public DateTimeOffset SignedAt { get; set; }
}
