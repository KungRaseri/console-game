using FluentValidation;

namespace Game.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Validator for CreateCharacter command.
/// </summary>
public class CreateCharacterValidator : AbstractValidator<CreateCharacterCommand>
{
    public CreateCharacterValidator()
    {
        RuleFor(x => x.PlayerName)
            .NotEmpty().WithMessage("Player name is required")
            .Length(1, 30).WithMessage("Player name must be between 1 and 30 characters")
            .Matches("^[a-zA-Z][a-zA-Z0-9 ]*$").WithMessage("Player name must start with a letter and contain only letters, numbers, and spaces");

        RuleFor(x => x.ClassName)
            .NotEmpty().WithMessage("Class name is required");

        RuleFor(x => x.AttributeAllocation)
            .NotNull().WithMessage("Attribute allocation is required")
            .Must(x => x.GetPointsSpent() == 27).WithMessage("All 27 attribute points must be allocated");
    }
}
