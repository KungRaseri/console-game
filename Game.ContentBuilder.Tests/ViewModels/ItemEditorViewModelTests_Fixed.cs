using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for ItemEditorViewModel (FIXED to match actual API)
/// </summary>
public class ItemEditorViewModelTests_Fixed : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public ItemEditorViewModelTests_Fixed()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_ItemEditor_Structure_Correctly()
    {
        // Arrange
        var testFile = CreateTestFile();

        // Act
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Items.Should().HaveCount(4, "test file has 4 items across 2 rarities");
        viewModel.Items.Should().Contain(i => i.Name == "sharp" && i.Rarity == "common");
        viewModel.Items.Should().Contain(i => i.Name == "keen" && i.Rarity == "common");
        viewModel.Items.Should().Contain(i => i.Name == "flaming" && i.Rarity == "rare");
        viewModel.Items.Should().Contain(i => i.Name == "frost" && i.Rarity == "rare");
        
        var sharpItem = viewModel.Items.First(i => i.Name == "sharp");
        sharpItem.DisplayName.Should().Be("Sharp");
        sharpItem.Traits.Should().Contain(t => t.Key == "damage_bonus");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void AddItem_Should_Add_New_Item()
    {
        // Arrange
        var testFile = CreateTestFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Items.Count;

        // Act
        viewModel.AddItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(initialCount + 1);
        viewModel.SelectedItem.Should().NotBeNull("newly added item should be selected");
        viewModel.SelectedItem!.Name.Should().Be("NewItem");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void DeleteItem_Should_Remove_Selected_Item()
    {
        // Arrange
        var testFile = CreateTestFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var itemToDelete = viewModel.Items.First();
        viewModel.SelectedItem = itemToDelete;

        // Act
        viewModel.DeleteItemCommand.Execute(null);

        // Assert
        viewModel.Items.Should().NotContain(itemToDelete);
        viewModel.SelectedItem.Should().BeNull("selected item should be cleared after delete");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void DeleteItem_Command_Should_Be_Disabled_When_No_Selection()
    {
        // Arrange
        var testFile = CreateTestFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedItem = null;

        // Assert
        viewModel.DeleteItemCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Save_Should_Persist_Changes_To_File()
    {
        // Arrange
        var testFile = CreateTestFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        viewModel.AddItemCommand.Execute(null);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var filePath = Path.Combine(_testDataPath, testFile);
        File.Exists(filePath).Should().BeTrue();
        
        var content = File.ReadAllText(filePath);
        content.Should().Contain("NewItem", "saved file should contain the added item");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Cancel_Should_Reload_Data()
    {
        // Arrange
        var testFile = CreateTestFile();
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);
        var originalCount = viewModel.Items.Count;
        viewModel.AddItemCommand.Execute(null);

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        viewModel.Items.Should().HaveCount(originalCount, "cancel should revert unsaved changes");
        viewModel.SelectedItem.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_Items_From_Multiple_Rarity_Levels()
    {
        // Arrange
        var testFile = CreateTestFile();

        // Act
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);

        // Assert
        var commonItems = viewModel.Items.Where(i => i.Rarity == "common").ToList();
        var rareItems = viewModel.Items.Where(i => i.Rarity == "rare").ToList();
        
        commonItems.Should().HaveCount(2, "test file has 2 common items");
        rareItems.Should().HaveCount(2, "test file has 2 rare items");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Rarities_Collection_Should_Be_Populated()
    {
        // Arrange
        var testFile = CreateTestFile();

        // Act
        var viewModel = new ItemEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Rarities.Should().NotBeEmpty();
        viewModel.Rarities.Should().Contain("common");
        viewModel.Rarities.Should().Contain("uncommon");
        viewModel.Rarities.Should().Contain("rare");
        viewModel.Rarities.Should().Contain("epic");
        viewModel.Rarities.Should().Contain("legendary");
    }

    private string CreateTestFile()
    {
        var filename = $"test_{Guid.NewGuid()}.json";
        var data = @"{
  ""common"": {
    ""sharp"": {
      ""displayName"": ""Sharp"",
      ""traits"": {
        ""damage_bonus"": { ""value"": 5, ""type"": ""number"" }
      }
    },
    ""keen"": {
      ""displayName"": ""Keen"",
      ""traits"": {
        ""damage_bonus"": { ""value"": 3, ""type"": ""number"" }
      }
    }
  },
  ""rare"": {
    ""flaming"": {
      ""displayName"": ""Flaming"",
      ""traits"": {
        ""damage_bonus"": { ""value"": 10, ""type"": ""number"" },
        ""fire_damage"": { ""value"": 15, ""type"": ""number"" }
      }
    },
    ""frost"": {
      ""displayName"": ""Frost"",
      ""traits"": {
        ""damage_bonus"": { ""value"": 10, ""type"": ""number"" },
        ""cold_damage"": { ""value"": 15, ""type"": ""number"" }
      }
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
