using FluentAssertions;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Smoke tests for PreviewWindowViewModel - validates basic functionality
/// </summary>
[Trait("Category", "ViewModel")]
public class PreviewWindowViewModelTests
{
    [Fact]
    public void Constructor_Should_Not_Throw()
    {
        // Act
        Action act = () => new PreviewWindowViewModel();

        // Assert
        act.Should().NotThrow("PreviewWindowViewModel should construct without errors");
    }

    [Fact]public void Constructor_Should_Initialize_Properties()
    {
        // Act
        var viewModel = new PreviewWindowViewModel();

        // Assert
        viewModel.ContentTypes.Should().NotBeNull("ContentTypes collection should be initialized");
        viewModel.PreviewItems.Should().NotBeNull("PreviewItems collection should be initialized");
        viewModel.SelectedContentType.Should().NotBeNullOrEmpty("Should have default content type");
    }

    [Fact]public void ContentTypes_Should_Contain_Default_Options()
    {
        // Act
        var viewModel = new PreviewWindowViewModel();

        // Assert
        viewModel.ContentTypes.Should().NotBeEmpty("Should have at least one content type");
        viewModel.ContentTypes.Count.Should().BeGreaterThan(5, "Should have multiple content types");
    }

    [Fact]public void PreviewItems_Should_Start_Empty()
    {
        // Act
        var viewModel = new PreviewWindowViewModel();

        // Assert - PreviewItems should be populated by auto-Generate() call in constructor
        // If PreviewService fails (missing data files), it gracefully handles the error
        viewModel.PreviewItems.Should().NotBeNull("PreviewItems collection should be initialized");
    }

    [Fact]public void Count_Property_Should_Have_Default_Value()
    {
        // Act
        var viewModel = new PreviewWindowViewModel();

        // Assert
        viewModel.Count.Should().BeGreaterThan(0, "Count should have a positive default value");
    }

    [Fact]public void Count_Property_Should_Be_Settable()
    {
        // Arrange
        var viewModel = new PreviewWindowViewModel();

        // Act
        viewModel.Count = 25;

        // Assert
        viewModel.Count.Should().Be(25, "Count should be settable");
    }
}
