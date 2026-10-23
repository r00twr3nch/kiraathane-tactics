using FluentValidation;

namespace KiraathaneTactics.Application.Features.Wall.SignWall;

public sealed class SignWallCommandValidator : AbstractValidator<SignWallCommand>
{
    public SignWallCommandValidator()
    {
        RuleFor(command => command.MatchId).NotEmpty();
        RuleFor(command => command.SeatToken).NotEmpty().MaximumLength(64);
        RuleFor(command => command.Name).MaximumLength(24);
    }
}
