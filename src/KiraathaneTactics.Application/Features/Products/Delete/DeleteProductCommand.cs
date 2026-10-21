using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Products.Delete;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
