using System.IO;
using FluentAssertions;
using RealmForge.Services;
using RealmForge.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RealmForge.Tests.ViewModels;

/// <summary>
/// Unit tests for ComponentDataEditorViewModel
/// Tests loading, editing, and saving component/data files (colors.json, traits.json, etc.)
/// </summary>
[Trait("Category", "ViewModel")]
public class ComponentDataEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonService;

    public ComponentDataEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ComponentEditorTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonService = new JsonEditorService(_testDataPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, recursive: true);
        }
    }

    #region Constructor and Loading Tests

    [Fact]
    public void Constructor_Should_Load_File_With_Components_Object()
    {
        // Arrange
        var testFile = "colors.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["type"] = "component_data",
                ["description"] = "Test colors"
            },
            ["components"] = new JObject
            {
                ["red"] = "#FF0000",
                ["green"] = "#00FF00",
                ["blue"] = "#0000FF"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.FileName.Should().Be("colors");
        viewModel.HasMetadata.Should().BeTrue();
        viewModel.MetadataVersion.Should().Be("4.0");
        viewModel.MetadataType.Should().Be("component_data");
        viewModel.MetadataDescription.Should().Be("Test colors");
        viewModel.Items.Should().HaveCount(3);
        viewModel.Items.Should().Contain(i => i.Key == "red" && i.Value == "\"#FF0000\"");
    }

    [Fact]
    public void Constructor_Should_Load_File_With_Settings_Object()
    {
        // Arrange
        var testFile = "config.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["description"] = "Test config"
            },
            ["settings"] = new JObject
            {
                ["maxItems"] = 100,
                ["enabled"] = true,
                ["theme"] = "dark"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.HasMetadata.Should().BeTrue();
        viewModel.Items.Should().HaveCount(3);
        viewModel.Items.Should().Contain(i => i.Key == "maxItems" && i.Value == "100");
        viewModel.Items.Should().Contain(i => i.Key == "enabled" && i.Value == "true");
    }

    [Fact]
    public void Constructor_Should_Load_File_With_Root_Level_Properties()
    {
        // Arrange
        var testFile = "simple.json";
        var testData = new JObject
        {
            ["property1"] = "value1",
            ["property2"] = 42,
            ["property3"] = true
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.HasMetadata.Should().BeFalse();
        viewModel.Items.Should().HaveCount(3);
        viewModel.Items.Should().Contain(i => i.Key == "property1");
    }

    [Fact]
    public void Constructor_Should_Handle_Nested_Objects()
    {
        // Arrange
        var testFile = "nested.json";
        var testData = new JObject
        {
            ["components"] = new JObject
            {
                ["item1"] = new JObject
                {
                    ["name"] = "Test",
                    ["value"] = 123
                },
                ["item2"] = new JArray { "a", "b", "c" }
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.Items.Should().HaveCount(2);
        var item1 = viewModel.Items.FirstOrDefault(i => i.Key == "item1");
        item1.Should().NotBeNull();
        item1!.Value.Should().Contain("Test");
        item1.Value.Should().Contain("123");
    }

    [Fact]
    public void Constructor_Should_Load_Metadata_Notes_As_Array()
    {
        // Arrange
        var testFile = "notes.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["notes"] = new JArray { "Note 1", "Note 2", "Note 3" }
            },
            ["components"] = new JObject { ["test"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.MetadataNotes.Should().HaveCount(3);
        viewModel.MetadataNotes.Should().Contain("Note 1");
        viewModel.MetadataNotesText.Should().Contain("Note 1");
        viewModel.MetadataNotesText.Should().Contain("Note 2");
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public void AddItem_Should_Add_New_Item_To_Collection()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["components"] = new JObject { ["existing"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount + 1);
        viewModel.Items.Last().Key.Should().Be("newKey");
        viewModel.Items.Last().Value.Should().Be("{}"); // New items start as empty objects
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void DeleteItem_Should_Remove_Selected_Item()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["components"] = new JObject
            {
                ["item1"] = "value1",
                ["item2"] = "value2",
                ["item3"] = "value3"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        viewModel.SelectedItem = viewModel.Items[1]; // Select "item2"

        // Act
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(2);
        viewModel.Items.Should().NotContain(i => i.Key == "item2");
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void DeleteItem_Should_Do_Nothing_When_No_Item_Selected()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["components"] = new JObject { ["item1"] = "value1" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        var initialCount = viewModel.Items.Count;
        viewModel.SelectedItem = null;

        // Act
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount);
    }

    [Fact]
    public void Save_Should_Persist_Changes_To_File()
    {
        // Arrange
        var testFile = "save-test.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject { ["original"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Modify data
        viewModel.AddItemCommand.Execute(null);
        viewModel.Items.Last().Key = "newItem";
        viewModel.Items.Last().Value = "\"newValue\"";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
        var savedData = JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        savedData["components"]!["newItem"]!.ToString().Should().Be("newValue");
    }

    [Fact]
    public void Save_Should_Preserve_Metadata()
    {
        // Arrange
        var testFile = "metadata-test.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["type"] = "component_data",
                ["description"] = "Original description",
                ["notes"] = new JArray { "Note 1", "Note 2" }
            },
            ["components"] = new JObject { ["item"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Modify metadata
        viewModel.MetadataDescription = "Updated description";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var savedData = JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        savedData["metadata"]!["description"]!.ToString().Should().Be("Updated description");
        savedData["metadata"]!["version"]!.ToString().Should().Be("4.0");
        savedData["metadata"]!["notes"]!.Should().NotBeNull();
    }

    [Fact]
    public void Refresh_Should_Reload_Data_From_File()
    {
        // Arrange
        var testFile = "refresh-test.json";
        var testData = new JObject
        {
            ["components"] = new JObject { ["item1"] = "original" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Modify by adding a new item (which sets IsDirty)
        viewModel.AddItemCommand.Execute(null);
        viewModel.IsDirty.Should().BeTrue();
        var itemCountAfterAdd = viewModel.Items.Count;

        // Act
        viewModel.RefreshCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
        viewModel.Items.Count.Should().Be(1, "should reload original data, discarding the added item");
        viewModel.Items[0].Value.Should().Be("\"original\"");
    }

    #endregion

    #region Change Tracking Tests

    [Fact]
    public void IsDirty_Should_Be_False_After_Initial_Load()
    {
        // Arrange & Act
        var testFile = "clean.json";
        var testData = new JObject { ["components"] = new JObject { ["item"] = "value" } };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_Metadata_Change()
    {
        // Arrange
        var testFile = "metadata-change.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject { ["item"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.MetadataVersion = "4.1";

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void RawJsonPreview_Should_Update_When_Data_Changes()
    {
        // Arrange
        var testFile = "preview.json";
        var testData = new JObject { ["components"] = new JObject { ["item"] = "value" } };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        var initialPreview = viewModel.RawJsonPreview;

        // Act
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.RawJsonPreview.Should().NotBe(initialPreview);
        viewModel.RawJsonPreview.Should().Contain("newKey");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Constructor_Should_Handle_Empty_File()
    {
        // Arrange
        var testFile = "empty.json";
        var testData = new JObject();
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.Items.Should().BeEmpty();
        viewModel.HasMetadata.Should().BeFalse();
    }

    [Fact]
    public void Save_Should_Handle_Parse_Errors_In_Item_Values()
    {
        // Arrange
        var testFile = "parse-error.json";
        var testData = new JObject { ["components"] = new JObject { ["item"] = "value" } };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Set invalid JSON
        viewModel.Items[0].Value = "invalid json {";

        // Act & Assert - should not throw, should handle gracefully
        Action act = () => viewModel.SaveCommand.Execute(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void MetadataNotesText_Should_Handle_Multiline_Input()
    {
        // Arrange
        var testFile = "notes-multiline.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject()
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.MetadataNotesText = "Line 1\r\nLine 2\nLine 3";

        // Assert
        viewModel.MetadataNotes.Should().HaveCount(3);
        viewModel.MetadataNotes[0].Should().Be("Line 1");
        viewModel.MetadataNotes[1].Should().Be("Line 2");
        viewModel.MetadataNotes[2].Should().Be("Line 3");
    }

    #endregion
}
