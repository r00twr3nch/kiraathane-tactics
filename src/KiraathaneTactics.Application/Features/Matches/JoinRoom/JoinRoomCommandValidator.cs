using FluentValidation;

namespace KiraathaneTactics.Application.Features.Matches.JoinRoom;

public sealed class JoinRoomCommandValidator : AbstractValidator<JoinRoomCommand>
{
    public JoinRoomCommandValidator()
    {
        RuleFor(x => x.RoomCode)
            .NotEmpty().WithErrorCode("Match.RoomCodeRequired").WithMessage("Oda kodu zorunludur.")
            .Length(6).WithErrorCode("Match.RoomCodeInvalid").WithMessage("Oda kodu 6 karakter olmalı.");
    }
}
