using Game.Models;
using Game.UI;
using Game.Handlers;
using Game.Services;
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
    private readonly IMediator _mediator;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly SaveGameService _saveGameService;
    private readonly CombatService _combatService;
    private readonly MenuService _menuService;
    private readonly ExplorationService _explorationService;
    private readonly CharacterCreationOrchestrator _characterCreation;
    private readonly LoadGameService _loadGameService;
    private readonly GameplayService _gameplayService;
    private readonly CombatOrchestrator _combatOrchestrator;
    private readonly InventoryOrchestrator _inventoryOrchestrator;
    private GameState _state;
    private bool _isRunning;
    private List<Item> _inventory;
    private string? _currentSaveId;
    private CombatLog? _combatLog;
    
    /// <summary>
    /// Get the current player character from the active save game.
    /// Returns null if no save game is active.
    /// </summary>
    private Character? Player => _saveGameService.GetCurrentSave()?.Character;

    public GameEngine(
        IMediator mediator,
        SaveGameService saveGameService,
        CombatService combatService,
        MenuService menuService,
        ExplorationService explorationService,
        CharacterCreationOrchestrator characterCreation,
        LoadGameService loadGameService,
        GameplayService gameplayService,
        CombatOrchestrator combatOrchestrator,
        InventoryOrchestrator inventoryOrchestrator)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
        _combatService = combatService;
        _menuService = menuService;
        _explorationService = explorationService;
        _characterCreation = characterCreation;
        _loadGameService = loadGameService;
        _gameplayService = gameplayService;
        _combatOrchestrator = combatOrchestrator;
        _inventoryOrchestrator = inventoryOrchestrator;
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
        var choice = _menuService.HandleMainMenu();
        
        switch (choice)
        {
            case "New Game":
                _state = GameState.CharacterCreation;
                break;
                
            case "Load Game":
                await LoadGameAsync();
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
        var (character, saveId, success) = await _characterCreation.CreateCharacterAsync();
        
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
        
        var action = _menuService.ShowInGameMenu();

        switch (action)
        {
            case "Explore":
                await ExploreAsync();
                break;

            case "🗺️  Travel":
                TravelToLocation();
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
        _combatService.InitializeCombat(enemy);
        
        // Initialize combat log
        _combatLog = new CombatLog(maxEntries: 15);
        
        // Delegate to CombatOrchestrator
        await _combatOrchestrator.HandleCombatAsync(Player, enemy, _combatLog);
        
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

        await _inventoryOrchestrator.HandleInventoryAsync(Player);
        _state = GameState.InGame;
    }

    private void HandlePaused()
    {
        var nextState = _menuService.HandlePauseMenu();
        
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

        var shouldEnterCombat = await _explorationService.ExploreAsync();
        
        if (shouldEnterCombat)
        {
            _state = GameState.Combat;
        }
    }

    /// <summary>
    /// Allow player to travel to a different location
    /// </summary>
    private void TravelToLocation()
    {
        _explorationService.TravelToLocation();
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
        _gameplayService.Rest(Player);
    }

    private void SaveGameAsync()
    {
        if (Player == null)
        {
            ConsoleUI.ShowError("No active game to save!");
            return;
        }
        
        _gameplayService.SaveGame(Player, _inventory, _currentSaveId);
    }

    private async Task LoadGameAsync()
    {
        var (selectedSave, loadSuccessful) = await _loadGameService.LoadGameAsync();
        
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

