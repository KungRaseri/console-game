using Game.Models;
using Game.Services;
using Game.Features.SaveLoad;
using Game.Shared.UI;
using Spectre.Console;
using Serilog;

namespace Game.Shared.Services;

/// <summary>
/// Service for handling all menu operations and navigation.
/// </summary>
public class MenuService
{
    private readonly GameStateService _gameState;
    private readonly SaveGameService _saveGameService;
    
    public MenuService(GameStateService gameState, SaveGameService saveGameService)
    {
        _gameState = gameState;
        _saveGameService = saveGameService;
    }
    
    /// <summary>
    /// Display the main menu and return the selected action.
    /// Returns the next GameState, or MainMenu if Exit is chosen (caller should check and set _isRunning = false).
    /// </summary>
    public string HandleMainMenu()
    {
        ConsoleUI.Clear();
        var choice = ConsoleUI.ShowMenu(
            "Main Menu",
            "New Game",
            "Load Game",
            "🏆 Hall of Fame",
            "Settings",
            "Exit"
        );

        return choice; // Return the choice string so caller can handle it
    }
    
    /// <summary>
    /// Display the in-game menu and return the selected action.
    /// </summary>
    public string ShowInGameMenu()
    {
        var player = _gameState.Player;
        
        Console.WriteLine();
        
        // Build menu options
        var menuOptions = new List<string>
        {
            "Explore",
            "🗺️  Travel",
            "⚔️  Combat",
            "View Character",
            "Inventory",
            "Rest"
        };
        
        // Add level-up option if player has unspent points
        if (player.UnspentAttributePoints > 0 || player.UnspentSkillPoints > 0)
        {
            menuOptions.Insert(3, $"[yellow]🌟 Level Up ({player.UnspentAttributePoints} attr, {player.UnspentSkillPoints} skill)[/]");
        }
        
        menuOptions.Add("Save Game");
        menuOptions.Add("Main Menu");
        
        return ConsoleUI.ShowMenu(
            $"[{player.Name}] - Level {player.Level} | HP: {player.Health}/{player.MaxHealth} | Gold: {player.Gold}",
            menuOptions.ToArray()
        );
    }
    
    /// <summary>
    /// Display the combat action menu.
    /// </summary>
    public string ShowCombatMenu()
    {
        return ConsoleUI.ShowMenu(
            "Choose your action:",
            "⚔️  Attack",
            "🛡️  Defend",
            "🧪 Use Item",
            "🏃 Flee"
        );
    }
    
    /// <summary>
    /// Display the inventory menu.
    /// </summary>
    public string ShowInventoryMenu()
    {
        return ConsoleUI.ShowMenu(
            "What would you like to do?",
            "View Item Details",
            "Use Item",
            "Equip Item",
            "Drop Item",
            "Sort Inventory",
            "Back to Game"
        );
    }
    
    /// <summary>
    /// Display the pause menu.
    /// </summary>
    public GameState HandlePauseMenu()
    {
        var choice = ConsoleUI.ShowMenu(
            "Game Paused",
            "Resume",
            "Save Game",
            "Main Menu"
        );

        return choice switch
        {
            "Resume" => GameState.InGame,
            "Save Game" => GameState.Paused, // Stay paused after saving
            "Main Menu" => GameState.MainMenu,
            _ => GameState.Paused
        };
    }
    
    /// <summary>
    /// Show item selection menu from a list and return selected item.
    /// </summary>
    public Item? SelectItemFromList(List<Item> items, string prompt)
    {
        if (!items.Any())
        {
            ConsoleUI.ShowWarning("No items available!");
            return null;
        }
        
        var itemNames = items.Select(i => $"{i.GetDisplayName()} ({GetRarityColor(i.Rarity)}{i.Rarity}[/])").ToList();
        itemNames.Add("[dim]Cancel[/]");
        
        var selection = ConsoleUI.ShowMenu(prompt, itemNames.ToArray());
        
        if (selection == "[dim]Cancel[/]")
        {
            return null;
        }
        
        var selectedIndex = itemNames.IndexOf(selection);
        return items[selectedIndex];
    }
    
    /// <summary>
    /// Show equipment slot selection menu for rings.
    /// </summary>
    public string ShowRingSlotMenu()
    {
        return ConsoleUI.ShowMenu(
            "Which ring slot?",
            "Ring Slot 1",
            "Ring Slot 2",
            "Cancel"
        );
    }
    
    private GameState HandleSettings()
    {
        ConsoleUI.ShowInfo("Settings not yet implemented");
        return GameState.MainMenu;
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
}
