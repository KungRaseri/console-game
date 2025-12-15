using FluentAssertions;
using Game.Core.Features.Achievement.Services;
using Game.Core.Features.SaveLoad;
using Game.Core.Models;
using Game.Core.Services;
using Game.Shared.Services;
using Game.Console.UI;
using Game.Core.Abstractions;
using Game.Data.Repositories;
using Spectre.Console.Testing;
using Xunit;

namespace Game.Tests.Features.Achievement.Services;

/// <summary>
/// Unit tests for AchievementService - Achievement unlocking and progress tracking
/// </summary>
public class AchievementServiceTests : IDisposable
{
    private readonly SaveGameService _saveService;
    private readonly IGameUI _consoleUI;
    private readonly AchievementService _achievementService;
    private readonly SaveGame _testSaveGame;
    private readonly string _testDbPath;

    public AchievementServiceTests()
    {
        var testConsole = new TestConsole();
        _consoleUI = new ConsoleUI(testConsole);

        // Use unique temporary file for each test
        _testDbPath = $"test-achievement-{Guid.NewGuid()}.db";
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }

        var apocalypseTimer = new ApocalypseTimer(_consoleUI);
        var repository = new SaveGameRepository(_testDbPath);
        _saveService = new SaveGameService(repository, apocalypseTimer);

        // Create test save game
        _testSaveGame = new SaveGame
        {
            Character = new Character
            {
                Name = "TestHero",
                Level = 10,
                Gold = 500
            },
            UnlockedAchievements = new List<string>(),
            CompletedQuests = new List<Quest>(),
            TotalEnemiesDefeated = 50,
            PlayTimeMinutes = 120,
            DeathCount = 0,
            DifficultyLevel = "Normal"
        };

        // Initialize with a new game (this sets _currentSave)
        var difficulty = new DifficultySettings { Name = "Normal" };
        _saveService.CreateNewGame(_testSaveGame.Character, difficulty);

        _achievementService = new AchievementService(_saveService, _consoleUI);
    }

    public void Dispose()
    {
        _saveService.Dispose();

        // Clean up test database file
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // Also clean up the -log file if it exists
        var logFile = _testDbPath + "-log";
        if (File.Exists(logFile))
        {
            try
            {
                File.Delete(logFile);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region UnlockAchievement Tests

    [Fact]
    public async Task UnlockAchievementAsync_Should_Unlock_New_Achievement()
    {
        // Arrange - quest achievement that should be unlockable
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "test_quest" });

        // Act
        var result = await _achievementService.UnlockAchievementAsync("first_steps");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("first_steps");
        result.IsUnlocked.Should().BeTrue();
        result.UnlockedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        currentSave.UnlockedAchievements.Should().Contain("first_steps");
    }

    [Fact]
    public async Task UnlockAchievementAsync_Should_Return_Null_If_Already_Unlocked()
    {
        // Arrange - already unlocked
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.UnlockedAchievements.Add("first_steps");

        // Act
        var result = await _achievementService.UnlockAchievementAsync("first_steps");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UnlockAchievementAsync_Should_Return_Null_If_Achievement_Not_Found()
    {
        // Act
        var result = await _achievementService.UnlockAchievementAsync("invalid_achievement_id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UnlockAchievementAsync_Should_Set_Unlocked_Timestamp()
    {
        // Arrange
        var beforeUnlock = DateTime.Now;

        // Act
        var result = await _achievementService.UnlockAchievementAsync("first_steps");

        // Assert
        result.Should().NotBeNull();
        result!.UnlockedAt.Should().BeOnOrAfter(beforeUnlock);
        result.UnlockedAt.Should().BeOnOrBefore(DateTime.Now);
    }

    #endregion

    #region CheckAllAchievements Tests

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Level_Achievement()
    {
        // Arrange - player reached level 20
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.Character.Level = 20;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock "Master" achievement
        result.Should().ContainSingle(a => a.Id == "master");
        currentSave.UnlockedAchievements.Should().Contain("master");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Enemy_Defeat_Achievement()
    {
        // Arrange - player defeated 100+ enemies
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.TotalEnemiesDefeated = 150;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock "Slayer" achievement
        result.Should().ContainSingle(a => a.Id == "slayer");
        currentSave.UnlockedAchievements.Should().Contain("slayer");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Not_Unlock_Already_Unlocked_Achievements()
    {
        // Arrange - player level 20, but achievement already unlocked
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.Character.Level = 20;
        currentSave.UnlockedAchievements.Add("master");

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should not unlock again
        result.Should().NotContain(a => a.Id == "master");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Multiple_Achievements()
    {
        // Arrange - meet multiple criteria
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.Character.Level = 20; // Master achievement
        currentSave.TotalEnemiesDefeated = 150; // Slayer achievement

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock both achievements
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == "master");
        result.Should().Contain(a => a.Id == "slayer");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Game_Completion_Achievement()
    {
        // Arrange - completed main quest
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock "Savior" achievement
        result.Should().ContainSingle(a => a.Id == "savior");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Difficulty_Completion_Achievement()
    {
        // Arrange - completed game on Apocalypse difficulty
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });
        currentSave.DifficultyLevel = "Apocalypse";

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock both completion and apocalypse achievements
        result.Should().Contain(a => a.Id == "savior");
        result.Should().Contain(a => a.Id == "apocalypse_survivor");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Unlock_Deathless_Achievement()
    {
        // Arrange - completed game without dying
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });
        currentSave.DeathCount = 0;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should unlock deathless achievement
        result.Should().Contain(a => a.Id == "deathless");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Not_Unlock_Deathless_If_Player_Died()
    {
        // Arrange - completed game but died
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });
        currentSave.DeathCount = 1;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should not unlock deathless achievement
        result.Should().NotContain(a => a.Id == "deathless");
    }

    #endregion

    #region GetUnlockedAchievements Tests

    [Fact]
    public async Task GetUnlockedAchievementsAsync_Should_Return_Unlocked_Achievements()
    {
        // Arrange
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.UnlockedAchievements.Add("first_steps");
        currentSave.UnlockedAchievements.Add("slayer");

        // Act
        var result = await _achievementService.GetUnlockedAchievementsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == "first_steps");
        result.Should().Contain(a => a.Id == "slayer");
        result.Should().AllSatisfy(a => a.IsUnlocked.Should().BeTrue());
    }

    [Fact]
    public async Task GetUnlockedAchievementsAsync_Should_Return_Empty_If_No_Unlocked()
    {
        // Arrange - no unlocked achievements
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.UnlockedAchievements.Clear();

        // Act
        var result = await _achievementService.GetUnlockedAchievementsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnlockedAchievementsAsync_Should_Mark_All_As_Unlocked()
    {
        // Arrange
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.UnlockedAchievements.Add("first_steps");
        currentSave.UnlockedAchievements.Add("master");

        // Act
        var result = await _achievementService.GetUnlockedAchievementsAsync();

        // Assert
        result.Should().AllSatisfy(a => a.IsUnlocked.Should().BeTrue());
    }

    #endregion

    #region Achievement Criteria Tests

    [Theory]
    [InlineData(5, false)]  // Not enough enemies
    [InlineData(99, false)] // Just under threshold
    [InlineData(100, true)] // Exactly at threshold
    [InlineData(150, true)] // Above threshold
    public async Task CheckAllAchievementsAsync_Should_Check_Enemy_Defeat_Threshold(int enemiesDefeated, bool shouldUnlock)
    {
        // Arrange
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.TotalEnemiesDefeated = enemiesDefeated;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert
        if (shouldUnlock)
            result.Should().ContainSingle(a => a.Id == "slayer");
        else
            result.Should().NotContain(a => a.Id == "slayer");
    }

    [Theory]
    [InlineData(10, false)]  // Below level requirement
    [InlineData(19, false)]  // Just under level
    [InlineData(20, true)]   // Exactly at level
    [InlineData(50, true)]   // Above level
    public async Task CheckAllAchievementsAsync_Should_Check_Level_Requirement(int level, bool shouldUnlock)
    {
        // Arrange
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.Character.Level = level;

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert
        if (shouldUnlock)
            result.Should().ContainSingle(a => a.Id == "master");
        else
            result.Should().NotContain(a => a.Id == "master");
    }

    #endregion

    #region Achievement Properties Tests

    [Fact]
    public async Task UnlockAchievementAsync_Should_Preserve_Achievement_Properties()
    {
        // Act
        var result = await _achievementService.UnlockAchievementAsync("master");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Master");
        result.Description.Should().Be("Reach level 20");
        result.Icon.Should().Be("👑");
        result.Category.Should().Be(AchievementCategory.Mastery);
        result.Points.Should().Be(50);
        result.IsSecret.Should().BeFalse();
    }

    [Fact]
    public async Task UnlockAchievementAsync_Should_Handle_Secret_Achievements()
    {
        // Arrange - meet deathless criteria
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });
        currentSave.DeathCount = 0;

        // Act
        var result = await _achievementService.UnlockAchievementAsync("deathless");

        // Assert
        result.Should().NotBeNull();
        result!.IsSecret.Should().BeTrue();
        result.Points.Should().Be(500);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Handle_Empty_Completed_Quests()
    {
        // Arrange - no completed quests
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.CompletedQuests.Clear();

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should not unlock quest-based achievements
        result.Should().NotContain(a => a.Id == "first_steps");
        result.Should().NotContain(a => a.Id == "savior");
    }

    [Fact]
    public async Task CheckAllAchievementsAsync_Should_Handle_All_Achievements_Already_Unlocked()
    {
        // Arrange - all achievements already unlocked
        var currentSave = _saveService.GetCurrentSave();
        currentSave!.UnlockedAchievements.AddRange(new[]
        {
            "first_steps", "slayer", "master", "savior", "apocalypse_survivor", "deathless"
        });

        // Meet all criteria
        currentSave.Character.Level = 20;
        currentSave.TotalEnemiesDefeated = 150;
        currentSave.CompletedQuests.Add(new Quest { Id = "main_06_final_boss" });

        // Act
        var result = await _achievementService.CheckAllAchievementsAsync();

        // Assert - should not unlock any (all already unlocked)
        result.Should().BeEmpty();
    }

    #endregion
}

