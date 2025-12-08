using Game.Models;
using Game.UI;
using Game.Data;
using Game.Utilities;

namespace Game.Services;

/// <summary>
/// Service for displaying character information and statistics.
/// </summary>
public static class CharacterViewService
{
    /// <summary>
    /// Display comprehensive character statistics.
    /// </summary>
    public static void ViewCharacter(Character player)
    {
        ConsoleUI.Clear();
        
        // Basic stats
        var statsContent = $"""
        [yellow]Name:[/] {player.Name} ([cyan]{player.ClassName}[/])
        [red]Health:[/] {player.Health}/{player.MaxHealth}
        [blue]Mana:[/] {player.Mana}/{player.MaxMana}
        [green]Level:[/] {player.Level}
        [cyan]Experience:[/] {player.Experience}/{player.Level * 100}
        [yellow]Gold:[/] {player.Gold}
        """;

        ConsoleUI.ShowPanel("Character Stats", statsContent, "green");
        
        // D20 Attributes
        var attributesContent = $"""
        [red]Strength (STR):[/] {player.Strength}
        [magenta]Dexterity (DEX):[/] {player.Dexterity}
        [green]Constitution (CON):[/] {player.Constitution}
        [cyan]Intelligence (INT):[/] {player.Intelligence}
        [blue]Wisdom (WIS):[/] {player.Wisdom}
        [yellow]Charisma (CHA):[/] {player.Charisma}
        """;
        
        Console.WriteLine();
        ConsoleUI.ShowPanel("D20 Attributes", attributesContent, "cyan");
        
        // Derived stats with skill bonuses
        var derivedContent = $"""
        [red]Physical Damage:[/] {player.GetPhysicalDamageBonus()} (+{(SkillEffectCalculator.GetPhysicalDamageMultiplier(player) - 1.0) * 100:F0}% from skills)
        [cyan]Magic Damage:[/] {player.GetMagicDamageBonus()} (+{(SkillEffectCalculator.GetMagicDamageMultiplier(player) - 1.0) * 100:F0}% from skills)
        [magenta]Dodge Chance:[/] {player.GetDodgeChance() + SkillEffectCalculator.GetDodgeChanceBonus(player):F1}%
        [yellow]Critical Chance:[/] {player.GetCriticalChance() + SkillEffectCalculator.GetCriticalChanceBonus(player):F1}%
        [green]Physical Defense:[/] {(int)(player.GetPhysicalDefense() * SkillEffectCalculator.GetPhysicalDefenseMultiplier(player))}
        [blue]Magic Resistance:[/] {player.GetMagicResistance():F1}%
        [gold1]Rare Find:[/] {player.GetRareItemChance():F1}%
        """;
        
        Console.WriteLine();
        ConsoleUI.ShowPanel("Combat Stats", derivedContent, "yellow");
        
        // Show learned skills
        if (player.LearnedSkills.Any())
        {
            Console.WriteLine();
            ConsoleUI.WriteColoredText("[bold cyan]📚 Learned Skills:[/]");
            Console.WriteLine();
            
            foreach (var skill in player.LearnedSkills.OrderBy(s => s.Type))
            {
                var typeColor = skill.Type switch
                {
                    SkillType.Combat => "red",
                    SkillType.Defense => "blue",
                    SkillType.Magic => "cyan",
                    SkillType.Utility => "yellow",
                    SkillType.Passive => "green",
                    _ => "white"
                };
                
                ConsoleUI.WriteColoredText($"  [{typeColor}]{skill.Name}[/] [dim](Rank {skill.CurrentRank}/{skill.MaxRank})[/] - {skill.Description}");
            }
        }
        
        // Show active skill bonuses
        var bonusSummary = SkillEffectCalculator.GetSkillBonusSummary(player);
        if (!bonusSummary.Contains("No active"))
        {
            Console.WriteLine();
            ConsoleUI.ShowPanel("Active Skill Bonuses", bonusSummary, "green");
        }
        
        Console.WriteLine();
        ConsoleUI.PressAnyKey();
    }
    
    /// <summary>
    /// Review the final character before starting the game.
    /// </summary>
    public static void ReviewCharacter(Character character, CharacterClass characterClass)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Character Summary", "Your Hero Awaits");
        
        var summary = new List<string>();
        summary.Add($"[yellow]Name:[/] {character.Name}");
        summary.Add($"[cyan]Class:[/] {character.ClassName}");
        summary.Add($"[green]Level:[/] {character.Level}");
        summary.Add("");
        summary.Add("[underline yellow]Final Attributes:[/]");
        summary.Add($"  [red]Strength (STR):[/]     {character.Strength}");
        summary.Add($"  [green]Dexterity (DEX):[/]    {character.Dexterity}");
        summary.Add($"  [yellow]Constitution (CON):[/] {character.Constitution}");
        summary.Add($"  [purple]Intelligence (INT):[/] {character.Intelligence}");
        summary.Add($"  [blue]Wisdom (WIS):[/]        {character.Wisdom}");
        summary.Add($"  [cyan]Charisma (CHA):[/]      {character.Charisma}");
        summary.Add("");
        summary.Add("[underline yellow]Vitals:[/]");
        summary.Add($"  [red]Health:[/] {character.MaxHealth}");
        summary.Add($"  [blue]Mana:[/]   {character.MaxMana}");
        summary.Add($"  [yellow]Gold:[/]   {character.Gold}");
        summary.Add("");
        summary.Add($"[underline yellow]Starting Equipment:[/] {character.Inventory.Count} items");
        
        ConsoleUI.ShowPanel("Your Character", string.Join("\n", summary), "cyan");
        
        ConsoleUI.PressAnyKey("Press any key to begin your adventure");
    }
    
    /// <summary>
    /// Display equipment and stats for a character.
    /// </summary>
    public static string GetEquipmentDisplay(Character player)
    {
        var lines = new List<string>();
        
        // Weapons
        lines.Add("[underline yellow]Weapons & Off-hand[/]");
        lines.Add($"  [yellow]Main Hand:[/] {GetItemDisplay(player.EquippedMainHand)}");
        lines.Add($"  [yellow]Off Hand:[/]  {GetItemDisplay(player.EquippedOffHand)}");
        lines.Add("");
        
        // Armor
        lines.Add("[underline yellow]Armor[/]");
        lines.Add($"  [yellow]Helmet:[/]    {GetItemDisplay(player.EquippedHelmet)}");
        lines.Add($"  [yellow]Shoulders:[/] {GetItemDisplay(player.EquippedShoulders)}");
        lines.Add($"  [yellow]Chest:[/]     {GetItemDisplay(player.EquippedChest)}");
        lines.Add($"  [yellow]Bracers:[/]   {GetItemDisplay(player.EquippedBracers)}");
        lines.Add($"  [yellow]Gloves:[/]    {GetItemDisplay(player.EquippedGloves)}");
        lines.Add($"  [yellow]Belt:[/]      {GetItemDisplay(player.EquippedBelt)}");
        lines.Add($"  [yellow]Legs:[/]      {GetItemDisplay(player.EquippedLegs)}");
        lines.Add($"  [yellow]Boots:[/]     {GetItemDisplay(player.EquippedBoots)}");
        lines.Add("");
        
        // Jewelry
        lines.Add("[underline yellow]Jewelry[/]");
        lines.Add($"  [yellow]Necklace:[/]  {GetItemDisplay(player.EquippedNecklace)}");
        lines.Add($"  [yellow]Ring 1:[/]    {GetItemDisplay(player.EquippedRing1)}");
        lines.Add($"  [yellow]Ring 2:[/]    {GetItemDisplay(player.EquippedRing2)}");
        lines.Add("");
        
        // D20 Attributes
        lines.Add("[underline yellow]Attributes[/]");
        var allSets = EquipmentSetRepository.GetAllSets();
        lines.Add($"  [red]Strength (STR):[/]     {player.GetTotalStrength(allSets)} ([grey]{player.Strength} base[/])");
        lines.Add($"  [green]Dexterity (DEX):[/]    {player.GetTotalDexterity(allSets)} ([grey]{player.Dexterity} base[/])");
        lines.Add($"  [yellow]Constitution (CON):[/] {player.GetTotalConstitution(allSets)} ([grey]{player.Constitution} base[/])");
        lines.Add($"  [purple]Intelligence (INT):[/] {player.GetTotalIntelligence(allSets)} ([grey]{player.Intelligence} base[/])");
        lines.Add($"  [blue]Wisdom (WIS):[/]        {player.GetTotalWisdom(allSets)} ([grey]{player.Wisdom} base[/])");
        lines.Add($"  [cyan]Charisma (CHA):[/]      {player.GetTotalCharisma(allSets)} ([grey]{player.Charisma} base[/])");
        lines.Add("");
        
        // Derived Stats
        lines.Add("[underline yellow]Derived Stats[/]");
        lines.Add($"  [red]Physical Damage:[/] +{player.GetPhysicalDamageBonus()}");
        lines.Add($"  [purple]Magic Damage:[/]    +{player.GetMagicDamageBonus()}");
        lines.Add($"  [green]Dodge Chance:[/]    {player.GetDodgeChance():F1}%");
        lines.Add($"  [yellow]Crit Chance:[/]     {player.GetCriticalChance():F1}%");
        lines.Add($"  [blue]Physical Defense:[/] {player.GetPhysicalDefense()}");
        lines.Add($"  [cyan]Magic Resist:[/]     {player.GetMagicResistance():F1}%");
        lines.Add($"  [magenta]Shop Discount:[/]   {player.GetShopDiscount():F1}%");
        lines.Add($"  [white]Rare Find:[/]        {player.GetRareItemChance():F1}%");
        
        // Active Equipment Sets
        var activeSets = player.GetActiveEquipmentSets();
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
    
    private static string GetItemDisplay(Item? item)
    {
        if (item == null) return "[grey]Empty[/]";
        
        var displayName = item.GetDisplayName();
        return $"{GetRarityColor(item.Rarity)}{displayName}[/]";
    }
    
    public static string GetRarityColor(ItemRarity rarity)
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
