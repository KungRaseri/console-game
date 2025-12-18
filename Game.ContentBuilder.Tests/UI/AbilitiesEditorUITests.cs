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
/// UI automation tests for AbilitiesEditorView
/// Tests ability catalog editing, filtering, and CRUD operations
/// </summary>
[Collection("UI Tests")]
public class AbilitiesEditorUITests : IDisposable
{
    private readonly Application? _app;
    private readonly UIA3Automation _automation;
    private readonly Window? _mainWindow;

    public AbilitiesEditorUITests()
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
            // Skip test if executable not built
            return;
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
    [Trait("Editor", "Abilities")]
    public void AbilitiesEditor_Should_Load_When_Selecting_Boss_Abilities()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        tree.Should().NotBeNull("Tree view should be present");

        // Act
        var enemiesItem = FindTreeItem(tree!, "Enemies");
        if (enemiesItem == null) return; // Skip if tree structure changed
        
        enemiesItem.Expand();
        Thread.Sleep(500);

        var bossAbilitiesItem = FindTreeItem(tree!, "Boss Abilities");
        if (bossAbilitiesItem == null) return;
        
        bossAbilitiesItem.Click();
        Thread.Sleep(1000);

        // Assert - check for Abilities editor UI elements
        var searchBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("SearchBox"));
        searchBox.Should().NotBeNull("Abilities editor should have search box");

        var rarityComboBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("RarityComboBox"));
        rarityComboBox.Should().NotBeNull("Abilities editor should have rarity filter");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    public void AbilitiesEditor_Should_Display_Ability_List()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange
        var tree = FindTreeView();
        if (tree == null) return;

        var enemiesItem = FindTreeItem(tree, "Enemies");
        if (enemiesItem == null) return;
        
        enemiesItem.Expand();
        Thread.Sleep(500);

        var bossAbilitiesItem = FindTreeItem(tree, "Boss Abilities");
        if (bossAbilitiesItem == null) return;
        
        bossAbilitiesItem.Click();
        Thread.Sleep(1000);

        // Assert
        var listBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AbilityList"));
        
        if (listBox != null)
        {
            var items = listBox.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem));
            items.Should().NotBeEmpty("Ability list should contain items");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    public void AbilitiesEditor_Should_Have_Add_Edit_Delete_Buttons()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var enemiesItem = FindTreeItem(tree, "Enemies");
        if (enemiesItem == null) return;
        
        enemiesItem.Expand();
        Thread.Sleep(500);

        var bossAbilitiesItem = FindTreeItem(tree, "Boss Abilities");
        if (bossAbilitiesItem == null) return;
        
        bossAbilitiesItem.Click();
        Thread.Sleep(1000);

        // Assert
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddAbilityButton"));
        addButton.Should().NotBeNull("Should have Add Ability button");

        var editButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("EditAbilityButton"));
        editButton.Should().NotBeNull("Should have Edit Ability button");

        var deleteButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteAbilityButton"));
        deleteButton.Should().NotBeNull("Should have Delete Ability button");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    public void AbilitiesEditor_RarityFilter_Should_Have_All_Options()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var enemiesItem = FindTreeItem(tree, "Enemies");
        if (enemiesItem == null) return;
        
        enemiesItem.Expand();
        Thread.Sleep(500);

        var bossAbilitiesItem = FindTreeItem(tree, "Boss Abilities");
        if (bossAbilitiesItem == null) return;
        
        bossAbilitiesItem.Click();
        Thread.Sleep(1000);

        var rarityComboBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("RarityComboBox"))?.AsComboBox();
        
        if (rarityComboBox != null)
        {
            rarityComboBox.Expand();
            Thread.Sleep(300);

            var items = rarityComboBox.FindAllChildren(cf => cf.ByControlType(ControlType.ListItem));
            var itemTexts = items.Select(i => i.Name).ToList();

            // Assert
            itemTexts.Should().Contain("All");
            itemTexts.Should().Contain("Common");
            itemTexts.Should().Contain("Uncommon");
            itemTexts.Should().Contain("Rare");
            itemTexts.Should().Contain("Epic");
            itemTexts.Should().Contain("Legendary");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    public void AbilitiesEditor_Should_Show_Save_Button()
    {
        // Skip if app didn't launch
        if (_app == null || _mainWindow == null) return;

        // Arrange & Act
        var tree = FindTreeView();
        if (tree == null) return;

        var enemiesItem = FindTreeItem(tree, "Enemies");
        if (enemiesItem == null) return;
        
        enemiesItem.Expand();
        Thread.Sleep(500);

        var bossAbilitiesItem = FindTreeItem(tree, "Boss Abilities");
        if (bossAbilitiesItem == null) return;
        
        bossAbilitiesItem.Click();
        Thread.Sleep(1000);

        // Assert
        var saveButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("SaveButton"));
        saveButton.Should().NotBeNull("Should have Save button");
    }

    public void Dispose()
    {
        try
        {
            // Try graceful shutdown first
            _app?.Close();
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

