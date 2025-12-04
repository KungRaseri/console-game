using FluentAssertions;
using Game.Settings;
using Xunit;

namespace Game.Tests.Settings;

public class GameSettingsTests
{
    [Fact]
    public void GameSettings_Should_Initialize_With_Valid_Defaults()
    {
        // Arrange & Act
        var settings = new GameSettings();
        var validator = new GameSettingsValidator();

        // Assert - Defaults should pass validation
        var result = validator.Validate(settings);
        result.IsValid.Should().BeTrue("default settings should be valid");

        // Test critical invariants (not specific values that might change)
        settings.Version.Should().NotBeNullOrEmpty();
        settings.DefaultDifficulty.Should().NotBeNullOrEmpty();
        settings.AutoSaveIntervalSeconds.Should().BeGreaterThan(0);
        settings.MaxSaveSlots.Should().BeGreaterThan(0);
        settings.StartingHealth.Should().BeGreaterThan(0);
        settings.MaxLevel.Should().BeGreaterThan(0);
        settings.ExperiencePerLevel.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GameSettings_Properties_Should_Be_Settable()
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        settings.DefaultDifficulty = "Hard";
        settings.StartingGold = 500;
        settings.MaxLevel = 50;

        // Assert
        settings.DefaultDifficulty.Should().Be("Hard");
        settings.StartingGold.Should().Be(500);
        settings.MaxLevel.Should().Be(50);
    }
}

public class AudioSettingsTests
{
    [Fact]
    public void AudioSettings_Should_Initialize_With_Valid_Defaults()
    {
        // Arrange & Act
        var settings = new AudioSettings();
        var validator = new AudioSettingsValidator();

        // Assert - Defaults should pass validation
        var result = validator.Validate(settings);
        result.IsValid.Should().BeTrue("default settings should be valid");

        // Test critical invariants (volume in valid range)
        settings.MasterVolume.Should().BeInRange(0.0, 1.0);
        settings.MusicVolume.Should().BeInRange(0.0, 1.0);
        settings.SfxVolume.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void AudioSettings_Properties_Should_Be_Settable()
    {
        // Arrange
        var settings = new AudioSettings();

        // Act
        settings.MasterVolume = 0.5;
        settings.Muted = true;
        settings.EnableBackgroundMusic = false;

        // Assert
        settings.MasterVolume.Should().Be(0.5);
        settings.Muted.Should().BeTrue();
        settings.EnableBackgroundMusic.Should().BeFalse();
    }
}

public class UISettingsTests
{
    [Fact]
    public void UISettings_Should_Initialize_With_Valid_Defaults()
    {
        // Arrange & Act
        var settings = new UISettings();
        var validator = new UISettingsValidator();

        // Assert - Defaults should pass validation
        var result = validator.Validate(settings);
        result.IsValid.Should().BeTrue("default settings should be valid");

        // Test critical invariants
        settings.ColorScheme.Should().NotBeNullOrEmpty();
        settings.AnimationSpeed.Should().NotBeNullOrEmpty();
        settings.PageSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UISettings_Properties_Should_Be_Settable()
    {
        // Arrange
        var settings = new UISettings();

        // Act
        settings.ColorScheme = "Dark";
        settings.ShowTutorials = false;
        settings.PageSize = 20;

        // Assert
        settings.ColorScheme.Should().Be("Dark");
        settings.ShowTutorials.Should().BeFalse();
        settings.PageSize.Should().Be(20);
    }
}

public class LoggingSettingsTests
{
    [Fact]
    public void LoggingSettings_Should_Initialize_With_Valid_Defaults()
    {
        // Arrange & Act
        var settings = new LoggingSettings();
        var validator = new LoggingSettingsValidator();

        // Assert - Defaults should pass validation
        var result = validator.Validate(settings);
        result.IsValid.Should().BeTrue("default settings should be valid");

        // Test critical invariants
        settings.LogLevel.Should().NotBeNullOrEmpty();
        settings.LogPath.Should().NotBeNullOrEmpty();
        settings.RetainDays.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LoggingSettings_Properties_Should_Be_Settable()
    {
        // Arrange
        var settings = new LoggingSettings();

        // Act
        settings.LogLevel = "Debug";
        settings.RetainDays = 30;
        settings.LogPath = "custom/logs/";

        // Assert
        settings.LogLevel.Should().Be("Debug");
        settings.RetainDays.Should().Be(30);
        settings.LogPath.Should().Be("custom/logs/");
    }
}

public class GameplaySettingsTests
{
    [Fact]
    public void GameplaySettings_Should_Initialize_With_Valid_Defaults()
    {
        // Arrange & Act
        var settings = new GameplaySettings();
        var validator = new GameplaySettingsValidator();

        // Assert - Defaults should pass validation
        var result = validator.Validate(settings);
        result.IsValid.Should().BeTrue("default settings should be valid");

        // Test critical invariants
        settings.BattleSpeed.Should().NotBeNullOrEmpty();
        settings.EncounterRate.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public void GameplaySettings_Properties_Should_Be_Settable()
    {
        // Arrange
        var settings = new GameplaySettings();

        // Act
        settings.EnableCheats = true;
        settings.BattleSpeed = "Fast";
        settings.EncounterRate = 0.5;

        // Assert
        settings.EnableCheats.Should().BeTrue();
        settings.BattleSpeed.Should().Be("Fast");
        settings.EncounterRate.Should().Be(0.5);
    }
}
