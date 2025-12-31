using FluentValidation;
using Game.Shared.Models;

namespace Game.Core.Features.Inventory.Commands;

/// <summary>
/// Validates the EquipItem command.
/// </summary>
public class EquipItemValidator : AbstractValidator<EquipItemCommand>
{
    public EquipItemValidator()
    {
        RuleFor(x => x.Player)
            .NotNull().WithMessage("Player cannot be null");

        RuleFor(x => x.Item)
            .NotNull().WithMessage("Item cannot be null");

        RuleFor(x => x.Item.Type)
            .NotEqual(ItemType.Consumable).WithMessage("Cannot equip consumable items")
            .When(x => x.Item != null);
    }
}