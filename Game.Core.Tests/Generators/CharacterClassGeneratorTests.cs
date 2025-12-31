using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Game.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Game.Core.Tests.Generators;

public class CharacterClassGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly CharacterClassGenerator _generator;

    public CharacterClassGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _generator = new CharacterClassGenerator(_dataCache, NullLogger<CharacterClassGenerator>.Instance);
        _dataCache.LoadAllData();
    }

    [Fact]
    public void Should_Load_All_Character_Classes_From_Catalog()
    {
        // Act
        var classes = _generator.GetAllClasses();

        // Assert
        classes.Should().NotBeNull();
        classes.Should().NotBeEmpty("catalog should contain character classes");
        classes.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Name), "all classes should have names");
    }

    [Fact]
    public void Should_Get_Class_By_Name()
    {
        // Arrange
        var allClasses = _generator.GetAllClasses();
        allClasses.Should().NotBeEmpty("need classes to test with");
        var firstClassName = allClasses.First().Name;

        // Act
        var foundClass = _generator.GetClassByName(firstClassName);

        // Assert
        foundClass.Should().NotBeNull($"should find class with name '{firstClassName}'");
        foundClass!.Name.Should().Be(firstClassName);
    }

    [Fact]
    public void Should_Return_Null_For_Nonexistent_Class()
    {
        // Act
        var nonexistentClass = _generator.GetClassByName("NonexistentClass123");

        // Assert
        nonexistentClass.Should().BeNull("nonexistent class should return null");
    }

    [Fact]
    public void All_Classes_Should_Have_Required_Properties()
    {
        // Act
        var classes = _generator.GetAllClasses();

        // Assert
        classes.Should().NotBeEmpty();
        
        foreach (var characterClass in classes)
        {
            characterClass.Name.Should().NotBeNullOrWhiteSpace("class name is required");
            characterClass.RarityWeight.Should().BeGreaterThan(0, "rarity weight must be positive");
        }
    }

    [Fact]
    public void Should_Generate_Consistent_Results()
    {
        // Act - call multiple times
        var classes1 = _generator.GetAllClasses();
        var classes2 = _generator.GetAllClasses();

        // Assert
        classes1.Should().HaveCount(classes2.Count);
        classes1.Select(c => c.Name).Should().BeEquivalentTo(classes2.Select(c => c.Name));
    }

    [Fact]
    public void Should_Handle_Subclass_Filtering()
    {
        // Arrange
        var allClasses = _generator.GetAllClasses();
        
        if (allClasses.Any())
        {
            // Act - check if we can get classes by category (if method exists)
            // For now, just verify we can call GetClassesByCategory with a test value
            var categoryClasses = _generator.GetClassesByCategory("warrior");

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