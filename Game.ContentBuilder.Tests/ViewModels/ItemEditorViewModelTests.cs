using Xunit;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Services;
using System.IO;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for ItemEditorViewModel (Prefix/Suffix editor)
/// </summary>
public class ItemEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public ItemEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    [Fact]
    public void Should_Load_Item_Prefix_Structure_Correctly()
    {
        // Arrange
        var testFile = "test_prefixes.json";
        var testData = @"{
  ""Flaming"": {
    ""displayName"": ""Flaming"",
    ""damageType"": ""fire"",
    ""damageBonus"": 10
  },
  ""Frozen"": {
    ""displayName"": ""Frozen"",
    ""damageType"": ""ice"",
    ""damageBonus"": 8
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Items.Should().HaveCount(2);
        viewModel.Items.Should().ContainKey("Flaming");
        viewModel.Items.Should().ContainKey("Frozen");
        viewModel.Items["Flaming"]["displayName"].ToString().Should().Be("Flaming");
        viewModel.Items["Frozen"]["damageBonus"].ToString().Should().Be("8");
    }

    [Fact]
    public void AddItem_Should_Add_New_Item()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "new_prefix";
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount + 1);
        viewModel.Items.Should().ContainKey("new_prefix");
        viewModel.NewItemKey.Should().BeEmpty();
    }

    [Fact]
    public void DeleteItem_Should_Remove_Item()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedItemKey = "Flaming";

        // Act
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().NotContainKey("Flaming");
    }

    [Fact]
    public void AddProperty_Should_Add_Property_To_Selected_Item()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedItemKey = "Flaming";

        // Act
        viewModel.NewPropertyKey = "newProperty";
        viewModel.NewPropertyValue = "newValue";
        viewModel.AddPropertyCommand.Execute(null);

        // Assert
        viewModel.Items["Flaming"].Should().ContainKey("newProperty");
        viewModel.Items["Flaming"]["newProperty"].ToString().Should().Be("newValue");
    }

    [Fact]
    public void Save_Should_Persist_Changes()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewItemKey = "Electric";
        viewModel.AddItemCommand.Execute(null);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var filePath = Path.Combine(_testDataPath, testFile);
        var content = File.ReadAllText(filePath);
        content.Should().Contain("Electric");
    }

    [Fact]
    public void Should_Not_Add_Item_With_Empty_Key()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "";
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount);
    }

    [Fact]
    public void Should_Not_Add_Duplicate_Item()
    {
        // Arrange
        var testFile = CreateMinimalItemFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "Flaming"; // Already exists
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount);
    }

    private string CreateMinimalItemFile()
    {
        var filename = $"test_{Guid.NewGuid()}.json";
        var data = @"{
  ""Flaming"": {
    ""displayName"": ""Flaming"",
    ""damageType"": ""fire""
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, filename), data);
        return filename;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            Directory.Delete(_testDataPath, true);
        }
    }
}
