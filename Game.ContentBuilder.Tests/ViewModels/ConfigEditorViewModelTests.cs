using System;
using System.IO;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Unit tests for ConfigEditorViewModel
/// Tests loading, editing, and saving .cbconfig.json files
/// </summary>
[Trait("Category", "ViewModel")]
public class ConfigEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonService;

    public ConfigEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ConfigEditorTests", Guid.NewGuid().ToString());
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
    public void Constructor_Should_Load_Complete_Config_File()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test Folder",
            ["icon"] = "Folder",
            ["sortOrder"] = 10,
            ["description"] = "Test description",
            ["defaultFileIcon"] = "File"
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.FileName.Should().Be(".cbconfig");
        viewModel.DisplayName.Should().Be("Test Folder");
        viewModel.Icon.Should().Be("Folder");
        viewModel.SortOrder.Should().Be(10);
        viewModel.Description.Should().Be("Test description");
        viewModel.DefaultFileIcon.Should().Be("File");
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_Load_Config_With_Only_Required_Fields()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Minimal Config",
            ["icon"] = "Star",
            ["sortOrder"] = 5
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.DisplayName.Should().Be("Minimal Config");
        viewModel.Icon.Should().Be("Star");
        viewModel.SortOrder.Should().Be(5);
        viewModel.Description.Should().BeEmpty();
        viewModel.DefaultFileIcon.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_Should_Handle_Missing_Optional_Fields()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Check",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.Description.Should().BeEmpty();
        viewModel.DefaultFileIcon.Should().BeEmpty();
    }

    #endregion

    #region CRUD Operations Tests

    [Fact]
    public void Save_Should_Persist_All_Fields_To_File()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Original",
            ["icon"] = "OldIcon",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Modify fields
        viewModel.DisplayName = "Updated Name";
        viewModel.Icon = "NewIcon";
        viewModel.SortOrder = 20;
        viewModel.Description = "New description";
        viewModel.DefaultFileIcon = "Document";

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
        var savedData = JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        savedData["displayName"]!.ToString().Should().Be("Updated Name");
        savedData["icon"]!.ToString().Should().Be("NewIcon");
        savedData["sortOrder"]!.Value<int>().Should().Be(20);
        savedData["description"]!.ToString().Should().Be("New description");
        savedData["defaultFileIcon"]!.ToString().Should().Be("Document");
    }

    [Fact]
    public void Save_Should_Omit_Empty_Optional_Fields()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Don't set optional fields (leave them empty)

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var savedData = JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        savedData.Should().NotContainKey("description");
        savedData.Should().NotContainKey("defaultFileIcon");
        savedData.Should().ContainKey("displayName");
        savedData.Should().ContainKey("icon");
        savedData.Should().ContainKey("sortOrder");
    }

    [Fact]
    public void Refresh_Should_Reload_Data_From_File()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Original",
            ["icon"] = "Original",
            ["sortOrder"] = 5
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Modify in memory
        viewModel.DisplayName = "Modified";
        viewModel.Icon = "Modified";
        viewModel.IsDirty.Should().BeTrue();

        // Act
        viewModel.RefreshCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
        viewModel.DisplayName.Should().Be("Original");
        viewModel.Icon.Should().Be("Original");
    }

    #endregion

    #region Change Tracking Tests

    [Fact]
    public void IsDirty_Should_Be_False_After_Initial_Load()
    {
        // Arrange & Act
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_DisplayName_Change()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Original",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.DisplayName = "Changed";

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_Icon_Change()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Original",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.Icon = "Changed";

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_SortOrder_Change()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.SortOrder = 99;

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_Description_Change()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.Description = "New description";

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_Should_Be_True_After_DefaultFileIcon_Change()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Act
        viewModel.DefaultFileIcon = "File";

        // Assert
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void IsDirty_Should_Be_False_After_Save()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);
        viewModel.DisplayName = "Changed";
        viewModel.IsDirty.Should().BeTrue();

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void RawJsonPreview_Should_Update_When_Data_Changes()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Original",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);
        var initialPreview = viewModel.RawJsonPreview;

        // Act
        viewModel.DisplayName = "Updated";

        // Assert
        viewModel.RawJsonPreview.Should().NotBe(initialPreview);
        viewModel.RawJsonPreview.Should().Contain("Updated");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_Should_Handle_Empty_File()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject();
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.DisplayName.Should().BeEmpty();
        viewModel.Icon.Should().BeEmpty();
        viewModel.SortOrder.Should().Be(0);
    }

    [Fact]
    public void Save_Should_Create_Valid_JSON_Structure()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Test",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        viewModel.DisplayName = "New Name";
        viewModel.Icon = "NewIcon";
        viewModel.SortOrder = 15;

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert - Should be parseable
        Action act = () => JObject.Parse(File.ReadAllText(Path.Combine(_testDataPath, testFile)));
        act.Should().NotThrow();
    }

    #endregion
}
