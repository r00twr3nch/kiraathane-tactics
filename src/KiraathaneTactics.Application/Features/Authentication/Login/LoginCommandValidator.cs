using FluentValidation;

namespace KiraathaneTactics.Application.Features.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("Auth.Email.Required").WithMessage("E-posta zorunludur.")
            .EmailAddress().WithErrorCode("Auth.Email.Invalid").WithMessage("Geçerli bir e-posta giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("Auth.Password.Required").WithMessage("Parola zorunludur.");
    }
}
