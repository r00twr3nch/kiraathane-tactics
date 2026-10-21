using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Login;

/// <summary>E-posta ve parola ile giriş isteği. Başarılıysa JWT access token döner.</summary>
public sealed record LoginCommand : ICommand<AuthenticationResponse>
{
    /// <summary>Kayıtlı e-posta adresi.</summary>
    /// <example>admin@cleantemplate.local</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>Hesap parolası.</summary>
    /// <example>Admin123!</example>
    public string Password { get; init; } = string.Empty;
}
