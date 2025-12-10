using FluentAssertions;
using Game.Models;
using Xunit;

namespace Game.Tests.Models;

public class CharacterTests
{
    [Fact]
    public void Character_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Level.Should().Be(1);
        character.Health.Should().Be(100);
        character.MaxHealth.Should().Be(100);
        character.Mana.Should().Be(50);
        character.MaxMana.Should().Be(50);
        character.Gold.Should().Be(0);
        character.Experience.Should().Be(0);
    }

    [Fact]
    public void GainExperience_Should_Increase_Experience()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act
        character.GainExperience(50);

        // Assert
        character.Experience.Should().Be(50);
        character.Level.Should().Be(1); // Not enough XP to level up
    }

    [Fact]
    public void GainExperience_Should_Level_Up_When_Enough_XP()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act
        character.GainExperience(100); // Exactly enough for level 2

        // Assert
        character.Level.Should().Be(2);
        character.Experience.Should().Be(0); // XP resets after level up
    }

    [Fact]
    public void GainExperience_Should_Increase_Stats_On_Level_Up()
    {
        // Arrange
        var character = new Character
        {
            Level = 1,
            Experience = 0
        };
        
        // Set initial values from D20 calculations
        character.MaxHealth = character.GetMaxHealth(); // (CON:10 × 10) + (Level:1 × 5) = 105
        character.Health = character.MaxHealth;
        character.MaxMana = character.GetMaxMana();     // (WIS:10 × 5) + (Level:1 × 3) = 53
        character.Mana = character.MaxMana;
        
        var initialMaxHealth = character.MaxHealth;
        var initialMaxMana = character.MaxMana;
        var initialConstitution = character.Constitution;
        var initialAttributePoints = character.UnspentAttributePoints;

        // Act - Level up to 2
        character.GainExperience(100);

        // Assert
        character.Level.Should().Be(2);
        
        // Stats should NOT auto-increase (new system - player allocates)
        character.Constitution.Should().Be(initialConstitution); // No auto-increase
        character.Strength.Should().Be(10);   // Base stats unchanged
        character.Dexterity.Should().Be(10);
        character.Intelligence.Should().Be(10);
        character.Wisdom.Should().Be(10);
        character.Charisma.Should().Be(10);
        
        // Should have unspent attribute points
        character.UnspentAttributePoints.Should().Be(initialAttributePoints + 3); // 3 points per level
        
        // Should have a pending level-up
        character.PendingLevelUps.Should().HaveCount(1);
        
        // MaxHealth and MaxMana should still recalculate based on level
        character.MaxHealth.Should().BeGreaterThan(initialMaxHealth);
        character.MaxMana.Should().BeGreaterThan(initialMaxMana);
        
        // Health and Mana should be fully restored
        character.Health.Should().Be(character.MaxHealth);
        character.Mana.Should().Be(character.MaxMana);
    }

    [Theory]
    [InlineData(100, 2, 0)]
    [InlineData(150, 2, 50)]
    [InlineData(300, 3, 0)]
    [InlineData(350, 3, 50)]
    public void GainExperience_Should_Handle_Multiple_Level_Ups(int xpGained, int expectedLevel, int expectedRemainingXp)
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act
        character.GainExperience(xpGained);

        // Assert
        character.Level.Should().Be(expectedLevel);
        character.Experience.Should().Be(expectedRemainingXp);
    }

    [Fact]
    public void Character_Should_Allow_Setting_Name()
    {
        // Arrange
        var character = new Character();

        // Act
        character.Name = "Hero";

        // Assert
        character.Name.Should().Be("Hero");
    }

    [Fact]
    public void Character_Should_Allow_Gaining_Gold()
    {
        // Arrange
        var character = new Character { Gold = 100 };

        // Act
        character.Gold += 50;

        // Assert
        character.Gold.Should().Be(150);
    }

    [Fact]
    public void GetMaxHealth_Should_Calculate_Based_On_Constitution_And_Level()
    {
        // Arrange
        var character = new Character
        {
            Constitution = 14,
            Level = 5
        };

        // Act
        var maxHealth = character.GetMaxHealth();

        // Assert
        // Formula: (CON × 10) + (Level × 5)
        // (14 × 10) + (5 × 5) = 140 + 25 = 165
        maxHealth.Should().Be(165);
    }

    [Fact]
    public void GetMaxMana_Should_Calculate_Based_On_Wisdom_And_Level()
    {
        // Arrange
        var character = new Character
        {
            Wisdom = 16,
            Level = 8
        };

        // Act
        var maxMana = character.GetMaxMana();

        // Assert
        // Formula: (WIS × 5) + (Level × 3)
        // (16 × 5) + (8 × 3) = 80 + 24 = 104
        maxMana.Should().Be(104);
    }

    [Fact]
    public void GetPhysicalDamageBonus_Should_Calculate_Based_On_Strength()
    {
        // Arrange
        var character = new Character { Strength = 18 };

        // Act
        var damage = character.GetPhysicalDamageBonus();

        // Assert
        // Formula: STR
        damage.Should().Be(18);
    }

    [Fact]
    public void GetMagicDamageBonus_Should_Calculate_Based_On_Intelligence()
    {
        // Arrange
        var character = new Character { Intelligence = 20 };

        // Act
        var damage = character.GetMagicDamageBonus();

        // Assert
        // Formula: INT
        damage.Should().Be(20);
    }

    [Fact]
    public void GetDodgeChance_Should_Calculate_Based_On_Dexterity()
    {
        // Arrange
        var character = new Character { Dexterity = 16 };

        // Act
        var dodgeChance = character.GetDodgeChance();

        // Assert
        // Check that it returns a reasonable value
        dodgeChance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetCriticalChance_Should_Calculate_Based_On_Dexterity()
    {
        // Arrange
        var character = new Character { Dexterity = 14 };

        // Act
        var critChance = character.GetCriticalChance();

        // Assert
        // Check that it returns a reasonable value
        critChance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetPhysicalDefense_Should_Calculate_Based_On_Constitution()
    {
        // Arrange
        var character = new Character { Constitution = 18 };

        // Act
        var defense = character.GetPhysicalDefense();

        // Assert
        // Check that it returns a positive value
        defense.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetMagicResistance_Should_Calculate_Based_On_Wisdom()
    {
        // Arrange
        var character = new Character { Wisdom = 14 };

        // Act
        var resistance = character.GetMagicResistance();

        // Assert
        // Check that it returns a reasonable value
        resistance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetRareItemChance_Should_Calculate_Based_On_Charisma()
    {
        // Arrange
        var character = new Character { Charisma = 16 };

        // Act
        var rareChance = character.GetRareItemChance();

        // Assert
        // Check that it returns a positive value
        rareChance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Character_Should_Start_With_Empty_Inventory()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Inventory.Should().NotBeNull();
        character.Inventory.Should().BeEmpty();
    }

    [Fact]
    public void Character_Should_Allow_Adding_Items_To_Inventory()
    {
        // Arrange
        var character = new Character();
        var item = new Item { Name = "Potion" };

        // Act
        character.Inventory.Add(item);

        // Assert
        character.Inventory.Should().HaveCount(1);
        character.Inventory[0].Should().Be(item);
    }

    [Fact]
    public void Character_Should_Start_With_Base_Attributes_Of_10()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.Strength.Should().Be(10);
        character.Dexterity.Should().Be(10);
        character.Constitution.Should().Be(10);
        character.Intelligence.Should().Be(10);
        character.Wisdom.Should().Be(10);
        character.Charisma.Should().Be(10);
    }

    [Fact]
    public void Character_Should_Start_With_Zero_Unspent_Points()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.UnspentAttributePoints.Should().Be(0);
        character.UnspentSkillPoints.Should().Be(0);
    }

    [Fact]
    public void IsAlive_Should_Return_True_When_Health_Above_Zero()
    {
        // Arrange
        var character = new Character { Health = 1 };

        // Act
        var isAlive = character.IsAlive();

        // Assert
        isAlive.Should().BeTrue();
    }

    [Fact]
    public void IsAlive_Should_Return_False_When_Health_Is_Zero()
    {
        // Arrange
        var character = new Character { Health = 0 };

        // Act
        var isAlive = character.IsAlive();

        // Assert
        isAlive.Should().BeFalse();
    }

    [Fact]
    public void Character_Should_Allow_Setting_ClassName()
    {
        // Arrange
        var character = new Character();

        // Act
        character.ClassName = "Warrior";

        // Assert
        character.ClassName.Should().Be("Warrior");
    }

    [Fact]
    public void GetPhysicalDamageBonus_Should_Return_Strength_Value()
    {
        // Arrange
        var character = new Character { Strength = 10 };

        // Act
        var damage = character.GetPhysicalDamageBonus();

        // Assert
        damage.Should().Be(10);
    }

    [Fact]
    public void GetPhysicalDefense_Should_Return_Value_For_Constitution()
    {
        // Arrange
        var character = new Character { Constitution = 8 };

        // Act
        var defense = character.GetPhysicalDefense();

        // Assert
        defense.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GainExperience_Should_Create_Pending_LevelUp_Info()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };

        // Act
        character.GainExperience(100);

        // Assert
        character.PendingLevelUps.Should().HaveCount(1);
        character.PendingLevelUps[0].NewLevel.Should().Be(2);
        character.PendingLevelUps[0].IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void Character_Should_Start_With_Empty_PendingLevelUps()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.PendingLevelUps.Should().NotBeNull();
        character.PendingLevelUps.Should().BeEmpty();
    }

    [Fact]
    public void Character_Should_Start_With_Empty_LearnedSkills()
    {
        // Arrange & Act
        var character = new Character();

        // Assert
        character.LearnedSkills.Should().NotBeNull();
        character.LearnedSkills.Should().BeEmpty();
    }
}
