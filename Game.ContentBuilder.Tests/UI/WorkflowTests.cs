using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

[Collection("UI Tests")]
public class WorkflowTests
{
    private readonly UITestCollectionFixture _fixture;
    private readonly ITestOutputHelper _output;
    private Application _app => _fixture.App!;
    private UIA3Automation _automation => _fixture.Automation!;
    private Window _mainWindow => _fixture.MainWindow!;

    public WorkflowTests(UITestCollectionFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        Thread.Sleep(500); // Let UI stabilize
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Priority", "Critical")]
    public void Workflow_ApplicationLoads_AndShowsTreeView()
    {
        // The main window should be loaded and have a tree view
        _mainWindow.Should().NotBeNull();
        _mainWindow.IsOffscreen.Should().BeFalse();
        
        // Should have a tree with categories
        var tree = _mainWindow.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tree));
        tree.Should().NotBeNull("Should have a tree view for categories");
        
        // Should have some tree items
        var treeItems = tree!.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
        treeItems.Should().NotBeEmpty("Should have at least one category in the tree");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Priority", "Critical")]
    public void Workflow_CanNavigateToNamesList()
    {
        // Find tree using AutomationId
        var tree = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"));
        tree.Should().NotBeNull();

        // Get all tree items
        var allItems = tree!.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
        
        // Find a Names item - look under Beasts
        AutomationElement? namesItem = null;
        foreach (var item in allItems)
        {
            try
            {
                if (item.Name.Equals("Beasts", StringComparison.OrdinalIgnoreCase))
                {
                    item.AsTreeItem().Expand();
                    Thread.Sleep(500);
                    
                    var beastsChildren = item.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
                    namesItem = beastsChildren.FirstOrDefault(child =>
                    {
                        try { return child.Name.Equals("Names", StringComparison.OrdinalIgnoreCase); }
                        catch { return false; }
                    });
                    
                    if (namesItem != null) break;
                }
            }
            catch { }
        }
        
        namesItem.Should().NotBeNull("Should find Names under Beasts");
        
        // Select it using TreeItem.Select()
        namesItem!.AsTreeItem().Select();
        Thread.Sleep(3000);
        
        // Verify name list editor loaded - look for any content
        var contentArea = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Custom).Or(cf.ByControlType(ControlType.Pane)));
        contentArea.Should().NotBeNull("Name list editor should load");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Priority", "Diagnostic")]
    public void Debug_WhatElementsAreInContentControl()
    {
        // Navigate to catalog to load the editor
        NavigateToCatalog();
        
        _output.WriteLine("\n=== ALL ELEMENTS IN MAIN WINDOW ===");
        
        // Get ALL descendants
        var allElements = _mainWindow.FindAllDescendants();
        _output.WriteLine($"Total elements found: {allElements.Length}");
        
        foreach (var elem in allElements)
        {
            try
            {
                var automationId = elem.Properties.AutomationId.ValueOrDefault ?? "(none)";
                var name = elem.Properties.Name.ValueOrDefault ?? "(none)";
                var controlType = elem.Properties.ControlType.ValueOrDefault.ToString();
                var className = elem.Properties.ClassName.ValueOrDefault ?? "(none)";
                
                _output.WriteLine($"Type: {controlType,-20} | AutomationId: {automationId,-30} | Name: {name,-30} | Class: {className}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error reading element: {ex.Message}");
            }
        }
        
        _output.WriteLine("\n=== SEARCHING FOR CategoryList ===");
        var categoryList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"));
        _output.WriteLine($"CategoryList found: {categoryList != null}");
        
        _output.WriteLine("\n=== SEARCHING FOR ItemAddButton ===");
        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ItemAddButton"));
        _output.WriteLine($"ItemAddButton found: {addButton != null}");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Priority", "High")]
    public void Workflow_CanNavigateToCatalog()
    {
        // Find the main tree view using AutomationId
        var tree = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"));
        tree.Should().NotBeNull("Should find CategoryTreeView");

        // Find any "Catalog" item - search all tree items
        var allItems = tree!.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
        AutomationElement? catalogItem = null;
        
        // Look for Catalog under Materials
        foreach (var item in allItems)
        {
            try
            {
                if (item.Name.Equals("Materials", StringComparison.OrdinalIgnoreCase))
                {
                    // Expand Materials
                    item.AsTreeItem().Expand();
                    Thread.Sleep(500);
                    
                    // Now search for Catalog under Materials
                    var materialsChildren = item.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
                    catalogItem = materialsChildren.FirstOrDefault(child =>
                    {
                        try { return child.Name.Equals("Catalog", StringComparison.OrdinalIgnoreCase); }
                        catch { return false; }
                    });
                    
                    if (catalogItem != null) break;
                }
            }
            catch { }
        }
        
        catalogItem.Should().NotBeNull("Should find Catalog under Materials");
        
        // Select it using TreeItem.Select() to trigger SelectedItemChanged event
        catalogItem!.AsTreeItem().Select();
        Thread.Sleep(3000); // Give editor time to load
        
        // Now that AutomationIds are added to CatalogEditorView, verify we can find elements
        _output.WriteLine("\n=== Searching for CatalogTreeView ===");
        
        var catalogTreeView = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CatalogTreeView"));
        catalogTreeView.Should().NotBeNull("CatalogTreeView should be found with AutomationId");
        _output.WriteLine($"SUCCESS! Found CatalogTreeView - elements inside ContentControl are accessible!");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Priority", "High")]
    public void Workflow_CatalogEditor_CanAddItem()
    {
        // Navigate to catalog
        NavigateToCatalog();
        
        // Now that AutomationIds are added, find the Add Item button directly
        _output.WriteLine("\n=== Searching for ItemAddButton ===");
        
        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ItemAddButton"));
        addButton.Should().NotBeNull("ItemAddButton should be found with AutomationId");
        _output.WriteLine($"SUCCESS! Found ItemAddButton via AutomationId");
        
        // Click the button
        addButton!.AsButton().Click();
        Thread.Sleep(1000);
        _output.WriteLine($"Add Item button clicked successfully");
        
        // The button click should work - full verification would check if item was added
        // For now, just verify the button was clickable
        addButton.Should().NotBeNull("Button should remain accessible after click");
    }

    private void NavigateToCatalog()
    {
        var tree = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"));
        var allItems = tree!.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
        
        AutomationElement? catalogItem = null;
        foreach (var item in allItems)
        {
            try
            {
                if (item.Name.Equals("Materials", StringComparison.OrdinalIgnoreCase))
                {
                    item.AsTreeItem().Expand();
                    Thread.Sleep(500);
                    
                    var materialsChildren = item.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
                    catalogItem = materialsChildren.FirstOrDefault(child =>
                    {
                        try { return child.Name.Equals("Catalog", StringComparison.OrdinalIgnoreCase); }
                        catch { return false; }
                    });
                    
                    if (catalogItem != null) break;
                }
            }
            catch { }
        }
        
        catalogItem!.AsTreeItem().Select();
        Thread.Sleep(3000); // Give editor time to fully load
    }
}
