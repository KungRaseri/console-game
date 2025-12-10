using FluentAssertions;
using Game.Models;

namespace Game.Tests.Models;

public class SaveGameTests
{
    [Fact]
    public void SaveGame_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var saveGame = new SaveGame();

        // Assert
        saveGame.Id.Should().NotBeNullOrEmpty();
        saveGame.PlayerName.Should().Be(string.Empty);
        saveGame.SaveDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        saveGame.Character.Should().NotBeNull();
        saveGame.Character.Inventory.Should().NotBeNull().And.BeEmpty(); // Inventory is in Character now
        saveGame.PlayTimeMinutes.Should().Be(0);
    }

    [Fact]
    public void SaveGame_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var save1 = new SaveGame();
        var save2 = new SaveGame();

        // Assert
        save1.Id.Should().NotBe(save2.Id);
    }

    [Fact]
    public void SaveGame_Properties_Should_Be_Settable()
    {
        // Arrange
        var saveGame = new SaveGame();
        var character = new Character { Name = "Hero", Level = 5 };
        var inventory = new List<Item> 
        { 
            new Item { Name = "Sword" },
            new Item { Name = "Potion" }
        };

        // Act
        saveGame.PlayerName = "TestPlayer";
        saveGame.Character = character;
        saveGame.Character.Inventory = inventory; // Inventory is in Character now
        saveGame.PlayTimeMinutes = 120;

        // Assert
        saveGame.PlayerName.Should().Be("TestPlayer");
        saveGame.Character.Name.Should().Be("Hero");
        saveGame.Character.Level.Should().Be(5);
        saveGame.Character.Inventory.Should().HaveCount(2); // Inventory is in Character now
        saveGame.PlayTimeMinutes.Should().Be(120);
    }

    [Fact]
    public void SaveGame_SaveDate_Can_Be_Set()
    {
        // Arrange
        var saveGame = new SaveGame();
        var customDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        saveGame.SaveDate = customDate;

        // Assert
        saveGame.SaveDate.Should().Be(customDate);
    }

    [Fact]
    public void SaveGame_Inventory_Can_Be_Modified()
    {
        // Arrange
        var saveGame = new SaveGame();

        // Act
        saveGame.Character.Inventory.Add(new Item { Name = "Sword" });
        saveGame.Character.Inventory.Add(new Item { Name = "Shield" });
        saveGame.Character.Inventory.Add(new Item { Name = "Potion" });

        // Assert
        saveGame.Character.Inventory.Should().HaveCount(3);
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Sword");
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Shield");
        saveGame.Character.Inventory.Should().Contain(i => i.Name == "Potion");
    }
}
