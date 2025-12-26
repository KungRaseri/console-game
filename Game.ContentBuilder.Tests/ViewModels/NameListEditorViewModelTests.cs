using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Comprehensive unit tests for NameListEditorViewModel
/// Tests pattern and component management operations
/// </summary>
[Trait("Category", "ViewModel")]
public class NameListEditorViewModelTests : IDisposable
{
  private readonly string _testDataPath;
  private readonly string _testFileName;
  private readonly JsonEditorService _jsonService;

  public NameListEditorViewModelTests()
  {
    // Setup test data directory
    _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testDataPath);

    _testFileName = "test-names.json";
    _jsonService = new JsonEditorService(_testDataPath);

    CreateTestNamesFile();
  }

  private void CreateTestNamesFile()
  {
    var testData = new JObject
    {
      ["metadata"] = new JObject
      {
        ["version"] = "4.0",
        ["type"] = "name_system",
        ["description"] = "Test name list",
        ["lastUpdated"] = "2025-12-26"
      },
      ["components"] = new JObject
      {
        ["prefix"] = new JArray
                {
                    new JObject { ["value"] = "Dark", ["rarityWeight"] = 10 },
                    new JObject { ["value"] = "Light", ["rarityWeight"] = 5 }
                },
        ["base"] = new JArray
                {
                    new JObject { ["value"] = "Sword", ["rarityWeight"] = 10 },
                    new JObject { ["value"] = "Blade", ["rarityWeight"] = 5 }
                }
      },
      ["patterns"] = new JArray
            {
                new JObject
                {
                    ["template"] = "{prefix} {base}",
                    ["weight"] = 100,
                    ["description"] = "Standard pattern"
                },
                new JObject
                {
                    ["template"] = "{base}",
                    ["weight"] = 50,
                    ["description"] = "Simple pattern"
                }
            }
    };

    File.WriteAllText(
        Path.Combine(_testDataPath, _testFileName),
        testData.ToString(Newtonsoft.Json.Formatting.Indented));
  }

  [Fact]
  public void Constructor_Should_Initialize_ViewModel_With_Valid_Data()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.Should().NotBeNull();
    viewModel.Metadata.Should().NotBeNull();
    viewModel.Patterns.Should().NotBeEmpty();
    viewModel.Components.Should().NotBeEmpty();
    viewModel.ComponentNames.Should().NotBeEmpty();
  }

  [Fact]
  public void Constructor_Should_Load_Metadata_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.Metadata.Version.Should().Be("4.0");
    viewModel.Metadata.Type.Should().Be("name_system");
    viewModel.Metadata.Description.Should().Be("Test name list");
  }

  [Fact]
  public void Constructor_Should_Load_Components_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.ComponentNames.Should().Contain("prefix");
    viewModel.ComponentNames.Should().Contain("base");
    viewModel.Components.Should().HaveCount(2);

    var prefixComponents = viewModel.Components.FirstOrDefault(c => c.Key == "prefix").Value;
    prefixComponents.Should().NotBeNull();
    prefixComponents.Should().HaveCountGreaterThan(0);
  }

  [Fact]
  public void Constructor_Should_Load_Patterns_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    // Patterns include loaded patterns plus default {base} pattern
        viewModel.Patterns.Should().HaveCountGreaterThanOrEqualTo(2);

    // Assert
    var basePattern = viewModel.Patterns.FirstOrDefault(p => p.PatternTemplate == "{base}");
    basePattern.Should().NotBeNull();
    basePattern!.IsReadOnly.Should().BeTrue("Default base pattern should be readonly");
    basePattern.Weight.Should().Be(100);
  }



  [Fact]
  public void PatternSearchText_Change_Should_Trigger_Filter()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);
    var initialFilteredCount = viewModel.FilteredPatterns.Count;

    // Act
    viewModel.PatternSearchText = "prefix";

    // Assert
    viewModel.FilteredPatterns.Should().NotBeEmpty();
    // Filtered results should contain patterns matching the search
    viewModel.FilteredPatterns.Should().OnlyContain(p =>
        p.PatternTemplate.Contains("prefix", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public void StatusMessage_Should_Be_Set_After_Load()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.StatusMessage.Should().NotBeNullOrEmpty();
    viewModel.StatusMessage.Should().Contain("names.json");
  }

  [Fact]
  public void ValidationErrors_Should_Be_Empty_For_Valid_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.ValidationErrors.Should().BeEmpty();
    viewModel.HasValidationErrors.Should().BeFalse();
  }

  [Fact]
  public void TotalComponentCount_Should_Be_Calculated()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.TotalComponentCount.Should().BeGreaterThan(0);
    // Should equal sum of all components across all groups
    var expectedCount = viewModel.Components.Sum(kvp => kvp.Value.Count);
    viewModel.TotalComponentCount.Should().Be(expectedCount);
  }

  [Fact]
  public void TotalPatternCount_Should_Be_Calculated()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.TotalPatternCount.Should().BeGreaterThan(0);
    viewModel.TotalPatternCount.Should().Be(viewModel.Patterns.Count);
  }

  [Fact]
  public void ComponentCounts_Should_Track_Components_Per_Group()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.ComponentCounts.Should().NotBeEmpty();
    viewModel.ComponentCounts.Should().ContainKey("prefix");
    viewModel.ComponentCounts.Should().ContainKey("base");
  }

  [Fact]
  public void SelectedPattern_Should_Be_Settable()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);
    var pattern = viewModel.Patterns.First();

    // Act
    viewModel.SelectedPattern = pattern;

    // Assert
    viewModel.SelectedPattern.Should().Be(pattern);
  }

  [Fact]
  public void FilteredPatterns_Should_Initially_Contain_All_Patterns()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.FilteredPatterns.Count.Should().Be(viewModel.Patterns.Count);
  }

  [Fact]
  public void PatternSearchText_Empty_Should_Show_All_Patterns()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);
    viewModel.PatternSearchText = "test";

    // Act
    viewModel.PatternSearchText = string.Empty;

    // Assert
    viewModel.FilteredPatterns.Count.Should().Be(viewModel.Patterns.Count);
  }

  [Fact]
  public void ComponentSearchText_Should_Be_Settable()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Act
    viewModel.ComponentSearchText = "test search";

    // Assert
    viewModel.ComponentSearchText.Should().Be("test search");
  }

  [Fact]
  public void Metadata_Should_Be_Modifiable()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _testFileName);

    // Act
    viewModel.Metadata.Description = "Modified description";

    // Assert
    viewModel.Metadata.Description.Should().Be("Modified description");
  }

  public void Dispose()
  {
    if (Directory.Exists(_testDataPath))
    {
      try
      {
        Directory.Delete(_testDataPath, true);
      }
      catch
      {
        // Ignore cleanup errors
      }
    }
  }
}
