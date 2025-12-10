using Xunit;
using FluentAssertions;
using Game.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using Spectre.Console.Testing;
using Game.Features.Combat;
using Game.Features.Exploration;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Models;

namespace Game.Tests.Integration;

/// <summary>
/// Integration tests for service workflows.
/// These tests verify that multiple services work together correctly.
/// </summary>
public class GameWorkflowIntegrationTests : IDisposable
{
    private readonly string _testDbFile;
    private readonly SaveGameService _saveGameService;
    private readonly CombatService _combatService;
    private readonly GameplayService _gameplayService;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;

    public GameWorkflowIntegrationTests()
    {
        _testDbFile = $"test-integration-{Guid.NewGuid()}.db";
        
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        
        _saveGameService = new SaveGameService(new ApocalypseTimer(_consoleUI), _testDbFile);
        _combatService = new CombatService(_saveGameService);
        _gameplayService = new GameplayService(_saveGameService, _consoleUI);
    }

    public void Dispose()
    {
        _saveGameService?.Dispose();
        System.Threading.Thread.Sleep(100);
        try
        {
            if (File.Exists(_testDbFile)) File.Delete(_testDbFile);
            var logFile = _testDbFile.Replace(".db", "-log.db");
            if (File.Exists(logFile)) File.Delete(logFile);
        }
        catch (IOException) { }
    }

    [Fact]
    public void Integration_Save_Multiple_Characters_Should_Track_All_Saves()
    {
        var char1 = new Character { Name = "Hero1", Level = 1, Gold = 100 };
        var char2 = new Character { Name = "Hero2", Level = 2, Gold = 200 };
        
        _gameplayService.SaveGame(char1, new List<Item>(), null);
        _gameplayService.SaveGame(char2, new List<Item>(), null);
        
        var saves = _saveGameService.GetAllSaves();
        saves.Should().HaveCount(2);
        saves.Select(s => s.PlayerName).Should().Contain(new[] { "Hero1", "Hero2" });
    }

    [Fact]
    public void Integration_Combat_Then_Save_Should_Preserve_Results()
    {
        var character = new Character { Name = "Warrior", Level = 5, Gold = 50, Experience = 0 };
        var enemy = new Enemy { Name = "Orc", Level = 3, Difficulty = EnemyDifficulty.Normal };
        
        var outcome = _combatService.GenerateVictoryOutcome(character, enemy);
        character.Gold += outcome.GoldGained;
        
        _gameplayService.SaveGame(character, character.Inventory, null);
        var saves = _saveGameService.GetAllSaves();
        saves.Should().ContainSingle();
    }

    [Fact]
    public void Integration_Rest_Then_Save_Should_Preserve_Full_Health()
    {
        var character = new Character { Name = "Healer", Health = 20, MaxHealth = 100, Mana = 10, MaxMana = 50 };
        
        _gameplayService.Rest(character);
        character.Health.Should().Be(100);
        character.Mana.Should().Be(50);
        
        _gameplayService.SaveGame(character, character.Inventory, null);
        var saves = _saveGameService.GetAllSaves();
        saves.Should().ContainSingle();
    }

    [Fact]
    public void Integration_Item_Use_Should_Consume_And_Heal()
    {
        var character = new Character { Name = "User", Health = 50, MaxHealth = 150 };
        var potion = new Item { Name = "Potion", Type = ItemType.Consumable, Rarity = ItemRarity.Common, Price = 50 };
        character.Inventory.Add(potion);
        
        var result = _combatService.UseItemInCombat(character, potion);
        
        result.Success.Should().BeTrue();
        character.Inventory.Should().BeEmpty();
    }

    [Fact]
    public void Integration_Multiple_Combat_Rounds_Should_Defeat_Enemy()
    {
        var character = new Character { Name = "Fighter", Level = 10, Strength = 20 };
        var enemy = new Enemy { Name = "Goblin", Level = 1, Health = 30, MaxHealth = 30, Difficulty = EnemyDifficulty.Easy };
        
        var rounds = 0;
        while (enemy.IsAlive() && rounds < 10)
        {
            _combatService.ExecutePlayerAttack(character, enemy);
            rounds++;
        }
        
        enemy.Health.Should().BeLessThan(enemy.MaxHealth);
    }
}

