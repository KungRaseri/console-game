using RealmEngine.Shared.Models;

namespace RealmEngine.Shared.Abstractions;

/// <summary>
/// Repository interface for managing Hall of Fame entries.
/// </summary>
public interface IHallOfFameRepository : IDisposable
{
    void AddEntry(HallOfFameEntry entry);
    List<HallOfFameEntry> GetAllEntries(int limit = 100);
    List<HallOfFameEntry> GetTopHeroes(int count = 10);
}
