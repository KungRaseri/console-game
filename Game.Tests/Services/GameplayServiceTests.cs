using Game.Models;
using Game.Services;
using Game.Features.Exploration;
using Game.Features.SaveLoad;
using Xunit;
using FluentAssertions;
using System;
using System.IO;
using System.Collections.Generic;

namespace Game.Tests.Services;

/// <summary>
/// Unit tests for GameplayService - Rest and save game operations
/// </summary>
public class GameplayServiceTests : IDisposable
{
    private readonly GameplayService _gameplayService;
    private readonly SaveGameService _saveGameService;
    private readonly string _testDbPath;

    public GameplayServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-gameplay-{Guid.NewGuid()}.db";
        _saveGameService = new SaveGameService(_testDbPath);
        _gameplayService = new GameplayService(_saveGameService);
    }

    public void Dispose()
    {
        // Clean up test database files
        try
        {
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
            
            var logFile = _testDbPath.Replace(".db", "-log.db");
            if (File.Exists(logFile))
                File.Delete(logFile);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void GameplayService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var service = new GameplayService(_saveGameService);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void GameplayService_Should_Have_Required_Dependencies()
    {
        // Assert
        _gameplayService.Should().NotBeNull();
        _saveGameService.Should().NotBeNull();
    }

    [Fact]
    public void Rest_Should_Restore_Health_To_Maximum()
    {
        // Arrange
        var player = new Character
        {
            Name = "TestHero",
            Health = 50,
            MaxHealth = 100,
            Mana = 30,
            MaxMana = 50
        };

        // Act
        _gameplayService.Rest(player);

        // Assert
        player.Health.Should().Be(player.MaxHealth, "Health should be restored to maximum");
    }

    [Fact]
    public void Rest_Should_Restore_Mana_To_Maximum()
    {
        // Arrange
        var player = new Character
        {
            Name = "TestMage",
            Health = 50,
            MaxHealth = 80,
            Mana = 10,
            MaxMana = 100
        };

        // Act
        _gameplayService.Rest(player);

        // Assert
        player.Mana.Should().Be(player.MaxMana, "Mana should be restored to maximum");
    }

    [Fact]
    public void Rest_Should_Restore_Both_Health_And_Mana()
    {
        // Arrange
        var player = new Character
        {
            Name = "TestWarrior",
            Health = 25,
            MaxHealth = 120,
            Mana = 5,
            MaxMana = 30
        };

        // Act
        _gameplayService.Rest(player);

        // Assert
        player.Health.Should().Be(120, "Health should be fully restored");
        player.Mana.Should().Be(30, "Mana should be fully restored");
    }

    [Fact]
    public void Rest_Should_Handle_Already_Full_Health_And_Mana()
    {
        // Arrange
        var player = new Character
        {
            Name = "TestRogue",
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50
        };

        // Act
        _gameplayService.Rest(player);

        // Assert
        player.Health.Should().Be(100, "Health should remain at maximum");
        player.Mana.Should().Be(50, "Mana should remain at maximum");
    }

    [Fact]
    public void Rest_Should_Handle_Null_Player_Gracefully()
    {
        // Act
        Action act = () => _gameplayService.Rest(null!);

        // Assert
        act.Should().NotThrow("Rest should handle null player gracefully");
    }

    [Fact]
    public void Rest_Should_Work_For_Character_With_Zero_Health()
    {
        // Arrange
        var player = new Character
        {
            Name = "DeadHero",
            Health = 0,
            MaxHealth = 100,
            Mana = 0,
            MaxMana = 50
        };

        // Act
        _gameplayService.Rest(player);

        // Assert
        player.Health.Should().Be(100, "Even dead characters should be revived by rest");
        player.Mana.Should().Be(50, "Mana should be fully restored");
    }

    [Fact]
    public void SaveGame_Should_Create_Save_When_Player_Is_Valid()
    {
        // Arrange
        var player = new Character
        {
            Name = "SavedHero",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        var inventory = new List<Item>();

        // Act
        _gameplayService.SaveGame(player, inventory, null);

        // Assert
        var saves = _saveGameService.GetAllSaves();
        saves.Should().NotBeEmpty("A save should have been created");
        saves.Should().ContainSingle(s => s.Character.Name == "SavedHero");
    }

    [Fact]
    public void SaveGame_Should_Update_Existing_Save_When_SaveId_Provided()
    {
        // Arrange
        var player = new Character
        {
            Name = "UpdatedHero",
            Level = 5,
            Health = 80,
            MaxHealth = 100
        };
        var inventory = new List<Item>();

        // Create initial save
        var initialSave = _saveGameService.CreateNewGame(player);
        var saveId = initialSave.Id;

        // Modify player
        player.Level = 10;
        player.Health = 150;
        player.MaxHealth = 150;

        // Act
        _gameplayService.SaveGame(player, inventory, saveId);

        // Assert
        var saves = _saveGameService.GetAllSaves();
        var updatedSave = saves.FirstOrDefault(s => s.Id == saveId);
        updatedSave.Should().NotBeNull();
        updatedSave!.Character.Level.Should().Be(10, "Save should be updated with new level");
        updatedSave.Character.MaxHealth.Should().Be(150, "Save should be updated with new max health");
    }

    [Fact]
    public void SaveGame_Should_Handle_Null_Player_Gracefully()
    {
        // Arrange
        var inventory = new List<Item>();

        // Act
        Action act = () => _gameplayService.SaveGame(null!, inventory, null);

        // Assert
        act.Should().NotThrow("SaveGame should handle null player gracefully");
        
        // Verify no save was created
        var saves = _saveGameService.GetAllSaves();
        saves.Should().BeEmpty("No save should be created for null player");
    }

    [Fact]
    public void SaveGame_Should_Preserve_Inventory()
    {
        // Arrange
        var player = new Character
        {
            Name = "InventoryHero",
            Level = 3
        };
        var inventory = new List<Item>
        {
            new Item { Name = "Sword", Type = ItemType.Weapon },
            new Item { Name = "Health Potion", Type = ItemType.Consumable }
        };

        // Act
        _gameplayService.SaveGame(player, inventory, null);

        // Assert
        var saves = _saveGameService.GetAllSaves();
        var save = saves.FirstOrDefault(s => s.Character.Name == "InventoryHero");
        
        save.Should().NotBeNull();
        save!.Character.Inventory.Should().HaveCount(2, "Inventory should be saved");
        save.Character.Inventory.Should().Contain(i => i.Name == "Sword");
        save.Character.Inventory.Should().Contain(i => i.Name == "Health Potion");
    }

    [Fact]
    public void Rest_Should_Work_For_Different_Character_Classes()
    {
        // Arrange
        var warrior = new Character { Name = "Warrior", ClassName = "Warrior", Health = 50, MaxHealth = 150, Mana = 10, MaxMana = 20 };
        var mage = new Character { Name = "Mage", ClassName = "Mage", Health = 30, MaxHealth = 80, Mana = 30, MaxMana = 150 };
        var rogue = new Character { Name = "Rogue", ClassName = "Rogue", Health = 40, MaxHealth = 100, Mana = 20, MaxMana = 60 };

        // Act
        _gameplayService.Rest(warrior);
        _gameplayService.Rest(mage);
        _gameplayService.Rest(rogue);

        // Assert
        warrior.Health.Should().Be(150);
        warrior.Mana.Should().Be(20);
        
        mage.Health.Should().Be(80);
        mage.Mana.Should().Be(150);
        
        rogue.Health.Should().Be(100);
        rogue.Mana.Should().Be(60);
    }

    [Fact]
    public void SaveGame_Should_Preserve_Player_Stats()
    {
        // Arrange
        var player = new Character
        {
            Name = "StatsHero",
            Level = 7,
            Experience = 1500,
            Health = 120,
            MaxHealth = 150,
            Mana = 80,
            MaxMana = 100,
            ClassName = "Paladin"
        };
        var inventory = new List<Item>();

        // Act
        _gameplayService.SaveGame(player, inventory, null);

        // Assert
        var saves = _saveGameService.GetAllSaves();
        var save = saves.FirstOrDefault(s => s.Character.Name == "StatsHero");
        
        save.Should().NotBeNull();
        save!.Character.Level.Should().Be(7);
        save.Character.Experience.Should().Be(1500);
        save.Character.Health.Should().Be(120);
        save.Character.MaxHealth.Should().Be(150);
        save.Character.Mana.Should().Be(80);
        save.Character.MaxMana.Should().Be(100);
        save.Character.ClassName.Should().Be("Paladin");
    }

    [Fact]
    public void SaveGame_Should_Handle_Empty_Inventory()
    {
        // Arrange
        var player = new Character { Name = "EmptyInventoryHero", Level = 1 };
        var emptyInventory = new List<Item>();

        // Act
        _gameplayService.SaveGame(player, emptyInventory, null);

        // Assert
        var saves = _saveGameService.GetAllSaves();
        var save = saves.FirstOrDefault(s => s.Character.Name == "EmptyInventoryHero");
        
        save.Should().NotBeNull();
        save!.Character.Inventory.Should().BeEmpty();
    }
}
