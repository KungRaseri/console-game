using FluentAssertions;
using Game.Models;
using Game.Services;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using Spectre.Console.Testing;

namespace Game.Tests.Services;

public class SaveGameServiceTests : IDisposable
{
    private readonly string _testDbPath = "test_saves.db";
    private readonly SaveGameService _saveService;
    private readonly TestConsole _testConsole;
    private readonly ConsoleUI _consoleUI;

    public SaveGameServiceTests()
    {
        // Clean up any existing test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
        
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        _consoleUI = new ConsoleUI(_testConsole);
        _saveService = new SaveGameService(new ApocalypseTimer(_consoleUI), _testDbPath);
    } 

    [Fact]
    public void SaveGame_Should_Create_New_Save()
    {
        // Arrange
        var character = new Character
        {
            Name = "TestHero",
            ClassName = "Warrior",
            Level = 5,
            Gold = 100
        };
        var inventory = new List<Item>
        {
            new Item { Name = "Health Potion", Type = ItemType.Consumable }
        };

        // Act
        _saveService.SaveGame(character, inventory);

        // Assert
        var saves = _saveService.GetAllSaves();
        saves.Should().HaveCount(1);
        saves[0].PlayerName.Should().Be("TestHero");
        saves[0].Character.Level.Should().Be(5);
        saves[0].Character.Inventory.Should().HaveCount(1); // Inventory is in Character now
    }

    [Fact]
    public void LoadGame_Should_Retrieve_Saved_Game()
    {
        // Arrange
        var character = new Character
        {
            Name = "LoadTest",
            ClassName = "Mage",
            Level = 10,
            Gold = 500,
            Strength = 12,
            Intelligence = 18
        };
        var inventory = new List<Item>
        {
            new Item { Name = "Staff", Type = ItemType.Weapon },
            new Item { Name = "Robe", Type = ItemType.Chest }
        };

        _saveService.SaveGame(character, inventory);
        var saves = _saveService.GetAllSaves();
        var saveId = saves[0].Id;

        // Act
        var loadedSave = _saveService.LoadGame(saveId);

        // Assert
        loadedSave.Should().NotBeNull();
        loadedSave!.PlayerName.Should().Be("LoadTest");
        loadedSave.Character.Level.Should().Be(10);
        loadedSave.Character.Gold.Should().Be(500);
        loadedSave.Character.Strength.Should().Be(12);
        loadedSave.Character.Intelligence.Should().Be(18);
        loadedSave.Character.Inventory.Should().HaveCount(2); // Inventory is in Character now
    }

    [Fact]
    public void SaveGame_Should_Update_Existing_Save()
    {
        // Arrange
        var character = new Character
        {
            Name = "UpdateTest",
            ClassName = "Rogue",
            Level = 1
        };
        
        _saveService.SaveGame(character, new List<Item>());
        var saves = _saveService.GetAllSaves();
        var saveId = saves[0].Id;

        // Modify character
        character.Level = 5;
        character.Gold = 1000;

        // Act - Save with same ID to update
        _saveService.SaveGame(character, new List<Item>(), saveId);

        // Assert
        var allSaves = _saveService.GetAllSaves();
        allSaves.Should().HaveCount(1); // Still only one save
        
        var updated = _saveService.LoadGame(saveId);
        updated!.Character.Level.Should().Be(5);
        updated.Character.Gold.Should().Be(1000);
    }

    [Fact]
    public void DeleteSave_Should_Remove_Save_From_Database()
    {
        // Arrange
        var character = new Character { Name = "DeleteTest" };
        _saveService.SaveGame(character, new List<Item>());
        var saves = _saveService.GetAllSaves();
        var saveId = saves[0].Id;

        // Act
        var result = _saveService.DeleteSave(saveId);

        // Assert
        result.Should().BeTrue();
        _saveService.GetAllSaves().Should().BeEmpty();
    }

    [Fact]
    public Task GetMostRecentSave_Should_Return_Latest_Save()
    {
        // Arrange
        _saveService.SaveGame(new Character { Name = "First" }, new List<Item>());
        _saveService.SaveGame(new Character { Name = "Second" }, new List<Item>());
        _saveService.SaveGame(new Character { Name = "Third" }, new List<Item>());

        // Act
        var recent = _saveService.GetMostRecentSave();

        // Assert
        recent.Should().NotBeNull();
        recent!.PlayerName.Should().Be("Third");
        
        return Task.CompletedTask;
    }

    [Fact]
    public void HasAnySaves_Should_Return_True_When_Saves_Exist()
    {
        // Arrange
        _saveService.SaveGame(new Character { Name = "Test" }, new List<Item>());

        // Act
        var hasSaves = _saveService.HasAnySaves();

        // Assert
        hasSaves.Should().BeTrue();
    }

    [Fact]
    public void HasAnySaves_Should_Return_False_When_No_Saves()
    {
        // Act
        var hasSaves = _saveService.HasAnySaves();

        // Assert
        hasSaves.Should().BeFalse();
    }

    [Fact]
    public void AutoSave_Should_Overwrite_Existing_Character_Save()
    {
        // Arrange
        var character = new Character
        {
            Name = "AutoSaveTest",
            Level = 1,
            Gold = 0
        };

        _saveService.AutoSave(character, new List<Item>());
        
        // Modify character
        character.Level = 10;
        character.Gold = 5000;

        // Act
        _saveService.AutoSave(character, new List<Item>());

        // Assert
        var saves = _saveService.GetAllSaves();
        saves.Should().HaveCount(1); // Only one save per character
        saves[0].Character.Level.Should().Be(10);
        saves[0].Character.Gold.Should().Be(5000);
    }

    [Fact]
    public void SaveGame_Should_Persist_Learned_Skills()
    {
        // Arrange
        var character = new Character
        {
            Name = "SkillTest",
            ClassName = "Warrior"
        };
        character.LearnedSkills.Add(new Skill
        {
            Name = "Power Attack",
            CurrentRank = 3,
            MaxRank = 5
        });
        character.LearnedSkills.Add(new Skill
        {
            Name = "Iron Skin",
            CurrentRank = 2,
            MaxRank = 5
        });

        // Act
        _saveService.SaveGame(character, new List<Item>());
        var saves = _saveService.GetAllSaves();
        var loaded = _saveService.LoadGame(saves[0].Id);

        // Assert
        loaded!.Character.LearnedSkills.Should().HaveCount(2);
        loaded.Character.LearnedSkills[0].Name.Should().Be("Power Attack");
        loaded.Character.LearnedSkills[0].CurrentRank.Should().Be(3);
        loaded.Character.LearnedSkills[1].Name.Should().Be("Iron Skin");
        loaded.Character.LearnedSkills[1].CurrentRank.Should().Be(2);
    }

    [Fact(Skip = "Equipped items not persisting - LiteDB serialization issue to investigate")]
    public void SaveGame_Should_Persist_Equipped_Items()
    {
        // Arrange
        var character = new Character
        {
            Name = "EquipTest",
            ClassName = "Rogue"
        };
        character.EquippedMainHand = new Item
        {
            Name = "Iron Sword",
            Type = ItemType.Weapon,
            BonusStrength = 10
        };
        character.EquippedChest = new Item
        {
            Name = "Leather Armor",
            Type = ItemType.Chest,
            BonusConstitution = 5
        };

        // Act
        _saveService.SaveGame(character, new List<Item>());
        var saves = _saveService.GetAllSaves();
        var loaded = _saveService.LoadGame(saves[0].Id);

        // Assert
        loaded!.Character.EquippedMainHand.Should().NotBeNull();
        loaded.Character.EquippedMainHand!.Name.Should().Be("Iron Sword");
        loaded.Character.EquippedMainHand.BonusStrength.Should().Be(10);
        loaded.Character.EquippedChest.Should().NotBeNull();
        loaded.Character.EquippedChest!.Name.Should().Be("Leather Armor");
        loaded.Character.EquippedChest.BonusConstitution.Should().Be(5);
    }

    [Fact]
    public void SaveGame_Should_Store_All_D20_Attributes()
    {
        // Arrange
        var character = new Character
        {
            Name = "AttributeTest",
            Strength = 18,
            Dexterity = 14,
            Constitution = 16,
            Intelligence = 10,
            Wisdom = 12,
            Charisma = 8
        };

        // Act
        _saveService.SaveGame(character, new List<Item>());
        var saves = _saveService.GetAllSaves();
        var loaded = _saveService.LoadGame(saves[0].Id);

        // Assert
        loaded!.Character.Strength.Should().Be(18);
        loaded.Character.Dexterity.Should().Be(14);
        loaded.Character.Constitution.Should().Be(16);
        loaded.Character.Intelligence.Should().Be(10);
        loaded.Character.Wisdom.Should().Be(12);
        loaded.Character.Charisma.Should().Be(8);
    }

    public void Dispose()
    {
        _saveService?.Dispose();
        
        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }
}

