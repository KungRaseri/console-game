using FluentValidation.TestHelper;
using RealmEngine.Core.Features.Combat.Commands.UseCombatItem;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Tests.Features.Combat.Validators;

[Trait("Category", "Feature")]
/// <summary>
/// Tests for UseCombatItemValidator.
/// </summary>
public class UseCombatItemValidatorTests
{
    private readonly UseCombatItemValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Player_Is_Null()
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = null!,
            Item = new Item { Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player)
            .WithErrorMessage("Player cannot be null");
    }

    [Fact]
    public void Should_Have_Error_When_Player_Health_Is_Zero()
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = 0, MaxHealth = 100 },
            Item = new Item { Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Player.Health)
            .WithErrorMessage("Player must be alive to use items");
    }

    [Fact]
    public void Should_Have_Error_When_Item_Is_Null()
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Item = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Item)
            .WithErrorMessage("Item cannot be null");
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Chest)]
    [InlineData(ItemType.Ring)]
    public void Should_Have_Error_When_Item_Is_Not_Consumable(ItemType itemType)
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = 100, MaxHealth = 100 },
            Item = new Item { Type = itemType }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Item.Type)
            .WithErrorMessage("Only consumable items can be used in combat");
    }

    [Fact]
    public void Should_Pass_When_Using_Consumable_Item()
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = 50, MaxHealth = 100 },
            Item = new Item { Name = "Health Potion", Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Player_Is_Alive_And_Item_Is_Consumable()
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = 1, MaxHealth = 100 },
            Item = new Item { Name = "Mana Potion", Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    public void Should_Pass_When_Player_Health_Is_Positive(int health)
    {
        // Arrange
        var command = new UseCombatItemCommand
        {
            Player = new Character { Health = health, MaxHealth = 100 },
            Item = new Item { Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Player.Health);
    }
}
