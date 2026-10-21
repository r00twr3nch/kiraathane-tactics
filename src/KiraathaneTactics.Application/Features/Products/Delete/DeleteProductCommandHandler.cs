using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Products.Common;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.Delete;

public sealed class DeleteProductCommandHandler(
    IGenericRepository<Product> productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound(request.Id));

        // Soft delete: AuditableEntity ISoftDeletable olduğu için DbContext bunu IsDeleted=true'ya çevirir.
        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
