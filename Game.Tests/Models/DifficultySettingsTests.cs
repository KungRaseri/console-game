using FluentAssertions;
using Game.Core.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Comprehensive tests for DifficultySettings model.
/// Target: 52.7% -> 95%+ coverage.
/// </summary>
public class DifficultySettingsTests
{
    #region Preset Tests - Easy Mode

    [Fact]
    public void Easy_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var easy = DifficultySettings.Easy;

        // Assert
        easy.Name.Should().Be("Easy");
        easy.Description.Should().Contain("Story Mode");
    }

    [Fact]
    public void Easy_Should_Have_Player_Advantage_Multipliers()
    {
        // Act
        var easy = DifficultySettings.Easy;

        // Assert
        easy.PlayerDamageMultiplier.Should().Be(1.5, "player should deal more damage");
        easy.EnemyDamageMultiplier.Should().Be(0.75, "enemies should deal less damage");
        easy.EnemyHealthMultiplier.Should().Be(0.75, "enemies should have less health");
        easy.GoldXPMultiplier.Should().Be(1.5, "should reward more gold and XP");
    }

    [Fact]
    public void Easy_Should_Have_Minimal_Death_Penalties()
    {
        // Act
        var easy = DifficultySettings.Easy;

        // Assert
        easy.GoldLossPercentage.Should().Be(0.05, "minimal gold loss");
        easy.XPLossPercentage.Should().Be(0.10, "minimal XP loss");
        easy.ItemsDroppedOnDeath.Should().Be(0, "no items dropped");
        easy.DropAllInventoryOnDeath.Should().BeFalse();
    }

    [Fact]
    public void Easy_Should_Not_Be_Permadeath_Or_Apocalypse()
    {
        // Act
        var easy = DifficultySettings.Easy;

        // Assert
        easy.IsPermadeath.Should().BeFalse();
        easy.IsApocalypse.Should().BeFalse();
        easy.AutoSaveOnly.Should().BeFalse();
    }

    #endregion

    #region Preset Tests - Normal Mode

    [Fact]
    public void Normal_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var normal = DifficultySettings.Normal;

        // Assert
        normal.Name.Should().Be("Normal");
        normal.Description.Should().Contain("Balanced");
        normal.Description.Should().Contain("Recommended");
    }

    [Fact]
    public void Normal_Should_Have_Balanced_Multipliers()
    {
        // Act
        var normal = DifficultySettings.Normal;

        // Assert
        normal.PlayerDamageMultiplier.Should().Be(1.0, "no player damage modifier");
        normal.EnemyDamageMultiplier.Should().Be(1.0, "no enemy damage modifier");
        normal.EnemyHealthMultiplier.Should().Be(1.0, "no enemy health modifier");
        normal.GoldXPMultiplier.Should().Be(1.0, "no gold/XP modifier");
    }

    [Fact]
    public void Normal_Should_Have_Standard_Death_Penalties()
    {
        // Act
        var normal = DifficultySettings.Normal;

        // Assert
        normal.GoldLossPercentage.Should().Be(0.10, "10% gold loss");
        normal.XPLossPercentage.Should().Be(0.25, "25% XP loss");
        normal.ItemsDroppedOnDeath.Should().Be(1, "one item dropped");
        normal.DropAllInventoryOnDeath.Should().BeFalse();
    }

    #endregion

    #region Preset Tests - Hard Mode

    [Fact]
    public void Hard_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var hard = DifficultySettings.Hard;

        // Assert
        hard.Name.Should().Be("Hard");
        hard.Description.Should().Contain("Experienced");
        hard.Description.Should().Contain("Tactical");
    }

    [Fact]
    public void Hard_Should_Have_Enemy_Advantage_Multipliers()
    {
        // Act
        var hard = DifficultySettings.Hard;

        // Assert
        hard.PlayerDamageMultiplier.Should().Be(0.75, "player deals less damage");
        hard.EnemyDamageMultiplier.Should().Be(1.25, "enemies deal more damage");
        hard.EnemyHealthMultiplier.Should().Be(1.25, "enemies have more health");
        hard.GoldXPMultiplier.Should().Be(1.0, "standard rewards");
    }

    [Fact]
    public void Hard_Should_Have_Severe_Death_Penalties()
    {
        // Act
        var hard = DifficultySettings.Hard;

        // Assert
        hard.GoldLossPercentage.Should().Be(0.20, "20% gold loss");
        hard.XPLossPercentage.Should().Be(0.50, "50% XP loss");
        hard.DropAllInventoryOnDeath.Should().BeTrue("should drop all items");
    }

    #endregion

    #region Preset Tests - Expert Mode

    [Fact]
    public void Expert_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var expert = DifficultySettings.Expert;

        // Assert
        expert.Name.Should().Be("Expert");
        expert.Description.Should().Contain("Brutal");
        expert.Description.Should().Contain("veterans");
    }

    [Fact]
    public void Expert_Should_Have_Harshest_Combat_Multipliers()
    {
        // Act
        var expert = DifficultySettings.Expert;

        // Assert
        expert.PlayerDamageMultiplier.Should().Be(0.50, "player deals half damage");
        expert.EnemyDamageMultiplier.Should().Be(1.50, "enemies deal 50% more damage");
        expert.EnemyHealthMultiplier.Should().Be(1.50, "enemies have 50% more health");
    }

    [Fact]
    public void Expert_Should_Have_Extreme_Death_Penalties()
    {
        // Act
        var expert = DifficultySettings.Expert;

        // Assert
        expert.GoldLossPercentage.Should().Be(0.30, "30% gold loss");
        expert.XPLossPercentage.Should().Be(0.75, "75% XP loss");
        expert.DropAllInventoryOnDeath.Should().BeTrue();
    }

    #endregion

    #region Preset Tests - Ironman Mode

    [Fact]
    public void Ironman_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var ironman = DifficultySettings.Ironman;

        // Assert
        ironman.Name.Should().Be("Ironman");
        ironman.Description.Should().Contain("No Reloading");
        ironman.Description.Should().Contain("permanent");
    }

    [Fact]
    public void Ironman_Should_Have_AutoSave_Only()
    {
        // Act
        var ironman = DifficultySettings.Ironman;

        // Assert
        ironman.AutoSaveOnly.Should().BeTrue("should only auto-save");
        ironman.IsPermadeath.Should().BeFalse("not permadeath, just auto-save");
    }

    [Fact]
    public void Ironman_Should_Have_Hard_Mode_Combat_Multipliers()
    {
        // Act
        var ironman = DifficultySettings.Ironman;

        // Assert
        ironman.PlayerDamageMultiplier.Should().Be(0.75);
        ironman.EnemyDamageMultiplier.Should().Be(1.25);
        ironman.EnemyHealthMultiplier.Should().Be(1.25);
    }

    [Fact]
    public void Ironman_Should_Have_Harsh_Death_Penalties()
    {
        // Act
        var ironman = DifficultySettings.Ironman;

        // Assert
        ironman.GoldLossPercentage.Should().Be(0.25, "25% gold loss");
        ironman.XPLossPercentage.Should().Be(0.50, "50% XP loss");
        ironman.DropAllInventoryOnDeath.Should().BeTrue();
    }

    #endregion

    #region Preset Tests - Permadeath Mode

    [Fact]
    public void Permadeath_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var permadeath = DifficultySettings.Permadeath;

        // Assert
        permadeath.Name.Should().Be("Permadeath");
        permadeath.Description.Should().Contain("Death Deletes Save");
        permadeath.Description.Should().Contain("Hall of Fame");
    }

    [Fact]
    public void Permadeath_Should_Have_IsPermadeath_Flag()
    {
        // Act
        var permadeath = DifficultySettings.Permadeath;

        // Assert
        permadeath.IsPermadeath.Should().BeTrue("death should delete save");
        permadeath.AutoSaveOnly.Should().BeTrue("should only auto-save");
    }

    [Fact]
    public void Permadeath_Should_Have_Expert_Combat_Multipliers()
    {
        // Act
        var permadeath = DifficultySettings.Permadeath;

        // Assert
        permadeath.PlayerDamageMultiplier.Should().Be(0.50);
        permadeath.EnemyDamageMultiplier.Should().Be(1.50);
        permadeath.EnemyHealthMultiplier.Should().Be(1.50);
    }

    [Fact]
    public void Permadeath_Should_Have_Total_Loss_On_Death()
    {
        // Act
        var permadeath = DifficultySettings.Permadeath;

        // Assert
        permadeath.GoldLossPercentage.Should().Be(1.0, "lose all gold");
        permadeath.XPLossPercentage.Should().Be(1.0, "lose all XP");
        permadeath.DropAllInventoryOnDeath.Should().BeTrue("drop all items");
    }

    #endregion

    #region Preset Tests - Apocalypse Mode

    [Fact]
    public void Apocalypse_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var apocalypse = DifficultySettings.Apocalypse;

        // Assert
        apocalypse.Name.Should().Be("Apocalypse");
        apocalypse.Description.Should().Contain("4-Hour");
        apocalypse.Description.Should().Contain("Speed Run");
    }

    [Fact]
    public void Apocalypse_Should_Have_IsApocalypse_Flag()
    {
        // Act
        var apocalypse = DifficultySettings.Apocalypse;

        // Assert
        apocalypse.IsApocalypse.Should().BeTrue("should enable apocalypse timer");
        apocalypse.ApocalypseTimeLimitMinutes.Should().Be(240, "4 hours = 240 minutes");
    }

    [Fact]
    public void Apocalypse_Should_Have_Balanced_Combat_Multipliers()
    {
        // Act
        var apocalypse = DifficultySettings.Apocalypse;

        // Assert
        apocalypse.PlayerDamageMultiplier.Should().Be(1.0, "balanced combat");
        apocalypse.EnemyDamageMultiplier.Should().Be(1.0);
        apocalypse.EnemyHealthMultiplier.Should().Be(1.0);
        apocalypse.GoldXPMultiplier.Should().Be(1.0);
    }

    [Fact]
    public void Apocalypse_Should_Have_Normal_Death_Penalties()
    {
        // Act
        var apocalypse = DifficultySettings.Apocalypse;

        // Assert
        apocalypse.GoldLossPercentage.Should().Be(0.10);
        apocalypse.XPLossPercentage.Should().Be(0.25);
        apocalypse.ItemsDroppedOnDeath.Should().Be(1);
    }

    [Fact]
    public void Apocalypse_Should_Not_Be_Permadeath_Or_AutoSave_Only()
    {
        // Act
        var apocalypse = DifficultySettings.Apocalypse;

        // Assert
        apocalypse.IsPermadeath.Should().BeFalse("apocalypse is not permadeath");
        apocalypse.AutoSaveOnly.Should().BeFalse("player can still save manually");
    }

    #endregion

    #region GetByName Tests

    [Theory]
    [InlineData("Easy")]
    [InlineData("Normal")]
    [InlineData("Hard")]
    [InlineData("Expert")]
    [InlineData("Ironman")]
    [InlineData("Permadeath")]
    [InlineData("Apocalypse")]
    public void GetByName_Should_Return_Correct_Difficulty(string name)
    {
        // Act
        var difficulty = DifficultySettings.GetByName(name);

        // Assert
        difficulty.Should().NotBeNull();
        difficulty.Name.Should().Be(name);
    }

    [Fact]
    public void GetByName_Should_Return_Normal_For_Unknown_Name()
    {
        // Act
        var difficulty = DifficultySettings.GetByName("UnknownMode");

        // Assert
        difficulty.Should().NotBeNull();
        difficulty.Name.Should().Be("Normal", "should default to Normal");
    }

    [Fact]
    public void GetByName_Should_Return_Normal_For_Null()
    {
        // Act
        var difficulty = DifficultySettings.GetByName(null!);

        // Assert
        difficulty.Should().NotBeNull();
        difficulty.Name.Should().Be("Normal");
    }

    [Fact]
    public void GetByName_Should_Return_Normal_For_Empty_String()
    {
        // Act
        var difficulty = DifficultySettings.GetByName("");

        // Assert
        difficulty.Should().NotBeNull();
        difficulty.Name.Should().Be("Normal");
    }

    [Fact]
    public void GetByName_Should_Be_Case_Sensitive()
    {
        // Act
        var lowerCase = DifficultySettings.GetByName("easy");
        var upperCase = DifficultySettings.GetByName("EASY");

        // Assert
        lowerCase.Name.Should().Be("Normal", "case mismatch should return default");
        upperCase.Name.Should().Be("Normal", "case mismatch should return default");
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_Should_Return_All_Seven_Difficulties()
    {
        // Act
        var all = DifficultySettings.GetAll();

        // Assert
        all.Should().HaveCount(7, "should have 7 difficulty modes");
    }

    [Fact]
    public void GetAll_Should_Contain_All_Difficulty_Names()
    {
        // Act
        var all = DifficultySettings.GetAll();
        var names = all.Select(d => d.Name).ToList();

        // Assert
        names.Should().Contain("Easy");
        names.Should().Contain("Normal");
        names.Should().Contain("Hard");
        names.Should().Contain("Expert");
        names.Should().Contain("Ironman");
        names.Should().Contain("Permadeath");
        names.Should().Contain("Apocalypse");
    }

    [Fact]
    public void GetAll_Should_Return_Difficulties_In_Correct_Order()
    {
        // Act
        var all = DifficultySettings.GetAll();

        // Assert
        all[0].Name.Should().Be("Easy");
        all[1].Name.Should().Be("Normal");
        all[2].Name.Should().Be("Hard");
        all[3].Name.Should().Be("Expert");
        all[4].Name.Should().Be("Ironman");
        all[5].Name.Should().Be("Permadeath");
        all[6].Name.Should().Be("Apocalypse");
    }

    [Fact]
    public void GetAll_Should_Return_New_List_Each_Call()
    {
        // Act
        var list1 = DifficultySettings.GetAll();
        var list2 = DifficultySettings.GetAll();

        // Assert
        list1.Should().NotBeSameAs(list2, "should return new list instance");
    }

    [Fact]
    public void GetAll_Difficulties_Should_Have_Descriptions()
    {
        // Act
        var all = DifficultySettings.GetAll();

        // Assert
        all.Should().AllSatisfy(d => d.Description.Should().NotBeNullOrEmpty("all difficulties should have descriptions"));
    }

    #endregion

    #region Property Tests - Custom Instance

    [Fact]
    public void DifficultySettings_Should_Initialize_With_Defaults()
    {
        // Act
        var custom = new DifficultySettings();

        // Assert
        custom.Name.Should().Be("Normal");
        custom.Description.Should().BeEmpty();
        custom.PlayerDamageMultiplier.Should().Be(1.0);
        custom.EnemyDamageMultiplier.Should().Be(1.0);
        custom.EnemyHealthMultiplier.Should().Be(1.0);
        custom.GoldXPMultiplier.Should().Be(1.0);
        custom.AutoSaveOnly.Should().BeFalse();
        custom.IsPermadeath.Should().BeFalse();
        custom.IsApocalypse.Should().BeFalse();
        custom.ApocalypseTimeLimitMinutes.Should().Be(240);
        custom.GoldLossPercentage.Should().Be(0.10);
        custom.XPLossPercentage.Should().Be(0.25);
        custom.DropAllInventoryOnDeath.Should().BeFalse();
        custom.ItemsDroppedOnDeath.Should().Be(1);
    }

    [Fact]
    public void DifficultySettings_Should_Allow_Custom_Values()
    {
        // Act
        var custom = new DifficultySettings
        {
            Name = "Custom",
            Description = "Custom Mode",
            PlayerDamageMultiplier = 2.0,
            EnemyDamageMultiplier = 0.5,
            EnemyHealthMultiplier = 0.5,
            GoldXPMultiplier = 3.0,
            GoldLossPercentage = 0.0,
            XPLossPercentage = 0.0,
            ItemsDroppedOnDeath = 0
        };

        // Assert
        custom.Name.Should().Be("Custom");
        custom.PlayerDamageMultiplier.Should().Be(2.0);
        custom.EnemyDamageMultiplier.Should().Be(0.5);
        custom.GoldXPMultiplier.Should().Be(3.0);
        custom.GoldLossPercentage.Should().Be(0.0);
        custom.XPLossPercentage.Should().Be(0.0);
    }

    #endregion

    #region Difficulty Comparison Tests

    [Fact]
    public void Easy_Should_Be_Easier_Than_Normal()
    {
        // Arrange
        var easy = DifficultySettings.Easy;
        var normal = DifficultySettings.Normal;

        // Assert
        easy.PlayerDamageMultiplier.Should().BeGreaterThan(normal.PlayerDamageMultiplier);
        easy.EnemyDamageMultiplier.Should().BeLessThan(normal.EnemyDamageMultiplier);
        easy.EnemyHealthMultiplier.Should().BeLessThan(normal.EnemyHealthMultiplier);
        easy.GoldLossPercentage.Should().BeLessThan(normal.GoldLossPercentage);
        easy.XPLossPercentage.Should().BeLessThan(normal.XPLossPercentage);
    }

    [Fact]
    public void Hard_Should_Be_Harder_Than_Normal()
    {
        // Arrange
        var hard = DifficultySettings.Hard;
        var normal = DifficultySettings.Normal;

        // Assert
        hard.PlayerDamageMultiplier.Should().BeLessThan(normal.PlayerDamageMultiplier);
        hard.EnemyDamageMultiplier.Should().BeGreaterThan(normal.EnemyDamageMultiplier);
        hard.EnemyHealthMultiplier.Should().BeGreaterThan(normal.EnemyHealthMultiplier);
        hard.GoldLossPercentage.Should().BeGreaterThan(normal.GoldLossPercentage);
        hard.XPLossPercentage.Should().BeGreaterThan(normal.XPLossPercentage);
    }

    [Fact]
    public void Expert_Should_Be_Harder_Than_Hard()
    {
        // Arrange
        var expert = DifficultySettings.Expert;
        var hard = DifficultySettings.Hard;

        // Assert
        expert.PlayerDamageMultiplier.Should().BeLessThan(hard.PlayerDamageMultiplier);
        expert.EnemyDamageMultiplier.Should().BeGreaterThan(hard.EnemyDamageMultiplier);
        expert.EnemyHealthMultiplier.Should().BeGreaterThan(hard.EnemyHealthMultiplier);
        expert.GoldLossPercentage.Should().BeGreaterThan(hard.GoldLossPercentage);
        expert.XPLossPercentage.Should().BeGreaterThan(hard.XPLossPercentage);
    }

    [Fact]
    public void Permadeath_And_Expert_Should_Have_Same_Combat_Difficulty()
    {
        // Arrange
        var permadeath = DifficultySettings.Permadeath;
        var expert = DifficultySettings.Expert;

        // Assert
        permadeath.PlayerDamageMultiplier.Should().Be(expert.PlayerDamageMultiplier);
        permadeath.EnemyDamageMultiplier.Should().Be(expert.EnemyDamageMultiplier);
        permadeath.EnemyHealthMultiplier.Should().Be(expert.EnemyHealthMultiplier);
    }

    #endregion
}
