using FluentAssertions;
using Game.Generators;
using Game.Models;
using Xunit;

namespace Game.Tests.Generators;

public class ItemGeneratorTests
{
    [Fact]
    public void Generate_Should_Create_Valid_Item()
    {
        // Act
        var item = ItemGenerator.Generate();

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBeNullOrEmpty();
        item.Name.Should().NotBeNullOrEmpty();
        item.Description.Should().NotBeNullOrEmpty();
        item.Price.Should().BeInRange(5, 1000);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Generate_Should_Create_Requested_Number_Of_Items(int count)
    {
        // Act
        var items = ItemGenerator.Generate(count);

        // Assert
        items.Should().HaveCount(count);
        items.Should().OnlyHaveUniqueItems(i => i.Id);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Armor)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Accessory)]
    [InlineData(ItemType.QuestItem)]
    public void GenerateByType_Should_Create_Items_Of_Specified_Type(ItemType type)
    {
        // Act
        var items = ItemGenerator.GenerateByType(type, 5);

        // Assert
        items.Should().HaveCount(5);
        items.Should().OnlyContain(i => i.Type == type);
    }

    [Fact]
    public void GenerateByType_Weapon_Should_Have_Weapon_Names()
    {
        // Act
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 5);

        // Assert
        foreach (var weapon in weapons)
        {
            weapon.Name.Should().MatchRegex("(Iron|Steel|Mythril|Dragon) (Sword|Axe|Bow|Dagger)");
        }
    }

    [Fact]
    public void GenerateByType_Consumable_Should_Have_Potion_Names()
    {
        // Act
        var potions = ItemGenerator.GenerateByType(ItemType.Consumable, 5);

        // Assert
        foreach (var potion in potions)
        {
            potion.Name.Should().Contain("Potion");
        }
    }
}
