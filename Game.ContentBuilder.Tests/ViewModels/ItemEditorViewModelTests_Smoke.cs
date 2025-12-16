using System;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Smoke tests for ItemEditorViewModel - validates basic functionality
/// Tests prefix/suffix item structure
/// </summary>
public class ItemEditorViewModelTests_Smoke
{
    private const string TestDataPath = "Game.Shared/Data";

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Not_Throw_With_Valid_File()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";

        // Act
        Action act = () => new ItemEditorViewModel(service, fileName);

        // Assert
        act.Should().NotThrow("ItemEditorViewModel should construct without errors");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Initialize_Items_Collection()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";

        // Act
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        viewModel.Items.Should().NotBeNull("Items collection should be initialized");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_Items_From_File()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";

        // Act
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        viewModel.Items.Should().NotBeEmpty("Should load items from file");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SelectedItem_Should_Start_Null()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";

        // Act
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        viewModel.SelectedItem.Should().BeNull("No item should be selected initially");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Items_Should_Have_Valid_Structure()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        foreach (var item in viewModel.Items)
        {
            item.Should().NotBeNull("Each item should be valid");
        }
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void SaveCommand_Should_Exist()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        viewModel.SaveCommand.Should().NotBeNull("SaveCommand should be available");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void AddItemCommand_Should_Exist()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "items-weapons-melee-swords.json";
        var viewModel = new ItemEditorViewModel(service, fileName);

        // Assert
        viewModel.AddItemCommand.Should().NotBeNull("AddItemCommand should be available");
    }
}
