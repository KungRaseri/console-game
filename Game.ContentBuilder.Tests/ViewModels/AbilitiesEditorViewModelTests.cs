using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FluentAssertions;
using Game.ContentBuilder.Services;
using Game.ContentBuilder.ViewModels;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Game.ContentBuilder.Tests.ViewModels;

/// <summary>
/// Comprehensive unit tests for AbilitiesEditorViewModel
/// Tests ability management, filtering, and search operations
/// </summary>
[Trait("Category", "ViewModel")]
public class AbilitiesEditorViewModelTests : IDisposable
{
    private readonly string _testDataPath;
    private readonly string _testFileName;
    private readonly JsonEditorService _jsonService;

    public AbilitiesEditorViewModelTests()
    {
        // Setup test data directory
        _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDataPath);
        
        _testFileName = "test-abilities.json";
        _jsonService = new JsonEditorService(_testDataPath);
        
        CreateTestAbilitiesFile();
    }

    private void CreateTestAbilitiesFile()
    {
        var testData = new JObject
        {
            ["metadata"] = new JObject
            {
                ["version"] = "1.0",
                ["type"] = "ability_catalog",
                ["description"] = "Test abilities",
                ["usage"] = "Testing",
                ["notes"] = new JArray { "Test note 1", "Test note 2" },
                ["total_abilities"] = 5
            },
            ["items"] = new JArray
            {
                new JObject
                {
                    ["name"] = "fireball",
                    ["displayName"] = "Fireball",
                    ["description"] = "Launches a ball of fire",
                    ["rarityWeight"] = 10
                },
                new JObject
                {
                    ["name"] = "ice_blast",
                    ["displayName"] = "Ice Blast",
                    ["description"] = "Freezes enemies in place",
                    ["rarityWeight"] = 35
                },
                new JObject
                {
                    ["name"] = "lightning_strike",
                    ["displayName"] = "Lightning Strike",
                    ["description"] = "Calls down a lightning bolt",
                    ["rarityWeight"] = 75
                },
                new JObject
                {
                    ["name"] = "earthquake",
                    ["displayName"] = "Earthquake",
                    ["description"] = "Shakes the ground violently",
                    ["rarityWeight"] = 150
                },
                new JObject
                {
                    ["name"] = "meteor_shower",
                    ["displayName"] = "Meteor Shower",
                    ["description"] = "Summons meteors from the sky",
                    ["rarityWeight"] = 250
                }
            }
        };

        File.WriteAllText(
            Path.Combine(_testDataPath, _testFileName),
            testData.ToString(Newtonsoft.Json.Formatting.Indented));
    }

    [Fact]
    public void Constructor_Should_Initialize_ViewModel_With_Valid_Data()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.FileName.Should().NotBeNullOrEmpty();
        viewModel.Abilities.Should().NotBeEmpty();
        viewModel.FilteredAbilities.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_Should_Load_Metadata_From_File()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.MetadataVersion.Should().Be("1.0");
        viewModel.MetadataType.Should().Be("ability_catalog");
        viewModel.MetadataDescription.Should().Be("Test abilities");
        viewModel.MetadataUsage.Should().Be("Testing");
    }

    [Fact]
    public void Constructor_Should_Load_Metadata_Notes()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.MetadataNotes.Should().HaveCount(2);
        viewModel.MetadataNotes.Should().Contain("Test note 1");
        viewModel.MetadataNotes.Should().Contain("Test note 2");
    }

    [Fact]
    public void Constructor_Should_Load_Abilities_From_File()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.Abilities.Should().HaveCount(5);
        viewModel.Abilities.Should().Contain(a => a.Name == "fireball");
        viewModel.Abilities.Should().Contain(a => a.Name == "ice_blast");
        viewModel.Abilities.Should().Contain(a => a.Name == "lightning_strike");
    }

    [Fact]
    public void Constructor_Should_Load_Ability_Properties()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        var fireball = viewModel.Abilities.First(a => a.Name == "fireball");
        fireball.DisplayName.Should().Be("Fireball");
        fireball.Description.Should().Be("Launches a ball of fire");
        fireball.RarityWeight.Should().Be(10);
    }

    [Fact]
    public void Constructor_Should_Initialize_FilteredAbilities_With_All_Abilities()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(viewModel.Abilities.Count);
    }

    [Fact]
    public void IsDirty_Should_Be_False_After_Initial_Load()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsEditing_Should_Be_False_Initially()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void AvailableRarities_Should_Contain_All_Options()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.AvailableRarities.Should().Contain("All");
        viewModel.AvailableRarities.Should().Contain("Common");
        viewModel.AvailableRarities.Should().Contain("Uncommon");
        viewModel.AvailableRarities.Should().Contain("Rare");
        viewModel.AvailableRarities.Should().Contain("Epic");
        viewModel.AvailableRarities.Should().Contain("Legendary");
    }

    [Fact]
    public void FilterRarity_Should_Default_To_All()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.FilterRarity.Should().Be("All");
    }

    [Fact]
    public void SearchText_Should_Be_Empty_Initially()
    {
        // Act
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Assert
        viewModel.SearchText.Should().BeEmpty();
    }

    [Fact]
    public void FilterRarity_Change_Should_Filter_Abilities()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.FilterRarity = "Rare";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(1);
        viewModel.FilteredAbilities.First().RarityWeight.Should().Be(75);
    }

    [Fact]
    public void FilterRarity_All_Should_Show_All_Abilities()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.FilterRarity = "Rare";

        // Act
        viewModel.FilterRarity = "All";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(viewModel.Abilities.Count);
    }

    [Fact]
    public void SearchText_Change_Should_Filter_By_Name()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.SearchText = "fire";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(1);
        viewModel.FilteredAbilities.First().Name.Should().Contain("fire");
    }

    [Fact]
    public void SearchText_Change_Should_Filter_By_DisplayName()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.SearchText = "Ice Blast";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(1);
        viewModel.FilteredAbilities.First().DisplayName.Should().Contain("Ice Blast");
    }

    [Fact]
    public void SearchText_Change_Should_Filter_By_Description()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.SearchText = "meteors";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(1);
        viewModel.FilteredAbilities.First().Description.Should().Contain("meteors");
    }

    [Fact]
    public void Search_And_Filter_Should_Work_Together()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.SearchText = "strike";
        viewModel.FilterRarity = "Rare";

        // Assert
        viewModel.FilteredAbilities.Should().HaveCount(1);
        var ability = viewModel.FilteredAbilities.First();
        ability.Name.Should().Contain("strike");
        ability.RarityWeight.Should().Be(75);
    }

    [Fact]
    public void AddAbilityCommand_Should_Enter_Edit_Mode()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.AddAbilityCommand.Execute(null);

        // Assert
        viewModel.IsEditing.Should().BeTrue();
        viewModel.EditName.Should().BeEmpty();
        viewModel.EditDisplayName.Should().BeEmpty();
        viewModel.EditDescription.Should().BeEmpty();
        viewModel.EditRarityWeight.Should().Be(10);
    }

    [Fact]
    public void EditAbilityCommand_Should_Populate_Edit_Fields()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        var ability = viewModel.Abilities.First();
        viewModel.SelectedAbility = ability;

        // Act
        viewModel.EditAbilityCommand.Execute(null);

        // Assert
        viewModel.IsEditing.Should().BeTrue();
        viewModel.EditName.Should().Be(ability.Name);
        viewModel.EditDisplayName.Should().Be(ability.DisplayName);
        viewModel.EditDescription.Should().Be(ability.Description);
        viewModel.EditRarityWeight.Should().Be(ability.RarityWeight);
    }

    [Fact]
    public void SaveEditCommand_Should_Add_New_Ability()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);
        var initialCount = viewModel.Abilities.Count;

        // Act
        viewModel.EditName = "new_ability";
        viewModel.EditDisplayName = "New Ability";
        viewModel.EditDescription = "A new test ability";
        viewModel.EditRarityWeight = 35;
        viewModel.SaveEditCommand.Execute(null);

        // Assert
        viewModel.Abilities.Should().HaveCount(initialCount + 1);
        viewModel.IsEditing.Should().BeFalse();
        viewModel.IsDirty.Should().BeTrue();
        
        var newAbility = viewModel.Abilities.Last();
        newAbility.Name.Should().Be("new_ability");
        newAbility.DisplayName.Should().Be("New Ability");
        newAbility.RarityWeight.Should().Be(35);
    }

    [Fact]
    public void SaveEditCommand_Should_Use_Name_As_DisplayName_If_Empty()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);

        // Act
        viewModel.EditName = "test_ability";
        viewModel.EditDisplayName = "";
        viewModel.SaveEditCommand.Execute(null);

        // Assert
        var newAbility = viewModel.Abilities.Last();
        newAbility.DisplayName.Should().Be("test_ability");
    }

    [Fact]
    public void SaveEditCommand_Should_Update_Existing_Ability()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        var ability = viewModel.Abilities.First();
        viewModel.SelectedAbility = ability;
        viewModel.EditAbilityCommand.Execute(null);

        // Act
        viewModel.EditName = "updated_name";
        viewModel.EditDisplayName = "Updated Display";
        viewModel.EditDescription = "Updated description";
        viewModel.EditRarityWeight = 150;
        viewModel.SaveEditCommand.Execute(null);

        // Assert
        ability.Name.Should().Be("updated_name");
        ability.DisplayName.Should().Be("Updated Display");
        ability.Description.Should().Be("Updated description");
        ability.RarityWeight.Should().Be(150);
        viewModel.IsEditing.Should().BeFalse();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void SaveEditCommand_Should_Not_Save_Without_Name()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);
        var initialCount = viewModel.Abilities.Count;

        // Act
        viewModel.EditName = "";
        viewModel.SaveEditCommand.Execute(null);

        // Assert
        viewModel.Abilities.Should().HaveCount(initialCount);
        viewModel.IsEditing.Should().BeTrue();
        viewModel.StatusMessage.Should().Contain("Error");
    }

    [Fact]
    public void CancelEditCommand_Should_Exit_Edit_Mode()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);

        // Act
        viewModel.CancelEditCommand.Execute(null);

        // Assert
        viewModel.IsEditing.Should().BeFalse();
    }

    [Fact]
    public void DeleteAbilityCommand_Should_Remove_Selected_Ability()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        var abilityToDelete = viewModel.Abilities.First();
        viewModel.SelectedAbility = abilityToDelete;
        var initialCount = viewModel.Abilities.Count;

        // Act
        viewModel.DeleteAbilityCommand.Execute(null);

        // Assert
        viewModel.Abilities.Should().HaveCount(initialCount - 1);
        viewModel.Abilities.Should().NotContain(abilityToDelete);
        viewModel.SelectedAbility.Should().BeNull();
        viewModel.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void ClearSearchCommand_Should_Reset_Filters()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.SearchText = "fire";
        viewModel.FilterRarity = "Rare";

        // Act
        viewModel.ClearSearchCommand.Execute(null);

        // Assert
        viewModel.SearchText.Should().BeEmpty();
        viewModel.FilterRarity.Should().Be("All");
    }

    [Fact]
    public void SaveFileCommand_Should_Mark_IsDirty_As_False()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);
        viewModel.EditName = "test";
        viewModel.SaveEditCommand.Execute(null);

        // Act
        viewModel.SaveFileCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void SaveFileCommand_Should_Update_Status_Message()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.SaveFileCommand.Execute(null);

        // Assert
        viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        viewModel.StatusMessage.Should().Contain("Saved");
    }

    [Fact]
    public void ReloadFileCommand_Should_Reset_IsDirty()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);
        viewModel.AddAbilityCommand.Execute(null);
        viewModel.EditName = "test";
        viewModel.SaveEditCommand.Execute(null);

        // Act
        viewModel.ReloadFileCommand.Execute(null);

        // Assert
        viewModel.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void StatusMessage_Should_Reflect_Filtered_Count()
    {
        // Arrange
        var viewModel = new AbilitiesEditorViewModel(_jsonService, _testFileName);

        // Act
        viewModel.FilterRarity = "Rare";

        // Assert
        viewModel.StatusMessage.Should().Contain("Showing 1 of 5");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
