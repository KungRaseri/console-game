using Xunit;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Services;
using System.IO;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Tests for NameCatalogEditorViewModel
/// </summary>
public class NameCatalogEditorViewModelTests
{
    private readonly string _testDataPath;
    private readonly JsonEditorService _jsonEditorService;

    public NameCatalogEditorViewModelTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        _jsonEditorService = new JsonEditorService(_testDataPath);
    }

    private string CreateTestNameCatalog()
    {
        var testFile = "first_names.json";
        var testData = @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""name_catalog"",
    ""description"": ""First names for NPCs""
  },
  ""categories"": {
    ""male_common"": [
      ""John"",
      ""Michael"",
      ""William""
    ],
    ""female_common"": [
      ""Mary"",
      ""Elizabeth"",
      ""Sarah""
    ],
    ""unisex"": [
      ""Alex"",
      ""Jordan""
    ]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, testFile), testData);
        return testFile;
    }

    [Fact]
    public void Constructor_Should_Load_Name_Catalog()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();

        // Act
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);

        // Assert
        viewModel.FileName.Should().Be("first_names");
        viewModel.Categories.Should().HaveCount(3);
        viewModel.TotalNameCount.Should().Be(8);
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void Should_Load_Categories_With_Correct_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();

        // Act
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);

        // Assert
        var maleCategory = viewModel.Categories.FirstOrDefault(c => c.Name == "male_common");
        maleCategory.Should().NotBeNull();
        maleCategory!.Names.Should().HaveCount(3);
        maleCategory.Names.Should().Contain("John");
        maleCategory.Names.Should().Contain("Michael");

        var femaleCategory = viewModel.Categories.FirstOrDefault(c => c.Name == "female_common");
        femaleCategory.Should().NotBeNull();
        femaleCategory!.Names.Should().HaveCount(3);

        var unisexCategory = viewModel.Categories.FirstOrDefault(c => c.Name == "unisex");
        unisexCategory.Should().NotBeNull();
        unisexCategory!.Names.Should().HaveCount(2);
    }

    [Fact]
    public void AddCategory_Should_Create_New_Empty_Category()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewCategoryName = "male_noble";

        // Act
        viewModel.AddCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(4);
        var newCategory = viewModel.Categories.FirstOrDefault(c => c.Name == "male_noble");
        newCategory.Should().NotBeNull();
        newCategory!.Names.Should().BeEmpty();
        viewModel.SelectedCategory.Should().Be(newCategory);
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void AddCategory_Should_Reject_Duplicate_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewCategoryName = "male_common"; // Already exists

        // Act
        viewModel.AddCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(3); // No new category added
        viewModel.StatusMessage.Should().Contain("already exists");
    }

    [Fact]
    public void AddCategory_Should_Reject_Empty_Name()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.NewCategoryName = "";

        // Act
        viewModel.AddCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(3);
        viewModel.StatusMessage.Should().Contain("cannot be empty");
    }

    [Fact]
    public void RemoveCategory_Should_Delete_Selected_Category_Without_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        
        // Add a new empty category (no confirmation dialog)
        viewModel.NewCategoryName = "empty_category";
        viewModel.AddCategoryCommand.Execute(null);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "empty_category");

        // Act
        viewModel.RemoveCategoryCommand.Execute(null);

        // Assert
        viewModel.Categories.Should().HaveCount(3); // Back to original 3
        viewModel.Categories.Should().NotContain(c => c.Name == "empty_category");
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void AddName_Should_Add_Name_To_Selected_Category()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");
        viewModel.NewNameInput = "James";

        // Act
        viewModel.AddNameCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().Contain("James");
        viewModel.SelectedCategory.Names.Should().HaveCount(4);
        viewModel.TotalNameCount.Should().Be(9);
        viewModel.NewNameInput.Should().BeEmpty();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void AddName_Should_Reject_Duplicate_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");
        viewModel.NewNameInput = "John"; // Already exists

        // Act
        viewModel.AddNameCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().HaveCount(3); // No duplicate added
        viewModel.StatusMessage.Should().Contain("already exists");
    }

    [Fact]
    public void AddName_Should_Validate_Name_Format()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");

        // Act & Assert - Too short
        viewModel.NewNameInput = "A";
        viewModel.AddNameCommand.Execute(null);
        viewModel.StatusMessage.Should().Contain("at least 2 characters");
        
        // Act & Assert - Invalid characters
        viewModel.NewNameInput = "John123";
        viewModel.AddNameCommand.Execute(null);
        viewModel.StatusMessage.Should().Contain("letters, hyphens, and apostrophes");

        // Act & Assert - Valid with hyphen
        viewModel.NewNameInput = "Mary-Jane";
        viewModel.AddNameCommand.Execute(null);
        viewModel.SelectedCategory.Names.Should().Contain("Mary-Jane");

        // Act & Assert - Valid with apostrophe
        viewModel.NewNameInput = "O'Connor";
        viewModel.AddNameCommand.Execute(null);
        viewModel.SelectedCategory.Names.Should().Contain("O'Connor");
    }

    [Fact]
    public void BulkAddNames_Should_Add_Multiple_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");
        viewModel.BulkNamesInput = "David, Robert, Richard";

        // Act
        viewModel.BulkAddNamesCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().Contain(new[] { "David", "Robert", "Richard" });
        viewModel.SelectedCategory.Names.Should().HaveCount(6);
        viewModel.TotalNameCount.Should().Be(11);
        viewModel.BulkNamesInput.Should().BeEmpty();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void BulkAddNames_Should_Handle_Line_Separated_Names()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "female_common");
        viewModel.BulkNamesInput = "Jennifer\nJessica\nAmanda";

        // Act
        viewModel.BulkAddNamesCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().Contain(new[] { "Jennifer", "Jessica", "Amanda" });
    }

    [Fact]
    public void BulkAddNames_Should_Skip_Duplicates_And_Validate()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");
        viewModel.BulkNamesInput = "David, John, Robert, Invalid123, A"; // John exists, Invalid123 and A are invalid

        // Act
        viewModel.BulkAddNamesCommand.Execute(null);

        // Assert
        viewModel.SelectedCategory.Names.Should().Contain(new[] { "David", "Robert" });
        viewModel.SelectedCategory.Names.Should().NotContain("Invalid123");
        viewModel.SelectedCategory.Names.Should().HaveCount(5); // 3 original + David + Robert
        viewModel.StatusMessage.Should().Contain("added");
        viewModel.StatusMessage.Should().Contain("skipped");
        viewModel.StatusMessage.Should().Contain("errors");
    }

    [Fact]
    public void SortCategory_Should_Sort_Names_Alphabetically()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        viewModel.SelectedCategory = viewModel.Categories.First(c => c.Name == "male_common");
        
        // Add names in random order
        viewModel.SelectedCategory.Names.Add("Zachary");
        viewModel.SelectedCategory.Names.Add("Aaron");
        viewModel.SelectedCategory.Names.Add("David");

        // Act
        viewModel.SortCategoryCommand.Execute(null);

        // Assert
        var names = viewModel.SelectedCategory.Names.ToList();
        names[0].Should().Be("Aaron");
        names[1].Should().Be("David");
        names[2].Should().Be("John");
        names[3].Should().Be("Michael");
        names[4].Should().Be("William");
        names[5].Should().Be("Zachary");
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public async Task SaveFile_Should_Persist_Changes()
    {
        // Arrange
        var testFile = CreateTestNameCatalog();
        var viewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        
        // Add a new category and name
        viewModel.NewCategoryName = "test_category";
        viewModel.AddCategoryCommand.Execute(null);
        viewModel.NewNameInput = "TestName";
        viewModel.AddNameCommand.Execute(null);

        // Act
        await viewModel.SaveFileCommand.ExecuteAsync(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();

        // Verify by reloading
        var reloadedViewModel = new NameCatalogEditorViewModel(_jsonEditorService, testFile);
        reloadedViewModel.Categories.Should().HaveCount(4);
        var testCategory = reloadedViewModel.Categories.FirstOrDefault(c => c.Name == "test_category");
        testCategory.Should().NotBeNull();
        testCategory!.Names.Should().Contain("TestName");
    }
}
