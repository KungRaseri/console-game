using FluentAssertions;
using Game.Core.Generators.Modern;
using Game.Data.Services;
using Game.Shared.Models;
using Game.Data.DTOs;

namespace Game.Core.Tests.Generators;

public class CharacterClassGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly CharacterClassGenerator _generator;

    public CharacterClassGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new CharacterClassGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Load_Character_Classes_From_Catalog()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();

        // Assert
        classes.Should().NotBeNull();
        classes.Should().NotBeEmpty();
        
        var fighterClass = classes.FirstOrDefault(c => c.Name.Equals("Fighter", StringComparison.OrdinalIgnoreCase));
        fighterClass.Should().NotBeNull();
        fighterClass!.Name.Should().Be("Fighter");
    }

    [Fact]
    public async Task Should_Get_Class_By_Name()
    {
        // Act
        var fighterClass = await _generator.GetClassByNameAsync("Fighter");

        // Assert
        fighterClass.Should().NotBeNull();
        fighterClass!.Name.Should().Be("Fighter");
        fighterClass.Description.Should().NotBeNullOrEmpty();
        fighterClass.BaseHealth.Should().BeGreaterThan(0);
        fighterClass.BaseMana.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Should_Return_Null_For_Non_Existent_Class()
    {
        // Act
        var result = await _generator.GetClassByNameAsync("NonExistentClass");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Should_Resolve_Starting_Abilities_References()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();
        
        // Assert
        classes.Should().NotBeEmpty();
        
        var classWithAbilities = classes.FirstOrDefault(c => c.StartingAbilities?.Any() == true);
        if (classWithAbilities != null)
        {
            classWithAbilities.StartingAbilities.Should().NotBeNull();
            classWithAbilities.StartingAbilities!.Should().NotBeEmpty();
            
            // Abilities should be resolved from @references to actual IDs
            foreach (var ability in classWithAbilities.StartingAbilities)
            {
                ability.Should().NotBeNullOrEmpty();
                ability.Should().NotStartWith("@", "References should be resolved to actual IDs");
            }
        }
    }

    [Fact]
    public async Task Should_Handle_Parent_Class_References()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();
        
        // Assert
        classes.Should().NotBeEmpty();
        
        var classWithParent = classes.FirstOrDefault(c => !string.IsNullOrEmpty(c.ParentClass));
        if (classWithParent != null)
        {
            classWithParent.ParentClass.Should().NotBeNullOrEmpty();
            classWithParent.ParentClass.Should().NotStartWith("@", "Parent class references should be resolved");
        }
    }

    [Fact]
    public async Task Should_Have_Valid_Character_Class_Properties()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();

        // Assert
        classes.Should().NotBeEmpty();
        
        foreach (var characterClass in classes)
        {
            characterClass.Name.Should().NotBeNullOrEmpty();
            characterClass.Description.Should().NotBeNullOrEmpty();
            characterClass.BaseHealth.Should().BeGreaterThan(0);
            characterClass.BaseMana.Should().BeGreaterOrEqualTo(0);
            characterClass.BaseStrength.Should().BeGreaterThan(0);
            characterClass.BaseDexterity.Should().BeGreaterThan(0);
            characterClass.BaseIntelligence.Should().BeGreaterThan(0);
            characterClass.BaseConstitution.Should().BeGreaterThan(0);
            characterClass.BaseWisdom.Should().BeGreaterThan(0);
            characterClass.BaseCharisma.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Should_Generate_Classes_With_Unique_IDs()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();

        // Assert
        classes.Should().NotBeEmpty();
        
        var ids = classes.Select(c => c.Id).ToList();
        ids.Should().OnlyHaveUniqueItems("Each class should have a unique ID");
    }

    [Fact]
    public async Task Should_Handle_Empty_Starting_Abilities_Gracefully()
    {
        // Act
        var classes = await _generator.GetAllClassesAsync();

        // Assert
        classes.Should().NotBeEmpty();
        
        foreach (var characterClass in classes)
        {
            // StartingAbilities can be null or empty, but if it exists, it should be valid
            if (characterClass.StartingAbilities != null)
            {
                characterClass.StartingAbilities.Should().AllSatisfy(ability => 
                {
                    ability.Should().NotBeNullOrEmpty();
                });
            }
        }
    }
}