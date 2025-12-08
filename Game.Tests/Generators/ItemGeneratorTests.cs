using System.Text.RegularExpressions;
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
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Necklace)]
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
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 10);

        // Assert - check that weapons have names from our JSON data
        foreach (var weapon in weapons)
        {
            weapon.Name.Should().NotBeNullOrWhiteSpace();
            // Weapon names should not be empty and should be actual weapon types
            weapon.Type.Should().Be(ItemType.Weapon);
        }
        
        // At least some weapons should have recognizable weapon words
        var hasWeaponWords = weapons.Any(w => 
            Regex.IsMatch(w.Name, @"(sword|axe|bow|dagger|spear|mace|staff|partisan|halberd|lance|pike|javelin|crossbow|flail|hammer|katana|claymore|rapier|scimitar|trident|glaive|club|maul)", RegexOptions.IgnoreCase));
        hasWeaponWords.Should().BeTrue("weapons should contain recognizable weapon type names");
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
