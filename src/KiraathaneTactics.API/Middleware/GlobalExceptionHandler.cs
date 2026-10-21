using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using ValidationException = KiraathaneTactics.Application.Common.Exceptions.ValidationException;

namespace KiraathaneTactics.API.Middleware;

/// <summary>
/// Tüm yakalanmamış exception'ları, başarı yanıtlarıyla AYNI <see cref="ApiResponse{T}"/> zarfına çevirir.
/// Böylece istemci (web/mobil) hata durumunda da aynı şekli işler.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "İşlenmemiş hata: {Message}", exception.Message);

        var (statusCode, message, errors) = Map(exception);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse.Fail(statusCode, message, errors), cancellationToken);

        return true;
    }

    private (int StatusCode, string Message, IReadOnlyList<ApiError> Errors) Map(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validation:
                // Her kalem kendi benzersiz kodunu (ValidationBehavior'da) zaten taşır.
                return (StatusCodes.Status400BadRequest,
                    "Bir veya daha fazla doğrulama hatası oluştu.", validation.Errors);

            case DomainException domain:
                // Domain hatası kendi koduyla gelir (örn. "Product.NameRequired").
                return (StatusCodes.Status400BadRequest, domain.Message,
                    [new ApiError(domain.Code, null, domain.Message)]);

            case UnauthorizedAccessException:
                return (StatusCodes.Status401Unauthorized, "Yetkisiz erişim.",
                    [new ApiError("Auth.Unauthorized", null, "Bu işlem için yetkiniz yok.")]);

            default:
                // Beklenmeyen hatada detayı yalnızca Development'ta göster; canlıda gizle.
                var detail = environment.IsDevelopment()
                    ? exception.Message
                    : "Beklenmeyen bir sunucu hatası oluştu.";
                return (StatusCodes.Status500InternalServerError, "Sunucu hatası.",
                    [new ApiError("Server.Unexpected", null, detail)]);
        }
    }
}
