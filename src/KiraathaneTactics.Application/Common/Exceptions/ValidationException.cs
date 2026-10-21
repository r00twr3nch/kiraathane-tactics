using KiraathaneTactics.Application.Common.Models;

namespace KiraathaneTactics.Application.Common.Exceptions;

/// <summary>
/// ValidationBehavior tarafından, FluentValidation kuralları başarısız olduğunda fırlatılır.
/// Her hata kalemi kendi kodunu (ErrorCode), alanını (Field) ve mesajını taşır.
/// Global exception handler bunu doğrudan ApiResponse zarfına yazar.
/// </summary>
public sealed class ValidationException(IReadOnlyList<ApiError> errors)
    : Exception("Bir veya daha fazla doğrulama hatası oluştu.")
{
    public IReadOnlyList<ApiError> Errors { get; } = errors;
}
