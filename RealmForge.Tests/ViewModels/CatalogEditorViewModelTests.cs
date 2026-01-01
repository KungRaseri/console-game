using System.IO;
using FluentAssertions;
using RealmForge.Services;
using RealmForge.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RealmForge.Tests.ViewModels;

/// <summary>
/// Comprehensive unit tests for CatalogEditorViewModel
/// Tests catalog/category/item management operations
/// </summary>
[Trait("Category", "ViewModel")]
public class CatalogEditorViewModelTests : IDisposable
{
  private readonly string _testDataPath;
  private readonly string _testFileName;
  private readonly JsonEditorService _jsonService;

  public CatalogEditorViewModelTests()
  {
    // Setup test data directory
    _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testDataPath);

    _testFileName = "test-catalog.json";
    _jsonService = new JsonEditorService(_testDataPath);

    CreateTestCatalogFile();
  }

  private void CreateTestCatalogFile()
  {
    var testData = new JObject
    {
      ["metadata"] = new JObject
      {
        ["version"] = "4.0",
        ["type"] = "item_catalog",
        ["description"] = "Test catalog",
        ["usage"] = "Testing purposes",
        ["notes"] = new JArray { "Test note 1", "Test note 2" }
      },
      ["weapon_types"] = new JObject
      {
        ["swords"] = new JObject
        {
          ["traits"] = new JObject
          {
            ["damageType"] = "slashing",
            ["weaponClass"] = "melee"
          },
          ["items"] = new JArray
                    {
                        new JObject
                        {
                            ["name"] = "Longsword",
                            ["rarityWeight"] = 10,
                            ["damage"] = "1d8",
                            ["traits"] = new JArray { "versatile", "metal" }
                        },
                        new JObject
                        {
                            ["name"] = "Shortsword",
                            ["rarityWeight"] = 15,
                            ["damage"] = "1d6",
                            ["traits"] = new JArray { "light", "metal" }
                        }
                    }
        },
        ["axes"] = new JObject
        {
          ["items"] = new JArray
                    {
                        new JObject
                        {
                            ["name"] = "Battleaxe",
                            ["rarityWeight"] = 8,
                            ["damage"] = "1d8"
                        }
                    }
        }
      }
    };

    File.WriteAllText(
        Path.Combine(_testDataPath, _testFileName),
        testData.ToString(Newtonsoft.Json.Formatting.Indented));
  }

  [Fact]
  public void Constructor_Should_Initialize_ViewModel_With_Valid_Data()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.Should().NotBeNull();
    viewModel.FileName.Should().NotBeNullOrEmpty();
    viewModel.TypeCatalogs.Should().NotBeEmpty();
  }

  [Fact]
  public void Constructor_Should_Load_Metadata_From_File()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.MetadataVersion.Should().Be("4.0");
    viewModel.MetadataType.Should().Be("item_catalog");
    viewModel.MetadataDescription.Should().Be("Test catalog");
    viewModel.MetadataUsage.Should().Be("Testing purposes");
  }

  [Fact]
  public void Constructor_Should_Load_Metadata_Notes()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.MetadataNotes.Should().HaveCount(2);
    viewModel.MetadataNotes.Should().Contain("Test note 1");
    viewModel.MetadataNotes.Should().Contain("Test note 2");
  }

  [Fact]
  public void MetadataNotesText_Should_Join_Notes_With_Newlines()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.MetadataNotesText.Should().Contain("Test note 1");
    viewModel.MetadataNotesText.Should().Contain("Test note 2");
  }

  [Fact]
  public void MetadataNotesText_Setter_Should_Split_By_Newlines()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Act
    viewModel.MetadataNotesText = "Line 1\r\nLine 2\nLine 3";

    // Assert
    viewModel.MetadataNotes.Should().HaveCount(3);
    viewModel.MetadataNotes.Should().Contain("Line 1");
    viewModel.MetadataNotes.Should().Contain("Line 2");
    viewModel.MetadataNotes.Should().Contain("Line 3");
  }

  [Fact]
  public void Constructor_Should_Load_Type_Catalogs()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.TypeCatalogs.Should().HaveCount(1);
    var catalog = viewModel.TypeCatalogs.First();
    catalog.Name.Should().Be("weapon_types");
  }

  [Fact]
  public void Constructor_Should_Load_Categories_Within_Catalogs()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    var catalog = viewModel.TypeCatalogs.First();
    catalog.Categories.Should().HaveCount(2);
    catalog.Categories.Should().Contain(c => c.Name == "swords");
    catalog.Categories.Should().Contain(c => c.Name == "axes");
  }

  [Fact]
  public void Constructor_Should_Load_Category_Traits()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    var catalog = viewModel.TypeCatalogs.First();
    var swordsCategory = catalog.Categories.First(c => c.Name == "swords");

    swordsCategory.Traits.Should().HaveCount(2);
    swordsCategory.Traits.Should().Contain(t => t.Key == "damageType" && t.Value == "slashing");
    swordsCategory.Traits.Should().Contain(t => t.Key == "weaponClass" && t.Value == "melee");
  }

  [Fact]
  public void Constructor_Should_Load_Items_Within_Categories()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    var catalog = viewModel.TypeCatalogs.First();
    var swordsCategory = catalog.Categories.First(c => c.Name == "swords");

    swordsCategory.Items.Should().HaveCount(2);
    swordsCategory.Items.Should().Contain(i => i.Name == "Longsword");
    swordsCategory.Items.Should().Contain(i => i.Name == "Shortsword");
  }

  [Fact]
  public void Constructor_Should_Load_Item_Properties()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    var catalog = viewModel.TypeCatalogs.First();
    var swordsCategory = catalog.Categories.First(c => c.Name == "swords");
    var longsword = swordsCategory.Items.First(i => i.Name == "Longsword");

    longsword.RarityWeight.Should().Be(10);
    longsword.Properties.Should().Contain(p => p.Key == "damage" && p.Value == "1d8");
  }

  [Fact]
  public void Constructor_Should_Load_Item_Traits()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    var catalog = viewModel.TypeCatalogs.First();
    var swordsCategory = catalog.Categories.First(c => c.Name == "swords");
    var longsword = swordsCategory.Items.First(i => i.Name == "Longsword");

    longsword.Traits.Should().HaveCount(2);
    longsword.Traits.Should().Contain("versatile");
    longsword.Traits.Should().Contain("metal");
  }

  [Fact]
  public void IsDirty_Should_Be_False_After_Initial_Load()
  {
    // Act
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Assert
    viewModel.IsDirty.Should().BeFalse();
  }

  [Fact]
  public void AddCategoryCommand_Should_Add_New_Category()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    viewModel.SelectedCatalog = viewModel.TypeCatalogs.First();
    var initialCount = viewModel.SelectedCatalog.Categories.Count;

    // Act
    viewModel.AddCategoryCommand.Execute(null);

    // Assert
    viewModel.SelectedCatalog.Categories.Should().HaveCount(initialCount + 1);
    viewModel.SelectedCategory.Should().NotBeNull();
    viewModel.SelectedCategory!.Name.Should().Be("new_category");
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void AddCategoryCommand_Should_Not_Add_When_No_Catalog_Selected()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    viewModel.SelectedCatalog = null;

    // Act
    viewModel.AddCategoryCommand.Execute(null);

    // Assert - should not throw or crash
    viewModel.SelectedCategory.Should().BeNull();
  }

  [Fact]
  public void RemoveCategoryCommand_Should_Remove_Selected_Category()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    viewModel.SelectedCatalog = viewModel.TypeCatalogs.First();
    var categoryToRemove = viewModel.SelectedCatalog.Categories.First();
    viewModel.SelectedCategory = categoryToRemove;
    var initialCount = viewModel.SelectedCatalog.Categories.Count;

    // Act
    viewModel.RemoveCategoryCommand.Execute(null);

    // Assert
    viewModel.SelectedCatalog.Categories.Should().HaveCount(initialCount - 1);
    viewModel.SelectedCatalog.Categories.Should().NotContain(categoryToRemove);
    viewModel.SelectedCategory.Should().BeNull();
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void AddItemCommand_Should_Add_New_Item_To_Selected_Category()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First();
    viewModel.SelectedCategory = category;
    var initialCount = category.Items.Count;

    // Act
    viewModel.AddItemCommand.Execute(null);

    // Assert
    category.Items.Should().HaveCount(initialCount + 1);
    viewModel.SelectedItem.Should().NotBeNull();
    viewModel.SelectedItem!.Name.Should().Be("New Item");
    viewModel.SelectedItem.RarityWeight.Should().Be(1);
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void RemoveItemCommand_Should_Remove_Selected_Item()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First(c => c.Items.Count > 0);
    viewModel.SelectedCategory = category;
    var itemToRemove = category.Items.First();
    viewModel.SelectedItem = itemToRemove;
    var initialCount = category.Items.Count;

    // Act
    viewModel.RemoveItemCommand.Execute(null);

    // Assert
    category.Items.Should().HaveCount(initialCount - 1);
    category.Items.Should().NotContain(itemToRemove);
    viewModel.SelectedItem.Should().BeNull();
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void AddTraitCommand_Should_Add_New_Trait_To_Category()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First();
    viewModel.SelectedCategory = category;
    var initialCount = category.Traits.Count;

    // Act
    viewModel.AddTraitCommand.Execute(null);

    // Assert
    category.Traits.Should().HaveCount(initialCount + 1);
    var newTrait = category.Traits.Last();
    newTrait.Key.Should().StartWith("trait_");
    newTrait.Value.Should().Be("value");
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void RemoveTraitCommand_Should_Remove_Specified_Trait()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First(c => c.Traits.Count > 0);
    viewModel.SelectedCategory = category;
    var traitToRemove = category.Traits.First();
    var initialCount = category.Traits.Count;

    // Act
    viewModel.RemoveTraitCommand.Execute(traitToRemove.Key);

    // Assert
    category.Traits.Should().HaveCount(initialCount - 1);
    category.Traits.Should().NotContain(traitToRemove);
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void AddPropertyCommand_Should_Add_New_Property_To_Item()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First(c => c.Items.Count > 0);
    var item = category.Items.First();
    viewModel.SelectedItem = item;
    var initialCount = item.Properties.Count;

    // Act
    viewModel.AddPropertyCommand.Execute(null);

    // Assert
    item.Properties.Should().HaveCount(initialCount + 1);
    var newProperty = item.Properties.Last();
    newProperty.Key.Should().Be("new_property");
    newProperty.Value.Should().Be("value");
    viewModel.IsDirty.Should().BeTrue();
  }

  [Fact]
  public void ShowCategoryTraits_Should_Be_True_When_Category_Selected_Without_Item()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First();

    // Act
    viewModel.SelectedCategory = category;
    viewModel.SelectedItem = null;

    // Assert
    viewModel.ShowCategoryTraits.Should().BeTrue();
  }

  [Fact]
  public void ShowCategoryTraits_Should_Be_False_When_Item_Selected()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    var catalog = viewModel.TypeCatalogs.First();
    var category = catalog.Categories.First(c => c.Items.Count > 0);
    var item = category.Items.First();

    // Act
    viewModel.SelectedCategory = category;
    viewModel.SelectedItem = item;

    // Assert
    viewModel.ShowCategoryTraits.Should().BeFalse();
  }

  [Fact]
  public void ShowCategoryTraits_Should_Be_False_When_No_Category_Selected()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);

    // Act
    viewModel.SelectedCategory = null;
    viewModel.SelectedItem = null;

    // Assert
    viewModel.ShowCategoryTraits.Should().BeFalse();
  }

  [Fact]
  public void SaveAsync_Should_Mark_IsDirty_As_False()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    viewModel.AddCategoryCommand.Execute(null); // Make it dirty

    // Act
    viewModel.SaveCommand.Execute(null);

    // Assert
    viewModel.IsDirty.Should().BeFalse();
  }

  [Fact]
  public void StatusMessage_Should_Be_Set_After_Operations()
  {
    // Arrange
    var viewModel = new CatalogEditorViewModel(_jsonService, _testFileName);
    viewModel.SelectedCatalog = viewModel.TypeCatalogs.First();

    // Act
    viewModel.AddCategoryCommand.Execute(null);

    // Assert
    viewModel.StatusMessage.Should().NotBeNullOrEmpty();
    viewModel.StatusMessage.Should().Contain("category");
  }

  public void Dispose()
  {
    if (Directory.Exists(_testDataPath))
    {
      try
      {
        Directory.Delete(_testDataPath, true);
      }
      catch
      {
        // Ignore cleanup errors
      }
    }
  }
}
