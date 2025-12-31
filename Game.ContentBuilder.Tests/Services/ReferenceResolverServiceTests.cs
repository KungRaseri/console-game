using FluentAssertions;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Xunit;

namespace Game.ContentBuilder.Tests.Services;

[Trait("Category", "Unit")]
public class ReferenceResolverServiceTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly ReferenceResolverService _service;

    public ReferenceResolverServiceTests()
    {
        // Create temporary test data directory
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);

        // Create test catalog structure
        CreateTestCatalogs();

        _service = new ReferenceResolverService(_testDataPath);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, true);
        }
    }

    private void CreateTestCatalogs()
    {
        // Create items/weapons catalog
        var weaponsDir = Path.Combine(_testDataPath, "items", "weapons");
        Directory.CreateDirectory(weaponsDir);
        File.WriteAllText(Path.Combine(weaponsDir, "catalog.json"), @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""weapons_catalog"",
    ""description"": ""Test weapons""
  },
  ""weapon_types"": {
    ""swords"": {
      ""items"": [
        {
          ""name"": ""iron-longsword"",
          ""rarityWeight"": 100,
          ""damage"": 10,
          ""durability"": 50
        },
        {
          ""name"": ""steel-shortsword"",
          ""rarityWeight"": 50,
          ""damage"": 8,
          ""durability"": 60
        }
      ]
    },
    ""axes"": {
      ""items"": [
        {
          ""name"": ""wooden-hatchet"",
          ""rarityWeight"": 80,
          ""damage"": 6,
          ""durability"": 30
        }
      ]
    }
  }
}");

        // Create abilities/active catalog
        var abilitiesDir = Path.Combine(_testDataPath, "abilities", "active");
        Directory.CreateDirectory(abilitiesDir);
        File.WriteAllText(Path.Combine(abilitiesDir, "catalog.json"), @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""active_abilities_catalog"",
    ""description"": ""Test abilities""
  },
  ""ability_types"": {
    ""offensive"": {
      ""items"": [
        {
          ""name"": ""basic-attack"",
          ""rarityWeight"": 100,
          ""manaCost"": 0,
          ""cooldown"": 1
        }
      ]
    },
    ""defensive"": {
      ""items"": [
        {
          ""name"": ""block"",
          ""rarityWeight"": 100,
          ""manaCost"": 5,
          ""cooldown"": 3
        }
      ]
    }
  }
}");

        // Create enemies/humanoid catalog
        var enemiesDir = Path.Combine(_testDataPath, "enemies", "humanoid");
        Directory.CreateDirectory(enemiesDir);
        File.WriteAllText(Path.Combine(enemiesDir, "catalog.json"), @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""humanoid_enemies_catalog"",
    ""description"": ""Test humanoid enemies""
  },
  ""enemy_types"": {
    ""goblins"": {
      ""items"": [
        {
          ""name"": ""goblin-warrior"",
          ""rarityWeight"": 100,
          ""level"": 5,
          ""health"": 50,
          ""abilities"": [""@abilities/active/offensive:basic-attack""]
        }
      ]
    }
  }
}");
    }

    #region Validation Tests

    [Theory]
    [InlineData("@items/weapons/swords:iron-longsword", true)]
    [InlineData("@abilities/active/offensive:basic-attack", true)]
    [InlineData("@items/weapons/swords:iron-longsword.damage", true)]
    [InlineData("@items/weapons/swords:*", true)]
    [InlineData("@items/weapons/swords:iron-longsword?", true)]
    [InlineData("items/weapons/swords:iron-longsword", false)]  // Missing @
    [InlineData("@items/weapons/:iron-longsword", false)]  // Missing category
    [InlineData("@items/weapons/swords:", false)]  // Missing item name
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidReference_Should_ValidateCorrectly(string? reference, bool expected)
    {
        // Act
        var result = _service.IsValidReference(reference!);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Parsing Tests

    [Fact]
    public void ParseReference_Should_ParseBasicReference()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/swords:iron-longsword");

        // Assert
        result.Should().NotBeNull();
        result!.Domain.Should().Be("items");
        result.Path.Should().Be("weapons");
        result.Category.Should().Be("swords");
        result.ItemName.Should().Be("iron-longsword");
        result.IsOptional.Should().BeFalse();
        result.IsWildcard.Should().BeFalse();
        result.Property.Should().BeNull();
    }

    [Fact]
    public void ParseReference_Should_ParseNestedPath()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/melee/swords:iron-longsword");

        // Assert
        result.Should().NotBeNull();
        result!.Path.Should().Be("weapons/melee");
    }

    [Fact]
    public void ParseReference_Should_ParsePropertyAccess()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/swords:iron-longsword.damage");

        // Assert
        result.Should().NotBeNull();
        result!.Property.Should().Be("damage");
    }

    [Fact]
    public void ParseReference_Should_ParseNestedPropertyAccess()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/swords:iron-longsword.stats.damage");

        // Assert
        result.Should().NotBeNull();
        result!.Property.Should().Be("stats.damage");
    }

    [Fact]
    public void ParseReference_Should_ParseOptionalReference()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/swords:iron-longsword?");

        // Assert
        result.Should().NotBeNull();
        result!.IsOptional.Should().BeTrue();
    }

    [Fact]
    public void ParseReference_Should_ParseWildcardReference()
    {
        // Act
        var result = _service.ParseReference("@items/weapons/swords:*");

        // Assert
        result.Should().NotBeNull();
        result!.IsWildcard.Should().BeTrue();
        result.ItemName.Should().Be("*");
    }

    [Fact]
    public void ParseReference_Should_ReturnNull_ForInvalidReference()
    {
        // Act
        var result = _service.ParseReference("invalid-reference");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Resolution Tests

    [Fact]
    public void ResolveReference_Should_ResolveBasicReference()
    {
        // Act
        var result = _service.ResolveReference("@items/weapons/swords:iron-longsword");

        // Assert
        result.Should().NotBeNull();
        result!["name"]!.ToString().Should().Be("iron-longsword");
        result["damage"]!.Value<int>().Should().Be(10);
        result["durability"]!.Value<int>().Should().Be(50);
    }

    [Fact]
    public void ResolveReference_Should_ResolvePropertyAccess()
    {
        // Act
        var result = _service.ResolveReference("@items/weapons/swords:iron-longsword.damage");

        // Assert
        result.Should().NotBeNull();
        result!.Value<int>().Should().Be(10);
    }

    [Fact]
    public void ResolveReference_Should_ResolveWildcardReference()
    {
        // Act
        var result = _service.ResolveReference("@items/weapons/swords:*");

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
        // Should be one of the swords
        var name = result["name"]!.ToString();
        name.Should().BeOneOf("iron-longsword", "steel-shortsword");
    }

    [Fact]
    public void ResolveReference_Should_ReturnNull_ForMissingItem()
    {
        // Act
        var result = _service.ResolveReference("@items/weapons/swords:nonexistent-sword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveReference_Should_ReturnNull_ForOptionalMissingItem()
    {
        // Act
        var result = _service.ResolveReference("@items/weapons/swords:nonexistent-sword?");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveReference_Should_ReturnNull_ForMissingCatalog()
    {
        // Act
        var result = _service.ResolveReference("@nonexistent/path/category:item");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveReference_Should_ResolveNestedReference()
    {
        // Resolve goblin which references an ability
        var result = _service.ResolveReference("@enemies/humanoid/goblins:goblin-warrior");

        // Assert
        result.Should().NotBeNull();
        var abilities = result!["abilities"] as JArray;
        abilities.Should().NotBeNull();
        abilities!.Count.Should().BeGreaterThan(0);
        abilities[0]!.ToString().Should().Be("@abilities/active/offensive:basic-attack");
    }

    #endregion

    #region Discovery Tests

    [Fact]
    public void GetAvailableDomains_Should_ReturnAllDomains()
    {
        // Act
        var domains = _service.GetAvailableDomains();

        // Assert
        domains.Should().NotBeEmpty();
        domains.Should().Contain("items");
        domains.Should().Contain("abilities");
        domains.Should().Contain("enemies");
    }

    [Fact]
    public void GetAvailableCategories_Should_ReturnCategoriesForDomain()
    {
        // Act
        var categories = _service.GetAvailableCategories("items", "weapons");

        // Assert
        categories.Should().NotBeEmpty();
        categories.Should().Contain("swords");
        categories.Should().Contain("axes");
    }

    [Fact]
    public void GetAvailableReferences_Should_ReturnAllItemsInCategory()
    {
        // Act
        var references = _service.GetAvailableReferences("items", "weapons", "swords");

        // Assert
        references.Should().NotBeEmpty();
        references.Should().Contain("@items/weapons/swords:iron-longsword");
        references.Should().Contain("@items/weapons/swords:steel-shortsword");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ValidateCatalogReferences_Should_DetectInvalidSyntax()
    {
        // Create a catalog with invalid reference (starts with @ but has invalid format)
        var testDir = Path.Combine(_testDataPath, "test");
        Directory.CreateDirectory(testDir);
        var testCatalog = Path.Combine(testDir, "catalog.json");
        File.WriteAllText(testCatalog, @"{
  ""version"": ""4.0"",
  ""type"": ""test_catalog"",
  ""description"": ""Test"",
  ""componentKeys"": [""items""],
  ""components"": {
    ""items"": [
      {
        ""name"": ""test-item"",
        ""rarityWeight"": 100,
        ""reference"": ""@invalid/missing/category""
      }
    ]
  }
}");

        // Act
        var errors = _service.ValidateCatalogReferences(testCatalog);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Message.Contains("Invalid reference syntax"));
    }

    [Fact]
    public void ValidateCatalogReferences_Should_DetectUnresolvableReferences()
    {
        // Create a catalog with unresolvable reference
        var testDir = Path.Combine(_testDataPath, "test2");
        Directory.CreateDirectory(testDir);
        var testCatalog = Path.Combine(testDir, "catalog.json");
        File.WriteAllText(testCatalog, @"{
  ""version"": ""4.0"",
  ""type"": ""test_catalog"",
  ""description"": ""Test"",
  ""componentKeys"": [""items""],
  ""components"": {
    ""items"": [
      {
        ""name"": ""test-item"",
        ""rarityWeight"": 100,
        ""reference"": ""@nonexistent/domain/category:item""
      }
    ]
  }
}");

        // Act
        var errors = _service.ValidateCatalogReferences(testCatalog);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Message.Contains("cannot be resolved"));
    }

    [Fact]
    public void ValidateCatalogReferences_Should_PassForValidReferences()
    {
        // Act
        var weaponsCatalog = Path.Combine(_testDataPath, "items", "weapons", "catalog.json");
        var errors = _service.ValidateCatalogReferences(weaponsCatalog);

        // Assert
        errors.Should().BeEmpty();
    }

    #endregion

    #region Cache Tests

    [Fact]
    public void ClearCache_Should_ClearCatalogCache()
    {
        // Resolve a reference to populate cache
        _service.ResolveReference("@items/weapons/swords:iron-longsword");

        // Act
        _service.ClearCache();

        // Should still work after cache clear (will reload from disk)
        var result = _service.ResolveReference("@items/weapons/swords:iron-longsword");

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ResolveReference_Should_HandleSpecialCharactersInNames()
    {
        // Create catalog with special characters
        var specialDir = Path.Combine(_testDataPath, "special", "items");
        Directory.CreateDirectory(specialDir);
        File.WriteAllText(Path.Combine(specialDir, "catalog.json"), @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""special_catalog"",
    ""description"": ""Test""
  },
  ""item_types"": {
    ""items"": {
      ""items"": [
        {
          ""name"": ""dragon-s-breath"",
          ""rarityWeight"": 100
        }
      ]
    }
  }
}");

        // Act
        var result = _service.ResolveReference("@special/items/items:dragon-s-breath");

        // Assert
        result.Should().NotBeNull();
        result!["name"]!.ToString().Should().Be("dragon-s-breath");
    }

    [Fact]
    public void ResolveReference_Should_RespectRarityWeightsInWildcard()
    {
        // Test that wildcard selection respects rarityWeight
        var selections = new Dictionary<string, int>();
        
        for (int i = 0; i < 100; i++)
        {
            var result = _service.ResolveReference("@items/weapons/swords:*");
            var name = result!["name"]!.ToString();
            
            if (!selections.ContainsKey(name))
                selections[name] = 0;
            selections[name]++;
        }

        // iron-longsword has rarityWeight 100, steel-shortsword has 50
        // So iron-longsword should be selected more often
        selections["iron-longsword"].Should().BeGreaterThan(selections["steel-shortsword"]);
    }

    #endregion
}
