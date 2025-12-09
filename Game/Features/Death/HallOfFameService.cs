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
    
    public HallOfFameService(string databasePath = "halloffame.db")
    {
        _db = new LiteDatabase(databasePath);
        _heroes = _db.GetCollection<HallOfFameEntry>("heroes");
        _heroes.EnsureIndex(x => x.GetFameScore());
    }
    
    /// <summary>
    /// Add a hero to the Hall of Fame.
    /// </summary>
    public void AddEntry(HallOfFameEntry entry)
    {
        try
        {
            _heroes.Insert(entry);
            Log.Information("Added {CharacterName} to Hall of Fame (Fame Score: {Score})",
                entry.CharacterName, entry.GetFameScore());
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
            .OrderByDescending(x => x.GetFameScore())
            .Take(limit)
            .ToList();
    }
    
    /// <summary>
    /// Get top entries by fame score.
    /// </summary>
    public List<HallOfFameEntry> GetTopHeroes(int count = 10)
    {
        return _heroes.FindAll()
            .OrderByDescending(x => x.GetFameScore())
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// Display Hall of Fame in console.
    /// </summary>
    public void DisplayHallOfFame()
    {
        var entries = GetTopHeroes(20);
        
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Hall of Fame", "Legendary Heroes");
        Console.WriteLine();
        
        if (entries.Count == 0)
        {
            ConsoleUI.WriteText("No heroes yet. Be the first to earn your place in history!");
            ConsoleUI.PressAnyKey();
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
                entry.GetFameScore().ToString(),
                entry.IsPermadeath ? "Yes" : "No",
                entry.DeathDate.ToShortDateString()
            });
        }
        
        ConsoleUI.ShowTable("Top Heroes", headers, rows);
        ConsoleUI.PressAnyKey();
    }
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}
