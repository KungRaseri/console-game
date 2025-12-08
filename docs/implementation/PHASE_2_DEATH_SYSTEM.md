# Phase 2: Death & Penalty System

**Status**: 🟡 Not Started  
**Estimated Time**: 3-4 hours  
**Previous Phase**: [Phase 1: Difficulty Foundation](./PHASE_1_DIFFICULTY_FOUNDATION.md)  
**Next Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)  
**Related**: [Phase 4: End-Game](./PHASE_4_ENDGAME.md)

---

## 📋 Overview

Implement comprehensive death handling with difficulty-specific penalties, item dropping, respawning, Hall of Fame for permadeath, and proper save integration.

---

## 🎯 Goals

1. ✅ Create `DeathHandler` service for centralized death logic
2. ✅ Implement difficulty-based penalties (gold, XP, items)
3. ✅ Create item dropping system with location tracking
4. ✅ Implement Hall of Fame for permadeath characters
5. ✅ Add respawn logic and proper UI feedback
6. ✅ Integrate with existing combat and save systems
7. ✅ Update this artifact with completion status

---

## 📁 Files to Create

### 1. `Game/Models/HallOfFameEntry.cs` (NEW)

```csharp
namespace Game.Models;

/// <summary>
/// Records a character's final moments for the Hall of Fame.
/// Used primarily for permadeath mode but can track any character death.
/// </summary>
public class HallOfFameEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CharacterName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int PlayTimeMinutes { get; set; }
    public int TotalEnemiesDefeated { get; set; }
    public int QuestsCompleted { get; set; }
    public int DeathCount { get; set; }
    public string DeathReason { get; set; } = "Unknown";
    public string DeathLocation { get; set; } = "Unknown";
    public DateTime DeathDate { get; set; } = DateTime.Now;
    public int AchievementsUnlocked { get; set; }
    public bool IsPermadeath { get; set; }
    public string DifficultyLevel { get; set; } = "Normal";
    
    /// <summary>
    /// Calculate a "fame score" for ranking.
    /// </summary>
    public int GetFameScore()
    {
        var score = Level * 100;
        score += QuestsCompleted * 50;
        score += TotalEnemiesDefeated * 5;
        score += AchievementsUnlocked * 200;
        
        if (IsPermadeath)
            score *= 2; // Double points for permadeath runs
        
        return score;
    }
    
    /// <summary>
    /// Get formatted playtime string.
    /// </summary>
    public string GetPlaytimeFormatted()
    {
        var hours = PlayTimeMinutes / 60;
        var minutes = PlayTimeMinutes % 60;
        return $"{hours}h {minutes}m";
    }
}
```

---

### 2. `Game/Services/DeathHandler.cs` (NEW)

```csharp
using Game.Models;
using Game.UI;
using Serilog;

namespace Game.Services;

/// <summary>
/// Handles player death, penalties, respawning, and permadeath.
/// </summary>
public class DeathHandler
{
    private readonly SaveGameService _saveGameService;
    private readonly Random _random = new();
    
    public DeathHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    /// <summary>
    /// Handle player death with difficulty-appropriate penalties.
    /// </summary>
    public void HandleDeath(Character character, SaveGame saveGame, string deathLocation, Enemy? killer = null)
    {
        var difficulty = _saveGameService.GetDifficultySettings();
        
        Log.Warning("Player death at {Location}. Difficulty: {Difficulty}, Death count: {DeathCount}",
            deathLocation, difficulty.Name, saveGame.DeathCount + 1);
        
        // Record death in save
        _saveGameService.RecordDeath();
        
        // Handle based on difficulty
        if (difficulty.IsPermadeath)
        {
            HandlePermadeath(character, saveGame, deathLocation, killer);
        }
        else
        {
            HandleStandardDeath(character, saveGame, deathLocation, difficulty);
        }
    }
    
    /// <summary>
    /// Handle standard death with penalties and respawn.
    /// </summary>
    private void HandleStandardDeath(Character character, SaveGame saveGame, string deathLocation, DifficultySettings difficulty)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("           YOU HAVE DIED!              ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        Thread.Sleep(2000);
        
        // Calculate penalties
        var goldLost = (int)(character.Gold * difficulty.GoldLossPercentage);
        var xpLost = (int)(GetXPToNextLevel(character) * difficulty.XPLossPercentage);
        
        // Apply gold penalty
        character.Gold = Math.Max(0, character.Gold - goldLost);
        _saveGameService.RecordGoldSpent(goldLost);
        
        // Apply XP penalty (can't delevel)
        character.Experience = Math.Max(0, character.Experience - xpLost);
        
        // Handle item dropping
        var droppedItems = HandleItemDropping(character, saveGame, deathLocation, difficulty);
        
        // Respawn with full health
        character.Health = character.MaxHealth;
        character.Mana = character.MaxMana;
        
        // Show penalty summary
        ConsoleUI.Clear();
        ConsoleUI.ShowWarning("Death Penalties Applied:");
        ConsoleUI.WriteText($"  • Lost {goldLost} gold");
        ConsoleUI.WriteText($"  • Lost {xpLost} XP");
        
        if (droppedItems.Count > 0)
        {
            ConsoleUI.WriteText($"  • Dropped {droppedItems.Count} item(s) at {deathLocation}");
            foreach (var item in droppedItems)
            {
                ConsoleUI.WriteText($"    - {item.Name}");
            }
            ConsoleUI.ShowInfo("You can return to recover your items!");
        }
        
        ConsoleUI.ShowSuccess($"Respawning in Hub Town with full health...");
        
        // Auto-save for Ironman mode
        if (difficulty.AutoSaveOnly)
        {
            _saveGameService.AutoSave(saveGame);
            ConsoleUI.ShowInfo("(Auto-saved - cannot reload)");
        }
        
        Log.Information("Standard death handled. Gold lost: {GoldLost}, XP lost: {XpLost}, Items dropped: {ItemCount}",
            goldLost, xpLost, droppedItems.Count);
        
        Thread.Sleep(3000);
    }
    
    /// <summary>
    /// Handle item dropping based on difficulty settings.
    /// </summary>
    private List<Item> HandleItemDropping(Character character, SaveGame saveGame, string deathLocation, DifficultySettings difficulty)
    {
        var droppedItems = new List<Item>();
        
        if (difficulty.ItemsDroppedOnDeath == 0)
            return droppedItems; // Easy mode - no items dropped
        
        if (difficulty.DropAllInventoryOnDeath)
        {
            // Drop ALL inventory items (Hard, Expert, Ironman)
            droppedItems.AddRange(character.Inventory);
            character.Inventory.Clear();
        }
        else if (difficulty.ItemsDroppedOnDeath > 0 && character.Inventory.Count > 0)
        {
            // Drop specific number of random items (Normal)
            var itemsToDrop = Math.Min(difficulty.ItemsDroppedOnDeath, character.Inventory.Count);
            
            for (int i = 0; i < itemsToDrop; i++)
            {
                var randomIndex = _random.Next(character.Inventory.Count);
                var droppedItem = character.Inventory[randomIndex];
                character.Inventory.RemoveAt(randomIndex);
                droppedItems.Add(droppedItem);
            }
        }
        
        // Store dropped items in save game
        if (droppedItems.Count > 0)
        {
            if (!saveGame.DroppedItemsAtLocations.ContainsKey(deathLocation))
            {
                saveGame.DroppedItemsAtLocations[deathLocation] = new List<Item>();
            }
            
            saveGame.DroppedItemsAtLocations[deathLocation].AddRange(droppedItems);
        }
        
        return droppedItems;
    }
    
    /// <summary>
    /// Handle permadeath - delete save and record in Hall of Fame.
    /// </summary>
    private void HandlePermadeath(Character character, SaveGame saveGame, string deathLocation, Enemy? killer)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("      PERMADEATH: GAME OVER            ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        Thread.Sleep(3000);
        
        // Create Hall of Fame entry
        var hallOfFameEntry = new HallOfFameEntry
        {
            CharacterName = character.Name,
            ClassName = character.ClassName,
            Level = character.Level,
            PlayTimeMinutes = saveGame.PlayTimeMinutes,
            TotalEnemiesDefeated = saveGame.TotalEnemiesDefeated,
            QuestsCompleted = saveGame.QuestsCompleted,
            DeathCount = saveGame.DeathCount,
            DeathReason = killer != null ? $"Slain by {killer.Name}" : "Unknown cause",
            DeathLocation = deathLocation,
            DeathDate = DateTime.Now,
            AchievementsUnlocked = saveGame.UnlockedAchievements.Count,
            IsPermadeath = true,
            DifficultyLevel = saveGame.DifficultyLevel
        };
        
        // Add to Hall of Fame
        AddToHallOfFame(hallOfFameEntry);
        
        // Show final statistics
        ShowPermadeathStatistics(character, saveGame, hallOfFameEntry);
        
        // Delete save
        _saveGameService.DeleteSave(saveGame.Id);
        
        ConsoleUI.ShowError("Your save file has been deleted.");
        ConsoleUI.ShowInfo("Your legend will be remembered in the Hall of Fame.");
        
        Log.Warning("Permadeath handled. Character: {Character}, Level: {Level}, Fame Score: {Score}",
            character.Name, character.Level, hallOfFameEntry.GetFameScore());
        
        Thread.Sleep(5000);
    }
    
    /// <summary>
    /// Show detailed statistics for permadeath game over.
    /// </summary>
    private void ShowPermadeathStatistics(Character character, SaveGame saveGame, HallOfFameEntry entry)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner($"{character.Name}'s Final Stand", "Your Journey Ends");
        
        ConsoleUI.WriteText($"Class: {character.ClassName}");
        ConsoleUI.WriteText($"Level: {character.Level}");
        ConsoleUI.WriteText($"Playtime: {entry.GetPlaytimeFormatted()}");
        Console.WriteLine();
        
        ConsoleUI.WriteText("Final Statistics:");
        ConsoleUI.WriteText($"  • Enemies Defeated: {saveGame.TotalEnemiesDefeated}");
        ConsoleUI.WriteText($"  • Quests Completed: {saveGame.QuestsCompleted}");
        ConsoleUI.WriteText($"  • Achievements: {saveGame.UnlockedAchievements.Count}");
        ConsoleUI.WriteText($"  • Gold Earned: {saveGame.TotalGoldEarned}g");
        ConsoleUI.WriteText($"  • Locations Visited: {saveGame.VisitedLocations.Count}");
        Console.WriteLine();
        
        ConsoleUI.WriteText($"Cause of Death: {entry.DeathReason}");
        ConsoleUI.WriteText($"Location: {entry.DeathLocation}");
        Console.WriteLine();
        
        ConsoleUI.ShowSuccess($"Fame Score: {entry.GetFameScore():N0}");
        
        Thread.Sleep(5000);
    }
    
    /// <summary>
    /// Add entry to Hall of Fame database.
    /// </summary>
    private void AddToHallOfFame(HallOfFameEntry entry)
    {
        try
        {
            using var db = new LiteDB.LiteDatabase("halloffame.db");
            var collection = db.GetCollection<HallOfFameEntry>("heroes");
            collection.Insert(entry);
            
            Log.Information("Added {CharacterName} to Hall of Fame with score {Score}",
                entry.CharacterName, entry.GetFameScore());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add entry to Hall of Fame");
        }
    }
    
    /// <summary>
    /// Retrieve dropped items at a location.
    /// </summary>
    public List<Item> RetrieveDroppedItems(SaveGame saveGame, string location)
    {
        if (saveGame.DroppedItemsAtLocations.TryGetValue(location, out var items))
        {
            var retrievedItems = new List<Item>(items);
            saveGame.DroppedItemsAtLocations.Remove(location);
            
            ConsoleUI.ShowSuccess($"Retrieved {retrievedItems.Count} dropped item(s)!");
            foreach (var item in retrievedItems)
            {
                ConsoleUI.WriteText($"  • {item.Name}");
            }
            
            return retrievedItems;
        }
        
        return new List<Item>();
    }
    
    /// <summary>
    /// Check if there are dropped items at a location.
    /// </summary>
    public bool HasDroppedItems(SaveGame saveGame, string location)
    {
        return saveGame.DroppedItemsAtLocations.ContainsKey(location) &&
               saveGame.DroppedItemsAtLocations[location].Count > 0;
    }
    
    /// <summary>
    /// Get XP required for next level (helper method).
    /// </summary>
    private int GetXPToNextLevel(Character character)
    {
        return character.Level * 100; // Match Character.ExperienceForNextLevel()
    }
}
```

---

### 3. `Game/Services/HallOfFameService.cs` (NEW)

```csharp
using Game.Models;
using LiteDB;
using Serilog;

namespace Game.Services;

/// <summary>
/// Manages Hall of Fame entries and leaderboards.
/// </summary>
public class HallOfFameService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<HallOfFameEntry> _heroes;
    
    public HallOfFameService(string databasePath = "halloffame.db")
    {
        _db = new LiteDatabase(databasePath);
        _heroes = _db.GetCollection<HallOfFameEntry>("heroes");
        
        // Create index on fame score for faster queries
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
            Log.Information("Added {CharacterName} to Hall of Fame (Score: {Score})",
                entry.CharacterName, entry.GetFameScore());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add Hall of Fame entry");
            throw;
        }
    }
    
    /// <summary>
    /// Get all Hall of Fame entries sorted by fame score.
    /// </summary>
    public List<HallOfFameEntry> GetAllEntries(int limit = 100)
    {
        return _heroes.FindAll()
            .OrderByDescending(e => e.GetFameScore())
            .Take(limit)
            .ToList();
    }
    
    /// <summary>
    /// Get top entries by fame score.
    /// </summary>
    public List<HallOfFameEntry> GetTopHeroes(int count = 10)
    {
        return _heroes.FindAll()
            .OrderByDescending(e => e.GetFameScore())
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// Get permadeath-only entries.
    /// </summary>
    public List<HallOfFameEntry> GetPermadeathHeroes(int limit = 50)
    {
        return _heroes.Find(x => x.IsPermadeath)
            .OrderByDescending(e => e.GetFameScore())
            .Take(limit)
            .ToList();
    }
    
    /// <summary>
    /// Get entries by difficulty level.
    /// </summary>
    public List<HallOfFameEntry> GetByDifficulty(string difficulty, int limit = 50)
    {
        return _heroes.Find(x => x.DifficultyLevel == difficulty)
            .OrderByDescending(e => e.GetFameScore())
            .Take(limit)
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
        
        if (entries.Count == 0)
        {
            ConsoleUI.ShowWarning("No heroes yet. Be the first!");
            return;
        }
        
        var headers = new[] { "Rank", "Name", "Class", "Level", "Score", "Difficulty", "Death" };
        var rows = new List<string[]>();
        
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var deathMark = entry.IsPermadeath ? "☠" : "†";
            
            rows.Add(new[]
            {
                $"#{i + 1}",
                entry.CharacterName,
                entry.ClassName,
                entry.Level.ToString(),
                entry.GetFameScore().ToString("N0"),
                entry.DifficultyLevel,
                $"{deathMark} {entry.DeathReason.Substring(0, Math.Min(30, entry.DeathReason.Length))}"
            });
        }
        
        ConsoleUI.ShowTable("Top 20 Heroes", headers, rows);
        ConsoleUI.PressAnyKey();
    }
    
    public void Dispose()
    {
        _db?.Dispose();
    }
}
```

---

## 📝 Files to Modify

### 4. `Game/Services/CombatService.cs` - Integrate Death Handler

**ADD** field and constructor parameter:
```csharp
private readonly DeathHandler _deathHandler;

public CombatService(SaveGameService? saveGameService = null, DeathHandler? deathHandler = null)
{
    _saveGameService = saveGameService ?? new SaveGameService();
    _deathHandler = deathHandler ?? new DeathHandler(_saveGameService);
}
```

**Modify combat loop** where player health <= 0:

**FIND** (around line 150):
```csharp
if (player.Health <= 0)
{
    result.Victory = false;
    result.Message = "You have been defeated!";
    return result;
}
```

**REPLACE WITH**:
```csharp
if (player.Health <= 0)
{
    result.Victory = false;
    result.Message = "You have been defeated!";
    
    // Trigger death handler
    // Note: Location and SaveGame need to be passed to CombatService
    // This is a design decision - either pass them or handle in GameEngine
    // For now, return defeat and handle in GameEngine
    
    return result;
}
```

---

### 5. `Game/GameEngine.cs` - Integrate Death in Combat

**ADD** field:
```csharp
private DeathHandler? _deathHandler;
private string _currentLocation = "Hub Town";
```

**Initialize in constructor or StartAsync**:
```csharp
_deathHandler = new DeathHandler(_saveGameService);
```

**In combat handling** (wherever combat result is processed):

**FIND**:
```csharp
if (!combatResult.Victory)
{
    ConsoleUI.ShowError(combatResult.Message);
    // Player died - currently no handling
}
```

**REPLACE WITH**:
```csharp
if (!combatResult.Victory && _player.Health <= 0)
{
    // Player died - handle death
    _deathHandler?.HandleDeath(_player, _saveGameService.GetCurrentSave()!, _currentLocation, enemy);
    
    // Check if permadeath deleted save
    if (_saveGameService.GetCurrentSave() == null)
    {
        // Return to main menu
        _state = GameState.MainMenu;
        return;
    }
    
    // Respawn in hub town
    _currentLocation = "Hub Town";
}
```

---

### 6. `Game/GameEngine.cs` - Add Item Recovery

**ADD** method to check for dropped items when entering location:
```csharp
private void CheckForDroppedItems()
{
    var saveGame = _saveGameService.GetCurrentSave();
    if (saveGame == null || _deathHandler == null)
        return;
    
    if (_deathHandler.HasDroppedItems(saveGame, _currentLocation))
    {
        ConsoleUI.ShowWarning($"You see your dropped items here!");
        
        if (ConsoleUI.Confirm("Retrieve your items?"))
        {
            var items = _deathHandler.RetrieveDroppedItems(saveGame, _currentLocation);
            _player.Inventory.AddRange(items);
            
            // Save the recovery
            _saveGameService.AutoSave(saveGame);
        }
    }
}
```

**Call this when player enters a location**:
```csharp
// After player travels to location
_currentLocation = selectedLocation;
CheckForDroppedItems();
```

---

### 7. `Game/GameEngine.cs` - Add Hall of Fame Menu

**In Main Menu** add option:
```csharp
var menuOptions = new[]
{
    "New Game",
    "Load Game",
    "Hall of Fame", // ADD THIS
    "Settings",
    "Quit"
};
```

**Handle selection**:
```csharp
case 2: // Hall of Fame
    using (var hallOfFame = new HallOfFameService())
    {
        hallOfFame.DisplayHallOfFame();
    }
    break;
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Normal Mode Death**:
   - [ ] Create Normal character
   - [ ] Die in combat
   - [ ] Verify lost 10% gold
   - [ ] Verify lost 25% XP (didn't delevel)
   - [ ] Verify dropped 1 item
   - [ ] Return to death location
   - [ ] Retrieve dropped item
   - [ ] Verify respawned in Hub Town with full health

2. **Ironman Mode Death**:
   - [ ] Create Ironman character
   - [ ] Accumulate gold and items
   - [ ] Die in combat
   - [ ] Verify lost 25% gold
   - [ ] Verify lost 50% XP
   - [ ] Verify dropped ALL inventory items
   - [ ] Verify auto-saved (cannot reload)
   - [ ] Return to death location
   - [ ] Retrieve all items

3. **Permadeath Mode**:
   - [ ] Create Permadeath character
   - [ ] Play for a while (get stats)
   - [ ] Die in combat
   - [ ] Verify save deleted
   - [ ] Verify Hall of Fame entry created
   - [ ] View Hall of Fame
   - [ ] Verify character appears with correct stats

4. **Easy Mode**:
   - [ ] Create Easy character
   - [ ] Die in combat
   - [ ] Verify lost only 5% gold, 10% XP
   - [ ] Verify NO items dropped
   - [ ] Verify respawned normally

5. **Hall of Fame**:
   - [ ] Create multiple permadeath characters
   - [ ] Die with different levels/stats
   - [ ] View Hall of Fame
   - [ ] Verify sorted by fame score
   - [ ] Verify all stats display correctly

### Edge Cases

- [ ] Die with 0 gold (no penalty error)
- [ ] Die at level 1 with 0 XP (no negative XP)
- [ ] Die with empty inventory (no item drop error)
- [ ] Die in Ironman with full inventory
- [ ] Multiple deaths in same location (items stack?)

---

## ✅ Completion Checklist

- [ ] Created `HallOfFameEntry.cs` model
- [ ] Created `DeathHandler.cs` service
- [ ] Created `HallOfFameService.cs` service
- [ ] Added death handling to `CombatService`
- [ ] Integrated death in `GameEngine` combat flow
- [ ] Added item dropping and recovery system
- [ ] Added Hall of Fame display to main menu
- [ ] Tested all difficulty death penalties
- [ ] Tested permadeath with Hall of Fame
- [ ] Tested item recovery system
- [ ] Verified Ironman auto-save on death
- [ ] Built successfully with no errors
- [ ] All existing tests still pass

---

## 📊 Completion Status

**Completed**: [Date]  
**Time Taken**: [Duration]  
**Build Status**: ⚪ Not Built  
**Test Results**: ⚪ Not Tested

### Issues Encountered
```
[List any issues or deviations from the plan]
```

### Notes
```
[Any additional notes about death handling, balancing, etc.]
```

---

## 🔗 Navigation

- **Previous Phase**: [Phase 1: Difficulty Foundation](./PHASE_1_DIFFICULTY_FOUNDATION.md)
- **Current Phase**: Phase 2 - Death System
- **Next Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)
- **See Also**: [Phase 4: End-Game](./PHASE_4_ENDGAME.md)

---

**Ready to implement? Copy this entire artifact into chat to begin Phase 2!** 🚀
