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
    private readonly GameStateService _gameState;
    private readonly MenuService _menuService;
    private readonly ExplorationService _explorationService;
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
        GameStateService gameState,
        CombatService combatService,
        MenuService menuService,
        ExplorationService explorationService)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
        _gameState = gameState;
        _combatService = combatService;
        _menuService = menuService;
        _explorationService = explorationService;
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
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Character Creation", "Forge your legend");

        // Step 1: Enter name
        var playerName = ConsoleUI.AskForInput("What is your name, brave adventurer?");
        
        // Step 2: Choose class
        var selectedClass = await SelectCharacterClassAsync();
        if (selectedClass == null)
        {
            _state = GameState.MainMenu;
            return;
        }
        
        // Step 3: Allocate attributes
        var allocation = await AllocateAttributesAsync(selectedClass);
        if (allocation == null)
        {
            _state = GameState.MainMenu;
            return;
        }
        
        // Step 4: Create character
        var newCharacter = Services.CharacterCreationService.CreateCharacter(playerName, selectedClass.Name, allocation);
        
        // Step 5: Review character
        ReviewCharacter(newCharacter, selectedClass);

        ConsoleUI.ShowSuccess($"Welcome, {newCharacter.Name} the {newCharacter.ClassName}!");
        await Task.Delay(500);

        // Publish character created event
        await _mediator.Publish(new CharacterCreated(newCharacter.Name));

        // Create save game with the new character
        var saveGame = _saveGameService.CreateNewGame(newCharacter);
        _currentSaveId = saveGame.Id;

        _state = GameState.InGame;
    }
    
    /// <summary>
    /// Let the player select their character class.
    /// </summary>
    private async Task<CharacterClass?> SelectCharacterClassAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Choose Your Class", "Each class offers unique strengths");
        
        var classes = Data.CharacterClassRepository.GetAllClasses();
        
        // Display all classes with details
        foreach (var cls in classes)
        {
            ConsoleUI.WriteText("");
            ConsoleUI.ShowPanel(
                $"{cls.Name} ({string.Join(", ", cls.PrimaryAttributes)})",
                $"{cls.Description}\n\n{cls.FlavorText}\n\n" +
                $"[cyan]Attribute Bonuses:[/]\n" +
                (cls.BonusStrength > 0 ? $"  +{cls.BonusStrength} Strength\n" : "") +
                (cls.BonusDexterity > 0 ? $"  +{cls.BonusDexterity} Dexterity\n" : "") +
                (cls.BonusConstitution > 0 ? $"  +{cls.BonusConstitution} Constitution\n" : "") +
                (cls.BonusIntelligence > 0 ? $"  +{cls.BonusIntelligence} Intelligence\n" : "") +
                (cls.BonusWisdom > 0 ? $"  +{cls.BonusWisdom} Wisdom\n" : "") +
                (cls.BonusCharisma > 0 ? $"  +{cls.BonusCharisma} Charisma\n" : "") +
                $"\n[yellow]Health Bonus:[/] {(cls.StartingHealthBonus >= 0 ? "+" : "")}{cls.StartingHealthBonus}\n" +
                $"[blue]Mana Bonus:[/] {(cls.StartingManaBonus >= 0 ? "+" : "")}{cls.StartingManaBonus}",
                "yellow"
            );
        }
        
        var classNames = classes.Select(c => c.Name).Append("Back to Menu").ToArray();
        var choice = ConsoleUI.ShowMenu("Select your class:", classNames);
        
        if (choice == "Back to Menu")
        {
            return null;
        }
        
        var selected = classes.FirstOrDefault(c => c.Name == choice);
        
        if (selected != null)
        {
            ConsoleUI.ShowSuccess($"You have chosen the path of the {selected.Name}!");
            await Task.Delay(300);
        }
        
        return selected;
    }
    
    /// <summary>
    /// Let the player allocate attribute points.
    /// </summary>
    private async Task<AttributeAllocation?> AllocateAttributesAsync(CharacterClass selectedClass)
    {
        var allocation = new AttributeAllocation();
        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.ShowBanner("Attribute Allocation", $"Points Remaining: {allocation.GetRemainingPoints()}/27");
            
            // Show current allocation
            var allocationLines = new List<string>();
            allocationLines.Add("[yellow]Current Attributes:[/]");
            allocationLines.Add("");
            
            foreach (var attr in attributes)
            {
                var current = allocation.GetAttributeValue(attr);
                var withBonus = current + GetClassBonus(selectedClass, attr);
                var isPrimary = selectedClass.PrimaryAttributes.Contains(attr);
                var primaryMark = isPrimary ? " [cyan]★[/]" : "";
                var color = isPrimary ? "cyan" : "white";
                
                allocationLines.Add($"  [{color}]{attr}:{primaryMark,-20}[/] {current,2} " +
                    $"(+{GetClassBonus(selectedClass, attr)} class) = [green]{withBonus}[/]");
            }
            
            allocationLines.Add("");
            allocationLines.Add("[grey]★ = Primary attribute for your class[/]");
            allocationLines.Add("");
            allocationLines.Add($"[yellow]Points Spent:[/] {allocation.GetPointsSpent()}/27");
            allocationLines.Add($"[cyan]Points Remaining:[/] {allocation.GetRemainingPoints()}");
            
            ConsoleUI.ShowPanel("Your Attributes", string.Join("\n", allocationLines), "green");
            
            // Menu options
            var options = new List<string>();
            foreach (var attr in attributes)
            {
                if (allocation.CanIncrease(attr))
                    options.Add($"Increase {attr}");
                if (allocation.CanDecrease(attr))
                    options.Add($"Decrease {attr}");
            }
            
            options.Add("Auto-Allocate (Recommended)");
            options.Add("Reset All");
            options.Add("Confirm & Continue");
            options.Add("Back to Class Selection");
            
            var choice = ConsoleUI.ShowMenu("Adjust attributes:", options.ToArray());
            
            if (choice.StartsWith("Increase "))
            {
                var attr = choice.Replace("Increase ", "");
                var current = allocation.GetAttributeValue(attr);
                allocation.SetAttributeValue(attr, current + 1);
            }
            else if (choice.StartsWith("Decrease "))
            {
                var attr = choice.Replace("Decrease ", "");
                var current = allocation.GetAttributeValue(attr);
                allocation.SetAttributeValue(attr, current - 1);
            }
            else if (choice == "Auto-Allocate (Recommended)")
            {
                // Auto-allocate based on class
                allocation = AutoAllocateAttributes(selectedClass);
                ConsoleUI.ShowSuccess("Attributes auto-allocated based on your class!");
                await Task.Delay(300);
            }
            else if (choice == "Reset All")
            {
                allocation = new AttributeAllocation();
                ConsoleUI.ShowInfo("Attributes reset to base values.");
                await Task.Delay(200);
            }
            else if (choice == "Confirm & Continue")
            {
                if (allocation.GetRemainingPoints() > 0)
                {
                    if (ConsoleUI.Confirm($"You have {allocation.GetRemainingPoints()} unspent points. Continue anyway?"))
                    {
                        return allocation;
                    }
                }
                else
                {
                    return allocation;
                }
            }
            else if (choice == "Back to Class Selection")
            {
                return null;
            }
        }
    }
    
    /// <summary>
    /// Auto-allocate attributes based on class recommendations.
    /// </summary>
    private AttributeAllocation AutoAllocateAttributes(CharacterClass characterClass)
    {
        var allocation = new AttributeAllocation();
        
        // Prioritize primary attributes
        foreach (var primary in characterClass.PrimaryAttributes)
        {
            allocation.SetAttributeValue(primary, 14); // High in primary
        }
        
        // Distribute remaining points to secondary stats
        var remaining = allocation.GetRemainingPoints();
        var secondaryAttrs = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" }
            .Where(a => !characterClass.PrimaryAttributes.Contains(a))
            .ToArray();
        
        // Bring all secondaries to 10 if possible
        foreach (var attr in secondaryAttrs)
        {
            if (remaining <= 0) break;
            var current = allocation.GetAttributeValue(attr);
            if (current < 10 && remaining >= 2)
            {
                allocation.SetAttributeValue(attr, 10);
                remaining = allocation.GetRemainingPoints();
            }
        }
        
        // Spend any leftover points on primary stats
        while (remaining > 0)
        {
            foreach (var primary in characterClass.PrimaryAttributes)
            {
                if (remaining <= 0) break;
                if (allocation.CanIncrease(primary))
                {
                    var current = allocation.GetAttributeValue(primary);
                    allocation.SetAttributeValue(primary, current + 1);
                    remaining = allocation.GetRemainingPoints();
                }
            }
            
            // Safety: if we can't allocate more, break
            if (!characterClass.PrimaryAttributes.Any(p => allocation.CanIncrease(p)))
                break;
        }
        
        return allocation;
    }
    
    /// <summary>
    /// Get class bonus for a specific attribute.
    /// </summary>
    private int GetClassBonus(CharacterClass characterClass, string attribute)
    {
        return attribute switch
        {
            "Strength" => characterClass.BonusStrength,
            "Dexterity" => characterClass.BonusDexterity,
            "Constitution" => characterClass.BonusConstitution,
            "Intelligence" => characterClass.BonusIntelligence,
            "Wisdom" => characterClass.BonusWisdom,
            "Charisma" => characterClass.BonusCharisma,
            _ => 0
        };
    }
    
    /// <summary>
    /// Review the final character before starting.
    /// </summary>
    private void ReviewCharacter(Character character, CharacterClass characterClass)
    {
        CharacterViewService.ReviewCharacter(character, characterClass);
    }

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
        _combatLog.AddEntry($"⚔️ Combat begins! A {enemy.Name} appears!", CombatLogType.Info);
        
        await _mediator.Publish(new CombatStarted(Player.Name, enemy.Name));
        
        bool playerDefending = false;
        
        // Combat loop
        while (Player.IsAlive() && enemy.IsAlive())
        {
            // Display combat status with log
            DisplayCombatStatusWithLog(Player, enemy);
            
            // Player turn
            var action = _menuService.ShowCombatMenu();
            
            playerDefending = false;
            
            switch (action)
            {
                case "⚔️  Attack":
                    await ExecutePlayerTurnAsync(Player, enemy, CombatActionType.Attack);
                    break;
                    
                case "🛡️  Defend":
                    playerDefending = true;
                    _combatLog?.AddEntry("You raise your guard!", CombatLogType.Defend);
                    ConsoleUI.ShowInfo("You raise your guard, ready to defend!");
                    await Task.Delay(300);
                    break;
                    
                case "🧪 Use Item":
                    var itemUsed = await UseItemInCombatMenuAsync(Player);
                    if (!itemUsed)
                    {
                        continue; // Don't advance turn if no item was used
                    }
                    break;
                    
                case "🏃 Flee":
                    var fleeResult = _combatService.AttemptFlee(Player, enemy);
                    if (fleeResult.Success)
                    {
                        _combatLog?.AddEntry("Successfully fled!", CombatLogType.Info);
                        ConsoleUI.ShowSuccess(fleeResult.Message);
                        await Task.Delay(500);
                        _state = GameState.InGame;
                        _combatLog = null;
                        return;
                    }
                    else
                    {
                        _combatLog?.AddEntry("Failed to escape!", CombatLogType.Info);
                        ConsoleUI.ShowError(fleeResult.Message);
                        await Task.Delay(500);
                    }
                    break;
            }
            
            // Check if enemy is defeated
            if (!enemy.IsAlive())
            {
                break;
            }
            
            // Enemy turn
            await Task.Delay(200);
            await ExecuteEnemyTurnAsync(Player, enemy, playerDefending);
            
            // Check if player is defeated
            if (!Player.IsAlive())
            {
                break;
            }
            
            // Apply regeneration at end of turn
            var regenAmount = SkillEffectCalculator.ApplyRegeneration(Player);
            if (regenAmount > 0)
            {
                _combatLog?.AddEntry($"💚 Regeneration healed {regenAmount} HP", CombatLogType.Heal);
                ConsoleUI.WriteColoredText($"[green]💚 Regeneration healed {regenAmount} HP[/]");
                await Task.Delay(300);
            }
            
            Console.WriteLine();
            await Task.Delay(600);
        }
        
        // Combat ended
        if (Player.IsAlive())
        {
            _combatLog?.AddEntry($"🎉 Victory! {enemy.Name} defeated!", CombatLogType.Victory);
            await HandleCombatVictoryAsync(Player, enemy);
        }
        else
        {
            _combatLog?.AddEntry("💀 You have been defeated...", CombatLogType.Defeat);
            await HandleCombatDefeatAsync(Player, enemy);
        }
        
        // Clear combat log
        _combatLog = null;
        _state = GameState.InGame;
    }
    
    private void DisplayCombatStatusWithLog(Character player, Enemy enemy)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("⚔️ COMBAT ⚔️", $"Fighting: {enemy.Name}");
        
        // Create main combat content
        var playerHealthPercent = (double)player.Health / player.MaxHealth * 100;
        var playerHealthColor = playerHealthPercent > 50 ? "green" : playerHealthPercent > 25 ? "yellow" : "red";
        var playerHealthBar = GenerateHealthBar(player.Health, player.MaxHealth, 20);
        
        var enemyHealthPercent = (double)enemy.Health / enemy.MaxHealth * 100;
        var enemyHealthColor = enemyHealthPercent > 50 ? "green" : enemyHealthPercent > 25 ? "yellow" : "red";
        var enemyHealthBar = GenerateHealthBar(enemy.Health, enemy.MaxHealth, 20);
        
        var difficultyColor = enemy.Difficulty switch
        {
            EnemyDifficulty.Easy => "green",
            EnemyDifficulty.Normal => "white",
            EnemyDifficulty.Hard => "yellow",
            EnemyDifficulty.Elite => "orange1",
            EnemyDifficulty.Boss => "red",
            _ => "white"
        };
        
        // Build main combat panel with fixed height to match log
        var combatInfo = new Panel(new Markup(
            $"[bold cyan]{player.Name}[/] - Level {player.Level}\n" +
            $"[{playerHealthColor}]HP: {player.Health}/{player.MaxHealth}[/] {playerHealthBar}\n" +
            $"[blue]MP: {player.Mana}/{player.MaxMana}[/]\n" +
            $"[dim]ATK: {player.GetPhysicalDamageBonus()} | DEF: {player.GetPhysicalDefense()}[/]\n" +
            $"\n\n\n" +
            $"[bold {difficultyColor}]VS[/]\n" +
            $"\n\n\n" +
            $"[bold {difficultyColor}]{enemy.Name}[/] - Level {enemy.Level} [dim]({enemy.Difficulty})[/]\n" +
            $"[{enemyHealthColor}]HP: {enemy.Health}/{enemy.MaxHealth}[/] {enemyHealthBar}\n" +
            $"[dim]ATK: {enemy.GetPhysicalDamageBonus()} | DEF: {enemy.GetPhysicalDefense()}[/]"
        ))
        {
            Header = new PanelHeader("[bold yellow]Battle Status[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan),
            Height = 17 // Match log height: 15 lines + 2 for borders
        };
        
        // Display with combat log
        var logEntries = _combatLog?.GetFormattedEntries() ?? new List<string>();
        ConsoleUI.ShowCombatLayout(combatInfo, logEntries);
        Console.WriteLine();
    }
    
    private static string GenerateHealthBar(int current, int max, int width)
    {
        var percent = (double)current / max;
        var filled = (int)(percent * width);
        var empty = width - filled;
        
        var color = percent > 0.5 ? "green" : percent > 0.25 ? "yellow" : "red";
        
        return $"[{color}]{'█'.ToString().PadRight(filled, '█')}[/][dim]{'░'.ToString().PadRight(empty, '░')}[/]";
    }
    
    private async Task ExecutePlayerTurnAsync(Character player, Enemy enemy, CombatActionType actionType)
    {
        var result = _combatService.ExecutePlayerAttack(player, enemy);
        
        await _mediator.Publish(new AttackPerformed(player.Name, enemy.Name, result.Damage));
        
        if (result.IsDodged)
        {
            _combatLog?.AddEntry($"💨 {result.Message}", CombatLogType.Dodge);
            ConsoleUI.WriteColoredText($"[yellow]💨 {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            _combatLog?.AddEntry($"💥 CRIT! {result.Damage} damage!", CombatLogType.Critical);
            ConsoleUI.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            _combatLog?.AddEntry($"⚔️ Hit for {result.Damage} damage", CombatLogType.PlayerAttack);
            ConsoleUI.WriteColoredText($"[green]⚔️  {result.Message}[/]");
        }
        
        if (!enemy.IsAlive())
        {
            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name));
        }
        
        await Task.Delay(300);
    }
    
    private async Task ExecuteEnemyTurnAsync(Character player, Enemy enemy, bool playerDefending)
    {
        var result = _combatService.ExecuteEnemyAttack(enemy, player, playerDefending);
        
        await _mediator.Publish(new DamageTaken(player.Name, result.Damage));
        
        if (result.IsDodged)
        {
            _combatLog?.AddEntry($"💨 Dodged {enemy.Name}'s attack!", CombatLogType.Dodge);
            ConsoleUI.WriteColoredText($"[cyan]💨 {result.Message}[/]");
        }
        else if (result.IsBlocked)
        {
            _combatLog?.AddEntry($"🛡️ Blocked {result.Damage} damage", CombatLogType.Defend);
            ConsoleUI.WriteColoredText($"[blue]🛡️  {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            _combatLog?.AddEntry($"💥 {enemy.Name} CRIT! {result.Damage} damage!", CombatLogType.EnemyAttack);
            ConsoleUI.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            _combatLog?.AddEntry($"🗡️ {enemy.Name} hit for {result.Damage}", CombatLogType.EnemyAttack);
            ConsoleUI.WriteColoredText($"[orange1]🗡️  {result.Message}[/]");
        }
        
        if (!player.IsAlive())
        {
            await _mediator.Publish(new PlayerDefeated(player.Name, enemy.Name));
        }
        
        await Task.Delay(300);
    }
    
    private async Task<bool> UseItemInCombatMenuAsync(Character player)
    {
        var consumables = player.Inventory.Where(i => i.Type == ItemType.Consumable).ToList();
        
        if (!consumables.Any())
        {
            ConsoleUI.ShowWarning("You have no consumable items!");
            await Task.Delay(300);
            return false;
        }
        
        var itemNames = consumables.Select(i => $"{i.Name} ({i.Rarity})").ToList();
        itemNames.Add("[dim]Cancel[/]");
        
        var selection = ConsoleUI.ShowMenu("Select an item to use:", itemNames.ToArray());
        
        if (selection == "[dim]Cancel[/]")
        {
            return false;
        }
        
        var selectedIndex = itemNames.IndexOf(selection);
        var item = consumables[selectedIndex];
        
        var result = _combatService.UseItemInCombat(player, item);
        
        if (result.Success)
        {
            _combatLog?.AddEntry($"✨ Used {item.Name}", CombatLogType.ItemUse);
            
            if (result.Healing > 0)
            {
                _combatLog?.AddEntry($"💚 Restored {result.Healing} HP", CombatLogType.Heal);
                ConsoleUI.WriteColoredText($"[green]✨ {result.Message}[/]");
            }
            else
            {
                ConsoleUI.WriteColoredText($"[cyan]✨ {result.Message}[/]");
            }
            
            await Task.Delay(500);
            return true;
        }
        else
        {
            ConsoleUI.ShowError(result.Message);
            await Task.Delay(300);
            return false;
        }
    }
    
    private async Task HandleCombatVictoryAsync(Character player, Enemy enemy)
    {
        ConsoleUI.Clear();
        
        var outcome = _combatService.GenerateVictoryOutcome(player, enemy);
        
        ConsoleUI.ShowBanner("🏆 VICTORY! 🏆", $"You defeated {enemy.Name}!");
        
        // Award XP
        var previousLevel = player.Level;
        player.GainExperience(outcome.XPGained);
        
        // Award gold
        player.Gold += outcome.GoldGained;
        
        // Display rewards
        ConsoleUI.ShowPanel(
            "Battle Rewards",
            $"[green]+{outcome.XPGained} XP[/] ({player.Experience}/{player.Level * 100} to next level)\n" +
            $"[yellow]+{outcome.GoldGained} Gold[/] (Total: {player.Gold})",
            "green"
        );
        
        // Check for level up
        if (player.Level > previousLevel)
        {
            Console.WriteLine();
            ConsoleUI.WriteColoredText($"[bold yellow]🌟 LEVEL UP! You are now level {player.Level}! 🌟[/]");
            await _mediator.Publish(new PlayerLeveledUp(player.Name, player.Level));
            await Task.Delay(500);
            
            // Process level-up allocation
            ConsoleUI.PressAnyKey("Press any key to allocate your level-up points...");
            await Services.LevelUpService.ProcessPendingLevelUpsAsync(player);
        }
        
        // Display loot
        if (outcome.LootDropped.Any())
        {
            Console.WriteLine();
            ConsoleUI.WriteColoredText("[cyan bold]💎 Loot Dropped![/]");
            
            foreach (var item in outcome.LootDropped)
            {
                player.Inventory.Add(item);
                
                var rarityColor = item.Rarity switch
                {
                    ItemRarity.Common => "white",
                    ItemRarity.Uncommon => "green",
                    ItemRarity.Rare => "blue",
                    ItemRarity.Epic => "purple",
                    ItemRarity.Legendary => "orange1",
                    _ => "white"
                };
                
                ConsoleUI.WriteColoredText($"  [{rarityColor}]• {item.Name} ({item.Rarity})[/]");
            }
        }
        
        await _mediator.Publish(new CombatEnded(player.Name, true));
        
        // Auto-save after combat
        try
        {
            _saveGameService.AutoSave(player, _inventory);
            ConsoleUI.WriteText("[grey]Game auto-saved[/]");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Auto-save after combat failed");
        }
        
        Console.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to continue...");
    }
    
    private async Task HandleCombatDefeatAsync(Character player, Enemy enemy)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("💀 DEFEAT 💀", $"You were defeated by {enemy.Name}...");
        
        ConsoleUI.ShowPanel(
            "Battle Summary",
            $"The {enemy.Name} proved too powerful.\n" +
            $"You have been knocked unconscious and lost some gold.",
            "red"
        );
        
        // Lose some gold
        var goldLost = (int)(player.Gold * 0.1); // Lose 10% of gold
        player.Gold = Math.Max(0, player.Gold - goldLost);
        
        if (goldLost > 0)
        {
            ConsoleUI.WriteColoredText($"[red]-{goldLost} Gold[/]");
        }
        
        // Restore some health
        player.Health = player.MaxHealth / 4; // Restore to 25% health
        
        ConsoleUI.WriteColoredText($"[dim]You wake up with {player.Health} HP remaining.[/]");
        
        await _mediator.Publish(new CombatEnded(player.Name, false));
        
        Console.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to continue...");
    }

    private async Task HandleInventoryAsync()
    {
        if (Player == null)
        {
            _state = GameState.InGame;
            return;
        }

        bool inInventory = true;

        while (inInventory)
        {
            Console.WriteLine();
            
            // Display inventory summary
            var inventoryCount = Player.Inventory.Count;
            var totalValue = Player.Inventory.Sum(i => i.Price);

            if (inventoryCount == 0)
            {
                ConsoleUI.ShowInfo("Your inventory is empty.");
                ConsoleUI.ShowPanel("Equipment", CharacterViewService.GetEquipmentDisplay(Player), "cyan");
                
                if (!ConsoleUI.Confirm("Return to game?"))
                {
                    continue;
                }
                inInventory = false;
                break;
            }

            // Show inventory stats
            ConsoleUI.ShowBanner($"Inventory ({inventoryCount} items)", $"Total Value: {totalValue} gold");

            // Group items by type for display
            var itemsByType = Player.Inventory
                .GroupBy(i => i.Type)
                .OrderBy(g => g.Key)
                .ToList();

            // Create display table
            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.AddColumn(new TableColumn("[yellow]Type[/]"));
            table.AddColumn(new TableColumn("[yellow]Items[/]"));

            foreach (var group in itemsByType)
            {
                var itemList = string.Join(", ", group.Select(i => 
                    $"{i.Name} ({GetRarityColor(i.Rarity)}{i.Rarity}[/])"));
                table.AddRow($"[cyan]{group.Key}[/]", itemList);
            }

            AnsiConsole.Write(table);
            Console.WriteLine();

            // Inventory actions
            var action = _menuService.ShowInventoryMenu();

            switch (action)
            {
                case "View Item Details":
                    await ViewItemDetailsAsync();
                    break;

                case "Use Item":
                    await UseItemAsync();
                    break;

                case "Equip Item":
                    await EquipItemAsync();
                    break;

                case "Drop Item":
                    await DropItemAsync();
                    break;

                case "Sort Inventory":
                    SortInventory();
                    break;

                case "Back to Game":
                    inInventory = false;
                    break;
            }
        }

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

        ConsoleUI.ShowInfo("You rest and recover...");

        Player.Health = Player.MaxHealth;
        Player.Mana = Player.MaxMana;

        ConsoleUI.ShowSuccess("Fully rested!");
    }

    private void SaveGameAsync()
    {
        if (Player == null)
        {
            ConsoleUI.ShowError("No active game to save!");
            return;
        }

        ConsoleUI.ShowInfo("Saving game...");

        try
        {
            _saveGameService.SaveGame(Player, _inventory, _currentSaveId);
            ConsoleUI.ShowSuccess("Game saved successfully!");
            Log.Information("Game saved for player {PlayerName}", Player.Name);
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError($"Failed to save game: {ex.Message}");
            Log.Error(ex, "Failed to save game");
        }
    }

    private async Task LoadGameAsync()
    {
        try
        {
            var saves = _saveGameService.GetAllSaves();

            if (!saves.Any())
            {
                ConsoleUI.ShowWarning("No saved games found!");
                await Task.Delay(500);
                return;
            }

            ConsoleUI.Clear();
            ConsoleUI.ShowBanner("Load Game", "Select a save to continue your adventure");

            // Display saves in a table
            var headers = new[] { "Player", "Class", "Level", "Last Played", "Play Time" };
            var rows = saves.Select(s =>
            {
                var timeSinceSave = DateTime.Now - s.SaveDate;
                var timeAgo = timeSinceSave.TotalHours < 24
                    ? $"{(int)timeSinceSave.TotalHours}h ago"
                    : $"{(int)timeSinceSave.TotalDays}d ago";
                
                var playTime = s.PlayTimeMinutes < 60
                    ? $"{s.PlayTimeMinutes}m"
                    : $"{s.PlayTimeMinutes / 60}h {s.PlayTimeMinutes % 60}m";

                return new[]
                {
                    s.Character.Name,
                    s.Character.ClassName,
                    s.Character.Level.ToString(),
                    timeAgo,
                    playTime
                };
            }).ToList();

            ConsoleUI.ShowTable("Available Saves", headers, rows);

            // Build menu options
            var menuOptions = saves.Select(s =>
                $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
            ).ToList();
            menuOptions.Add("Delete a Save");
            menuOptions.Add("Back to Menu");

            var choice = ConsoleUI.ShowMenu("Select save:", menuOptions.ToArray());

            if (choice == "Back to Menu")
            {
                return;
            }

            if (choice == "Delete a Save")
            {
                await DeleteSaveAsync(saves);
                return;
            }

            // Find selected save
            var selectedIndex = menuOptions.IndexOf(choice);
            var selectedSave = saves[selectedIndex];

            // Load the save
            ConsoleUI.ShowProgress("Loading game...", task =>
            {
                task.MaxValue = 100;
                for (int i = 0; i <= 100; i += 20)
                {
                    task.Value = i;
                    Thread.Sleep(150);
                }
            });

            // Save loaded - set current save ID
            _currentSaveId = selectedSave.Id;
            _inventory = selectedSave.Character.Inventory; // Inventory is now tracked in Character

            ConsoleUI.ShowSuccess($"Welcome back, {Player!.Name}!");
            Log.Information("Game loaded for player {PlayerName}", Player.Name);
            await Task.Delay(500);

            _state = GameState.InGame;
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowError($"Failed to load game: {ex.Message}");
            Log.Error(ex, "Failed to load game");
            await Task.Delay(500);
        }
    }

    private async Task DeleteSaveAsync(List<SaveGame> saves)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Delete Save", "⚠️ This action cannot be undone!");

        var menuOptions = saves.Select(s =>
            $"{s.Character.Name} - Level {s.Character.Level} {s.Character.ClassName}"
        ).ToList();
        menuOptions.Add("Cancel");

        var choice = ConsoleUI.ShowMenu("Select save to delete:", menuOptions.ToArray());

        if (choice == "Cancel")
        {
            await LoadGameAsync(); // Return to load menu
            return;
        }

        var selectedIndex = menuOptions.IndexOf(choice);
        var selectedSave = saves[selectedIndex];

        if (ConsoleUI.Confirm($"Delete save for {selectedSave.Character.Name}?"))
        {
            try
            {
                _saveGameService.DeleteSave(selectedSave.Id);
                ConsoleUI.ShowSuccess("Save deleted successfully!");
                Log.Information("Save deleted for player {PlayerName}", selectedSave.Character.Name);
                await Task.Delay(300);
                
                // Return to load menu
                await LoadGameAsync();
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError($"Failed to delete save: {ex.Message}");
                Log.Error(ex, "Failed to delete save");
                await Task.Delay(500);
            }
        }
        else
        {
            await LoadGameAsync(); // Return to load menu
        }
    }

    private async Task ShutdownGameAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Thanks for Playing!", "See you next time!");
        await Task.Delay(300);
    }

    // ========== Inventory Helper Methods ==========

    private static string GetItemDisplay(Item? item)
    {
        if (item == null) return "[grey]Empty[/]";
        
        var displayName = item.GetDisplayName();
        return $"{CharacterViewService.GetRarityColor(item.Rarity)}{displayName}[/]";
    }

    private static string GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "[white]",
            ItemRarity.Uncommon => "[green]",
            ItemRarity.Rare => "[blue]",
            ItemRarity.Epic => "[purple]",
            ItemRarity.Legendary => "[orange1]",
            _ => "[grey]"
        };
    }

    private async Task ViewItemDetailsAsync()
    {
        if (Player == null || Player.Inventory.Count == 0) return;

        var item = SelectItemFromInventory("Select an item to view");
        if (item == null) return;

        Console.WriteLine();
        
        var detailLines = new List<string>
        {
            $"[yellow]Name:[/] {item.GetDisplayName()}",
            $"[yellow]Type:[/] {item.Type}",
            $"[yellow]Rarity:[/] {GetRarityColor(item.Rarity)}{item.Rarity}[/]",
            $"[yellow]Value:[/] {item.Price} gold"
        };
        
        // Show upgrade level
        if (item.UpgradeLevel > 0)
        {
            detailLines.Add($"[cyan]⬆ Upgrade Level: +{item.UpgradeLevel}[/] (+{item.UpgradeLevel * 2} to all stats)");
        }
        
        // Show two-handed indicator
        if (item.IsTwoHanded)
        {
            detailLines.Add($"[red]⚔️ Two-Handed Weapon[/]");
        }
        
        // Show set membership
        if (!string.IsNullOrEmpty(item.SetName))
        {
            detailLines.Add($"[cyan]Set: {item.SetName}[/]");
        }
        
        // Show stats if item has any bonuses
        bool hasStats = item.BonusStrength > 0 || item.BonusDexterity > 0 || item.BonusConstitution > 0 
                        || item.BonusIntelligence > 0 || item.BonusWisdom > 0 || item.BonusCharisma > 0;
        
        if (hasStats)
        {
            detailLines.Add("");
            detailLines.Add("[underline]Base Bonuses:[/]");
            if (item.BonusStrength > 0) 
                detailLines.Add($"  [red]+{item.BonusStrength} Strength[/]");
            if (item.BonusDexterity > 0) 
                detailLines.Add($"  [green]+{item.BonusDexterity} Dexterity[/]");
            if (item.BonusConstitution > 0) 
                detailLines.Add($"  [yellow]+{item.BonusConstitution} Constitution[/]");
            if (item.BonusIntelligence > 0) 
                detailLines.Add($"  [purple]+{item.BonusIntelligence} Intelligence[/]");
            if (item.BonusWisdom > 0) 
                detailLines.Add($"  [blue]+{item.BonusWisdom} Wisdom[/]");
            if (item.BonusCharisma > 0) 
                detailLines.Add($"  [cyan]+{item.BonusCharisma} Charisma[/]");
        }
        
        // Show enchantments
        if (item.Enchantments.Any())
        {
            detailLines.Add("");
            detailLines.Add("[underline]Enchantments:[/]");
            foreach (var enchantment in item.Enchantments)
            {
                var enchantColor = enchantment.Rarity switch
                {
                    EnchantmentRarity.Minor => "grey",
                    EnchantmentRarity.Lesser => "green",
                    EnchantmentRarity.Greater => "blue",
                    EnchantmentRarity.Superior => "purple",
                    EnchantmentRarity.Legendary => "orange1",
                    _ => "white"
                };
                
                detailLines.Add($"  [{enchantColor}]{enchantment.Name}[/]");
                if (enchantment.BonusStrength > 0) 
                    detailLines.Add($"    [red]+{enchantment.BonusStrength} Strength[/]");
                if (enchantment.BonusDexterity > 0) 
                    detailLines.Add($"    [green]+{enchantment.BonusDexterity} Dexterity[/]");
                if (enchantment.BonusConstitution > 0) 
                    detailLines.Add($"    [yellow]+{enchantment.BonusConstitution} Constitution[/]");
                if (enchantment.BonusIntelligence > 0) 
                    detailLines.Add($"    [purple]+{enchantment.BonusIntelligence} Intelligence[/]");
                if (enchantment.BonusWisdom > 0) 
                    detailLines.Add($"    [blue]+{enchantment.BonusWisdom} Wisdom[/]");
                if (enchantment.BonusCharisma > 0) 
                    detailLines.Add($"    [cyan]+{enchantment.BonusCharisma} Charisma[/]");
                
                if (!string.IsNullOrEmpty(enchantment.SpecialEffect))
                {
                    detailLines.Add($"    [cyan]{enchantment.SpecialEffect}[/]");
                }
            }
        }
        
        // Show total bonuses if enchanted or upgraded
        if (item.Enchantments.Any() || item.UpgradeLevel > 0)
        {
            detailLines.Add("");
            detailLines.Add("[underline]Total Bonuses:[/]");
            var totalStr = item.GetTotalBonusStrength();
            var totalDex = item.GetTotalBonusDexterity();
            var totalCon = item.GetTotalBonusConstitution();
            var totalInt = item.GetTotalBonusIntelligence();
            var totalWis = item.GetTotalBonusWisdom();
            var totalCha = item.GetTotalBonusCharisma();
            
            if (totalStr > 0) detailLines.Add($"  [red]+{totalStr} Strength[/]");
            if (totalDex > 0) detailLines.Add($"  [green]+{totalDex} Dexterity[/]");
            if (totalCon > 0) detailLines.Add($"  [yellow]+{totalCon} Constitution[/]");
            if (totalInt > 0) detailLines.Add($"  [purple]+{totalInt} Intelligence[/]");
            if (totalWis > 0) detailLines.Add($"  [blue]+{totalWis} Wisdom[/]");
            if (totalCha > 0) detailLines.Add($"  [cyan]+{totalCha} Charisma[/]");
        }
        
        detailLines.Add("");
        detailLines.Add($"[yellow]Description:[/] {(string.IsNullOrEmpty(item.Description) ? "[grey]No description[/]" : item.Description)}");
        
        var details = string.Join("\n", detailLines);

        ConsoleUI.ShowPanel($"Item Details", details, "cyan");
        await Task.Delay(500);
    }

    private async Task UseItemAsync()
    {
        if (Player == null || Player.Inventory.Count == 0) return;

        var consumables = Player.Inventory.Where(i => i.Type == ItemType.Consumable).ToList();
        
        if (consumables.Count == 0)
        {
            ConsoleUI.ShowWarning("You have no consumable items!");
            await Task.Delay(300);
            return;
        }

        var item = _menuService.SelectItemFromList(consumables, "Select a consumable to use");
        if (item == null) return;

        var healthBefore = Player.Health;
        var manaBefore = Player.Mana;

        // Apply consumable effects (simplified - similar to InventoryService logic)
        ApplyConsumableEffects(item, Player);

        // Remove from inventory
        Player.Inventory.Remove(item);
        await _mediator.Publish(new ItemAcquired(Player.Name, $"{item.Name} (used)"));

        // Show results
        Console.WriteLine();
        ConsoleUI.ShowSuccess($"Used {item.Name}!");
        
        if (Player.Health != healthBefore)
        {
            ConsoleUI.ShowInfo($"Health: {healthBefore} → {Player.Health}");
        }
        if (Player.Mana != manaBefore)
        {
            ConsoleUI.ShowInfo($"Mana: {manaBefore} → {Player.Mana}");
        }

        await Task.Delay(500);
    }

    private async Task EquipItemAsync()
    {
        if (Player == null || Player.Inventory.Count == 0) return;

        var equipable = Player.Inventory
            .Where(i => i.Type != ItemType.Consumable && i.Type != ItemType.QuestItem)
            .ToList();

        if (equipable.Count == 0)
        {
            ConsoleUI.ShowWarning("You have no equipable items!");
            await Task.Delay(300);
            return;
        }

        var item = _menuService.SelectItemFromList(equipable, "Select an item to equip");
        if (item == null) return;

        Item? unequipped = null;

        switch (item.Type)
        {
            case ItemType.Weapon:
                // Check if this is a two-handed weapon
                if (item.IsTwoHanded && Player.EquippedOffHand != null)
                {
                    var confirm = ConsoleUI.Confirm($"This is a two-handed weapon and will unequip your off-hand ({Player.EquippedOffHand.Name}). Continue?");
                    if (!confirm)
                    {
                        return;
                    }
                    
                    // Unequip off-hand first
                    Player.Inventory.Add(Player.EquippedOffHand);
                    Player.EquippedOffHand = null;
                    ConsoleUI.ShowInfo($"Unequipped {Player.EquippedOffHand?.Name ?? "off-hand"}");
                }
                
                unequipped = Player.EquippedMainHand;
                Player.EquippedMainHand = item;
                break;

            case ItemType.Shield:
            case ItemType.OffHand:
                // Check if main hand has a two-handed weapon
                if (Player.EquippedMainHand != null && Player.EquippedMainHand.IsTwoHanded)
                {
                    ConsoleUI.ShowWarning($"Cannot equip off-hand while wielding a two-handed weapon ({Player.EquippedMainHand.Name})!");
                    await Task.Delay(500);
                    return;
                }
                
                unequipped = Player.EquippedOffHand;
                Player.EquippedOffHand = item;
                break;

            case ItemType.Helmet:
                unequipped = Player.EquippedHelmet;
                Player.EquippedHelmet = item;
                break;

            case ItemType.Shoulders:
                unequipped = Player.EquippedShoulders;
                Player.EquippedShoulders = item;
                break;

            case ItemType.Chest:
                unequipped = Player.EquippedChest;
                Player.EquippedChest = item;
                break;

            case ItemType.Bracers:
                unequipped = Player.EquippedBracers;
                Player.EquippedBracers = item;
                break;

            case ItemType.Gloves:
                unequipped = Player.EquippedGloves;
                Player.EquippedGloves = item;
                break;

            case ItemType.Belt:
                unequipped = Player.EquippedBelt;
                Player.EquippedBelt = item;
                break;

            case ItemType.Legs:
                unequipped = Player.EquippedLegs;
                Player.EquippedLegs = item;
                break;

            case ItemType.Boots:
                unequipped = Player.EquippedBoots;
                Player.EquippedBoots = item;
                break;

            case ItemType.Necklace:
                unequipped = Player.EquippedNecklace;
                Player.EquippedNecklace = item;
                break;

            case ItemType.Ring:
                // Special handling for rings - let player choose slot
                unequipped = await EquipRingAsync(item);
                break;

            default:
                ConsoleUI.ShowWarning($"Cannot equip {item.Type} type items!");
                await Task.Delay(300);
                return;
        }

        // Remove from inventory
        Player.Inventory.Remove(item);

        // Add previously equipped item back to inventory
        if (unequipped != null)
        {
            Player.Inventory.Add(unequipped);
        }

        ConsoleUI.ShowSuccess($"Equipped {item.Name}!");
        if (unequipped != null)
        {
            ConsoleUI.ShowInfo($"Unequipped {unequipped.Name}");
        }

        await Task.Delay(500);
    }

    private Task<Item?> EquipRingAsync(Item ring)
    {
        if (Player == null) return Task.FromResult<Item?>(null);

        // Both rings empty - equip to slot 1
        if (Player.EquippedRing1 == null && Player.EquippedRing2 == null)
        {
            Player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 1 empty - equip there
        if (Player.EquippedRing1 == null)
        {
            Player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 2 empty - equip there
        if (Player.EquippedRing2 == null)
        {
            Player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Both rings equipped - ask which to replace
        var choice = ConsoleUI.ShowMenu(
            "Both ring slots are occupied. Which ring slot?",
            $"Ring 1: {Player.EquippedRing1.Name}",
            $"Ring 2: {Player.EquippedRing2.Name}",
            "Cancel"
        );

        if (choice.StartsWith("Ring 1"))
        {
            var old = Player.EquippedRing1;
            Player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(old);
        }
        else if (choice.StartsWith("Ring 2"))
        {
            var old = Player.EquippedRing2;
            Player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(old);
        }

        return Task.FromResult<Item?>(null); // Cancelled
    }

    private async Task DropItemAsync()
    {
        if (Player == null || Player.Inventory.Count == 0) return;

        var item = SelectItemFromInventory("Select an item to drop");
        if (item == null) return;

        if (!ConsoleUI.Confirm($"Drop {item.Name}? This cannot be undone."))
        {
            return;
        }

        Player.Inventory.Remove(item);
        ConsoleUI.ShowWarning($"Dropped {item.Name}");
        Log.Information("Player {PlayerName} dropped item: {ItemName}", Player.Name, item.Name);

        await Task.Delay(300);
    }

    private void SortInventory()
    {
        if (Player == null || Player.Inventory.Count == 0) return;

        var sortChoice = ConsoleUI.ShowMenu(
            "Sort by...",
            "Name",
            "Type",
            "Rarity",
            "Value",
            "Cancel"
        );

        switch (sortChoice)
        {
            case "Name":
                Player.Inventory.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                ConsoleUI.ShowSuccess("Sorted by name");
                break;

            case "Type":
                Player.Inventory.Sort((a, b) => a.Type.CompareTo(b.Type));
                ConsoleUI.ShowSuccess("Sorted by type");
                break;

            case "Rarity":
                Player.Inventory.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
                ConsoleUI.ShowSuccess("Sorted by rarity");
                break;

            case "Value":
                Player.Inventory.Sort((a, b) => b.Price.CompareTo(a.Price));
                ConsoleUI.ShowSuccess("Sorted by value");
                break;
        }
    }

    private Item? SelectItemFromInventory(string prompt)
    {
        if (Player == null || Player.Inventory.Count == 0) return null;
        return _menuService.SelectItemFromList(Player.Inventory, prompt);
    }

    private static void ApplyConsumableEffects(Item item, Character character)
    {
        var itemNameLower = item.Name.ToLower();

        // Mana potions (check first to avoid "potion" matching health)
        if (itemNameLower.Contains("mana") || itemNameLower.Contains("magic") || itemNameLower.Contains("energy"))
        {
            var manaAmount = item.Rarity switch
            {
                ItemRarity.Common => 20,
                ItemRarity.Uncommon => 35,
                ItemRarity.Rare => 50,
                ItemRarity.Epic => 75,
                ItemRarity.Legendary => 100,
                _ => 15
            };

            character.Mana = Math.Min(character.Mana + manaAmount, character.MaxMana);
        }
        // Health potions
        else if (itemNameLower.Contains("health") || itemNameLower.Contains("potion") || itemNameLower.Contains("healing"))
        {
            var healAmount = item.Rarity switch
            {
                ItemRarity.Common => 30,
                ItemRarity.Uncommon => 50,
                ItemRarity.Rare => 75,
                ItemRarity.Epic => 100,
                ItemRarity.Legendary => 150,
                _ => 20
            };

            character.Health = Math.Min(character.Health + healAmount, character.MaxHealth);
        }
        // Default: small health boost
        else
        {
            character.Health = Math.Min(character.Health + 10, character.MaxHealth);
        }
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

