using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Products.Common;

namespace KiraathaneTactics.Application.Features.Products.GetById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
