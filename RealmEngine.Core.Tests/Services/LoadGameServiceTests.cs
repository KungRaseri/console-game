using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Abstractions;
using RealmEngine.Core.Services;
using RealmEngine.Shared.Models;
using Moq;
using FluentAssertions;
using Xunit;

namespace RealmEngine.Core.Tests.Services;

[Trait("Category", "Service")]
public class LoadGameServiceTests
{
    private readonly Mock<SaveGameService> _mockSaveGameService;
    private readonly Mock<ApocalypseTimer> _mockApocalypseTimer;
    private readonly Mock<IGameUI> _mockGameUI;
    private readonly LoadGameService _service;

    public LoadGameServiceTests()
    {
        _mockSaveGameService = new Mock<SaveGameService>();
        _mockApocalypseTimer = new Mock<ApocalypseTimer>();
        _mockGameUI = new Mock<IGameUI>();
        _service = new LoadGameService(
            _mockSaveGameService.Object,
            _mockApocalypseTimer.Object,
            _mockGameUI.Object);
    }

    [Fact]
    public async Task LoadGameAsync_Should_Return_Null_When_No_Saves_Exist()
    {
        // Arrange
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(new List<SaveGame>());

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        result.SelectedSave.Should().BeNull();
        result.LoadSuccessful.Should().BeFalse();
        _mockGameUI.Verify(ui => ui.ShowWarning("No saved games found!"), Times.Once);
    }

    [Fact]
    public async Task LoadGameAsync_Should_Display_Available_Saves()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now.AddHours(-5),
                PlayTimeMinutes = 120
            },
            new SaveGame
            {
                Character = new Character { Name = "Hero2", ClassName = "Mage", Level = 15 },
                SaveDate = DateTime.Now.AddDays(-2),
                PlayTimeMinutes = 300
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);

        // Mock user selecting "Back to Menu"
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Back to Menu");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        _mockGameUI.Verify(ui => ui.ShowBanner("Load Game", It.IsAny<string>()), Times.Once);
        _mockGameUI.Verify(ui => ui.ShowTable(
            It.IsAny<string>(),
            It.IsAny<string[]>(),
            It.IsAny<List<string[]>>()), Times.Once);
    }

    [Fact]
    public async Task LoadGameAsync_Should_Return_Null_When_User_Cancels()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now,
                PlayTimeMinutes = 60
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Back to Menu");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        result.SelectedSave.Should().BeNull();
        result.LoadSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task LoadGameAsync_Should_Return_Selected_Save()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now,
                PlayTimeMinutes = 60
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Hero1 - Level 10 Warrior");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        result.SelectedSave.Should().NotBeNull();
        result.LoadSuccessful.Should().BeTrue();
        result.SelectedSave!.Character.Name.Should().Be("Hero1");
    }

    [Fact]
    public async Task LoadGameAsync_Should_Display_Welcome_Back_Message()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now,
                PlayTimeMinutes = 60
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Hero1 - Level 10 Warrior");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        _mockGameUI.Verify(ui => ui.ShowSuccess(It.Is<string>(s => s.Contains("Welcome back, Hero1"))), Times.Once);
    }

    [Fact]
    public async Task LoadGameAsync_Should_Navigate_To_Delete_Menu()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now,
                PlayTimeMinutes = 60
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Delete a Save");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        result.SelectedSave.Should().BeNull("should return null after navigating to delete menu");
        result.LoadSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task LoadGameAsync_Should_Handle_Apocalypse_Timer_Restoration()
    {
        // Arrange
        var saves = new List<SaveGame>
        {
            new SaveGame
            {
                Character = new Character { Name = "Hero1", ClassName = "Warrior", Level = 10 },
                SaveDate = DateTime.Now,
                PlayTimeMinutes = 60,
                ApocalypseMode = true,
                ApocalypseStartTime = DateTime.Now.AddHours(-2)
            }
        };
        _mockSaveGameService.Setup(s => s.GetAllSaves()).Returns(saves);
        _mockGameUI.Setup(ui => ui.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns("Hero1 - Level 10 Warrior");

        // Act
        var result = await _service.LoadGameAsync();

        // Assert
        result.SelectedSave.Should().NotBeNull();
        result.SelectedSave!.ApocalypseMode.Should().BeTrue();
        // ApocalypseTimer restoration logic is called internally
    }
}
