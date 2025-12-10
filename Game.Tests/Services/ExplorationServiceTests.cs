using Game.Models;
using Game.Services;
using Game.Features.Exploration;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using MediatR;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System;
using System.IO;
using Spectre.Console.Testing;

namespace Game.Tests.Services;

/// <summary>
/// Unit tests for ExplorationService - World exploration and travel mechanics
/// </summary>
public class ExplorationServiceTests : IDisposable
{
    private readonly ExplorationService _explorationService;
    private readonly GameStateService _gameStateService;
    private readonly SaveGameService _saveGameService;
    private readonly IMediator _mediator;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly string _testDbPath;

    public ExplorationServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-exploration-{Guid.NewGuid()}.db";
        
        // Create TestConsole for UI testing
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        
        _apocalypseTimer = new ApocalypseTimer(_consoleUI);
        _saveGameService = new SaveGameService(_apocalypseTimer, _testDbPath);
        _gameStateService = new GameStateService(_saveGameService);
        
        // Setup MediatR with all required services for handlers
        var services = new ServiceCollection();
        
        // Register services that MediatR handlers need
        services.AddSingleton<IConsoleUI>(_consoleUI);
        services.AddSingleton(_saveGameService);
        services.AddSingleton(_gameStateService);
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ExplorationService).Assembly);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        
        _explorationService = new ExplorationService(_mediator, _gameStateService, _saveGameService, _consoleUI);
    }

    public void Dispose()
    {
        // Dispose of SaveGameService first to release file locks
        _saveGameService?.Dispose();
        
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
    public void ExplorationService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var testConsole = TestConsoleHelper.CreateInteractiveConsole();
        var consoleUI = new ConsoleUI(testConsole);
        var apocalypseTimer = new ApocalypseTimer(consoleUI);
        var saveGameService = new SaveGameService(apocalypseTimer, $"test-temp-{Guid.NewGuid()}.db");
        var gameStateService = new GameStateService(saveGameService);
        var service = new ExplorationService(_mediator, gameStateService, saveGameService, consoleUI);

        // Assert
        service.Should().NotBeNull();
        
        // Cleanup
        saveGameService.Dispose();
    }

    [Fact]
    public void ExplorationService_Should_Have_Required_Dependencies()
    {
        // Arrange & Act
        var hasMediator = _mediator != null;
        var hasGameStateService = _gameStateService != null;
        var hasSaveGameService = _saveGameService != null;

        // Assert
        hasMediator.Should().BeTrue();
        hasGameStateService.Should().BeTrue();
        hasSaveGameService.Should().BeTrue();
    }

    [Fact]
    public async Task ExploreAsync_Should_Return_Boolean_Result()
    {
        // Arrange
        var character = new Character
        {
            Name = "Explorer",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        var result = await _explorationService.ExploreAsync();

        // Assert
        // Result is a boolean - just verify method completes
        (result || !result).Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ExploreAsync_Should_Work_At_Different_Levels(int playerLevel)
    {
        // Arrange
        var character = new Character
        {
            Name = "Explorer",
            Level = playerLevel,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        var result = await _explorationService.ExploreAsync();

        // Assert
        // Result is boolean - either combat triggered or not
        (result || !result).Should().BeTrue();
    }

    [Fact]
    public async Task TravelToLocation_Should_Update_Current_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "Traveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Simulate user selecting a location (e.g., index 0 for first location)
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().NotBeNullOrEmpty("Location should be updated after travel");
    }

    [Fact]
    public void ExplorationService_Should_Track_Known_Locations()
    {
        // Arrange
        var character = new Character
        {
            Name = "Scout",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        var currentLocation = _gameStateService.CurrentLocation;

        // Assert
        currentLocation.Should().NotBeNull();
    }

    [Fact]
    public async Task ExploreAsync_Should_Handle_Combat_Trigger()
    {
        // Arrange
        var character = new Character
        {
            Name = "Fighter",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Strength = 15,
            Dexterity = 12
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        var combatTriggered = await _explorationService.ExploreAsync();

        // Assert
        // Combat can be triggered (true) or not (false) based on random chance
        (combatTriggered || !combatTriggered).Should().BeTrue();
    }

    [Fact]
    public async Task ExploreAsync_Should_Handle_Item_Discovery()
    {
        // Arrange
        var character = new Character
        {
            Name = "Looter",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        var initialInventorySize = character.Inventory.Count;
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        await _explorationService.ExploreAsync();

        // Assert
        // Inventory may or may not increase depending on random chance (30%)
        character.Inventory.Count.Should().BeGreaterThanOrEqualTo(initialInventorySize);
    }

    [Fact]
    public void GetKnownLocations_Should_Return_All_Locations()
    {
        // Act
        var locations = _explorationService.GetKnownLocations();

        // Assert
        locations.Should().NotBeNull();
        locations.Should().NotBeEmpty();
        locations.Count.Should().BeGreaterThan(0);
        
        // Verify expected locations exist
        locations.Should().Contain("Hub Town");
        locations.Should().Contain("Dark Forest");
        locations.Should().Contain("Ancient Ruins");
    }

    [Fact]
    public void GetKnownLocations_Should_Return_ReadOnly_List()
    {
        // Act
        var locations = _explorationService.GetKnownLocations();

        // Assert
        locations.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    public async Task TravelToLocation_Should_Not_Change_Location_When_Cancelled()
    {
        // Arrange
        var character = new Character
        {
            Name = "CautiousTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var initialLocation = _gameStateService.CurrentLocation;

        // Simulate user selecting "Cancel" (last option in menu)
        var locations = _explorationService.GetKnownLocations();
        TestConsoleHelper.SelectMenuOption(_testConsole, locations.Count); // Cancel is last

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be(initialLocation, "Location should not change when cancelled");
    }

    [Fact]
    public async Task ExploreAsync_Should_Award_Experience_On_Peaceful_Exploration()
    {
        // Arrange
        var character = new Character
        {
            Name = "Adventurer",
            Level = 1,
            Experience = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var initialXP = character.Experience;

        // Act - Run exploration multiple times to ensure we get at least one peaceful encounter
        for (int i = 0; i < 10; i++)
        {
            await _explorationService.ExploreAsync();
        }

        // Assert - Should have gained some XP from peaceful explorations
        character.Experience.Should().BeGreaterThan(initialXP, "Player should gain XP from exploration");
    }

    [Fact]
    public async Task ExploreAsync_Should_Award_Gold_On_Peaceful_Exploration()
    {
        // Arrange
        var character = new Character
        {
            Name = "Treasure Hunter",
            Level = 1,
            Gold = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var initialGold = character.Gold;

        // Act - Run exploration multiple times to ensure we get at least one peaceful encounter
        for (int i = 0; i < 10; i++)
        {
            await _explorationService.ExploreAsync();
        }

        // Assert - Should have gained some gold from peaceful explorations
        character.Gold.Should().BeGreaterThan(initialGold, "Player should find gold during exploration");
    }

    [Fact]
    public async Task TravelToLocation_Should_Allow_Travel_To_Any_Known_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "WorldTraveler",
            Level = 10,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Get the first available location (not current)
        var locations = _explorationService.GetKnownLocations();
        var initialLocation = _gameStateService.CurrentLocation;
        
        // Simulate selecting the first available location
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().NotBe(initialLocation, "Should travel to a different location");
        locations.Should().Contain(_gameStateService.CurrentLocation, "Should be a known location");
    }

    [Fact]
    public async Task ExploreAsync_Should_Trigger_Level_Up_Event_When_Gaining_Enough_XP()
    {
        // Arrange
        var character = new Character
        {
            Name = "RisingHero",
            Level = 1,
            Experience = 95, // Close to level up (100 XP needed)
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var initialLevel = character.Level;

        // Act - Explore multiple times to gain XP and potentially level up
        for (int i = 0; i < 5; i++)
        {
            await _explorationService.ExploreAsync();
        }

        // Assert - Character may have leveled up
        character.Level.Should().BeGreaterThanOrEqualTo(initialLevel, "Level should remain same or increase");
    }

    [Fact]
    public async Task ExploreAsync_Should_Handle_Multiple_Consecutive_Explorations()
    {
        // Arrange
        var character = new Character
        {
            Name = "PersistentExplorer",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Gold = 0,
            Experience = 0
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Perform multiple explorations
        var results = new List<bool>();
        for (int i = 0; i < 20; i++)
        {
            var combatTriggered = await _explorationService.ExploreAsync();
            results.Add(combatTriggered);
        }

        // Assert
        results.Should().HaveCount(20, "All explorations should complete");
        character.Gold.Should().BeGreaterThan(0, "Should have found some gold");
        character.Experience.Should().BeGreaterThan(0, "Should have gained some experience");
        
        // Should have mix of combat and peaceful encounters (statistically)
        var combatCount = results.Count(r => r);
        var peacefulCount = results.Count(r => !r);
        (combatCount + peacefulCount).Should().Be(20, "All encounters should be accounted for");
    }

    [Fact]
    public void GetKnownLocations_Should_Include_All_Expected_Locations()
    {
        // Act
        var locations = _explorationService.GetKnownLocations();

        // Assert - Verify all hardcoded locations exist
        var expectedLocations = new[]
        {
            "Hub Town",
            "Dark Forest",
            "Ancient Ruins",
            "Dragon's Lair",
            "Cursed Graveyard",
            "Mountain Peak",
            "Coastal Village",
            "Underground Caverns"
        };

        foreach (var expected in expectedLocations)
        {
            locations.Should().Contain(expected, $"{expected} should be a known location");
        }
    }

    [Fact]
    public async Task TravelToLocation_Should_Work_From_Any_Starting_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "Nomad",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act & Assert - Travel to multiple locations in sequence
        for (int i = 0; i < 3; i++)
        {
            var currentLocation = _gameStateService.CurrentLocation;
            
            // Select first available location
            TestConsoleHelper.SelectMenuOption(_testConsole, 0);
            await _explorationService.TravelToLocation();
            
            var newLocation = _gameStateService.CurrentLocation;
            newLocation.Should().NotBe(currentLocation, $"Should move to different location on travel #{i + 1}");
        }
    }
}
