using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using Game.Core.Features.Death.Commands;
using Game.Core.Features.Combat;
using Game.Console.UI;
using Game.Console.Services;
using Game.Core.Utilities;
using MediatR;
using Serilog;
using Spectre.Console;

namespace Game.Console.Orchestrators;

/// <summary>
/// Orchestrates high-level combat flow including turns, victory, and defeat.
/// </summary>
public class CombatOrchestrator
{
    private readonly IMediator _mediator;
    private readonly CombatService _combatService;
    private readonly SaveGameService _saveGameService;
    private readonly GameStateService _gameStateService;
    private readonly MenuService _menuService;
    private readonly IConsoleUI _console;
    private readonly LevelUpService _levelUpService;

    /// <summary>
    /// Delay multiplier for animations (0 = instant, 1 = normal speed). Useful for testing.
    /// </summary>
    public int DelayMultiplier { get; set; } = 1;

    private Task DelayAsync(int milliseconds)
    {
        if (DelayMultiplier == 0) return Task.CompletedTask;
        return Task.Delay(milliseconds * DelayMultiplier);
    }

    public CombatOrchestrator(
        IMediator mediator,
        CombatService combatService,
        SaveGameService saveGameService,
        GameStateService gameStateService,
        MenuService menuService,
        IConsoleUI console,
        LevelUpService levelUpService)
    {
        _mediator = mediator;
        _combatService = combatService;
        _saveGameService = saveGameService;
        _gameStateService = gameStateService;
        _menuService = menuService;
        _console = console;
        _levelUpService = levelUpService;
    }

    /// <summary>
    /// Handles the full combat encounter.
    /// Returns true if player won, false if player lost.
    /// </summary>
    public async Task<bool> HandleCombatAsync(Character player, Enemy enemy, CombatLog combatLog)
    {
        Log.Information("Combat started: {PlayerName} vs {EnemyName}", player.Name, enemy.Name);

        combatLog.AddEntry($"⚔️ Battle begins against {enemy.Name}!", CombatLogType.Info);
        _console.ShowBanner("⚔️ COMBAT ⚔️", $"A wild {enemy.Name} appears!");
        _console.WriteColoredText($"[yellow]Level {enemy.Level} {enemy.Difficulty} enemy![/]");
        await _mediator.Publish(new CombatStarted(player.Name, enemy.Name));
        await DelayAsync(500);

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
                    _console.ShowSuccess(fleeResult.Message);
                    await DelayAsync(500);
                    return false; // Combat ended, but not a victory
                }
                else
                {
                    combatLog.AddEntry("Failed to escape!", CombatLogType.Info);
                    _console.ShowError(fleeResult.Message);
                    await DelayAsync(500);
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
            await DelayAsync(200);
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
                _console.WriteColoredText($"[green]💚 Regeneration healed {regenAmount} HP[/]");
                await DelayAsync(300);
            }

            System.Console.WriteLine();
            await DelayAsync(600);
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
        _console.Clear();
        _console.ShowBanner("⚔️ COMBAT ⚔️", $"Fighting: {enemy.Name}");

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
        _console.ShowCombatLayout(combatInfo, logEntries);
        System.Console.WriteLine();
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
            _console.WriteColoredText($"[yellow]💨 {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            combatLog.AddEntry($"💥 CRIT! {result.Damage} damage!", CombatLogType.Critical);
            _console.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            combatLog.AddEntry($"⚔️ Hit for {result.Damage} damage", CombatLogType.PlayerAttack);
            _console.WriteColoredText($"[green]⚔️  {result.Message}[/]");
        }

        if (!enemy.IsAlive())
        {
            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name));
        }

        await DelayAsync(300);
    }

    private async Task ExecuteEnemyTurnAsync(Character player, Enemy enemy, bool playerDefending, CombatLog combatLog)
    {
        var result = _combatService.ExecuteEnemyAttack(enemy, player, playerDefending);

        await _mediator.Publish(new DamageTaken(player.Name, result.Damage));

        if (result.IsDodged)
        {
            combatLog.AddEntry($"💨 Dodged {enemy.Name}'s attack!", CombatLogType.Dodge);
            _console.WriteColoredText($"[cyan]💨 {result.Message}[/]");
        }
        else if (result.IsBlocked)
        {
            combatLog.AddEntry($"🛡️ Blocked {result.Damage} damage", CombatLogType.Defend);
            _console.WriteColoredText($"[blue]🛡️  {result.Message}[/]");
        }
        else if (result.IsCritical)
        {
            combatLog.AddEntry($"💥 {enemy.Name} CRIT! {result.Damage} damage!", CombatLogType.EnemyAttack);
            _console.WriteColoredText($"[red bold]💥 {result.Message}[/]");
        }
        else
        {
            combatLog.AddEntry($"🗡️ {enemy.Name} hit for {result.Damage}", CombatLogType.EnemyAttack);
            _console.WriteColoredText($"[orange1]🗡️  {result.Message}[/]");
        }

        if (!player.IsAlive())
        {
            await _mediator.Publish(new PlayerDefeated(player.Name, enemy.Name));
        }

        await DelayAsync(300);
    }

    private async Task<bool> UseItemInCombatMenuAsync(Character player, CombatLog combatLog)
    {
        var consumables = player.Inventory.Where(i => i.Type == ItemType.Consumable).ToList();

        if (!consumables.Any())
        {
            _console.ShowWarning("You have no consumable items!");
            await DelayAsync(300);
            return false;
        }

        var itemNames = consumables.Select(i => $"{i.Name} ({i.Rarity})").ToList();
        itemNames.Add("[dim]Cancel[/]");

        var selection = _console.ShowMenu("Select an item to use:", itemNames.ToArray());

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
                _console.WriteColoredText($"[green]✨ {result.Message}[/]");
            }
            else
            {
                _console.WriteColoredText($"[cyan]✨ {result.Message}[/]");
            }

            await DelayAsync(500);
            return true;
        }
        else
        {
            _console.ShowError(result.Message);
            await DelayAsync(300);
            return false;
        }
    }

    private async Task HandleCombatVictoryAsync(Character player, Enemy enemy)
    {
        _console.Clear();

        var outcome = _combatService.GenerateVictoryOutcome(player, enemy);

        _console.ShowBanner("🏆 VICTORY! 🏆", $"You defeated {enemy.Name}!");

        // Award XP
        var previousLevel = player.Level;
        player.GainExperience(outcome.XPGained);

        // Award gold
        player.Gold += outcome.GoldGained;

        // Display rewards
        _console.ShowPanel(
            "Battle Rewards",
            $"[green]+{outcome.XPGained} XP[/] ({player.Experience}/{player.Level * 100} to next level)\n" +
            $"[yellow]+{outcome.GoldGained} Gold[/] (Total: {player.Gold})",
            "green"
        );

        // Check for level up
        if (player.Level > previousLevel)
        {
            System.Console.WriteLine();
            _console.WriteColoredText($"[bold yellow]🌟 LEVEL UP! You are now level {player.Level}! 🌟[/]");
            await _mediator.Publish(new PlayerLeveledUp(player.Name, player.Level));
            await DelayAsync(500);

            // Process level-up allocation
            _console.PressAnyKey("Press any key to allocate your level-up points...");
            await _levelUpService.ProcessPendingLevelUpsAsync(player);
        }

        // Display loot
        if (outcome.LootDropped.Any())
        {
            System.Console.WriteLine();
            _console.WriteColoredText("[cyan bold]💎 Loot Dropped![/]");

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

                _console.WriteColoredText($"  [{rarityColor}]• {item.Name} ({item.Rarity})[/]");
            }
        }

        await _mediator.Publish(new CombatEnded(player.Name, true));

        // Auto-save after combat
        try
        {
            _saveGameService.AutoSave(player, player.Inventory);
            _console.WriteText("[grey]Game auto-saved[/]");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Auto-save after combat failed");
        }

        System.Console.WriteLine();
        _console.PressAnyKey("Press any key to continue...");
    }

    private async Task HandleCombatDefeatAsync(Character player, Enemy enemy)
    {
        // Get current location from GameStateService
        var currentLocation = _gameStateService.CurrentLocation;

        // Use death command to handle player death with difficulty-based penalties
        var deathResult = await _mediator.Send(new HandlePlayerDeathCommand
        {
            Player = player,
            DeathLocation = currentLocation,
            Killer = enemy
        });

        // If permadeath, the save is deleted and player is sent to main menu
        // The death handler already shows all the UI and messages
        // No need to show additional UI here

        await _mediator.Publish(new CombatEnded(player.Name, false));
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
