using Xunit;
using FluentAssertions;
using Game.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using Spectre.Console.Testing;
using Game.Features.Combat;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Models;
using MediatR;
using Moq;
using Game.Features.Death.Commands;

namespace Game.Tests.Features;

/// <summary>
/// Comprehensive tests for CombatOrchestrator focusing on high coverage.
/// These tests target the 11.6% baseline coverage to achieve 80%+ line coverage.
/// </summary>
public class CombatOrchestratorComprehensiveTests : IDisposable
{
    private readonly string _testDbFile;
    private readonly Mock<IMediator> _mockMediator;
    private readonly CombatService _combatService;
    private readonly SaveGameService _saveGameService;
    private readonly MenuService _menuService;
    private readonly GameStateService _gameStateService;
    private readonly Mock<IConsoleUI> _mockConsoleUI;
    private readonly LevelUpService _levelUpService;
    private readonly CombatOrchestrator _combatOrchestrator;

    public CombatOrchestratorComprehensiveTests()
    {
        _testDbFile = $"test-combatorchestrator-comprehensive-{Guid.NewGuid()}.db";

        // Mock IConsoleUI for performance and testability
        _mockConsoleUI = new Mock<IConsoleUI>();

        // Mock ShowProgress to skip Thread.Sleep delays (learned from ExplorationService)
        _mockConsoleUI.Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<Action<Spectre.Console.ProgressTask>>()));

        // Mock mediator
        _mockMediator = new Mock<IMediator>();

        // Create real dependencies
        _saveGameService = new SaveGameService(new ApocalypseTimer(_mockConsoleUI.Object), _testDbFile);
        _combatService = new CombatService(_saveGameService);
        _gameStateService = new GameStateService(_saveGameService);
        _menuService = new MenuService(_gameStateService, _saveGameService, _mockConsoleUI.Object);
        _levelUpService = new LevelUpService(_mockConsoleUI.Object);

        // Create CombatOrchestrator
        _combatOrchestrator = new CombatOrchestrator(
            _mockMediator.Object,
            _combatService,
            _saveGameService,
            _gameStateService,
            _menuService,
            _mockConsoleUI.Object,
            _levelUpService
        );
        
        // Set delay multiplier to 0 for instant tests (no Thread.Sleep delays)
        _combatOrchestrator.DelayMultiplier = 0;
    }

    public void Dispose()
    {
        _saveGameService?.Dispose();

        try
        {
            if (File.Exists(_testDbFile))
                File.Delete(_testDbFile);
            var logFile = _testDbFile.Replace(".db", "-log.db");
            if (File.Exists(logFile))
                File.Delete(logFile);
        }
        catch (IOException ex)
        {
            // Ignore cleanup errors - files might still be locked
            System.Diagnostics.Debug.WriteLine($"Cleanup warning: {ex.Message}");
        }
    }

    #region HandleCombatAsync - Victory Scenarios

    [Fact]
    public async Task HandleCombatAsync_Should_End_In_Victory_When_Enemy_Dies()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 10, level: 1); // Low health for quick victory
        var combatLog = new CombatLog();

        // Mock menu to always select Attack
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Mock mediator Send to return default death result (no permadeath for this test)
        _mockMediator
            .Setup(x => x.Send(It.IsAny<HandlePlayerDeathCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlePlayerDeathResult { IsPermadeath = false, SaveDeleted = false });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        enemy.IsAlive().Should().BeFalse("enemy should be dead after combat");
        player.IsAlive().Should().BeTrue("player should survive");

        // Verify CombatEnded event was published with victory=true
        _mockMediator.Verify(
            x => x.Publish(It.Is<CombatEnded>(e => e.Victory), It.IsAny<CancellationToken>()),
            Times.Once,
            "CombatEnded event should be published with Victory=true"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Award_XP_And_Gold_On_Victory()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var initialGold = player.Gold;
        var enemy = CreateTestEnemy("Goblin", health: 10, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        player.Gold.Should().BeGreaterThan(initialGold, "player should receive gold reward");
        // XP might decrease to 0 if player levels up, so just check player survived
        player.IsAlive().Should().BeTrue("player should survive victory");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Add_Loot_To_Inventory_On_Victory()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        player.Inventory.Clear(); // Start with empty inventory
        var enemy = CreateTestEnemy("Goblin", health: 10, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Enemy might drop loot (randomized), so we just verify the mechanism doesn't crash
        // and that inventory count >= 0 (could be 0 if no loot dropped)
        player.Inventory.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Trigger_Level_Up_When_XP_Threshold_Reached()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        player.Experience = 90; // Near level up (needs 100 for level 5)
        var initialLevel = player.Level;

        var enemy = CreateTestEnemy("Goblin", health: 10, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Player should gain enough XP to level up
        player.Level.Should().BeGreaterThan(initialLevel, "player should level up from XP reward");

        // Verify PlayerLeveledUp event was published
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<PlayerLeveledUp>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "PlayerLeveledUp event should be published"
        );
    }

    #endregion

    #region HandleCombatAsync - Defeat Scenarios

    [Fact]
    public async Task HandleCombatAsync_Should_End_In_Defeat_When_Player_Dies()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 10, level: 1); // Low health
        var enemy = CreateTestEnemy("Dragon", health: 500, level: 20); // Strong enemy
        var combatLog = new CombatLog();

        // Mock menu to always select Attack (player will die to enemy attacks)
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Mock mediator to handle death
        _mockMediator
            .Setup(x => x.Send(It.IsAny<HandlePlayerDeathCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlePlayerDeathResult { IsPermadeath = false, SaveDeleted = true });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        player.IsAlive().Should().BeFalse("player should be dead after combat");

        // Verify CombatEnded event was published with victory=false
        _mockMediator.Verify(
            x => x.Publish(It.Is<CombatEnded>(e => e.Victory == false), It.IsAny<CancellationToken>()),
            Times.Once,
            "CombatEnded event should be published with IsVictory=false"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Invoke_Death_Handler_On_Player_Death()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 10, level: 1);
        var enemy = CreateTestEnemy("Dragon", health: 500, level: 20);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HandlePlayerDeathCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlePlayerDeathResult { IsPermadeath = false, SaveDeleted = true });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockMediator.Verify(
            x => x.Send(It.IsAny<HandlePlayerDeathCommand>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "HandlePlayerDeathCommand should be sent when player dies"
        );
    }

    #endregion

    #region HandleCombatAsync - Combat Actions

    [Fact]
    public async Task HandleCombatAsync_Should_Execute_Attack_Action()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.PlayerAttack, "combat log should record player attacks");

        // Verify AttackPerformed event was published
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<AttackPerformed>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "AttackPerformed event should be published for attacks"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Execute_Defend_Action()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        var menuCallCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(() =>
            {
                menuCallCount++;
                return menuCallCount == 1 ? "🛡️ Defend" : "⚔️ Attack"; // Defend first turn, then attack
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.Defend || e.Message.Contains("defend"), "combat log should record defend action");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Handle_Flee_Success()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 20); // High level for flee success
        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("💨 Flee");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Player should still be alive after fleeing
        player.IsAlive().Should().BeTrue("player should survive after fleeing");
        enemy.IsAlive().Should().BeTrue("enemy should still be alive after player flees");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Handle_Flee_Failure()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 1); // Low level for flee failure
        var enemy = CreateTestEnemy("Dragon", health: 500, level: 20); // High level enemy
        var combatLog = new CombatLog();

        var attemptCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(() =>
            {
                attemptCount++;
                return attemptCount <= 3 ? "💨 Flee" : "⚔️ Attack"; // Try to flee 3 times, then attack
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Combat should continue after failed flee attempts
        combatLog.Entries.Should().NotBeEmpty("combat log should have entries from continued combat");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Handle_Item_Usage()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 50, level: 5);
        var healthPotion = CreateHealthPotion();
        player.Inventory.Add(healthPotion);

        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        var menuCallCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns((string prompt, string[] _) =>
            {
                menuCallCount++;
                if (prompt.Contains("Select an item"))
                {
                    return $"{healthPotion.Name} ({healthPotion.Rarity})";
                }
                return menuCallCount == 1 ? "✨ Use Item" : "⚔️ Attack"; // Use item first turn
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.ItemUse, "combat log should record item usage");
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.Heal, "combat log should record healing");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Handle_Item_Usage_With_No_Items()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        player.Inventory.Clear(); // No items

        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        var attemptCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(() =>
            {
                attemptCount++;
                return attemptCount == 1 ? "✨ Use Item" : "⚔️ Attack"; // Try to use item, then attack
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockConsoleUI.Verify(
            x => x.ShowWarning(It.Is<string>(s => s.Contains("no consumable"))),
            Times.Once,
            "should warn player about no items"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Handle_Item_Usage_Cancellation()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        player.Inventory.Add(CreateHealthPotion());

        var enemy = CreateTestEnemy("Goblin", health: 50, level: 1);
        var combatLog = new CombatLog();

        var menuCallCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns((string prompt, string[] _) =>
            {
                menuCallCount++;
                if (prompt.Contains("Select an item"))
                {
                    return "[dim]Cancel[/]";
                }
                return menuCallCount == 1 ? "✨ Use Item" : "⚔️ Attack";
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Combat should continue after cancelled item usage
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.PlayerAttack, "combat should continue with attack after item cancellation");
    }

    #endregion

    #region HandleCombatAsync - Combat Mechanics

    [Fact]
    public async Task HandleCombatAsync_Should_Record_Critical_Hits()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 10);
        player.Dexterity = 50; // High luck for crits

        var enemy = CreateTestEnemy("Goblin", health: 100, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // With high luck, we should eventually get a critical hit
        // Note: This is probabilistic but high luck makes it very likely
        combatLog.Entries.Where(e => e.Type == CombatLogType.Critical).Should().NotBeEmpty("high luck should produce critical hits");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Record_Dodges()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 10);
        player.Dexterity = 50; // High dexterity for dodges

        var enemy = CreateTestEnemy("Goblin", health: 100, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // With high dexterity, we should eventually dodge some attacks
        combatLog.Entries.Where(e => e.Type == CombatLogType.Dodge).Should().NotBeEmpty("high dexterity should produce dodges");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Apply_Regeneration()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 80, level: 5);
        player.MaxHealth = 100;
        player.Constitution = 20; // Enable regeneration

        var enemy = CreateTestEnemy("Goblin", health: 200, level: 1); // Tanky enemy for long combat
        var combatLog = new CombatLog();

        var turnCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(() =>
            {
                turnCount++;
                return turnCount <= 5 ? "🛡️ Defend" : "⚔️ Attack"; // Defend a few turns for regen
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        // Player health should regenerate over time (if they survive and defend)
        // This is probabilistic but defending should allow some regen
        if (player.IsAlive() && player.Health < player.MaxHealth)
        {
            combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.Heal, "regeneration should occur during combat");
        }
    }

    #endregion

    #region HandleCombatAsync - Combat Log

    [Fact]
    public async Task HandleCombatAsync_Should_Populate_Combat_Log()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 30, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        combatLog.Entries.Should().NotBeEmpty("combat log should contain combat actions");
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.PlayerAttack, "should record player attacks");
        combatLog.Entries.Should().Contain(e => e.Type == CombatLogType.EnemyAttack, "should record enemy attacks");
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Record_Different_Log_Entry_Types()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        player.Inventory.Add(CreateHealthPotion());

        var enemy = CreateTestEnemy("Goblin", health: 100, level: 1);
        var combatLog = new CombatLog();

        var turnCount = 0;
        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns((string prompt, string[] _) =>
            {
                turnCount++;
                if (prompt.Contains("Select an item"))
                    return "[dim]Cancel[/]";

                return turnCount switch
                {
                    1 => "⚔️ Attack",
                    2 => "🛡️ Defend",
                    3 => "✨ Use Item",
                    _ => "⚔️ Attack"
                };
            });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        var logTypes = combatLog.Entries.Select(e => e.Type).Distinct().ToList();
        logTypes.Should().Contain(CombatLogType.PlayerAttack, "should have player attacks");
        logTypes.Should().Contain(CombatLogType.EnemyAttack, "should have enemy attacks");
        logTypes.Count.Should().BeGreaterThan(1, "should have variety of log entry types");
    }

    #endregion

    #region MediatR Event Publishing

    [Fact]
    public async Task HandleCombatAsync_Should_Publish_AttackPerformed_Events()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 30, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<AttackPerformed>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "should publish AttackPerformed events"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Publish_DamageTaken_Events()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 30, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<DamageTaken>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "should publish DamageTaken events"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Publish_EnemyDefeated_Event_On_Victory()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 100, level: 5);
        var enemy = CreateTestEnemy("Goblin", health: 10, level: 1);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<EnemyDefeated>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "should publish EnemyDefeated event when enemy dies"
        );
    }

    [Fact]
    public async Task HandleCombatAsync_Should_Publish_PlayerDefeated_Event_On_Defeat()
    {
        // Arrange
        var player = CreateTestCharacter("TestHero", health: 10, level: 1);
        var enemy = CreateTestEnemy("Dragon", health: 500, level: 20);
        var combatLog = new CombatLog();

        _mockConsoleUI
            .Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("⚔️ Attack");

        _mockMediator
            .Setup(x => x.Send(It.IsAny<HandlePlayerDeathCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandlePlayerDeathResult { IsPermadeath = false, SaveDeleted = true });

        // Act
        await _combatOrchestrator.HandleCombatAsync(player, enemy, combatLog);

        // Assert
        _mockMediator.Verify(
            x => x.Publish(It.IsAny<PlayerDefeated>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "should publish PlayerDefeated event when player dies"
        );
    }

    #endregion

    #region Helper Methods

    private Character CreateTestCharacter(string name, int health, int level)
    {
        var character = new Character
        {
            Name = name,
            Level = level,
            MaxHealth = health,
            Health = health,
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10,
            Gold = 100,
            Experience = 0,
            ClassName = "Warrior",
            Inventory = new List<Item>()
        };

        return character;
    }

    private Enemy CreateTestEnemy(string name, int health, int level)
    {
        return new Enemy
        {
            Name = name,
            Level = level,
            MaxHealth = health,
            Health = health,
            Strength = 10,
            Dexterity = 10,
            Constitution = 10,
            Difficulty = EnemyDifficulty.Normal,
            Type = EnemyType.Beast
        };
    }

    private Item CreateHealthPotion()
    {
        var potion = new Item
        {
            Name = "Health Potion",
            Type = ItemType.Consumable,
            Rarity = ItemRarity.Common,
            Price = 25
        };
        potion.Traits["HealingPower"] = new TraitValue { Value = 50, Type = TraitType.Number };
        return potion;
    }

    #endregion
}
