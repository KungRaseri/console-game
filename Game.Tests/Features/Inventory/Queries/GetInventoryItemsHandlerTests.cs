using FluentAssertions;
using Game.Core.Features.Inventory.Queries;
using Game.Core.Models;
using Xunit;

namespace Game.Tests.Features.Inventory.Queries;

/// <summary>
/// Tests for GetInventoryItemsHandler.
/// </summary>
public class GetInventoryItemsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Empty_List_For_Empty_Inventory()
    {
        // Arrange
        var handler = new GetInventoryItemsHandler();
        var player = new Character { Name = "Test" };
        var query = new GetInventoryItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Inventory_Items()
    {
        // Arrange
        var handler = new GetInventoryItemsHandler();
        var player = new Character { Name = "Test" };
        player.Inventory.Add(new Item { Name = "Potion" });
        player.Inventory.Add(new Item { Name = "Sword" });
        player.Inventory.Add(new Item { Name = "Shield" });
        var query = new GetInventoryItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalItems.Should().Be(3);
        result.Items.Select(i => i.Name).Should().Contain(new[] { "Potion", "Sword", "Shield" });
    }

    [Fact]
    public async Task Handle_Should_Return_Correct_TotalItems_Count()
    {
        // Arrange
        var handler = new GetInventoryItemsHandler();
        var player = new Character { Name = "Test" };
        for (int i = 0; i < 10; i++)
        {
            player.Inventory.Add(new Item { Name = $"Item{i}" });
        }
        var query = new GetInventoryItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalItems.Should().Be(10);
    }

    [Fact]
    public async Task Handle_Should_Not_Modify_Original_Inventory()
    {
        // Arrange
        var handler = new GetInventoryItemsHandler();
        var player = new Character { Name = "Test" };
        player.Inventory.Add(new Item { Name = "Potion" });
        var query = new GetInventoryItemsQuery { Player = player };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.Items.Clear(); // Modify returned list

        // Assert
        player.Inventory.Should().HaveCount(1); // Original should be unchanged
    }
}
