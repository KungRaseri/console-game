using Xunit;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Services;
using System.IO;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for FlatItemEditorViewModel
/// </summary>
public class FlatItemEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public FlatItemEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    [Fact]
    public void Should_Load_FlatItem_Structure_Correctly()
    {
        // Arrange
        var testFile = "test_metals.json";
        var testData = @"{
  ""iron"": {
    ""displayName"": ""Iron"",
    ""traits"": {
      ""durability"": 50,
      ""value"": 10,
      ""rarity"": ""common""
    }
  },
  ""steel"": {
    ""displayName"": ""Steel"",
    ""traits"": {
      ""durability"": 75,
      ""value"": 25,
      ""rarity"": ""uncommon""
    }
  },
  ""mithril"": {
    ""displayName"": ""Mithril"",
    ""traits"": {
      ""durability"": 100,
      ""value"": 500,
      ""rarity"": ""rare""
    }
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Items.Should().HaveCount(3);
        viewModel.Items.Should().ContainKey("iron");
        viewModel.Items.Should().ContainKey("steel");
        viewModel.Items.Should().ContainKey("mithril");
        
        viewModel.Items["iron"].DisplayName.Should().Be("Iron");
        viewModel.Items["steel"].Traits.Should().ContainKey("durability");
        viewModel.Items["mithril"].Traits["rarity"].ToString().Should().Be("rare");
    }

    [Fact]
    public void AddItem_Should_Add_New_Item_With_Default_Structure()
    {
        // Arrange
        var testFile = CreateMinimalFlatItemFile();
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "new_item";
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount + 1);
        viewModel.Items.Should().ContainKey("new_item");
        viewModel.Items["new_item"].DisplayName.Should().Be("new_item");
        viewModel.Items["new_item"].Traits.Should().NotBeNull();
        viewModel.NewItemKey.Should().BeEmpty();
    }

    [Fact]
    public void DeleteItem_Should_Remove_Item()
    {
        // Arrange
        var testFile = CreateMinimalFlatItemFile();
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedItemKey = "iron";

        // Act
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().NotContainKey("iron");
    }

    [Fact]
    public void Save_Should_Persist_Changes_To_File()
    {
        // Arrange
        var testFile = CreateMinimalFlatItemFile();
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewItemKey = "gold";
        viewModel.AddItemCommand.Execute(null);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var filePath = Path.Combine(_testDataPath, testFile);
        File.Exists(filePath).Should().BeTrue();
        
        var content = File.ReadAllText(filePath);
        content.Should().Contain("gold");
    }

    [Fact]
    public void Should_Not_Add_Item_With_Empty_Key()
    {
        // Arrange
        var testFile = CreateMinimalFlatItemFile();
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "";
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount);
    }

    [Fact]
    public void Should_Not_Add_Duplicate_Item_Key()
    {
        // Arrange
        var testFile = CreateMinimalFlatItemFile();
        var viewModel = new FlatItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.NewItemKey = "iron"; // Already exists
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount);
    }

    private string CreateMinimalFlatItemFile()
    {
        var filename = $"test_{Guid.NewGuid()}.json";
        var data = @"{
  ""iron"": {
    ""displayName"": ""Iron"",
    ""traits"": {
      ""durability"": 50
    }
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
