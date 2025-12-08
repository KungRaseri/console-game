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
    private readonly CharacterCreationOrchestrator _characterCreation;
    private readonly LoadGameService _loadGameService;
    private readonly GameplayService _gameplayService;
    private readonly CombatOrchestrator _combatOrchestrator;
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
        ExplorationService explorationService,
        CharacterCreationOrchestrator characterCreation,
        LoadGameService loadGameService,
        GameplayService gameplayService,
        CombatOrchestrator combatOrchestrator)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
        _gameState = gameState;
        _combatService = combatService;
        _menuService = menuService;
        _explorationService = explorationService;
        _characterCreation = characterCreation;
        _loadGameService = loadGameService;
        _gameplayService = gameplayService;
        _combatOrchestrator = combatOrchestrator;
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

