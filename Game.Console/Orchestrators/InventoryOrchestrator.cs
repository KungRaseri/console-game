using Game.Core.Models;
using Game.Console.UI;
using Game.Console.Services;
using MediatR;
using Serilog;
using Spectre.Console;

namespace Game.Console.Orchestrators;

/// <summary>
/// Orchestrates high-level inventory UI flows and item management.
/// </summary>
public class InventoryOrchestrator
{
    private readonly IMediator _mediator;
    private readonly MenuService _menuService;
    private readonly IConsoleUI _console;
    private readonly CharacterViewService _characterView;

    public InventoryOrchestrator(
        IMediator mediator, 
        MenuService menuService, 
        IConsoleUI console,
        CharacterViewService characterView)
    {
        _mediator = mediator;
        _menuService = menuService;
        _console = console;
        _characterView = characterView;
    }

    /// <summary>
    /// Main inventory management loop.
    /// </summary>
    public async Task HandleInventoryAsync(Character player)
    {
        if (player == null)
        {
            Log.Warning("HandleInventoryAsync called with null player");
            return;
        }

        bool inInventory = true;

        while (inInventory)
        {
            System.Console.WriteLine();
            
            // Display inventory summary
            var inventoryCount = player.Inventory.Count;
            var totalValue = player.Inventory.Sum(i => i.Price);

            if (inventoryCount == 0)
            {
                _console.ShowInfo("Your inventory is empty.");
                _console.ShowPanel("Equipment", _characterView.GetEquipmentDisplay(player), "cyan");
                
                if (!_console.Confirm("Return to game?"))
                {
                    continue;
                }
                inInventory = false;
                break;
            }

            // Show inventory stats
            _console.ShowBanner($"Inventory ({inventoryCount} items)", $"Total Value: {totalValue} gold");

            // Group items by type for display
            var itemsByType = player.Inventory
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
                    $"{i.Name} ({_characterView.GetRarityColor(i.Rarity)}{i.Rarity}[/])"));
                table.AddRow($"[cyan]{group.Key}[/]", itemList);
            }

            AnsiConsole.Write(table);
            System.Console.WriteLine();

            // Inventory actions
            var action = _menuService.ShowInventoryMenu();

            switch (action)
            {
                case "View Item Details":
                    await ViewItemDetailsAsync(player);
                    break;

                case "Use Item":
                    await UseItemAsync(player);
                    break;

                case "Equip Item":
                    await EquipItemAsync(player);
                    break;

                case "Drop Item":
                    await DropItemAsync(player);
                    break;

                case "Sort Inventory":
                    SortInventory(player);
                    break;

                case "Back to Game":
                    inInventory = false;
                    break;
            }
        }
    }

    /// <summary>
    /// Display detailed information about an item.
    /// </summary>
    public async Task ViewItemDetailsAsync(Character player)
    {
        if (player == null || player.Inventory.Count == 0) return;

        var item = SelectItemFromInventory(player, "Select an item to view");
        if (item == null) return;

        System.Console.WriteLine();
        
        var detailLines = new List<string>
        {
            $"[yellow]Name:[/] {item.GetDisplayName()}",
            $"[yellow]Type:[/] {item.Type}",
            $"[yellow]Rarity:[/] {_characterView.GetRarityColor(item.Rarity)}{item.Rarity}[/]",
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

        _console.ShowPanel($"Item Details", details, "cyan");
        await Task.Delay(500);
    }

    /// <summary>
    /// Use a consumable item.
    /// </summary>
    public async Task UseItemAsync(Character player)
    {
        if (player == null || player.Inventory.Count == 0) return;

        var consumables = player.Inventory.Where(i => i.Type == ItemType.Consumable).ToList();
        
        if (consumables.Count == 0)
        {
            _console.ShowWarning("You have no consumable items!");
            await Task.Delay(300);
            return;
        }

        var item = _menuService.SelectItemFromList(consumables, "Select a consumable to use");
        if (item == null) return;

        var healthBefore = player.Health;
        var manaBefore = player.Mana;

        // Apply consumable effects
        ApplyConsumableEffects(item, player);

        // Remove from inventory
        player.Inventory.Remove(item);
        await _mediator.Publish(new ItemAcquired(player.Name, $"{item.Name} (used)"));

        // Show results
        System.Console.WriteLine();
        _console.ShowSuccess($"Used {item.Name}!");
        
        if (player.Health != healthBefore)
        {
            _console.ShowInfo($"Health: {healthBefore} → {player.Health}");
        }
        if (player.Mana != manaBefore)
        {
            _console.ShowInfo($"Mana: {manaBefore} → {player.Mana}");
        }

        await Task.Delay(500);
    }

    /// <summary>
    /// Equip an item from inventory.
    /// </summary>
    public async Task EquipItemAsync(Character player)
    {
        if (player == null || player.Inventory.Count == 0) return;

        var equipable = player.Inventory
            .Where(i => i.Type != ItemType.Consumable && i.Type != ItemType.QuestItem)
            .ToList();

        if (equipable.Count == 0)
        {
            _console.ShowWarning("You have no equipable items!");
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
                if (item.IsTwoHanded && player.EquippedOffHand != null)
                {
                    var confirm = _console.Confirm($"This is a two-handed weapon and will unequip your off-hand ({player.EquippedOffHand.Name}). Continue?");
                    if (!confirm)
                    {
                        return;
                    }
                    
                    // Unequip off-hand first
                    player.Inventory.Add(player.EquippedOffHand);
                    player.EquippedOffHand = null;
                    _console.ShowInfo($"Unequipped {player.EquippedOffHand?.Name ?? "off-hand"}");
                }
                
                unequipped = player.EquippedMainHand;
                player.EquippedMainHand = item;
                break;

            case ItemType.Shield:
            case ItemType.OffHand:
                // Check if main hand has a two-handed weapon
                if (player.EquippedMainHand != null && player.EquippedMainHand.IsTwoHanded)
                {
                    _console.ShowWarning($"Cannot equip off-hand while wielding a two-handed weapon ({player.EquippedMainHand.Name})!");
                    await Task.Delay(500);
                    return;
                }
                
                unequipped = player.EquippedOffHand;
                player.EquippedOffHand = item;
                break;

            case ItemType.Helmet:
                unequipped = player.EquippedHelmet;
                player.EquippedHelmet = item;
                break;

            case ItemType.Shoulders:
                unequipped = player.EquippedShoulders;
                player.EquippedShoulders = item;
                break;

            case ItemType.Chest:
                unequipped = player.EquippedChest;
                player.EquippedChest = item;
                break;

            case ItemType.Bracers:
                unequipped = player.EquippedBracers;
                player.EquippedBracers = item;
                break;

            case ItemType.Gloves:
                unequipped = player.EquippedGloves;
                player.EquippedGloves = item;
                break;

            case ItemType.Belt:
                unequipped = player.EquippedBelt;
                player.EquippedBelt = item;
                break;

            case ItemType.Legs:
                unequipped = player.EquippedLegs;
                player.EquippedLegs = item;
                break;

            case ItemType.Boots:
                unequipped = player.EquippedBoots;
                player.EquippedBoots = item;
                break;

            case ItemType.Necklace:
                unequipped = player.EquippedNecklace;
                player.EquippedNecklace = item;
                break;

            case ItemType.Ring:
                // Special handling for rings - let player choose slot
                unequipped = await EquipRingAsync(player, item);
                break;

            default:
                _console.ShowWarning($"Cannot equip {item.Type} type items!");
                await Task.Delay(300);
                return;
        }

        // Remove from inventory
        player.Inventory.Remove(item);

        // Add previously equipped item back to inventory
        if (unequipped != null)
        {
            player.Inventory.Add(unequipped);
        }

        _console.ShowSuccess($"Equipped {item.Name}!");
        if (unequipped != null)
        {
            _console.ShowInfo($"Unequipped {unequipped.Name}");
        }

        await Task.Delay(500);
    }

    /// <summary>
    /// Drop an item from inventory.
    /// </summary>
    public async Task DropItemAsync(Character player)
    {
        if (player == null || player.Inventory.Count == 0) return;

        var item = SelectItemFromInventory(player, "Select an item to drop");
        if (item == null) return;

        if (!_console.Confirm($"Drop {item.Name}? This cannot be undone."))
        {
            return;
        }

        player.Inventory.Remove(item);
        _console.ShowWarning($"Dropped {item.Name}");
        Log.Information("Player {PlayerName} dropped item: {ItemName}", player.Name, item.Name);

        await Task.Delay(300);
    }

    /// <summary>
    /// Sort inventory by various criteria.
    /// </summary>
    public void SortInventory(Character player)
    {
        if (player == null || player.Inventory.Count == 0) return;

        var sortChoice = _console.ShowMenu(
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
                player.Inventory.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                _console.ShowSuccess("Sorted by name");
                break;

            case "Type":
                player.Inventory.Sort((a, b) => a.Type.CompareTo(b.Type));
                _console.ShowSuccess("Sorted by type");
                break;

            case "Rarity":
                player.Inventory.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
                _console.ShowSuccess("Sorted by rarity");
                break;

            case "Value":
                player.Inventory.Sort((a, b) => b.Price.CompareTo(a.Price));
                _console.ShowSuccess("Sorted by value");
                break;
        }
    }

    // ========== Private Helper Methods ==========

    private Task<Item?> EquipRingAsync(Character player, Item ring)
    {
        if (player == null) return Task.FromResult<Item?>(null);

        // Both rings empty - equip to slot 1
        if (player.EquippedRing1 == null && player.EquippedRing2 == null)
        {
            player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 1 empty - equip there
        if (player.EquippedRing1 == null)
        {
            player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Ring 2 empty - equip there
        if (player.EquippedRing2 == null)
        {
            player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(null);
        }

        // Both rings equipped - ask which to replace
        var choice = _console.ShowMenu(
            "Both ring slots are occupied. Which ring slot?",
            $"Ring 1: {player.EquippedRing1.Name}",
            $"Ring 2: {player.EquippedRing2.Name}",
            "Cancel"
        );

        if (choice.StartsWith("Ring 1"))
        {
            var old = player.EquippedRing1;
            player.EquippedRing1 = ring;
            return Task.FromResult<Item?>(old);
        }
        else if (choice.StartsWith("Ring 2"))
        {
            var old = player.EquippedRing2;
            player.EquippedRing2 = ring;
            return Task.FromResult<Item?>(old);
        }

        return Task.FromResult<Item?>(null); // Cancelled
    }

    private Item? SelectItemFromInventory(Character player, string prompt)
    {
        if (player == null || player.Inventory.Count == 0) return null;
        return _menuService.SelectItemFromList(player.Inventory, prompt);
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
