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
            Experience = 0,
            MaxHealth = 100,
            MaxMana = 50
        };

        // Act
        character.GainExperience(100);

        // Assert
        character.Level.Should().Be(2);
        character.MaxHealth.Should().Be(110); // +10 per level
        character.Health.Should().Be(110); // Restored to max
        character.MaxMana.Should().Be(55); // +5 per level
        character.Mana.Should().Be(55); // Restored to max
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
}
