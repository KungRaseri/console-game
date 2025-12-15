using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using Game.Shared.Services;
using Game.Console.UI;
using Game.Core.Abstractions;
using Game.Tests.Helpers;
using Game.Core.Models;
using Xunit;
using FluentAssertions;
using System;
using System.IO;
using Spectre.Console.Testing;

namespace Game.Tests.Services;

/// <summary>
/// Unit tests for LoadGameService - Save game loading and deletion
/// Note: Most LoadGameService methods require interactive UI (ConsoleUI), 
/// so these tests focus on instantiation, dependencies, and basic structure.
/// </summary>
public class LoadGameServiceTests : IDisposable
{
    private readonly LoadGameService _loadGameService;
    private readonly SaveGameService _saveGameService;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;
    private readonly string _testDbPath;

    public LoadGameServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-loadgame-{Guid.NewGuid()}.db";
        
        // Create TestConsole for UI testing
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        
        // Create services with TestConsole
        _apocalypseTimer = new ApocalypseTimer(_consoleUI);
        _saveGameService = new SaveGameService(_apocalypseTimer, _testDbPath);
        _loadGameService = new LoadGameService(_saveGameService, _apocalypseTimer, _consoleUI);
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
    public void LoadGameService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var testConsole = TestConsoleHelper.CreateInteractiveConsole();
        var ConsoleUI = new ConsoleUI(testConsole);
        var apocalypseTimer = new ApocalypseTimer(ConsoleUI);
        var saveGameService = new SaveGameService(apocalypseTimer, $"test-temp-{Guid.NewGuid()}.db");
        var service = new LoadGameService(saveGameService, apocalypseTimer, ConsoleUI);

        // Assert
        service.Should().NotBeNull();
        
        // Cleanup
        saveGameService.Dispose();
    }

    [Fact]
    public void LoadGameService_Should_Have_Required_Dependencies()
    {
        // Assert
        _loadGameService.Should().NotBeNull();
        _saveGameService.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist()
    {
        // Arrange
        // Simulate user selecting "Go Back" option (index 0 when no saves exist)
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        var (save, success) = await _loadGameService.LoadGameAsync();

        // Assert
        save.Should().BeNull("No saves exist to load");
        success.Should().BeFalse("User selected Go Back");
        
        // Verify console output shows appropriate message
        var output = TestConsoleHelper.GetOutput(_testConsole);
        output.Should().Contain("No saved games found", "Should inform user no saves exist");
    }

    [Fact]
    public async Task LoadGameAsync_Should_Display_Available_Saves_When_Saves_Exist()
    {
        // Arrange - Create test save and persist it to database
        var testCharacter = new Character
        {
            Name = "TestHero",
            ClassName = "Warrior",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50
        };
        var saveGame = _saveGameService.CreateNewGame(testCharacter, DifficultySettings.Normal);
        _saveGameService.SaveGame(saveGame); // Persist to database

        // Simulate user selecting "Go Back" option to avoid loading
        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Save is at index 0, Delete at 1, Go Back at index 2

        // Act
        var (save, success) = await _loadGameService.LoadGameAsync();

        // Assert
        save.Should().BeNull("User selected Go Back instead of loading");
        success.Should().BeFalse();
        
        // Verify console output shows the save in a table
        var output = TestConsoleHelper.GetOutput(_testConsole);
        output.Should().Contain("TestHero", "Save should be displayed");
        output.Should().Contain("Level", "Table should show level column");
    }

    [Fact]
    public void LoadGameService_Should_Work_With_Empty_SaveGameService()
    {
        // Arrange
        var saves = _saveGameService.GetAllSaves();

        // Assert
        saves.Should().BeEmpty("No saves have been created yet");
        _loadGameService.Should().NotBeNull();
    }
}
