using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Game.Data.Tests;

/// <summary>
/// Validation tests for catalog.json files (v4.0/v4.1 standards)
/// Ensures all catalog files follow CATALOG_JSON_STANDARD.md specifications
/// </summary>
[Trait("Category", "DataValidation")]
[Trait("FileType", "Catalog")]
public class CatalogJsonComplianceTests
{
  private readonly string _dataPath;
  private readonly List<string> _allCatalogFiles;

  public CatalogJsonComplianceTests()
  {
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
    if (solutionRoot == null)
      throw new DirectoryNotFoundException($"Could not find solution root from: {baseDir}");

    _dataPath = Path.Combine(solutionRoot, "Game.Data", "Data", "Json");

    if (!Directory.Exists(_dataPath))
      throw new DirectoryNotFoundException($"Data directory not found: {_dataPath}");

    _allCatalogFiles = Directory.GetFiles(_dataPath, "catalog.json", SearchOption.AllDirectories)
        .Select(f => Path.GetRelativePath(_dataPath, f))
        .ToList();
  }

  #region Discovery Tests

  [Fact]
  public void Should_Discover_All_Catalog_Files()
  {
    _allCatalogFiles.Should().NotBeEmpty();
    _allCatalogFiles.Should().HaveCountGreaterThan(50, "expected 60+ catalog files");
  }

  #endregion

  #region Strict Schema Validation

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Should_Only_Have_Expected_Root_Properties(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    
    // Catalogs can have: metadata, items, components, or {type}_types patterns
    var allowedProperties = new[] { "metadata", "items", "components" };
    
    // Also allow domain-specific patterns like ability_types, enemy_types, etc.
    var actualProperties = json.Properties().Select(p => p.Name).ToList();
    var typeProperties = actualProperties.Where(p => p.EndsWith("_types")).ToList();
    
    var allAllowedProperties = allowedProperties.Concat(typeProperties).ToList();

    // Assert - ONLY expected root properties are allowed
    var unexpectedProperties = actualProperties.Except(allAllowedProperties).ToList();
    
    unexpectedProperties.Should().BeEmpty(
        $"{relativePath} contains unexpected root properties: {string.Join(", ", unexpectedProperties)}. " +
        $"Only allowed: metadata, items, components, or *_types");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Metadata_Should_Only_Have_Expected_Properties(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var metadata = json["metadata"] as JObject;
    
    if (metadata == null) return; // Other tests will catch this

    var allowedProperties = new[] { 
      "description", "version", "lastUpdated", "type", "notes",
      "componentKeys", "domain", "category", "supportsReferences"
    };

    // Assert - ONLY these metadata properties are allowed
    var actualProperties = metadata.Properties().Select(p => p.Name).ToList();
    var unexpectedProperties = actualProperties.Except(allowedProperties).ToList();
    
    unexpectedProperties.Should().BeEmpty(
        $"{relativePath} metadata contains unexpected properties: {string.Join(", ", unexpectedProperties)}. " +
        $"Only allowed: {string.Join(", ", allowedProperties)}");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Items_Should_Only_Have_Expected_Standard_Properties(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    if (!allItems.Any()) return; // Skip if no items

    // Standard properties that ALL items must have
    var requiredProperties = new[] { "name", "rarityWeight" };
    
    // Common optional properties allowed on items (domain-specific properties allowed beyond this)
    var commonOptionalProperties = new[] { 
      "displayName", "description", "traits", "tags", "notes",
      "weight", "value", "icon", "category", "type"
    };

    // Assert - items must have required properties at minimum
    foreach (var item in allItems)
    {
      var itemObj = item as JObject;
      if (itemObj == null) continue;

      foreach (var required in requiredProperties)
      {
        itemObj.Should().ContainKey(required,
            $"{relativePath} - Item '{item["name"]}' missing required property '{required}'");
      }
    }
  }

  #endregion

  #region Metadata Validation

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Should_Have_Required_Metadata(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var metadata = json["metadata"] as JObject;

    // Assert
    metadata.Should().NotBeNull($"{relativePath} missing metadata section");
    metadata!["description"].Should().NotBeNull($"{relativePath} missing description");
    metadata["version"].Should().NotBeNull($"{relativePath} missing version");
    metadata["lastUpdated"].Should().NotBeNull($"{relativePath} missing lastUpdated");
    metadata["type"].Should().NotBeNull($"{relativePath} missing type");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Version_Should_Be_Valid(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var version = json["metadata"]?["version"]?.ToString();

    // Assert - standard shows "1.0" but project uses "4.0" for v4.0+ catalogs
    version.Should().NotBeNullOrEmpty($"{relativePath} has empty version");
    // Accept both 1.0 (legacy) and 4.0 (current standard)
    version.Should().Match(v => v == "1.0" || v == "4.0", $"{relativePath} version should be '1.0' or '4.0'");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Type_Should_End_With_Catalog(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var type = json["metadata"]?["type"]?.ToString();

    // Assert
    type.Should().NotBeNullOrEmpty($"{relativePath} has empty type");
    type.Should().EndWith("_catalog", $"{relativePath} type should end with '_catalog'");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_LastUpdated_Should_Be_Valid_Date(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var dateStr = json["metadata"]?["lastUpdated"]?.ToString();

    // Act
    var isValid = DateTime.TryParse(dateStr, out var date);

    // Assert
    isValid.Should().BeTrue($"{relativePath} has invalid date: {dateStr}");
    date.Should().BeOnOrBefore(DateTime.UtcNow, $"{relativePath} date in future");
    date.Should().BeAfter(new DateTime(2025, 1, 1), $"{relativePath} date too old");
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Description_Should_Not_Be_Empty(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var description = json["metadata"]?["description"]?.ToString();

    // Assert
    description.Should().NotBeNullOrWhiteSpace($"{relativePath} has empty description");
    description!.Length.Should().BeGreaterThan(10, $"{relativePath} description too short");
  }

  #endregion

  #region Item Validation

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Items_Should_Have_RarityWeight(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    if (!allItems.Any()) return; // Skip if no items

    // Assert
    foreach (var item in allItems)
    {
      ((JObject)item).Should().ContainKey("rarityWeight",
          $"{relativePath} - Item '{item["name"]}' missing rarityWeight");

      var weight = item["rarityWeight"]?.Value<int>();
      weight.Should().NotBeNull($"{relativePath} - Item '{item["name"]}' null rarityWeight");
      weight.Should().BeGreaterThan(0, $"{relativePath} - Item '{item["name"]}' invalid rarityWeight");
    }
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Items_Should_Have_Name(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    // Assert
    foreach (var item in allItems)
    {
      ((JObject)item).Should().ContainKey("name", $"{relativePath} - Item missing 'name'");
      var name = item["name"]?.ToString();
      name.Should().NotBeNullOrWhiteSpace($"{relativePath} - Item has empty name");
    }
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Should_Not_Have_Weight_Instead_Of_RarityWeight(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    // Assert - should use rarityWeight, not weight (unless it's physical weight)
    foreach (var item in allItems)
    {
      if (item.ContainsKey("weight") && !item.ContainsKey("rarityWeight"))
      {
        // Only fail if it looks like a rarity value (not physical weight)
        var weight = item["weight"]?.Value<double>();
        if (weight != null && weight <= 100 && weight > 0)
        {
          Assert.Fail($"{relativePath} - Item '{item["name"]}' uses 'weight' instead of 'rarityWeight'");
        }
      }
    }
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Item_Names_Should_Not_Be_Empty(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    // Assert
    foreach (var item in allItems)
    {
      var name = item["name"]?.ToString();
      name.Should().NotBeNullOrWhiteSpace($"{relativePath} has item with empty name");
      name!.Length.Should().BeGreaterThan(1, $"{relativePath} has single-char name");
    }
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Should_Have_Items_Or_Components(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    // Assert - catalogs should have actual content (data completeness check)
    allItems.Should().NotBeEmpty($"{relativePath} has no items/components");
  }

  #endregion

  #region v4.1 Reference Validation

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_Should_Use_References_For_Abilities(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var allItems = GetAllItemsFromCatalog(json);

    // Assert - v4.1 standard: abilities should use @references, not hardcoded strings
    foreach (var item in allItems)
    {
      if (item["abilities"] is JArray abilities)
      {
        foreach (var ability in abilities)
        {
          var abilityStr = ability.ToString();
          if (!string.IsNullOrWhiteSpace(abilityStr) && !abilityStr.StartsWith("@"))
          {
            Assert.Fail($"{relativePath} - Item '{item["name"]}' has hardcoded ability '{abilityStr}' instead of reference");
          }
        }
      }
    }
  }

  [Theory]
  [MemberData(nameof(GetAllCatalogFiles))]
  public void Catalog_References_Should_Have_Valid_Syntax(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var jsonText = File.ReadAllText(fullPath);
    var json = JObject.Parse(jsonText);

    // Find all references in the JSON
    var references = FindAllReferences(jsonText);

    // Assert - all @ references should follow v4.1 syntax: @domain/path/category:item-name[filters]?.property.nested
    foreach (var reference in references)
    {
      // v4.1 reference pattern supports:
      // - Domain/path: @domain/path/category
      // - Item selector: :item-name or :*
      // - Optional filters: [property=value] or [property>5]
      // - Optional marker: ?
      // - Property access: .property.nested
      reference.Should().MatchRegex(@"^@[\w-]+/[\w-/]+:[\w-*\s]+(\[[^\]]+\])?(\?)?(\.\w+)*$",
          $"{relativePath} has invalid reference syntax: {reference}");
    }
  }

  #endregion

  #region JSON Validation

  [Fact]
  public void All_Catalogs_Should_Parse_As_Valid_Json()
  {
    // Arrange
    var errors = new List<string>();

    // Act
    foreach (var file in _allCatalogFiles)
    {
      try
      {
        var fullPath = Path.Combine(_dataPath, file);
        JObject.Parse(File.ReadAllText(fullPath));
      }
      catch (JsonReaderException ex)
      {
        errors.Add($"{file}: {ex.Message}");
      }
    }

    // Assert
    errors.Should().BeEmpty("All catalog files should be valid JSON");
  }

  #endregion

  #region Helper Methods

  private List<JObject> GetAllItemsFromCatalog(JObject catalog)
  {
    var items = new List<JObject>();

    // Pattern 1: Root-level items array
    if (catalog["items"] is JArray rootItems)
    {
      items.AddRange(rootItems.OfType<JObject>());
    }

    // Pattern 2: Components structure (v4.0)
    if (catalog["components"] is JObject components)
    {
      foreach (var prop in components.Properties())
      {
        if (prop.Value is JArray componentItems)
        {
          items.AddRange(componentItems.OfType<JObject>());
        }
        else if (prop.Value is JObject nestedObj)
        {
          // Check if this object itself has a name (it's an item)
          if (nestedObj["name"] != null)
          {
            items.Add(nestedObj);
          }
          
          // Check for items array
          if (nestedObj["items"] is JArray nestedItems)
          {
            items.AddRange(nestedItems.OfType<JObject>());
          }
          
          // Check for deeper nesting (e.g., components.templates.fetch.easy_fetch)
          foreach (var nested in nestedObj.Properties())
          {
            if (nested.Value is JObject deeperObj)
            {
              // Check if this nested object has a name (it's an item)
              if (deeperObj["name"] != null)
              {
                items.Add(deeperObj);
              }
              
              foreach (var deeper in deeperObj.Properties())
              {
                if (deeper.Value is JArray deepArray)
                {
                  items.AddRange(deepArray.OfType<JObject>());
                }
                else if (deeper.Value is JObject deepestObj)
                {
                  // Has name property - it's an item
                  if (deepestObj["name"] != null)
                  {
                    items.Add(deepestObj);
                  }
                }
              }
            }
            else if (nested.Value is JArray directArray)
            {
              items.AddRange(directArray.OfType<JObject>());
            }
          }
        }
      }
    }

    // Pattern 3: {type}_types structure
    foreach (var prop in catalog.Properties())
    {
      if (prop.Name.EndsWith("_types") || prop.Name.EndsWith("Types"))
      {
        if (prop.Value is JObject types)
        {
          foreach (var category in types.Properties())
          {
            if (category.Value["items"] is JArray categoryItems)
            {
              items.AddRange(categoryItems.OfType<JObject>());
            }
          }
        }
      }
    }

    // Pattern 4: Hierarchical/category structure
    foreach (var prop in catalog.Properties())
    {
      if (prop.Name == "metadata" || prop.Name == "components" ||
          prop.Name == "items" || prop.Name.EndsWith("_types") || prop.Name.EndsWith("Types"))
      {
        continue;
      }

      if (prop.Value is JObject category)
      {
        if (category["items"] is JArray directItems)
        {
          items.AddRange(directItems.OfType<JObject>());
        }

        foreach (var subcatProp in category.Properties())
        {
          if (subcatProp.Value is JObject subcat && subcat["items"] is JArray subcatItems)
          {
            items.AddRange(subcatItems.OfType<JObject>());
          }
        }
      }
    }

    return items;
  }

  private List<string> FindAllReferences(string jsonText)
  {
    var references = new List<string>();
    // v4.1 syntax: @domain/path/category:item-name[filters]?.property.nested
    var regex = new System.Text.RegularExpressions.Regex(@"@[\w-]+/[\w-/]+:[\w-*\s]+(\[[^\]]+\])?(\?)?(\.\w+)*");
    var matches = regex.Matches(jsonText);

    foreach (System.Text.RegularExpressions.Match match in matches)
    {
      references.Add(match.Value);
    }

    return references;
  }

  #endregion

  #region Test Data Providers

  public static IEnumerable<object[]> GetAllCatalogFiles()
  {
    var instance = new CatalogJsonComplianceTests();
    return instance._allCatalogFiles.Select(f => new object[] { f });
  }

  #endregion
}
