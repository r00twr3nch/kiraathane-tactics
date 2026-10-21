using Microsoft.Extensions.DependencyInjection;

namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>
/// ISender'ın varsayılan implementasyonu.
/// İlgili handler'ı ve pipeline behavior'larını DI'dan çözer, zinciri kurar ve çalıştırır.
/// (MediatR'a gerek kalmadan, hafif ve bağımlılıksız bir dağıtıcı.)
/// </summary>
public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // 1) İlgili handler'ı çöz
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"'{requestType.Name}' için bir handler kayıtlı değil. Beklenen: {handlerType.Name}");

        var handleMethod = handlerType.GetMethod("Handle")!;

        RequestHandlerDelegate<TResponse> pipeline = () =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        // 2) Behavior'ları tersten sararak zinciri kur (ilk kayıt = en dış halka)
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = serviceProvider.GetServices(behaviorType).Where(b => b is not null).ToArray();
        var behaviorHandleMethod = behaviorType.GetMethod("Handle")!;

        for (var i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i]!;
            var next = pipeline;
            pipeline = () =>
                (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, [request, next, cancellationToken])!;
        }

        return pipeline();
    }
}
