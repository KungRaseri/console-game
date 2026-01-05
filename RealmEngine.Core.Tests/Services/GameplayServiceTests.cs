using RealmEngine.Core.Features.Exploration;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Abstractions;
using RealmEngine.Shared.Models;
using Moq;
using FluentAssertions;
using Xunit;

namespace RealmEngine.Core.Tests.Services;

[Trait("Category", "Service")]
public class GameplayServiceTests
{
    private readonly Mock<SaveGameService> _mockSaveGameService;
    private readonly Mock<IGameUI> _mockGameUI;
    private readonly GameplayService _service;

    public GameplayServiceTests()
    {
        _mockSaveGameService = new Mock<SaveGameService>();
        _mockGameUI = new Mock<IGameUI>();
        _service = new GameplayService(_mockSaveGameService.Object, _mockGameUI.Object);
    }

    [Fact]
    public void Rest_Should_Restore_Health_To_Maximum()
    {
        // Arrange
        var player = new Character
        {
            Name = "Test Player",
            Health = 50,
            MaxHealth = 100,
            Mana = 30,
            MaxMana = 100
        };

        // Act
        _service.Rest(player);

        // Assert
        player.Health.Should().Be(player.MaxHealth, "rest should restore health to maximum");
    }

    [Fact]
    public void Rest_Should_Restore_Mana_To_Maximum()
    {
        // Arrange
        var player = new Character
        {
            Name = "Test Player",
            Health = 50,
            MaxHealth = 100,
            Mana = 30,
            MaxMana = 100
        };

        // Act
        _service.Rest(player);

        // Assert
        player.Mana.Should().Be(player.MaxMana, "rest should restore mana to maximum");
    }

    [Fact]
    public void Rest_Should_Display_Messages()
    {
        // Arrange
        var player = new Character { Name = "Test Player", Health = 50, MaxHealth = 100, Mana = 30, MaxMana = 100 };

        // Act
        _service.Rest(player);

        // Assert
        _mockGameUI.Verify(ui => ui.ShowInfo(It.Is<string>(s => s.Contains("rest"))), Times.Once);
        _mockGameUI.Verify(ui => ui.ShowSuccess("Fully rested!"), Times.Once);
    }

    [Fact]
    public void Rest_Should_Handle_Null_Player_Gracefully()
    {
        // Act
        var act = () => _service.Rest(null!);

        // Assert
        act.Should().NotThrow("should handle null player gracefully");
    }

    [Fact]
    public void SaveGame_Should_Call_SaveGameService()
    {
        // Arrange
        var player = new Character { Name = "Test Player" };
        var inventory = new List<Item>();
        var saveId = "save-123";

        // Act
        _service.SaveGame(player, inventory, saveId);

        // Assert
        _mockSaveGameService.Verify(
            s => s.SaveGame(player, inventory, saveId),
            Times.Once,
            "should delegate to SaveGameService");
    }

    [Fact]
    public void SaveGame_Should_Display_Success_Message()
    {
        // Arrange
        var player = new Character { Name = "Test Player" };
        var inventory = new List<Item>();
        _mockSaveGameService.Setup(s => s.SaveGame(It.IsAny<Character>(), It.IsAny<List<Item>>(), It.IsAny<string>()));

        // Act
        _service.SaveGame(player, inventory, null);

        // Assert
        _mockGameUI.Verify(ui => ui.ShowInfo(It.Is<string>(s => s.Contains("Saving"))), Times.Once);
        _mockGameUI.Verify(ui => ui.ShowSuccess(It.Is<string>(s => s.Contains("saved successfully"))), Times.Once);
    }

    [Fact]
    public void SaveGame_Should_Display_Error_Message_On_Failure()
    {
        // Arrange
        var player = new Character { Name = "Test Player" };
        var inventory = new List<Item>();
        _mockSaveGameService
            .Setup(s => s.SaveGame(It.IsAny<Character>(), It.IsAny<List<Item>>(), It.IsAny<string>()))
            .Throws(new Exception("Disk error"));

        // Act
        _service.SaveGame(player, inventory, null);

        // Assert
        _mockGameUI.Verify(ui => ui.ShowError(It.Is<string>(s => s.Contains("Failed to save"))), Times.Once);
    }

    [Fact]
    public void SaveGame_Should_Show_Error_When_Player_Is_Null()
    {
        // Arrange
        var inventory = new List<Item>();

        // Act
        _service.SaveGame(null!, inventory, null);

        // Assert
        _mockGameUI.Verify(ui => ui.ShowError("No active game to save!"), Times.Once);
        _mockSaveGameService.Verify(s => s.SaveGame(It.IsAny<Character>(), It.IsAny<List<Item>>(), It.IsAny<string>()),
            Times.Never,
            "should not attempt save when player is null");
    }
}
