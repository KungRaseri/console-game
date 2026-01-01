using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Data.Tests;

[Trait("Category", "Compliance")]
/// <summary>
/// Validation tests for component/data JSON files (non-catalog, non-names, non-config files)
/// Ensures all component files are valid JSON and contain meaningful data
/// </summary>
[Trait("Category", "DataValidation")]
[Trait("FileType", "Component")]
public class ComponentJsonComplianceTests
{
  private readonly string _dataPath;
  private readonly List<string> _allComponentFiles;

  public ComponentJsonComplianceTests()
  {
    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
    if (solutionRoot == null)
      throw new DirectoryNotFoundException($"Could not find solution root from: {baseDir}");

    _dataPath = Path.Combine(solutionRoot, "RealmEngine.Data", "Data", "Json");

    if (!Directory.Exists(_dataPath))
      throw new DirectoryNotFoundException($"Data directory not found: {_dataPath}");

    // Discover component/data files (all other .json files)
    var allJsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories)
        .Select(f => Path.GetRelativePath(_dataPath, f))
        .ToList();

    _allComponentFiles = allJsonFiles
        .Where(f => !f.EndsWith("catalog.json") &&
                    !f.EndsWith("names.json") &&
                    !f.EndsWith(".cbconfig.json"))
        .ToList();
  }

  #region Discovery Tests

  [Fact]
  public void Should_Discover_All_Component_Files()
  {
    // Assert - should have found component files
    _allComponentFiles.Should().NotBeEmpty();
  }

  #endregion

  #region JSON Validation

  [Theory]
  [MemberData(nameof(GetAllComponentFiles))]
  public void Component_Should_Be_Valid_Json(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);

    // Act & Assert
    var act = () => JToken.Parse(File.ReadAllText(fullPath));
    act.Should().NotThrow<JsonReaderException>($"{relativePath} should be valid JSON");
  }

  [Fact]
  public void All_Component_Files_Should_Parse_As_Valid_Json()
  {
    // Arrange
    var errors = new List<string>();

    // Act
    foreach (var file in _allComponentFiles)
    {
      try
      {
        var fullPath = Path.Combine(_dataPath, file);
        JToken.Parse(File.ReadAllText(fullPath));
      }
      catch (JsonReaderException ex)
      {
        errors.Add($"{file}: {ex.Message}");
      }
    }

    // Assert
    errors.Should().BeEmpty("All component files should be valid JSON");
  }

  #endregion

  #region Content Validation

  [Theory]
  [MemberData(nameof(GetAllComponentFiles))]
  public void Component_Should_Not_Be_Empty(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JToken.Parse(File.ReadAllText(fullPath));

    // Assert
    json.Should().NotBeNull($"{relativePath} should not be null");

    if (json is JObject obj)
    {
      obj.Should().NotBeEmpty($"{relativePath} should not be empty object");

      // For config-style files, check that nested settings exist
      if (obj["settings"] is JObject settings)
      {
        settings.Should().NotBeEmpty($"{relativePath} settings should not be empty");
      }
    }
    else if (json is JArray arr)
    {
      arr.Should().NotBeEmpty($"{relativePath} should not be empty array");
    }
  }

  [Theory]
  [MemberData(nameof(GetAllComponentFiles))]
  public void Component_Should_Have_Meaningful_Content(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JToken.Parse(File.ReadAllText(fullPath));

    // Act - check that file has actual data (not just empty structures)
    var hasContent = false;

    if (json is JObject obj)
    {
      // Check for settings object (config files)
      if (obj["settings"] is JObject settings && settings.Properties().Any())
      {
        hasContent = true;
      }
      // Check for any non-empty arrays or objects with properties
      else
      {
        foreach (var property in obj.Properties())
        {
          if (property.Value is JArray arr && arr.Count > 0)
          {
            hasContent = true;
            break;
          }
          else if (property.Value is JObject innerObj && innerObj.Properties().Any())
          {
            hasContent = true;
            break;
          }
        }
      }
    }
    else if (json is JArray arr)
    {
      hasContent = arr.Count > 0;
    }

    // Assert
    hasContent.Should().BeTrue($"{relativePath} should contain meaningful data (non-empty arrays or objects)");
  }

  #endregion

  #region Metadata Validation (Optional)

  [Theory]
  [MemberData(nameof(GetAllComponentFiles))]
  public void Component_With_Metadata_Should_Have_Valid_Fields(string relativePath)
  {
    // Arrange
    var fullPath = Path.Combine(_dataPath, relativePath);
    var json = JObject.Parse(File.ReadAllText(fullPath));
    var metadata = json["metadata"] as JObject;

    // Skip if no metadata section
    if (metadata == null)
      return;

    // Assert - if metadata exists, validate required fields
    metadata["version"].Should().NotBeNull($"{relativePath} metadata missing version");
    metadata["lastUpdated"].Should().NotBeNull($"{relativePath} metadata missing lastUpdated");

    var version = metadata["version"]?.ToString();
    if (!string.IsNullOrEmpty(version))
    {
      version.Should().Be("4.0", $"{relativePath} should use version 4.0");
    }

    var lastUpdated = metadata["lastUpdated"]?.ToString();
    if (!string.IsNullOrEmpty(lastUpdated))
    {
      DateTime.TryParse(lastUpdated, out _).Should().BeTrue($"{relativePath} lastUpdated should be valid date");
    }
  }

  #endregion

  #region Test Data Providers

  public static IEnumerable<object[]> GetAllComponentFiles()
  {
    var instance = new ComponentJsonComplianceTests();
    return instance._allComponentFiles.Select(f => new object[] { f });
  }

  #endregion
}
