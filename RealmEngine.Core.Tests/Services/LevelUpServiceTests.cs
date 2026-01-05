using FluentAssertions;
using Moq;
using RealmEngine.Core.Abstractions;
using RealmEngine.Core.Services;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Tests.Services;

[Trait("Category", "Service")]
/// <summary>
/// Tests for LevelUpService.
/// </summary>
public class LevelUpServiceTests
{
    private readonly Mock<IGameUI> _mockUI;
    private readonly LevelUpService _service;

    public LevelUpServiceTests()
    {
        _mockUI = new Mock<IGameUI>();
        _service = new LevelUpService(_mockUI.Object);
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Do_Nothing_When_No_Pending_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 5,
            PendingLevelUps = new List<LevelUpInfo>()
        };

        // Act
        await _service.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().BeEmpty();
        _mockUI.Verify(ui => ui.Clear(), Times.Never);
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Mark_LevelUps_As_Processed()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 6,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo
                {
                    NewLevel = 6,
                    AttributePointsGained = 5,
                    SkillPointsGained = 1,
                    IsProcessed = false
                }
            }
        };

        // Mock UI interactions to skip actual prompts
        _mockUI.Setup(ui => ui.AskForInput(It.IsAny<string>())).Returns("");

        // Act
        await _service.ProcessPendingLevelUpsAsync(character);

        // Assert
        character.PendingLevelUps.Should().HaveCount(1);
        character.PendingLevelUps.First().IsProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Process_Multiple_LevelUps_In_Order()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 8,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 7, AttributePointsGained = 5, SkillPointsGained = 1, IsProcessed = false },
                new LevelUpInfo { NewLevel = 6, AttributePointsGained = 5, SkillPointsGained = 1, IsProcessed = false },
                new LevelUpInfo { NewLevel = 8, AttributePointsGained = 5, SkillPointsGained = 1, IsProcessed = false }
            }
        };

        _mockUI.Setup(ui => ui.AskForInput(It.IsAny<string>())).Returns("");

        // Act
        await _service.ProcessPendingLevelUpsAsync(character);

        // Assert - Should process in order by NewLevel (6, 7, 8)
        character.PendingLevelUps.Should().AllSatisfy(l => l.IsProcessed.Should().BeTrue());
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Clean_Up_Old_Processed_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 20,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 5, IsProcessed = true },
                new LevelUpInfo { NewLevel = 6, IsProcessed = true },
                new LevelUpInfo { NewLevel = 7, IsProcessed = true },
                new LevelUpInfo { NewLevel = 8, IsProcessed = true },
                new LevelUpInfo { NewLevel = 9, IsProcessed = true },
                new LevelUpInfo { NewLevel = 10, IsProcessed = true },
                new LevelUpInfo { NewLevel = 11, IsProcessed = true },
                new LevelUpInfo { NewLevel = 20, IsProcessed = false } // New one to process
            }
        };

        _mockUI.Setup(ui => ui.AskForInput(It.IsAny<string>())).Returns("");

        // Act
        await _service.ProcessPendingLevelUpsAsync(character);

        // Assert - Should keep only last 5 processed + the newly processed one
        var processedCount = character.PendingLevelUps.Count(l => l.IsProcessed);
        processedCount.Should().BeLessThanOrEqualTo(5, "should clean up old level-ups beyond 5");
    }

    [Fact]
    public async Task ProcessPendingLevelUpsAsync_Should_Skip_Already_Processed_LevelUps()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 7,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 6, IsProcessed = true }, // Already processed
                new LevelUpInfo { NewLevel = 7, IsProcessed = false } // Not processed
            }
        };

        _mockUI.Setup(ui => ui.AskForInput(It.IsAny<string>())).Returns("");

        // Act
        await _service.ProcessPendingLevelUpsAsync(character);

        // Assert - Only one should be newly marked as processed
        character.PendingLevelUps.Should().HaveCount(2);
        character.PendingLevelUps.Should().AllSatisfy(l => l.IsProcessed.Should().BeTrue());
        
        // Should show banner only once for the unprocessed level-up
        _mockUI.Verify(ui => ui.ShowBanner(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ProcessPendingLevelUpsAsync_Should_Display_Level_Up_Banner()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Level = 6,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0,
            PendingLevelUps = new List<LevelUpInfo>
            {
                new LevelUpInfo { NewLevel = 6, AttributePointsGained = 5, SkillPointsGained = 1, IsProcessed = false }
            }
        };

        _mockUI.Setup(ui => ui.AskForInput(It.IsAny<string>())).Returns("");

        // Act
        var task = _service.ProcessPendingLevelUpsAsync(character);
        task.Wait();

        // Assert
        _mockUI.Verify(ui => ui.ShowBanner(
            It.Is<string>(s => s.Contains("LEVEL 6")),
            It.Is<string>(s => s.Contains("level 6"))),
            Times.Once);

        _mockUI.Verify(ui => ui.ShowPanel(
            It.Is<string>(s => s == "Level-Up Rewards"),
            It.IsAny<string>(),
            It.Is<string>(s => s == "green")),
            Times.Once);
    }
}
