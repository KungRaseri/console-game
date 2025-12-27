using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// UI tests for the Abilities Editor (ability_catalog.json format)
/// Tests focus on UI element presence and basic interactions.
/// Complex functionality (add/remove/edit) is tested in AbilitiesEditorViewModelTests.
/// </summary>
[Collection("UI Tests")]
public class AbilitiesEditor_ComprehensiveTests
{
    private readonly UITestCollectionFixture _fixture;
    private readonly ITestOutputHelper _output;
    private Application _app => _fixture.App!;
    private UIA3Automation _automation => _fixture.Automation!;
    private Window _mainWindow => _fixture.MainWindow!;

    public AbilitiesEditor_ComprehensiveTests(UITestCollectionFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        Thread.Sleep(500);
        NavigateToAbilitiesEditor(); // Navigate to enemies/boss-abilities.json
    }

    #region Diagnostic Tests

    [Fact]
    [Trait("Category", "Diagnostic")]
    public void Debug_WhatEditorLoaded()
    {
        // Dump information about what's actually loaded
        _output.WriteLine($"=== CHECKING ABILITIES EDITOR STATE ===");
        
        var allElements = _mainWindow.FindAllDescendants();
        _output.WriteLine($"Total elements: {allElements.Length}");
        
        // Look for any abilities-related elements
        var abilitiesElements = allElements.Where(e => 
            (e.Properties.AutomationId.ValueOrDefault?.Contains("Abilit", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.Name?.Contains("Abilit", StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
        
        _output.WriteLine($"\n=== ABILITIES-RELATED ELEMENTS ({abilitiesElements.Count}) ===");
        foreach (var elem in abilitiesElements.Take(20))
        {
            try
            {
                _output.WriteLine($"Type: {elem.ControlType} | AutomationId: '{elem.Properties.AutomationId.ValueOrDefault}' | Name: '{elem.Name}'");
            }
            catch { }
        }
        
        // Check if Abilities Editor view is loaded
        var abilitiesView = _mainWindow.FindFirstDescendant(cf => cf.ByClassName("AbilitiesEditorView"));
        _output.WriteLine($"\n=== AbilitiesEditorView found: {abilitiesView != null} ===");
    }

    #endregion

    #region UI Element Presence Tests

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "List")]
    public void Should_Display_Abilities_ListView()
    {
        // Act - Find abilities list control
        var abilitiesList = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AbilitiesListView"));
        
        abilitiesList.Should().NotBeNull("Should display abilities list control");

        // For GridView-based ListView, check for DataItem controls instead of ListItem
        var listItems = abilitiesList!.FindAllDescendants(cf => 
            cf.ByControlType(ControlType.DataItem).Or(cf.ByControlType(ControlType.ListItem)));
        
        // Should have at least one ability from loaded file
        listItems.Should().NotBeEmpty("Should display ability items for loaded abilities");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "Search")]
    public void Should_Display_Search_TextBox()
    {
        // Act - Find search control
        var searchBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("SearchTextBox"));
        
        searchBox.Should().NotBeNull("Search textbox should exist");
        searchBox!.IsEnabled.Should().BeTrue("Search textbox should be enabled");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "Filter")]
    public void Should_Display_Rarity_Filter_ComboBox()
    {
        // Act - Find rarity filter control
        var rarityFilter = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("FilterRarityComboBox"));
        
        rarityFilter.Should().NotBeNull("Rarity filter combobox should exist");
        rarityFilter!.IsEnabled.Should().BeTrue("Rarity filter should be enabled");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "Buttons")]
    public void Should_Display_Add_Ability_Button()
    {
        // This test verifies the Add Ability button exists and is clickable
        // Actual ability creation logic is tested in ViewModel tests
        
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddAbilityButton"));
        
        addButton.Should().NotBeNull("Add ability button should exist");
        addButton!.IsEnabled.Should().BeTrue("AddAbilityButton should be enabled");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "Buttons")]
    public void Should_Display_Edit_And_Delete_Buttons()
    {
        // This test verifies Edit/Delete buttons exist
        // Actual edit/delete logic is tested in ViewModel tests
        
        var editButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("EditAbilityButton"));
        var deleteButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("DeleteAbilityButton"));
        
        editButton.Should().NotBeNull("Edit ability button should exist");
        deleteButton.Should().NotBeNull("Delete ability button should exist");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "Edit")]
    public void Should_Display_Edit_Form_Fields()
    {
        // Click Add to enter edit mode
        var addButton = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AddAbilityButton"));
        addButton?.Click();
        Thread.Sleep(500);

        // Verify edit form fields exist
        var nameTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AbilityNameTextBox"));
        var descriptionTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AbilityDescriptionTextBox"));
        var rarityWeightTextBox = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("AbilityRarityWeightTextBox"));

        nameTextBox.Should().NotBeNull("Ability name textbox should exist in edit mode");
        descriptionTextBox.Should().NotBeNull("Ability description textbox should exist in edit mode");
        rarityWeightTextBox.Should().NotBeNull("Ability rarity weight textbox should exist in edit mode");
    }

    [Fact]
    [Trait("Category", "UI")]
    [Trait("Editor", "Abilities")]
    [Trait("Feature", "StatusBar")]
    public void Should_Display_Status_Bar()
    {
        // Act - Find status bar (use ControlType instead of AutomationId)
        var statusBar = _mainWindow.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.StatusBar));

        // Assert - Status bar should exist
        statusBar.Should().NotBeNull("Should display status bar");
    }

    #endregion

    private void NavigateToAbilitiesEditor()
    {
        // Navigate to enemies/beasts/abilities.json
        var treeView = _mainWindow.FindFirstDescendant(cf => 
            cf.ByAutomationId("CategoryTreeView"));

        if (treeView != null)
        {
            var allItems = treeView.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));
            
            // Find and expand "Enemies" category
            AutomationElement? enemiesItem = allItems.FirstOrDefault(item =>
            {
                try { return item.Name.Equals("Enemies", StringComparison.OrdinalIgnoreCase); }
                catch { return false; }
            });
            enemiesItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and expand "Beasts" within Enemies
            var beastsItem = enemiesItem?.FindFirstDescendant(cf => 
                cf.ByName("Beasts"));
            beastsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and select "Abilities" file within Beasts
            var abilitiesItem = beastsItem?.FindFirstDescendant(cf => 
                cf.ByName("Abilities"));
            
            abilitiesItem?.AsTreeItem()?.Select();
            Thread.Sleep(1000);
        }
    }
}
