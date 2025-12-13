using FluentAssertions;
using Game.Features.SaveLoad;
using Game.Features.SaveLoad.Queries;
using Game.Models;
using Game.Shared.Services;
using Game.Shared.UI;
using Moq;
using Xunit;

namespace Game.Tests.Features.SaveLoad.Queries;

/// <summary>
/// Tests for GetMostRecentSaveHandler.
/// </summary>
public class GetMostRecentSaveHandlerTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly Mock<IConsoleUI> _mockConsoleUI;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly SaveGameService _saveGameService;

    public GetMostRecentSaveHandlerTests()
    {
        _testDbPath = $"test-getrecent-{Guid.NewGuid()}.db";
        _mockConsoleUI = new Mock<IConsoleUI>();
        _apocalypseTimer = new ApocalypseTimer(_mockConsoleUI.Object);
        _saveGameService = new SaveGameService(_apocalypseTimer, _testDbPath);
    }

    [Fact]
    public async Task Handle_Should_Return_Most_Recent_Save()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        
        // Create first save
        var player1 = new Character { Name = "OldSave", Level = 1, Health = 100, MaxHealth = 100 };
        var save1 = _saveGameService.CreateNewGame(player1, DifficultySettings.Normal);
        _saveGameService.SaveGame(player1, new List<Item>(), save1.Id);

        // Create second save (more recent)
        var player2 = new Character { Name = "NewSave", Level = 5, Health = 150, MaxHealth = 150 };
        var save2 = _saveGameService.CreateNewGame(player2, DifficultySettings.Normal);
        _saveGameService.SaveGame(player2, new List<Item>(), save2.Id);

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.Character.Name.Should().Be("NewSave");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_No_Saves()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Only_Save_When_One_Exists()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        var player = new Character { Name = "OnlyOne", Level = 10, Health = 200, MaxHealth = 200 };
        var saveGame = _saveGameService.CreateNewGame(player, DifficultySettings.Normal);
        _saveGameService.SaveGame(player, new List<Item>(), saveGame.Id);

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.Character.Name.Should().Be("OnlyOne");
    }

    [Fact]
    public async Task Handle_Should_Return_Most_Recent_After_Update()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        
        // Create two saves
        var player1 = new Character { Name = "First", Level = 1, Health = 100, MaxHealth = 100 };
        var save1 = _saveGameService.CreateNewGame(player1, DifficultySettings.Normal);
        _saveGameService.SaveGame(player1, new List<Item>(), save1.Id);

        var player2 = new Character { Name = "Second", Level = 5, Health = 150, MaxHealth = 150 };
        var save2 = _saveGameService.CreateNewGame(player2, DifficultySettings.Normal);
        _saveGameService.SaveGame(player2, new List<Item>(), save2.Id);

        // Update first save (should become most recent)
        player1.Level = 20;
        _saveGameService.SaveGame(player1, new List<Item>(), save1.Id);

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.Character.Name.Should().Be("First");
        result.SaveGame.Character.Level.Should().Be(20);
    }

    [Fact]
    public async Task Handle_Should_Return_Save_With_Complete_Data()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        var player = new Character
        {
            Name = "CompletePlayer",
            ClassName = "Warrior",
            Level = 15,
            Health = 250,
            MaxHealth = 300,
            Mana = 50,
            MaxMana = 100,
            Gold = 1500,
            Experience = 7500
        };
        var items = new List<Item>
        {
            new Item { Name = "Sword", Type = ItemType.Weapon },
            new Item { Name = "Potion", Type = ItemType.Consumable }
        };
        var saveGame = _saveGameService.CreateNewGame(player, DifficultySettings.Hard);
        _saveGameService.SaveGame(player, items, saveGame.Id);

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.Character.Name.Should().Be("CompletePlayer");
        result.SaveGame.Character.Level.Should().Be(15);
        result.SaveGame.Character.Gold.Should().Be(1500);
        result.SaveGame.DifficultyLevel.Should().Be("Hard");
    }

    [Fact]
    public async Task Handle_Should_Return_Latest_Among_Many_Saves()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        
        // Create multiple saves with delays to ensure different timestamps
        for (int i = 1; i <= 5; i++)
        {
            var player = new Character { Name = $"Player{i}", Level = i, Health = 100, MaxHealth = 100 };
            var save = _saveGameService.CreateNewGame(player, DifficultySettings.Normal);
            _saveGameService.SaveGame(player, new List<Item>(), save.Id);
            await Task.Delay(10); // Ensure different timestamps
        }

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.Character.Name.Should().Be("Player5");
        result.SaveGame.Character.Level.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_Return_Save_With_Correct_Difficulty()
    {
        // Arrange
        var handler = new GetMostRecentSaveHandler(_saveGameService);
        var player = new Character { Name = "ExpertPlayer", Level = 1, Health = 50, MaxHealth = 50 };
        var saveGame = _saveGameService.CreateNewGame(player, DifficultySettings.Expert);
        _saveGameService.SaveGame(player, new List<Item>(), saveGame.Id);

        var query = new GetMostRecentSaveQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.SaveGame.Should().NotBeNull();
        result.SaveGame!.DifficultyLevel.Should().Be("Expert");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _saveGameService?.Dispose();
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
            
            var logDbPath = _testDbPath.Replace(".db", "-log.db");
            if (File.Exists(logDbPath))
            {
                File.Delete(logDbPath);
            }
        }
    }
}
