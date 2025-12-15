using FluentAssertions;
using FluentValidation.TestHelper;
using Game.Core.Settings;
using Xunit;

namespace Game.Tests.Settings;

public class GameSettingsValidatorTests
{
    private readonly GameSettingsValidator _validator = new();

    [Theory]
    [InlineData("Easy")]
    [InlineData("Normal")]
    [InlineData("Hard")]
    [InlineData("Nightmare")]
    public void Should_Not_Have_Error_When_Difficulty_Is_Valid(string difficulty)
    {
        // Arrange
        var settings = new GameSettings { DefaultDifficulty = difficulty };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.DefaultDifficulty);
    }

    [Theory]
    [InlineData("Impossible")]
    [InlineData("")]
    [InlineData("easy")] // Case sensitive
    public void Should_Have_Error_When_Difficulty_Is_Invalid(string difficulty)
    {
        // Arrange
        var settings = new GameSettings { DefaultDifficulty = difficulty };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.DefaultDifficulty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(300)]
    [InlineData(3600)]
    public void Should_Not_Have_Error_When_AutoSaveInterval_Is_Valid(int interval)
    {
        // Arrange
        var settings = new GameSettings { AutoSaveIntervalSeconds = interval };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.AutoSaveIntervalSeconds);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3601)]
    public void Should_Have_Error_When_AutoSaveInterval_Is_Invalid(int interval)
    {
        // Arrange
        var settings = new GameSettings { AutoSaveIntervalSeconds = interval };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.AutoSaveIntervalSeconds);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 999)]
    public void Should_Not_Have_Error_When_Health_And_Level_Are_Valid(int health, int maxLevel)
    {
        // Arrange
        var settings = new GameSettings 
        { 
            StartingHealth = health,
            MaxLevel = maxLevel 
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.StartingHealth);
        result.ShouldNotHaveValidationErrorFor(s => s.MaxLevel);
    }

    [Fact]
    public void Should_Have_Error_When_StartingHealth_Is_Zero_Or_Negative()
    {
        // Arrange
        var settings = new GameSettings { StartingHealth = 0 };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.StartingHealth);
    }

    [Fact]
    public void Should_Have_Error_When_StartingGold_Is_Negative()
    {
        // Arrange
        var settings = new GameSettings { StartingGold = -1 };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.StartingGold);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Settings_Are_Valid()
    {
        // Arrange
        var settings = new GameSettings
        {
            DefaultDifficulty = "Normal",
            AutoSaveIntervalSeconds = 300,
            MaxSaveSlots = 10,
            StartingGold = 100,
            StartingHealth = 100,
            StartingMana = 50,
            MaxLevel = 100,
            ExperiencePerLevel = 100
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class AudioSettingsValidatorTests
{
    private readonly AudioSettingsValidator _validator = new();

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Should_Not_Have_Error_When_Volumes_Are_Valid(double volume)
    {
        // Arrange
        var settings = new AudioSettings 
        { 
            MasterVolume = volume,
            MusicVolume = volume,
            SfxVolume = volume 
        };

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(s => s.MasterVolume);
        result.ShouldNotHaveValidationErrorFor(s => s.MusicVolume);
        result.ShouldNotHaveValidationErrorFor(s => s.SfxVolume);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void Should_Have_Error_When_MasterVolume_Is_Out_Of_Range(double volume)
    {
        // Arrange
        var settings = new AudioSettings { MasterVolume = volume };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.MasterVolume);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Settings_Are_Valid()
    {
        // Arrange
        var settings = new AudioSettings
        {
            MasterVolume = 0.8,
            MusicVolume = 0.7,
            SfxVolume = 0.9,
            Muted = false,
            EnableBackgroundMusic = true,
            EnableSoundEffects = true
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class UISettingsValidatorTests
{
    private readonly UISettingsValidator _validator = new();

    [Theory]
    [InlineData("Default")]
    [InlineData("Dark")]
    [InlineData("Light")]
    [InlineData("Colorful")]
    [InlineData("Minimal")]
    public void Should_Not_Have_Error_When_ColorScheme_Is_Valid(string colorScheme)
    {
        // Arrange
        var settings = new UISettings { ColorScheme = colorScheme };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.ColorScheme);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("dark")] // Case sensitive
    public void Should_Have_Error_When_ColorScheme_Is_Invalid(string colorScheme)
    {
        // Arrange
        var settings = new UISettings { ColorScheme = colorScheme };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.ColorScheme);
    }

    [Theory]
    [InlineData("Slow")]
    [InlineData("Normal")]
    [InlineData("Fast")]
    [InlineData("Instant")]
    public void Should_Not_Have_Error_When_AnimationSpeed_Is_Valid(string speed)
    {
        // Arrange
        var settings = new UISettings { AnimationSpeed = speed };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.AnimationSpeed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_Not_Have_Error_When_PageSize_Is_Valid(int pageSize)
    {
        // Arrange
        var settings = new UISettings { PageSize = pageSize };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_Have_Error_When_PageSize_Is_Invalid(int pageSize)
    {
        // Arrange
        var settings = new UISettings { PageSize = pageSize };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.PageSize);
    }
}

public class LoggingSettingsValidatorTests
{
    private readonly LoggingSettingsValidator _validator = new();

    [Theory]
    [InlineData("Verbose")]
    [InlineData("Debug")]
    [InlineData("Information")]
    [InlineData("Warning")]
    [InlineData("Error")]
    [InlineData("Fatal")]
    public void Should_Not_Have_Error_When_LogLevel_Is_Valid(string logLevel)
    {
        // Arrange
        var settings = new LoggingSettings { LogLevel = logLevel };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.LogLevel);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("info")] // Case sensitive
    public void Should_Have_Error_When_LogLevel_Is_Invalid(string logLevel)
    {
        // Arrange
        var settings = new LoggingSettings { LogLevel = logLevel };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.LogLevel);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(365)]
    public void Should_Not_Have_Error_When_RetainDays_Is_Valid(int days)
    {
        // Arrange
        var settings = new LoggingSettings { RetainDays = days };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.RetainDays);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(366)]
    public void Should_Have_Error_When_RetainDays_Is_Invalid(int days)
    {
        // Arrange
        var settings = new LoggingSettings { RetainDays = days };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.RetainDays);
    }

    [Fact]
    public void Should_Have_Error_When_LogPath_Is_Empty()
    {
        // Arrange
        var settings = new LoggingSettings { LogPath = "" };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.LogPath);
    }
}

public class GameplaySettingsValidatorTests
{
    private readonly GameplaySettingsValidator _validator = new();

    [Theory]
    [InlineData("Slow")]
    [InlineData("Normal")]
    [InlineData("Fast")]
    [InlineData("Instant")]
    public void Should_Not_Have_Error_When_BattleSpeed_Is_Valid(string speed)
    {
        // Arrange
        var settings = new GameplaySettings { BattleSpeed = speed };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.BattleSpeed);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("")]
    [InlineData("fast")] // Case sensitive
    public void Should_Have_Error_When_BattleSpeed_Is_Invalid(string speed)
    {
        // Arrange
        var settings = new GameplaySettings { BattleSpeed = speed };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.BattleSpeed);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Should_Not_Have_Error_When_EncounterRate_Is_Valid(double rate)
    {
        // Arrange
        var settings = new GameplaySettings { EncounterRate = rate };

        // Act & Assert
        _validator.TestValidate(settings).ShouldNotHaveValidationErrorFor(s => s.EncounterRate);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Should_Have_Error_When_EncounterRate_Is_Out_Of_Range(double rate)
    {
        // Arrange
        var settings = new GameplaySettings { EncounterRate = rate };

        // Act & Assert
        _validator.TestValidate(settings).ShouldHaveValidationErrorFor(s => s.EncounterRate);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Settings_Are_Valid()
    {
        // Arrange
        var settings = new GameplaySettings
        {
            EnableCheats = false,
            AllowMultipleSaves = true,
            PermanentDeath = false,
            BattleSpeed = "Normal",
            ShowEnemyStats = true,
            RandomEncounters = true,
            EncounterRate = 0.3
        };

        // Act
        var result = _validator.Validate(settings);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
