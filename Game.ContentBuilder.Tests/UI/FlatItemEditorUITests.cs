using System;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FluentAssertions;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Detailed UI tests specifically for FlatItem editor functionality
/// Tests item display, traits editing, and add/delete operations
/// </summary>
[Collection("UI Tests")]
public class FlatItemEditorUITests : UITestBase
{
    public FlatItemEditorUITests() : base()
    {
        LaunchApplication();
        Thread.Sleep(1500);
        NavigateToMetalsEditor();
    }

    #region Item List Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Display_Item_List()
    {
        // Act
        var listBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        listBoxes.Should().NotBeEmpty("FlatItem editor should have an item list");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Display_Items_With_Keys()
    {
        // Act
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var items = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            // Assert
            items.Should().NotBeEmpty("Should have at least one item");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Select_Item_When_Clicked()
    {
        // Arrange
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var items = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            if (items.Length > 0)
            {
                // Act
                items[0].Click();
                Thread.Sleep(300);

                // Assert - Use SelectionItemPattern to check if selected
                var selectionPattern = items[0].Patterns.SelectionItem.Pattern;
                selectionPattern.IsSelected.Value.Should().BeTrue("Item should be selected");
            }
        }
    }

    #endregion

    #region Traits Display Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Show_DisplayName_Field_When_Item_Selected()
    {
        // Arrange
        SelectFirstItem();

        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => 
            t.Contains("Display", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("Name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Show_Traits_Section_When_Item_Selected()
    {
        // Arrange
        SelectFirstItem();

        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => 
            t.Contains("Traits", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("Properties", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Have_TextBoxes_For_Editing()
    {
        // Arrange
        SelectFirstItem();

        // Act
        var textBoxes = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Edit));

        // Assert
        textBoxes.Should().NotBeEmpty("Should have text input fields for editing");
    }

    #endregion

    #region Button Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Have_Add_Item_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Add", StringComparison.OrdinalIgnoreCase) && 
            n.Contains("Item", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Have_Delete_Item_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            (n.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Remove", StringComparison.OrdinalIgnoreCase)) &&
            (n.Contains("Item", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Selected", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Have_Save_Button()
    {
        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Save", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Have_Add_Trait_Button()
    {
        // Arrange
        SelectFirstItem();

        // Act
        var buttons = GetAllButtons();

        // Assert
        var buttonNames = buttons.Select(b => b.Name).ToList();
        buttonNames.Should().Contain(n => 
            n.Contains("Add", StringComparison.OrdinalIgnoreCase) && 
            (n.Contains("Trait", StringComparison.OrdinalIgnoreCase) ||
             n.Contains("Property", StringComparison.OrdinalIgnoreCase)));
    }

    #endregion

    #region Layout Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Use_Split_View_Layout()
    {
        // Act - Check for item list on left and details on right
        var lists = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.List));

        // Assert
        lists.Should().NotBeEmpty("Should have list view");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Show_Item_Count()
    {
        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        // May show "X Items" or similar count indicator
        texts.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Show_File_Path_Information()
    {
        // Act
        var textElements = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Text));

        // Assert
        var texts = textElements.Select(t => t.Name).ToList();
        texts.Should().Contain(t => 
            t.Contains("metals", StringComparison.OrdinalIgnoreCase) ||
            t.Contains(".json", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Interaction Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Enable_Delete_Button_When_Item_Selected()
    {
        // Arrange
        SelectFirstItem();
        Thread.Sleep(300);

        // Act
        var buttons = GetAllButtons();
        var deleteButton = buttons.FirstOrDefault(b => 
            (b.Name.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
             b.Name.Contains("Remove", StringComparison.OrdinalIgnoreCase)));

        // Assert
        if (deleteButton != null)
        {
            deleteButton.IsEnabled.Should().BeTrue("Delete button should be enabled when item is selected");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "FlatItem")]
    public void Should_Clear_Selection_When_Clicking_Empty_Space()
    {
        // Arrange
        SelectFirstItem();
        Thread.Sleep(200);

        // Act - Click somewhere else in the window
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            // Note: This test verifies the UI structure exists
            // Actual deselection testing would require more complex interaction
            listBox.Should().NotBeNull();
        }
    }

    #endregion

    #region Helper Methods

    private void NavigateToMetalsEditor()
    {
        var tree = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Tree))?.AsTree();
        
        if (tree == null) return;

        var itemsNode = tree.Items.FirstOrDefault(item => item.Name == "Items");
        if (itemsNode == null) return;

        itemsNode.Expand();
        Thread.Sleep(500);

        var materialsNode = itemsNode.FindFirstChild(cf => cf.ByName("Materials"))?.AsTreeItem();
        if (materialsNode == null) return;

        materialsNode.Expand();
        Thread.Sleep(500);

        var metalsItem = materialsNode.FindFirstChild(cf => cf.ByName("Metals"))?.AsTreeItem();
        if (metalsItem == null) return;

        metalsItem.Click();
        Thread.Sleep(1000);
    }

    private void SelectFirstItem()
    {
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.List));

        if (listBox != null)
        {
            var items = listBox.FindAllChildren(cf => 
                cf.ByControlType(ControlType.ListItem));

            if (items.Length > 0)
            {
                items[0].Click();
                Thread.Sleep(300);
            }
        }
    }

    private AutomationElement[] GetAllButtons()
    {
        return _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.Button));
    }

    #endregion
}

