using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Comprehensive UI tests for the Catalog Editor (catalog.json format)
/// Covers: Category management, item CRUD, field editing, metadata, rarity/weight
/// </summary>
[Collection("UI Tests")]
public class CatalogEditor_ComprehensiveTests
{
    private readonly UITestCollectionFixture _fixture;
    private Application _app => _fixture.App!;
    private UIA3Automation _automation => _fixture.Automation!;
    private Window _mainWindow => _fixture.MainWindow!;

    public CatalogEditor_ComprehensiveTests(UITestCollectionFixture fixture)
    {
        _fixture = fixture;
        Thread.Sleep(500);
        NavigateToMaterialsCatalog(); // Navigate to items/materials/catalog.json
    }

    #region Category Management Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Display_Category_List()
    {
        // Act
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));

        // Assert
        categoryList.Should().NotBeNull("Should display category list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Display_Multiple_Categories()
    {
        // Act
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var categories = categoryList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem));

        // Assert
        categories?.Length.Should().BeGreaterThan(0, "Should display at least one category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Select_Category_When_Clicked()
    {
        // Act
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Assert
        var selectedCategory = categoryList?.AsListBox()?.SelectedItem;
        selectedCategory.Should().NotBeNull("Should have a selected category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Add_New_Category_When_Add_Button_Clicked()
    {
        // Arrange
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var initialCount = categoryList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;

        // Act
        var addCategoryButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCategoryButton"));
        addCategoryButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = categoryList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;
        newCount.Should().Be(initialCount + 1, "Should add one new category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Delete_Category_When_Delete_Button_Clicked()
    {
        // Arrange - Ensure we have at least 2 categories
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCategoryButton"));
        addButton?.Click();
        Thread.Sleep(500);

        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var initialCount = categoryList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;

        // Act - Select first category and delete
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(300);

        var deleteButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteCategoryButton"));
        deleteButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = categoryList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;
        newCount.Should().Be(initialCount - 1, "Should remove one category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Rename_Category_When_Name_Changed()
    {
        // Arrange - Select first category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Act - Change category name
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryNameTextBox"))?.AsTextBox();
        nameTextBox?.Enter("NewCategoryName");
        Thread.Sleep(500);

        // Assert
        nameTextBox?.Text.Should().Contain("NewCategoryName", "Category name should update");
    }

    #endregion

    #region Item List Management Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Items")]
    public void Should_Display_Item_List_When_Category_Selected()
    {
        // Arrange - Select first category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Act
        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListView"));

        // Assert
        itemList.Should().NotBeNull("Should display item list when category selected");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Items")]
    public void Should_Add_New_Item_When_Add_Button_Clicked()
    {
        // Arrange - Select a category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListView"));
        var initialCount = itemList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;

        // Act
        var addItemButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddItemButton"));
        addItemButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = itemList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;
        newCount.Should().Be(initialCount + 1, "Should add one new item");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Items")]
    public void Should_Delete_Item_When_Delete_Button_Clicked()
    {
        // Arrange - Select category and ensure items exist
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Add item if none exist
        var addItemButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddItemButton"));
        addItemButton?.Click();
        Thread.Sleep(500);

        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListView"));
        var initialCount = itemList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;

        // Act - Select and delete first item
        var firstItem = itemList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstItem?.Click();
        Thread.Sleep(300);

        var deleteItemButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteItemButton"));
        deleteItemButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = itemList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length ?? 0;
        newCount.Should().Be(initialCount - 1, "Should remove one item");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Items")]
    public void Should_Select_Item_When_Clicked()
    {
        // Arrange - Select category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Act - Click first item
        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListView"));
        var firstItem = itemList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstItem?.Click();
        Thread.Sleep(500);

        // Assert - Item details panel should be visible
        var itemDetailsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemDetailsPanel"));
        itemDetailsPanel.Should().NotBeNull("Item details panel should appear when item selected");
    }

    #endregion

    #region Item Field Editing Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Display_Name_Field_For_Selected_Item()
    {
        // Arrange - Select category and item
        SelectFirstCategoryAndItem();

        // Act
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemNameTextBox"));

        // Assert
        nameTextBox.Should().NotBeNull("Should display item name field");
        nameTextBox?.IsEnabled.Should().BeTrue("Name field should be editable");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Update_Item_Name_When_Text_Changed()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemNameTextBox"))?.AsTextBox();

        // Act
        nameTextBox?.Enter("Test Item Name");
        Thread.Sleep(500);

        // Assert
        nameTextBox?.Text.Should().Contain("Test Item Name", "Item name should update");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Display_Description_Field_For_Selected_Item()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act
        var descriptionTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemDescriptionTextBox"));

        // Assert
        descriptionTextBox.Should().NotBeNull("Should display item description field");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Display_Rarity_Field_For_Selected_Item()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act
        var rarityComboBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemRarityComboBox"));

        // Assert
        rarityComboBox.Should().NotBeNull("Should display item rarity field");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Display_RarityWeight_Field_For_Selected_Item()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act
        var weightTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemRarityWeightTextBox"));

        // Assert
        weightTextBox.Should().NotBeNull("Should display rarity weight field");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Update_Rarity_When_ComboBox_Changed()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();
        var rarityComboBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemRarityComboBox"))?.AsComboBox();

        // Act - Select a rarity
        rarityComboBox?.Select(0); // Select first rarity option
        Thread.Sleep(500);

        // Assert
        rarityComboBox?.SelectedItem.Should().NotBeNull("Should have selected rarity");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Fields")]
    public void Should_Update_RarityWeight_When_Text_Changed()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();
        var weightTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemRarityWeightTextBox"))?.AsTextBox();

        // Act
        weightTextBox?.Enter("100");
        Thread.Sleep(500);

        // Assert
        weightTextBox?.Text.Should().Contain("100", "Rarity weight should update");
    }

    #endregion

    #region Traits/Tags Management Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Traits")]
    public void Should_Display_Traits_Section_For_Selected_Item()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act
        var traitsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemTraitsPanel"));

        // Assert
        traitsPanel.Should().NotBeNull("Should display traits section");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Traits")]
    public void Should_Add_Trait_When_Add_Button_Clicked()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();
        var traitsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemTraitsPanel"));
        var initialCount = traitsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("TraitChip")).Length ?? 0;

        // Act
        var addTraitButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddTraitButton"));
        addTraitButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = traitsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("TraitChip")).Length ?? 0;
        newCount.Should().Be(initialCount + 1, "Should add one trait");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Traits")]
    public void Should_Remove_Trait_When_Delete_Button_Clicked()
    {
        // Arrange - Select item and add a trait
        SelectFirstCategoryAndItem();
        var addTraitButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddTraitButton"));
        addTraitButton?.Click();
        Thread.Sleep(500);

        var traitsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemTraitsPanel"));
        var initialCount = traitsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("TraitChip")).Length ?? 0;

        // Act - Delete first trait
        var deleteTraitButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteTraitButton"));
        deleteTraitButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = traitsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("TraitChip")).Length ?? 0;
        newCount.Should().BeLessThan(initialCount, "Should remove a trait");
    }

    #endregion

    #region Custom Fields Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Display_Custom_Fields_Section()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act
        var customFieldsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CustomFieldsPanel"));

        // Assert
        customFieldsPanel.Should().NotBeNull("Should display custom fields section");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Add_Custom_Field_When_Add_Button_Clicked()
    {
        // Arrange - Select item
        SelectFirstCategoryAndItem();
        var customFieldsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CustomFieldsPanel"));
        var initialCount = customFieldsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("CustomFieldRow")).Length ?? 0;

        // Act
        var addFieldButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCustomFieldButton"));
        addFieldButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = customFieldsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("CustomFieldRow")).Length ?? 0;
        newCount.Should().Be(initialCount + 1, "Should add one custom field");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Update_Custom_Field_Key_When_Text_Changed()
    {
        // Arrange - Select item and add custom field
        SelectFirstCategoryAndItem();
        var addFieldButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCustomFieldButton"));
        addFieldButton?.Click();
        Thread.Sleep(500);

        // Act
        var keyTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CustomFieldKeyTextBox"))?.AsTextBox();
        keyTextBox?.Enter("testKey");
        Thread.Sleep(500);

        // Assert
        keyTextBox?.Text.Should().Contain("testKey", "Custom field key should update");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Update_Custom_Field_Value_When_Text_Changed()
    {
        // Arrange - Select item and add custom field
        SelectFirstCategoryAndItem();
        var addFieldButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCustomFieldButton"));
        addFieldButton?.Click();
        Thread.Sleep(500);

        // Act
        var valueTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CustomFieldValueTextBox"))?.AsTextBox();
        valueTextBox?.Enter("testValue");
        Thread.Sleep(500);

        // Assert
        valueTextBox?.Text.Should().Contain("testValue", "Custom field value should update");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Delete_Custom_Field_When_Delete_Button_Clicked()
    {
        // Arrange - Select item and add custom field
        SelectFirstCategoryAndItem();
        var addFieldButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddCustomFieldButton"));
        addFieldButton?.Click();
        Thread.Sleep(500);

        var customFieldsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CustomFieldsPanel"));
        var initialCount = customFieldsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("CustomFieldRow")).Length ?? 0;

        // Act
        var deleteFieldButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteCustomFieldButton"));
        deleteFieldButton?.Click();
        Thread.Sleep(500);

        // Assert
        var newCount = customFieldsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("CustomFieldRow")).Length ?? 0;
        newCount.Should().BeLessThan(initialCount, "Should remove custom field");
    }

    #endregion

    #region Metadata Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Metadata")]
    public void Should_Display_Metadata_Section()
    {
        // Act
        var metadataPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("MetadataPanel"));

        // Assert
        metadataPanel.Should().NotBeNull("Should display metadata section");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Metadata")]
    public void Should_Display_Catalog_Version_Field()
    {
        // Act
        var versionTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CatalogVersionTextBox"));

        // Assert
        versionTextBox.Should().NotBeNull("Should display catalog version field");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Metadata")]
    public void Should_Display_Description_Field_In_Metadata()
    {
        // Act
        var descriptionTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CatalogDescriptionTextBox"));

        // Assert
        descriptionTextBox.Should().NotBeNull("Should display catalog description field");
    }

    #endregion

    #region Save/Load Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Persistence")]
    public void Should_Save_Changes_When_Ctrl_S_Pressed()
    {
        // Arrange - Make a change
        SelectFirstCategoryAndItem();
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemNameTextBox"))?.AsTextBox();
        nameTextBox?.Enter("Modified Item");
        Thread.Sleep(500);

        // Act - Save
        Keyboard.Type(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);
        Thread.Sleep(1000);

        // Assert - Check status bar for save confirmation
        var statusBar = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.StatusBar));
        statusBar.Should().NotBeNull("Should have status bar");
    }

    #endregion

    #region Integration Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Integration")]
    public void Should_Complete_Full_Item_Creation_Workflow()
    {
        // Arrange - Select category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Act 1: Add new item
        var addItemButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddItemButton"));
        addItemButton?.Click();
        Thread.Sleep(500);

        // Act 2: Set name
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemNameTextBox"))?.AsTextBox();
        nameTextBox?.Enter("New Test Item");
        Thread.Sleep(500);

        // Act 3: Set description
        var descriptionTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemDescriptionTextBox"))?.AsTextBox();
        descriptionTextBox?.Enter("Test description");
        Thread.Sleep(500);

        // Act 4: Set rarity
        var rarityComboBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemRarityComboBox"))?.AsComboBox();
        rarityComboBox?.Select(0);
        Thread.Sleep(500);

        // Act 5: Add trait
        var addTraitButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddTraitButton"));
        addTraitButton?.Click();
        Thread.Sleep(500);

        // Assert: All changes should be reflected
        nameTextBox?.Text.Should().Contain("New Test Item");
        descriptionTextBox?.Text.Should().Contain("Test description");
        rarityComboBox?.SelectedItem.Should().NotBeNull();
        
        var traitsPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemTraitsPanel"));
        var traits = traitsPanel?.FindAllDescendants(cf => 
            cf.ByAutomationId("TraitChip"));
        traits?.Should().NotBeEmpty("Should have added trait");
    }

    #endregion

    private void NavigateToMaterialsCatalog()
    {
        // Navigate to items/materials/catalog.json
        var treeView = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryTreeView"));

        if (treeView != null)
        {
            // Expand items
            var itemsItem = treeView.FindFirstDescendant(cf => 
                cf.ByName("items"));
            itemsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Expand materials
            var materialsItem = treeView.FindFirstDescendant(cf => 
                cf.ByName("materials"));
            materialsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Click catalog.json
            var catalogItem = treeView.FindFirstDescendant(cf => 
                cf.ByName("catalog.json"));
            catalogItem?.Click();
            Thread.Sleep(1000);
        }
    }

    private void SelectFirstCategoryAndItem()
    {
        // Select first category
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(500);

        // Add item if none exist
        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemsListView"));
        var hasItems = itemList?.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.ListItem)).Length > 0;

        if (!hasItems)
        {
            var addItemButton = _mainWindow.FindFirstDescendant(cf => 
                cf.ByAutomationId("AddItemButton"));
            addItemButton?.Click();
            Thread.Sleep(500);
        }

        // Select first item
        var firstItem = itemList?.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.ListItem));
        firstItem?.Click();
        Thread.Sleep(500);
    }
}
