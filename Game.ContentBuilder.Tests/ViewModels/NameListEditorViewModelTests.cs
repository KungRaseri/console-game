using Xunit;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Services;
using System.IO;
using System.Linq;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for NameListEditorViewModel
/// </summary>
public class NameListEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public NameListEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    [Fact]
    public void Should_Load_Simple_Category_Array_Structure()
    {
        // Arrange
        var testFile = "test_adjectives.json";
        var testData = @"{
  ""positive"": [
    ""magnificent"",
    ""exquisite"",
    ""pristine""
  ],
  ""negative"": [
    ""broken"",
    ""rusted"",
    ""damaged""
  ]
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Categories.Should().HaveCount(2);
        
        var positiveCategory = viewModel.Categories.FirstOrDefault(c => c.CategoryName == "positive");
        positiveCategory.Should().NotBeNull();
        positiveCategory!.Names.Should().HaveCount(3);
        positiveCategory.Names.Should().Contain("magnificent");
        
        var negativeCategory = viewModel.Categories.FirstOrDefault(c => c.CategoryName == "negative");
        negativeCategory.Should().NotBeNull();
        negativeCategory!.Names.Should().HaveCount(3);
        negativeCategory.Names.Should().Contain("broken");
    }

    [Fact]
    public void Should_Load_Nested_Items_Wrapper_Structure()
    {
        // Arrange
        var testFile = "test_weapon_names.json";
        var testData = @"{
  ""items"": {
    ""swords"": [
      ""Excalibur"",
      ""Moonblade""
    ],
    ""axes"": [
      ""Stormcleaver"",
      ""Frostbite""
    ]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.Categories.Should().HaveCount(2);
        
        var swordsCategory = viewModel.Categories.FirstOrDefault(c => c.CategoryName == "swords");
        swordsCategory.Should().NotBeNull();
        swordsCategory!.Names.Should().Contain("Excalibur");
        
        var axesCategory = viewModel.Categories.FirstOrDefault(c => c.CategoryName == "axes");
        axesCategory.Should().NotBeNull();
        axesCategory!.Names.Should().Contain("Stormcleaver");
    }

    [Fact]
    public void Should_Skip_Non_Array_Categories_Like_Variants()
    {
        // Arrange - enemy names structure with variants object
        var testFile = "test_enemy_names.json";
        var testData = @"{
  ""prefixes"": [
    ""Dire"",
    ""Wild""
  ],
  ""creatures"": [
    ""Wolf"",
    ""Bear""
  ],
  ""variants"": {
    ""wolf"": [
      ""Alpha Wolf"",
      ""Dire Wolf""
    ],
    ""bear"": [
      ""Grizzly Bear"",
      ""Cave Bear""
    ]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);

        // Act
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);

        // Assert
        // Should only load prefixes and creatures (arrays), skip variants (nested object)
        viewModel.Categories.Should().HaveCount(2);
        viewModel.Categories.Should().Contain(c => c.CategoryName == "prefixes");
        viewModel.Categories.Should().Contain(c => c.CategoryName == "creatures");
        viewModel.Categories.Should().NotContain(c => c.CategoryName == "variants");
    }

    [Fact]
    public void AddName_Should_Add_Name_To_Selected_Category()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First();
        var initialCount = viewModel.SelectedCategory.Names.Count;

        // Act
        viewModel.NewNameInput = "new name";
        viewModel.AddNameCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().HaveCount(initialCount + 1);
        viewModel.SelectedCategory.Names.Should().Contain("new name");
        viewModel.NewNameInput.Should().BeEmpty();
    }

    [Fact]
    public void DeleteName_Should_Remove_Name_From_Category()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First();
        var nameToDelete = viewModel.SelectedCategory.Names.First();

        // Act
        viewModel.SelectedName = nameToDelete;
        viewModel.DeleteNameCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().NotContain(nameToDelete);
    }

    [Fact]
    public void AddCategory_Should_Create_New_Category()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        var initialCount = viewModel.Categories.Count;

        // Act
        viewModel.NewCategoryName = "new_category";
        viewModel.AddCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(initialCount + 1);
        viewModel.Categories.Should().Contain(c => c.CategoryName == "new_category");
        viewModel.NewCategoryName.Should().BeEmpty();
    }

    [Fact]
    public void Save_Should_Preserve_Items_Wrapper_If_Present()
    {
        // Arrange
        var testFile = "test_with_wrapper.json";
        var testData = @"{
  ""items"": {
    ""category1"": [""item1""]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        
        viewModel.SelectedCategory = viewModel.Categories.First();
        viewModel.NewNameInput = "item2";
        viewModel.AddNameCommand.Execute(null);

        // Act
        viewModel.SaveCommand.Execute(null);

        // Assert
        var content = File.ReadAllText(Path.Combine(_testDataPath, testFile));
        content.Should().Contain("\"items\"");
        content.Should().Contain("item2");
    }

    [Fact]
    public void Should_Not_Add_Name_Without_Selected_Category()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = null;

        // Act
        viewModel.NewNameInput = "new name";
        viewModel.AddNameCommand.Execute(null);

        // Assert - should not throw or crash
        viewModel.NewNameInput.Should().Be("new name"); // Input not cleared
    }

    [Fact]
    public void Should_Not_Add_Empty_Name()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First();
        var initialCount = viewModel.SelectedCategory.Names.Count;

        // Act
        viewModel.NewNameInput = "";
        viewModel.AddNameCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().HaveCount(initialCount);
    }

    [Fact]
    public void Should_Not_Add_Duplicate_Category()
    {
        // Arrange
        var testFile = CreateMinimalNameListFile();
        var viewModel = new NameListEditorViewModel(_jsonEditorService, testFile);
        var existingCategoryName = viewModel.Categories.First().CategoryName;
        var initialCount = viewModel.Categories.Count;

        // Act
        viewModel.NewCategoryName = existingCategoryName;
        viewModel.AddCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(initialCount);
    }

    private string CreateMinimalNameListFile()
    {
        var filename = $"test_{Guid.NewGuid()}.json";
        var data = @"{
  ""category1"": [""item1"", ""item2""],
  ""category2"": [""item3"", ""item4""]
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
