using Game.Core.Models;
using Game.Core.Generators;
using Game.Shared.Services;
using Serilog;
using Polly;
using Polly.Retry;
using Spectre.Console;

namespace Game.Console;

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
                    _services.Console.ShowError($"An error occurred: {ex.Message}");

                    // Ask if player wants to continue
                    if (!_services.Console.Confirm("Continue playing?"))
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
            _services.Console.ShowError($"Fatal error: {ex.Message}");
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
        _services.Console.Clear();
        _services.Console.ShowBanner("Loading Game...", "Please wait while the game initializes");

        _services.Console.ShowProgress("Initializing...", task =>
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
                // TODO: Implement Hall of Fame display in UI layer
                _services.Console.ShowInfo("Hall of Fame - Coming Soon!");
                // _services.HallOfFame.DisplayHallOfFame();
                break;

            case "Settings":
                _services.Console.ShowInfo("Settings not yet implemented");
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

        // Check apocalypse timer
        var saveGame = _services.SaveGame.GetCurrentSave();
        if (saveGame != null && saveGame.ApocalypseMode)
        {
            _services.ApocalypseTimer.CheckTimeWarnings();

            if (_services.ApocalypseTimer.IsExpired())
            {
                await HandleApocalypseGameOverAsync();
                return;
            }
        }

        // Display HUD with timer
        DisplayGameHUD();

        System.Console.WriteLine();

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
                await _services.LevelUpService.ProcessPendingLevelUpsAsync(Player);
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
                if (_services.Console.Confirm("Return to main menu? (unsaved progress will be lost)"))
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
        var enemy = EnemyGenerator.Generate(Player.Level, EnemyDifficulty.Normal);

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

        _services.ApocalypseTimer.Pause();
        try
        {
            await _services.Inventory.HandleInventoryAsync(Player);
            _state = GameState.InGame;
        }
        finally
        {
            _services.ApocalypseTimer.Resume();
        }
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
                if (_services.Console.Confirm("Return to main menu? (unsaved progress will be lost)"))
                {
                    _state = GameState.MainMenu;
                    _currentSaveId = null;
                }
                break;
        }
    }

    private async Task HandleGameOverAsync()
    {
        _services.Console.Clear();
        _services.Console.ShowBanner("GAME OVER", $"{Player?.Name ?? "Hero"} has fallen...");

        _services.Console.PressAnyKey("Press any key to return to main menu");

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
        _services.ApocalypseTimer.Pause();
        try
        {
            await _services.Exploration.TravelToLocation();
        }
        finally
        {
            _services.ApocalypseTimer.Resume();
        }
    }

    private async Task ViewCharacterAsync()
    {
        if (Player == null) return;

        _services.ApocalypseTimer.Pause();
        try
        {
            _services.CharacterView.ViewCharacter(Player);
            await Task.CompletedTask;
        }
        finally
        {
            _services.ApocalypseTimer.Resume();
        }
    }

    private void RestAsync()
    {
        if (Player == null) return;

        _services.ApocalypseTimer.Pause();
        try
        {
            _services.Gameplay.Rest(Player);
        }
        finally
        {
            _services.ApocalypseTimer.Resume();
        }
    }

    private void SaveGameAsync()
    {
        if (Player == null)
        {
            _services.Console.ShowError("No active game to save!");
            return;
        }

        _services.ApocalypseTimer.Pause();
        try
        {
            _services.Gameplay.SaveGame(Player, _inventory, _currentSaveId);
        }
        finally
        {
            _services.ApocalypseTimer.Resume();
        }
    }

    private async Task LoadGameAsync()
    {
        var (selectedSave, loadSuccessful) = await _services.LoadGame.LoadGameAsync();

        if (loadSuccessful && selectedSave != null)
        {
            _currentSaveId = selectedSave.Id;
            _inventory = selectedSave.Character.Inventory;

            // Check if apocalypse timer expired immediately after loading
            if (selectedSave.ApocalypseMode && _services.ApocalypseTimer.IsExpired())
            {
                _state = GameState.InGame; // Set state so HandleApocalypseGameOverAsync works correctly
                await HandleApocalypseGameOverAsync();
                return;
            }

            _state = GameState.InGame;
        }
    }

    private async Task ShutdownGameAsync()
    {
        _services.Console.Clear();
        _services.Console.ShowBanner("Thanks for Playing!", "See you next time!");
        await Task.Delay(300);
    }

    /// <summary>
    /// Display the game HUD with character info and apocalypse timer if active.
    /// </summary>
    private void DisplayGameHUD()
    {
        System.Console.Clear();

        // Top bar with character info and timer
        var leftInfo = $"[cyan]{Player?.Name}[/] | Level {Player?.Level} {Player?.ClassName}";
        var centerInfo = $"[green]❤ {Player?.Health}/{Player?.MaxHealth}[/]  [blue]⚡ {Player?.Mana}/{Player?.MaxMana}[/]  [yellow]💰 {Player?.Gold}g[/]";
        var rightInfo = "";

        // Add timer if in Apocalypse mode
        var saveGame = _services.SaveGame.GetCurrentSave();
        if (saveGame != null && saveGame.ApocalypseMode)
        {
            rightInfo = _services.ApocalypseTimer.GetColoredTimeDisplay();
        }

        // Calculate spacing for centered layout
        var leftLen = _services.Console.StripMarkup(leftInfo).Length;
        var centerLen = _services.Console.StripMarkup(centerInfo).Length;
        var rightLen = _services.Console.StripMarkup(rightInfo).Length;

        var totalWidth = System.Console.WindowWidth;
        var spacing = Math.Max(2, (totalWidth - leftLen - centerLen - rightLen) / 2);

        // Display HUD
        System.Console.WriteLine(new string('═', totalWidth));
        AnsiConsole.MarkupLine($"{leftInfo}{new string(' ', spacing)}{centerInfo}{new string(' ', spacing)}{rightInfo}");
        System.Console.WriteLine(new string('═', totalWidth));
        System.Console.WriteLine();
    }

    /// <summary>
    /// Handle apocalypse game over when timer expires.
    /// </summary>
    private async Task HandleApocalypseGameOverAsync()
    {
        _services.Console.Clear();

        // Dramatic apocalypse sequence
        _services.Console.ShowError("═══════════════════════════════════════");
        await Task.Delay(500);
        _services.Console.ShowError("        TIME HAS RUN OUT...            ");
        await Task.Delay(1000);
        _services.Console.ShowError("═══════════════════════════════════════");
        await Task.Delay(1500);

        System.Console.Clear();
        _services.Console.ShowError("The world trembles...");
        await Task.Delay(1000);

        _services.Console.ShowError("The sky darkens...");
        await Task.Delay(1000);

        _services.Console.ShowError("Reality fractures...");
        await Task.Delay(1000);

        System.Console.Clear();
        _services.Console.ShowError("═════════════════════════════════════════════════");
        _services.Console.ShowError("                                                 ");
        _services.Console.ShowError("         THE APOCALYPSE HAS COME                 ");
        _services.Console.ShowError("                                                 ");
        _services.Console.ShowError("═════════════════════════════════════════════════");
        await Task.Delay(2000);

        System.Console.WriteLine();
        _services.Console.WriteText("The world crumbles into eternal darkness...");
        _services.Console.WriteText("You failed to stop the inevitable.");
        System.Console.WriteLine();

        // Show final statistics
        var saveGame = _services.SaveGame.GetCurrentSave();
        if (saveGame != null)
        {
            var elapsed = _services.ApocalypseTimer.GetElapsedMinutes();
            _services.Console.WriteText($"Time Survived: {elapsed / 60}h {elapsed % 60}m");
            _services.Console.WriteText($"Final Level: {Player?.Level ?? 0}");
            _services.Console.WriteText($"Quests Completed: {saveGame.QuestsCompleted}");
            _services.Console.WriteText($"Enemies Defeated: {saveGame.TotalEnemiesDefeated}");
            System.Console.WriteLine();

            // Check progress
            var mainQuestProgress = CalculateMainQuestProgress(saveGame);
            if (mainQuestProgress >= 0.8) // 80% complete
            {
                _services.Console.ShowWarning("You were so close... Only a few quests remained.");
            }
            else if (mainQuestProgress >= 0.5)
            {
                _services.Console.ShowWarning("You made significant progress, but it wasn't enough.");
            }
            else
            {
                _services.Console.ShowWarning("You barely scratched the surface of what was needed.");
            }
        }

        System.Console.WriteLine();
        _services.Console.ShowError("═════════════════════════════════════════════════");
        _services.Console.ShowError("                  GAME OVER                      ");
        _services.Console.ShowError("═════════════════════════════════════════════════");

        Log.Warning("Apocalypse game over. Player failed to complete main quest in time.");

        await Task.Delay(3000);

        // Ask if they want to try again
        if (_services.Console.Confirm("Try again?"))
        {
            _state = GameState.MainMenu;
        }
        else
        {
            _isRunning = false;
        }
    }

    /// <summary>
    /// Calculate main quest progress for apocalypse game over.
    /// </summary>
    private double CalculateMainQuestProgress(SaveGame saveGame)
    {
        // Simple calculation - can be enhanced in Phase 4 with actual main quest tracking
        var totalQuests = Math.Max(1, saveGame.QuestsCompleted + saveGame.ActiveQuests.Count);
        return (double)saveGame.QuestsCompleted / totalQuests;
    }
}

/// <summary>
/// Represents the current state of the game
/// </summary>

