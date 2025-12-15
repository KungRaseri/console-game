using FluentValidation;

namespace Game.Core.Settings;

/// <summary>
/// Validates GameSettings configuration
/// </summary>
public class GameSettingsValidator : AbstractValidator<GameSettings>
{
    public GameSettingsValidator()
    {
        RuleFor(x => x.DefaultDifficulty)
            .NotEmpty()
            .Must(d => new[] { "Easy", "Normal", "Hard", "Nightmare" }.Contains(d))
            .WithMessage("Difficulty must be Easy, Normal, Hard, or Nightmare");

        RuleFor(x => x.AutoSaveIntervalSeconds)
            .GreaterThan(0)
            .LessThanOrEqualTo(3600)
            .WithMessage("AutoSave interval must be between 1 and 3600 seconds");

        RuleFor(x => x.MaxSaveSlots)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("MaxSaveSlots must be between 1 and 100");

        RuleFor(x => x.StartingGold)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StartingGold cannot be negative");

        RuleFor(x => x.StartingHealth)
            .GreaterThan(0)
            .LessThanOrEqualTo(9999)
            .WithMessage("StartingHealth must be between 1 and 9999");

        RuleFor(x => x.StartingMana)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(9999)
            .WithMessage("StartingMana must be between 0 and 9999");

        RuleFor(x => x.MaxLevel)
            .GreaterThan(0)
            .LessThanOrEqualTo(999)
            .WithMessage("MaxLevel must be between 1 and 999");

        RuleFor(x => x.ExperiencePerLevel)
            .GreaterThan(0)
            .WithMessage("ExperiencePerLevel must be greater than 0");
    }
}

/// <summary>
/// Validates AudioSettings configuration
/// </summary>
public class AudioSettingsValidator : AbstractValidator<AudioSettings>
{
    public AudioSettingsValidator()
    {
        RuleFor(x => x.MasterVolume)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("MasterVolume must be between 0.0 and 1.0");

        RuleFor(x => x.MusicVolume)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("MusicVolume must be between 0.0 and 1.0");

        RuleFor(x => x.SfxVolume)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("SfxVolume must be between 0.0 and 1.0");
    }
}

/// <summary>
/// Validates UISettings configuration
/// </summary>
public class UISettingsValidator : AbstractValidator<UISettings>
{
    public UISettingsValidator()
    {
        RuleFor(x => x.ColorScheme)
            .NotEmpty()
            .Must(c => new[] { "Default", "Dark", "Light", "Colorful", "Minimal" }.Contains(c))
            .WithMessage("ColorScheme must be Default, Dark, Light, Colorful, or Minimal");

        RuleFor(x => x.AnimationSpeed)
            .NotEmpty()
            .Must(s => new[] { "Slow", "Normal", "Fast", "Instant" }.Contains(s))
            .WithMessage("AnimationSpeed must be Slow, Normal, Fast, or Instant");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}

/// <summary>
/// Validates LoggingSettings configuration
/// </summary>
public class LoggingSettingsValidator : AbstractValidator<LoggingSettings>
{
    public LoggingSettingsValidator()
    {
        RuleFor(x => x.LogLevel)
            .NotEmpty()
            .Must(l => new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" }.Contains(l))
            .WithMessage("LogLevel must be Verbose, Debug, Information, Warning, Error, or Fatal");

        RuleFor(x => x.LogPath)
            .NotEmpty()
            .WithMessage("LogPath cannot be empty");

        RuleFor(x => x.RetainDays)
            .GreaterThan(0)
            .LessThanOrEqualTo(365)
            .WithMessage("RetainDays must be between 1 and 365");
    }
}

/// <summary>
/// Validates GameplaySettings configuration
/// </summary>
public class GameplaySettingsValidator : AbstractValidator<GameplaySettings>
{
    public GameplaySettingsValidator()
    {
        RuleFor(x => x.BattleSpeed)
            .NotEmpty()
            .Must(s => new[] { "Slow", "Normal", "Fast", "Instant" }.Contains(s))
            .WithMessage("BattleSpeed must be Slow, Normal, Fast, or Instant");

        RuleFor(x => x.EncounterRate)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("EncounterRate must be between 0.0 and 1.0");
    }
}
