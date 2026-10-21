namespace KiraathaneTactics.Domain.Common;

/// <summary>
/// Tüm entity'lerin türediği temel sınıf. Birincil anahtar (Id) burada tanımlıdır.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
