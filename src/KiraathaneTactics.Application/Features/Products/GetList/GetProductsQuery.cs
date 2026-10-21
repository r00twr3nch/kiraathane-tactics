using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Products.Common;

namespace KiraathaneTactics.Application.Features.Products.GetList;

/// <summary>
/// Ürünleri sayfalı şekilde, isme göre opsiyonel arama ile listeler.
/// Sayfalama alanları (PageNumber, PageSize) <see cref="PaginatedRequest"/> tabanından gelir.
/// </summary>
public sealed record GetProductsQuery : PaginatedRequest, IQuery<PagedResult<ProductDto>>
{
    /// <summary>Ürün adında geçen metne göre filtreler (opsiyonel).</summary>
    /// <example>laptop</example>
    public string? Search { get; init; }
}
