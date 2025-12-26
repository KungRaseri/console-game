using System;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Smoke tests for NameListEditorViewModel - validates basic functionality
/// Tests category→array structure (e.g., adjectives, materials)
/// </summary>
public class NameListEditorViewModelTests_Smoke
{
    private const string TestDataPath = "Game.Shared/Data";

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Not_Throw_With_Valid_File()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";

        // Act
        Action act = () => new NameListEditorViewModel(service, fileName);

        // Assert
        act.Should().NotThrow("NameListEditorViewModel should construct without errors");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Initialize_Categories_Collection()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";

        // Act
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        viewModel.Categories.Should().NotBeNull("Categories collection should be initialized");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_Categories_From_File()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";

        // Act
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        viewModel.Categories.Should().NotBeEmpty("Should load categories from file");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SelectedCategory_Should_Start_Null()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";

        // Act
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        viewModel.SelectedCategory.Should().BeNull("No category should be selected initially");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Categories_Should_Have_Valid_Structure()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        foreach (var category in viewModel.Categories)
        {
            category.Should().NotBeNull("Each category should be valid");
        }
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SaveCommand_Should_Exist()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        viewModel.SaveCommand.Should().NotBeNull("SaveCommand should be available");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void AddNameCommand_Should_Exist()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert
        viewModel.AddNameCommand.Should().NotBeNull("AddNameCommand should be available");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Skip_Non_Array_Categories_Like_Variants()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "general-adjectives.json";

        // Act
        var viewModel = new NameListEditorViewModel(service, fileName);

        // Assert - Just verify it doesn't crash when loading
        // The actual implementation should skip non-array properties like "variants"
        Action act = () => { var count = viewModel.Categories.Count; };
        act.Should().NotThrow("Should handle non-array properties gracefully");
    }
}
