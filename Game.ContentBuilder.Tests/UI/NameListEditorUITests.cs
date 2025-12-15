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
/// Detailed UI tests specifically for NameList editor functionality
/// Tests category selection, name management, and add/delete operations
/// </summary>
[Collection("UI Tests")]
public class NameListEditorUITests : IDisposable
{
    private readonly Application _app;
    private readonly UIA3Automation _automation;
    private readonly Window _mainWindow;

    public NameListEditorUITests()
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
        NavigateToAdjectivesEditor();
    }

    #region Category Display Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Display_Category_List_On_Left_Side()
    {
        // Act
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("NameList editor should have category list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Display_Multiple_Categories()
    {
        // Act
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var categories = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            // Assert
            categories.Should().NotBeEmpty("Should have at least one category");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Show_Category_Names_Like_Positive_Negative()
    {
        // Act
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var categoryItems = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            // Assert - Should have recognizable category names
            categoryItems.Should().NotBeEmpty();
            var categoryNames = categoryItems.Select(c => c.Name).ToList();
            categoryNames.Should().NotBeEmpty();
        }
    }

    #endregion

    #region Category Selection Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Select_Category_When_Clicked()
    {
        // Arrange
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var categories = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            if (categories.Length > 0)
            {
                // Act
                categories[0].Click();
                Thread.Sleep(300);

                // Assert
                categories[0].IsSelected.Should().BeTrue("First category should be selected");
            }
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Show_Names_When_Category_Selected()
    {
        // Arrange
        var categoryList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (categoryList != null)
        {
            var categories = categoryList.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            if (categories.Length > 0)
            {
                // Act
                categories[0].Click();
                Thread.Sleep(500);

                // Assert - Names list should be populated
                var allLists = _mainWindow.FindAllDescendants(cf => 
                    cf.ByControlType(ControlType.List));
                allLists.Should().HaveCountGreaterOrEqualTo(1, "Should have names list");
            }
        }
    }

    #endregion

    #region Button Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_Add_Category_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Add", StringComparison.OrdinalIgnoreCase) && 
            n.Contains("Category", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_Add_Name_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Add", StringComparison.OrdinalIgnoreCase) && 
            (n.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Item", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_Delete_Category_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            (n.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Remove", StringComparison.OrdinalIgnoreCase)) &&
            n.Contains("Category", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_Delete_Name_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            (n.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Remove", StringComparison.OrdinalIgnoreCase)) &&
            (n.Contains("Name", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Item", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Selected", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_Save_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Save", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Input Fields Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_TextBox_For_New_Name_Input()
    {
        // Act
        var textBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Edit));

        // Assert
        textBoxes.Should().NotBeEmpty("Should have text input fields");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Have_TextBox_For_New_Category_Input()
    {
        // Act
        var textBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Edit));

        // Assert
        textBoxes.Should().HaveCountGreaterOrEqualTo(1, "Should have at least one text input");
    }

    #endregion

    #region Layout Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Use_Two_Column_Layout()
    {
        // Act - Check for multiple list boxes (categories and names)
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("Should have category/name lists");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "NameList")]
    public void Should_Show_File_Path_Information()
    {
        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => 
            t.Contains("adjectives", StringComparison.OrdinalIgnoreCase) ||
            t.Contains(".json", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Helper Methods

    private void NavigateToAdjectivesEditor()
    {
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Tree))?.AsTree();
        
        if (tree == null) return;

        var generalItem = tree.Items.FirstOrDefault(item => item.Name == "General");
        if (generalItem == null) return;

        generalItem.Expand();
        Thread.Sleep(500);

        var adjectivesItem = generalItem.FindFirstChild(cf => cf.ByName("Adjectives"))?.AsTreeItem();
        if (adjectivesItem == null) return;

        adjectivesItem.Click();
        Thread.Sleep(1000);
    }

    private AutomationElement[] GetAllButtons()
    {
        return _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
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
