using System;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// UI automation tests for the ContentBuilder application using FlaUI
/// These tests launch the actual WPF application and interact with it
/// </summary>
public class ContentBuilderUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public ContentBuilderUITests()
    {
        // Build path to the ContentBuilder executable
        // From: Game.ContentBuilder.Tests/bin/Debug/net9.0-windows/
        // To:   Game.ContentBuilder/bin/Debug/net9.0-windows/Game.ContentBuilder.exe
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
                $"ContentBuilder executable not found at: {fullExePath}. " +
                "Please build the Game.ContentBuilder project first.");
        }

        // Launch the application
        _automation = new UIA3Automation();
        _app = Application.Launch(fullExePath);

        // Wait for main window to appear (10 second timeout)
        _mainWindow = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));

        if (_mainWindow == null)
        {
            throw new InvalidOperationException("Main window failed to load within timeout period");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Application_Should_Launch_Successfully()
    {
        // Assert
        _mainWindow.Should().NotBeNull();
        _mainWindow.Title.Should().Be("Game Content Builder");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void MainWindow_Should_Have_TreeView_And_Editor_Area()
    {
        // Assert - main window structure
        _mainWindow.Should().NotBeNull();
        
        // Look for a tree control (category tree)
        var treeControls = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree));
        
        treeControls.Should().NotBeEmpty("Main window should have a tree view for categories");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void TreeView_Should_Contain_General_Category()
    {
        // Arrange - find tree view
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))?.AsTree();
        
        tree.Should().NotBeNull("Tree view should exist");

        // Act - get all tree items
        var treeItems = tree!.Items;
        
        // Assert - should have "General" as one of the top-level categories
        var generalItem = treeItems.FirstOrDefault(item => item.Name == "General");
        generalItem.Should().NotBeNull("Tree should contain 'General' category");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void General_Category_Should_Expand_And_Show_Children()
    {
        // Arrange - find General category
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))?.AsTree();
        
        var generalItem = tree!.Items.FirstOrDefault(item => item.Name == "General");
        generalItem.Should().NotBeNull();

        // Act - expand the General category
        generalItem!.Expand();
        
        // Wait a moment for UI to update
        Thread.Sleep(500);

        // Get children
        var children = generalItem.Items;

        // Assert - should have child items (Colors, Smells, Sounds, etc.)
        children.Should().NotBeEmpty("General category should have child items");
        children.Length.Should().BeGreaterThanOrEqualTo(9, "General category should have at least 9 files");
        
        // Check for specific expected items
        var colorItem = children.FirstOrDefault(item => item.Name == "Colors");
        colorItem.Should().NotBeNull("General category should contain 'Colors' item");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Clicking_Colors_Should_Load_Editor()
    {
        // Arrange - expand General and find Colors
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))?.AsTree();
        
        var generalItem = tree!.Items.FirstOrDefault(item => item.Name == "General");
        generalItem!.Expand();
        Thread.Sleep(500);

        var colorsItem = generalItem.Items.FirstOrDefault(item => item.Name == "Colors");
        colorsItem.Should().NotBeNull();

        // Act - click on Colors
        colorsItem!.Click();
        
        // Wait for editor to load
        Thread.Sleep(1000);

        // Assert - check if editor area has content (look for any tab control which indicates editor loaded)
        var tabControls = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tab));
        
        tabControls.Should().NotBeEmpty("Editor should load with tab control for HybridArray structure");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Items_Category_Should_Have_Subcategories()
    {
        // Arrange - find Items category
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))?.AsTree();
        
        var itemsCategory = tree!.Items.FirstOrDefault(item => item.Name == "Items");
        itemsCategory.Should().NotBeNull("Tree should contain 'Items' category");

        // Act - expand Items
        itemsCategory!.Expand();
        Thread.Sleep(500);

        var children = itemsCategory.Items;

        // Assert - should have subcategories (Weapons, Armor, Consumables, etc.)
        children.Should().NotBeEmpty("Items category should have subcategories");
        children.Length.Should().BeGreaterThanOrEqualTo(4, "Items should have at least 4 subcategories");
        
        var weaponsItem = children.FirstOrDefault(item => item.Name == "Weapons");
        weaponsItem.Should().NotBeNull("Items should contain 'Weapons' subcategory");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void All_Main_Categories_Should_Be_Present()
    {
        // Arrange
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree))?.AsTree();
        
        tree.Should().NotBeNull();
        var topLevelItems = tree!.Items;

        // Assert - verify all 5 main categories
        var expectedCategories = new[] { "General", "Items", "Enemies", "NPCs", "Quests" };
        
        foreach (var categoryName in expectedCategories)
        {
            var category = topLevelItems.FirstOrDefault(item => item.Name == categoryName);
            category.Should().NotBeNull($"Tree should contain '{categoryName}' category");
        }

        topLevelItems.Length.Should().Be(5, "Tree should have exactly 5 top-level categories");
    }

    [Fact]
    [Trait("Category", "UI")]
    public void Window_Should_Be_Resizable()
    {
        // Arrange - get initial size
        var initialWidth = _mainWindow.BoundingRectangle.Width;
        var initialHeight = _mainWindow.BoundingRectangle.Height;

        // Act - resize window
        _mainWindow.Patterns.Transform.Pattern.Resize(1200, 800);
        Thread.Sleep(300);

        // Assert
        var newWidth = _mainWindow.BoundingRectangle.Width;
        var newHeight = _mainWindow.BoundingRectangle.Height;

        ((double)newWidth).Should().BeApproximately(1200, 50, "Window width should be resizable");
        ((double)newHeight).Should().BeApproximately(800, 50, "Window height should be resizable");
    }

    public void Dispose()
    {
        // Close the application and clean up
        try
        {
            // Try graceful shutdown first
            _app?.Close(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // If graceful shutdown fails, force kill
            _app?.Kill();
        }
        finally
        {
            _automation?.Dispose();
        }
    }
}
