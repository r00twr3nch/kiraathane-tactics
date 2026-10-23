using FluentValidation;

namespace KiraathaneTactics.Application.Features.Authentication.Rename;

public sealed class RenameCommandValidator : AbstractValidator<RenameCommand>
{
    public RenameCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("Auth.FullName.Required").WithMessage("Duvar adı zorunludur.")
            .MinimumLength(2).WithErrorCode("Auth.FullName.TooShort").WithMessage("Duvar adı en az 2 karakter olmalı.")
            .MaximumLength(24).WithErrorCode("Auth.FullName.TooLong").WithMessage("Duvar adı en fazla 24 karakter olabilir.");
    }
}
