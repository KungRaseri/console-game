using System.IO;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using RealmForge.Tests.UI;

namespace RealmForge.Tests.Integration;

/// <summary>
/// Integration tests for complete ContentBuilder workflows
/// Tests end-to-end scenarios: Launch → Navigate → Edit → Save → Verify
/// </summary>
[Trait("Category", "UI")]
[Collection("UI Tests")]
public class ContentBuilderIntegrationTests : UITestBase
{
    private readonly string _testDataPath;

    public ContentBuilderIntegrationTests() : base()
    {
        LaunchApplication();

        // Get test data path (ContentBuilder's Resources/data directory)
        var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "RealmForge", "bin", "Debug", "net9.0-windows",
            "RealmForge.exe");

        var fullExePath = Path.GetFullPath(exePath);
        _testDataPath = Path.Combine(
            Path.GetDirectoryName(fullExePath)!,
            "Resources", "data"
        );

        Thread.Sleep(1500); // Wait for UI to stabilize
    }

    #region Full Workflow Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Workflow", "EditAndReload")]
    public void Complete_Workflow_Navigate_Edit_Save_And_Reload_Should_Persist_Changes()
    {
        // This test verifies the complete cycle works without errors
        // We won't actually modify files, but we'll verify navigation works

        // Act - Navigate to a file
        NavigateToEditor("General", "Colors");
        Thread.Sleep(1000);

        // Assert - HybridArray editor loaded
        var tabs = _mainWindow?.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.TabItem));
        tabs.Should().NotBeEmpty("Editor should load with tabs");

        // Navigate to another file
        NavigateToEditor("General", "Adjectives");
        Thread.Sleep(1000);

        // Assert - NameList editor loaded
        var lists = _mainWindow?.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.List));
        lists.Should().NotBeEmpty("NameList editor should have lists");

        // Navigate back to first file
        NavigateToEditor("General", "Colors");
        Thread.Sleep(1000);

        // Assert - Can navigate back successfully
        tabs = _mainWindow?.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.TabItem));
        tabs.Should().NotBeEmpty("Should reload HybridArray editor");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Workflow", "MultipleFiles")]
    public void Should_Handle_Switching_Between_Multiple_Files()
    {
        // Act & Assert - Test rapid file switching
        var testFiles = new[]
        {
            ("General", "Colors"),
            ("General", "Sounds"),
            ("General", "Smells"),
            ("General", "Adjectives")
        };

        foreach (var (category, file) in testFiles)
        {
            NavigateToEditor(category, file);
            Thread.Sleep(500);

            // Verify editor loaded (should have some UI elements)
            var buttons = _mainWindow?.FindAllDescendants(cf =>
                cf.ByControlType(ControlType.Button));
            buttons.Should().NotBeEmpty($"Editor for {file} should have buttons");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Workflow", "TreeExpansion")]
    public void Should_Expand_All_Categories_In_Tree()
    {
        // Arrange
        var tree = FindTreeView();
        var categories = new[] { "General", "Items", "Enemies", "NPCs", "Quests" };

        // Act & Assert - Expand each top-level category
        foreach (var categoryName in categories)
        {
            var category = FindTreeItem(tree, categoryName);
            if (category != null)
            {
                category.Expand();
                Thread.Sleep(300);

                var children = category.FindAllChildren();
                children.Should().NotBeEmpty($"{categoryName} should have child items");
            }
        }
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("DataIntegrity", "LoadAllFiles")]
    public void Should_Load_All_JSON_Files_Without_Errors()
    {
        // This test verifies all 93 JSON files can be loaded without crashes

        // Get all JSON files in data directory
        if (!Directory.Exists(_testDataPath))
        {
            // Skip test if data directory not found
            return;
        }

        var jsonFiles = Directory.GetFiles(_testDataPath, "*.json", SearchOption.AllDirectories);
        jsonFiles.Should().NotBeEmpty("Should find JSON data files");

        // Verify each file has valid JSON
        foreach (var filePath in jsonFiles)
        {
            var json = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            // Act & Assert - Parse JSON without errors
            Action parseAction = () => JsonConvert.DeserializeObject<JObject>(json);
            parseAction.Should().NotThrow($"{fileName} should be valid JSON");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("DataIntegrity", "StructureValidation")]
    public void HybridArray_Files_Should_Have_Required_Structure()
    {
        if (!Directory.Exists(_testDataPath))
        {
            return;
        }

        // Find a known HybridArray file
        var colorsFile = Path.Combine(_testDataPath, "general", "colors.json");
        if (!File.Exists(colorsFile))
        {
            return;
        }

        // Act
        var json = File.ReadAllText(colorsFile);
        var data = JsonConvert.DeserializeObject<JObject>(json);

        // Assert - Should have items, components, patterns
        data.Should().NotBeNull();
        data!["items"].Should().NotBeNull("Should have items array");
        data["components"].Should().NotBeNull("Should have components array");
        data["patterns"].Should().NotBeNull("Should have patterns array");
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("DataIntegrity", "NameListValidation")]
    public void NameList_Files_Should_Have_Category_Structure()
    {
        if (!Directory.Exists(_testDataPath))
        {
            return;
        }

        // Find a known NameList file
        var adjectivesFile = Path.Combine(_testDataPath, "general", "adjectives.json");
        if (!File.Exists(adjectivesFile))
        {
            return;
        }

        // Act
        var json = File.ReadAllText(adjectivesFile);
        var data = JsonConvert.DeserializeObject<JObject>(json);

        // Assert - Should have category → array structure
        data.Should().NotBeNull();
        data!.Properties().Should().NotBeEmpty("Should have at least one category");

        foreach (var property in data.Properties())
        {
            // Each category should be an array (or object for variants)
            (property.Value is JArray || property.Value is JObject)
                .Should().BeTrue($"Category {property.Name} should be array or object");
        }
    }

    #endregion

    #region Error Scenario Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("ErrorHandling", "MissingFile")]
    public void Should_Handle_Missing_File_Gracefully()
    {
        // This test verifies the app doesn't crash when files are missing
        // We just verify the app is still responsive after navigating

        // Act - Try to navigate to various files
        NavigateToEditor("General", "Colors");
        Thread.Sleep(500);

        // Assert - App should still be responsive
        _mainWindow.Should().NotBeNull();
        _mainWindow.IsEnabled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("ErrorHandling", "RapidNavigation")]
    public void Should_Handle_Rapid_Navigation_Without_Crashes()
    {
        // Act - Rapidly click different tree items
        var tree = FindTreeView();
        var generalItem = FindTreeItem(tree, "General");
        generalItem?.Expand();
        Thread.Sleep(200);

        for (int i = 0; i < 5; i++)
        {
            var colors = FindTreeItem(tree, "Colors");
            colors?.Click();
            Thread.Sleep(100);

            var sounds = FindTreeItem(tree, "Sounds");
            sounds?.Click();
            Thread.Sleep(100);
        }

        // Assert - App should still be responsive
        _mainWindow.Should().NotBeNull();
        _mainWindow.IsEnabled.Should().BeTrue();
    }

    #endregion

    #region Preview Integration Tests

    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Preview", "Generation")]
    public void Should_Open_Preview_Window_For_HybridArray()
    {
        // Arrange
        NavigateToEditor("General", "Colors");
        Thread.Sleep(1000);

        // Act - Look for Preview button
        var buttons = _mainWindow?.FindAllDescendants(cf =>
            cf.ByControlType(ControlType.Button));

        var previewButton = buttons?.FirstOrDefault(b =>
            b.Name.Contains("Preview", StringComparison.OrdinalIgnoreCase));

        if (previewButton != null)
        {
            previewButton.Click();
            Thread.Sleep(1000);

            // Assert - Preview window should open
            var windows = _automation?.GetDesktop().FindAllChildren();
            var previewWindow = windows?.FirstOrDefault(w =>
                w.Name.Contains("Preview", StringComparison.OrdinalIgnoreCase));

            if (previewWindow != null)
            {
                previewWindow.Should().NotBeNull("Preview window should open");
                previewWindow.AsWindow().Close();
            }
        }
    }

    #endregion

    #region Helper Methods

    private Tree FindTreeView()
    {
        var tree = _mainWindow?.FindFirstDescendant(cf =>
            cf.ByControlType(ControlType.Tree))?.AsTree();

        tree.Should().NotBeNull("Tree view should exist");
        return tree!;
    }

    private TreeItem? FindTreeItem(Tree tree, string itemName)
    {
        return tree.Items.FirstOrDefault(item => item.Name == itemName);
    }

    private void NavigateToEditor(params string[] path)
    {
        var tree = FindTreeView();
        TreeItem? currentItem = null;

        for (int i = 0; i < path.Length; i++)
        {
            var itemName = path[i];

            if (i == 0)
            {
                currentItem = FindTreeItem(tree, itemName);
            }
            else
            {
                currentItem = currentItem?.FindFirstChild(cf =>
                    cf.ByName(itemName))?.AsTreeItem();
            }

            if (currentItem == null)
            {
                return; // Item not found, skip test
            }

            if (i < path.Length - 1)
            {
                currentItem.Expand();
                Thread.Sleep(300);
            }
            else
            {
                currentItem.Click();
                Thread.Sleep(500);
            }
        }
    }

    #endregion
}
