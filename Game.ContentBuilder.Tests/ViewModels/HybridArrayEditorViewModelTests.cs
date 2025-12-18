using Xunit;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Services;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for HybridArrayEditorViewModel
/// </summary>
public class HybridArrayEditorViewModelTests
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public HybridArrayEditorViewModelTests()
    {
        // Setup test data directory
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    [Fact]
    public void Should_Load_HybridArray_Structure_Correctly()
    {
        // Arrange
        var testFile = "test_colors.json";
        var testData = @"{
  ""items"": [
    ""crimson"",
    ""azure"",
    ""emerald""
  ],
  ""components"": {
    ""base_colors"": [
      ""red"",
      ""blue"",
      ""green""
    ],
    ""modifiers"": [
      ""dark"",
      ""light""
    ]
  },
  ""patterns"": [
    ""base_color"",
    ""modifier + base_color""
  ],
  ""metadata"": {
    ""version"": ""1.0""
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Items.Should().HaveCount(3);
        viewModel.Items.Should().Contain("crimson");
        viewModel.Items.Should().Contain("azure");
        viewModel.Items.Should().Contain("emerald");

        viewModel.ComponentGroups.Should().HaveCount(2);
        viewModel.ComponentGroups[0].Name.Should().Be("base_colors");
        viewModel.ComponentGroups[0].Components.Should().HaveCount(3);
        viewModel.ComponentGroups[1].Name.Should().Be("modifiers");
        viewModel.ComponentGroups[1].Components.Should().HaveCount(2);

        viewModel.Patterns.Should().HaveCount(2);
        viewModel.Patterns.Should().Contain(p => p.Pattern == "base_color");

        viewModel.TotalItemsCount.Should().Be(3);
        viewModel.TotalComponentsCount.Should().Be(5);
        viewModel.TotalPatternsCount.Should().Be(2);
    }

    [Fact]
    public void AddItem_Should_Add_Item_And_Update_Count()
    {
        // Arrange
        var testFile = CreateMinimalHybridFile();
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemInput = "test item";
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount + 1);
        viewModel.Items.Should().Contain("test item");
        viewModel.TotalItemsCount.Should().Be(initialCount + 1);
        viewModel.NewItemInput.Should().BeEmpty();
    }

    [Fact]
    public void DeleteItem_Should_Remove_Item_And_Update_Count()
    {
        // Arrange
        var testFile = CreateMinimalHybridFile();
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewItemInput = "item to delete";
        viewModel.AddItemCommand.Execute(null);
        var itemCount = viewModel.Items.Count;

        // Act
        viewModel.SelectedItem = "item to delete";
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(itemCount - 1);
        viewModel.Items.Should().NotContain("item to delete");
        viewModel.TotalItemsCount.Should().Be(itemCount - 1);
    }

    [Fact]
    public void AddComponentGroup_Should_Create_New_Group()
    {
        // Arrange
        var testFile = CreateMinimalHybridFile();
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);

        // Act
        viewModel.NewComponentGroupName = "test_group";
        viewModel.AddComponentGroupCommand.Execute(null);

        // Assert
        viewModel.ComponentGroups.Should().Contain(g => g.Name == "test_group");
        viewModel.NewComponentGroupName.Should().BeEmpty();
    }

    [Fact]
    public void AddComponent_Should_Add_To_Selected_Group()
    {
        // Arrange
        var testFile = CreateMinimalHybridFile();
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewComponentGroupName = "test_group";
        viewModel.AddComponentGroupCommand.Execute(null);
        viewModel.SelectedComponentGroup = viewModel.ComponentGroups.First(g => g.Name == "test_group");

        // Act
        viewModel.NewComponentInput = "test component";
        viewModel.AddComponentCommand.Execute(null);

        // Assert
        viewModel.SelectedComponentGroup.Components.Should().Contain("test component");
        viewModel.NewComponentInput.Should().BeEmpty();
    }

    [Fact]
    public void Save_Should_Preserve_Metadata()
    {
        // Arrange
        var testFile = "test_with_metadata.json";
        var testData = @"{
  ""items"": [""item1""],
  ""components"": {},
  ""patterns"": [],
  ""metadata"": {
    ""version"": ""1.0"",
    ""author"": ""test""
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);

        // Act
        viewModel.NewItemInput = "new item";
        viewModel.AddItemCommand.Execute(null);
        viewModel.SaveCommand.Execute(null);

        // Assert
        var savedJson = File.ReadAllText(Path.Combine(_testDataPath, testFile));
        var savedData = JObject.Parse(savedJson);
        savedData["metadata"]!["version"]!.ToString().Should().Be("1.0");
        savedData["metadata"]!["author"]!.ToString().Should().Be("test");
        savedData["items"]!.Should().HaveCount(2);
    }

    [Fact]
    public void CanSave_Should_Return_True_When_Content_Exists()
    {
        // Arrange
        var testFile = CreateMinimalHybridFile();
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewItemInput = "test";
        viewModel.AddItemCommand.Execute(null);

        // Act & Assert
        viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void Should_Handle_Empty_File_Gracefully()
    {
        // Arrange
        var testFile = "empty.json";
        var testData = @"{
  ""items"": [],
  ""components"": {},
  ""patterns"": []
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new HybridArrayEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Items.Should().BeEmpty();
        viewModel.ComponentGroups.Should().BeEmpty();
        viewModel.Patterns.Should().BeEmpty();
        viewModel.TotalItemsCount.Should().Be(0);
        viewModel.TotalComponentsCount.Should().Be(0);
        viewModel.TotalPatternsCount.Should().Be(0);
    }

    private string CreateMinimalHybridFile()
    {
        var fileName = $"test_{Guid.NewGuid()}.json";
        var data = @"{
  ""items"": [],
  ""components"": {},
  ""patterns"": []
}";
        File.WriteAllText(Path.Combine(_testDataPath, fileName), data);
        return fileName;
    }
}
