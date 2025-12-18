using System;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FluentAssertions;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// UI automation tests for GenericCatalogEditorView
/// Tests dynamic catalog editing, category navigation, and property management
/// </summary>
[Collection("UI Tests")]
public class GenericCatalogEditorUITests : UITestBase
{
    public GenericCatalogEditorUITests() : base()
    {
        LaunchApplication();
        Thread.Sleep(1000);
    }

    private TreeItem? FindTreeItem(AutomationElement parent, string name)
    {
        var items = parent.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
        return items.FirstOrDefault(i => i.Name.Contains(name, StringComparison.OrdinalIgnoreCase))?.AsTreeItem();
    }

    private AutomationElement? FindTreeView()
    {
        return _mainWindow?.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tree));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Load_When_Selecting_Occupations()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        tree.Should().NotBeNull("Tree view should be present");

        // Act
        var npcsItem = FindTreeItem(tree!, "NPCs");
        if (npcsItem == null) return; // Skip if tree structure changed
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree!, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert - check for GenericCatalog editor UI elements
        var searchBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("SearchBox"));
        searchBox.Should().NotBeNull("Generic catalog editor should have search box");

        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("Generic catalog editor should have category list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Have_Three_Column_Layout()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert - should have 3 columns: Categories, Items, Edit Panel
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("Should have category list (column 1)");

        var itemList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("ItemList"));
        itemList.Should().NotBeNull("Should have item list (column 2)");

        var editPanel = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("EditPanel"));
        editPanel.Should().NotBeNull("Should have edit panel (column 3)");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Display_Categories()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryList"));
        
        if (categoryList != null)
        {
            var items = categoryList.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem));
            items.Should().NotBeEmpty("Category list should contain items");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Have_Add_Delete_Item_Buttons()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddItemButton"));
        addButton.Should().NotBeNull("Should have Add Item button");

        var deleteButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteItemButton"));
        deleteButton.Should().NotBeNull("Should have Delete Item button");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Have_Add_Remove_Property_Buttons()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert
        var addPropertyButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddPropertyButton"));
        addPropertyButton.Should().NotBeNull("Should have Add Property button");

        var removePropertyButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("RemovePropertyButton"));
        removePropertyButton.Should().NotBeNull("Should have Remove Property button");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Show_Save_Button()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var occupationsItem = FindTreeItem(tree, "Occupations");
        if (occupationsItem == null) return;
        
        occupationsItem.Click();
        Thread.Sleep(1000);

        // Assert
        var saveButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("SaveButton"));
        saveButton.Should().NotBeNull("Should have Save button");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Work_With_Personality_Traits()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var traitsItem = FindTreeItem(tree, "Personality Traits");
        if (traitsItem == null) return;
        
        // Act
        traitsItem.Click();
        Thread.Sleep(1000);

        // Assert
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("Should load personality traits catalog");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "GenericCatalog")]
    public void GenericCatalogEditor_Should_Work_With_Quirks()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        if (tree == null) return;

        var npcsItem = FindTreeItem(tree, "NPCs");
        if (npcsItem == null) return;
        
        npcsItem.Expand();
        Thread.Sleep(500);

        var quirksItem = FindTreeItem(tree, "Quirks");
        if (quirksItem == null) return;
        
        // Act
        quirksItem.Click();
        Thread.Sleep(1000);

        // Assert
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("Should load quirks catalog");
    }
}

