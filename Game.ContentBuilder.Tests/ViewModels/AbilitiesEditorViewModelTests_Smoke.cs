using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Smoke tests for AbilitiesEditorViewModel - validates basic functionality
/// Tests ability catalog structure with rarity-based filtering
/// </summary>
public class AbilitiesEditorViewModelTests_Smoke
{
    private const string TestDataPath = "Game.Shared/Data/Enemies";

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Constructor_Should_Load_Boss_Abilities()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            // Skip if file doesn't exist
            return;
        }

        // Act
        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.FileName.Should().NotBeNullOrEmpty();
        viewModel.Abilities.Should().NotBeNull();
        viewModel.FilteredAbilities.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_Abilities_From_File()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        // Act
        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.Abilities.Should().NotBeEmpty("Should load abilities from file");
        viewModel.FilteredAbilities.Should().NotBeEmpty("Should populate filtered abilities");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Abilities_Should_Have_Valid_Structure()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        foreach (var ability in viewModel.Abilities)
        {
            ability.Should().NotBeNull("Each ability should be valid");
            ability.Name.Should().NotBeNullOrEmpty("Ability name should not be empty");
        }
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void StatusMessage_Should_Be_Set_After_Load()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        // Act
        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.StatusMessage.Should().NotBeNullOrEmpty("Status message should be set after loading");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void IsDirty_Should_Be_False_After_Load()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        // Act
        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.IsDirty.Should().BeFalse("File should not be dirty after initial load");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Metadata_Should_Be_Loaded()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        // Act
        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.MetadataType.Should().NotBeNullOrEmpty("Metadata type should be loaded");
        viewModel.MetadataVersion.Should().NotBeNullOrEmpty("Metadata version should be loaded");
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void Should_Load_Multiple_Ability_Files()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var files = new[] 
        { 
            "boss-abilities.json",
            "elite-abilities.json",
            "standard-abilities.json"
        };

        // Act & Assert
        foreach (var file in files)
        {
            if (File.Exists(Path.Combine(TestDataPath, file)))
            {
                Action act = () => new AbilitiesEditorViewModel(service, file);
                act.Should().NotThrow($"Should load {file} without errors");
            }
        }
    }

    [Fact]
    [Trait("Category", "ViewModel")]
    public void AvailableRarities_Should_Include_All_Options()
    {
        // Arrange
        var service = new JsonEditorService(TestDataPath);
        var fileName = "boss-abilities.json";
        
        if (!File.Exists(Path.Combine(TestDataPath, fileName)))
        {
            return;
        }

        var viewModel = new AbilitiesEditorViewModel(service, fileName);

        // Assert
        viewModel.AvailableRarities.Should().Contain("All");
        viewModel.AvailableRarities.Should().Contain("Common");
        viewModel.AvailableRarities.Should().Contain("Uncommon");
        viewModel.AvailableRarities.Should().Contain("Rare");
        viewModel.AvailableRarities.Should().Contain("Epic");
        viewModel.AvailableRarities.Should().Contain("Legendary");
    }
}
