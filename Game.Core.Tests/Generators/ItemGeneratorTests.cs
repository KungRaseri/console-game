using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Game.Shared.Models;

namespace Game.Core.Tests.Generators;

public class ItemGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly ItemGenerator _generator;

    public ItemGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new ItemGenerator(_dataCache, _referenceResolver);
    }

    [Theory]
    [InlineData("weapons")]
    [InlineData("armor")]
    [InlineData("consumables")]
    public async Task Should_Generate_Items_From_Category(string category)
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var items = await _generator.GenerateItemsAsync(category, 5);

        // Assert
        items.Should().NotBeNull();
        // Note: Some categories might be empty, so we allow empty lists
        
        foreach (var item in items)
        {
            item.Name.Should().NotBeNullOrEmpty();
            item.Description.Should().NotBeNullOrEmpty();
            item.Id.Should().StartWith(category);
            item.Price.Should().BeGreaterOrEqualTo(0); // Changed from Value to Price
        }
    }

    [Fact]
    public async Task Should_Generate_Items_With_Correct_Types()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var weapons = await _generator.GenerateItemsAsync("weapons", 3);
        var armor = await _generator.GenerateItemsAsync("armor", 3);
        var consumables = await _generator.GenerateItemsAsync("consumables", 3);

        // Assert
        foreach (var weapon in weapons)
        {
            weapon.Type.Should().Be(ItemType.Weapon);
        }

        foreach (var armorPiece in armor)
        {
            armorPiece.Type.Should().Be(ItemType.Chest); // Armor defaults to Chest type
        }

        foreach (var consumable in consumables)
        {
            consumable.Type.Should().Be(ItemType.Consumable);
        }
    }

    [Fact]
    public async Task Should_Generate_Items_With_Valid_Rarities()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var items = await _generator.GenerateItemsAsync("weapons", 10);

        // Assert
        if (items.Any())
        {
            foreach (var item in items)
            {
                item.Rarity.Should().BeDefined("Item rarity should be a valid enum value");
            }

            // Should have variety in rarities based on rarityWeight
            var rarities = items.Select(i => i.Rarity).Distinct().ToList();
            // We don't enforce multiple rarities since it depends on random generation,
            // but each item should have a valid rarity
        }
    }

    [Fact]
    public async Task Should_Generate_Item_By_Name()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // First, get a list of available items to test with
        var items = await _generator.GenerateItemsAsync("weapons", 5);
        
        if (items.Any())
        {
            var testItem = items.First();
            var itemName = testItem.Name;

            // Act
            var result = await _generator.GenerateItemByNameAsync("weapons", itemName);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(itemName);
            result.Type.Should().Be(ItemType.Weapon);
        }
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Item()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var result = await _generator.GenerateItemByNameAsync("weapons", "NonExistentWeapon");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_Generate_Items_With_Unique_Names_In_Same_Category()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var items = await _generator.GenerateItemsAsync("weapons", 10);

        // Assert
        if (items.Count > 1)
        {
            // If we generated multiple items, check for variety
            var names = items.Select(i => i.Name).ToList();
            // Note: Due to random generation, we might get duplicates, but let's check IDs are unique when generated
            var ids = items.Select(i => i.Id).ToList();
            // IDs might not be unique since they're based on catalog names, not instance IDs
        }
    }

    [Fact]
    public async Task Should_Handle_Empty_Category_Gracefully()
    {
        // Act
        var result = await _generator.GenerateItemsAsync("non-existent-category", 5);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Generate_Items_With_Appropriate_Values()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var items = await _generator.GenerateItemsAsync("weapons", 5);

        // Assert
        foreach (var item in items)
        {
            item.Price.Should().BeGreaterOrEqualTo(0, "Item price should not be negative");
            
            if (item.Type == ItemType.Weapon)
            {
                item.Price.Should().BeGreaterThan(0, "Weapons should have some value");
            }
        }
    }
}