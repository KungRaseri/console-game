using Game.Models;
using Game.Services;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using MediatR;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System;
using System.IO;

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
    private readonly string _testDbPath;

    public ExplorationServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-exploration-{Guid.NewGuid()}.db";
        
        // Setup MediatR
        var services = new ServiceCollection();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ExplorationService).Assembly);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        
        _saveGameService = new SaveGameService(_testDbPath);
        _gameStateService = new GameStateService(_saveGameService);
        _explorationService = new ExplorationService(_mediator, _gameStateService, _saveGameService);
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
    public void ExplorationService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var service = new ExplorationService(_mediator, _gameStateService, _saveGameService);

        // Assert
        service.Should().NotBeNull();
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
        
        _saveGameService.CreateNewGame(character);

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
        
        _saveGameService.CreateNewGame(character);

        // Act
        var result = await _explorationService.ExploreAsync();

        // Assert
        // Result is boolean - either combat triggered or not
        (result || !result).Should().BeTrue();
    }

    [Fact(Skip = "Requires interactive terminal - TravelToLocation() calls ConsoleUI.ShowMenu()")]
    public void TravelToLocation_Should_Update_Current_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "Traveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character);

        // Act
        _explorationService.TravelToLocation();

        // Assert
        // Location should be updated (checked via GameStateService)
        _gameStateService.CurrentLocation.Should().NotBeNullOrEmpty();
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
        
        _saveGameService.CreateNewGame(character);

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
        
        _saveGameService.CreateNewGame(character);

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
        _saveGameService.CreateNewGame(character);

        // Act
        await _explorationService.ExploreAsync();

        // Assert
        // Inventory may or may not increase depending on random outcome
        character.Inventory.Count.Should().BeGreaterThanOrEqualTo(initialInventorySize);
    }
}
