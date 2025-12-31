using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace Game.Tests.Integration;

/// <summary>
/// Tests for JSON v4.0 catalog standards compliance
/// Validates structure, metadata, componentKeys, rarityWeight, and cross-domain references
/// </summary>
public class CatalogComplianceTests
{
    private const string DataPath = "../../../../Game.Data/Data/Json";
    private readonly List<string> _catalogPaths;

    public CatalogComplianceTests()
    {
        _catalogPaths = new List<string>
        {
            // Abilities domain (2 catalogs)
            "abilities/active/catalog.json",
            "abilities/passive/catalog.json",
            
            // Classes domain (1 catalog)
            "classes/catalog.json",
            
            // Enemies domain (4 catalogs)
            "enemies/beast/catalog.json",
            "enemies/humanoid/catalog.json",
            "enemies/undead/catalog.json",
            "enemies/magical/catalog.json",
            
            // Items domain (6 catalogs)
            "items/weapons/catalog.json",
            "items/armor/catalog.json",
            "items/potions/catalog.json",
            "items/materials/catalog.json",
            "items/jewelry/catalog.json",
            "items/misc/catalog.json",
            
            // NPCs domain (2 catalogs)
            "npcs/catalog.json",
            "npcs/social_classes/catalog.json",
            
            // Quests domain (1 catalog)
            "quests/catalog.json",
            
            // World domain (3 catalogs)
            "world/locations/towns/catalog.json",
            "world/locations/dungeons/catalog.json",
            "world/locations/wilderness/catalog.json",
            "world/regions/catalog.json",
            "world/environments/catalog.json",
            
            // Social domain (4 catalogs)
            "social/dialogue/styles/catalog.json",
            "social/dialogue/greetings/catalog.json",
            "social/dialogue/farewells/catalog.json",
            "social/dialogue/responses/catalog.json",
            
            // Organizations domain (3 catalogs)
            "organizations/factions/catalog.json",
            "organizations/guilds/catalog.json",
            "organizations/shops/catalog.json",
            "organizations/businesses/catalog.json"
        };
    }

    #region JSON v4.0 Structure Tests

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Should_Have_Required_Metadata(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));

        // Assert
        json.Should().ContainKey("description", $"{catalogPath} missing description");
        json.Should().ContainKey("version", $"{catalogPath} missing version");
        json.Should().ContainKey("lastUpdated", $"{catalogPath} missing lastUpdated");
        json.Should().ContainKey("type", $"{catalogPath} missing type");
        
        json["version"]?.ToString().Should().Be("4.0", $"{catalogPath} not using v4.0");
        json["type"]?.ToString().Should().EndWith("catalog", $"{catalogPath} invalid type");
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Should_Have_Valid_LastUpdated_Date(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));

        // Act
        var dateStr = json["lastUpdated"]?.ToString();
        var isValidDate = DateTime.TryParse(dateStr, out var date);

        // Assert
        isValidDate.Should().BeTrue($"{catalogPath} has invalid lastUpdated date: {dateStr}");
        date.Should().BeOnOrBefore(DateTime.UtcNow, $"{catalogPath} lastUpdated is in the future");
        date.Should().BeAfter(new DateTime(2025, 1, 1), $"{catalogPath} lastUpdated too old");
    }

    [Theory]
    [MemberData(nameof(GetHierarchicalCatalogs))]
    public void Should_Have_ComponentKeys_For_Hierarchical_Catalogs(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));

        // Assert
        if (json["type"]?.ToString().Contains("hierarchical") == true)
        {
            json.Should().ContainKey("componentKeys", $"{catalogPath} missing componentKeys");
            var keys = json["componentKeys"] as JArray;
            keys.Should().NotBeNull().And.NotBeEmpty($"{catalogPath} componentKeys empty");
        }
    }

    [Theory]
    [MemberData(nameof(GetHierarchicalCatalogs))]
    public void Should_Have_Components_Section(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));

        // Assert
        json.Should().ContainKey("components", $"{catalogPath} missing components section");
        var components = json["components"] as JObject;
        components.Should().NotBeNull($"{catalogPath} components not an object");
    }

    [Theory]
    [MemberData(nameof(GetHierarchicalCatalogs))]
    public void ComponentKeys_Should_Match_Actual_Components(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var componentKeys = json["componentKeys"] as JArray;
        var components = json["components"] as JObject;

        // Act
        var declaredKeys = componentKeys?.Select(k => k.ToString()).ToList() ?? new List<string>();
        var actualKeys = components?.Properties().Select(p => p.Name).ToList() ?? new List<string>();

        // Assert
        declaredKeys.Should().BeEquivalentTo(actualKeys, 
            $"{catalogPath} componentKeys don't match actual components");
    }

    #endregion

    #region RarityWeight Tests

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void All_Items_Should_Have_RarityWeight(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allItems = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var item in allItems)
        {
            item.Should().ContainKey("rarityWeight", 
                $"{catalogPath} - Item '{item["name"]}' missing rarityWeight");
            
            var weight = item["rarityWeight"];
            weight.Should().NotBeNull($"{catalogPath} - Item '{item["name"]}' has null rarityWeight");
            
            var weightValue = weight.Value<int>();
            weightValue.Should().BeGreaterThan(0, 
                $"{catalogPath} - Item '{item["name"]}' has invalid rarityWeight: {weightValue}");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Should_Have_Valid_RarityWeight_Distribution(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allItems = GetAllItemsFromCatalog(json);

        if (!allItems.Any()) return; // Skip empty catalogs

        // Act
        var weights = allItems.Select(i => i["rarityWeight"]?.Value<int>() ?? 0).ToList();
        var commonItems = weights.Count(w => w <= 10);  // Common
        var uncommonItems = weights.Count(w => w > 10 && w <= 20);  // Uncommon
        var rareItems = weights.Count(w => w > 20 && w <= 30);  // Rare
        var epicItems = weights.Count(w => w > 30 && w <= 40);  // Epic
        var legendaryItems = weights.Count(w => w > 40);  // Legendary

        // Assert - Common items should be most frequent
        if (weights.Any(w => w <= 10))
        {
            commonItems.Should().BeGreaterThanOrEqualTo(rareItems, 
                $"{catalogPath} should have more common items than rare");
        }
    }

    #endregion

    #region Name Field Tests

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void All_Items_Should_Have_Name_Field(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allItems = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var item in allItems)
        {
            item.Should().ContainKey("name", $"{catalogPath} - Item missing 'name' field");
            var name = item["name"]?.ToString();
            name.Should().NotBeNullOrWhiteSpace($"{catalogPath} - Item has empty name");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Item_Names_Should_Be_Unique_Within_Category(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var components = json["components"] as JObject;

        if (components == null) return;

        // Act & Assert
        foreach (var category in components.Properties())
        {
            var items = category.Value as JArray;
            if (items == null) continue;

            var names = items
                .Select(i => i["name"]?.ToString())
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            names.Should().OnlyHaveUniqueItems(
                $"{catalogPath} - Category '{category.Name}' has duplicate item names");
        }
    }

    #endregion

    #region Cross-Domain Reference Tests

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void References_Should_Use_Valid_Syntax(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allReferences = FindAllReferences(json);

        // Assert
        foreach (var reference in allReferences)
        {
            reference.Should().MatchRegex(@"^@[\w-]+/([\w-]+/)*[\w-]+:[\w-*]+(\?)?(\.\w+)*$",
                $"{catalogPath} - Invalid reference syntax: {reference}");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void References_Should_Point_To_Valid_Domains(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allReferences = FindAllReferences(json);
        var validDomains = new[] { "abilities", "classes", "enemies", "items", "npcs", "quests", 
            "world", "social", "organizations", "general" };

        // Assert
        foreach (var reference in allReferences)
        {
            var domain = reference.Split('/')[0].TrimStart('@');
            validDomains.Should().Contain(domain,
                $"{catalogPath} - Reference uses invalid domain: {reference}");
        }
    }

    [Fact]
    public void Classes_Should_Reference_Valid_Abilities()
    {
        // Arrange
        var classesPath = Path.Combine(DataPath, "classes/catalog.json");
        var json = JObject.Parse(File.ReadAllText(classesPath));
        var allClasses = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var classItem in allClasses)
        {
            var abilities = classItem["startingAbilities"] as JArray;
            if (abilities != null)
            {
                foreach (var ability in abilities)
                {
                    var abilityRef = ability.ToString();
                    abilityRef.Should().StartWith("@abilities/",
                        $"Class '{classItem["name"]}' has invalid ability reference: {abilityRef}");
                }
            }
        }
    }

    [Fact]
    public void Quests_Should_Reference_Valid_Locations()
    {
        // Arrange
        var questsPath = Path.Combine(DataPath, "quests/catalog.json");
        var json = JObject.Parse(File.ReadAllText(questsPath));

        // Assert
        var locationRefs = json["location_references"] as JObject;
        locationRefs.Should().NotBeNull("Quests catalog should have location_references");
        
        var wilderness = locationRefs?["wilderness"]?.ToString();
        var towns = locationRefs?["towns"]?.ToString();
        var dungeons = locationRefs?["dungeons"]?.ToString();

        wilderness.Should().Be("@world/locations/wilderness");
        towns.Should().Be("@world/locations/towns");
        dungeons.Should().Be("@world/locations/dungeons");
    }

    [Fact]
    public void Regions_Should_Reference_Valid_Factions_And_Locations()
    {
        // Arrange
        var regionsPath = Path.Combine(DataPath, "world/regions/catalog.json");
        var json = JObject.Parse(File.ReadAllText(regionsPath));
        var allRegions = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var region in allRegions)
        {
            // Check capital reference
            var capital = region["capital"]?.ToString();
            if (!string.IsNullOrEmpty(capital))
            {
                capital.Should().MatchRegex(@"^@world/locations/(towns|cities):\w+",
                    $"Region '{region["name"]}' has invalid capital reference: {capital}");
            }

            // Check faction references
            var factions = region["factions"] as JArray;
            if (factions != null)
            {
                foreach (var faction in factions)
                {
                    var factionRef = faction.ToString();
                    factionRef.Should().StartWith("@organizations/factions:",
                        $"Region '{region["name"]}' has invalid faction reference: {factionRef}");
                }
            }
        }
    }

    [Fact]
    public void Environments_Should_Reference_Valid_Enemies_And_Items()
    {
        // Arrange
        var envPath = Path.Combine(DataPath, "world/environments/catalog.json");
        var json = JObject.Parse(File.ReadAllText(envPath));
        var biomes = json["components"]?["biomes"] as JArray;

        // Assert
        if (biomes != null)
        {
            foreach (var biome in biomes)
            {
                // Check enemy references
                var encounters = biome["commonEncounters"] as JArray;
                if (encounters != null)
                {
                    foreach (var encounter in encounters)
                    {
                        var encounterRef = encounter.ToString();
                        encounterRef.Should().StartWith("@enemies/",
                            $"Biome '{biome["name"]}' has invalid encounter reference: {encounterRef}");
                    }
                }

                // Check item references
                var resources = biome["resources"] as JArray;
                if (resources != null)
                {
                    foreach (var resource in resources)
                    {
                        var resourceRef = resource.ToString();
                        resourceRef.Should().StartWith("@items/",
                            $"Biome '{biome["name"]}' has invalid resource reference: {resourceRef}");
                    }
                }
            }
        }
    }

    [Fact]
    public void Shops_Should_Reference_Valid_Item_Categories()
    {
        // Arrange
        var shopsPath = Path.Combine(DataPath, "organizations/shops/catalog.json");
        var json = JObject.Parse(File.ReadAllText(shopsPath));
        var allShops = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var shop in allShops)
        {
            var inventory = shop["inventory"]?["categories"] as JArray;
            if (inventory != null)
            {
                foreach (var category in inventory)
                {
                    var categoryRef = category["category"]?.ToString();
                    if (!string.IsNullOrEmpty(categoryRef))
                    {
                        categoryRef.Should().StartWith("@items/",
                            $"Shop '{shop["name"]}' has invalid inventory reference: {categoryRef}");
                    }
                }
            }
        }
    }

    #endregion

    #region Content Quality Tests

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Should_Not_Have_Empty_Categories(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var components = json["components"] as JObject;

        if (components == null) return;

        // Assert
        foreach (var category in components.Properties())
        {
            var items = category.Value as JArray;
            items.Should().NotBeNull()
                .And.NotBeEmpty($"{catalogPath} - Category '{category.Name}' is empty");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void All_Items_Should_Have_Description(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allItems = GetAllItemsFromCatalog(json);

        // Assert
        foreach (var item in allItems)
        {
            item.Should().ContainKey("description", 
                $"{catalogPath} - Item '{item["name"]}' missing description");
            
            var description = item["description"]?.ToString();
            description.Should().NotBeNullOrWhiteSpace(
                $"{catalogPath} - Item '{item["name"]}' has empty description");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllCatalogs))]
    public void Should_Not_Have_Forbidden_Fields(string catalogPath)
    {
        // Arrange
        var fullPath = Path.Combine(DataPath, catalogPath);
        var json = JObject.Parse(File.ReadAllText(fullPath));
        var allItems = GetAllItemsFromCatalog(json);
        var forbiddenFields = new[] { "example", "todo", "fixme", "test" };

        // Assert
        foreach (var item in allItems)
        {
            foreach (var field in forbiddenFields)
            {
                item.Should().NotContainKey(field,
                    $"{catalogPath} - Item '{item["name"]}' contains forbidden field: {field}");
            }
        }
    }

    #endregion

    #region Helper Methods

    private List<JObject> GetAllItemsFromCatalog(JObject catalog)
    {
        var items = new List<JObject>();
        var components = catalog["components"] as JObject;

        if (components == null) return items;

        foreach (var category in components.Properties())
        {
            var categoryItems = category.Value as JArray;
            if (categoryItems != null)
            {
                items.AddRange(categoryItems.OfType<JObject>());
            }
        }

        return items;
    }

    private List<string> FindAllReferences(JToken token)
    {
        var references = new List<string>();

        if (token is JValue value && value.Type == JTokenType.String)
        {
            var str = value.ToString();
            if (str.StartsWith("@"))
            {
                references.Add(str);
            }
        }
        else if (token is JArray array)
        {
            foreach (var item in array)
            {
                references.AddRange(FindAllReferences(item));
            }
        }
        else if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                references.AddRange(FindAllReferences(property.Value));
            }
        }

        return references;
    }

    public static IEnumerable<object[]> GetAllCatalogs()
    {
        var instance = new CatalogComplianceTests();
        return instance._catalogPaths.Select(path => new object[] { path });
    }

    public static IEnumerable<object[]> GetHierarchicalCatalogs()
    {
        var instance = new CatalogComplianceTests();
        // Most catalogs are hierarchical
        return instance._catalogPaths
            .Where(p => !p.Contains("classes/catalog.json")) // Flat catalog
            .Select(path => new object[] { path });
    }

    #endregion
}
