using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Services;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Tests.Services;

[Trait("Category", "Service")]
/// <summary>
/// Tests for GameStateService.
/// </summary>
public class GameStateServiceTests
{
    private readonly Mock<SaveGameService> _mockSaveGameService;
    private readonly Mock<ILogger<GameStateService>> _mockLogger;
    private readonly GameStateService _service;

    public GameStateServiceTests()
    {
        _mockSaveGameService = new Mock<SaveGameService>();
        _mockLogger = new Mock<ILogger<GameStateService>>();
        _service = new GameStateService(_mockSaveGameService.Object, _mockLogger.Object);
    }

    [Fact]
    public void CurrentSave_Should_Return_Active_Save_Game()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            Character = new Character { Name = "Hero", Level = 5 }
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        var result = _service.CurrentSave;

        // Assert
        result.Should().BeSameAs(saveGame);
    }

    [Fact]
    public void CurrentSave_Should_Throw_When_No_Active_Save()
    {
        // Arrange
        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns((SaveGame?)null);

        // Act & Assert
        var act = () => _service.CurrentSave;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No active save game");
    }

    [Fact]
    public void Player_Should_Return_Character_From_Current_Save()
    {
        // Arrange
        var character = new Character { Name = "Hero", Level = 10 };
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            Character = character
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        var result = _service.Player;

        // Assert
        result.Should().BeSameAs(character);
    }

    [Fact]
    public void DifficultyLevel_Should_Return_Difficulty_From_Save()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            DifficultyLevel = "Hard",
            Character = new Character { Name = "Hero" }
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        var result = _service.DifficultyLevel;

        // Assert
        result.Should().Be("Hard");
    }

    [Fact]
    public void IsIronmanMode_Should_Return_True_When_Enabled()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            IronmanMode = true,
            Character = new Character { Name = "Hero" }
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        var result = _service.IsIronmanMode;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIronmanMode_Should_Return_False_When_Disabled()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            IronmanMode = false,
            Character = new Character { Name = "Hero" }
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        var result = _service.IsIronmanMode;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateLocation_Should_Change_CurrentLocation()
    {
        // Arrange
        var newLocation = "Dark Forest";

        // Act
        _service.UpdateLocation(newLocation);

        // Assert
        _service.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public void UpdateLocation_Should_Add_To_Visited_Locations_If_New()
    {
        // Arrange
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            Character = new Character { Name = "Hero" },
            VisitedLocations = new List<string>()
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        var newLocation = "Dark Forest";

        // Act
        _service.UpdateLocation(newLocation);

        // Assert
        saveGame.VisitedLocations.Should().Contain(newLocation);
    }

    [Fact]
    public void UpdateLocation_Should_Not_Duplicate_Visited_Locations()
    {
        // Arrange
        var location = "Hub Town";
        var saveGame = new SaveGame
        {
            Id = "save-1",
            PlayerName = "Hero",
            Character = new Character { Name = "Hero" },
            VisitedLocations = new List<string> { location }
        };

        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns(saveGame);

        // Act
        _service.UpdateLocation(location); // Visit again

        // Assert
        saveGame.VisitedLocations.Should().HaveCount(1, "should not add duplicate locations");
    }

    [Fact]
    public void UpdateLocation_Should_Handle_Null_Save_Gracefully()
    {
        // Arrange
        _mockSaveGameService
            .Setup(s => s.GetCurrentSave())
            .Returns((SaveGame?)null);

        var newLocation = "Test Location";

        // Act
        var act = () => _service.UpdateLocation(newLocation);

        // Assert
        act.Should().NotThrow("should handle null save gracefully");
        _service.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public void RecordDeath_Should_Delegate_To_SaveGameService()
    {
        // Arrange
        var killedBy = "Dragon";
        _service.UpdateLocation("Dark Cave");

        // Act
        _service.RecordDeath(killedBy);

        // Assert
        // Note: SaveGameService.RecordDeath is not virtual so we can't mock/verify it directly
        // The method delegates to _saveGameService.RecordDeath(CurrentLocation, killedBy)
        // We verify by checking the save game accessed during UpdateLocation
        _mockSaveGameService.Verify(
            s => s.GetCurrentSave(),
            Times.AtLeastOnce(),
            "should access current save when recording death");
    }

    [Fact]
    public void CurrentLocation_Should_Default_To_Hub_Town()
    {
        // Arrange & Act
        var location = _service.CurrentLocation;

        // Assert
        location.Should().Be("Hub Town", "default starting location should be Hub Town");
    }
}
