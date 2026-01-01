using FluentAssertions;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Xunit;

namespace Game.ContentBuilder.Tests.Integration;

[Trait("Category", "Integration")]
public class ReferenceResolutionIntegrationTests
{
    private readonly string _dataPath;
    private readonly ReferenceResolverService _resolver;

    public ReferenceResolutionIntegrationTests()
    {
        // Navigate to Game.Data/Data/Json from test project
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // baseDir is typically: Game.ContentBuilder.Tests/bin/Debug/net9.0-windows/
        // Navigate up to solution root (4 levels: net9.0-windows -> Debug -> bin -> Game.ContentBuilder.Tests -> solution)
        var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
        if (solutionRoot == null)
            throw new DirectoryNotFoundException($"Could not find solution root from: {baseDir}");
            
        _dataPath = Path.Combine(solutionRoot, "RealmEngine.Data", "Data", "Json");

        if (!Directory.Exists(_dataPath))
        {
            throw new DirectoryNotFoundException($"Data directory not found: {_dataPath}");
        }

        _resolver = new ReferenceResolverService(_dataPath);
    }

    #region Domain Discovery Tests

    [Fact]
    public void Should_Discover_All_Domains()
    {
        // Act
        var domains = _resolver.GetAvailableDomains();

        // Assert
        domains.Should().NotBeEmpty();
        domains.Should().Contain("items");
        domains.Should().Contain("enemies");
        domains.Should().Contain("npcs");
        domains.Should().Contain("quests");
        domains.Should().Contain("abilities");
        domains.Should().Contain("classes");
        domains.Should().Contain("world");
        domains.Should().Contain("social");
        domains.Should().Contain("organizations");
        domains.Should().Contain("general");
    }

    [Fact]
    public void Should_Discover_New_Domain_World()
    {
        // Act
        var domains = _resolver.GetAvailableDomains();

        // Assert
        domains.Should().Contain("world");
    }

    [Fact]
    public void Should_Discover_New_Domain_Social()
    {
        // Act
        var domains = _resolver.GetAvailableDomains();

        // Assert
        domains.Should().Contain("social");
    }

    [Fact]
    public void Should_Discover_New_Domain_Organizations()
    {
        // Act
        var domains = _resolver.GetAvailableDomains();

        // Assert
        domains.Should().Contain("organizations");
    }

    #endregion

    #region Items Domain Tests

    [Fact]
    public void Should_Resolve_Items_Weapons_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:Longsword";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().NotBeNull();
        result!["name"]!.ToString().Should().Be("Longsword");
        result["rarityWeight"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Resolve_Items_Armor_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("items", "armor");
        if (categories.Count == 0) return; // Skip if no armor catalog

        var refs = _resolver.GetAvailableReferences("items", "armor", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Get_Available_Weapon_Categories()
    {
        // Act
        var categories = _resolver.GetAvailableCategories("items", "weapons");

        // Assert
        categories.Should().NotBeEmpty();
        categories.Should().Contain("swords");
    }

    #endregion

    #region Enemies Domain Tests

    [Fact]
    public void Should_Resolve_Enemy_Reference()
    {
        // Arrange
        var reference = "@enemies/goblinoids/goblins:Goblin";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().NotBeNull();
        result!["name"]?.ToString().Should().Be("Goblin");
    }

    [Fact]
    public void Should_Resolve_Enemy_With_Ability_References()
    {
        // Arrange
        var reference = "@enemies/goblinoids/goblins:Goblin";

        // Act
        var enemy = _resolver.ResolveReference(reference);

        // Assert
        enemy.Should().NotBeNull();
        var abilities = enemy!["abilities"] as JArray;
        abilities.Should().NotBeNull();

        // Verify ability references are in correct format
        foreach (var ability in abilities!)
        {
            var abilityRef = ability.ToString();
            abilityRef.Should().StartWith("@abilities/");
        }
    }

    #endregion

    #region Abilities Domain Tests

    [Fact]
    public void Should_Resolve_Ability_Reference()
    {
        // Arrange
        var reference = "@abilities/active/offensive:Infernal Flames";

        // Act
        var components = _resolver.ParseReference(reference);
        System.Console.WriteLine($"Parsed: Domain={components?.Domain}, Path={components?.Path}, Category={components?.Category}, Item={components?.ItemName}");
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().NotBeNull();
        result!["name"]?.ToString().Should().Be("Infernal Flames");
    }

    #endregion

    #region Classes Domain Tests

    [Fact]
    public void Should_Resolve_Class_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("classes", ".");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("classes", ".", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
    }

    #endregion

    #region Quests Domain Tests

    [Fact]
    public void Should_Resolve_Quest_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("quests", "main-story");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("quests", "main-story", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Resolve_Quest_With_Location_References()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("quests", "main-story");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("quests", "main-story", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var quest = _resolver.ResolveReference(refs[0]);

        // Assert - quest may have location references
        quest.Should().NotBeNull();
        var locationRef = quest!["location"]?.ToString();
        if (locationRef != null && locationRef.StartsWith("@"))
        {
            locationRef.Should().StartWith("@world/");
        }
    }

    #endregion

    #region World Domain Tests (New)

    [Fact]
    public void Should_Resolve_World_Region_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("world", "regions");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("world", "regions", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
        result["rarityWeight"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Resolve_World_Environment_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("world", "environments");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("world", "environments", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
    }

    #endregion

    #region Organizations Domain Tests (New)

    [Fact]
    public void Should_Resolve_Organization_Guild_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("organizations", "guilds");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("organizations", "guilds", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
        result["rarityWeight"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Resolve_Organization_Shop_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("organizations", "shops");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("organizations", "shops", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
        result["rarityWeight"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Resolve_Organization_Business_Reference()
    {
        // Arrange
        var categories = _resolver.GetAvailableCategories("organizations", "businesses");
        if (categories.Count == 0) return;

        var refs = _resolver.GetAvailableReferences("organizations", "businesses", categories[0]);
        if (refs.Count == 0) return;

        // Act
        var result = _resolver.ResolveReference(refs[0]);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
        result["rarityWeight"].Should().NotBeNull();
    }

    #endregion

    #region Wildcard Tests

    [Fact]
    public void Should_Resolve_Wildcard_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:*";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().NotBeNull();
        result!["name"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Respect_RarityWeight_In_Wildcard()
    {
        // Arrange
        var reference = "@items/weapons/swords:*";
        var selections = new Dictionary<string, int>();

        // Act - resolve wildcard 100 times
        for (int i = 0; i < 100; i++)
        {
            _resolver.ClearCache(); // Clear cache to get new random selection
            var result = _resolver.ResolveReference(reference);
            if (result != null)
            {
                var name = result["name"]!.ToString();
                if (!selections.ContainsKey(name))
                    selections[name] = 0;
                selections[name]++;
            }
        }

        // Assert - should have selected multiple items
        selections.Should().NotBeEmpty();
        selections.Count.Should().BeGreaterThan(1, "wildcard should select different items");
    }

    #endregion

    #region Property Access Tests

    [Fact]
    public void Should_Resolve_Property_Access()
    {
        // Arrange
        var reference = "@items/weapons/swords:Longsword.rarityWeight";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().NotBeNull();
        result!.Type.Should().Be(JTokenType.Integer);
    }

    #endregion

    #region Optional Reference Tests

    [Fact]
    public void Should_Return_Null_For_Optional_Missing_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:nonexistent-sword?";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Should_Return_Null_For_Non_Optional_Missing_Reference()
    {
        // Arrange
        var reference = "@items/weapons/swords:nonexistent-sword";

        // Act
        var result = _resolver.ResolveReference(reference);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Cross-Domain Reference Tests

    [Fact]
    public void Should_Validate_Cross_Domain_References_In_Enemies()
    {
        // Arrange - find an enemy catalog
        var enemyCatalog = Path.Combine(_dataPath, "enemies", "goblinoids", "catalog.json");
        if (!File.Exists(enemyCatalog)) return;

        // Act
        var errors = _resolver.ValidateCatalogReferences(enemyCatalog);

        // Assert
        errors.Where(e => e.Severity == ErrorSeverity.Error).Should().BeEmpty();
    }

    [Fact]
    public void Should_Validate_Cross_Domain_References_In_Classes()
    {
        // Arrange
        var classCatalog = Path.Combine(_dataPath, "classes", "catalog.json");
        if (!File.Exists(classCatalog)) return;

        // Act
        var errors = _resolver.ValidateCatalogReferences(classCatalog);

        // Assert
        errors.Where(e => e.Severity == ErrorSeverity.Error).Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Should_Cache_Catalog_Loads()
    {
        // Arrange
        var reference = "@items/weapons/swords:iron-longsword";

        // Act - resolve same reference twice
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _resolver.ResolveReference(reference);
        var firstTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        _resolver.ResolveReference(reference);
        var secondTime = stopwatch.ElapsedMilliseconds;

        // Assert - second should be faster due to caching
        secondTime.Should().BeLessThanOrEqualTo(firstTime);
    }

    #endregion

    #region Comprehensive Domain Coverage

    [Theory]
    [InlineData("items")]
    [InlineData("enemies")]
    [InlineData("npcs")]
    [InlineData("quests")]
    [InlineData("abilities")]
    [InlineData("classes")]
    [InlineData("world")]
    [InlineData("social")]
    [InlineData("organizations")]
    public void Should_Have_At_Least_One_Catalog_Per_Domain(string domain)
    {
        // Act
        var domainPath = Path.Combine(_dataPath, domain);
        
        // Assert
        Directory.Exists(domainPath).Should().BeTrue($"{domain} domain should exist");
        
        // Find at least one catalog.json
        var catalogs = Directory.GetFiles(domainPath, "catalog.json", SearchOption.AllDirectories);
        catalogs.Should().NotBeEmpty($"{domain} should have at least one catalog.json");
    }

    #endregion
}
