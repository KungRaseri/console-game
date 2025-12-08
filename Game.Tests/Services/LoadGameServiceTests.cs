using Game.Services;
using Game.Features.SaveLoad;
using Xunit;
using FluentAssertions;
using System;
using System.IO;

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
    private readonly string _testDbPath;

    public LoadGameServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-loadgame-{Guid.NewGuid()}.db";
        _saveGameService = new SaveGameService(_testDbPath);
        _loadGameService = new LoadGameService(_saveGameService);
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
    public void LoadGameService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var service = new LoadGameService(_saveGameService);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void LoadGameService_Should_Have_Required_Dependencies()
    {
        // Assert
        _loadGameService.Should().NotBeNull();
        _saveGameService.Should().NotBeNull();
    }

    [Fact(Skip = "LoadGameAsync requires interactive terminal - calls ConsoleUI.ShowMenu()")]
    public async Task LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist()
    {
        // Note: This test is skipped because LoadGameAsync calls ConsoleUI methods
        // which require an interactive terminal.
        
        // If we could test it, it would look like:
        // var (save, success) = await _loadGameService.LoadGameAsync();
        // save.Should().BeNull();
        // success.Should().BeFalse();
        
        await Task.CompletedTask;
    }

    [Fact(Skip = "LoadGameAsync requires interactive terminal - calls ConsoleUI.ShowTable() and ShowMenu()")]
    public async Task LoadGameAsync_Should_Display_Available_Saves_When_Saves_Exist()
    {
        // Note: This test is skipped because LoadGameAsync calls ConsoleUI methods
        // which require an interactive terminal.
        
        // If we could test it, we would:
        // 1. Create some test saves
        // 2. Call LoadGameAsync()
        // 3. Verify that saves are displayed in a table
        // 4. Verify menu options are shown
        
        await Task.CompletedTask;
    }

    [Fact(Skip = "DeleteSaveAsync is private and requires interactive terminal")]
    public async Task DeleteSaveAsync_Should_Delete_Save_With_Confirmation()
    {
        // Note: This test is skipped because:
        // 1. DeleteSaveAsync is a private method
        // 2. It calls ConsoleUI.Confirm() which requires an interactive terminal
        
        // If we could test it with reflection, we would:
        // 1. Create a test save
        // 2. Call DeleteSaveAsync via reflection
        // 3. Verify save is deleted after confirmation
        
        await Task.CompletedTask;
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
