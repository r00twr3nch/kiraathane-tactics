namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>Pipeline zincirinde bir sonraki adımı (handler veya sonraki behavior) temsil eder.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Handler çalışmadan önce/sonra araya giren çapraz-kesit (cross-cutting) davranış.
/// Örn: validation, logging, transaction, caching, performance ölçümü.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
