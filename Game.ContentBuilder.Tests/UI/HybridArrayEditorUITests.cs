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
/// Detailed UI tests specifically for HybridArray editor functionality
/// Tests Items, Components, and Patterns tabs with add/edit/delete operations
/// </summary>
[Collection("UI Tests")]
public class HybridArrayEditorUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public HybridArrayEditorUITests()
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
        NavigateToColorsEditor();
    }

    #region Tab Navigation Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Should_Show_Three_Tabs_Items_Components_Patterns()
    {
        // Arrange & Act
        var tabs = GetAllTabItems();

        // Assert
        tabs.Should().HaveCountGreaterThanOrEqualTo(3, "Should have Items, Components, and Patterns tabs");
        var tabNames = tabs.Select(t => t.Name).ToList();
        tabNames.Should().Contain("Items");
        tabNames.Should().Contain("Components");
        tabNames.Should().Contain("Patterns");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Should_Switch_Between_Tabs_Without_Errors()
    {
        // Act & Assert
        ClickTab("Items");
        Thread.Sleep(300);
        AssertTabIsActive("Items");

        ClickTab("Components");
        Thread.Sleep(300);
        AssertTabIsActive("Components");

        ClickTab("Patterns");
        Thread.Sleep(300);
        AssertTabIsActive("Patterns");

        // Switch back
        ClickTab("Items");
        Thread.Sleep(300);
        AssertTabIsActive("Items");
    }

    #endregion

    #region Items Tab Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Items_Tab_Should_Display_List_Of_Items()
    {
        // Arrange
        ClickTab("Items");
        Thread.Sleep(500);

        // Act
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("Items tab should have a list box");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Items_Tab_Should_Have_Add_And_Delete_Buttons()
    {
        // Arrange
        ClickTab("Items");
        Thread.Sleep(300);

        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Add", StringComparison.OrdinalIgnoreCase));
        buttonNames.Should().Contain(n => n.Contains("Delete", StringComparison.OrdinalIgnoreCase) || 
                                          n.Contains("Remove", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Items_Tab_Should_Show_Item_Count()
    {
        // Arrange
        ClickTab("Items");
        Thread.Sleep(300);

        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => t.Contains("Items:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Should_Select_Item_In_List()
    {
        // Arrange
        ClickTab("Items");
        Thread.Sleep(500);

        // Act
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));
        
        if (listBox != null)
        {
            var items = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));
            
            if (items.Length > 0)
            {
                // Assert - First item should be selectable
                items[0].Click();
                Thread.Sleep(200);
                var selectionPattern = items[0].Patterns.SelectionItem.Pattern;
                selectionPattern.IsSelected.Value.Should().BeTrue("Item should be selected");
            }
        }
    }

    #endregion

    #region Components Tab Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Components_Tab_Should_Display_List_Of_Components()
    {
        // Arrange
        ClickTab("Components");
        Thread.Sleep(500);

        // Act
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("Components tab should have a list box");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Components_Tab_Should_Have_Add_And_Delete_Buttons()
    {
        // Arrange
        ClickTab("Components");
        Thread.Sleep(300);

        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Add", StringComparison.OrdinalIgnoreCase));
        buttonNames.Should().Contain(n => n.Contains("Delete", StringComparison.OrdinalIgnoreCase) || 
                                          n.Contains("Remove", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Components_Tab_Should_Show_Component_Count()
    {
        // Arrange
        ClickTab("Components");
        Thread.Sleep(300);

        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => t.Contains("Components:", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Patterns Tab Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Patterns_Tab_Should_Display_List_Of_Patterns()
    {
        // Arrange
        ClickTab("Patterns");
        Thread.Sleep(500);

        // Act
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("Patterns tab should have a list box");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Patterns_Tab_Should_Have_Add_And_Delete_Buttons()
    {
        // Arrange
        ClickTab("Patterns");
        Thread.Sleep(300);

        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Add", StringComparison.OrdinalIgnoreCase));
        buttonNames.Should().Contain(n => n.Contains("Delete", StringComparison.OrdinalIgnoreCase) || 
                                          n.Contains("Remove", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Patterns_Tab_Should_Show_Pattern_Count()
    {
        // Arrange
        ClickTab("Patterns");
        Thread.Sleep(300);

        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => t.Contains("Patterns:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Patterns_Tab_Should_Have_Preview_Button()
    {
        // Arrange
        ClickTab("Patterns");
        Thread.Sleep(300);

        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => n.Contains("Preview", StringComparison.OrdinalIgnoreCase) ||
                                          n.Contains("Generate", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Save/Load Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Should_Have_Save_Button_In_All_Tabs()
    {
        // Test Items tab
        ClickTab("Items");
        Thread.Sleep(200);
        AssertSaveButtonExists();

        // Test Components tab
        ClickTab("Components");
        Thread.Sleep(200);
        AssertSaveButtonExists();

        // Test Patterns tab
        ClickTab("Patterns");
        Thread.Sleep(200);
        AssertSaveButtonExists();
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "HybridArray")]
    public void Should_Show_File_Path_In_Status_Or_Title()
    {
        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => 
            t.Contains("colors", StringComparison.OrdinalIgnoreCase) ||
            t.Contains(".json", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Helper Methods

    private void NavigateToColorsEditor()
    {
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Tree))?.AsTree();
        
        if (tree == null) return;

        var generalItem = tree.Items.FirstOrDefault(item => item.Name == "General");
        if (generalItem == null) return;

        generalItem.Expand();
        Thread.Sleep(500);

        var colorsItem = generalItem.FindFirstChild(cf => cf.ByName("Colors"))?.AsTreeItem();
        if (colorsItem == null) return;

        colorsItem.Click();
        Thread.Sleep(1000);
    }

    private AutomationElement[] GetAllTabItems()
    {
        return _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.TabItem));
    }

    private AutomationElement[] GetAllButtons()
    {
        return _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
    }

    private void ClickTab(string tabName)
    {
        var tabs = GetAllTabItems();
        var tab = tabs.FirstOrDefault(t => t.Name == tabName);
        
        if (tab != null)
        {
            tab.Click();
            Thread.Sleep(300);
        }
    }

    private void AssertTabIsActive(string tabName)
    {
        var tabs = GetAllTabItems();
        var tab = tabs.FirstOrDefault(t => t.Name == tabName);
        
        tab.Should().NotBeNull($"Tab '{tabName}' should exist");
        // Tab should be visible when active
        tab!.IsOffscreen.Should().BeFalse($"Tab '{tabName}' should be visible");
    }

    private void AssertSaveButtonExists()
    {
        var buttons = GetAllButtons();
        var saveButton = buttons.FirstOrDefault(b => 
            b.Name.Contains("Save", StringComparison.OrdinalIgnoreCase));
        
        saveButton.Should().NotBeNull("Save button should exist");
    }

    #endregion

    public void Dispose()
    {
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
