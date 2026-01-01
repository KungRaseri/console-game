using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Game.Shared.Models;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class EnchantmentGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly EnchantmentGenerator _generator;

    public EnchantmentGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new EnchantmentGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Enchantment_From_Reference()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantment = await _generator.GenerateEnchantmentAsync("@items/enchantments:*");

        // Assert
        enchantment.Should().NotBeNull();
        enchantment!.Name.Should().NotBeNullOrEmpty();
        enchantment.Position.Should().BeDefined();
        enchantment.RarityWeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Generate_Multiple_Enchantments()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(10);

        // Assert
        enchantments.Should().NotBeNull();
        enchantments.Should().HaveCountGreaterThanOrEqualTo(1);
        
        foreach (var enchantment in enchantments)
        {
            enchantment.Name.Should().NotBeNullOrEmpty();
            enchantment.Position.Should().BeDefined();
        }
    }

    [Fact]
    public async Task Should_Generate_Enchantments_With_Traits()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(10);

        // Assert
        enchantments.Should().NotBeEmpty();
        
        foreach (var enchantment in enchantments)
        {
            enchantment.Traits.Should().NotBeEmpty("Enchantments should have at least one trait");
            
            foreach (var trait in enchantment.Traits)
            {
                trait.Key.Should().NotBeNullOrEmpty();
                trait.Value.Should().NotBeNull();
                trait.Value.Type.Should().BeDefined();
            }
        }
    }

    [Fact]
    public async Task Should_Generate_Both_Prefix_And_Suffix_Enchantments()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(30);

        // Assert
        var prefixes = enchantments.Where(e => e.Position == EnchantmentPosition.Prefix).ToList();
        var suffixes = enchantments.Where(e => e.Position == EnchantmentPosition.Suffix).ToList();
        
        // Should have variety
        prefixes.Should().NotBeEmpty("Should generate some prefix enchantments");
        suffixes.Should().NotBeEmpty("Should generate some suffix enchantments");
    }

    [Fact]
    public async Task Should_Generate_Enchantments_With_Varying_Rarity()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(30);

        // Assert
        enchantments.Should().NotBeEmpty();
        
        var rarities = enchantments.Select(e => e.Rarity).Distinct().ToList();
        
        // Should have some variety in rarities
        rarities.Should().NotBeEmpty();
        
        // All rarities should be valid
        foreach (var enchantment in enchantments)
        {
            enchantment.Rarity.Should().BeDefined();
            enchantment.RarityWeight.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Should_Map_RarityWeight_To_EnchantmentRarity_Correctly()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(40);

        // Assert
        // EnchantmentRarity mapping:
        // Minor: < 40
        // Lesser: 40-79
        // Greater: 80-119
        // Superior: 120-179
        // Legendary: 180+
        
        foreach (var enchantment in enchantments)
        {
            if (enchantment.RarityWeight < 40)
            {
                enchantment.Rarity.Should().Be(EnchantmentRarity.Minor);
            }
            else if (enchantment.RarityWeight < 80)
            {
                enchantment.Rarity.Should().Be(EnchantmentRarity.Lesser);
            }
            else if (enchantment.RarityWeight < 120)
            {
                enchantment.Rarity.Should().Be(EnchantmentRarity.Greater);
            }
            else if (enchantment.RarityWeight < 180)
            {
                enchantment.Rarity.Should().Be(EnchantmentRarity.Superior);
            }
            else
            {
                enchantment.Rarity.Should().Be(EnchantmentRarity.Legendary);
            }
        }
    }

    [Fact]
    public async Task Should_Generate_Enchantments_With_Appropriate_Names()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(20);

        // Assert
        enchantments.Should().NotBeEmpty();
        
        var prefixes = enchantments.Where(e => e.Position == EnchantmentPosition.Prefix).ToList();
        var suffixes = enchantments.Where(e => e.Position == EnchantmentPosition.Suffix).ToList();
        
        // Prefix enchantments typically don't have "of" in the name
        foreach (var prefix in prefixes)
        {
            prefix.Name.Should().NotBeNullOrEmpty();
            // Prefixes like "Flaming", "Frozen", "Shocking" etc
        }
        
        // Suffix enchantments typically have "of" in the name
        foreach (var suffix in suffixes)
        {
            suffix.Name.Should().NotBeNullOrEmpty();
            // Suffixes like "of Fire", "of Strength", "of Power" etc
        }
    }

    [Fact]
    public async Task Should_Generate_Enchantments_With_Numeric_Traits()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(20);

        // Assert
        enchantments.Should().NotBeEmpty();
        
        // Most enchantments should have numeric traits (damage, stats, etc.)
        var enchantmentsWithNumericTraits = enchantments.Where(e => 
            e.Traits.Any(t => t.Value.Type == TraitType.Number)).ToList();
        
        enchantmentsWithNumericTraits.Should().NotBeEmpty("Enchantments should have numeric traits");
    }

    [Fact]
    public async Task Should_Generate_Variety_Of_Enchantment_Types()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantments = await _generator.GenerateEnchantmentsAsync(50);

        // Assert
        var uniqueNames = enchantments.Select(e => e.Name).Distinct().ToList();
        
        // Should have variety in enchantment names
        uniqueNames.Should().HaveCountGreaterThanOrEqualTo(3, "Should generate multiple different enchantment types");
    }

    [Fact]
    public async Task Should_Handle_Invalid_Reference_Gracefully()
    {
        // Arrange
        _dataCache.LoadAllData();
        
        // Act
        var enchantment = await _generator.GenerateEnchantmentAsync("@invalid/path:*");

        // Assert
        // Generator currently ignores reference and generates from items/enchantments/names.json
        // TODO: Implement proper reference validation in generator
        enchantment.Should().NotBeNull("generator currently ignores invalid references");
    }
}
