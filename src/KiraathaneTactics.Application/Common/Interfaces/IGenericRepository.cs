using System.Linq.Expressions;
using KiraathaneTactics.Domain.Common;

namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// Tüm entity'ler için ortak veri erişim sözleşmesi.
/// Implementasyonu Infrastructure katmanındadır (EF Core).
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Filtreleme/sıralama/sayfalama için ham sorgu. AsNoTracking ile gelir.</summary>
    IQueryable<TEntity> Query();

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
