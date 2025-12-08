using FluentAssertions;
using Game.Models;
using Xunit;

namespace Game.Tests.Models;

public class ItemTests
{
    [Fact]
    public void Item_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var item = new Item();

        // Assert
        item.Id.Should().NotBeNullOrEmpty();
        item.Name.Should().Be(string.Empty);
        item.Description.Should().Be(string.Empty);
        item.Price.Should().Be(0);
        item.Rarity.Should().Be(ItemRarity.Common);
        item.Type.Should().Be(ItemType.Consumable);
    }

    [Fact]
    public void Item_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var item1 = new Item();
        var item2 = new Item();

        // Assert
        item1.Id.Should().NotBe(item2.Id);
    }

    [Fact]
    public void Item_Properties_Should_Be_Settable()
    {
        // Arrange
        var item = new Item();

        // Act
        item.Name = "Health Potion";
        item.Description = "Restores 50 HP";
        item.Price = 25;
        item.Rarity = ItemRarity.Rare;
        item.Type = ItemType.Consumable;

        // Assert
        item.Name.Should().Be("Health Potion");
        item.Description.Should().Be("Restores 50 HP");
        item.Price.Should().Be(25);
        item.Rarity.Should().Be(ItemRarity.Rare);
        item.Type.Should().Be(ItemType.Consumable);
    }

    [Theory]
    [InlineData(ItemRarity.Common)]
    [InlineData(ItemRarity.Uncommon)]
    [InlineData(ItemRarity.Rare)]
    [InlineData(ItemRarity.Epic)]
    [InlineData(ItemRarity.Legendary)]
    public void Item_Should_Accept_All_Rarity_Levels(ItemRarity rarity)
    {
        // Arrange & Act
        var item = new Item { Rarity = rarity };

        // Assert
        item.Rarity.Should().Be(rarity);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Chest)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.QuestItem)]
    [InlineData(ItemType.Ring)]
    public void Item_Should_Accept_All_Item_Types(ItemType type)
    {
        // Arrange & Act
        var item = new Item { Type = type };

        // Assert
        item.Type.Should().Be(type);
    }
}

public class NPCTests
{
    [Fact]
    public void NPC_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var npc = new NPC();

        // Assert
        npc.Id.Should().NotBeNullOrEmpty();
        npc.Name.Should().Be(string.Empty);
        npc.Age.Should().Be(0);
        npc.Occupation.Should().Be(string.Empty);
        npc.Gold.Should().Be(0);
        npc.Dialogue.Should().Be(string.Empty);
        npc.IsFriendly.Should().BeTrue();
    }

    [Fact]
    public void NPC_Should_Generate_Unique_Ids()
    {
        // Arrange & Act
        var npc1 = new NPC();
        var npc2 = new NPC();

        // Assert
        npc1.Id.Should().NotBe(npc2.Id);
    }

    [Fact]
    public void NPC_Properties_Should_Be_Settable()
    {
        // Arrange
        var npc = new NPC();

        // Act
        npc.Name = "Marcus the Merchant";
        npc.Age = 45;
        npc.Occupation = "Trader";
        npc.Gold = 500;
        npc.Dialogue = "Welcome to my shop!";
        npc.IsFriendly = true;

        // Assert
        npc.Name.Should().Be("Marcus the Merchant");
        npc.Age.Should().Be(45);
        npc.Occupation.Should().Be("Trader");
        npc.Gold.Should().Be(500);
        npc.Dialogue.Should().Be("Welcome to my shop!");
        npc.IsFriendly.Should().BeTrue();
    }

    [Fact]
    public void NPC_Can_Be_Hostile()
    {
        // Arrange & Act
        var npc = new NPC { IsFriendly = false };

        // Assert
        npc.IsFriendly.Should().BeFalse();
    }
}

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

public class GameEventsTests
{
    [Fact]
    public void CharacterCreated_Should_Have_PlayerName()
    {
        // Arrange & Act
        var event1 = new CharacterCreated("Hero");

        // Assert
        event1.PlayerName.Should().Be("Hero");
    }

    [Fact]
    public void PlayerLeveledUp_Should_Have_PlayerName_And_NewLevel()
    {
        // Arrange & Act
        var event1 = new PlayerLeveledUp("Hero", 5);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.NewLevel.Should().Be(5);
    }

    [Fact]
    public void GoldGained_Should_Have_PlayerName_And_Amount()
    {
        // Arrange & Act
        var event1 = new GoldGained("Hero", 100);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.Amount.Should().Be(100);
    }

    [Fact]
    public void DamageTaken_Should_Have_PlayerName_And_Amount()
    {
        // Arrange & Act
        var event1 = new DamageTaken("Hero", 25);

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.Amount.Should().Be(25);
    }

    [Fact]
    public void ItemAcquired_Should_Have_PlayerName_And_ItemName()
    {
        // Arrange & Act
        var event1 = new ItemAcquired("Hero", "Sword of Destiny");

        // Assert
        event1.PlayerName.Should().Be("Hero");
        event1.ItemName.Should().Be("Sword of Destiny");
    }

    [Fact]
    public void GameEvents_Should_Be_Records()
    {
        // Arrange
        var event1 = new CharacterCreated("Hero");
        var event2 = new CharacterCreated("Hero");
        var event3 = new CharacterCreated("Villain");

        // Act & Assert - Records have value equality
        event1.Should().Be(event2);
        event1.Should().NotBe(event3);
    }
}
