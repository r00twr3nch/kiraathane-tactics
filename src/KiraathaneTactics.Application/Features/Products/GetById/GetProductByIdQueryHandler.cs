using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Products.Common;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.GetById;

public sealed class GetProductByIdQueryHandler(IGenericRepository<Product> productRepository)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);

        return product is null
            ? ProductErrors.NotFound(request.Id)
            : product.ToDto();
    }
}
