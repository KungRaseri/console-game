// QUICK START: Using Settings in Your Code
// ==========================================

// Example 1: Inject GameSettings into GameEngine
// -----------------------------------------------
using Microsoft.Extensions.Options;
using Game.Settings;

public class GameEngine
{
    private readonly GameSettings _gameSettings;
    private readonly UISettings _uiSettings;

    // Add IOptions<T> to constructor
    public GameEngine(
        IMediator mediator,
        IOptions<GameSettings> gameSettings,
        IOptions<UISettings> uiSettings)
    {
        _mediator = mediator;
        _gameSettings = gameSettings.Value;  // ← Get settings value
        _uiSettings = uiSettings.Value;
    }

    // Use settings instead of hardcoded values
    private Character CreateNewCharacter(string name)
    {
        return new Character
        {
            Name = name,
            Level = 1,
            Gold = _gameSettings.StartingGold,      // ← From config
            Health = _gameSettings.StartingHealth,  // ← From config
            MaxHealth = _gameSettings.StartingHealth,
            Mana = _gameSettings.StartingMana,      // ← From config
            MaxMana = _gameSettings.StartingMana,
            Experience = 0
        };
    }

    // Use UI settings
    private async Task HandleMainMenuAsync()
    {
        if (_uiSettings.ShowTutorials)  // ← From config
        {
            ConsoleUI.ShowInfo("Welcome to the game! Use arrow keys to navigate menus.");
        }

        // Use page size setting
        var choice = ConsoleUI.ShowMenu(
            "Main Menu",
            "New Game",
            "Load Game",
            "Settings",
            "Exit"
        );
    }
}

// Example 2: Inject AudioSettings into AudioService
// --------------------------------------------------
public class AudioService
{
    private readonly AudioSettings _settings;

    public AudioService(IOptions<AudioSettings> settings)
    {
        _settings = settings.Value;
    }

    public void PlayBackgroundMusic(string filePath)
    {
        if (!_settings.EnableBackgroundMusic || _settings.Muted)
            return;

        // Calculate effective volume
        var effectiveVolume = _settings.MasterVolume * _settings.MusicVolume;

        // Play music with calculated volume
        // ... NAudio code here
    }

    public void PlaySoundEffect(string filePath)
    {
        if (!_settings.EnableSoundEffects || _settings.Muted)
            return;

        var effectiveVolume = _settings.MasterVolume * _settings.SfxVolume;

        // Play SFX
    }
}

// Example 3: Override Settings via Environment Variables
// -------------------------------------------------------
// In PowerShell (before running game):
//   $env:Game__DefaultDifficulty = "Hard"
//   $env:Audio__MasterVolume = "0.5"
//   $env:UI__ColorScheme = "Dark"
//   dotnet run --project Game

// Example 4: Test with Custom Settings
// -------------------------------------
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;

public class GameEngineTests
{
    [Fact]
    public void Should_Create_Character_With_Configured_Starting_Values()
    {
        // Arrange - Create custom test settings
        var gameSettings = Options.Create(new GameSettings
        {
            StartingGold = 500,
            StartingHealth = 200,
            StartingMana = 75
        });

        var uiSettings = Options.Create(new UISettings());

        var mockMediator = new Mock<IMediator>();
        var engine = new GameEngine(mockMediator.Object, gameSettings, uiSettings);

        // Act
        var character = engine.CreateNewCharacter("TestHero");

        // Assert
        character.Gold.Should().Be(500);
        character.Health.Should().Be(200);
        character.Mana.Should().Be(75);
    }
}

// Example 5: Read Settings Directly (Without DI)
// -----------------------------------------------
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var gameDifficulty = configuration["Game:DefaultDifficulty"];  // "Normal"
var masterVolume = configuration.GetValue<double>("Audio:MasterVolume");  // 0.8
