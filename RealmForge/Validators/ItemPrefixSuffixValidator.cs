using FluentValidation;
using RealmForge.Models;

namespace RealmForge.Validators;

/// <summary>
/// Validator for ItemPrefixSuffix model
/// </summary>
public class ItemPrefixSuffixValidator : AbstractValidator<ItemPrefixSuffix>
{
    public ItemPrefixSuffixValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9 ]+$").WithMessage("Name can only contain letters, numbers, and spaces");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display Name is required")
            .MinimumLength(2).WithMessage("Display Name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Display Name must not exceed 50 characters");

        RuleFor(x => x.Rarity)
            .NotEmpty().WithMessage("Rarity is required")
            .Must(BeValidRarity).WithMessage("Rarity must be: common, uncommon, rare, epic, or legendary");

        RuleFor(x => x.Traits)
            .NotNull().WithMessage("Traits collection is required");

        RuleForEach(x => x.Traits)
            .SetValidator(new ItemTraitValidator());
    }

    private bool BeValidRarity(string rarity)
    {
        var validRarities = new[] { "common", "uncommon", "rare", "epic", "legendary" };
        return validRarities.Contains(rarity.ToLower());
    }
}

/// <summary>
/// Validator for ItemTrait model
/// </summary>
public class ItemTraitValidator : AbstractValidator<ItemTrait>
{
    public ItemTraitValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Trait type is required")
            .MinimumLength(2).WithMessage("Trait type must be at least 2 characters");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Trait data type is required")
            .Must(BeValidType).WithMessage("Type must be: number, string, or boolean");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Trait value is required")
            .Must((trait, value) => ValidateValueMatchesType(trait.Type, value))
            .WithMessage("Value must match the specified type");
    }

    private bool BeValidType(string type)
    {
        var validTypes = new[] { "number", "string", "boolean" };
        return validTypes.Contains(type.ToLower());
    }

    private bool ValidateValueMatchesType(string type, object value)
    {
        if (value == null) return false;

        return type.ToLower() switch
        {
            "number" => double.TryParse(value.ToString(), out _) ||
                       int.TryParse(value.ToString(), out _) ||
                       value is double || value is int || value is long || value is float,
            "boolean" => bool.TryParse(value.ToString(), out _) || value is bool,
            "string" => true, // Any value can be a string
            _ => false
        };
    }
}
