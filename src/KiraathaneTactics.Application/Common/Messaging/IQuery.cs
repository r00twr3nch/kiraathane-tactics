using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>
/// Veriyi okuyan, durumu değiştirmeyen istek. Daima bir değer döndürür (Result&lt;TResponse&gt;).
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
