using Game.Models;
using Game.UI;
using Game.Handlers;
using MediatR;
using Serilog;
using Polly;
using Polly.Retry;

namespace Game;

/// <summary>
/// Core game engine that manages the game loop and state
/// </summary>
public class GameEngine
{
    private readonly IMediator _mediator;
    private readonly ResiliencePipeline _resiliencePipeline;
    private Character? _player;
    private GameState _state;
    private bool _isRunning;

    public GameEngine(IMediator mediator)
    {
        _mediator = mediator;
        _state = GameState.MainMenu;
        _isRunning = false;

        // Configure Polly resilience pipeline for error handling
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    Log.Warning("Retrying operation (attempt {Attempt})", args.AttemptNumber);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Main game loop - processes turns until game exits
    /// </summary>
    public async Task RunAsync()
    {
        _isRunning = true;
        Log.Information("Game engine starting");

        try
        {
            // Initialize
            await InitializeLoadingScreenAsync();

            // Main game loop
            while (_isRunning)
            {
                try
                {
                    // Execute with resilience
                    await _resiliencePipeline.ExecuteAsync(async ct =>
                    {
                        await ProcessGameTickAsync();
                    });

                    // Small delay to prevent CPU spinning (optional)
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in game loop");
                    ConsoleUI.ShowError($"An error occurred: {ex.Message}");

                    // Ask if player wants to continue
                    if (!ConsoleUI.Confirm("Continue playing?"))
                    {
                        _isRunning = false;
                    }
                }
            }

            // Cleanup
            await ShutdownGameAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in game engine");
            ConsoleUI.ShowError($"Fatal error: {ex.Message}");
        }
        finally
        {
            Log.Information("Game engine stopped");
        }
    }

    /// <summary>
    /// Processes a single game "tick" or turn
    /// </summary>
    private async Task ProcessGameTickAsync()
    {
        switch (_state)
        {
            case GameState.MainMenu:
                await HandleMainMenuAsync();
                break;

            case GameState.CharacterCreation:
                await HandleCharacterCreationAsync();
                break;

            case GameState.InGame:
                await HandleInGameAsync();
                break;

            case GameState.Combat:
                await HandleCombatAsync();
                break;

            case GameState.Inventory:
                await HandleInventoryAsync();
                break;

            case GameState.Paused:
                await HandlePausedAsync();
                break;

            case GameState.GameOver:
                await HandleGameOverAsync();
                break;

            default:
                Log.Warning("Unknown game state: {State}", _state);
                _state = GameState.MainMenu;
                break;
        }
    }

    private async Task InitializeLoadingScreenAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Loading Game...", "Please wait while the game initializes");

        ConsoleUI.ShowProgress("Initializing...", task =>
        {
            task.MaxValue = 100;
            for (int i = 0; i <= 100; i += 10)
            {
                task.Value = i;
                Thread.Sleep(100);
            }
        });

        _state = GameState.MainMenu;

        await Task.CompletedTask;
    }

    private async Task HandleMainMenuAsync()
    {
        ConsoleUI.Clear();
        var choice = ConsoleUI.ShowMenu(
            "Main Menu",
            "New Game",
            "Load Game",
            "Settings",
            "Exit"
        );

        switch (choice)
        {
            case "New Game":
                _state = GameState.CharacterCreation;
                break;

            case "Load Game":
                // TODO: Implement load game
                ConsoleUI.ShowInfo("Load game not yet implemented");
                await Task.Delay(1000);
                break;

            case "Settings":
                // TODO: Implement settings
                ConsoleUI.ShowInfo("Settings not yet implemented");
                await Task.Delay(1000);
                break;

            case "Exit":
                _isRunning = false;
                break;
        }
    }

    private async Task HandleCharacterCreationAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Character Creation", "Create your hero");

        var playerName = ConsoleUI.AskForInput("What is your name, brave adventurer?");

        _player = new Character
        {
            Name = playerName,
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            Gold = 100,
            Experience = 0
        };

        ConsoleUI.ShowSuccess($"Welcome, {_player.Name}!");

        // Publish character created event
        await _mediator.Publish(new CharacterCreated(_player.Name));

        _state = GameState.InGame;
        await Task.Delay(1000);
    }

    private async Task HandleInGameAsync()
    {
        if (_player == null)
        {
            _state = GameState.MainMenu;
            return;
        }

        Console.WriteLine();
        var action = ConsoleUI.ShowMenu(
            $"[{_player.Name}] - Level {_player.Level} | HP: {_player.Health}/{_player.MaxHealth} | Gold: {_player.Gold}",
            "Explore",
            "View Character",
            "Inventory",
            "Rest",
            "Save Game",
            "Main Menu"
        );

        switch (action)
        {
            case "Explore":
                await ExploreAsync();
                break;

            case "View Character":
                await ViewCharacterAsync();
                break;

            case "Inventory":
                _state = GameState.Inventory;
                break;

            case "Rest":
                await RestAsync();
                break;

            case "Save Game":
                await SaveGameAsync();
                break;

            case "Main Menu":
                if (ConsoleUI.Confirm("Return to main menu? (unsaved progress will be lost)"))
                {
                    _state = GameState.MainMenu;
                    _player = null;
                }
                break;
        }
    }

    private async Task HandleCombatAsync()
    {
        // TODO: Implement combat system
        ConsoleUI.ShowWarning("Combat system coming soon!");
        await Task.Delay(1000);
        _state = GameState.InGame;
    }

    private async Task HandleInventoryAsync()
    {
        // TODO: Implement inventory management
        ConsoleUI.ShowInfo("Inventory management coming soon!");
        await Task.Delay(1000);
        _state = GameState.InGame;
    }

    private async Task HandlePausedAsync()
    {
        var choice = ConsoleUI.ShowMenu(
            "Game Paused",
            "Resume",
            "Save Game",
            "Main Menu"
        );

        switch (choice)
        {
            case "Resume":
                _state = GameState.InGame;
                break;

            case "Save Game":
                await SaveGameAsync();
                break;

            case "Main Menu":
                if (ConsoleUI.Confirm("Return to main menu? (unsaved progress will be lost)"))
                {
                    _state = GameState.MainMenu;
                    _player = null;
                }
                break;
        }
    }

    private async Task HandleGameOverAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("GAME OVER", $"{_player?.Name ?? "Hero"} has fallen...");

        ConsoleUI.PressAnyKey("Press any key to return to main menu");

        _state = GameState.MainMenu;
        _player = null;

        await Task.CompletedTask;
    }

    private async Task ExploreAsync()
    {
        if (_player == null) return;

        ConsoleUI.ShowInfo("You venture forth into the unknown...");

        // Simulate exploration
        ConsoleUI.ShowProgress("Exploring...", task =>
        {
            task.MaxValue = 100;
            for (int i = 0; i <= 100; i += 10)
            {
                task.Value = i;
                Thread.Sleep(100);
            }
        });

        // Gain XP
        var xpGained = Random.Shared.Next(10, 50);
        _player.GainExperience(xpGained);

        // Check if leveled up
        var newLevel = _player.Level;
        if (newLevel > _player.Level - 1)
        {
            await _mediator.Publish(new PlayerLeveledUp(_player.Name, newLevel));
        }

        ConsoleUI.ShowSuccess($"Gained {xpGained} XP!");

        // Find gold
        var goldFound = Random.Shared.Next(5, 25);
        _player.Gold += goldFound;
        await _mediator.Publish(new GoldGained(_player.Name, goldFound));

        await Task.Delay(1000);
    }

    private async Task ViewCharacterAsync()
    {
        if (_player == null) return;

        var statsContent = $"""
        [yellow]Name:[/] {_player.Name}
        [red]Health:[/] {_player.Health}/{_player.MaxHealth}
        [blue]Mana:[/] {_player.Mana}/{_player.MaxMana}
        [green]Level:[/] {_player.Level}
        [cyan]Experience:[/] {_player.Experience}/{_player.Level * 100}
        [yellow]Gold:[/] {_player.Gold}
        """;

        ConsoleUI.ShowPanel("Character Stats", statsContent, "green");
        ConsoleUI.PressAnyKey();

        await Task.CompletedTask;
    }

    private async Task RestAsync()
    {
        if (_player == null) return;

        ConsoleUI.ShowInfo("You rest and recover...");

        _player.Health = _player.MaxHealth;
        _player.Mana = _player.MaxMana;

        ConsoleUI.ShowSuccess("Fully rested!");
        await Task.Delay(1000);
    }

    private async Task SaveGameAsync()
    {
        ConsoleUI.ShowInfo("Saving game...");

        // TODO: Implement save with LiteDB
        await Task.Delay(500);

        ConsoleUI.ShowSuccess("Game saved!");
        await Task.Delay(1000);
    }

    private async Task ShutdownGameAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Thanks for Playing!", "See you next time!");
        await Task.Delay(1000);
    }
}

/// <summary>
/// Represents the current state of the game
/// </summary>
public enum GameState
{
    MainMenu,
    CharacterCreation,
    InGame,
    Combat,
    Inventory,
    Paused,
    GameOver
}
