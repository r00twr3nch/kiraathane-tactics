using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Authentication.Common;

namespace KiraathaneTactics.Application.Features.Authentication.Register;

/// <summary>Yeni kullanıcı kaydı isteği. Başarılıysa otomatik giriş yapıp token döner.</summary>
public sealed record RegisterCommand : ICommand<AuthenticationResponse>
{
    /// <summary>Kullanıcının benzersiz e-posta adresi.</summary>
    /// <example>kullanici@ornek.com</example>
    public string Email { get; init; } = string.Empty;

    /// <summary>Parola: en az 8 karakter, en az bir büyük + bir küçük harf ve bir rakam.</summary>
    /// <example>Sifre123</example>
    public string Password { get; init; } = string.Empty;

    /// <summary>Kullanıcının ad soyadı.</summary>
    /// <example>Ahmet Yılmaz</example>
    public string FullName { get; init; } = string.Empty;
}
