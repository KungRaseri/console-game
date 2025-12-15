using FluentAssertions;
using Game.Core.Models;

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

    #region GetFormattedEntries Tests

    [Fact]
    public void GetFormattedEntries_Should_Return_Empty_List_For_Empty_Log()
    {
        // Arrange
        var log = new CombatLog();

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted.Should().BeEmpty();
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Single_Entry()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Test message", CombatLogType.Info);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted.Should().HaveCount(1);
        formatted[0].Should().Contain("Test message");
        formatted[0].Should().Contain("[dim]"); // Info type uses dim color
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_PlayerAttack_With_Green()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Player hits enemy", CombatLogType.PlayerAttack);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[green]");
        formatted[0].Should().Contain("Player hits enemy");
        formatted[0].Should().EndWith("[/]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_EnemyAttack_With_Red()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Enemy hits player", CombatLogType.EnemyAttack);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[red]");
        formatted[0].Should().Contain("Enemy hits player");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Critical_With_Orange()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("CRITICAL HIT!", CombatLogType.Critical);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[orange1]");
        formatted[0].Should().Contain("CRITICAL HIT!");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Dodge_With_Yellow()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Dodged attack", CombatLogType.Dodge);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[yellow]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Heal_With_Cyan()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Restored 25 HP", CombatLogType.Heal);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[cyan]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Defend_With_Blue()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Defending", CombatLogType.Defend);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[blue]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_ItemUse_With_Purple()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Used Health Potion", CombatLogType.ItemUse);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[purple]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Victory_With_Lime()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Victory!", CombatLogType.Victory);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[lime]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Defeat_With_Red()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Defeated", CombatLogType.Defeat);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().StartWith("[red]");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_Multiple_Entries_With_Different_Colors()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("Player attacks", CombatLogType.PlayerAttack);
        log.AddEntry("Enemy attacks", CombatLogType.EnemyAttack);
        log.AddEntry("Critical hit!", CombatLogType.Critical);
        log.AddEntry("Healed 50 HP", CombatLogType.Heal);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted.Should().HaveCount(4);
        formatted[0].Should().StartWith("[green]"); // PlayerAttack
        formatted[1].Should().StartWith("[red]");   // EnemyAttack
        formatted[2].Should().StartWith("[orange1]"); // Critical
        formatted[3].Should().StartWith("[cyan]");  // Heal
    }

    [Fact]
    public void GetFormattedEntries_Should_Preserve_Entry_Order()
    {
        // Arrange
        var log = new CombatLog();
        log.AddEntry("First", CombatLogType.Info);
        log.AddEntry("Second", CombatLogType.PlayerAttack);
        log.AddEntry("Third", CombatLogType.EnemyAttack);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted[0].Should().Contain("First");
        formatted[1].Should().Contain("Second");
        formatted[2].Should().Contain("Third");
    }

    [Fact]
    public void GetFormattedEntries_Should_Format_All_CombatLogTypes()
    {
        // Arrange
        var log = new CombatLog(15);
        log.AddEntry("Info", CombatLogType.Info);
        log.AddEntry("PlayerAttack", CombatLogType.PlayerAttack);
        log.AddEntry("EnemyAttack", CombatLogType.EnemyAttack);
        log.AddEntry("Critical", CombatLogType.Critical);
        log.AddEntry("Dodge", CombatLogType.Dodge);
        log.AddEntry("Heal", CombatLogType.Heal);
        log.AddEntry("Defend", CombatLogType.Defend);
        log.AddEntry("ItemUse", CombatLogType.ItemUse);
        log.AddEntry("Victory", CombatLogType.Victory);
        log.AddEntry("Defeat", CombatLogType.Defeat);

        // Act
        var formatted = log.GetFormattedEntries();

        // Assert
        formatted.Should().HaveCount(10);
        formatted.Should().AllSatisfy(entry => 
        {
            entry.Should().StartWith("[");
            entry.Should().EndWith("[/]");
        });
    }

    #endregion
}
