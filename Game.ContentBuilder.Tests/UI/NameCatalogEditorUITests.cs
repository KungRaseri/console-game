using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using Xunit;
using FluentAssertions;
using FlaUI.Core.Definitions;

namespace Game.ContentBuilder.Tests.UI;

[Trait("Category", "UI")]
public class NameCatalogEditorUITests : UITestBase
{
    public NameCatalogEditorUITests() : base()
    {
        LaunchApplication();
        Thread.Sleep(1500); // Allow app to fully load
        NavigateToFirstNamesEditor();
    }

    [Fact]
    public void Can_Navigate_To_NameCatalog_Editor()
    {
        // Assert - Editor should already be loaded by constructor
        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("NameCatalogEditor should be loaded");
        
        // Verify we're viewing a name catalog (should have multiple categories)
        var categoryListBox = categoryList.AsListBox();
        categoryListBox.Items.Should().NotBeEmpty("Should have at least one category");
    }

    [Fact]
    public void Can_Select_Category_And_View_Names()
    {
        // Arrange - Already at first_names.json
        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        categoryList.Should().NotBeNull();
        
        // Act - Select male_common category (should exist in real data)
        var maleCategory = categoryList!.Items.FirstOrDefault(i => i.Name.Contains("male_common"));
        maleCategory.Should().NotBeNull("male_common category should exist");
        maleCategory!.Click();
        Thread.Sleep(500);

        // Assert - Should see names from real data
        var namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        namesList!.Items.Length.Should().BeGreaterThanOrEqualTo(10, "male_common should have at least 10 names in real data");
    }

    [Fact]
    public void Can_Add_Single_Name()
    {
        // Arrange - Select a category
        SelectCategory("male_common");

        var namesList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        var initialCount = namesList!.Items.Length;

        // Act - Add a new name
        var newNameInput = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NewNameInput"))?.AsTextBox();
        newNameInput.Should().NotBeNull();
        newNameInput!.Text = "TestName_" + Guid.NewGuid().ToString().Substring(0, 8);

        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddNameButton"))?.AsButton();
        addButton.Should().NotBeNull();
        addButton!.Click();
        Thread.Sleep(500);

        // Assert - Name should be added
        namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        namesList!.Items.Length.Should().Be(initialCount + 1, "One name should be added");
    }

    [Fact]
    public void Can_Add_Category()
    {
        // Arrange
        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        var initialCount = categoryList!.Items.Length;

        // Act - Add a new category
        var newCategoryInput = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NewCategoryInput"))?.AsTextBox();
        newCategoryInput.Should().NotBeNull();
        
        var uniqueCategoryName = "test_category_" + Guid.NewGuid().ToString().Substring(0, 8);
        newCategoryInput!.Text = uniqueCategoryName;

        var addCategoryButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddCategoryButton"))?.AsButton();
        addCategoryButton.Should().NotBeNull();
        addCategoryButton!.Click();
        Thread.Sleep(500);

        // Assert
        categoryList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        categoryList!.Items.Should().HaveCount(initialCount + 1);
        
        var newCategory = categoryList.Items.FirstOrDefault(i => i.Name.Contains(uniqueCategoryName));
        newCategory.Should().NotBeNull($"{uniqueCategoryName} category should be added");
    }

    [Fact]
    public void Save_Button_Appears_When_Dirty()
    {
        // Arrange - Select a category
        SelectCategory("male_common");

        // Act - Make a change
        var newNameInput = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("NewNameInput"))?.AsTextBox();
        newNameInput!.Text = "TestName_Dirty";
        
        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddNameButton"))?.AsButton();
        addButton!.Click();
        Thread.Sleep(500);

        // Assert
        var saveButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("SaveButton"))?.AsButton();
        saveButton.Should().NotBeNull();
        saveButton!.IsEnabled.Should().BeTrue("Save button should be enabled when IsDirty is true");
    }

    [Fact]
    public void Can_Use_Bulk_Add()
    {
        // Arrange - Select a category
        SelectCategory("female_common");

        var namesList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        var initialCount = namesList!.Items.Length;

        // Act - Expand bulk add
        var bulkAddExpander = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BulkAddExpander"));
        bulkAddExpander.Should().NotBeNull();
        bulkAddExpander!.Click(); // Toggle expander
        Thread.Sleep(500);

        var bulkInput = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BulkNamesInput"))?.AsTextBox();
        bulkInput.Should().NotBeNull();
        bulkInput!.Text = "TestName1, TestName2, TestName3";

        var bulkAddButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BulkAddButton"))?.AsButton();
        bulkAddButton.Should().NotBeNull();
        bulkAddButton!.Click();
        Thread.Sleep(1000);

        // Assert - 3 names should be added
        namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        namesList!.Items.Length.Should().Be(initialCount + 3, "3 names should be added via bulk add");
    }

    /// <summary>
    /// Navigates to NPCs → Names → First Names in the real data structure
    /// </summary>
    private void NavigateToFirstNamesEditor()
    {
        // Ensure main window is available
        _mainWindow.Should().NotBeNull("Main window must be available before navigation");

        // Find tree view
        var treeView = ExecuteWithTimeout(() =>
        {
            var tree = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"))?.AsTree();
            tree.Should().NotBeNull("CategoryTreeView should exist");
            return tree!;
        }, TimeSpan.FromSeconds(5), "Find CategoryTreeView");

        // Find and expand NPCs node
        var npcsNode = ExecuteWithTimeout(() =>
        {
            var node = treeView.Items.FirstOrDefault(i => i.Name == "NPCs");
            node.Should().NotBeNull("NPCs node should exist in tree");
            return node!;
        }, TimeSpan.FromSeconds(3), "Find NPCs node");

        npcsNode.Expand();
        Thread.Sleep(500);

        // Find and expand Names folder
        var namesFolder = ExecuteWithTimeout(() =>
        {
            var node = npcsNode.Items.FirstOrDefault(i => i.Name == "Names");
            node.Should().NotBeNull("Names folder should exist under NPCs");
            return node!;
        }, TimeSpan.FromSeconds(3), "Find Names folder");

        namesFolder.Expand();
        Thread.Sleep(500);

        // Find and click "First Names" node
        var firstNamesNode = ExecuteWithTimeout(() =>
        {
            var node = namesFolder.Items.FirstOrDefault(i => i.Name == "First Names");
            node.Should().NotBeNull("First Names node should exist under Names folder");
            return node!;
        }, TimeSpan.FromSeconds(3), "Find First Names node");

        firstNamesNode.Click();
        Thread.Sleep(1500); // Give editor time to load
    }

    private void SelectCategory(string categoryName)
    {
        // Ensure main window is available
        _mainWindow.Should().NotBeNull("Main window must be available before selecting category");

        // Find category list
        var categoryList = ExecuteWithTimeout(() =>
        {
            var list = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
            list.Should().NotBeNull("CategoryList should exist");
            return list!;
        }, TimeSpan.FromSeconds(5), "Find CategoryList");

        // Find and click category
        var category = ExecuteWithTimeout(() =>
        {
            var cat = categoryList.Items.FirstOrDefault(i => i.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase));
            cat.Should().NotBeNull($"Category '{categoryName}' should exist");
            return cat!;
        }, TimeSpan.FromSeconds(3), $"Find category '{categoryName}'");

        category.Click();
        Thread.Sleep(500);
    }
}
