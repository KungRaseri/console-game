using System;
using System.IO;
using System.Linq;
using FlaUI.Core.AutomationElements;
using Xunit;
using FluentAssertions;
using System.Diagnostics;
using FlaUI.Core.Definitions;

namespace Game.ContentBuilder.Tests.UI;

[Trait("Category", "UI")]
public class NameCatalogEditorUITests : UITestBase
{
    private readonly string _testDataPath;

    public NameCatalogEditorUITests() : base()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderUITests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        CreateTestDataFiles();

        try
        {
            Environment.SetEnvironmentVariable("CONTENTBUILDER_DATA_PATH", _testDataPath);
            LaunchApplication();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void CreateTestDataFiles()
    {
        var firstNamesJson = @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""name_catalog"",
    ""description"": ""First names for NPCs""
  },
  ""categories"": {
    ""male_common"": [
      ""John"",
      ""Michael"",
      ""William""
    ],
    ""female_common"": [
      ""Mary"",
      ""Elizabeth""
    ]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, "first_names.json"), firstNamesJson);

        var lastNamesJson = @"{
  ""metadata"": {
    ""version"": ""4.0"",
    ""type"": ""name_catalog"",
    ""description"": ""Last names for NPCs""
  },
  ""categories"": {
    ""common"": [
      ""Smith"",
      ""Johnson""
    ]
  }
}";
        File.WriteAllText(Path.Combine(_testDataPath, "last_names.json"), lastNamesJson);
    }

    [Fact]
    public void Can_Navigate_To_NameCatalog_Editor()
    {
        // Arrange
        _mainWindow.Should().NotBeNull();
        var treeView = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"))?.AsTree();
        treeView.Should().NotBeNull();

        // Act - Expand Names node
        var namesNode = treeView!.Items.FirstOrDefault(i => i.Name.Contains("Names"));
        namesNode.Should().NotBeNull();
        namesNode!.Expand();
        Thread.Sleep(500);

        // Find and click first_names.json
        var firstNamesNode = namesNode.Items.FirstOrDefault(i => i.Name.Contains("first_names"));
        firstNamesNode.Should().NotBeNull();
        firstNamesNode!.Click();
        Thread.Sleep(1000);

        // Assert - Editor should load
        var categoryList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"));
        categoryList.Should().NotBeNull("NameCatalogEditor should be loaded");
    }

    [Fact]
    public void Can_Select_Category_And_View_Names()
    {
        // Arrange - Navigate to editor
        NavigateToFirstNamesEditor();

        // Act
        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        categoryList.Should().NotBeNull();
        
        var maleCategory = categoryList!.Items.FirstOrDefault(i => i.Name.Contains("male_common"));
        maleCategory.Should().NotBeNull();
        maleCategory!.Click();
        Thread.Sleep(500);

        // Assert
        var namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        namesList!.Items.Length.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Can_Add_Single_Name()
    {
        // Arrange
        NavigateToFirstNamesEditor();
        SelectCategory("male_common");

        // Act
        var newNameInput = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("NewNameInput"))?.AsTextBox();
        newNameInput.Should().NotBeNull();
        newNameInput!.Text = "James";

        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddNameButton"))?.AsButton();
        addButton.Should().NotBeNull();
        addButton!.Click();
        Thread.Sleep(500);

        // Assert
        var namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        var jamesItem = namesList!.Items.FirstOrDefault(i => i.Name.Contains("James"));
        jamesItem.Should().NotBeNull("James should be added to the list");
    }

    [Fact]
    public void Can_Add_Category()
    {
        // Arrange
        NavigateToFirstNamesEditor();

        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        var initialCount = categoryList!.Items.Length;

        // Act
        var newCategoryInput = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NewCategoryInput"))?.AsTextBox();
        newCategoryInput.Should().NotBeNull();
        newCategoryInput!.Text = "male_noble";

        var addCategoryButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddCategoryButton"))?.AsButton();
        addCategoryButton.Should().NotBeNull();
        addCategoryButton!.Click();
        Thread.Sleep(500);

        // Assert
        categoryList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        categoryList!.Items.Should().HaveCount(initialCount + 1);
        
        var nobleCategory = categoryList.Items.FirstOrDefault(i => i.Name.Contains("male_noble"));
        nobleCategory.Should().NotBeNull("male_noble category should be added");
    }

    [Fact]
    public void Save_Button_Appears_When_Dirty()
    {
        // Arrange
        NavigateToFirstNamesEditor();
        SelectCategory("male_common");

        // Act - Make a change
        var newNameInput = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("NewNameInput"))?.AsTextBox();
        newNameInput!.Text = "TestName";
        
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
        // Arrange
        NavigateToFirstNamesEditor();
        SelectCategory("female_common");

        // Act - Expand bulk add
        var bulkAddExpander = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("BulkAddExpander"));
        bulkAddExpander.Should().NotBeNull();
        bulkAddExpander!.Click(); // Toggle expander
        Thread.Sleep(500);

        var bulkInput = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BulkNamesInput"))?.AsTextBox();
        bulkInput.Should().NotBeNull();
        bulkInput!.Text = "Jennifer, Jessica, Amanda";

        var bulkAddButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("BulkAddButton"))?.AsButton();
        bulkAddButton.Should().NotBeNull();
        bulkAddButton!.Click();
        Thread.Sleep(1000);

        // Assert
        var namesList = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NamesList"))?.AsListBox();
        namesList.Should().NotBeNull();
        namesList!.Items.Length.Should().BeGreaterThanOrEqualTo(5); // 2 original + 3 new
    }

    private void NavigateToFirstNamesEditor()
    {
        var treeView = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"))?.AsTree();
        var namesNode = treeView!.Items.FirstOrDefault(i => i.Name.Contains("Names"));
        namesNode!.Expand();
        Thread.Sleep(500);

        var firstNamesNode = namesNode.Items.FirstOrDefault(i => i.Name.Contains("first_names"));
        firstNamesNode!.Click();
        Thread.Sleep(1000);
    }

    private void SelectCategory(string categoryName)
    {
        var categoryList = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryList"))?.AsListBox();
        var category = categoryList!.Items.FirstOrDefault(i => i.Name.Contains(categoryName));
        category.Should().NotBeNull($"Category {categoryName} should exist");
        category!.Click();
        Thread.Sleep(500);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        if (disposing)
        {
            try
            {
                if (Directory.Exists(_testDataPath))
                {
                    Directory.Delete(_testDataPath, true);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }
    }
}

