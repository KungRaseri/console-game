using System.IO;
using FluentAssertions;
using RealmForge.Models;
using RealmForge.Services;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RealmForge.Tests.Integration;

/// <summary>
/// Integration tests for new CRUD functionality
/// Tests file type detection, JsonEditorService JObject methods, and editor routing
/// </summary>
[Trait("Category", "Integration")]
public class CrudIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonService;

    public CrudIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "CrudIntegrationTests", Guid.NewGuid().ToString());
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

    #region JsonEditorService JObject Tests

    [Fact]
    public void LoadJObject_Should_Load_Valid_JSON_File()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["property1"] = "value1",
            ["property2"] = 42,
            ["nested"] = new JObject
            {
                ["inner"] = "data"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var result = _jsonService.LoadJObject(testFile);

        // Assert
        result.Should().NotBeNull();
        result!["property1"]!.ToString().Should().Be("value1");
        result["property2"]!.Value<int>().Should().Be(42);
        result["nested"]!["inner"]!.ToString().Should().Be("data");
    }

    [Fact]
    public void LoadJObject_Should_Return_Null_For_Missing_File()
    {
        // Act
        var result = _jsonService.LoadJObject("nonexistent.json");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void LoadJObject_Should_Throw_For_Invalid_JSON()
    {
        // Arrange
        var testFile = "invalid.json";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), "{ invalid json");

        // Act & Assert
        Action act = () => _jsonService.LoadJObject(testFile);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SaveJObject_Should_Write_Valid_JSON_To_File()
    {
        // Arrange
        var testFile = "output.json";
        var testData = new JObject
        {
            ["key1"] = "value1",
            ["key2"] = new JArray { 1, 2, 3 },
            ["key3"] = new JObject { ["nested"] = true }
        };

        // Act
        _jsonService.SaveJObject(testFile, testData);

        // Assert
        File.Exists(Path.Combine(_testDataPath, testFile)).Should().BeTrue();
        var loaded = JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        loaded["key1"]!.ToString().Should().Be("value1");
        loaded["key2"]!.Should().BeOfType<JArray>();
        loaded["key2"]!.Count().Should().Be(3);
    }

    [Fact]
    public void SaveJObject_Should_Create_Backup_Of_Existing_File()
    {
        // Arrange
        var testFile = "backup-test.json";
        var originalData = new JObject { ["version"] = "1.0" };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), originalData.ToString());

        var newData = new JObject { ["version"] = "2.0" };

        // Act
        _jsonService.SaveJObject(testFile, newData);

        // Assert
        var backupDir = Path.Combine(_testDataPath, "backups");
        Directory.Exists(backupDir).Should().BeTrue();
        var backupFiles = Directory.GetFiles(backupDir, "backup-test_*.json");
        backupFiles.Should().NotBeEmpty("Backup should have been created");
    }

    [Fact]
    public void SaveJObject_Should_Preserve_Formatting()
    {
        // Arrange
        var testFile = "formatted.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject { ["item1"] = "value1" }
        };

        // Act
        _jsonService.SaveJObject(testFile, testData);

        // Assert
        var fileContent = File.ReadAllText(Path.Combine(_testDataPath, testFile));
        fileContent.Should().Contain("\n"); // Should be indented/formatted
        fileContent.Should().Contain("  "); // Should have indentation
    }

    [Fact]
    public void LoadJObject_And_SaveJObject_Should_Round_Trip_Data()
    {
        // Arrange
        var testFile = "roundtrip.json";
        var originalData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["notes"] = new JArray { "note1", "note2" }
            },
            ["components"] = new JObject
            {
                ["red"] = "#FF0000",
                ["green"] = "#00FF00"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), originalData.ToString());

        // Act
        var loaded = _jsonService.LoadJObject(testFile);
        loaded.Should().NotBeNull();

        // Modify slightly
        loaded!["components"]!["blue"] = "#0000FF";

        _jsonService.SaveJObject(testFile, loaded);
        var reloaded = _jsonService.LoadJObject(testFile);

        // Assert
        reloaded.Should().NotBeNull();
        reloaded!["metadata"]!["version"]!.ToString().Should().Be("4.0");
        reloaded["components"]!["red"]!.ToString().Should().Be("#FF0000");
        reloaded["components"]!["blue"]!.ToString().Should().Be("#0000FF");
    }

    #endregion

    #region FileTypeDetector Tests

    [Fact]
    public void DetectFileType_Should_Recognize_Config_Files()
    {
        // Arrange
        var configPath = Path.Combine("enemies", "dragons", ".cbconfig.json");

        // Act
        var fileType = FileTypeDetector.DetectFileType(configPath);

        // Assert
        fileType.Should().Be(JsonFileType.ConfigFile);
    }

    [Fact]
    public void DetectFileType_Should_Recognize_Component_Files()
    {
        // Arrange - Various component file types
        var testFiles = new[]
        {
            "enemies/dragons/colors.json",
            "npcs/traits.json",
            "quests/objectives.json",
            "items/weapons/traits.json",
            "general/rarity_config.json"
        };

        foreach (var testFile in testFiles)
        {
            // Act
            var fileType = FileTypeDetector.DetectFileType(testFile);

            // Assert
            fileType.Should().Be(JsonFileType.ComponentData,
                $"{testFile} should be detected as ComponentData");
        }
    }

    [Fact]
    public void DetectFileType_Should_Still_Recognize_Catalog_Files()
    {
        // Arrange
        var testFiles = new[]
        {
            "enemies/catalog.json",
            "items/weapons/catalog.json",
            "abilities/offensive/catalog.json"
        };

        foreach (var testFile in testFiles)
        {
            // Act
            var fileType = FileTypeDetector.DetectFileType(testFile);

            // Assert
            fileType.Should().Be(JsonFileType.GenericCatalog,
                $"{testFile} should be detected as GenericCatalog");
        }
    }

    [Fact]
    public void DetectFileType_Should_Still_Recognize_Names_Files()
    {
        // Arrange
        var testFiles = new[]
        {
            "enemies/names.json",
            "items/weapons/names.json",
            "abilities/offensive/names.json"
        };

        foreach (var testFile in testFiles)
        {
            // Act
            var fileType = FileTypeDetector.DetectFileType(testFile);

            // Assert
            fileType.Should().Be(JsonFileType.NamesFile,
                $"{testFile} should be detected as NamesFile");
        }
    }

    [Fact]
    public void GetEditorType_Should_Return_ConfigEditor_For_Config_Files()
    {
        // Arrange
        var configPath = "enemies/.cbconfig.json";

        // Act
        var editorType = FileTypeDetector.GetEditorType(configPath);

        // Assert
        editorType.Should().Be(EditorType.ConfigEditor);
    }

    [Fact]
    public void GetEditorType_Should_Return_ComponentDataEditor_For_Component_Files()
    {
        // Arrange
        var testFiles = new[]
        {
            "enemies/dragons/colors.json",
            "npcs/traits.json",
            "quests/objectives.json"
        };

        foreach (var testFile in testFiles)
        {
            // Act
            var editorType = FileTypeDetector.GetEditorType(testFile);

            // Assert
            editorType.Should().Be(EditorType.ComponentDataEditor,
                $"{testFile} should use ComponentDataEditor");
        }
    }

    [Fact]
    public void GetEditorType_Should_Return_CatalogEditor_For_Catalog_Files()
    {
        // Arrange
        var catalogPath = "enemies/catalog.json";

        // Act
        var editorType = FileTypeDetector.GetEditorType(catalogPath);

        // Assert
        editorType.Should().Be(EditorType.CatalogEditor);
    }

    [Fact]
    public void GetEditorType_Should_Return_NameListEditor_For_Names_Files()
    {
        // Arrange
        var namesPath = "enemies/names.json";

        // Act
        var editorType = FileTypeDetector.GetEditorType(namesPath);

        // Assert
        editorType.Should().Be(EditorType.NameListEditor);
    }

    #endregion

    #region End-to-End Component File Workflow

    [Fact]
    public void EndToEnd_Should_Load_Edit_And_Save_Component_File()
    {
        // Arrange - Create a component file
        var testFile = "colors.json";
        var originalData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "4.0",
                ["type"] = "component_data"
            },
            ["components"] = new JObject
            {
                ["red"] = "#FF0000",
                ["green"] = "#00FF00"
            }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), originalData.ToString());

        // Act - Load
        var loaded = _jsonService.LoadJObject(testFile);
        loaded.Should().NotBeNull();

        // Act - Edit
        loaded!["components"]!["blue"] = "#0000FF";
        loaded["components"]!["red"] = "#FF00FF"; // Modify existing

        // Act - Save
        _jsonService.SaveJObject(testFile, loaded);

        // Assert - Verify changes persisted
        var reloaded = _jsonService.LoadJObject(testFile);
        reloaded.Should().NotBeNull();
        reloaded!["components"]!["blue"]!.ToString().Should().Be("#0000FF");
        reloaded["components"]!["red"]!.ToString().Should().Be("#FF00FF");
        reloaded["components"]!["green"]!.ToString().Should().Be("#00FF00");
        reloaded["metadata"]!["version"]!.ToString().Should().Be("4.0");
    }

    [Fact]
    public void EndToEnd_Should_Load_Edit_And_Save_Config_File()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var originalData = new JObject
        {
            ["displayName"] = "Original Name",
            ["icon"] = "Folder",
            ["sortOrder"] = 5
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), originalData.ToString());

        // Act - Load
        var loaded = _jsonService.LoadJObject(testFile);
        loaded.Should().NotBeNull();

        // Act - Edit
        loaded!["displayName"] = "Updated Name";
        loaded["sortOrder"] = 10;
        loaded["description"] = "New description";

        // Act - Save
        _jsonService.SaveJObject(testFile, loaded);

        // Assert
        var reloaded = _jsonService.LoadJObject(testFile);
        reloaded.Should().NotBeNull();
        reloaded!["displayName"]!.ToString().Should().Be("Updated Name");
        reloaded["sortOrder"]!.Value<int>().Should().Be(10);
        reloaded["description"]!.ToString().Should().Be("New description");
    }

    #endregion

    #region Error Handling

    [Fact]
    public void LoadJObject_Should_Handle_Concurrent_Access_Gracefully()
    {
        // Arrange
        var testFile = "concurrent.json";
        var testData = new JObject { ["data"] = "test" };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act - Load multiple times
        var result1 = _jsonService.LoadJObject(testFile);
        var result2 = _jsonService.LoadJObject(testFile);
        var result3 = _jsonService.LoadJObject(testFile);

        // Assert - All should succeed
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
    }

    [Fact]
    public void SaveJObject_Should_Handle_Large_JSON_Files()
    {
        // Arrange - Create a large JSON object
        var testFile = "large.json";
        var largeData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject()
        };

        // Add 1000 items
        for (int i = 0; i < 1000; i++)
        {
            largeData["components"]![$"item{i}"] = $"value{i}";
        }

        // Act
        _jsonService.SaveJObject(testFile, largeData);

        // Assert
        var reloaded = _jsonService.LoadJObject(testFile);
        reloaded.Should().NotBeNull();
        reloaded!["components"]!.Count().Should().Be(1000);
    }

    #endregion
}
