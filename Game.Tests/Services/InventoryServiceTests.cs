using Game.Models;
using Game.Services;
using Game.Features.Inventory;
using FluentAssertions;
using MediatR;

namespace Game.Tests.Services;

/// <summary>
/// Simple test mediator that doesn't publish events
/// </summary>
class TestMediator : IMediator
{
    public List<INotification> PublishedNotifications { get; } = new();

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification is INotification n)
        {
            PublishedNotifications.Add(n);
        }
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        PublishedNotifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        throw new NotImplementedException();
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class InventoryServiceTests
{
    private readonly TestMediator _mediator;

    public InventoryServiceTests()
    {
        _mediator = new TestMediator();
    }

    #region Constructor Tests

    [Fact]
    public void InventoryService_Should_Initialize_Empty()
    {
        // Arrange & Act
        var service = new InventoryService(_mediator);

        // Assert
        service.Count.Should().Be(0);
        service.GetAllItems().Should().BeEmpty();
    }

    [Fact]
    public void InventoryService_Should_Initialize_With_Existing_Items()
    {
        // Arrange
        var existingItems = new List<Item>
        {
            new Item { Name = "Sword" },
            new Item { Name = "Shield" }
        };

        // Act
        var service = new InventoryService(_mediator, existingItems);

        // Assert
        service.Count.Should().Be(2);
        service.GetAllItems().Should().HaveCount(2);
    }

    #endregion

    #region AddItem Tests

    [Fact]
    public async Task AddItemAsync_Should_Add_Item_To_Inventory()
    {
        // Arrange
        var service = new InventoryService(_mediator);
        var item = new Item { Name = "Health Potion" };

        // Act
        var result = await service.AddItemAsync(item, "TestPlayer");

        // Assert
        result.Should().BeTrue();
        service.Count.Should().Be(1);
        service.GetAllItems().Should().Contain(item);
    }

    [Fact]
    public async Task AddItemAsync_Should_Publish_ItemAcquired_Event()
    {
        // Arrange
        var service = new InventoryService(_mediator);
        var item = new Item { Name = "Magic Scroll" };

        // Act
        await service.AddItemAsync(item, "TestPlayer");

        // Assert
        _mediator.PublishedNotifications.Should().HaveCount(1);
        var notification = _mediator.PublishedNotifications[0] as ItemAcquired;
        notification.Should().NotBeNull();
        notification!.PlayerName.Should().Be("TestPlayer");
        notification.ItemName.Should().Be("Magic Scroll");
    }

    [Fact]
    public async Task AddItemAsync_Should_Return_False_For_Null_Item()
    {
        // Arrange
        var service = new InventoryService(_mediator);

        // Act
        var result = await service.AddItemAsync(null!, "TestPlayer");

        // Assert
        result.Should().BeFalse();
        service.Count.Should().Be(0);
    }

    #endregion

    #region RemoveItem Tests

    [Fact]
    public void RemoveItem_By_Id_Should_Remove_Item()
    {
        // Arrange
        var item = new Item { Id = "test-id", Name = "Sword" };
        var inventory = new List<Item> { item };
        var service = new InventoryService(_mediator, inventory);

        // Act
        var result = service.RemoveItem("test-id");

        // Assert
        result.Should().BeTrue();
        service.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_By_Id_Should_Return_False_For_NonExistent_Item()
    {
        // Arrange
        var service = new InventoryService(_mediator);

        // Act
        var result = service.RemoveItem("non-existent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveItem_By_Reference_Should_Remove_Item()
    {
        // Arrange
        var item = new Item { Name = "Helmet" };
        var inventory = new List<Item> { item };
        var service = new InventoryService(_mediator, inventory);

        // Act
        var result = service.RemoveItem(item);

        // Assert
        result.Should().BeTrue();
        service.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_By_Reference_Should_Return_False_For_Null()
    {
        // Arrange
        var service = new InventoryService(_mediator);

        // Act
        var result = service.RemoveItem((Item)null!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetItemsByType Tests

    [Fact]
    public void GetItemsByType_Should_Return_Items_Of_Specified_Type()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Sword", Type = ItemType.Weapon },
            new Item { Name = "Potion", Type = ItemType.Consumable },
            new Item { Name = "Axe", Type = ItemType.Weapon }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        var weapons = service.GetItemsByType(ItemType.Weapon);

        // Assert
        weapons.Should().HaveCount(2);
        weapons.Should().OnlyContain(i => i.Type == ItemType.Weapon);
    }

    [Fact]
    public void GetItemsByType_Should_Return_Empty_List_When_No_Matches()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Sword", Type = ItemType.Weapon }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        var armor = service.GetItemsByType(ItemType.Boots);

        // Assert
        armor.Should().BeEmpty();
    }

    #endregion

    #region GetItemsByRarity Tests

    [Fact]
    public void GetItemsByRarity_Should_Return_Items_Of_Specified_Rarity()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Common Sword", Rarity = ItemRarity.Common },
            new Item { Name = "Legendary Axe", Rarity = ItemRarity.Legendary },
            new Item { Name = "Common Shield", Rarity = ItemRarity.Common }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        var commonItems = service.GetItemsByRarity(ItemRarity.Common);

        // Assert
        commonItems.Should().HaveCount(2);
        commonItems.Should().OnlyContain(i => i.Rarity == ItemRarity.Common);
    }

    #endregion

    #region FindItemById Tests

    [Fact]
    public void FindItemById_Should_Return_Item_When_Found()
    {
        // Arrange
        var item = new Item { Id = "unique-id", Name = "Artifact" };
        var inventory = new List<Item> { item };
        var service = new InventoryService(_mediator, inventory);

        // Act
        var found = service.FindItemById("unique-id");

        // Assert
        found.Should().NotBeNull();
        found.Should().BeSameAs(item);
    }

    [Fact]
    public void FindItemById_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var service = new InventoryService(_mediator);

        // Act
        var found = service.FindItemById("non-existent");

        // Assert
        found.Should().BeNull();
    }

    #endregion

    #region UseItemAsync Tests

    [Fact]
    public async Task UseItemAsync_Should_Apply_Health_Potion_Effects()
    {
        // Arrange
        var healthPotion = new Item
        {
            Name = "Health Potion",
            Type = ItemType.Consumable,
            Rarity = ItemRarity.Common
        };
        var inventory = new List<Item> { healthPotion };
        var service = new InventoryService(_mediator, inventory);
        var character = new Character { Health = 50, MaxHealth = 100 };

        // Act
        var result = await service.UseItemAsync(healthPotion, character, "TestPlayer");

        // Assert
        result.Should().BeTrue();
        character.Health.Should().Be(80); // +30 for common health potion
        service.Count.Should().Be(0); // Consumable removed
    }

    [Fact]
    public async Task UseItemAsync_Should_Apply_Mana_Potion_Effects()
    {
        // Arrange
        var manaPotion = new Item
        {
            Name = "Mana Potion",
            Type = ItemType.Consumable,
            Rarity = ItemRarity.Uncommon
        };
        var inventory = new List<Item> { manaPotion };
        var service = new InventoryService(_mediator, inventory);
        var character = new Character { Mana = 20, MaxMana = 100 };

        // Act
        var result = await service.UseItemAsync(manaPotion, character, "TestPlayer");

        // Assert
        result.Should().BeTrue();
        character.Mana.Should().Be(55); // +35 for uncommon mana potion
        service.Count.Should().Be(0);
    }

    [Fact]
    public async Task UseItemAsync_Should_Not_Exceed_Max_Health()
    {
        // Arrange
        var healthPotion = new Item
        {
            Name = "Healing Potion",
            Type = ItemType.Consumable,
            Rarity = ItemRarity.Legendary
        };
        var inventory = new List<Item> { healthPotion };
        var service = new InventoryService(_mediator, inventory);
        var character = new Character { Health = 90, MaxHealth = 100 };

        // Act
        await service.UseItemAsync(healthPotion, character, "TestPlayer");

        // Assert
        character.Health.Should().Be(100); // Capped at MaxHealth
    }

    [Fact]
    public async Task UseItemAsync_Should_Return_False_For_Non_Consumable()
    {
        // Arrange
        var weapon = new Item { Name = "Sword", Type = ItemType.Weapon };
        var inventory = new List<Item> { weapon };
        var service = new InventoryService(_mediator, inventory);
        var character = new Character();

        // Act
        var result = await service.UseItemAsync(weapon, character, "TestPlayer");

        // Assert
        result.Should().BeFalse();
        service.Count.Should().Be(1); // Item not removed
    }

    [Fact]
    public async Task UseItemAsync_Should_Return_False_For_Item_Not_In_Inventory()
    {
        // Arrange
        var potion = new Item { Name = "Potion", Type = ItemType.Consumable };
        var service = new InventoryService(_mediator); // Empty inventory
        var character = new Character();

        // Act
        var result = await service.UseItemAsync(potion, character, "TestPlayer");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Utility Method Tests

    [Fact]
    public void HasItemOfType_Should_Return_True_When_Type_Exists()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Type = ItemType.Weapon },
            new Item { Type = ItemType.Consumable }
        };
        var service = new InventoryService(_mediator, items);

        // Act & Assert
        service.HasItemOfType(ItemType.Weapon).Should().BeTrue();
        service.HasItemOfType(ItemType.Consumable).Should().BeTrue();
    }

    [Fact]
    public void HasItemOfType_Should_Return_False_When_Type_Does_Not_Exist()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Type = ItemType.Weapon }
        };
        var service = new InventoryService(_mediator, items);

        // Act & Assert
        service.HasItemOfType(ItemType.Boots).Should().BeFalse();
    }

    [Fact]
    public void GetTotalValue_Should_Sum_All_Item_Prices()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Price = 100 },
            new Item { Price = 250 },
            new Item { Price = 50 }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        var totalValue = service.GetTotalValue();

        // Assert
        totalValue.Should().Be(400);
    }

    [Fact]
    public void GetTotalValue_Should_Return_Zero_For_Empty_Inventory()
    {
        // Arrange
        var service = new InventoryService(_mediator);

        // Act
        var totalValue = service.GetTotalValue();

        // Assert
        totalValue.Should().Be(0);
    }

    [Fact]
    public void Clear_Should_Remove_All_Items()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Item1" },
            new Item { Name = "Item2" },
            new Item { Name = "Item3" }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        service.Clear();

        // Assert
        service.Count.Should().Be(0);
        service.GetAllItems().Should().BeEmpty();
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public void SortByName_Should_Sort_Items_Alphabetically()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Zebra Sword" },
            new Item { Name = "Alpha Shield" },
            new Item { Name = "Beta Potion" }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        service.SortByName();

        // Assert
        var allItems = service.GetAllItems();
        allItems[0].Name.Should().Be("Alpha Shield");
        allItems[1].Name.Should().Be("Beta Potion");
        allItems[2].Name.Should().Be("Zebra Sword");
    }

    [Fact]
    public void SortByType_Should_Sort_Items_By_Type()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Item1", Type = ItemType.Weapon },
            new Item { Name = "Item2", Type = ItemType.Ring },
            new Item { Name = "Item3", Type = ItemType.Consumable }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        service.SortByType();

        // Assert
        var allItems = service.GetAllItems();
        allItems[0].Type.Should().Be(ItemType.Consumable); // Enum order
        allItems[1].Type.Should().Be(ItemType.Weapon);
        allItems[2].Type.Should().Be(ItemType.Ring);
    }

    [Fact]
    public void SortByRarity_Should_Sort_Items_By_Rarity_Descending()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Item1", Rarity = ItemRarity.Common },
            new Item { Name = "Item2", Rarity = ItemRarity.Legendary },
            new Item { Name = "Item3", Rarity = ItemRarity.Rare }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        service.SortByRarity();

        // Assert
        var allItems = service.GetAllItems();
        allItems[0].Rarity.Should().Be(ItemRarity.Legendary);
        allItems[1].Rarity.Should().Be(ItemRarity.Rare);
        allItems[2].Rarity.Should().Be(ItemRarity.Common);
    }

    [Fact]
    public void SortByValue_Should_Sort_Items_By_Price_Descending()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Name = "Item1", Price = 50 },
            new Item { Name = "Item2", Price = 500 },
            new Item { Name = "Item3", Price = 150 }
        };
        var service = new InventoryService(_mediator, items);

        // Act
        service.SortByValue();

        // Assert
        var allItems = service.GetAllItems();
        allItems[0].Price.Should().Be(500);
        allItems[1].Price.Should().Be(150);
        allItems[2].Price.Should().Be(50);
    }

    #endregion
}

