using KiraathaneTactics.Application.Common.Extensions;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Products.Common;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.GetList;

public sealed class GetProductsQueryHandler(IGenericRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = productRepository.Query();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        // Projeksiyon veritabanı tarafında: yalnızca gereken kolonlar çekilir.
        var projected = query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductDto(
                p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CreatedAt));

        return await projected.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
