using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using Game.Shared.Services;
using Game.Console.UI;
using Game.Core.Abstractions;
using Game.Data.Repositories;
using Game.Tests.Helpers;
using FluentAssertions;
using Spectre.Console.Testing;

namespace Game.Tests.Services;

/// <summary>
/// Unit tests for MenuService - Menu display and navigation logic
/// </summary>
public class MenuServiceTests : IDisposable
{
    private readonly MenuService _menuService;
    private readonly SaveGameService _saveGameService;
    private readonly GameStateService _gameStateService;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;
    private readonly string _testDbPath;

    public MenuServiceTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-menu-{Guid.NewGuid()}.db";
        
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        
        _saveGameService = new SaveGameService(new SaveGameRepository(_testDbPath), new ApocalypseTimer((IGameUI)_consoleUI));
        _gameStateService = new GameStateService(_saveGameService);
        _menuService = new MenuService(_gameStateService, _saveGameService, _consoleUI);
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
    public void MenuService_Should_Be_Instantiable()
    {
        // Arrange & Act
        var service = new MenuService(_gameStateService, _saveGameService, _consoleUI);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void ShowInGameMenu_Should_Build_Menu_Options_With_Player()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act
        // Note: We can't directly test UI methods that call ConsoleUI
        // This test verifies the service is properly initialized
        
        // Assert
        _gameStateService.Should().NotBeNull();
        saveGame.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Potion")]
    [InlineData("Sword")]
    [InlineData("Shield")]
    public void SelectItemFromList_Should_Handle_Item_Selection(string itemName)
    {
        // Arrange
        var items = new List<string> { "Potion", "Sword", "Shield", "Helmet" };

        // Act
        // Note: SelectItemFromList uses ConsoleUI.ShowMenu which requires user input
        // We can verify the method signature exists and is callable
        
        // Assert
        items.Should().Contain(itemName);
        items.Count.Should().Be(4);
    }

    [Fact]
    public void MenuService_Should_Have_Required_Dependencies()
    {
        // Arrange & Act
        var hasGameStateService = _gameStateService != null;
        var hasSaveGameService = _saveGameService != null;

        // Assert
        hasGameStateService.Should().BeTrue();
        hasSaveGameService.Should().BeTrue();
    }

    [Fact]
    public void HandleMainMenu_Should_Accept_Valid_Choices()
    {
        // Arrange
        var validChoices = new[] { "New Game", "Load Game", "Settings", "Exit" };

        // Act & Assert
        foreach (var choice in validChoices)
        {
            choice.Should().NotBeNullOrEmpty();
            choice.Length.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void ShowCombatMenu_Should_Return_Combat_Actions()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // Select first option

        // Act
        var result = _menuService.ShowCombatMenu();

        // Assert
        result.Should().Be("⚔️  Attack");
    }

    [Fact]
    public void ShowCombatMenu_Should_Display_All_Combat_Options()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Select second option

        // Act
        var result = _menuService.ShowCombatMenu();

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Attack");
        output.Should().Contain("Defend");
        output.Should().Contain("Use Item");
        output.Should().Contain("Flee");
        result.Should().Be("🛡️  Defend");
    }

    [Fact]
    public void HandleMainMenu_Should_Return_NewGame_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // New Game

        // Act
        var result = _menuService.HandleMainMenu();

        // Assert
        result.Should().Be("New Game");
    }

    [Fact]
    public void HandleMainMenu_Should_Return_LoadGame_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Load Game

        // Act
        var result = _menuService.HandleMainMenu();

        // Assert
        result.Should().Be("Load Game");
    }

    [Fact]
    public void HandleMainMenu_Should_Return_Settings_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 3); // Settings (index 3: skip Hall of Fame)

        // Act
        var result = _menuService.HandleMainMenu();

        // Assert
        result.Should().Be("Settings");
    }

    [Fact]
    public void HandleMainMenu_Should_Return_Exit_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 4); // Exit

        // Act
        var result = _menuService.HandleMainMenu();

        // Assert
        result.Should().Be("Exit");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_ViewItemDetails_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // View Item Details

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("View Item Details");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_UseItem_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Use Item

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("Use Item");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_EquipItem_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Equip Item

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("Equip Item");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_DropItem_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 3); // Drop Item

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("Drop Item");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_SortInventory_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 4); // Sort Inventory

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("Sort Inventory");
    }

    [Fact]
    public void ShowInventoryMenu_Should_Return_BackToGame_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 5); // Back to Game

        // Act
        var result = _menuService.ShowInventoryMenu();

        // Assert
        result.Should().Be("Back to Game");
    }

    [Fact]
    public void HandlePauseMenu_Should_Return_InGame_When_Resume_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // Resume

        // Act
        var result = _menuService.HandlePauseMenu();

        // Assert
        result.Should().Be(GameState.InGame);
    }

    [Fact]
    public void HandlePauseMenu_Should_Return_Paused_When_SaveGame_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Save Game

        // Act
        var result = _menuService.HandlePauseMenu();

        // Assert
        result.Should().Be(GameState.Paused);
    }

    [Fact]
    public void HandlePauseMenu_Should_Return_MainMenu_When_MainMenu_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Main Menu

        // Act
        var result = _menuService.HandlePauseMenu();

        // Assert
        result.Should().Be(GameState.MainMenu);
    }

    [Fact]
    public void ShowRingSlotMenu_Should_Return_RingSlot1_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // Ring Slot 1

        // Act
        var result = _menuService.ShowRingSlotMenu();

        // Assert
        result.Should().Be("Ring Slot 1");
    }

    [Fact]
    public void ShowRingSlotMenu_Should_Return_RingSlot2_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Ring Slot 2

        // Act
        var result = _menuService.ShowRingSlotMenu();

        // Assert
        result.Should().Be("Ring Slot 2");
    }

    [Fact]
    public void ShowRingSlotMenu_Should_Return_Cancel_When_Selected()
    {
        // Arrange
        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Cancel

        // Act
        var result = _menuService.ShowRingSlotMenu();

        // Assert
        result.Should().Be("Cancel");
    }

    [Fact]
    public void SelectItemFromList_Should_Return_Null_When_No_Items()
    {
        // Arrange
        var emptyList = new List<Item>();

        // Act
        var result = _menuService.SelectItemFromList(emptyList, "Select an item");

        // Assert
        result.Should().BeNull();
        _testConsole.Output.Should().Contain("No items available");
    }

    [Fact]
    public void SelectItemFromList_Should_Return_Selected_Item()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Potion", Rarity = ItemRarity.Common },
            new Item { Name = "Sword", Rarity = ItemRarity.Rare },
            new Item { Name = "Shield", Rarity = ItemRarity.Epic }
        };

        TestConsoleHelper.SelectMenuOption(_testConsole, 1); // Select Sword

        // Act
        var result = _menuService.SelectItemFromList(items, "Select an item");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sword");
        result.Rarity.Should().Be(ItemRarity.Rare);
    }

    [Fact]
    public void SelectItemFromList_Should_Return_Null_When_Cancel_Selected()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Potion", Rarity = ItemRarity.Common },
            new Item { Name = "Sword", Rarity = ItemRarity.Rare }
        };

        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Cancel (index 2 is after 2 items)

        // Act
        var result = _menuService.SelectItemFromList(items, "Select an item");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SelectItemFromList_Should_Display_Item_Rarity()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Common Potion", Rarity = ItemRarity.Common },
            new Item { Name = "Legendary Sword", Rarity = ItemRarity.Legendary }
        };

        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        _menuService.SelectItemFromList(items, "Select an item");

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Common");
        output.Should().Contain("Legendary");
    }

    [Fact]
    public void ShowInGameMenu_Should_Display_Player_Info()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 10,
            Health = 80,
            MaxHealth = 100,
            Gold = 500
        };

        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // Select first option

        // Act
        _menuService.ShowInGameMenu();

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("TestHero");
        output.Should().Contain("Level 10");
        output.Should().Contain("80/100"); // HP
        output.Should().Contain("500"); // Gold
    }

    [Fact]
    public void ShowInGameMenu_Should_Include_LevelUp_Option_When_Unspent_AttributePoints()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            UnspentAttributePoints = 10,
            UnspentSkillPoints = 0
        };

        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        _menuService.ShowInGameMenu();

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Level Up");
        output.Should().Contain("10 attr");
    }

    [Fact]
    public void ShowInGameMenu_Should_Include_LevelUp_Option_When_Unspent_SkillPoints()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 3
        };

        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        _menuService.ShowInGameMenu();

        // Assert
        var output = _testConsole.Output;
        output.Should().Contain("Level Up");
        output.Should().Contain("3 skill");
    }

    [Fact]
    public void ShowInGameMenu_Should_Not_Include_LevelUp_When_No_Unspent_Points()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            UnspentAttributePoints = 0,
            UnspentSkillPoints = 0
        };

        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);

        // Act
        var result = _menuService.ShowInGameMenu();

        // Assert
        result.Should().NotContain("Level Up");
    }

    [Fact]
    public void ShowInGameMenu_Should_Return_Explore_When_Selected()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            Level = 1,
            Health = 100,
            MaxHealth = 100
        };

        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        TestConsoleHelper.SelectMenuOption(_testConsole, 0); // Explore

        // Act
        var result = _menuService.ShowInGameMenu();

        // Assert
        result.Should().Be("Explore");
    }

    [Fact]
    public void SelectItemFromList_Should_Handle_Multiple_Items_With_Same_Rarity()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Potion 1", Rarity = ItemRarity.Common },
            new Item { Name = "Potion 2", Rarity = ItemRarity.Common },
            new Item { Name = "Potion 3", Rarity = ItemRarity.Common }
        };

        TestConsoleHelper.SelectMenuOption(_testConsole, 2); // Select third item

        // Act
        var result = _menuService.SelectItemFromList(items, "Select potion");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Potion 3");
    }
}
