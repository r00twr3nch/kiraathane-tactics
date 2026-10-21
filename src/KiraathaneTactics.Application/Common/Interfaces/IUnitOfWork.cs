namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// Bir iş akışındaki tüm değişiklikleri tek transaction'da kaydeder.
/// Repository'ler değişiklikleri izler; kalıcı hale getirmek için SaveChanges çağrılır.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
