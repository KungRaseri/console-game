using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RealmEngine.Core.Features.Combat;
using RealmEngine.Core.Features.Combat.Commands.AttackEnemy;
using RealmEngine.Core.Features.Quests.Commands;
using RealmEngine.Core.Features.Quests.Services;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Services;
using RealmEngine.Core.Abstractions;
using RealmEngine.Shared.Abstractions;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Quest.Integration;

/// <summary>
/// Integration tests for the complete quest system workflow.
/// Tests: Quest initialization → Start quest → Kill enemies → Auto-complete → Rewards.
/// </summary>
public class QuestIntegrationTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;
    private readonly MainQuestService _mainQuestService;
    private readonly SaveGame _testSaveGame;

    public QuestIntegrationTests()
    {
        var services = new ServiceCollection();

        // Create mock timer and repository
        var mockTimer = new Mock<IApocalypseTimer>();
        mockTimer.Setup(t => t.GetBonusMinutes()).Returns(0);
        
        var mockRepository = new Mock<ISaveGameRepository>();
        var testSave = new SaveGame(); // Will be set later
        mockRepository.Setup(r => r.SaveGame(It.IsAny<SaveGame>())).Callback<SaveGame>(s => {
            // Do nothing for tests
        });

        // Register all required services
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StartQuestCommand).Assembly));
        services.AddSingleton(mockTimer.Object);
        services.AddSingleton(mockRepository.Object);
        services.AddSingleton<SaveGameService>();
        services.AddSingleton<ISaveGameService>(sp => sp.GetRequiredService<SaveGameService>());
        services.AddSingleton<MainQuestService>();
        services.AddSingleton<QuestService>();
        services.AddSingleton<QuestProgressService>();
        services.AddSingleton<QuestRewardService>();
        services.AddSingleton<QuestInitializationService>();
        services.AddSingleton<CombatService>();
        services.AddSingleton<AbilityCatalogService>();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _saveGameService = _serviceProvider.GetRequiredService<SaveGameService>();
        _mainQuestService = _serviceProvider.GetRequiredService<MainQuestService>();

        // Create test save game
        var character = new Character { Name = "TestHero", ClassName = "Warrior", Level = 1 };
        _testSaveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
    }

    [Fact]
    public async Task Should_Initialize_Starting_Quest_On_New_Game()
    {
        // Arrange
        var character = new Character { Name = "TestHero", ClassName = "Warrior", Level = 1 };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        var result = await _mediator.Send(new InitializeStartingQuestsCommand(saveGame));

        // Assert
        result.Success.Should().BeTrue();
        result.QuestsInitialized.Should().Be(1);
        saveGame.AvailableQuests.Should().ContainSingle();
        saveGame.AvailableQuests[0].Id.Should().Be("main_01_awakening");
    }

    [Fact]
    public async Task Should_Complete_Quest_When_Enemy_Defeated_With_Matching_Objective()
    {
        // Arrange - Create character and start game
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Strength = 15
        };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        _saveGameService.SetCurrentSave(saveGame);

        // Get the second main quest (defeat_shrine_guardian)
        var quest = await _mainQuestService.GetQuestByIdAsync("main_02_first_trial");
        quest.Should().NotBeNull();

        // Start the quest manually (bypass prerequisites for test)
        quest!.IsActive = true;
        saveGame.ActiveQuests.Add(quest);

        // Create enemy matching the quest objective
        var enemy = new Enemy 
        { 
            Name = "Shrine Guardian", 
            Health = 50, 
            MaxHealth = 50,
            PhysicalDefense = 0,
            XPReward = 100,
            GoldReward = 50,
            Type = EnemyType.Boss
        };

        // Act - Attack enemy until defeated
        AttackEnemyResult? finalResult = null;
        while (enemy.Health > 0)
        {
            finalResult = await _mediator.Send(new AttackEnemyCommand(character, enemy, null));
        }

        // Assert
        finalResult.Should().NotBeNull();
        finalResult!.IsEnemyDefeated.Should().BeTrue();

        // Quest should be auto-completed
        saveGame.ActiveQuests.Should().BeEmpty();
        saveGame.CompletedQuests.Should().ContainSingle();
        saveGame.CompletedQuests[0].Id.Should().Be("main_02_first_trial");
        saveGame.QuestsCompleted.Should().Be(1);

        // Character should have received quest rewards
        character.Experience.Should().BeGreaterThan(100); // Enemy XP + Quest XP
        character.Gold.Should().BeGreaterThan(50); // Enemy Gold + Quest Gold
    }

    [Fact]
    public async Task Should_Update_Quest_Progress_For_Multiple_Enemy_Kills()
    {
        // Arrange
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 10,
            Health = 150,
            MaxHealth = 150,
            Strength = 20
        };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        _saveGameService.SetCurrentSave(saveGame);

        // Get quest that requires multiple kills (main_05_into_abyss - defeat 5 demons)
        var quest = await _mainQuestService.GetQuestByIdAsync("main_05_into_abyss");
        quest.Should().NotBeNull();

        quest!.IsActive = true;
        saveGame.ActiveQuests.Add(quest);

        // Act - Kill 5 demons
        for (int i = 0; i < 5; i++)
        {
            var demon = new Enemy 
            { 
                Name = $"Abyssal Demon {i+1}", 
                Health = 30, 
                MaxHealth = 30,
                PhysicalDefense = 0,
                XPReward = 50,
                GoldReward = 25,
                Type = EnemyType.Demon
            };

            while (demon.Health > 0)
            {
                await _mediator.Send(new AttackEnemyCommand(character, demon, null));
            }
        }

        // Assert
        var demonObjective = quest.ObjectiveProgress.FirstOrDefault(kvp => kvp.Key.Contains("demon"));
        demonObjective.Should().NotBeNull();
        demonObjective.Value.Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task Should_Not_Complete_Quest_Until_All_Objectives_Met()
    {
        // Arrange
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 10,
            Health = 150,
            MaxHealth = 150,
            Strength = 20
        };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        _saveGameService.SetCurrentSave(saveGame);

        // Get quest with multiple objectives (main_05_into_abyss)
        var quest = await _mainQuestService.GetQuestByIdAsync("main_05_into_abyss");
        quest.Should().NotBeNull();

        quest!.IsActive = true;
        saveGame.ActiveQuests.Add(quest);

        // Act - Kill only 3 demons (not enough to complete)
        for (int i = 0; i < 3; i++)
        {
            var demon = new Enemy 
            { 
                Name = "Abyssal Demon", 
                Health = 30, 
                MaxHealth = 30,
                PhysicalDefense = 0,
                XPReward = 50,
                GoldReward = 25,
                Type = EnemyType.Demon
            };

            while (demon.Health > 0)
            {
                await _mediator.Send(new AttackEnemyCommand(character, demon, null));
            }
        }

        // Assert - Quest should still be active (not enough kills)
        saveGame.ActiveQuests.Should().ContainSingle();
        saveGame.CompletedQuests.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Unlock_Next_Quest_After_Completing_Prerequisite()
    {
        // Arrange
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 1,
            Health = 100,
            MaxHealth = 100
        };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        _saveGameService.SetCurrentSave(saveGame);

        // Initialize starting quest
        await _mediator.Send(new InitializeStartingQuestsCommand(saveGame));

        // Accept first quest
        var startResult = await _mediator.Send(new StartQuestCommand("main_01_awakening"));
        startResult.Success.Should().BeTrue();

        // Act - Complete first quest manually (simulating objective completion)
        var firstQuest = saveGame.ActiveQuests.First();
        firstQuest.ObjectiveProgress["reach_ancient_shrine"] = 1; // Mark objective complete

        var completeResult = await _mediator.Send(new CompleteQuestCommand("main_01_awakening"));

        // Assert
        completeResult.Success.Should().BeTrue();
        saveGame.CompletedQuests.Should().ContainSingle(q => q.Id == "main_01_awakening");
        
        // Second quest should now be available
        saveGame.AvailableQuests.Should().Contain(q => q.Id == "main_02_first_trial");
    }

    [Fact]
    public async Task Should_Award_Apocalypse_Bonus_Time_When_Quest_Completed_In_Apocalypse_Mode()
    {
        // Arrange
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 1
        };
        
        var apocalypseDifficulty = new DifficultySettings
        {
            Name = "Normal",
            IsApocalypse = true,
            PlayerDamageMultiplier = 1.0,
            EnemyHealthMultiplier = 1.0,
            GoldXPMultiplier = 1.0
        };

        var saveGame = _saveGameService.CreateNewGame(character, apocalypseDifficulty);
        _saveGameService.SetCurrentSave(saveGame);
        saveGame.ApocalypseMode.Should().BeTrue();

        // Get quest with apocalypse bonus
        var quest = await _mainQuestService.GetQuestByIdAsync("main_01_awakening");
        quest.Should().NotBeNull();
        quest!.ApocalypseBonusMinutes.Should().Be(15);

        quest.IsActive = true;
        saveGame.ActiveQuests.Add(quest);

        // Mark objective complete
        quest.ObjectiveProgress["reach_ancient_shrine"] = 1;

        var initialBonusMinutes = saveGame.ApocalypseBonusMinutes;

        // Act
        var result = await _mediator.Send(new CompleteQuestCommand("main_01_awakening"));

        // Assert
        result.Success.Should().BeTrue();
        saveGame.ApocalypseBonusMinutes.Should().Be(initialBonusMinutes + 15);
    }

    [Fact]
    public async Task Should_Track_Enemies_Defeated_By_Type()
    {
        // Arrange
        var character = new Character 
        { 
            Name = "TestHero", 
            ClassName = "Warrior", 
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Strength = 15
        };
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        _saveGameService.SetCurrentSave(saveGame);

        var goblin = new Enemy 
        { 
            Name = "Goblin", 
            Health = 20, 
            MaxHealth = 20,
            Type = EnemyType.Humanoid
        };

        // Act
        while (goblin.Health > 0)
        {
            await _mediator.Send(new AttackEnemyCommand(character, goblin, null));
        }

        // Assert
        saveGame.TotalEnemiesDefeated.Should().Be(1);
        saveGame.EnemiesDefeatedByType.Should().ContainKey("humanoid");
        saveGame.EnemiesDefeatedByType["humanoid"].Should().Be(1);
    }

    // Helper classes for testing
    private class TestApocalypseTimer : IApocalypseTimer
    {
        private int _bonusMinutes = 0;

        public int GetBonusMinutes() => _bonusMinutes;
        public void AddBonusMinutes(int minutes) => _bonusMinutes += minutes;
        public DateTime? GetStartTime() => DateTime.Now;
        public int GetRemainingMinutes() => 60;
        public void Start(DateTime startTime, int bonusMinutes) => _bonusMinutes = bonusMinutes;
        public void Stop() { }
    }

    private class InMemorySaveGameRepository : ISaveGameRepository
    {
        private readonly Dictionary<string, SaveGame> _saves = new();

        public void Save(SaveGame saveGame)
        {
            _saves[saveGame.Id] = saveGame;
        }

        public SaveGame? Load(string saveId)
        {
            return _saves.TryGetValue(saveId, out var save) ? save : null;
        }

        public List<SaveGame> LoadAll()
        {
            return _saves.Values.ToList();
        }

        public void Delete(string saveId)
        {
            _saves.Remove(saveId);
        }
    }
}
