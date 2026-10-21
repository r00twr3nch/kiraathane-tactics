using FluentValidation;

namespace KiraathaneTactics.Application.Features.Matches.UsePerk;

public sealed class UsePerkCommandValidator : AbstractValidator<UsePerkCommand>
{
    public UsePerkCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithErrorCode("Match.Id.Required").WithMessage("Maç kimliği zorunludur.");

        RuleFor(x => x.Side)
            .IsInEnum().WithErrorCode("Match.Side.Invalid").WithMessage("Taraf Light ya da Dark olmalı.");

        RuleFor(x => x.Perk)
            .IsInEnum().WithErrorCode("Match.Perk.Invalid").WithMessage("Bilinmeyen joker.");
    }
}
