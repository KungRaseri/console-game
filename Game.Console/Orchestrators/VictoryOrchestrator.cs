using MediatR;
using Game.Console.UI;
using Game.Core.Models;
using Game.Core.Features.Achievement.Queries;
using Serilog;
using Game.Core.Features.Victory.Commands;

namespace Game.Console.Orchestrators;

/// <summary>
/// Orchestrates the multi-step victory sequence with UI interactions.
/// </summary>
public class VictoryOrchestrator
{
    private readonly IMediator _mediator;
    private readonly IConsoleUI _console;

    public VictoryOrchestrator(IMediator mediator, IConsoleUI console)
    {
        _mediator = mediator;
        _console = console;
    }

    public async Task<bool> ShowVictorySequenceAsync()
    {
        // Trigger victory command
        var victoryResult = await _mediator.Send(new TriggerVictoryCommand());

        if (!victoryResult.Success || victoryResult.Statistics == null)
            return false;

        var stats = victoryResult.Statistics;

        // Victory sequence
        _console.Clear();
        await ShowDramaticVictoryAsync();
        await ShowStatisticsAsync(stats);
        await ShowAchievementsAsync();

        // Offer New Game+
        return await OfferNewGamePlusAsync();
    }

    private async Task ShowDramaticVictoryAsync()
    {
        _console.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(500);
        _console.ShowSuccess("        THE DARK LORD HAS FALLEN!              ");
        await Task.Delay(1000);
        _console.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(1500);

        System.Console.Clear();
        _console.ShowSuccess("Light returns to the world...");
        await Task.Delay(2000);

        _console.ShowSuccess("The apocalypse has been averted...");
        await Task.Delay(2000);

        _console.ShowSuccess("You are the savior of the realm!");
        await Task.Delay(2000);

        System.Console.Clear();
        _console.ShowSuccess("═══════════════════════════════════════════════");
        _console.ShowSuccess("                                               ");
        _console.ShowSuccess("              🏆 VICTORY! 🏆                   ");
        _console.ShowSuccess("                                               ");
        _console.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(3000);
    }

    private async Task ShowStatisticsAsync(VictoryStatistics stats)
    {
        System.Console.Clear();
        _console.ShowBanner("Final Statistics", $"{stats.PlayerName} the {stats.ClassName}");
        System.Console.WriteLine();

        var headers = new[] { "Stat", "Value" };
        var rows = new List<string[]>
        {
            new[] { "Final Level", stats.FinalLevel.ToString() },
            new[] { "Difficulty", stats.Difficulty },
            new[] { "Play Time", $"{stats.PlayTimeMinutes / 60}h {stats.PlayTimeMinutes % 60}m" },
            new[] { "Quests Completed", stats.QuestsCompleted.ToString() },
            new[] { "Enemies Defeated", stats.EnemiesDefeated.ToString() },
            new[] { "Deaths", stats.DeathCount.ToString() },
            new[] { "Achievements", $"{stats.AchievementsUnlocked} unlocked" },
            new[] { "Total Gold Earned", $"{stats.TotalGoldEarned}g" }
        };

        _console.ShowTable("Your Journey", headers, rows);

        Log.Information("Victory statistics displayed for {PlayerName}", stats.PlayerName);

        await Task.Delay(5000);
    }

    private async Task ShowAchievementsAsync()
    {
        System.Console.Clear();
        _console.ShowBanner("Achievements", "Your Accomplishments");
        System.Console.WriteLine();

        var query = new GetUnlockedAchievementsQuery();
        var achievements = await _mediator.Send(query);

        if (achievements.Any())
        {
            foreach (var achievement in achievements)
            {
                _console.WriteText($"{achievement.Icon} {achievement.Title} - {achievement.Points} points");
            }
        }
        else
        {
            _console.WriteText("No achievements unlocked yet.");
        }

        await Task.Delay(4000);
    }

    private async Task<bool> OfferNewGamePlusAsync()
    {
        System.Console.Clear();
        _console.ShowBanner("New Game+", "Continue Your Journey");
        System.Console.WriteLine();

        _console.WriteText("You have completed the game!");
        _console.WriteText("New Game+ is now available with:");
        _console.WriteText("  • Increased enemy difficulty");
        _console.WriteText("  • Better rewards (XP, gold, items)");
        _console.WriteText("  • Keep your achievements");
        _console.WriteText("  • Start with bonus stats");
        System.Console.WriteLine();

        if (_console.Confirm("Start New Game+?"))
        {
            var result = await _mediator.Send(new StartNewGamePlusCommand());

            if (result.Success)
            {
                _console.ShowSuccess(result.Message);
                await Task.Delay(2000);
                return true;
            }
        }

        return false;
    }
}
