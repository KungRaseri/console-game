using Game.Models;
using Game.Services;
using Game.Features.SaveLoad;
using Game.Shared.UI;
using Game.Shared.Services;
using Game.Utilities;
using MediatR;
using Serilog;
using Spectre.Console;

namespace Game.Features.Combat;

/// <summary>
/// Orchestrates high-level combat flow including turns, victory, and defeat.
/// </summary>
public class CombatOrchestrator
{
    private readonly IMediator _mediator;
    private readonly CombatService _combatService;
    private readonly SaveGameService _saveGameService;
    private readonly MenuService _menuService;

    public CombatOrchestrator(
        IMediator mediator,
        CombatService combatService,
        SaveGameService saveGameService,
        MenuService menuService)
    {
        _mediator = mediator;
        _combatService = combatService;
        _saveGameService = saveGameService;
        _menuService = menuService;
    }

    /// <summary>
    /// Handles the full combat encounter.
    /// Returns true if player won, false if player lost.
    /// </summary>
    public async Task<bool> HandleCombatAsync(Character player, Enemy enemy, CombatLog combatLog)
    {
        Log.Information("Combat started: {PlayerName} vs {EnemyName}", player.Name, enemy.Name);

        combatLog.AddEntry($"⚔️ Battle begins against {enemy.Name}!", CombatLogType.Info);
        ConsoleUI.ShowBanner("⚔️ COMBAT ⚔️", $"A wild {enemy.Name} appears!");
        ConsoleUI.WriteColoredText($"[yellow]Level {enemy.Level} {enemy.Difficulty} enemy![/]");
        await _mediator.Publish(new CombatStarted(player.Name, enemy.Name));
        await Task.Delay(500);

        bool playerDefending = false;

        // Combat loop
        while (player.IsAlive() && enemy.IsAlive())
        {
            // Display combat status
            DisplayCombatStatusWithLog(player, enemy, combatLog);

            // Player turn
            var actionChoice = _menuService.ShowCombatMenu();
            var actionType = ParseCombatAction(actionChoice);

            if (actionType == CombatActionType.Flee)
            {
                var fleeResult = _combatService.AttemptFlee(player, enemy);
                if (fleeResult.Success)
                {
                    combatLog.AddEntry("💨 Escaped successfully!", CombatLogType.Info);
                    ConsoleUI.ShowSuccess(fleeResult.Message);
                    await Task.Delay(500);
                    return false; // Combat ended, but not a victory
                }
                else
                {
                    combatLog.AddEntry("Failed to escape!", CombatLogType.Info);
                    ConsoleUI.ShowError(fleeResult.Message);
                    await Task.Delay(500);
                }
            }
            else if (actionType == CombatActionType.UseItem)
            {
                var itemUsed = await UseItemInCombatMenuAsync(player, combatLog);
                if (itemUsed)
                {
                    // Item used counts as player's turn
                    playerDefending = false;
                }
                else
                {
                    continue; // Item menu cancelled, re-show combat menu
                }
            }
            else
            {
                playerDefending = (actionType == CombatActionType.Defend);
                await ExecutePlayerTurnAsync(player, enemy, actionType, combatLog);

                // Check if enemy is defeated
                if (!enemy.IsAlive())
                {
                    break;
                }
            }

            // Enemy turn
            await Task.Delay(200);
            await ExecuteEnemyTurnAsync(player, enemy, playerDefending, combatLog);

            // Check if player is defeated
            if (!player.IsAlive())
            {
                break;
            }

            // Apply regeneration at end of turn
            var regenAmount = SkillEffectCalculator.ApplyRegeneration(player);
            if (regenAmount > 0)
            {
                combatLog.AddEntry($"💚 Regeneration healed {regenAmount} HP", CombatLogType.Heal);
                ConsoleUI.WriteColoredText($"[green]💚 Regeneration healed {regenAmount} HP[/]");
                await Task.Delay(300);
            }

            Console.WriteLine();
            await Task.Delay(600);
        }

        // Combat ended
        if (player.IsAlive())
        {
            combatLog.AddEntry($"🎉 Victory! {enemy.Name} defeated!", CombatLogType.Victory);
            await HandleCombatVictoryAsync(player, enemy);
            return true;
        }
        else
        {
            combatLog.AddEntry("💀 You have been defeated...", CombatLogType.Defeat);
            await HandleCombatDefeatAsync(player, enemy);
            return false;
        }
    }

    private void DisplayCombatStatusWithLog(Character player, Enemy enemy, CombatLog combatLog)
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
        var logEntries = combatLog.GetFormattedEntries();
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

    private async Task ExecutePlayerTurnAsync(Character player, Enemy enemy, CombatActionType actionType, CombatLog combatLog)
    {
        var result = _combatService.ExecutePlayerAttack(player, enemy);

        await _mediator.Publish(new AttackPerformed(player.Name, enemy.Name, result.Damage));

        if (result.IsDodged)
        {
            combatLog.AddEntry($"💨 {result.Message}", CombatLogType.Dodge);
            ConsoleUI.WriteColoredText($"[yellow]💨 {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            combatLog.AddEntry($"💥 CRIT! {result.Damage} damage!", CombatLogType.Critical);
            ConsoleUI.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            combatLog.AddEntry($"⚔️ Hit for {result.Damage} damage", CombatLogType.PlayerAttack);
            ConsoleUI.WriteColoredText($"[green]⚔️  {result.Message}[/]");
        }

        if (!enemy.IsAlive())
        {
            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name));
        }

        await Task.Delay(300);
    }

    private async Task ExecuteEnemyTurnAsync(Character player, Enemy enemy, bool playerDefending, CombatLog combatLog)
    {
        var result = _combatService.ExecuteEnemyAttack(enemy, player, playerDefending);

        await _mediator.Publish(new DamageTaken(player.Name, result.Damage));

        if (result.IsDodged)
        {
            combatLog.AddEntry($"💨 Dodged {enemy.Name}'s attack!", CombatLogType.Dodge);
            ConsoleUI.WriteColoredText($"[cyan]💨 {result.Message}[/]");
        }
        else if (result.IsBlocked)
        {
            combatLog.AddEntry($"🛡️ Blocked {result.Damage} damage", CombatLogType.Defend);
            ConsoleUI.WriteColoredText($"[blue]🛡️  {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            combatLog.AddEntry($"💥 {enemy.Name} CRIT! {result.Damage} damage!", CombatLogType.EnemyAttack);
            ConsoleUI.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            combatLog.AddEntry($"🗡️ {enemy.Name} hit for {result.Damage}", CombatLogType.EnemyAttack);
            ConsoleUI.WriteColoredText($"[orange1]🗡️  {result.Message}[/]");
        }

        if (!player.IsAlive())
        {
            await _mediator.Publish(new PlayerDefeated(player.Name, enemy.Name));
        }

        await Task.Delay(300);
    }

    private async Task<bool> UseItemInCombatMenuAsync(Character player, CombatLog combatLog)
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
            combatLog.AddEntry($"✨ Used {item.Name}", CombatLogType.ItemUse);

            if (result.Healing > 0)
            {
                combatLog.AddEntry($"💚 Restored {result.Healing} HP", CombatLogType.Heal);
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
            await LevelUpService.ProcessPendingLevelUpsAsync(player);
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
            _saveGameService.AutoSave(player, player.Inventory);
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

    private static CombatActionType ParseCombatAction(string choice)
    {
        return choice switch
        {
            "⚔️ Attack" => CombatActionType.Attack,
            "🛡️ Defend" => CombatActionType.Defend,
            "✨ Use Item" => CombatActionType.UseItem,
            "💨 Flee" => CombatActionType.Flee,
            _ => CombatActionType.Attack
        };
    }
}
