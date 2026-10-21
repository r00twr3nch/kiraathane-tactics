namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>
/// İsteği (Command/Query) ilgili handler'a yönlendiren dağıtıcı.
/// Controller'lar yalnızca bu arayüze bağımlıdır; handler'ları doğrudan tanımaz.
/// </summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
