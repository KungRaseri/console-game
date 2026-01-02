using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RealmEngine.Core.Generators.Modern;
using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class CharacterClassGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly CharacterClassGenerator _generator;

    public CharacterClassGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        var mockLogger = new Mock<ILogger<ReferenceResolverService>>();
        _referenceResolver = new ReferenceResolverService(_dataCache, mockLogger.Object);
        _generator = new CharacterClassGenerator(_dataCache, _referenceResolver, NullLogger<CharacterClassGenerator>.Instance);
        _dataCache.LoadAllData();
    }

    [Fact]
    public async Task Should_Load_All_Character_Classes_From_Catalog()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync(hydrate: false);

        // Assert
        classes.Should().NotBeNull();
        classes.Should().NotBeEmpty("catalog should contain character classes");
        classes.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Name), "all classes should have names");
    }

    [Fact]
    public async Task Should_Get_Class_By_Name()
    {
        // Arrange
        var allClasses = await _generator.GetAllClassesAsync(hydrate: false);
        allClasses.Should().NotBeEmpty("need classes to test with");
        var firstClassName = allClasses.First().Name;

        // Act
        var foundClass = await _generator.GetClassByNameAsync(firstClassName, hydrate: false);

        // Assert
        foundClass.Should().NotBeNull($"should find class with name '{firstClassName}'");
        foundClass!.Name.Should().Be(firstClassName);
    }

    [Fact]
    public async Task Should_Return_Null_For_Nonexistent_Class()
    {
        // Act
        var nonexistentClass = await _generator.GetClassByNameAsync("NonexistentClass123", hydrate: false);

        // Assert
        nonexistentClass.Should().BeNull("nonexistent class should return null");
    }

    [Fact]
    public async Task All_Classes_Should_Have_Required_Properties()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync(hydrate: false);

        // Assert
        classes.Should().NotBeEmpty();
        
        foreach (var characterClass in classes)
        {
            characterClass.Name.Should().NotBeNullOrWhiteSpace("class name is required");
            characterClass.RarityWeight.Should().BeGreaterThan(0, "rarity weight must be positive");
        }
    }

    [Fact]
    public async Task Should_Generate_Consistent_Results()
    {
        // Act - call multiple times
        var classes1 = await _generator.GetAllClassesAsync(hydrate: false);
        var classes2 = await _generator.GetAllClassesAsync(hydrate: false);

        // Assert
        classes1.Should().HaveCount(classes2.Count);
        classes1.Select(c => c.Name).Should().BeEquivalentTo(classes2.Select(c => c.Name));
    }

    [Fact]
    public async Task Should_Handle_Subclass_Filtering()
    {
        // Arrange
        var allClasses = await _generator.GetAllClassesAsync(hydrate: false);
        
        if (allClasses.Any())
        {
            // Act - check if we can get classes by category (if method exists)
            // For now, just verify we can call GetClassesByCategory with a test value
            var categoryClasses = await _generator.GetClassesByCategoryAsync("warrior", hydrate: false);

            // Assert
            categoryClasses.Should().NotBeNull();
        }
    }

    [Fact]
    public void Should_Have_Data_Cache_Loaded()
    {
        // Act
        var stats = _dataCache.GetStats();
        var totalFiles = _dataCache.TotalFilesLoaded;

        // Assert
        totalFiles.Should().BeGreaterThan(0, "should have loaded JSON files");
        stats.Should().NotBeNull();
    }
}