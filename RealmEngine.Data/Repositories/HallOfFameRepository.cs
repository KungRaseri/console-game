using RealmEngine.Shared.Models;
using RealmEngine.Shared.Abstractions;
using LiteDB;
using Serilog;

namespace RealmEngine.Data.Repositories;

/// <summary>
/// Repository for managing Hall of Fame entries for permadeath characters.
/// </summary>
public class HallOfFameRepository : IHallOfFameRepository
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<HallOfFameEntry> _heroes;

    /// <summary>
    /// Initializes a new instance of the HallOfFameRepository with the specified database path.
    /// </summary>
    public HallOfFameRepository(string databasePath = "halloffame.db")
    {
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
    /// Disposes the database connection.
    /// </summary>
    public void Dispose()
    {
        _db?.Dispose();
    }
}
