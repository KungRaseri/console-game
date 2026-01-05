using FluentAssertions;
using MediatR;
using Moq;
using RealmEngine.Core.Features.Death.Queries;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Death.Queries;

public class GetDroppedItemsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Dropped_Items_At_Location()
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        var droppedItems = new List<Item>
        {
            new() { Name = "Health Potion", Type = ItemType.Consumable },
            new() { Name = "Iron Sword", Type = ItemType.Weapon }
        };

        var saveGame = new SaveGame
        {
            CharacterName = "Hero",
            DroppedItemsAtLocations = new Dictionary<string, List<Item>>
            {
                ["Dark Cave"] = droppedItems
            }
        };

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns(saveGame);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = "Dark Cave" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(i => i.Name == "Health Potion");
        result.Items.Should().Contain(i => i.Name == "Iron Sword");
        result.HasItems.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Result_When_No_Items_At_Location()
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        var saveGame = new SaveGame
        {
            CharacterName = "Hero",
            DroppedItemsAtLocations = new Dictionary<string, List<Item>>
            {
                ["Forest"] = new List<Item>
                {
                    new() { Name = "Mana Potion", Type = ItemType.Consumable }
                }
            }
        };

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns(saveGame);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = "Dark Cave" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.HasItems.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_Result_When_No_SaveGame()
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns((SaveGame?)null);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = "Anywhere" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.HasItems.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Call_SaveGameService_GetCurrentSave()
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        var saveGame = new SaveGame
        {
            CharacterName = "Hero",
            DroppedItemsAtLocations = new Dictionary<string, List<Item>>()
        };

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns(saveGame);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = "Town" };

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        mockSaveGameService.Verify(x => x.GetCurrentSave(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Multiple_Items_Of_Different_Types()
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        var droppedItems = new List<Item>
        {
            new() { Name = "Health Potion", Type = ItemType.Consumable, Value = 10 },
            new() { Name = "Iron Sword", Type = ItemType.Weapon, Value = 50 },
            new() { Name = "Leather Armor", Type = ItemType.Armor, Value = 30 },
            new() { Name = "Iron Ore", Type = ItemType.Material, Value = 5 }
        };

        var saveGame = new SaveGame
        {
            CharacterName = "Hero",
            DroppedItemsAtLocations = new Dictionary<string, List<Item>>
            {
                ["Battlefield"] = droppedItems
            }
        };

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns(saveGame);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = "Battlefield" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(4);
        result.Items.Should().Contain(i => i.Type == ItemType.Consumable);
        result.Items.Should().Contain(i => i.Type == ItemType.Weapon);
        result.Items.Should().Contain(i => i.Type == ItemType.Armor);
        result.Items.Should().Contain(i => i.Type == ItemType.Material);
        result.HasItems.Should().BeTrue();
    }

    [Theory]
    [InlineData("Dark Cave")]
    [InlineData("Forest")]
    [InlineData("Mountain Pass")]
    [InlineData("Dungeon Level 3")]
    public async Task Handle_Should_Work_With_Different_Locations(string location)
    {
        // Arrange
        var mockSaveGameService = new Mock<SaveGameService>();

        var droppedItems = new List<Item>
        {
            new() { Name = "Test Item", Type = ItemType.Consumable }
        };

        var saveGame = new SaveGame
        {
            CharacterName = "Hero",
            DroppedItemsAtLocations = new Dictionary<string, List<Item>>
            {
                [location] = droppedItems
            }
        };

        mockSaveGameService.Setup(x => x.GetCurrentSave()).Returns(saveGame);

        var handler = new GetDroppedItemsHandler(mockSaveGameService.Object);

        var query = new GetDroppedItemsQuery { Location = location };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.HasItems.Should().BeTrue();
    }
}
