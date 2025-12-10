using FluentAssertions;
using Game.Features.Inventory.Queries;
using Game.Models;
using Xunit;

namespace Game.Tests.Features.Inventory.Queries;

/// <summary>
/// Tests for GetEquippedItemsHandler.
/// </summary>
public class GetEquippedItemsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_All_Equipment_Slots()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var player = new Character { Name = "Test" };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EquippedItems.Should().NotBeNull();
        result.EquippedItems.Should().HaveCount(13); // 13 equipment slots
        result.EquippedItems.Keys.Should().Contain("MainHand");
        result.EquippedItems.Keys.Should().Contain("OffHand");
        result.EquippedItems.Keys.Should().Contain("Helmet");
        result.EquippedItems.Keys.Should().Contain("Ring1");
        result.EquippedItems.Keys.Should().Contain("Ring2");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_For_Empty_Slots()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var player = new Character { Name = "Test" };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EquippedItems.Values.Should().AllBeEquivalentTo((Item?)null);
    }

    [Fact]
    public async Task Handle_Should_Return_Equipped_MainHand()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var sword = new Item { Name = "Iron Sword", Type = ItemType.Weapon };
        var player = new Character 
        { 
            Name = "Test",
            EquippedMainHand = sword
        };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EquippedItems["MainHand"].Should().Be(sword);
        result.EquippedItems["MainHand"]!.Name.Should().Be("Iron Sword");
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Equipped_Items()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var sword = new Item { Name = "Iron Sword", Type = ItemType.Weapon };
        var helmet = new Item { Name = "Iron Helmet", Type = ItemType.Helmet };
        var ring = new Item { Name = "Gold Ring", Type = ItemType.Ring };
        
        var player = new Character 
        { 
            Name = "Test",
            EquippedMainHand = sword,
            EquippedHelmet = helmet,
            EquippedRing1 = ring
        };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EquippedItems["MainHand"].Should().Be(sword);
        result.EquippedItems["Helmet"].Should().Be(helmet);
        result.EquippedItems["Ring1"].Should().Be(ring);
        result.EquippedItems["Ring2"].Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Include_All_Armor_Slots()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var player = new Character { Name = "Test" };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EquippedItems.Keys.Should().Contain(new[] 
        {
            "Helmet", "Shoulders", "Chest", "Bracers", "Gloves", "Belt", "Legs", "Boots"
        });
    }

    [Fact]
    public async Task Handle_Should_Include_Accessory_Slots()
    {
        // Arrange
        var handler = new GetEquippedItemsHandler();
        var player = new Character { Name = "Test" };
        var query = new GetEquippedItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.EquippedItems.Keys.Should().Contain(new[] { "Necklace", "Ring1", "Ring2" });
    }
}
