using FluentValidation;

namespace KiraathaneTactics.Application.Features.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Login) || !string.IsNullOrWhiteSpace(x.Email))
            .WithErrorCode("Auth.Login.Required").WithMessage("Kullanıcı adı zorunludur.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("Auth.Password.Required").WithMessage("Parola zorunludur.");
    }
}
