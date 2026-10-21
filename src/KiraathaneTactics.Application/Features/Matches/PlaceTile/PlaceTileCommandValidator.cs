using FluentValidation;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Application.Features.Matches.PlaceTile;

public sealed class PlaceTileCommandValidator : AbstractValidator<PlaceTileCommand>
{
    public PlaceTileCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithErrorCode("Match.Id.Required").WithMessage("Maç kimliği zorunludur.");

        RuleFor(x => x.TileDefinitionId)
            .NotEmpty().WithErrorCode("Tile.Id.Required").WithMessage("Taş tanımı zorunludur.");

        RuleFor(x => x.Side)
            .IsInEnum().WithErrorCode("Match.Side.Invalid").WithMessage("Taraf Light ya da Dark olmalı.");

        RuleFor(x => x.Row)
            .InclusiveBetween(0, BoardPosition.Size - 1)
            .WithErrorCode("Match.OutOfBounds")
            .WithMessage("Satır 0 ile 4 arasında olmalı.");

        RuleFor(x => x.Column)
            .InclusiveBetween(0, BoardPosition.Size - 1)
            .WithErrorCode("Match.OutOfBounds")
            .WithMessage("Sütun 0 ile 4 arasında olmalı.");

        RuleFor(x => x.Perk)
            .Must(perk => perk is null || perk != PerkType.LafEbeligi)
            .WithErrorCode("Match.PerkRequiresPlacement")
            .WithMessage("Laf Ebeliği taş koyarken kullanılmaz.");
    }
}
