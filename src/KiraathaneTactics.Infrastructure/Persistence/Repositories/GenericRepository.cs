using System.Linq.Expressions;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Infrastructure.Persistence.Repositories;

/// <summary>
/// IGenericRepository'nin EF Core implementasyonu.
/// Yazma işlemleri ChangeTracker'a yazar; kalıcılık IUnitOfWork.SaveChangesAsync ile olur.
/// </summary>
public class GenericRepository<TEntity>(ApplicationDbContext context)
    : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Set.AnyAsync(predicate, cancellationToken);

    // Salt-okuma sorgular için: değişiklik izleme kapalı (performans)
    public IQueryable<TEntity> Query() => Set.AsNoTracking();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);
}
