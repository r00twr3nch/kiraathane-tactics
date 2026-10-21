using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>
/// Sistemin durumunu değiştiren istek (yazma). Değer döndürmez, yalnızca Result döner.
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Sistemin durumunu değiştiren ve bir değer (TResponse) döndüren istek.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
