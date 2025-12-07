using FluentAssertions;
using Game.Models;
using Game.Data;
using Game.Services;

namespace Game.Tests.Services;

/// <summary>
/// Tests for character creation service.
/// </summary>
public class CharacterCreationTests
{
    [Fact]
    public void CreateCharacter_Should_Apply_Class_Bonuses()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10
        };

        // Act - Create a Warrior (gets +2 STR, +1 CON)
        var character = CharacterCreationService.CreateCharacter("TestWarrior", "Warrior", allocation);

        // Assert
        character.Name.Should().Be("TestWarrior");
        character.ClassName.Should().Be("Warrior");
        character.Strength.Should().Be(12); // 10 + 2 class bonus
        character.Constitution.Should().Be(11); // 10 + 1 class bonus
        character.Dexterity.Should().Be(10); // No bonus
    }

    [Fact]
    public void CreateCharacter_Should_Give_Starting_Equipment()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 12,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 8,
            Wisdom = 8,
            Charisma = 8
        };

        // Act
        var character = CharacterCreationService.CreateCharacter("TestChar", "Warrior", allocation);

        // Assert
        character.Inventory.Should().NotBeEmpty();
        character.Inventory.Should().Contain(i => i.Name.Contains("Sword") || i.Name.Contains("sword"));
        character.Inventory.Should().Contain(i => i.Name.Contains("Shield") || i.Name.Contains("shield"));
    }

    [Fact]
    public void CreateCharacter_Should_Calculate_Health_With_Bonus()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 14,
            Dexterity = 8,
            Constitution = 14, // 14 * 10 + 1 * 5 = 145
            Intelligence = 8,
            Wisdom = 8,
            Charisma = 8
        };

        // Act - Warrior gets +10 health bonus
        var character = CharacterCreationService.CreateCharacter("TestChar", "Warrior", allocation);

        // Assert
        // Base: (CON:15 × 10) + (Level:1 × 5) = 155
        // +10 from Warrior class = 165
        character.MaxHealth.Should().Be(165);
        character.Health.Should().Be(165); // Should be fully healed
    }

    [Fact]
    public void CreateCharacter_Should_Calculate_Mana_With_Bonus()
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 8,
            Dexterity = 8,
            Constitution = 8,
            Intelligence = 14,
            Wisdom = 14, // 14 * 5 + 1 * 3 = 73
            Charisma = 8
        };

        // Act - Mage gets +20 mana bonus
        var character = CharacterCreationService.CreateCharacter("TestChar", "Mage", allocation);

        // Assert
        // Base: (WIS:15 × 5) + (Level:1 × 3) = 78
        // +20 from Mage class = 98
        character.MaxMana.Should().Be(98);
        character.Mana.Should().Be(98); // Should be full
    }

    [Theory]
    [InlineData("Warrior")]
    [InlineData("Rogue")]
    [InlineData("Mage")]
    [InlineData("Cleric")]
    [InlineData("Ranger")]
    [InlineData("Paladin")]
    public void CreateCharacter_Should_Work_For_All_Classes(string className)
    {
        // Arrange
        var allocation = new AttributeAllocation
        {
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10
        };

        // Act
        var character = CharacterCreationService.CreateCharacter("Test", className, allocation);

        // Assert
        character.Should().NotBeNull();
        character.ClassName.Should().Be(className);
        character.Inventory.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCharacter_Should_Give_Starting_Gold()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act
        var character = CharacterCreationService.CreateCharacter("Test", "Warrior", allocation);

        // Assert
        character.Gold.Should().Be(100);
    }

    [Fact]
    public void CreateCharacter_Should_Throw_For_Invalid_Class()
    {
        // Arrange
        var allocation = new AttributeAllocation();

        // Act & Assert
        var act = () => CharacterCreationService.CreateCharacter("Test", "InvalidClass", allocation);
        act.Should().Throw<ArgumentException>().WithMessage("Unknown class: InvalidClass");
    }

    [Fact]
    public void CreateCharacter_Should_Throw_For_Invalid_Allocation()
    {
        // Arrange - spend more than 27 points
        var allocation = new AttributeAllocation
        {
            Strength = 15,
            Dexterity = 15,
            Constitution = 15,
            Intelligence = 15,
            Wisdom = 15,
            Charisma = 15
        };

        // Act & Assert
        var act = () => CharacterCreationService.CreateCharacter("Test", "Warrior", allocation);
        act.Should().Throw<ArgumentException>().WithMessage("*attribute allocation*");
    }

    [Fact]
    public void CreateQuickStartCharacter_Should_Auto_Allocate_Attributes()
    {
        // Act
        var character = CharacterCreationService.CreateQuickStartCharacter("QuickTest", "Warrior");

        // Assert
        character.Should().NotBeNull();
        character.Name.Should().Be("QuickTest");
        character.ClassName.Should().Be("Warrior");
        
        // Warrior's primary attributes (STR, CON) should be higher
        character.Strength.Should().BeGreaterThan(character.Intelligence);
        character.Constitution.Should().BeGreaterThan(character.Wisdom);
    }
}
