namespace KiraathaneTactics.Application.Features.Authentication.Common;

/// <summary>Başarılı kayıt/giriş yanıtı: token ve temel kullanıcı bilgisi.</summary>
/// <param name="UserId">Kullanıcının kimliği.</param>
/// <param name="Email">Kullanıcının e-posta adresi.</param>
/// <param name="FullName">Kullanıcının ad soyadı.</param>
/// <param name="Role">Kullanıcı rolü (User / Admin).</param>
/// <param name="AccessToken">Sonraki isteklerde 'Bearer {token}' olarak gönderilecek JWT.</param>
/// <param name="ExpiresAt">Token'ın son geçerlilik anı (UTC).</param>
public sealed record AuthenticationResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    DateTimeOffset ExpiresAt);
