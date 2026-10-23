using FluentValidation;

namespace KiraathaneTactics.Application.Features.Authentication.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("Auth.Email.Required").WithMessage("E-posta zorunludur.")
            .EmailAddress().WithErrorCode("Auth.Email.Invalid").WithMessage("Geçerli bir e-posta giriniz.")
            .MaximumLength(256).WithErrorCode("Auth.Email.TooLong").WithMessage("E-posta en fazla 256 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("Auth.Password.Required").WithMessage("Parola zorunludur.")
            .MinimumLength(8).WithErrorCode("Auth.Password.TooShort").WithMessage("Parola en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithErrorCode("Auth.Password.MissingUppercase").WithMessage("Parola en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithErrorCode("Auth.Password.MissingLowercase").WithMessage("Parola en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithErrorCode("Auth.Password.MissingDigit").WithMessage("Parola en az bir rakam içermelidir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("Auth.FullName.Required").WithMessage("Ad soyad zorunludur.")
            .MinimumLength(2).WithErrorCode("Auth.FullName.TooShort").WithMessage("Duvar adı en az 2 karakter olmalı.")
            .MaximumLength(24).WithErrorCode("Auth.FullName.TooLong").WithMessage("Duvar adı en fazla 24 karakter olabilir.");
    }
}
