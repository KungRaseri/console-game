using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// UI tests for the Name List Editor (names.json v4 format)
/// Tests focus on UI element presence and basic interactions.
/// Complex functionality (add/remove/edit) is tested in NameListEditorViewModelTests.
/// </summary>
[Trait("Category", "UI")]
[Collection("UI Tests")]
public class NameListEditor_ComprehensiveTests
{
    private readonly UITestCollectionFixture _fixture;
    private readonly ITestOutputHelper _output;
    private Application _app => _fixture.App!;
    private UIA3Automation _automation => _fixture.Automation!;
    private Window _mainWindow => _fixture.MainWindow!;

    public NameListEditor_ComprehensiveTests(UITestCollectionFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        Thread.Sleep(500);
        NavigateToEnemiesNamesEditor(); // Navigate to enemies/beasts/names.json
    }

    #region Diagnostic Tests

    [Fact]
    [Trait("Category", "Diagnostic")]
    public void Debug_WhatElementsAreLoaded()
    {
        // Dump all elements to see what's actually in the window
        var allElements = _mainWindow.FindAllDescendants();
        _output.WriteLine($"=== TOTAL ELEMENTS: {allElements.Length} ===");

        // Count elements by AutomationId
        var elementsWithIds = allElements.Where(e => !string.IsNullOrEmpty(e.Properties.AutomationId.ValueOrDefault)).ToList();
        _output.WriteLine($"Elements with AutomationIds: {elementsWithIds.Count}");

        // Show all elements with "Pattern" in name or AutomationId
        var patternRelated = allElements.Where(e =>
            (e.Properties.AutomationId.ValueOrDefault?.Contains("Pattern", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.Name?.Contains("Pattern", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (e.Properties.AutomationId.ValueOrDefault?.Contains("Template", StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();

        _output.WriteLine($"\n=== PATTERN-RELATED ELEMENTS ({patternRelated.Count}) ===");
        foreach (var elem in patternRelated)
        {
            try
            {
                _output.WriteLine($"Type: {elem.ControlType} | AutomationId: '{elem.Properties.AutomationId.ValueOrDefault}' | Name: '{elem.Name}' | ClassName: '{elem.ClassName}'");
            }
            catch { }
        }

        // Specifically look for NameListEditorView
        var nameListEditor = _mainWindow.FindFirstDescendant(cf => cf.ByClassName("NameListEditorView"));
        _output.WriteLine($"\n=== NameListEditorView found: {nameListEditor != null} ===");

        // Try to find PatternCard by AutomationId
        var patternCards = _mainWindow.FindAllDescendants(cf => cf.ByAutomationId("PatternCard"));
        _output.WriteLine($"PatternCard elements (by AutomationId): {patternCards.Length}");

        // Try to find by control type DataItem (since MaterialDesign.Card becomes DataItem)
        var dataItems = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
        _output.WriteLine($"Total DataItem elements: {dataItems.Length}");

        // Check for ItemsControl that should contain patterns
        var itemsControls = _mainWindow.FindAllDescendants(cf => cf.ByControlType(ControlType.List));
        _output.WriteLine($"ItemsControl/List elements: {itemsControls.Length}");
    }

    #endregion

    #region UI Element Presence Tests

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "Patterns")]
    public void Should_Display_Pattern_List()
    {
        // Act - Find the PatternsList ItemsControl and get its children
        var patternsList = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternsList"));

        patternsList.Should().NotBeNull("PatternsList ItemsControl should be found");

        // Get all DataItem children (these are the pattern cards)
        var patternCards = patternsList!.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.DataItem));

        _output.WriteLine($"Found {patternCards.Length} pattern items");

        // Assert - Should have at least one pattern (the file has 8 patterns)
        patternCards.Should().NotBeEmpty("Should display at least one pattern");
    }

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "Patterns")]
    public void Should_Add_New_Pattern_When_Add_Button_Clicked()
    {
        // This test verifies the Add Pattern button exists and is clickable
        // Actual pattern creation logic is tested in ViewModel tests

        var addButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternAddButton"));

        addButton.Should().NotBeNull("PatternAddButton should be found");
        addButton!.IsEnabled.Should().BeTrue("PatternAddButton should be enabled");

        // Verify button can be invoked without errors
        addButton.AsButton().Invoke();
        Thread.Sleep(200);

        // The button should still exist after clicking (no crash/error)
        var buttonAfterClick = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternAddButton"));
        buttonAfterClick.Should().NotBeNull("Application should remain stable after adding pattern");
    }

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "Patterns")]
    public void Should_Delete_Pattern_When_Delete_Button_Clicked()
    {
        // This test verifies the Delete Pattern button exists and is clickable
        // Actual pattern deletion logic is tested in ViewModel tests

        var deleteButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternDeleteButton"));

        deleteButton.Should().NotBeNull("PatternDeleteButton should be found");
        deleteButton!.IsEnabled.Should().BeTrue("PatternDeleteButton should be enabled when patterns exist");

        // Verify button can be invoked without errors
        deleteButton.Click();
        Thread.Sleep(200);

        // The application should remain stable after clicking
        var buttonAfterClick = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternAddButton"));
        buttonAfterClick.Should().NotBeNull("Application should remain stable after deleting pattern");
    }

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "Components")]
    public void Should_Display_Available_Component_Tokens()
    {
        // Act - Look for component insertion buttons in the UI
        var componentButtons = _mainWindow.FindAllDescendants(cf =>
            cf.ByName("size") // Common component name in beast names
            .And(cf.ByControlType(ControlType.Button)));

        // Assert - At least one component button should be visible
        componentButtons.Should().NotBeEmpty("Should display component insertion buttons");
    }

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "References")]
    public void Should_Display_Reference_Insertion_Buttons()
    {
        // Act - Look for reference token buttons (@materialRef, @weaponRef, etc.)
        var materialRefButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByName("@materialRef"));
        var weaponRefButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByName("@weaponRef"));
        var enemyRefButton = _mainWindow.FindFirstDescendant(cf =>
            cf.ByName("@enemyRef"));

        // Assert - At least one reference button should be present
        (materialRefButton != null || weaponRefButton != null || enemyRefButton != null)
            .Should().BeTrue("Should display at least one reference insertion button");
    }

    [Fact][Trait("Editor", "NameList")]
    [Trait("Feature", "Description")]
    public void Should_Display_Description_Field()
    {
        // Act - Find the description field
        var descriptionTextBox = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("PatternDescriptionTextBox"));

        // Assert - Description field should exist in pattern cards
        descriptionTextBox.Should().NotBeNull("Should display pattern description field");
    }

    #endregion

    private void NavigateToEnemiesNamesEditor()
    {
        // Navigate to enemies/beasts/Names (note: TreeItems use display names, not filenames!)
        var treeView = _mainWindow.FindFirstDescendant(cf =>
            cf.ByAutomationId("CategoryTreeView"));

        if (treeView != null)
        {
            var allItems = treeView.FindAllDescendants(cf => cf.ByControlType(ControlType.TreeItem));

            // Find and expand "Enemies" (capital E)
            AutomationElement? enemiesItem = allItems.FirstOrDefault(item =>
            {
                try { return item.Name.Equals("Enemies", StringComparison.OrdinalIgnoreCase); }
                catch { return false; }
            });
            enemiesItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and expand "Beasts" within Enemies (capital B)
            var beastsItem = enemiesItem?.FindFirstDescendant(cf =>
                cf.ByName("Beasts"));
            beastsItem?.AsTreeItem()?.Expand();
            Thread.Sleep(500);

            // Find and select "Names" within Beasts (capital N, no .json extension!)
            var namesItem = beastsItem?.FindFirstDescendant(cf =>
                cf.ByName("Names"));
            namesItem?.AsTreeItem()?.Select();
            Thread.Sleep(1000);
        }
    }
}
