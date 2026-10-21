using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KiraathaneTactics.Infrastructure.Persistence.Interceptors;

/// <summary>
/// SaveChanges sırasında otomatik olarak:
///  - Audit alanlarını (CreatedAt/By, UpdatedAt/By) doldurur,
///  - Soft-delete'i (fiziksel silme yerine IsDeleted=true) uygular.
/// </summary>
public sealed class AuditableEntityInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider)
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditInformation(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ApplyAuditInformation(DbContext? context)
    {
        if (context is null)
            return;

        var now = dateTimeProvider.UtcNow;
        var userId = currentUserService.UserId?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.CreatedBy = userId;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = userId;
                        break;
                }
            }

            // Soft delete: hard delete'i modified'e çevir
            if (entry is { State: EntityState.Deleted, Entity: ISoftDeletable softDeletable })
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
            }
        }
    }
}
