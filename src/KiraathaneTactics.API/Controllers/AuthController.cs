using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Authentication.Common;
using KiraathaneTactics.Application.Features.Authentication.Login;
using KiraathaneTactics.Application.Features.Authentication.Register;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

/// <summary>
/// Kimlik doğrulama işlemleri: kayıt ve giriş. Başarılı işlemler JWT access token döndürür.
/// </summary>
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Yeni kullanıcı kaydı oluşturur.</summary>
    /// <remarks>
    /// Başarılı kayıttan sonra kullanıcı otomatik giriş yapmış sayılır ve token döner.
    /// Parola kuralları: en az 8 karakter, en az 1 büyük + 1 küçük harf + 1 rakam.
    ///
    /// Örnek istek gövdesi:
    ///
    ///     POST /api/auth/register
    ///     {
    ///        "email": "kullanici@ornek.com",
    ///        "password": "Sifre123",
    ///        "fullName": "Ad Soyad"
    ///     }
    /// </remarks>
    /// <param name="command">Kayıt bilgileri (e-posta, parola, ad soyad).</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Kayıt başarılı; access token döner.</response>
    /// <response code="400">Doğrulama hatası (geçersiz e-posta/parola).</response>
    /// <response code="409">Bu e-posta zaten kayıtlı.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(command, cancellationToken));

    /// <summary>E-posta ve parola ile giriş yapar.</summary>
    /// <remarks>
    /// Dönen <c>accessToken</c> değerini sonraki isteklerde
    /// <c>Authorization: Bearer {token}</c> başlığı ile gönder.
    /// (Swagger'da sağ üstteki <b>Authorize</b> butonunu kullanabilirsin.)
    ///
    /// Örnek istek gövdesi:
    ///
    ///     POST /api/auth/login
    ///     {
    ///        "email": "admin@cleantemplate.local",
    ///        "password": "Admin123!"
    ///     }
    /// </remarks>
    /// <param name="command">Giriş bilgileri (e-posta, parola).</param>
    /// <param name="cancellationToken">İstek iptal jetonu.</param>
    /// <response code="200">Giriş başarılı; access token döner.</response>
    /// <response code="401">E-posta veya parola hatalı.</response>
    /// <response code="403">Hesap pasif durumda.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthenticationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(command, cancellationToken));
}
