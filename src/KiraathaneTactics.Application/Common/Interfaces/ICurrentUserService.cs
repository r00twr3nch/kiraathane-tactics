namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// O anki HTTP isteğine ait kullanıcı bilgisini sağlar (JWT claim'lerinden).
/// Implementasyonu API katmanındadır (IHttpContextAccessor).
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
