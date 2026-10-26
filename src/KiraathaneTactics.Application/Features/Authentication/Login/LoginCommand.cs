using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Login;

/// <summary>Kullanıcı adı veya e-posta ve parola ile giriş. Başarılıysa JWT access token döner.</summary>
public sealed record LoginCommand : ICommand<AuthenticationResponse>
{
    /// <summary>Kayıtlı kullanıcı adı. Eski hesaplar e-posta da yazabilir.</summary>
    /// <example>Çaycı Nuri</example>
    public string Login { get; init; } = string.Empty;

    /// <summary>Eski istemciler hâlâ e-posta gönderir. Login boşsa bu okunur.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Hesap parolası.</summary>
    /// <example>Admin123!</example>
    public string Password { get; init; } = string.Empty;
}
