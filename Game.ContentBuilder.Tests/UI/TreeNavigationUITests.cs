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
/// UI tests for tree navigation and category browsing in ContentBuilder
/// Tests the main tree view structure and navigation between different file categories
/// </summary>
[Collection("UI Tests")]
public class TreeNavigationUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;
    private readonly Tree _tree;

    public TreeNavigationUITests()
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

        Thread.Sleep(1500);

        _tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Tree))?.AsTree();

        if (_tree == null)
        {
            throw new InvalidOperationException("Tree view not found");
        }
    }

    #region Tree Structure Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Display_Top_Level_Categories()
    {
        // Act
        var rootItems = _tree.Items;

        // Assert
        rootItems.Should().NotBeEmpty("Tree should have root categories");
        rootItems.Length.Should().BeGreaterThanOrEqualTo(3, "Should have at least General, Items, and other categories");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Have_General_Category()
    {
        // Act
        var generalItem = FindTreeItem("General");

        // Assert
        generalItem.Should().NotBeNull("Tree should have 'General' category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Have_Items_Category()
    {
        // Act
        var itemsItem = FindTreeItem("Items");

        // Assert
        itemsItem.Should().NotBeNull("Tree should have 'Items' category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Have_Enemies_Category()
    {
        // Act
        var enemiesItem = FindTreeItem("Enemies");

        // Assert
        enemiesItem.Should().NotBeNull("Tree should have 'Enemies' category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Have_NPCs_Category()
    {
        // Act
        var npcsItem = FindTreeItem("NPCs");

        // Assert
        npcsItem.Should().NotBeNull("Tree should have 'NPCs' category");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Tree_Should_Have_Quests_Category()
    {
        // Act
        var questsItem = FindTreeItem("Quests");

        // Assert
        questsItem.Should().NotBeNull("Tree should have 'Quests' category");
    }

    #endregion

    #region Expand/Collapse Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Expand_General_Category()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();

        // Act
        generalItem!.Expand();
        Thread.Sleep(500);

        // Assert
        generalItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Expanded, "General category should be expanded");
        var children = generalItem.FindAllChildren();
        children.Should().NotBeEmpty("General category should have child items");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Collapse_General_Category()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(300);

        // Act
        generalItem.Collapse();
        Thread.Sleep(300);

        // Assert
        generalItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Collapsed, "General category should be collapsed");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Expand_Items_Category()
    {
        // Arrange
        var itemsItem = FindTreeItem("Items");
        itemsItem.Should().NotBeNull();

        // Act
        itemsItem!.Expand();
        Thread.Sleep(500);

        // Assert
        itemsItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Expanded, "Items category should be expanded");
        var children = itemsItem.FindAllChildren();
        children.Should().NotBeEmpty("Items category should have child items");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Toggle_Expansion_Multiple_Times()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();

        // Act & Assert - Expand/collapse cycle
        for (int i = 0; i < 3; i++)
        {
            generalItem!.Expand();
            Thread.Sleep(200);
            generalItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Expanded, $"Should be expanded on iteration {i}");

            generalItem.Collapse();
            Thread.Sleep(200);
            generalItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Collapsed, $"Should be collapsed on iteration {i}");
        }
    }

    #endregion

    #region Child Item Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void General_Category_Should_Contain_Colors()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        // Act
        var colorsItem = generalItem.FindFirstChild(cf => cf.ByName("Colors"));

        // Assert
        colorsItem.Should().NotBeNull("General category should contain 'Colors' item");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void General_Category_Should_Contain_Adjectives()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        // Act
        var adjectivesItem = generalItem.FindFirstChild(cf => cf.ByName("Adjectives"));

        // Assert
        adjectivesItem.Should().NotBeNull("General category should contain 'Adjectives' item");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void General_Category_Should_Contain_Multiple_Files()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        // Act
        var children = generalItem.FindAllChildren();

        // Assert
        children.Should().HaveCountGreaterThanOrEqualTo(5, "General should have multiple files (Colors, Sounds, Smells, Adjectives, etc.)");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Items_Category_Should_Have_Subcategories()
    {
        // Arrange
        var itemsItem = FindTreeItem("Items");
        itemsItem.Should().NotBeNull();
        itemsItem!.Expand();
        Thread.Sleep(500);

        // Act
        var children = itemsItem.FindAllChildren();

        // Assert
        children.Should().NotBeEmpty("Items category should have subcategories like Weapons, Materials, etc.");
    }

    #endregion

    #region Selection Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Select_File_When_Clicked()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        var colorsItem = generalItem.FindFirstChild(cf => cf.ByName("Colors"))?.AsTreeItem();
        colorsItem.Should().NotBeNull();

        // Act
        colorsItem!.Click();
        Thread.Sleep(500);

        // Assert
        colorsItem.IsSelected.Should().BeTrue("Colors item should be selected");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Load_Editor_When_File_Selected()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        var colorsItem = generalItem.FindFirstChild(cf => cf.ByName("Colors"))?.AsTreeItem();
        colorsItem.Should().NotBeNull();

        // Act
        colorsItem!.Click();
        Thread.Sleep(1000);

        // Assert - Editor should load (check for tabs or other editor elements)
        var tabs = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.TabItem));
        tabs.Should().NotBeEmpty("Editor should load with tabs when file is selected");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Change_Selection_Between_Files()
    {
        // Arrange
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(500);

        // Act & Assert - Select Colors
        var colorsItem = generalItem.FindFirstChild(cf => cf.ByName("Colors"))?.AsTreeItem();
        colorsItem.Should().NotBeNull();
        colorsItem!.Click();
        Thread.Sleep(500);
        colorsItem.IsSelected.Should().BeTrue();

        // Select Sounds
        var soundsItem = generalItem.FindFirstChild(cf => cf.ByName("Sounds"))?.AsTreeItem();
        soundsItem.Should().NotBeNull();
        soundsItem!.Click();
        Thread.Sleep(500);
        soundsItem.IsSelected.Should().BeTrue();
    }

    #endregion

    #region Nested Navigation Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Navigate_To_Nested_File()
    {
        // Arrange
        var itemsItem = FindTreeItem("Items");
        itemsItem.Should().NotBeNull();
        itemsItem!.Expand();
        Thread.Sleep(500);

        var materialsItem = itemsItem.FindFirstChild(cf => cf.ByName("Materials"))?.AsTreeItem();
        if (materialsItem != null)
        {
            materialsItem.Expand();
            Thread.Sleep(500);

            // Act
            var metalsItem = materialsItem.FindFirstChild(cf => cf.ByName("Metals"))?.AsTreeItem();

            // Assert
            metalsItem.Should().NotBeNull("Should be able to navigate to nested Metals file");

            if (metalsItem != null)
            {
                metalsItem.Click();
                Thread.Sleep(500);
                metalsItem.IsSelected.Should().BeTrue("Nested file should be selectable");
            }
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Component", "TreeView")]
    public void Should_Expand_Multiple_Categories_Simultaneously()
    {
        // Arrange & Act
        var generalItem = FindTreeItem("General");
        generalItem.Should().NotBeNull();
        generalItem!.Expand();
        Thread.Sleep(300);

        var itemsItem = FindTreeItem("Items");
        itemsItem.Should().NotBeNull();
        itemsItem!.Expand();
        Thread.Sleep(300);

        // Assert
        generalItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Expanded, "General should remain expanded");
        itemsItem.ExpandCollapseState.Should().Be(FlaUI.Core.Definitions.ExpandCollapseState.Expanded, "Items should be expanded");
    }

    #endregion

    #region Helper Methods

    private TreeItem? FindTreeItem(string itemName)
    {
        return _tree.Items.FirstOrDefault(item => item.Name == itemName);
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
