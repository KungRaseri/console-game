using FluentValidation;

namespace Game.Features.Combat.Commands.DefendAction;

/// <summary>
/// Validates the DefendAction command.
/// </summary>
public class DefendActionValidator : AbstractValidator<DefendActionCommand>
{
    public DefendActionValidator()
    {
        RuleFor(x => x.Player)
            .NotNull().WithMessage("Player cannot be null");

        RuleFor(x => x.Player.Health)
            .GreaterThan(0).WithMessage("Player must be alive to defend")
            .When(x => x.Player != null);
    }
}
