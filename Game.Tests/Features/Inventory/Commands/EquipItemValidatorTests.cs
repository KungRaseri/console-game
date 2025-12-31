using FluentAssertions;
using Game.Core.Features.Inventory.Commands;
using Game.Shared.Models;

namespace Game.Tests.Features.Inventory.Commands;

public class EquipItemValidatorTests
{
    private readonly EquipItemValidator _validator;

    public EquipItemValidatorTests()
    {
        _validator = new EquipItemValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Player_Is_Null()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = null!,
            Item = new Item { Name = "Sword", Type = ItemType.Weapon }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Player");
    }

    [Fact]
    public void Should_Have_Error_When_Item_Is_Null()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = new Character { Name = "Hero" },
            Item = null!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Item");
    }

    [Fact]
    public void Should_Have_Error_When_Item_Is_Consumable()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = new Character { Name = "Hero" },
            Item = new Item { Name = "Potion", Type = ItemType.Consumable }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Item.Type");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Equipping_Weapon()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = new Character { Name = "Hero" },
            Item = new Item { Name = "Sword", Type = ItemType.Weapon }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Equipping_Armor()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = new Character { Name = "Hero" },
            Item = new Item { Name = "Chainmail", Type = ItemType.Chest }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Equipping_Accessory()
    {
        // Arrange
        var command = new EquipItemCommand
        {
            Player = new Character { Name = "Hero" },
            Item = new Item { Name = "Ring", Type = ItemType.Ring }
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
