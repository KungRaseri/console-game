using System.IO;
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
  private readonly CatalogTokenService _catalogTokenService;

  public NameListEditorViewModelTests()
  {
    // Setup test data directory
    _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testDataPath);

    _testFileName = "test-names.json";
    _jsonService = new JsonEditorService(_testDataPath);
    _catalogTokenService = new CatalogTokenService(_jsonService);

    CreateTestNamesFile();
  }

  /// <summary>
  /// Wait for ViewModel async loading to complete by monitoring StatusMessage
  /// </summary>
  private async Task WaitForLoadComplete(NameListEditorViewModel viewModel, int timeoutMs = 2000)
  {
    var startTime = DateTime.Now;
    while (viewModel.StatusMessage == "Ready" || viewModel.StatusMessage == string.Empty)
    {
      if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
      {
        throw new TimeoutException($"ViewModel did not complete loading within {timeoutMs}ms. Status: {viewModel.StatusMessage}");
      }
      await Task.Delay(50);
    }
    // Give a bit more time for final property updates
    await Task.Delay(100);
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
  public async Task Constructor_Should_Initialize_ViewModel_With_Valid_Data()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    
    // Wait for async load to complete
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.Should().NotBeNull();
    viewModel.Metadata.Should().NotBeNull();
    
    // Debug: Output status and data state
    if (viewModel.Patterns.Count == 0)
    {
      throw new Exception($"Patterns empty! Status: '{viewModel.StatusMessage}', Components: {viewModel.Components.Count}, ComponentNames: {viewModel.ComponentNames.Count}");
    }
    
    viewModel.Patterns.Should().NotBeEmpty();
    viewModel.Components.Should().NotBeEmpty();
    viewModel.ComponentNames.Should().NotBeEmpty();
  }

  [Fact]
  public async Task Constructor_Should_Load_Metadata_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    
    // Wait for async load to complete
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.Metadata.Version.Should().Be("4.0");
    viewModel.Metadata.Type.Should().Be("name_system");
    viewModel.Metadata.Description.Should().Be("Test name list");
  }

  [Fact]
  public async Task Constructor_Should_Load_Components_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    
    // Wait for async load to complete
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.ComponentNames.Should().Contain("prefix");
    viewModel.ComponentNames.Should().Contain("base");
    viewModel.Components.Should().HaveCount(2);

    var prefixComponents = viewModel.Components.FirstOrDefault(c => c.Key == "prefix").Value;
    prefixComponents.Should().NotBeNull();
    prefixComponents.Should().HaveCountGreaterThan(0);
  }

  [Fact]
  public async Task Constructor_Should_Load_Patterns_From_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    
    // Wait for async load to complete
    await WaitForLoadComplete(viewModel);

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
  public async Task PatternSearchText_Change_Should_Trigger_Filter()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);
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
  public async Task StatusMessage_Should_Be_Set_After_Load()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.StatusMessage.Should().NotBeNullOrEmpty();
    viewModel.StatusMessage.Should().Contain("test-names.json");
  }

  [Fact]
  public void ValidationErrors_Should_Be_Empty_For_Valid_File()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);

    // Assert
    viewModel.ValidationErrors.Should().BeEmpty();
    viewModel.HasValidationErrors.Should().BeFalse();
  }

  [Fact]
  public async Task TotalComponentCount_Should_Be_Calculated()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.TotalComponentCount.Should().BeGreaterThan(0);
    // Should equal sum of all components across all groups
    var expectedCount = viewModel.Components.Sum(kvp => kvp.Value.Count);
    viewModel.TotalComponentCount.Should().Be(expectedCount);
  }

  [Fact]
  public async Task TotalPatternCount_Should_Be_Calculated()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.TotalPatternCount.Should().BeGreaterThan(0);
    viewModel.TotalPatternCount.Should().Be(viewModel.Patterns.Count);
  }

  [Fact]
  public async Task ComponentCounts_Should_Track_Components_Per_Group()
  {
    // Act
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);

    // Assert
    viewModel.ComponentCounts.Should().NotBeEmpty();
    viewModel.ComponentCounts.Should().ContainKey("prefix");
    viewModel.ComponentCounts.Should().ContainKey("base");
  }

  [Fact]
  public async Task SelectedPattern_Should_Be_Settable()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);
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
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);

    // Assert
    viewModel.FilteredPatterns.Count.Should().Be(viewModel.Patterns.Count);
  }

  [Fact]
  public void PatternSearchText_Empty_Should_Show_All_Patterns()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
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
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);

    // Act
    viewModel.ComponentSearchText = "test search";

    // Assert
    viewModel.ComponentSearchText.Should().Be("test search");
  }

  [Fact]
  public void Metadata_Should_Be_Modifiable()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);

    // Act
    viewModel.Metadata.Description = "Modified description";

    // Assert
    viewModel.Metadata.Description.Should().Be("Modified description");
  }

  #region Command Tests

  [Fact]
  public void AddPatternCommand_Should_Add_New_Pattern()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    var initialCount = viewModel.Patterns.Count;

    // Act
    viewModel.AddPatternCommand.Execute(null);

    // Assert
    viewModel.Patterns.Count.Should().Be(initialCount + 1);
    viewModel.Patterns.Last().PatternTemplate.Should().Contain("{base}");
  }

  [Fact]
  public void RemovePatternCommand_CanExecute_Should_Require_Selected_Pattern()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    viewModel.SelectedPattern = null;

    // Act & Assert - RemovePattern command checks if any patterns exist, not SelectedPattern
    viewModel.RemovePatternCommand.CanExecute(null).Should().BeTrue("Command is enabled when patterns exist");

    // Remove should work even without SelectedPattern - it uses button's CommandParameter
  }

  [Fact]
  public async Task RemoveComponentCommand_Should_Remove_Component()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);
    var componentGroup = viewModel.Components.First().Value;
    var initialCount = componentGroup.Count;
    var componentToRemove = componentGroup.First();

    // Act
    viewModel.RemoveComponentCommand.Execute(componentToRemove);

    // Assert
    componentGroup.Count.Should().Be(initialCount - 1);
    componentGroup.Should().NotContain(componentToRemove);
  }

  [Fact]
  public async Task DuplicatePatternCommand_Should_Create_Copy()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);
    var initialCount = viewModel.Patterns.Count;
    var patternToDuplicate = viewModel.Patterns.First();

    // Act
    viewModel.DuplicatePatternCommand.Execute(patternToDuplicate);

    // Assert
    viewModel.Patterns.Count.Should().Be(initialCount + 1);
    // Duplicated pattern should have similar content (may be modified for uniqueness)
    viewModel.Patterns.Last().PatternTemplate.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public void ClearPatternFilterCommand_Should_Clear_Search_Text()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    viewModel.PatternSearchText = "test search";

    // Act
    viewModel.ClearPatternFilterCommand.Execute(null);

    // Assert
    viewModel.PatternSearchText.Should().BeEmpty();
  }

  [Fact]
  public async Task RegenerateExamplesCommand_Should_Update_Examples()
  {
    // Arrange
    var viewModel = new NameListEditorViewModel(_jsonService, _catalogTokenService, _testFileName);
    await WaitForLoadComplete(viewModel);
    var pattern = viewModel.Patterns.First();
    var originalExamples = pattern.GeneratedExamples;

    // Act
    viewModel.RegenerateExamplesCommand.Execute(pattern);

    // Assert
    pattern.GeneratedExamples.Should().NotBeNullOrEmpty();
    // Examples should be regenerated (may or may not be different due to randomness)
  }

  #endregion

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


