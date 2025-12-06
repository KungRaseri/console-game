using Game.Models;
using Game.UI;
using Game.Handlers;
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
        if (_player == null)
        {
            _state = GameState.InGame;
            return;
        }

        bool inInventory = true;

        while (inInventory)
        {
            Console.WriteLine();
            
            // Display inventory summary
            var inventoryCount = _player.Inventory.Count;
            var totalValue = _player.Inventory.Sum(i => i.Price);

            if (inventoryCount == 0)
            {
                ConsoleUI.ShowInfo("Your inventory is empty.");
                ConsoleUI.ShowPanel("Equipment", GetEquipmentDisplay(), "cyan");
                
                if (!ConsoleUI.Confirm("Return to game?"))
                {
                    continue;
                }
                inInventory = false;
                break;
            }

            // Show inventory stats
            ConsoleUI.ShowBanner($"Inventory ({inventoryCount} items)", $"Total Value: {totalValue} gold");

            // Show equipped items
            ConsoleUI.ShowPanel("Equipment", GetEquipmentDisplay(), "cyan");

            // Group items by type for display
            var itemsByType = _player.Inventory
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
            var action = ConsoleUI.ShowMenu(
                "What would you like to do?",
                "View Item Details",
                "Use Item",
                "Equip Item",
                "Drop Item",
                "Sort Inventory",
                "Back to Game"
            );

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

        // Random chance to find an item (30% chance)
        if (Random.Shared.Next(100) < 30)
        {
            var foundItem = Generators.ItemGenerator.Generate();
            
            _player.Inventory.Add(foundItem);
            await _mediator.Publish(new ItemAcquired(_player.Name, foundItem.Name));
            
            ConsoleUI.ShowSuccess($"Found: {foundItem.Name} ({GetRarityColor(foundItem.Rarity)}{foundItem.Rarity}[/])!");
        }

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

    // ========== Inventory Helper Methods ==========

    private string GetEquipmentDisplay()
    {
        if (_player == null) return "No character";

        var lines = new List<string>();
        
        // Weapons
        lines.Add("[underline yellow]Weapons & Off-hand[/]");
        lines.Add($"  [yellow]Main Hand:[/] {GetItemDisplay(_player.EquippedMainHand)}");
        lines.Add($"  [yellow]Off Hand:[/]  {GetItemDisplay(_player.EquippedOffHand)}");
        lines.Add("");
        
        // Armor
        lines.Add("[underline yellow]Armor[/]");
        lines.Add($"  [yellow]Helmet:[/]    {GetItemDisplay(_player.EquippedHelmet)}");
        lines.Add($"  [yellow]Shoulders:[/] {GetItemDisplay(_player.EquippedShoulders)}");
        lines.Add($"  [yellow]Chest:[/]     {GetItemDisplay(_player.EquippedChest)}");
        lines.Add($"  [yellow]Bracers:[/]   {GetItemDisplay(_player.EquippedBracers)}");
        lines.Add($"  [yellow]Gloves:[/]    {GetItemDisplay(_player.EquippedGloves)}");
        lines.Add($"  [yellow]Belt:[/]      {GetItemDisplay(_player.EquippedBelt)}");
        lines.Add($"  [yellow]Legs:[/]      {GetItemDisplay(_player.EquippedLegs)}");
        lines.Add($"  [yellow]Boots:[/]     {GetItemDisplay(_player.EquippedBoots)}");
        lines.Add("");
        
        // Jewelry
        lines.Add("[underline yellow]Jewelry[/]");
        lines.Add($"  [yellow]Necklace:[/]  {GetItemDisplay(_player.EquippedNecklace)}");
        lines.Add($"  [yellow]Ring 1:[/]    {GetItemDisplay(_player.EquippedRing1)}");
        lines.Add($"  [yellow]Ring 2:[/]    {GetItemDisplay(_player.EquippedRing2)}");
        lines.Add("");
        
        // D20 Attributes
        lines.Add("[underline yellow]Attributes[/]");
        var allSets = Data.EquipmentSetRepository.GetAllSets();
        lines.Add($"  [red]Strength (STR):[/]     {_player.GetTotalStrength(allSets)} ([grey]{_player.Strength} base[/])");
        lines.Add($"  [green]Dexterity (DEX):[/]    {_player.GetTotalDexterity(allSets)} ([grey]{_player.Dexterity} base[/])");
        lines.Add($"  [yellow]Constitution (CON):[/] {_player.GetTotalConstitution(allSets)} ([grey]{_player.Constitution} base[/])");
        lines.Add($"  [purple]Intelligence (INT):[/] {_player.GetTotalIntelligence(allSets)} ([grey]{_player.Intelligence} base[/])");
        lines.Add($"  [blue]Wisdom (WIS):[/]        {_player.GetTotalWisdom(allSets)} ([grey]{_player.Wisdom} base[/])");
        lines.Add($"  [cyan]Charisma (CHA):[/]      {_player.GetTotalCharisma(allSets)} ([grey]{_player.Charisma} base[/])");
        lines.Add("");
        
        // Derived Stats
        lines.Add("[underline yellow]Derived Stats[/]");
        lines.Add($"  [red]Physical Damage:[/] +{_player.GetPhysicalDamageBonus()}");
        lines.Add($"  [purple]Magic Damage:[/]    +{_player.GetMagicDamageBonus()}");
        lines.Add($"  [green]Dodge Chance:[/]    {_player.GetDodgeChance():F1}%");
        lines.Add($"  [yellow]Crit Chance:[/]     {_player.GetCriticalChance():F1}%");
        lines.Add($"  [blue]Physical Defense:[/] {_player.GetPhysicalDefense()}");
        lines.Add($"  [cyan]Magic Resist:[/]     {_player.GetMagicResistance():F1}%");
        lines.Add($"  [magenta]Shop Discount:[/]   {_player.GetShopDiscount():F1}%");
        lines.Add($"  [white]Rare Find:[/]        {_player.GetRareItemChance():F1}%");
        
        // Active Equipment Sets
        var activeSets = _player.GetActiveEquipmentSets();
        if (activeSets.Any())
        {
            lines.Add("");
            lines.Add("[underline yellow]Active Equipment Sets[/]");
            
            foreach (var (setName, piecesEquipped) in activeSets)
            {
                var set = allSets.FirstOrDefault(s => s.Name == setName);
                if (set != null)
                {
                    lines.Add($"  [cyan]{setName}:[/] {piecesEquipped}/{set.SetItemNames.Count} pieces");
                    
                    // Show active bonuses
                    foreach (var (requiredPieces, bonus) in set.Bonuses.OrderBy(b => b.Key))
                    {
                        if (piecesEquipped >= requiredPieces)
                        {
                            lines.Add($"    [green]✓[/] ({requiredPieces}) {bonus.Description}");
                        }
                        else
                        {
                            lines.Add($"    [grey]○ ({requiredPieces}) {bonus.Description}[/]");
                        }
                    }
                }
            }
        }
        
        return string.Join("\n", lines);
    }

    private string GetItemDisplay(Item? item)
    {
        if (item == null) return "[grey]Empty[/]";
        
        var displayName = item.GetDisplayName();
        return $"{GetRarityColor(item.Rarity)}{displayName}[/]";
    }

    private string GetRarityColor(ItemRarity rarity)
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
        if (_player == null || _player.Inventory.Count == 0) return;

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
        await Task.Delay(1500);
    }

    private async Task UseItemAsync()
    {
        if (_player == null || _player.Inventory.Count == 0) return;

        var consumables = _player.Inventory.Where(i => i.Type == ItemType.Consumable).ToList();
        
        if (consumables.Count == 0)
        {
            ConsoleUI.ShowWarning("You have no consumable items!");
            await Task.Delay(1500);
            return;
        }

        var item = SelectItemFromList(consumables, "Select a consumable to use");
        if (item == null) return;

        var healthBefore = _player.Health;
        var manaBefore = _player.Mana;

        // Apply consumable effects (simplified - similar to InventoryService logic)
        ApplyConsumableEffects(item, _player);

        // Remove from inventory
        _player.Inventory.Remove(item);
        await _mediator.Publish(new ItemAcquired(_player.Name, $"{item.Name} (used)"));

        // Show results
        Console.WriteLine();
        ConsoleUI.ShowSuccess($"Used {item.Name}!");
        
        if (_player.Health != healthBefore)
        {
            ConsoleUI.ShowInfo($"Health: {healthBefore} → {_player.Health}");
        }
        if (_player.Mana != manaBefore)
        {
            ConsoleUI.ShowInfo($"Mana: {manaBefore} → {_player.Mana}");
        }

        await Task.Delay(2000);
    }

    private async Task EquipItemAsync()
    {
        if (_player == null || _player.Inventory.Count == 0) return;

        var equipable = _player.Inventory
            .Where(i => i.Type != ItemType.Consumable && i.Type != ItemType.QuestItem)
            .ToList();

        if (equipable.Count == 0)
        {
            ConsoleUI.ShowWarning("You have no equipable items!");
            await Task.Delay(1500);
            return;
        }

        var item = SelectItemFromList(equipable, "Select an item to equip");
        if (item == null) return;

        Item? unequipped = null;

        switch (item.Type)
        {
            case ItemType.Weapon:
                // Check if this is a two-handed weapon
                if (item.IsTwoHanded && _player.EquippedOffHand != null)
                {
                    var confirm = ConsoleUI.Confirm($"This is a two-handed weapon and will unequip your off-hand ({_player.EquippedOffHand.Name}). Continue?");
                    if (!confirm)
                    {
                        return;
                    }
                    
                    // Unequip off-hand first
                    _player.Inventory.Add(_player.EquippedOffHand);
                    _player.EquippedOffHand = null;
                    ConsoleUI.ShowInfo($"Unequipped {_player.EquippedOffHand?.Name ?? "off-hand"}");
                }
                
                unequipped = _player.EquippedMainHand;
                _player.EquippedMainHand = item;
                break;

            case ItemType.Shield:
            case ItemType.OffHand:
                // Check if main hand has a two-handed weapon
                if (_player.EquippedMainHand != null && _player.EquippedMainHand.IsTwoHanded)
                {
                    ConsoleUI.ShowWarning($"Cannot equip off-hand while wielding a two-handed weapon ({_player.EquippedMainHand.Name})!");
                    await Task.Delay(2000);
                    return;
                }
                
                unequipped = _player.EquippedOffHand;
                _player.EquippedOffHand = item;
                break;

            case ItemType.Helmet:
                unequipped = _player.EquippedHelmet;
                _player.EquippedHelmet = item;
                break;

            case ItemType.Shoulders:
                unequipped = _player.EquippedShoulders;
                _player.EquippedShoulders = item;
                break;

            case ItemType.Chest:
                unequipped = _player.EquippedChest;
                _player.EquippedChest = item;
                break;

            case ItemType.Bracers:
                unequipped = _player.EquippedBracers;
                _player.EquippedBracers = item;
                break;

            case ItemType.Gloves:
                unequipped = _player.EquippedGloves;
                _player.EquippedGloves = item;
                break;

            case ItemType.Belt:
                unequipped = _player.EquippedBelt;
                _player.EquippedBelt = item;
                break;

            case ItemType.Legs:
                unequipped = _player.EquippedLegs;
                _player.EquippedLegs = item;
                break;

            case ItemType.Boots:
                unequipped = _player.EquippedBoots;
                _player.EquippedBoots = item;
                break;

            case ItemType.Necklace:
                unequipped = _player.EquippedNecklace;
                _player.EquippedNecklace = item;
                break;

            case ItemType.Ring:
                // Special handling for rings - let player choose slot
                unequipped = await EquipRingAsync(item);
                break;

            default:
                ConsoleUI.ShowWarning($"Cannot equip {item.Type} type items!");
                await Task.Delay(1500);
                return;
        }

        // Remove from inventory
        _player.Inventory.Remove(item);

        // Add previously equipped item back to inventory
        if (unequipped != null)
        {
            _player.Inventory.Add(unequipped);
        }

        ConsoleUI.ShowSuccess($"Equipped {item.Name}!");
        if (unequipped != null)
        {
            ConsoleUI.ShowInfo($"Unequipped {unequipped.Name}");
        }

        await Task.Delay(1500);
    }

    private Task<Item?> EquipRingAsync(Item ring)
    {
        if (_player == null) return Task.FromResult<Item?>(null);

        // Both rings empty - equip to slot 1
        if (_player.EquippedRing1 == null && _player.EquippedRing2 == null)
        {
            _player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 1 empty - equip there
        if (_player.EquippedRing1 == null)
        {
            _player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 2 empty - equip there
        if (_player.EquippedRing2 == null)
        {
            _player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Both rings equipped - ask which to replace
        var choice = ConsoleUI.ShowMenu(
            "Both ring slots are occupied. Which ring slot?",
            $"Ring 1: {_player.EquippedRing1.Name}",
            $"Ring 2: {_player.EquippedRing2.Name}",
            "Cancel"
        );

        if (choice.StartsWith("Ring 1"))
        {
            var old = _player.EquippedRing1;
            _player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(old);
        }
        else if (choice.StartsWith("Ring 2"))
        {
            var old = _player.EquippedRing2;
            _player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(old);
        }

        return Task.FromResult<Item?>(null); // Cancelled
    }

    private async Task DropItemAsync()
    {
        if (_player == null || _player.Inventory.Count == 0) return;

        var item = SelectItemFromInventory("Select an item to drop");
        if (item == null) return;

        if (!ConsoleUI.Confirm($"Drop {item.Name}? This cannot be undone."))
        {
            return;
        }

        _player.Inventory.Remove(item);
        ConsoleUI.ShowWarning($"Dropped {item.Name}");
        Log.Information("Player {PlayerName} dropped item: {ItemName}", _player.Name, item.Name);

        await Task.Delay(1000);
    }

    private void SortInventory()
    {
        if (_player == null || _player.Inventory.Count == 0) return;

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
                _player.Inventory.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                ConsoleUI.ShowSuccess("Sorted by name");
                break;

            case "Type":
                _player.Inventory.Sort((a, b) => a.Type.CompareTo(b.Type));
                ConsoleUI.ShowSuccess("Sorted by type");
                break;

            case "Rarity":
                _player.Inventory.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
                ConsoleUI.ShowSuccess("Sorted by rarity");
                break;

            case "Value":
                _player.Inventory.Sort((a, b) => b.Price.CompareTo(a.Price));
                ConsoleUI.ShowSuccess("Sorted by value");
                break;
        }
    }

    private Item? SelectItemFromInventory(string prompt)
    {
        if (_player == null || _player.Inventory.Count == 0) return null;
        return SelectItemFromList(_player.Inventory, prompt);
    }

    private Item? SelectItemFromList(List<Item> items, string prompt)
    {
        if (items.Count == 0) return null;

        var itemNames = items.Select(i => 
            $"{i.Name} ({GetRarityColor(i.Rarity)}{i.Rarity}[/]) - {i.Type}").ToArray();
        
        var selected = ConsoleUI.ShowMenu(prompt, itemNames.Concat(new[] { "Cancel" }).ToArray());
        
        if (selected == "Cancel") return null;

        var index = Array.IndexOf(itemNames, selected);
        return index >= 0 && index < items.Count ? items[index] : null;
    }

    private void ApplyConsumableEffects(Item item, Character character)
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
