using FluentAssertions;
using Game.Core.Features.CharacterCreation.Commands;
using Game.Core.Features.SaveLoad;
using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Abstractions;
using Game.Data.Repositories;
using MediatR;
using Moq;

namespace Game.Tests.Features.CharacterCreation.Commands;

/// <summary>
/// Tests for CreateCharacterHandler.
/// </summary>
public class CreateCharacterHandlerTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IGameUI> _mockConsoleUI;
    private readonly SaveGameService _saveGameService;
    private readonly ApocalypseTimer _apocalypseTimer;

    public CreateCharacterHandlerTests()
    {
        _testDbPath = $"test-createchar-{Guid.NewGuid()}.db";
        _mockMediator = new Mock<IMediator>();
        _mockConsoleUI = new Mock<IGameUI>();
        _apocalypseTimer = new ApocalypseTimer(_mockConsoleUI.Object);
        var repository = new SaveGameRepository(_testDbPath);
        _saveGameService = new SaveGameService(repository, _apocalypseTimer);
    }

    /// <summary>
    /// Create a valid attribute allocation that satisfies point-buy rules.
    /// Total 27 points: 15(9pts) + 14(7pts) + 13(5pts) + 12(4pts) + 10(2pts) + 8(0pts) = 27 points
    /// </summary>
    private static AttributeAllocation CreateAllocation(
        int strength = 15,    // Costs 9 points (8→15)
        int constitution = 14, // Costs 7 points (8→14)
        int dexterity = 13,    // Costs 5 points (8→13)
        int intelligence = 12, // Costs 4 points (8→12)
        int wisdom = 10,       // Costs 2 points (8→10)
        int charisma = 8)      // Costs 0 points (default)
    {
        // Note: This totals exactly 27 points and is valid
        return new AttributeAllocation
        {
            Strength = strength,
            Constitution = constitution,
            Dexterity = dexterity,
            Intelligence = intelligence,
            Wisdom = wisdom,
            Charisma = charisma
        };
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
        }
    }

    [Fact]
    public async Task Handle_Should_Create_Character_With_Valid_Attributes()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var allocation = new AttributeAllocation
        {
            Strength = 15,
            Constitution = 14,
            Dexterity = 13,
            Intelligence = 12,
            Wisdom = 10,
            Charisma = 8
        };
        var command = new CreateCharacterCommand
        {
            PlayerName = "TestHero",
            ClassName = "Warrior",
            AttributeAllocation = allocation
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Character.Should().NotBeNull();
        result.Character!.Name.Should().Be("TestHero");
        result.Character.ClassName.Should().Be("Warrior");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_Message()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "Hero",
            ClassName = "Mage",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Message.Should().Be("Welcome, Hero the Mage!");
    }

    [Fact]
    public async Task Handle_Should_Publish_CharacterCreated_Event()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "TestHero",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Publish(
            It.Is<CharacterCreated>(e => e.PlayerName == "TestHero"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_SaveGame()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "TestHero",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SaveGameId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Set_Character_Attributes_Correctly()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        // Valid allocation: 15(9pts) + 14(7pts) + 13(5pts) + 11(3pts) + 10(2pts) + 9(1pt) = 27pts
        var command = new CreateCharacterCommand
        {
            PlayerName = "TestHero",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation(strength: 15, constitution: 14, dexterity: 13, intelligence: 11, wisdom: 10, charisma: 9)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.Strength.Should().BeGreaterThanOrEqualTo(15); // May be higher due to class bonus
        result.Character.Constitution.Should().BeGreaterThanOrEqualTo(14);
        result.Character.Dexterity.Should().BeGreaterThanOrEqualTo(13);
        result.Character.Intelligence.Should().BeGreaterThanOrEqualTo(11);
        result.Character.Wisdom.Should().BeGreaterThanOrEqualTo(10);
        result.Character.Charisma.Should().BeGreaterThanOrEqualTo(9);
    }

    [Fact]
    public async Task Handle_Should_Create_Warrior_Character()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "WarriorTest",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.ClassName.Should().Be("Warrior");
        result.Character.Level.Should().Be(1);
        result.Character.Health.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_Should_Create_Mage_Character()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        // Mage allocation: Focus on Intelligence
        // Valid: 8(0) + 10(2) + 12(4) + 15(9) + 14(7) + 13(5) = 27pts
        var command = new CreateCharacterCommand
        {
            PlayerName = "MageTest",
            ClassName = "Mage",
            AttributeAllocation = CreateAllocation(strength: 8, constitution: 10, dexterity: 12, intelligence: 15, wisdom: 14, charisma: 13)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.ClassName.Should().Be("Mage");
        result.Character.Mana.Should().BeGreaterThan(0);
        result.Character.Intelligence.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task Handle_Should_Create_Rogue_Character()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        // Rogue allocation: Focus on Dexterity
        // Valid: 12(4) + 10(2) + 15(9) + 13(5) + 8(0) + 14(7) = 27pts
        var command = new CreateCharacterCommand
        {
            PlayerName = "RogueTest",
            ClassName = "Rogue",
            AttributeAllocation = CreateAllocation(strength: 12, constitution: 10, dexterity: 15, intelligence: 13, wisdom: 8, charisma: 14)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.ClassName.Should().Be("Rogue");
        result.Character.Dexterity.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task Handle_Should_Initialize_Character_At_Level_1()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "NewHero",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.Level.Should().Be(1);
        result.Character.Experience.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Initialize_Character_With_Full_Health()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "HealthTest",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.Health.Should().Be(result.Character.MaxHealth);
        result.Character.Health.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_Should_Create_SaveGame_With_Correct_Player_Name()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "TestPlayerName",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.SaveGameId.Should().NotBeNullOrEmpty();
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame.Should().NotBeNull();
        saveGame!.PlayerName.Should().Be("TestPlayerName");
        saveGame.Character.Name.Should().Be("TestPlayerName");
    }

    [Fact]
    public async Task Handle_Should_Return_Character_With_Starting_Equipment()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);
        var command = new CreateCharacterCommand
        {
            PlayerName = "InventoryTest",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Character.Should().NotBeNull();
        result.Character!.Inventory.Should().NotBeNull();
        result.Character.Inventory.Should().NotBeEmpty(); // Characters start with equipment
    }

    [Fact]
    public async Task Handle_Should_Create_Character_With_Different_Names()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);

        // Act & Assert - First character
        var command1 = new CreateCharacterCommand
        {
            PlayerName = "Hero1",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };
        var result1 = await handler.Handle(command1, CancellationToken.None);
        result1.Character.Should().NotBeNull();
        result1.Character!.Name.Should().Be("Hero1");

        // Act & Assert - Second character
        var command2 = new CreateCharacterCommand
        {
            PlayerName = "Hero2",
            ClassName = "Mage",
            AttributeAllocation = CreateAllocation()
        };
        var result2 = await handler.Handle(command2, CancellationToken.None);
        result2.Character.Should().NotBeNull();
        result2.Character!.Name.Should().Be("Hero2");
    }

    [Fact]
    public async Task Handle_Should_Generate_Unique_SaveGame_Ids()
    {
        // Arrange
        var handler = new CreateCharacterHandler(_mockMediator.Object, _saveGameService);

        // Act
        var command1 = new CreateCharacterCommand
        {
            PlayerName = "Hero1",
            ClassName = "Warrior",
            AttributeAllocation = CreateAllocation()
        };
        var result1 = await handler.Handle(command1, CancellationToken.None);

        var command2 = new CreateCharacterCommand
        {
            PlayerName = "Hero2",
            ClassName = "Mage",
            AttributeAllocation = CreateAllocation()
        };
        var result2 = await handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.SaveGameId.Should().NotBe(result2.SaveGameId);
    }
}
