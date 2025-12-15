using System;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Comprehensive UI tests for all ContentBuilder editor pages
/// Tests each editor type: HybridArray, NameList, FlatItem, and ItemPrefix/Suffix
/// </summary>
[Collection("UI Tests")]
public class AllEditorsUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public AllEditorsUITests()
    {
        var testAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        var exePath = Path.Combine(
            testAssemblyPath,
            "..", "..", "..", "..",
            "Game.ContentBuilder", "bin", "Debug", "net9.0-windows",
            "Game.ContentBuilder.exe"
        );

        var fullExePath = Path.GetFullPath(exePath);

        if (!File.Exists(fullExePath))
        {
            throw new FileNotFoundException(
                $"ContentBuilder executable not found at: {fullExePath}");
        }

        _automation = new UIA3Automation();
        _app = Application.Launch(fullExePath);
        _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(15));

        if (_mainWindow == null)
        {
            throw new InvalidOperationException("Main window failed to load");
        }

        // Wait for UI to stabilize
        Thread.Sleep(1000);
    }

    #region HybridArray Editor Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void HybridArrayEditor_Should_Load_When_Selecting_Colors()
    {
        // Arrange
        var tree = FindTreeView();
        
        // Act
        var generalItem = FindTreeItem(tree, "General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);
        
        var colorsItem = FindTreeItem(tree, "Colors");
        colorsItem.Should().NotBeNull();
        colorsItem!.Click();
        Thread.Sleep(1000);

        // Assert
        // HybridArray editor should show tabs: Items, Components, Patterns
        var tabs = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.TabItem));
        
        tabs.Should().NotBeEmpty();
        var tabNames = tabs.Select(t => t.Name).ToList();
        tabNames.Should().Contain("Items");
        tabNames.Should().Contain("Components");
        tabNames.Should().Contain("Patterns");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void HybridArrayEditor_Should_Show_Item_Count_Chips()
    {
        // Arrange & Act
        NavigateToEditor("General", "Colors");

        // Assert
        // Should show chips with counts (Items: X, Components: Y, Patterns: Z)
        var textBlocks = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));
        
        var chipTexts = textBlocks.Select(t => t.Name).ToList();
        chipTexts.Should().Contain(t => t.Contains("Items:"));
        chipTexts.Should().Contain(t => t.Contains("Components:"));
        chipTexts.Should().Contain(t => t.Contains("Patterns:"));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void HybridArrayEditor_Items_Tab_Should_Have_Add_Remove_Buttons()
    {
        // Arrange & Act
        NavigateToEditor("General", "Colors");
        
        // Click Items tab
        ClickTab("Items");

        // Assert
        var buttons = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
        
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain("Add Item");
        buttonNames.Should().Contain("Delete Selected");
    }

    #endregion

    #region NameList Editor Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void NameListEditor_Should_Load_When_Selecting_Adjectives()
    {
        // Arrange & Act
        NavigateToEditor("General", "Adjectives");
        Thread.Sleep(1000);

        // Assert
        // NameList editor should show categories and names
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));
        
        listBoxes.Should().NotBeEmpty("NameList editor should have list boxes");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void NameListEditor_Should_Have_Category_Selection()
    {
        // Arrange & Act
        NavigateToEditor("General", "Adjectives");

        // Assert
        // Should have a list for categories (left side)
        var lists = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));
        
        lists.Should().HaveCountGreaterThanOrEqualTo(1, "Should have at least category list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void NameListEditor_Should_Have_Add_Category_And_Name_Buttons()
    {
        // Arrange & Act
        NavigateToEditor("General", "Adjectives");

        // Assert
        var buttons = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
        
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Add") || n.Contains("Category") || n.Contains("Name"));
    }

    #endregion

    #region FlatItem Editor Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void FlatItemEditor_Should_Load_When_Selecting_Metals()
    {
        // Arrange & Act
        NavigateToEditor("Items", "Materials", "Metals");
        Thread.Sleep(1000);

        // Assert
        // FlatItem editor should load without errors
        _mainWindow.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void FlatItemEditor_Should_Show_Item_List()
    {
        // Arrange & Act
        NavigateToEditor("Items", "Materials", "Metals");

        // Assert
        var lists = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));
        
        lists.Should().NotBeEmpty("FlatItem editor should have an item list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void FlatItemEditor_Should_Have_Add_Delete_Buttons()
    {
        // Arrange & Act
        NavigateToEditor("Items", "Materials", "Metals");

        // Assert
        var buttons = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
        
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Add") || n.Contains("Delete"));
    }

    #endregion

    #region Navigation Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Navigation")]
    public void Should_Navigate_Between_Different_Editors()
    {
        // Act & Assert - Navigate to HybridArray editor
        NavigateToEditor("General", "Colors");
        Thread.Sleep(500);
        var tabs1 = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem));
        tabs1.Should().NotBeEmpty("HybridArray should have tabs");

        // Navigate to NameList editor
        NavigateToEditor("General", "Adjectives");
        Thread.Sleep(500);
        var lists = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.List));
        lists.Should().NotBeEmpty("NameList should have lists");

        // Navigate back to HybridArray
        NavigateToEditor("General", "Sounds");
        Thread.Sleep(500);
        var tabs2 = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.TabItem));
        tabs2.Should().NotBeEmpty("HybridArray should still have tabs");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Navigation")]
    public void Should_Expand_And_Collapse_Tree_Categories()
    {
        // Arrange
        var tree = FindTreeView();
        var itemsNode = FindTreeItem(tree, "Items");
        itemsNode.Should().NotBeNull();

        // Act - Expand
        itemsNode!.Expand();
        Thread.Sleep(300);

        // Assert - Should see child nodes
        var childNodes = itemsNode.FindAllChildren();
        childNodes.Should().NotBeEmpty("Items should have child categories");

        // Act - Collapse
        itemsNode.Collapse();
        Thread.Sleep(300);

        // Assert - Collapsed state
        itemsNode.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Collapsed, "Items node should be collapsed after Collapse()");
    }

    #endregion

    #region Status Bar Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "StatusBar")]
    public void StatusBar_Should_Update_When_Selecting_Items()
    {
        // Arrange
        NavigateToEditor("General", "Colors");
        Thread.Sleep(500);

        // Assert
        var statusTexts = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));
        
        var statusMessages = statusTexts.Select(t => t.Name).ToList();
        statusMessages.Should().Contain(m => 
            m.Contains("Colors") || m.Contains("general/colors.json"));
    }

    #endregion

    #region Save/Load Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "SaveLoad")]
    public void Save_Button_Should_Be_Present_In_All_Editors()
    {
        // Test HybridArray
        NavigateToEditor("General", "Colors");
        AssertSaveButtonExists();

        // Test NameList
        NavigateToEditor("General", "Adjectives");
        AssertSaveButtonExists();

        // Test FlatItem
        NavigateToEditor("Items", "Materials", "Metals");
        AssertSaveButtonExists();
    }

    #endregion

    #region Helper Methods

    private Tree FindTreeView()
    {
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Tree))?.AsTree();
        
        tree.Should().NotBeNull("Tree view should exist");
        return tree!;
    }

    private TreeItem? FindTreeItem(Tree tree, string itemName)
    {
        return tree.Items.FirstOrDefault(item => item.Name == itemName);
    }

    private void NavigateToEditor(params string[] path)
    {
        var tree = FindTreeView();
        TreeItem? currentItem = null;

        for (int i = 0; i < path.Length; i++)
        {
            var itemName = path[i];
            
            if (i == 0)
            {
                currentItem = FindTreeItem(tree, itemName);
            }
            else
            {
                currentItem = currentItem?.FindFirstChild(cf => 
                    cf.ByName(itemName))?.AsTreeItem();
            }

            currentItem.Should().NotBeNull($"Tree item '{itemName}' should exist");
            
            if (i < path.Length - 1)
            {
                currentItem!.Expand();
                Thread.Sleep(300);
            }
            else
            {
                currentItem!.Click();
                Thread.Sleep(500);
            }
        }
    }

    private void ClickTab(string tabName)
    {
        var tabs = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.TabItem));
        
        var tab = tabs.FirstOrDefault(t => t.Name == tabName);
        tab.Should().NotBeNull($"Tab '{tabName}' should exist");
        tab!.Click();
        Thread.Sleep(300);
    }

    private void AssertSaveButtonExists()
    {
        var buttons = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
        
        var saveButton = buttons.FirstOrDefault(b => 
            b.Name.Contains("Save", StringComparison.OrdinalIgnoreCase));
        
        saveButton.Should().NotBeNull("Save button should exist in editor");
    }

    #endregion

    public void Dispose()
    {
        try
        {
            _app?.Close();
            _automation?.Dispose();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
