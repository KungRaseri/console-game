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

    [Fact]
    public void CombatStarted_Should_Have_PlayerName_And_EnemyName()
    {
        // Arrange & Act
        var evt = new CombatStarted("Warrior", "Dragon");

        // Assert
        evt.PlayerName.Should().Be("Warrior");
        evt.EnemyName.Should().Be("Dragon");
    }

    [Fact]
    public void CombatEnded_Should_Have_PlayerName_And_Victory()
    {
        // Arrange & Act
        var evt = new CombatEnded("Mage", true);

        // Assert
        evt.PlayerName.Should().Be("Mage");
        evt.Victory.Should().BeTrue();
    }

    [Fact]
    public void PlayerDefeated_Should_Have_PlayerName_And_EnemyName()
    {
        // Arrange & Act
        var evt = new PlayerDefeated("Rogue", "Goblin");

        // Assert
        evt.PlayerName.Should().Be("Rogue");
        evt.EnemyName.Should().Be("Goblin");
    }

    [Fact]
    public void AttackPerformed_Should_Have_All_Properties()
    {
        // Arrange & Act
        var evt = new AttackPerformed("Hero", "Enemy", 50);

        // Assert
        evt.AttackerName.Should().Be("Hero");
        evt.DefenderName.Should().Be("Enemy");
        evt.Damage.Should().Be(50);
    }

    [Fact]
    public void EnemyDefeated_Should_Have_PlayerName_And_EnemyName()
    {
        // Arrange & Act
        var evt = new EnemyDefeated("Champion", "Troll");

        // Assert
        evt.PlayerName.Should().Be("Champion");
        evt.EnemyName.Should().Be("Troll");
    }
}
