using FluentAssertions;
using Game.Models;

namespace Game.Tests.Models;

public class GameEventsTests
{
    [Fact]
    public void CharacterCreated_Should_Have_PlayerName()
    {
        // Arrange & Act
        var event1 = new CharacterCreated("Hero");

        // Assert
        event1.PlayerName.Should().Be("Hero");
    }

    [Fact]
    public void PlayerLeveledUp_Should_Have_PlayerName_And_NewLevel()
    {
        // Arrange & Act
        var event1 = new PlayerLeveledUp("Hero", 5);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.NewLevel.Should().Be(5);
    }

    [Fact]
    public void GoldGained_Should_Have_PlayerName_And_Amount()
    {
        // Arrange & Act
        var event1 = new GoldGained("Hero", 100);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.Amount.Should().Be(100);
    }

    [Fact]
    public void DamageTaken_Should_Have_PlayerName_And_Amount()
    {
        // Arrange & Act
        var event1 = new DamageTaken("Hero", 25);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.Amount.Should().Be(25);
    }

    [Fact]
    public void ItemAcquired_Should_Have_PlayerName_And_ItemName()
    {
        // Arrange & Act
        var event1 = new ItemAcquired("Hero", "Sword of Destiny");

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.ItemName.Should().Be("Sword of Destiny");
    }

    [Fact]
    public void GameEvents_Should_Be_Records()
    {
        // Arrange
        var event1 = new CharacterCreated("Hero");
        var event2 = new CharacterCreated("Hero");
        var event3 = new CharacterCreated("Villain");

        // Act & Assert - Records have value equality
        event1.Should().Be(event2);
        event1.Should().NotBe(event3);
    }
}
