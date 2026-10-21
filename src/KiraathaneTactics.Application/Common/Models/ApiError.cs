namespace KiraathaneTactics.Application.Common.Models;

/// <summary>Tek bir hata kalemi. İstemci `code` ile programatik, `message` ile kullanıcıya gösterim yapar.</summary>
/// <param name="Code">Makine-okur hata kodu (örn. "Auth.InvalidCredentials", "Validation").</param>
/// <param name="Field">Doğrulama hatasında ilgili alan adı (örn. "email"); diğer hatalarda null.</param>
/// <param name="Message">İnsan-okur hata açıklaması.</param>
public sealed record ApiError(string Code, string? Field, string Message);
