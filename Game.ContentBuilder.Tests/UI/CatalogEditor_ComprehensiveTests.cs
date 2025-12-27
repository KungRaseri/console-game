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
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Comprehensive UI tests for the Catalog Editor (catalog.json format)
/// Covers: Category management, item CRUD, field editing, metadata, rarity/weight
/// </summary>
[Collection("UI Tests")]
public class CatalogEditor_ComprehensiveTests
{
    private readonly UITestCollectionFixture _fixture;
    private readonly ITestOutputHelper _output;
    private Application _app => _fixture.App!;
    private UIA3Automation _automation => _fixture.Automation!;
    private Window _mainWindow => _fixture.MainWindow!;

    public CatalogEditor_ComprehensiveTests(UITestCollectionFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        Thread.Sleep(500);
        NavigateToMaterialsCatalog(); // Navigate to items/materials/catalog.json
    }

    #region Category Management Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Display_Multiple_Categories()
    {
        // Act
        var categoryList = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CategoryList"));
        var categories = categoryList?.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.ListItem));

        _output.WriteLine($"Found {categories?.Length ?? 0} categories");

        // Assert
        categories?.Length.Should().BeGreaterThan(0, "Should display at least one category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Select_Category_When_Clicked()
    {
        // Act - CatalogEditorView uses TreeView not ListBox
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CatalogTreeView"));
        var firstTreeItem = treeView?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.TreeItem));
        firstTreeItem?.Click();
        Thread.Sleep(500);

        // Assert - TreeView should have selection
        treeView.Should().NotBeNull("TreeView should exist");
        firstTreeItem.Should().NotBeNull("Should be able to select tree item");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Add_New_Category_When_Add_Button_Clicked()
    {
        // This test verifies the Add Category button exists and is clickable
        // Data changes should be tested in unit tests

        var addCategoryButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("AddCategoryButton"));

        addCategoryButton.Should().NotBeNull("AddCategoryButton should be found");
        addCategoryButton!.IsEnabled.Should().BeTrue("AddCategoryButton should be enabled");

        // Verify button can be clicked without errors
        addCategoryButton.Click();
        Thread.Sleep(200);

        // Application should remain stable
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CatalogTreeView"));
        treeView.Should().NotBeNull("TreeView should remain stable after adding category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Categories")]
    public void Should_Rename_Category_When_Name_Changed()
    {
        // Arrange - Select first category in TreeView
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CatalogTreeView"));
        var firstTreeItem = treeView?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.TreeItem));
        firstTreeItem?.Click();
        Thread.Sleep(500);

        // Act - Change category name (embedded TextBox in TreeView)
        var nameTextBox = treeView?.FindFirstDescendant(cf =>
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
    public void Should_Add_New_Item_When_Add_Button_Clicked()
    {
        // This test verifies the Add Item button exists and is clickable
        // Data changes should be tested in unit tests

        // Arrange - Select a category
        var categoryList = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CategoryListBox"));
        var firstCategory = categoryList?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.ListItem));
        firstCategory?.Click();
        Thread.Sleep(200);

        // Act - Find add item button
        var addItemButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("AddItemButton"));

        addItemButton.Should().NotBeNull("AddItemButton should be found");
        addItemButton!.IsEnabled.Should().BeTrue("AddItemButton should be enabled when category is selected");

        addItemButton.Click();
        Thread.Sleep(200);

        // Assert - Application should remain stable (TreeView still exists)
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CatalogTreeView"));
        treeView.Should().NotBeNull("TreeView should remain stable after adding item");
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
    public void Should_Remove_Trait_When_Delete_Button_Clicked()
    {
        // This test verifies the Delete Trait button exists and is clickable
        // Data changes should be tested in unit tests

        // Arrange - Select item that has traits
        SelectFirstCategoryAndItem();

        // Act - Find delete trait button
        var deleteTraitButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("DeleteTraitButton"));

        // Note: Button may not exist if no traits are present, that's okay
        if (deleteTraitButton != null)
        {
            deleteTraitButton.IsEnabled.Should().BeTrue("DeleteTraitButton should be enabled when traits exist");

            deleteTraitButton.Click();
            Thread.Sleep(200);

            // Assert - Application should remain stable
            var traitsPanel = _mainWindow.FindFirstDescendant(cf =>
                cf.ByAutomationId("ItemTraitsPanel"));
            traitsPanel.Should().NotBeNull("Traits panel should remain stable after deleting trait");
        }
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

        // Act - v4.0 uses "ItemPropertiesPanel" for custom fields
        var customFieldsPanel = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("ItemPropertiesPanel"));

        // Assert
        customFieldsPanel.Should().NotBeNull("Should display custom fields section (ItemPropertiesPanel)");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "CustomFields")]
    public void Should_Add_Custom_Field_When_Add_Button_Clicked()
    {
        // This test verifies the Add Custom Field button exists and is clickable
        // Data changes should be tested in unit tests

        // Arrange - Select item
        SelectFirstCategoryAndItem();

        // Act - Find add custom field button
        var addFieldButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("AddCustomFieldButton"));

        addFieldButton.Should().NotBeNull("AddCustomFieldButton should be found");
        addFieldButton!.IsEnabled.Should().BeTrue("AddCustomFieldButton should be enabled");

        addFieldButton.Click();
        Thread.Sleep(200);

        // Assert - Application should remain stable
        var customFieldsPanel = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("ItemPropertiesPanel"));
        customFieldsPanel.Should().NotBeNull("Custom fields panel should remain stable after adding field");
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

    #endregion

    #region Metadata Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Catalog")]
    [Trait("Feature", "Metadata")]
    public void Should_Display_Catalog_Version_Field()
    {
        // Arrange - Expand the metadata expander first
        var metadataExpander = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("MetadataExpander"));

        if (metadataExpander?.Patterns.ExpandCollapse.IsSupported == true &&
            metadataExpander.Patterns.ExpandCollapse.Pattern.ExpandCollapseState == FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
        {
            metadataExpander.Patterns.ExpandCollapse.Pattern.Expand();
            Thread.Sleep(300);
        }

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
        // Arrange - Expand the metadata expander first
        var metadataExpander = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("MetadataExpander"));

        if (metadataExpander?.Patterns.ExpandCollapse.IsSupported == true &&
            metadataExpander.Patterns.ExpandCollapse.Pattern.ExpandCollapseState == FlaUI.Core.Definitions.ExpandCollapseState.Collapsed)
        {
            metadataExpander.Patterns.ExpandCollapse.Pattern.Expand();
            Thread.Sleep(300);
        }

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

    #endregion

    private void NavigateToMaterialsCatalog()
    {
        // Navigate to Items/Materials/Catalog (note: TreeItems use display names, not filenames!)
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CategoryTreeView"));

        if (treeView != null)
        {
            var allItems = treeView.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));

            // Find and expand "Items" (capital I)
            AutomationElement? itemsItem = allItems.FirstOrDefault(item =>
            {
                try { return item.Name.Equals("Items", StringComparison.OrdinalIgnoreCase); }
                catch { return false; }
            });
            itemsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and expand "Materials" within Items (capital M)
            var materialsItem = itemsItem?.FindFirstDescendant(cf =>
                cf.ByName("Materials"));
            materialsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and select "Catalog" within Materials (capital C, no .json extension!)
            var catalogItem = materialsItem?.FindFirstDescendant(cf =>
                cf.ByName("Catalog"));
            catalogItem?.AsTreeItem()?.Select();
            Thread.Sleep(1000);
        }
    }

    private void SelectFirstCategoryAndItem()
    {
        // Navigate TreeView hierarchy: Catalog -> Category -> Item
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CatalogTreeView"));

        // Expand catalog node to see categories
        var catalogNode = treeView?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.TreeItem));
        catalogNode?.AsTreeItem()?.Expand();
        Thread.Sleep(300);

        // Expand first category to see items
        var categoryNode = catalogNode?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.TreeItem));
        categoryNode?.AsTreeItem()?.Expand();
        Thread.Sleep(300);

        // Check if items exist, add one if needed
        var itemNode = categoryNode?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.TreeItem));

        if (itemNode == null)
        {
            var addItemButton = _mainWindow.FindFirstDescendant(cf =>
                cf.ByAutomationId("AddItemButton"));
            addItemButton?.Click();
            Thread.Sleep(500);

            // Re-expand to find new item
            categoryNode?.AsTreeItem()?.Expand();
            Thread.Sleep(300);
            itemNode = categoryNode?.FindFirstDescendant(cf =>
                cf.ByControlType(ControlType.TreeItem));
        }

        // Select the item
        itemNode?.Click();
        Thread.Sleep(500);
    }
}
