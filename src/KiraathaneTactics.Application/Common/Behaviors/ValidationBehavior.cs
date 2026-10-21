using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using FluentValidation;
using ValidationException = KiraathaneTactics.Application.Common.Exceptions.ValidationException;

namespace KiraathaneTactics.Application.Common.Behaviors;

/// <summary>
/// İstek handler'a ulaşmadan önce, o isteğe ait tüm FluentValidation kurallarını çalıştırır.
/// Hata varsa ValidationException fırlatır; handler hiç çalışmaz.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            // Her doğrulama hatası kendi benzersiz kodunu taşır.
            // Kod, validator'da .WithErrorCode("...") ile verilir; verilmezse alan bazlı bir koda düşülür.
            var errors = failures
                .Select(f => new ApiError(
                    Code: string.IsNullOrWhiteSpace(f.ErrorCode) ? $"Validation.{f.PropertyName}" : f.ErrorCode,
                    Field: f.PropertyName,
                    Message: f.ErrorMessage))
                .ToList();

            throw new ValidationException(errors);
        }

        return await next();
    }
}
