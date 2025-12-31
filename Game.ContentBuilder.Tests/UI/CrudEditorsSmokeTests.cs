using System.IO;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Views;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// UI smoke tests for new CRUD editors
/// Tests that ComponentDataEditor and ConfigEditor can be instantiated
/// </summary>
[Trait("Category", "UI")]
public class CrudEditorsSmokeTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonService;

    public CrudEditorsSmokeTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "CrudEditorsSmokeTests", Guid.NewGuid().ToString());
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

    #region ComponentDataEditorView Smoke Tests

    [Fact]
    [Trait("Smoke", "ComponentDataEditor")]
    public void ComponentDataEditorView_Should_Instantiate_Without_Errors()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["components"] = new JObject { ["test"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        Action act = () =>
        {
            var view = new ComponentDataEditorView
            {
                DataContext = viewModel
            };
        };

        // Assert
        act.Should().NotThrow("ComponentDataEditorView should instantiate without errors");
    }

    [Fact]
    [Trait("Smoke", "ComponentDataEditor")]
    public void ComponentDataEditorViewModel_Should_Have_All_Required_Commands()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject { ["components"] = new JObject() };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.AddItemCommand.Should().NotBeNull("Should have AddItemCommand");
        viewModel.DeleteItemCommand.Should().NotBeNull("Should have DeleteItemCommand");
        viewModel.SaveCommand.Should().NotBeNull("Should have SaveCommand");
        viewModel.RefreshCommand.Should().NotBeNull("Should have RefreshCommand");
    }

    [Fact]
    [Trait("Smoke", "ComponentDataEditor")]
    public void ComponentDataEditorViewModel_Should_Have_Required_Properties()
    {
        // Arrange
        var testFile = "test.json";
        var testData = new JObject
        {
            ["metadata"] = new JObject { ["version"] = "4.0" },
            ["components"] = new JObject { ["item"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.FileName.Should().NotBeNullOrEmpty("Should have FileName");
        viewModel.Items.Should().NotBeNull("Should have Items collection");
        viewModel.RawJsonPreview.Should().NotBeNullOrEmpty("Should have RawJsonPreview");
        viewModel.IsDirty.Should().BeFalse("IsDirty should start false");
    }

    #endregion

    #region ConfigEditorView Smoke Tests

    [Fact]
    [Trait("Smoke", "ConfigEditor")]
    public void ConfigEditorView_Should_Instantiate_Without_Errors()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Folder",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);
        Action act = () =>
        {
            var view = new ConfigEditorView
            {
                DataContext = viewModel
            };
        };

        // Assert
        act.Should().NotThrow("ConfigEditorView should instantiate without errors");
    }

    [Fact]
    [Trait("Smoke", "ConfigEditor")]
    public void ConfigEditorViewModel_Should_Have_All_Required_Commands()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Folder",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.SaveCommand.Should().NotBeNull("Should have SaveCommand");
        viewModel.RefreshCommand.Should().NotBeNull("Should have RefreshCommand");
    }

    [Fact]
    [Trait("Smoke", "ConfigEditor")]
    public void ConfigEditorViewModel_Should_Have_Required_Properties()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test Folder",
            ["icon"] = "Folder",
            ["sortOrder"] = 5
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);

        // Assert
        viewModel.FileName.Should().NotBeNullOrEmpty("Should have FileName");
        viewModel.DisplayName.Should().NotBeNullOrEmpty("Should have DisplayName");
        viewModel.Icon.Should().NotBeNullOrEmpty("Should have Icon");
        viewModel.SortOrder.Should().BeGreaterThanOrEqualTo(0, "Should have SortOrder");
        viewModel.RawJsonPreview.Should().NotBeNullOrEmpty("Should have RawJsonPreview");
        viewModel.IsDirty.Should().BeFalse("IsDirty should start false");
    }

    #endregion

    #region View and ViewModel Integration

    [Fact]
    [Trait("Smoke", "Integration")]
    public void ComponentDataEditor_View_And_ViewModel_Should_Bind_Correctly()
    {
        // Arrange
        var testFile = "binding-test.json";
        var testData = new JObject
        {
            ["components"] = new JObject { ["test"] = "value" }
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ComponentDataEditorViewModel(_jsonService, testFile);
        var view = new ComponentDataEditorView { DataContext = viewModel };

        // Assert
        view.DataContext.Should().Be(viewModel, "DataContext should be set");
        viewModel.Items.Should().HaveCountGreaterThan(0, "Should have loaded items");
    }

    [Fact]
    [Trait("Smoke", "Integration")]
    public void ConfigEditor_View_And_ViewModel_Should_Bind_Correctly()
    {
        // Arrange
        var testFile = ".cbconfig.json";
        var testData = new JObject
        {
            ["displayName"] = "Test",
            ["icon"] = "Folder",
            ["sortOrder"] = 0
        };
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData.ToString());

        // Act
        var viewModel = new ConfigEditorViewModel(_jsonService, testFile);
        var view = new ConfigEditorView { DataContext = viewModel };

        // Assert
        view.DataContext.Should().Be(viewModel, "DataContext should be set");
        viewModel.DisplayName.Should().NotBeNullOrEmpty("Should have loaded displayName");
    }
    
    #endregion
}
