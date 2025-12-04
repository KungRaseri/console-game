# ConsoleUI Integration Guide

This document provides a comprehensive guide to the expanded `ConsoleUI` and `ConsoleUIExpanded` classes, which provide rich terminal UI capabilities using Spectre.Console.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Core Features (ConsoleUI)](#core-features-consoleui)
3. [Advanced Features (ConsoleUIExpanded)](#advanced-features-consoleuiexpanded)
4. [Usage Examples](#usage-examples)
5. [Security Best Practices](#security-best-practices)
6. [Integration Patterns](#integration-patterns)

---

## Quick Start

### Basic Usage

```csharp
using Game.UI;

// Clear screen and show a banner
ConsoleUI.Clear();
ConsoleUI.ShowBanner("Welcome to the Game!", "Press any key to start");

// Get user input safely
string playerName = ConsoleUI.AskForInput("Enter your name:");

// Show a menu
string choice = ConsoleUI.ShowMenu(
    "Main Menu",
    "New Game",
    "Load Game",
    "Exit"
);

// Display messages
ConsoleUI.ShowSuccess("Game saved successfully!");
ConsoleUI.ShowError("Failed to load save file");
ConsoleUI.ShowWarning("Low health!");
ConsoleUI.ShowInfo("New quest available");
```

---

## Core Features (ConsoleUI)

### Text Output

#### Safe Text (Auto-Escaped)
```csharp
// Always safe - escapes markup characters
ConsoleUI.WriteText(userInput);  // "[red]" displays as literal text
```

#### Colored Text (Trusted Content Only)
```csharp
// Only use with hardcoded strings
ConsoleUI.WriteColoredText("[green]Health restored![/]");
```

#### Banners
```csharp
ConsoleUI.ShowBanner("Game Title", "Subtitle text");
```

### User Input

#### Text Input
```csharp
string name = ConsoleUI.AskForInput("What is your name?");
```

#### Numeric Input with Validation
```csharp
int age = ConsoleUI.AskForNumber("Enter age:", min: 1, max: 100);
```

#### Confirmation
```csharp
bool confirmed = ConsoleUI.Confirm("Are you sure?");
```

### Menus

#### Simple Menu
```csharp
string choice = ConsoleUI.ShowMenu(
    "Choose an action",
    "Option 1",
    "Option 2",
    "Option 3"
);
```

#### Multi-Select Menu
```csharp
List<string> selected = ConsoleUI.ShowMultiSelectMenu(
    "Select items",
    "Item 1",
    "Item 2",
    "Item 3"
);
```

### Display Elements

#### Tables
```csharp
ConsoleUI.ShowTable(
    "Inventory",
    new[] { "Item", "Quantity", "Value" },
    new List<string[]>
    {
        new[] { "Sword", "1", "100" },
        new[] { "Potion", "5", "50" }
    }
);
```

#### Panels
```csharp
ConsoleUI.ShowPanel(
    "Important Message",
    "This is the content inside the panel",
    "red"  // Border color
);
```

#### Progress Bars
```csharp
ConsoleUI.ShowProgress("Loading...", task =>
{
    task.MaxValue = 100;
    for (int i = 0; i <= 100; i += 10)
    {
        task.Value = i;
        Thread.Sleep(100);
    }
});
```

---

## Advanced Features (ConsoleUIExpanded)

### ASCII Art Titles

```csharp
using Game.UI;

ConsoleUIExpanded.ShowTitle("GAME TITLE", Color.Green);
```

### Async Operations with Spinners

```csharp
// With return value
var data = await ConsoleUIExpanded.ShowSpinnerAsync(
    "Loading game data...",
    async () =>
    {
        await Task.Delay(2000);
        return await LoadGameDataAsync();
    }
);

// Without return value  
await ConsoleUIExpanded.ShowSpinnerAsync(
    "Saving...",
    async () =>
    {
        await SaveGameAsync();
    }
);
```

### Tree Views (Hierarchical Data)

```csharp
ConsoleUIExpanded.ShowTree("Inventory", tree =>
{
    var weapons = tree.AddNode("[yellow]Weapons[/]");
    weapons.AddNode("Sword");
    weapons.AddNode("Bow");
    
    var potions = tree.AddNode("[blue]Potions[/]");
    potions.AddNode("Health Potion x5");
    potions.AddNode("Mana Potion x3");
});
```

### Charts

#### Bar Chart
```csharp
var stats = new Dictionary<string, double>
{
    { "Strength", 15 },
    { "Dexterity", 12 },
    { "Intelligence", 18 }
};

ConsoleUIExpanded.ShowBarChart("Character Stats", stats);
```

#### Breakdown Chart (Percentages)
```csharp
var distribution = new Dictionary<string, double>
{
    { "Warriors", 40 },
    { "Mages", 35 },
    { "Rogues", 25 }
};

ConsoleUIExpanded.ShowBreakdownChart("Class Distribution", distribution);
```

### Calendar

```csharp
ConsoleUIExpanded.ShowCalendar(2025, 12, "Game Events");
```

### Exception Display

```csharp
try
{
    // Game logic
}
catch (Exception ex)
{
    ConsoleUIExpanded.ShowException(ex);
}
```

### Layout & Columns

#### Multi-Column Layout
```csharp
ConsoleUIExpanded.ShowColumns(
    new Panel("Column 1 content"),
    new Panel("Column 2 content"),
    new Panel("Column 3 content")
);
```

#### Grid Layout
```csharp
ConsoleUIExpanded.ShowGrid(grid =>
{
    grid.AddColumn();
    grid.AddColumn();
    grid.AddRow("Label 1", "Value 1");
    grid.AddRow("Label 2", "Value 2");
});
```

#### Key-Value Lists
```csharp
var playerInfo = new Dictionary<string, string>
{
    { "Name", "Hero" },
    { "Class", "Warrior" },
    { "Level", "15" },
    { "Gold", "1234" }
};

ConsoleUIExpanded.ShowKeyValueList("Player Info", playerInfo, "green");
```

### Character Stats Display

```csharp
// Using Character model
ConsoleUIExpanded.ShowCharacterStats(character);

// Health bar
ConsoleUIExpanded.ShowHealthBar(75, 100, "Health");
```

### Advanced Prompts

#### Object Menu (Select from Custom Objects)
```csharp
var items = new List<Item> { /* ... */ };

var selected = ConsoleUIExpanded.ShowObjectMenu(
    "Select an item",
    items,
    item => item.Name  // Property to display
);
```

#### Password Input (Masked)
```csharp
string password = ConsoleUIExpanded.AskForPassword("Enter password:");
```

#### Input with Default Value
```csharp
string name = ConsoleUIExpanded.AskForInputWithDefault(
    "Character name:",
    "Hero"
);
```

#### Input with Custom Validation
```csharp
string email = ConsoleUIExpanded.AskForInputWithValidation(
    "Enter email:",
    input =>
    {
        if (!input.Contains("@"))
            return ValidationResult.Error("Must contain @");
        return ValidationResult.Success();
    }
);
```

### Utility Methods

```csharp
// Blank lines
ConsoleUIExpanded.WriteLine();
ConsoleUIExpanded.WriteLines(3);

// Console title
ConsoleUIExpanded.SetTitle("My Game - Level 5");

// Divider
ConsoleUIExpanded.ShowDivider("=", "yellow");

// Rules
ConsoleUIExpanded.ShowRule("Section Title", "blue");
```

---

## Usage Examples

### Example 1: Enhanced Main Menu

```csharp
public async Task ShowMainMenuAsync()
{
    ConsoleUI.Clear();
    ConsoleUIExpanded.ShowTitle("EPIC GAME");
    ConsoleUIExpanded.ShowRule("Main Menu");
    
    var choice = ConsoleUI.ShowMenu(
        "What would you like to do?",
        "Start New Game",
        "Continue Game",
        "Settings",
        "Exit"
    );
    
    switch (choice)
    {
        case "Start New Game":
            await StartNewGameAsync();
            break;
        // ...
    }
}
```

### Example 2: Character Sheet

```csharp
public void ShowCharacterSheet(Character player)
{
    ConsoleUI.Clear();
    ConsoleUIExpanded.ShowRule(player.Name, "green");
    
    // Main stats
    ConsoleUIExpanded.ShowCharacterStats(player);
    
    ConsoleUIExpanded.WriteLine();
    
    // Skills tree
    ConsoleUIExpanded.ShowTree("Skills", tree =>
    {
        var combat = tree.AddNode("[red]Combat[/]");
        combat.AddNode($"Sword Mastery: Level {player.SwordLevel}");
        combat.AddNode($"Shield Block: Level {player.ShieldLevel}");
        
        var magic = tree.AddNode("[blue]Magic[/]");
        magic.AddNode($"Fireball: Level {player.FireballLevel}");
    });
    
    ConsoleUI.PressAnyKey();
}
```

### Example 3: Inventory Management

```csharp
public void ShowInventory(List<Item> items)
{
    ConsoleUI.Clear();
    ConsoleUIExpanded.ShowTitle("INVENTORY");
    
    if (items.Count == 0)
    {
        ConsoleUI.ShowWarning("Your inventory is empty");
        return;
    }
    
    var selected = ConsoleUIExpanded.ShowObjectMenu(
        "Select an item to use or drop",
        items,
        item => $"{item.Name} x{item.Quantity}"
    );
    
    var action = ConsoleUI.ShowMenu(
        $"What do you want to do with {selected.Name}?",
        "Use",
        "Drop",
        "Cancel"
    );
    
    switch (action)
    {
        case "Use":
            UseItem(selected);
            ConsoleUI.ShowSuccess($"Used {selected.Name}");
            break;
        case "Drop":
            if (ConsoleUI.Confirm($"Drop {selected.Name}?"))
            {
                DropItem(selected);
                ConsoleUI.ShowInfo("Item dropped");
            }
            break;
    }
}
```

### Example 4: Battle System

```csharp
public async Task ShowBattleAsync(Character player, Enemy enemy)
{
    ConsoleUI.Clear();
    ConsoleUIExpanded.ShowRule("BATTLE!", "red");
    
    // Show combatants side by side
    ConsoleUIExpanded.ShowColumns(
        CreateCharacterPanel(player),
        new Panel("[red]VS[/]").Border(BoxBorder.Double),
        CreateEnemyPanel(enemy)
    );
    
    while (player.Health > 0 && enemy.Health > 0)
    {
        var action = ConsoleUI.ShowMenu(
            "Choose your action",
            "Attack",
            "Defend",
            "Use Item",
            "Flee"
        );
        
        await ProcessBattleActionAsync(action, player, enemy);
        
        // Show health bars
        ConsoleUIExpanded.ShowHealthBar(player.Health, player.MaxHealth, "Your Health");
        ConsoleUIExpanded.ShowHealthBar(enemy.Health, enemy.MaxHealth, "Enemy Health");
    }
}
```

### Example 5: Async Loading Screen

```csharp
public async Task<GameData> LoadGameWithSpinnerAsync()
{
    return await ConsoleUIExpanded.ShowSpinnerAsync(
        "Loading game world...",
        async () =>
        {
            var data = new GameData();
            data.World = await LoadWorldAsync();
            data.NPCs = await LoadNPCsAsync();
            data.Quests = await LoadQuestsAsync();
            return data;
        }
    );
}
```

---

## Security Best Practices

### ✅ DO

```csharp
// Always use safe methods for user input
ConsoleUI.WriteText(playerName);  // Auto-escaped
ConsoleUI.ShowSuccess(message);    // Auto-escaped
ConsoleUI.ShowMenu(title, options); // Auto-escaped

// Use WriteText for any user-generated content
ConsoleUI.WriteText(chatMessage);
```

### ❌ DON'T

```csharp
// Never use WriteColoredText with user input
ConsoleUI.WriteColoredText(userInput);  // DANGEROUS!

// Don't manually build markup with user data
AnsiConsole.MarkupLine($"[green]{userName}[/]");  // DANGEROUS!
```

### When to Escape Manually

If you need to use advanced Spectre.Console features directly:

```csharp
string safeName = userInput.Replace("[", "[[").Replace("]", "]]");
AnsiConsole.MarkupLine($"Welcome, [green]{safeName}[/]!");
```

---

## Integration Patterns

### Pattern 1: Game State Display

```csharp
public class GameEngine
{
    private void UpdateGameDisplay()
    {
        ConsoleUI.Clear();
        ConsoleUIExpanded.ShowRule($"Day {currentDay}", "yellow");
        
        var stats = new Dictionary<string, string>
        {
            { "Gold", player.Gold.ToString() },
            { "Health", $"{player.Health}/{player.MaxHealth}" },
            { "Location", currentLocation.Name }
        };
        
        ConsoleUIExpanded.ShowKeyValueList("Status", stats);
    }
}
```

### Pattern 2: Event Logging

```csharp
public class EventLogger
{
    public void LogEvent(GameEvent evt)
    {
        switch (evt.Severity)
        {
            case EventSeverity.Info:
                ConsoleUI.ShowInfo(evt.Message);
                break;
            case EventSeverity.Warning:
                ConsoleUI.ShowWarning(evt.Message);
                break;
            case EventSeverity.Error:
                ConsoleUI.ShowError(evt.Message);
                break;
            case EventSeverity.Success:
                ConsoleUI.ShowSuccess(evt.Message);
                break;
        }
    }
}
```

### Pattern 3: Tutorial System

```csharp
public class TutorialSystem
{
    public void ShowTutorialStep(string title, string content)
    {
        ConsoleUI.Clear();
        ConsoleUIExpanded.ShowRule("Tutorial", "cyan");
        
        ConsoleUI.ShowPanel(
            title,
            content,
            "cyan"
        );
        
        ConsoleUI.PressAnyKey("Press any key to continue the tutorial...");
    }
}
```

---

## Tips & Best Practices

1. **Use ConsoleUI for common tasks** - It's the secure, tested foundation
2. **Use ConsoleUIExpanded for advanced features** - When you need extra visual polish
3. **Always clear the screen** before major transitions (`ConsoleUI.Clear()`)
4. **Use ShowRule or ShowDivider** to separate sections
5. **Leverage ShowSpinnerAsync** for long-running operations
6. **Create reusable Panel methods** for consistent styling
7. **Test with different terminal sizes** - Be responsive
8. **Use color strategically** - Don't overdo it

---

## Complete Example: Shop System

```csharp
public class ShopSystem
{
    public async Task RunShopAsync(Character player, Shop shop)
    {
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUIExpanded.ShowTitle("MERCHANT");
            ConsoleUIExpanded.ShowRule($"{shop.Name} - Your Gold: {player.Gold}", "yellow");
            
            // Show items in a table
            var headers = new[] { "Item", "Price", "Description" };
            var rows = shop.Items.Select(i => new[] 
            {
                i.Name,
                i.Price.ToString(),
                i.Description
            }).ToList();
            
            ConsoleUI.ShowTable("Available Items", headers, rows);
            
            var action = ConsoleUI.ShowMenu(
                "What would you like to do?",
                "Buy Item",
                "Sell Item",
                "Leave Shop"
            );
            
            switch (action)
            {
                case "Buy Item":
                    await BuyItemAsync(player, shop);
                    break;
                case "Sell Item":
                    await SellItemAsync(player, shop);
                    break;
                case "Leave Shop":
                    ConsoleUI.ShowInfo("Thanks for visiting!");
                    return;
            }
        }
    }
    
    private async Task BuyItemAsync(Character player, Shop shop)
    {
        var item = ConsoleUIExpanded.ShowObjectMenu(
            "Select an item to buy",
            shop.Items,
            i => $"{i.Name} - {i.Price} gold"
        );
        
        if (player.Gold < item.Price)
        {
            ConsoleUI.ShowError("Not enough gold!");
            await Task.Delay(1500);
            return;
        }
        
        if (ConsoleUI.Confirm($"Buy {item.Name} for {item.Price} gold?"))
        {
            player.Gold -= item.Price;
            player.Inventory.Add(item);
            
            await ConsoleUIExpanded.ShowSpinnerAsync(
                "Processing transaction...",
                async () => await Task.Delay(500)
            );
            
            ConsoleUI.ShowSuccess($"Purchased {item.Name}!");
            await Task.Delay(1000);
        }
    }
}
```

---

## Conclusion

The expanded ConsoleUI system provides everything you need to create beautiful, secure, and user-friendly console applications. Use `ConsoleUI` for everyday tasks and `ConsoleUIExpanded` when you need that extra visual polish.

Happy coding! 🎮
