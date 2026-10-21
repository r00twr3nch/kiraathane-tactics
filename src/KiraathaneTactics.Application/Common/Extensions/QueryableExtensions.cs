using KiraathaneTactics.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Bir IQueryable'ı veritabanı tarafında sayfalayıp PagedResult'a dönüştürür.
    /// TotalCount ayrı bir COUNT sorgusuyla, sayfa verisi tek sorguyla çekilir.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
