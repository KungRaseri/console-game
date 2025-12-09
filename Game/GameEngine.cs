using Game.Models;
using Game.Shared.UI;
using Game.Shared.Events;
using Game.Services;
using Game.Features.Combat;
using Game.Features.Inventory;
using Game.Features.CharacterCreation;
using Game.Features.SaveLoad;
using Game.Features.Exploration;
using Game.Shared.Services;
using Game.Utilities;
using MediatR;
using Serilog;
using Polly;
using Polly.Retry;
using Spectre.Console;

namespace Game;

/// <summary>
/// Core game engine that manages the game loop and state
/// </summary>
public class GameEngine
{
    private readonly GameEngineServices _services;
    private readonly ResiliencePipeline _resiliencePipeline;
    private GameState _state;
    private bool _isRunning;
    private List<Item> _inventory;
    private string? _currentSaveId;
    private CombatLog? _combatLog;
    
    /// <summary>
    /// Get the current player character from the active save game.
    /// Returns null if no save game is active.
    /// </summary>
    private Character? Player => _services.SaveGame.GetCurrentSave()?.Character;

    public GameEngine(GameEngineServices services)
    {
        _services = services;
        _state = GameState.MainMenu;
        _isRunning = false;
        _inventory = new List<Item>();

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

                    // Small delay to prevent CPU spinning
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
                HandlePaused();
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
        var choice = _services.Menu.HandleMainMenu();
        
        switch (choice)
        {
            case "New Game":
                _state = GameState.CharacterCreation;
                break;
                
            case "Load Game":
                await LoadGameAsync();
                break;
                
            case "🏆 Hall of Fame":
                _services.HallOfFame.DisplayHallOfFame();
                break;
                
            case "Settings":
                ConsoleUI.ShowInfo("Settings not yet implemented");
                break;
                
            case "Exit":
                _isRunning = false;
                break;
        }
    }

    private async Task HandleCharacterCreationAsync()
    {
        var (character, saveId, success) = await _services.CharacterCreation.CreateCharacterAsync();
        
        if (success && character != null && saveId != null)
        {
            _currentSaveId = saveId;
            _state = GameState.InGame;
        }
        else
        {
            _state = GameState.MainMenu;
        }
    }
    
    /// <summary>
    /// Let the player select their character class.
    /// </summary>
    
    private async Task HandleInGameAsync()
    {
        if (Player == null)
        {
            _state = GameState.MainMenu;
            return;
        }

        Console.WriteLine();
        
        var action = _services.Menu.ShowInGameMenu();

        switch (action)
        {
            case "Explore":
                await ExploreAsync();
                break;

            case "🗺️  Travel":
                await TravelToLocation();
                break;

            case "⚔️  Combat":
                _state = GameState.Combat;
                break;

            case "View Character":
                await ViewCharacterAsync();
                break;
                
            case var s when s.Contains("Level Up"):
                await Services.LevelUpService.ProcessPendingLevelUpsAsync(Player);
                break;

            case "Inventory":
                _state = GameState.Inventory;
                break;

            case "Rest":
                RestAsync();
                break;

            case "Save Game":
                SaveGameAsync();
                break;

            case "Main Menu":
                if (ConsoleUI.Confirm("Return to main menu? (unsaved progress will be lost)"))
                {
                    _state = GameState.MainMenu;
                    _currentSaveId = null; // Clear current save
                }
                break;
        }
    }

    private async Task HandleCombatAsync()
    {
        if (Player == null)
        {
            _state = GameState.InGame;
            return;
        }

        // Generate enemy based on player level
        var enemy = Generators.EnemyGenerator.Generate(Player.Level, EnemyDifficulty.Normal);
        
        // Initialize combat with difficulty scaling
        _services.CombatLogic.InitializeCombat(enemy);
        
        // Initialize combat log
        _combatLog = new CombatLog(maxEntries: 15);
        
        // Delegate to CombatOrchestrator
        await _services.Combat.HandleCombatAsync(Player, enemy, _combatLog);
        
        // Clear combat log and return to game
        _combatLog = null;
        _state = GameState.InGame;
    }
    

    private async Task HandleInventoryAsync()
    {
        if (Player == null)
        {
            _state = GameState.InGame;
            return;
        }

        await _services.Inventory.HandleInventoryAsync(Player);
        _state = GameState.InGame;
    }

    private void HandlePaused()
    {
        var nextState = _services.Menu.HandlePauseMenu();
        
        switch (nextState)
        {
            case GameState.InGame:
                _state = GameState.InGame;
                break;
                
            case GameState.Paused:
                // Save was requested
                SaveGameAsync();
                break;
                
            case GameState.MainMenu:
                if (ConsoleUI.Confirm("Return to main menu? (unsaved progress will be lost)"))
                {
                    _state = GameState.MainMenu;
                    _currentSaveId = null;
                }
                break;
        }
    }

    private async Task HandleGameOverAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("GAME OVER", $"{Player?.Name ?? "Hero"} has fallen...");

        ConsoleUI.PressAnyKey("Press any key to return to main menu");

        _state = GameState.MainMenu;
        _currentSaveId = null; // Clear current save

        await Task.CompletedTask;
    }

    private async Task ExploreAsync()
    {
        if (Player == null) return;

        var shouldEnterCombat = await _services.Exploration.ExploreAsync();
        
        if (shouldEnterCombat)
        {
            _state = GameState.Combat;
        }
    }

    /// <summary>
    /// Allow player to travel to a different location
    /// </summary>
    private async Task TravelToLocation()
    {
        await _services.Exploration.TravelToLocation();
    }

    private async Task ViewCharacterAsync()
    {
        if (Player == null) return;

        CharacterViewService.ViewCharacter(Player);
        await Task.CompletedTask;
    }

    private void RestAsync()
    {
        if (Player == null) return;
        _services.Gameplay.Rest(Player);
    }

    private void SaveGameAsync()
    {
        if (Player == null)
        {
            ConsoleUI.ShowError("No active game to save!");
            return;
        }
        
        _services.Gameplay.SaveGame(Player, _inventory, _currentSaveId);
    }

    private async Task LoadGameAsync()
    {
        var (selectedSave, loadSuccessful) = await _services.LoadGame.LoadGameAsync();
        
        if (loadSuccessful && selectedSave != null)
        {
            _currentSaveId = selectedSave.Id;
            _inventory = selectedSave.Character.Inventory;
            _state = GameState.InGame;
        }
    }

    private async Task ShutdownGameAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Thanks for Playing!", "See you next time!");
        await Task.Delay(300);
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

