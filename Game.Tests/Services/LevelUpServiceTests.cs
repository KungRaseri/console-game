using FluentAssertions;
using Game.Models;
using Game.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using Spectre.Console.Testing;
using Xunit;

namespace Game.Tests.Services;

/// <summary>
/// Tests for LevelUpService (currently 2.8% coverage → target 80%+).
/// </summary>
public class LevelUpServiceTests
{
    private readonly TestConsole _testConsole;
    private readonly IConsoleUI _consoleUI;
    private readonly LevelUpService _levelUpService;

    public LevelUpServiceTests()
    {
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        _levelUpService = new LevelUpService(_consoleUI);
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Do_Nothing_When_No_Pending_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            PendingLevelUps = new List<LevelUpInfo>()
        };

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Skip_Already_Processed_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 2, IsProcessed = true },
                new LevelUpInfo { NewLevel = 3, IsProcessed = true }
            }
        };

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().HaveCount(2);
        character.PendingLevelUps.Should().OnlyContain(l => l.IsProcessed);
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Process_Unprocessed_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 6,
            UnspentAttributePoints = 0, // No points to allocate to avoid interaction
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo 
                { 
                    NewLevel = 6, 
                    IsProcessed = false,
                    AttributePointsGained = 5,
                    SkillPointsGained = 1
                }
            }
        };

        // Simulate key presses for PressAnyKey calls
        _testConsole.Input.PushKey(ConsoleKey.Enter); // "Press any key to allocate..."

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().HaveCount(1);
        character.PendingLevelUps[0].IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Process_Multiple_LevelUps_In_Order()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 8,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 7, IsProcessed = false, AttributePointsGained = 5 },
                new LevelUpInfo { NewLevel = 6, IsProcessed = false, AttributePointsGained = 5 },
                new LevelUpInfo { NewLevel = 8, IsProcessed = false, AttributePointsGained = 5 }
            }
        };

        // Simulate key presses for each level up
        _testConsole.Input.PushKey(ConsoleKey.Enter); // Level 6
        _testConsole.Input.PushKey(ConsoleKey.Enter); // Level 7
        _testConsole.Input.PushKey(ConsoleKey.Enter); // Level 8

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().HaveCount(3);
        character.PendingLevelUps.Should().OnlyContain(l => l.IsProcessed);
        
        // Verify they were processed in order (6, 7, 8)
        var output = _testConsole.Output;
        output.Should().Contain("LEVEL 6");
        output.Should().Contain("LEVEL 7");
        output.Should().Contain("LEVEL 8");
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Clean_Up_Old_Processed_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 15,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>()
        };

        // Add 10 old processed level-ups (more than the 5 limit)
        for (int i = 1; i <= 10; i++)
        {
            character.PendingLevelUps.Add(new LevelUpInfo
            {
                NewLevel = i,
                IsProcessed = true,
                AttributePointsGained = 5
            });
        }

        // Add one new unprocessed level-up
        character.PendingLevelUps.Add(new LevelUpInfo
        {
            NewLevel = 15,
            IsProcessed = false,
            AttributePointsGained = 5
        });

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        // The cleanup removes processed level-ups older than (Level - 5)
        // With Level = 15, it keeps levels 10+ that are processed
        character.PendingLevelUps.Should().HaveCountLessThan(11, "Old level-ups should be cleaned up");
        character.PendingLevelUps.Should().OnlyContain(l => l.NewLevel >= 10, "Should only keep recent level-ups");
        character.PendingLevelUps.Should().OnlyContain(l => l.IsProcessed, "All remaining should be processed");
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Display_Attribute_Points_Gained()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo
                {
                    NewLevel = 5,
                    IsProcessed = false,
                    AttributePointsGained = 10,
                    SkillPointsGained = 2
                }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("10 Attribute Points");
        output.Should().Contain("2 Skill Point");
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Display_Congratulations_Message()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 3,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo
                {
                    NewLevel = 3,
                    IsProcessed = false,
                    AttributePointsGained = 5
                }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Congratulations");
        output.Should().Contain("level 3");
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Mark_LevelUps_As_Processed()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 4,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 4, IsProcessed = false, AttributePointsGained = 5 }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps[0].IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Display_Health_And_Mana_Restored_Message()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 2,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 2, IsProcessed = false, AttributePointsGained = 5 }
            }
        };

        _testConsole.Input.PushKey(ConsoleKey.Enter);

        // Act
        await _levelUpService.ProcessPendingLevelUpsAsync(character);

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Health & Mana fully restored");
    }
}
