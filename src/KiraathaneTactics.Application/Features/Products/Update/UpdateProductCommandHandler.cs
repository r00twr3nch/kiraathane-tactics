using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Products.Common;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.Update;

public sealed class UpdateProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound(request.Id));

        product.Update(request.Name, request.Description, request.Price, request.StockQuantity);

        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
