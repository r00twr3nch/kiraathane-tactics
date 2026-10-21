namespace KiraathaneTactics.Application.Common.Messaging;

/// <summary>
/// Bir yanıt (TResponse) döndüren istek. Command ve Query'lerin ortak tabanı.
/// </summary>
public interface IRequest<out TResponse>;
