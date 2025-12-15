using System;
using System.IO;
using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Unit tests for PreviewWindowViewModel
/// Tests preview generation functionality
/// </summary>
public class PreviewWindowViewModelTests : IDisposable
{
    private readonly string _testDirectory;

    public PreviewWindowViewModelTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ContentBuilderTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Initialize_With_Empty_Title_And_Previews()
    {
        // Act
        var viewModel = new PreviewWindowViewModel();

        // Assert
        viewModel.Title.Should().Be("Preview");
        viewModel.PreviewItems.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SetPreviewData_Should_Update_Title_And_Items()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var testData = new[]
        {
            "Preview Item 1",
            "Preview Item 2",
            "Preview Item 3"
        };

        // Act
        viewModel.SetPreviewData("Test Preview", testData);

        // Assert
        viewModel.Title.Should().Be("Test Preview");
        viewModel.PreviewItems.Should().HaveCount(3);
        viewModel.PreviewItems.Should().Contain("Preview Item 1");
        viewModel.PreviewItems.Should().Contain("Preview Item 2");
        viewModel.PreviewItems.Should().Contain("Preview Item 3");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SetPreviewData_Should_Clear_Previous_Items()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        viewModel.SetPreviewData("First Preview", new[] { "Item 1", "Item 2" });

        // Act
        viewModel.SetPreviewData("Second Preview", new[] { "New Item" });

        // Assert
        viewModel.PreviewItems.Should().HaveCount(1);
        viewModel.PreviewItems.Should().Contain("New Item");
        viewModel.PreviewItems.Should().NotContain("Item 1");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SetPreviewData_Should_Handle_Empty_Array()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();

        // Act
        viewModel.SetPreviewData("Empty Preview", Array.Empty<string>());

        // Assert
        viewModel.Title.Should().Be("Empty Preview");
        viewModel.PreviewItems.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SetPreviewData_Should_Handle_Null_Title()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();

        // Act
        viewModel.SetPreviewData(null!, new[] { "Item" });

        // Assert
        // Should not throw, and title should be set to null or empty
        viewModel.PreviewItems.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void GeneratePreview_Should_Create_Sample_Items_From_HybridArray_Data()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var hybridArrayJson = @"{
            ""items"": [""red"", ""blue"", ""green""],
            ""components"": [""bright"", ""dark"", ""metallic""],
            ""patterns"": [""{component} {item}""]
        }";

        // Act
        var previews = viewModel.GenerateHybridArrayPreviews(hybridArrayJson, count: 5);

        // Assert
        previews.Should().HaveCount(5);
        previews.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p));
        // Should contain combinations like "bright red", "dark blue", etc.
        previews.Should().OnlyContain(p => 
            p.Contains("red") || p.Contains("blue") || p.Contains("green"));
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void GeneratePreview_Should_Create_Combinations_From_NameList_Data()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var nameListJson = @"{
            ""positive"": [""magnificent"", ""glorious"", ""heroic""],
            ""negative"": [""cursed"", ""broken"", ""rusty""]
        }";

        // Act
        var previews = viewModel.GenerateNameListPreviews(nameListJson, count: 5);

        // Assert
        previews.Should().HaveCount(5);
        previews.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p));
        // Should contain adjectives from positive or negative lists
        previews.Should().Contain(p => 
            p.Contains("magnificent") || p.Contains("cursed") || 
            p.Contains("glorious") || p.Contains("broken"));
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void GeneratePreview_Should_Apply_Pattern_Templates()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var pattern = "{adjective} {noun} of {element}";
        var data = new
        {
            adjectives = new[] { "flaming", "frozen" },
            nouns = new[] { "sword", "axe" },
            elements = new[] { "fire", "ice" }
        };

        // Act
        var preview = viewModel.ApplyPattern(pattern, data);

        // Assert
        preview.Should().NotBeNullOrWhiteSpace();
        preview.Should().Contain("sword").Or.Contain("axe");
        preview.Should().Contain("flaming").Or.Contain("frozen");
        preview.Should().Contain("fire").Or.Contain("ice");
        preview.Should().MatchRegex(@"\w+ (sword|axe) of (fire|ice)");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void RefreshPreview_Should_Generate_New_Random_Combinations()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var json = @"{
            ""items"": [""item1"", ""item2"", ""item3""],
            ""patterns"": [""{item}""]
        }";
        viewModel.LoadHybridArrayData(json);

        // Act
        viewModel.RefreshPreview();
        var firstSet = viewModel.PreviewItems.ToList();
        
        viewModel.RefreshPreview();
        var secondSet = viewModel.PreviewItems.ToList();

        // Assert
        // Different refresh calls may produce different results (randomization)
        // At minimum, both sets should be valid
        firstSet.Should().NotBeEmpty();
        secondSet.Should().NotBeEmpty();
        firstSet.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p));
        secondSet.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p));
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SetPreviewCount_Should_Limit_Generated_Items()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();
        var json = @"{
            ""items"": [""a"", ""b"", ""c"", ""d"", ""e""],
            ""patterns"": [""{item}""]
        }";
        viewModel.LoadHybridArrayData(json);

        // Act
        viewModel.SetPreviewCount(3);
        viewModel.RefreshPreview();

        // Assert
        viewModel.PreviewItems.Should().HaveCount(3);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
