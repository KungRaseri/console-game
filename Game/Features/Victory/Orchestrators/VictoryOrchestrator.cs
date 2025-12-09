using MediatR;
using Game.Shared.UI;
using Game.Features.Victory.Commands;
using Serilog;

namespace Game.Features.Victory.Orchestrators;

/// <summary>
/// Orchestrates the multi-step victory sequence with UI interactions.
/// </summary>
public class VictoryOrchestrator
{
    private readonly IMediator _mediator;
    
    public VictoryOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<bool> ShowVictorySequenceAsync()
    {
        // Trigger victory command
        var victoryResult = await _mediator.Send(new TriggerVictoryCommand());
        
        if (!victoryResult.Success || victoryResult.Statistics == null)
            return false;
        
        var stats = victoryResult.Statistics;
        
        // Victory sequence
        ConsoleUI.Clear();
        await ShowDramaticVictoryAsync();
        await ShowStatisticsAsync(stats);
        await ShowAchievementsAsync();
        
        // Offer New Game+
        return await OfferNewGamePlusAsync();
    }
    
    private async Task ShowDramaticVictoryAsync()
    {
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(500);
        ConsoleUI.ShowSuccess("        THE DARK LORD HAS FALLEN!              ");
        await Task.Delay(1000);
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(1500);
        
        Console.Clear();
        ConsoleUI.ShowSuccess("Light returns to the world...");
        await Task.Delay(2000);
        
        ConsoleUI.ShowSuccess("The apocalypse has been averted...");
        await Task.Delay(2000);
        
        ConsoleUI.ShowSuccess("You are the savior of the realm!");
        await Task.Delay(2000);
        
        Console.Clear();
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        ConsoleUI.ShowSuccess("                                               ");
        ConsoleUI.ShowSuccess("              🏆 VICTORY! 🏆                   ");
        ConsoleUI.ShowSuccess("                                               ");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(3000);
    }
    
    private async Task ShowStatisticsAsync(VictoryStatistics stats)
    {
        Console.Clear();
        ConsoleUI.ShowBanner("Final Statistics", $"{stats.PlayerName} the {stats.ClassName}");
        Console.WriteLine();
        
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
        
        ConsoleUI.ShowTable("Your Journey", headers, rows);
        
        Log.Information("Victory statistics displayed for {PlayerName}", stats.PlayerName);
        
        await Task.Delay(5000);
    }
    
    private async Task ShowAchievementsAsync()
    {
        Console.Clear();
        ConsoleUI.ShowBanner("Achievements", "Your Accomplishments");
        Console.WriteLine();
        
        var query = new Features.Achievement.Queries.GetUnlockedAchievementsQuery();
        var achievements = await _mediator.Send(query);
        
        if (achievements.Any())
        {
            foreach (var achievement in achievements)
            {
                ConsoleUI.WriteText($"{achievement.Icon} {achievement.Title} - {achievement.Points} points");
            }
        }
        else
        {
            ConsoleUI.WriteText("No achievements unlocked yet.");
        }
        
        await Task.Delay(4000);
    }
    
    private async Task<bool> OfferNewGamePlusAsync()
    {
        Console.Clear();
        ConsoleUI.ShowBanner("New Game+", "Continue Your Journey");
        Console.WriteLine();
        
        ConsoleUI.WriteText("You have completed the game!");
        ConsoleUI.WriteText("New Game+ is now available with:");
        ConsoleUI.WriteText("  • Increased enemy difficulty");
        ConsoleUI.WriteText("  • Better rewards (XP, gold, items)");
        ConsoleUI.WriteText("  • Keep your achievements");
        ConsoleUI.WriteText("  • Start with bonus stats");
        Console.WriteLine();
        
        if (ConsoleUI.Confirm("Start New Game+?"))
        {
            var result = await _mediator.Send(new StartNewGamePlusCommand());
            
            if (result.Success)
            {
                ConsoleUI.ShowSuccess(result.Message);
                await Task.Delay(2000);
                return true;
            }
        }
        
        return false;
    }
}
