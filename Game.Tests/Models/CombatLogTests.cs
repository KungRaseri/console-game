using FluentAssertions;
using Game.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Tests for CombatLog model.
/// </summary>
public class CombatLogTests
{
    #region Initialization Tests

    [Fact]
    public void CombatLog_Should_Initialize_With_Default_Max_Entries()
    {
        // Act
        var log = new CombatLog();

        // Assert
        log.Entries.Should().BeEmpty();
    }

    [Fact]
    public void CombatLog_Should_Initialize_With_Custom_Max_Entries()
    {
        // Act
        var log = new CombatLog(20);

        // Assert
        log.Entries.Should().BeEmpty();
    }

    #endregion

    #region Add Entry Tests

    [Fact]
    public void AddEntry_Should_Add_Message_To_Log()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("Player attacks enemy");

        // Assert
        log.Entries.Should().HaveCount(1);
        log.Entries[0].Message.Should().Be("Player attacks enemy");
    }

    [Fact]
    public void AddEntry_Should_Use_Default_Info_Type()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("Test message");

        // Assert
        log.Entries[0].Type.Should().Be(CombatLogType.Info);
    }

    [Theory]
    [InlineData(CombatLogType.Info)]
    [InlineData(CombatLogType.PlayerAttack)]
    [InlineData(CombatLogType.EnemyAttack)]
    [InlineData(CombatLogType.Heal)]
    [InlineData(CombatLogType.Critical)]
    [InlineData(CombatLogType.Dodge)]
    [InlineData(CombatLogType.Victory)]
    public void AddEntry_Should_Support_All_Log_Types(CombatLogType type)
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("Test message", type);

        // Assert
        log.Entries[0].Type.Should().Be(type);
    }

    [Fact]
    public void AddEntry_Should_Set_Timestamp()
    {
        // Arrange
        var log = new CombatLog();
        var before = DateTime.Now;

        // Act
        log.AddEntry("Test");
        var after = DateTime.Now;

        // Assert
        log.Entries[0].Timestamp.Should().BeOnOrAfter(before);
        log.Entries[0].Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void AddEntry_Should_Add_Multiple_Entries()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("Entry 1");
        log.AddEntry("Entry 2");
        log.AddEntry("Entry 3");

        // Assert
        log.Entries.Should().HaveCount(3);
        log.Entries[0].Message.Should().Be("Entry 1");
        log.Entries[1].Message.Should().Be("Entry 2");
        log.Entries[2].Message.Should().Be("Entry 3");
    }

    #endregion

    #region Max Entries Tests

    [Fact]
    public void AddEntry_Should_Trim_Old_Entries_When_Max_Exceeded()
    {
        // Arrange
        var log = new CombatLog(3);

        // Act
        log.AddEntry("Entry 1");
        log.AddEntry("Entry 2");
        log.AddEntry("Entry 3");
        log.AddEntry("Entry 4"); // Should remove Entry 1

        // Assert
        log.Entries.Should().HaveCount(3);
        log.Entries[0].Message.Should().Be("Entry 2");
        log.Entries[1].Message.Should().Be("Entry 3");
        log.Entries[2].Message.Should().Be("Entry 4");
    }

    [Fact]
    public void AddEntry_Should_Keep_Rolling_Log_Of_Recent_Entries()
    {
        // Arrange
        var log = new CombatLog(5);

        // Act - Add 10 entries
        for (int i = 1; i <= 10; i++)
        {
            log.AddEntry($"Entry {i}");
        }

        // Assert - Should only keep last 5
        log.Entries.Should().HaveCount(5);
        log.Entries[0].Message.Should().Be("Entry 6");
        log.Entries[4].Message.Should().Be("Entry 10");
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_Should_Remove_All_Entries()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Entry 1");
        log.AddEntry("Entry 2");
        log.AddEntry("Entry 3");

        // Act
        log.Clear();

        // Assert
        log.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Clear_Should_Work_On_Empty_Log()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        var act = () => log.Clear();

        // Assert
        act.Should().NotThrow();
        log.Entries.Should().BeEmpty();
    }

    #endregion

    #region ReadOnly Tests

    [Fact]
    public void Entries_Should_Be_ReadOnly()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Test");

        // Assert
        log.Entries.Should().BeAssignableTo<IReadOnlyList<CombatLogEntry>>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CombatLog_Complete_Combat_Workflow()
    {
        // Arrange
        var log = new CombatLog(10);

        // Act - Combat sequence
        log.AddEntry("Combat started!", CombatLogType.Info);
        log.AddEntry("Player attacks Goblin for 15 damage", CombatLogType.PlayerAttack);
        log.AddEntry("Goblin attacks Player for 8 damage", CombatLogType.EnemyAttack);
        log.AddEntry("Player CRITICAL HIT for 30 damage!", CombatLogType.Critical);
        log.AddEntry("Player uses Health Potion, restored 25 HP", CombatLogType.Heal);
        log.AddEntry("Goblin defeated!", CombatLogType.Victory);

        // Assert
        log.Entries.Should().HaveCount(6);
        log.Entries[0].Message.Should().Contain("Combat started");
        log.Entries[3].Type.Should().Be(CombatLogType.Critical);
        log.Entries[4].Type.Should().Be(CombatLogType.Heal);
        log.Entries[5].Type.Should().Be(CombatLogType.Victory);
    }

    [Fact]
    public void CombatLog_Should_Handle_Long_Combat()
    {
        // Arrange
        var log = new CombatLog(15);

        // Act - Simulate 30 combat actions
        for (int i = 1; i <= 30; i++)
        {
            log.AddEntry($"Action {i}", CombatLogType.PlayerAttack);
        }

        // Assert - Should only keep last 15
        log.Entries.Should().HaveCount(15);
        log.Entries[0].Message.Should().Be("Action 16");
        log.Entries[14].Message.Should().Be("Action 30");
    }

    [Fact]
    public void CombatLog_Should_Support_Different_Log_Types_In_Sequence()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("Start", CombatLogType.Info);
        log.AddEntry("Player Hit", CombatLogType.PlayerAttack);
        log.AddEntry("Enemy Hit", CombatLogType.EnemyAttack);
        log.AddEntry("Heal", CombatLogType.Heal);
        log.AddEntry("Dodge", CombatLogType.Dodge);
        log.AddEntry("Crit", CombatLogType.Critical);

        // Assert
        log.Entries.Should().HaveCount(6);
        log.Entries.Select(e => e.Type).Should().ContainInOrder(
            CombatLogType.Info,
            CombatLogType.PlayerAttack,
            CombatLogType.EnemyAttack,
            CombatLogType.Heal,
            CombatLogType.Dodge,
            CombatLogType.Critical
        );
    }

    [Fact]
    public void CombatLog_Timestamps_Should_Be_Sequential()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        log.AddEntry("First");
        log.AddEntry("Second");
        log.AddEntry("Third");

        // Assert - Timestamps should be close together and in order
        log.Entries[0].Timestamp.Should().BeOnOrBefore(log.Entries[1].Timestamp);
        log.Entries[1].Timestamp.Should().BeOnOrBefore(log.Entries[2].Timestamp);
    }

    #endregion
}
