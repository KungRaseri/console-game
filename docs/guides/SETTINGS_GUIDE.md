# Settings Configuration Guide

## Overview

The game now uses **Microsoft.Extensions.Configuration** for robust, validated, strongly-typed settings management.

## Architecture

```
┌─────────────────────────────────────────────┐
│         Configuration Sources               │
├─────────────────────────────────────────────┤
│  1. appsettings.json (versioned defaults)   │
│  2. appsettings.{Environment}.json (optional│
│  3. Environment Variables (overrides)       │
│  4. .env file (secrets via DotNetEnv)       │
└─────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────┐
│      Strongly-Typed Settings Classes        │
├─────────────────────────────────────────────┤
│  • GameSettings                             │
│  • AudioSettings                            │
│  • UISettings                               │
│  • LoggingSettings                          │
│  • GameplaySettings                         │
└─────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────┐
│      FluentValidation Validators            │
├─────────────────────────────────────────────┤
│  Validates on startup, throws if invalid    │
└─────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────┐
│   Dependency Injection (IOptions<T>)        │
├─────────────────────────────────────────────┤
│  Injected into services and game engine     │
└─────────────────────────────────────────────┘
```

## Files Created

### Configuration Files
- `Game/appsettings.json` - Default game settings (versioned in git)
- `Game/.env` - Secrets and API keys (NOT versioned)

### Settings Classes
- `Game/Settings/GameSettings.cs` - Core game configuration
- `Game/Settings/AudioSettings.cs` - Sound and music settings
- `Game/Settings/UISettings.cs` - User interface preferences
- `Game/Settings/LoggingSettings.cs` - Logging configuration
- `Game/Settings/GameplaySettings.cs` - Gameplay mechanics settings

### Validation
- `Game/Settings/SettingsValidator.cs` - FluentValidation validators for all settings

## Usage Examples

### 1. Inject Settings into Services

```csharp
using Microsoft.Extensions.Options;
using Game.Settings;

public class AudioService
{
    private readonly AudioSettings _settings;
    
    public AudioService(IOptions<AudioSettings> settings)
    {
        _settings = settings.Value;
    }
    
    public void PlayMusic(string file)
    {
        if (!_settings.EnableBackgroundMusic || _settings.Muted)
            return;
            
        var volume = _settings.MasterVolume * _settings.MusicVolume;
        // Play music at calculated volume
    }
}
```

### 2. Access Settings in GameEngine

```csharp
using Microsoft.Extensions.Options;
using Game.Settings;

public class GameEngine
{
    private readonly GameSettings _gameSettings;
    private readonly UISettings _uiSettings;
    private readonly IMediator _mediator;
    
    public GameEngine(
        IMediator mediator,
        IOptions<GameSettings> gameSettings,
        IOptions<UISettings> uiSettings)
    {
        _mediator = mediator;
        _gameSettings = gameSettings.Value;
        _uiSettings = uiSettings.Value;
    }
    
    private async Task HandleCharacterCreationAsync()
    {
        // Use settings
        var character = new Character
        {
            Gold = _gameSettings.StartingGold,
            Health = _gameSettings.StartingHealth,
            Mana = _gameSettings.StartingMana
        };
        
        // Use UI settings
        if (_uiSettings.ShowTutorials)
        {
            ConsoleUI.ShowInfo("Welcome! Create your character...");
        }
    }
}
```

### 3. Override Settings with Environment Variables

```bash
# In .env file or system environment
Game__DefaultDifficulty=Hard
Audio__MasterVolume=0.5
UI__ColorScheme=Dark
```

**Note:** Use double underscores `__` to represent nested JSON structure!

### 4. Environment-Specific Settings

Create `appsettings.Development.json`:
```json
{
  "Gameplay": {
    "EnableCheats": true,
    "PermanentDeath": false
  },
  "Logging": {
    "LogLevel": "Debug"
  }
}
```

Then set environment variable:
```bash
# Windows PowerShell
$env:ENVIRONMENT="Development"

# Run game
dotnet run --project Game
```

## Configuration Sections

### GameSettings
```json
{
  "Game": {
    "Version": "1.0.0",
    "DefaultDifficulty": "Normal",
    "AutoSave": true,
    "AutoSaveIntervalSeconds": 300,
    "MaxSaveSlots": 10,
    "StartingGold": 100,
    "StartingHealth": 100,
    "StartingMana": 50,
    "MaxLevel": 100,
    "ExperiencePerLevel": 100
  }
}
```

**Validation Rules:**
- `DefaultDifficulty`: Must be Easy, Normal, Hard, or Nightmare
- `AutoSaveIntervalSeconds`: 1-3600 seconds
- `MaxSaveSlots`: 1-100
- `StartingHealth`: 1-9999
- `MaxLevel`: 1-999

### AudioSettings
```json
{
  "Audio": {
    "MasterVolume": 0.8,
    "MusicVolume": 0.7,
    "SfxVolume": 0.9,
    "Muted": false,
    "EnableBackgroundMusic": true,
    "EnableSoundEffects": true
  }
}
```

**Validation Rules:**
- All volume settings: 0.0-1.0

### UISettings
```json
{
  "UI": {
    "ColorScheme": "Default",
    "ShowTutorials": true,
    "AnimationSpeed": "Normal",
    "ShowHealthBars": true,
    "ShowDamageNumbers": true,
    "ConfirmOnExit": true,
    "PageSize": 10
  }
}
```

**Validation Rules:**
- `ColorScheme`: Default, Dark, Light, Colorful, or Minimal
- `AnimationSpeed`: Slow, Normal, Fast, or Instant
- `PageSize`: 1-100

### GameplaySettings
```json
{
  "Gameplay": {
    "EnableCheats": false,
    "AllowMultipleSaves": true,
    "PermanentDeath": false,
    "BattleSpeed": "Normal",
    "ShowEnemyStats": true,
    "RandomEncounters": true,
    "EncounterRate": 0.3
  }
}
```

**Validation Rules:**
- `BattleSpeed`: Slow, Normal, Fast, or Instant
- `EncounterRate`: 0.0-1.0

## Validation

Settings are **automatically validated on startup**. If any setting is invalid, the game will:
1. Throw `InvalidOperationException` with detailed error messages
2. Log the validation errors
3. Prevent the game from starting

Example error message:
```
Configuration validation failed for GameSettings:
  - DefaultDifficulty: Difficulty must be Easy, Normal, Hard, or Nightmare
  - StartingHealth: StartingHealth must be between 1 and 9999
```

## Best Practices

### ✅ DO
- **Commit `appsettings.json`** to git (default settings)
- **Use `.env` for secrets** (API keys, passwords)
- **Create environment-specific files** for dev/test/prod (`appsettings.Development.json`)
- **Use `IOptions<T>`** pattern for dependency injection
- **Override with environment variables** for deployment
- **Add validation rules** for new settings

### ❌ DON'T
- Don't hardcode settings in code
- Don't commit `.env` file to git
- Don't skip validation for new settings
- Don't use magic numbers - add them to settings

## Migration from Static Values

**Before:**
```csharp
// Hardcoded values
const int StartingGold = 100;
const int MaxLevel = 100;
```

**After:**
```csharp
// Inject settings
public GameEngine(IOptions<GameSettings> settings)
{
    var gold = settings.Value.StartingGold;
    var maxLevel = settings.Value.MaxLevel;
}
```

## Secrets Management

Keep secrets in `.env` (not versioned):
```bash
# .env
MEDIATR_LICENSE_KEY=your-key-here
DATABASE_CONNECTION=Server=...;Password=...
API_KEY=secret-api-key
```

Access via environment variables:
```csharp
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
```

Or add to configuration:
```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables() // ← Loads from .env via DotNetEnv
    .Build();
```

## Testing

Test with custom settings:
```csharp
[Fact]
public void GameEngine_Should_Use_Configured_Starting_Gold()
{
    // Arrange
    var settings = Options.Create(new GameSettings 
    { 
        StartingGold = 500 
    });
    var engine = new GameEngine(mediator, settings, uiSettings);
    
    // Act & Assert
    var character = engine.CreateCharacter("Hero");
    character.Gold.Should().Be(500);
}
```

## Troubleshooting

### Settings not loading?
1. Check `appsettings.json` is in `Game/` folder
2. Verify `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in `.csproj`
3. Check for JSON syntax errors
4. Look for validation errors in logs

### Environment variables not working?
- Use double underscores: `Game__DefaultDifficulty` not `Game.DefaultDifficulty`
- Set before running: `$env:Game__DefaultDifficulty="Hard"; dotnet run`
- Verify with: `[Environment]::GetEnvironmentVariable("Game__DefaultDifficulty")`

### Validation failing?
- Check allowed values in `SettingsValidator.cs`
- Review error message for specific property and rule
- Verify `appsettings.json` values match validation rules

## Future Enhancements

- [ ] Add `IOptionsMonitor<T>` for hot-reload of settings
- [ ] Create in-game settings menu to modify and save user preferences
- [ ] Add `UserSettingsService` to save to AppData folder
- [ ] Support multiple profiles (e.g., player-specific settings)
- [ ] Add settings import/export functionality
