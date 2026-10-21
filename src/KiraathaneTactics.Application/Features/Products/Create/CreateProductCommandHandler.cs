using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.Create;

public sealed class CreateProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
