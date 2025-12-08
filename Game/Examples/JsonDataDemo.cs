using Game.Generators;
using Game.Models;
using Game.Services;
using Game.UI;
using Spectre.Console;

namespace Game.Examples;

/// <summary>
/// Demonstrates the new JSON-based data system for procedural generation.
/// </summary>
public static class JsonDataDemo
{
    public static void Run()
    {
        ConsoleUI.ShowBanner("JSON Data System Demo", "Procedural Content Generation");
        
        // Show data stats
        ShowDataStatistics();
        
        AnsiConsole.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to see generated items...");
        
        // Generate and display items
        ShowGeneratedItems();
        
        AnsiConsole.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to see generated enemies...");
        
        // Generate and display enemies
        ShowGeneratedEnemies();
        
        AnsiConsole.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to see generated NPCs...");
        
        // Generate and display NPCs
        ShowGeneratedNPCs();
        
        AnsiConsole.WriteLine();
        ConsoleUI.ShowSuccess("Demo complete! All generators are using JSON data.");
    }
    
    private static void ShowDataStatistics()
    {
        ConsoleUI.ShowPanel("Data Loaded from JSON Files", @"
📁 Items
   • Weapon Prefixes: 38 across 5 rarities
   • Weapon Types: 54 variations
   • Armor Materials: 30 across 5 rarities
   • Enchantment Suffixes: 60 across 10 types

🐉 Enemies
   • Beast Names: 15 prefixes, 15 creatures, 24 variants
   • Undead Names: 14 prefixes, 14 creatures, 16 variants
   • Demon Names: 14 prefixes, 14 creatures, 16 variants
   • Dragon Names: 10 prefixes, 13 colors, 6 types
   • Elemental Names: 14 prefixes, 12 creatures
   • Humanoid Names: 14 professions, 12 roles

👥 NPCs
   • Fantasy Names: 20 male, 20 female, 20 surnames
   • Occupations: 100+ across 10 categories
   • Dialogue Templates: 50+ context-specific lines
", "white");
    }
    
    private static void ShowGeneratedItems()
    {
        AnsiConsole.MarkupLine("\n[yellow]━━━ Generated Weapons (from JSON data) ━━━[/]");
        
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 5);
        foreach (var weapon in weapons)
        {
            var rarityColor = weapon.Rarity switch
            {
                ItemRarity.Common => "gray",
                ItemRarity.Uncommon => "green",
                ItemRarity.Rare => "blue",
                ItemRarity.Epic => "purple",
                ItemRarity.Legendary => "orange1",
                _ => "white"
            };
            
            AnsiConsole.MarkupLine($"  [{rarityColor}]● {weapon.Name.Replace("[", "[[").Replace("]", "]]")}[/] ({weapon.Rarity})");
        }
        
        AnsiConsole.MarkupLine("\n[yellow]━━━ Generated Armor (from JSON data) ━━━[/]");
        
        var armor = ItemGenerator.GenerateByType(ItemType.Chest, 5);
        foreach (var item in armor)
        {
            var rarityColor = item.Rarity switch
            {
                ItemRarity.Common => "gray",
                ItemRarity.Uncommon => "green",
                ItemRarity.Rare => "blue",
                ItemRarity.Epic => "purple",
                ItemRarity.Legendary => "orange1",
                _ => "white"
            };
            
            AnsiConsole.MarkupLine($"  [{rarityColor}]● {item.Name.Replace("[", "[[").Replace("]", "]]")}[/] ({item.Rarity})");
        }
    }
    
    private static void ShowGeneratedEnemies()
    {
        var enemyTypes = new[] 
        { 
            EnemyType.Beast, 
            EnemyType.Undead, 
            EnemyType.Demon, 
            EnemyType.Elemental, 
            EnemyType.Dragon,
            EnemyType.Humanoid 
        };
        
        AnsiConsole.MarkupLine("\n[red]━━━ Generated Enemies (from JSON data) ━━━[/]");
        
        foreach (var type in enemyTypes)
        {
            var enemy = EnemyGenerator.GenerateByType(type, playerLevel: 10);
            
            var typeColor = type switch
            {
                EnemyType.Beast => "green",
                EnemyType.Undead => "gray",
                EnemyType.Demon => "red",
                EnemyType.Elemental => "cyan",
                EnemyType.Dragon => "orange1",
                EnemyType.Humanoid => "yellow",
                _ => "white"
            };
            
            var safeName = enemy.Name.Replace("[", "[[").Replace("]", "]]");
            AnsiConsole.MarkupLine($"  [{typeColor}]⚔[/] {safeName} (Level {enemy.Level} {type})");
        }
    }
    
    private static void ShowGeneratedNPCs()
    {
        AnsiConsole.MarkupLine("\n[cyan]━━━ Generated NPCs (from JSON data) ━━━[/]");
        
        var npcs = NpcGenerator.Generate(8);
        foreach (var npc in npcs)
        {
            var friendlyIcon = npc.IsFriendly ? "😊" : "😠";
            var friendlyColor = npc.IsFriendly ? "green" : "red";
            
            var safeName = npc.Name.Replace("[", "[[").Replace("]", "]]");
            var safeOccupation = npc.Occupation.Replace("[", "[[").Replace("]", "]]");
            var safeDialogue = npc.Dialogue.Replace("[", "[[").Replace("]", "]]");
            
            AnsiConsole.MarkupLine($"  [{friendlyColor}]{friendlyIcon}[/] [bold]{safeName}[/], {safeOccupation}");
            AnsiConsole.MarkupLine($"     [dim]\"{safeDialogue}\"[/]");
        }
    }
}
