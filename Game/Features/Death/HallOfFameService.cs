using Game.Models;
using Game.Shared.UI;
using LiteDB;
using Serilog;

namespace Game.Features.Death;

/// <summary>
/// Manages Hall of Fame entries for permadeath characters.
/// </summary>
public class HallOfFameService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<HallOfFameEntry> _heroes;
    private readonly IConsoleUI _console;
    
    public HallOfFameService(IConsoleUI console, string databasePath = "halloffame.db")
    {
        _console = console;
        _db = new LiteDatabase(databasePath);
        _heroes = _db.GetCollection<HallOfFameEntry>("heroes");
        _heroes.EnsureIndex(x => x.FameScore);
    }
    
    /// <summary>
    /// Add a hero to the Hall of Fame.
    /// </summary>
    public void AddEntry(HallOfFameEntry entry)
    {
        try
        {
            entry.CalculateFameScore(); // Calculate and store fame score before inserting
            _heroes.Insert(entry);
            Log.Information("Added {CharacterName} to Hall of Fame (Fame Score: {Score})",
                entry.CharacterName, entry.FameScore);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add Hall of Fame entry for {CharacterName}", entry.CharacterName);
        }
    }
    
    /// <summary>
    /// Get all Hall of Fame entries sorted by fame score.
    /// </summary>
    public List<HallOfFameEntry> GetAllEntries(int limit = 100)
    {
        return _heroes.FindAll()
            .OrderByDescending(x => x.FameScore)
            .Take(limit)
            .ToList();
    }
    
    /// <summary>
    /// Get top entries by fame score.
    /// </summary>
    public List<HallOfFameEntry> GetTopHeroes(int count = 10)
    {
        return _heroes.FindAll()
            .OrderByDescending(x => x.FameScore)
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// Display Hall of Fame in console.
    /// </summary>
    public void DisplayHallOfFame()
    {
        var entries = GetTopHeroes(20);
        
        _console.Clear();
        _console.ShowBanner("Hall of Fame", "Legendary Heroes");
        Console.WriteLine();
        
        if (entries.Count == 0)
        {
            _console.WriteText("No heroes yet. Be the first to earn your place in history!");
            _console.PressAnyKey();
            return;
        }
        
        var headers = new[] { "Rank", "Name", "Class", "Level", "Score", "Permadeath", "Date" };
        var rows = new List<string[]>();
        
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            rows.Add(new[]
            {
                $"#{i + 1}",
                entry.CharacterName,
                entry.ClassName,
                entry.Level.ToString(),
                entry.FameScore.ToString(),
                entry.IsPermadeath ? "Yes" : "No",
                entry.DeathDate.ToShortDateString()
            });
        }
        
        _console.ShowTable("Top Heroes", headers, rows);
        _console.PressAnyKey();
    }
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}
