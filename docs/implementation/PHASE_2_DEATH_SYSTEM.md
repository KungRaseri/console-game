# Phase 2: Death & Penalty System

**Status**: ⚪ Ready to Start  
**Prerequisites**: ✅ Phase 1 complete (Difficulty System) + CQRS/Vertical Slice Architecture  
**Estimated Time**: 3-4 hours  
**Previous Phase**: [Phase 1: Difficulty Foundation](./PHASE_1_DIFFICULTY_FOUNDATION.md)  
**Next Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)  
**Related**: [Phase 4: End-Game](./PHASE_4_ENDGAME.md)

---

## 📋 Overview

Implement comprehensive death handling with difficulty-specific penalties, item dropping, respawning, Hall of Fame for permadeath, and proper save integration using **CQRS + Vertical Slice Architecture**.

**✅ Pre-Phase Foundation Complete:**

- Location tracking system exists (can be accessed via GameStateService)
- SaveGameService has basic save management in `Features/SaveLoad/`
- GameStateService provides centralized access to game state
- SaveGame model has DroppedItemsAtLocations dictionary ready
- CQRS infrastructure with MediatR ready for commands/queries

---

## 🎯 Goals

1. ✅ Create `Death` feature with Commands and Services
2. ✅ Implement difficulty-based penalties (gold, XP, items)
3. ✅ Create item dropping system with location tracking
4. ✅ Implement Hall of Fame for permadeath characters
5. ✅ Add respawn logic and proper UI feedback
6. ✅ Integrate with Combat feature using MediatR commands
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

### 2. `Game/Features/Death/` (NEW FEATURE FOLDER)

Create this folder structure:

```
Game/Features/Death/
├── Commands/
│   ├── HandlePlayerDeathCommand.cs
│   └── HandlePlayerDeathHandler.cs
├── Queries/
│   ├── GetDroppedItemsQuery.cs
│   └── GetDroppedItemsHandler.cs
├── DeathService.cs
└── HallOfFameService.cs
```

---

### 3. `Game/Features/Death/Commands/HandlePlayerDeathCommand.cs` (NEW)

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Death.Commands;

/// <summary>
/// Command to handle player death with appropriate penalties.
/// </summary>
public record HandlePlayerDeathCommand : IRequest<HandlePlayerDeathResult>
{
    public required Character Player { get; init; }
    public required string DeathLocation { get; init; }
    public Enemy? Killer { get; init; }
}

/// <summary>
/// Result of player death handling.
/// </summary>
public record HandlePlayerDeathResult
{
    public required bool IsPermadeath { get; init; }
    public required bool SaveDeleted { get; init; }
    public List<Item> DroppedItems { get; init; } = new();
    public int GoldLost { get; init; }
    public int XPLost { get; init; }
    public string? HallOfFameId { get; init; }
}
```

---

### 4. `Game/Features/Death/Commands/HandlePlayerDeathHandler.cs` (NEW)

```csharp
using Game.Features.Death;
using Game.Features.SaveLoad;
using Game.Models;
using Game.Shared.UI;
using MediatR;
using Serilog;

namespace Game.Features.Death.Commands;

/// <summary>
/// Handles player death with difficulty-appropriate penalties.
/// </summary>
public class HandlePlayerDeathHandler : IRequestHandler<HandlePlayerDeathCommand, HandlePlayerDeathResult>
{
    private readonly DeathService _deathService;
    private readonly SaveGameService _saveGameService;
    private readonly HallOfFameService _hallOfFameService;
    
    public HandlePlayerDeathHandler(
        DeathService deathService,
        SaveGameService saveGameService,
        HallOfFameService hallOfFameService)
    {
        _deathService = deathService;
        _saveGameService = saveGameService;
        _hallOfFameService = hallOfFameService;
    }
    
    public async Task<HandlePlayerDeathResult> Handle(HandlePlayerDeathCommand request, CancellationToken ct)
    {
        var player = request.Player;
        var location = request.DeathLocation;
        var killer = request.Killer;
        var saveGame = _saveGameService.GetCurrentSave();
        
        if (saveGame == null)
        {
            Log.Error("No active save game found during death handling");
            return new HandlePlayerDeathResult
            {
                IsPermadeath = false,
                SaveDeleted = false
            };
        }
        
        var difficulty = _saveGameService.GetDifficultySettings();
        
        Log.Warning("Player death at {Location}. Difficulty: {Difficulty}, Death count: {DeathCount}",
            location, difficulty.Name, saveGame.DeathCount + 1);
        
        // Record death in save
        saveGame.DeathCount++;
        saveGame.LastDeathLocation = location;
        saveGame.LastDeathDate = DateTime.Now;
        
        // Handle based on difficulty
        if (difficulty.IsPermadeath)
        {
            return await HandlePermadeathAsync(player, saveGame, location, killer);
        }
        else
        {
            return await HandleStandardDeathAsync(player, saveGame, location, difficulty);
        }
    }
    
    private async Task<HandlePlayerDeathResult> HandleStandardDeathAsync(
        Character player, SaveGame saveGame, string location, DifficultySettings difficulty)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("           YOU HAVE DIED               ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);
        
        Console.WriteLine();
        ConsoleUI.WriteText($"You died at: {location}");
        ConsoleUI.WriteText($"Death count: {saveGame.DeathCount}");
        Console.WriteLine();
        
        // Calculate penalties
        var goldLost = (int)(player.Gold * difficulty.GoldLossPercentage);
        var xpLost = (int)(player.Experience * difficulty.XPLossPercentage);
        
        // Apply penalties
        player.Gold = Math.Max(0, player.Gold - goldLost);
        player.Experience = Math.Max(0, player.Experience - xpLost);
        
        // Handle item dropping
        var droppedItems = _deathService.HandleItemDropping(
            player, saveGame, location, difficulty);
        
        // Show penalties
        ConsoleUI.ShowError($"Penalties:");
        if (goldLost > 0)
            ConsoleUI.WriteText($"  • Lost {goldLost} gold");
        if (xpLost > 0)
            ConsoleUI.WriteText($"  • Lost {xpLost} XP");
        if (droppedItems.Count > 0)
        {
            if (difficulty.DropAllInventoryOnDeath)
                ConsoleUI.WriteText($"  • Dropped ALL {droppedItems.Count} items at {location}");
            else
                ConsoleUI.WriteText($"  • Dropped {droppedItems.Count} item(s) at {location}");
        }
        
        Console.WriteLine();
        
        // Respawn
        player.Health = player.MaxHealth;
        player.Mana = player.MaxMana;
        
        ConsoleUI.ShowSuccess("You have respawned at Hub Town with full health!");
        ConsoleUI.ShowInfo("Return to your death location to recover dropped items.");
        
        await Task.Delay(3000);
        
        // Auto-save in Ironman mode
        if (difficulty.AutoSaveOnly)
        {
            _saveGameService.SaveGame(saveGame);
            ConsoleUI.ShowInfo("Game auto-saved (Ironman mode)");
            await Task.Delay(1000);
        }
        
        return new HandlePlayerDeathResult
        {
            IsPermadeath = false,
            SaveDeleted = false,
            DroppedItems = droppedItems,
            GoldLost = goldLost,
            XPLost = xpLost
        };
    }
    
    private async Task<HandlePlayerDeathResult> HandlePermadeathAsync(
        Character player, SaveGame saveGame, string location, Enemy? killer)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowError("═══════════════════════════════════════");
        ConsoleUI.ShowError("          PERMADEATH                   ");
        ConsoleUI.ShowError("═══════════════════════════════════════");
        await Task.Delay(2000);
        
        Console.WriteLine();
        ConsoleUI.ShowError($"{player.Name} has fallen.");
        ConsoleUI.WriteText($"Location: {location}");
        if (killer != null)
            ConsoleUI.WriteText($"Slain by: {killer.Name}");
        Console.WriteLine();
        
        await Task.Delay(2000);
        
        // Create Hall of Fame entry
        var entry = new HallOfFameEntry
        {
            CharacterName = player.Name,
            ClassName = player.ClassName,
            Level = player.Level,
            PlayTimeMinutes = saveGame.PlayTimeMinutes,
            TotalEnemiesDefeated = saveGame.EnemiesDefeated,
            QuestsCompleted = saveGame.QuestsCompleted,
            DeathCount = saveGame.DeathCount,
            DeathReason = killer != null ? $"Slain by {killer.Name}" : "Unknown cause",
            DeathLocation = location,
            DeathDate = DateTime.Now,
            AchievementsUnlocked = saveGame.UnlockedAchievements.Count,
            IsPermadeath = true,
            DifficultyLevel = saveGame.DifficultyLevel
        };
        
        _hallOfFameService.AddEntry(entry);
        
        // Show final statistics
        ShowPermadeathStatistics(player, saveGame, entry);
        
        await Task.Delay(5000);
        
        // Delete save
        _saveGameService.DeleteSave(saveGame.Id);
        
        ConsoleUI.ShowError("Your save file has been deleted.");
        ConsoleUI.ShowInfo($"Your legacy lives on in the Hall of Fame (Score: {entry.GetFameScore()})");
        
        await Task.Delay(3000);
        
        return new HandlePlayerDeathResult
        {
            IsPermadeath = true,
            SaveDeleted = true,
            HallOfFameId = entry.Id
        };
    }
    
    private void ShowPermadeathStatistics(Character player, SaveGame saveGame, HallOfFameEntry entry)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner($"{player.Name}'s Legacy", "Final Statistics");
        Console.WriteLine();
        
        ConsoleUI.WriteText($"Class: {player.ClassName}");
        ConsoleUI.WriteText($"Level: {player.Level}");
        ConsoleUI.WriteText($"Playtime: {entry.GetPlaytimeFormatted()}");
        ConsoleUI.WriteText($"Enemies Defeated: {saveGame.EnemiesDefeated}");
        ConsoleUI.WriteText($"Quests Completed: {saveGame.QuestsCompleted}");
        ConsoleUI.WriteText($"Locations Discovered: {saveGame.DiscoveredLocations.Count}");
        ConsoleUI.WriteText($"Achievements Unlocked: {saveGame.UnlockedAchievements.Count}");
        Console.WriteLine();
        ConsoleUI.WriteText($"Fame Score: {entry.GetFameScore()}");
    }
}
```

---

### 5. `Game/Features/Death/Queries/GetDroppedItemsQuery.cs` (NEW)

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Death.Queries;

/// <summary>
/// Query to get dropped items at a specific location.
/// </summary>
public record GetDroppedItemsQuery : IRequest<GetDroppedItemsResult>
{
    public required string Location { get; init; }
}

/// <summary>
/// Result containing dropped items at the location.
/// </summary>
public record GetDroppedItemsResult
{
    public List<Item> Items { get; init; } = new();
    public bool HasItems => Items.Count > 0;
}
```

---

### 6. `Game/Features/Death/Queries/GetDroppedItemsHandler.cs` (NEW)

```csharp
using Game.Features.SaveLoad;
using MediatR;

namespace Game.Features.Death.Queries;

/// <summary>
/// Handles retrieval of dropped items at a location.
/// </summary>
public class GetDroppedItemsHandler : IRequestHandler<GetDroppedItemsQuery, GetDroppedItemsResult>
{
    private readonly SaveGameService _saveGameService;
    
    public GetDroppedItemsHandler(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<GetDroppedItemsResult> Handle(GetDroppedItemsQuery request, CancellationToken ct)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
        {
            return new GetDroppedItemsResult();
        }
        
        if (saveGame.DroppedItemsAtLocations.TryGetValue(request.Location, out var items))
        {
            return new GetDroppedItemsResult { Items = items };
        }
        
        return new GetDroppedItemsResult();
    }
}
```

---

### 7. `Game/Features/Death/DeathService.cs` (NEW)

```csharp
using Game.Features.SaveLoad;
using Game.Models;
using Serilog;

namespace Game.Features.Death;

/// <summary>
/// Service for death-related operations (penalties, item dropping, etc.).
/// </summary>
public class DeathService
{
    private readonly Random _random = new();
    
    /// <summary>
    /// Handle item dropping based on difficulty settings.
    /// </summary>
    public List<Item> HandleItemDropping(
        Character player,
        SaveGame saveGame,
        string location,
        DifficultySettings difficulty)
    {
        var droppedItems = new List<Item>();
        
        // Early exit if no items to drop
        if (difficulty.ItemsDroppedOnDeath == 0 && !difficulty.DropAllInventoryOnDeath)
        {
            return droppedItems;
        }
        
        // Drop all inventory
        if (difficulty.DropAllInventoryOnDeath)
        {
            droppedItems.AddRange(player.Inventory);
            player.Inventory.Clear();
            
            Log.Information("Dropped all {Count} items at {Location}", droppedItems.Count, location);
        }
        // Drop random items
        else
        {
            var itemsToDrop = Math.Min(difficulty.ItemsDroppedOnDeath, player.Inventory.Count);
            
            for (int i = 0; i < itemsToDrop; i++)
            {
                if (player.Inventory.Count == 0) break;
                
                var randomIndex = _random.Next(player.Inventory.Count);
                var item = player.Inventory[randomIndex];
                
                droppedItems.Add(item);
                player.Inventory.RemoveAt(randomIndex);
            }
            
            Log.Information("Dropped {Count} random items at {Location}", droppedItems.Count, location);
        }
        
        // Store dropped items in save game
        if (droppedItems.Count > 0)
        {
            if (!saveGame.DroppedItemsAtLocations.ContainsKey(location))
            {
                saveGame.DroppedItemsAtLocations[location] = new List<Item>();
            }
            
            saveGame.DroppedItemsAtLocations[location].AddRange(droppedItems);
        }
        
        return droppedItems;
    }
    
    /// <summary>
    /// Retrieve dropped items from a location.
    /// </summary>
    public List<Item> RetrieveDroppedItems(SaveGame saveGame, string location)
    {
        if (saveGame.DroppedItemsAtLocations.TryGetValue(location, out var items))
        {
            saveGame.DroppedItemsAtLocations.Remove(location);
            return items;
        }
        
        return new List<Item>();
    }
}
```

---

### 8. `Game/Features/Death/HallOfFameService.cs` (NEW)

```csharp
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
```

---

## 📝 Files to Modify

### 9. `Game/Program.cs` - Register Death Services

**FIND** the service registration section (around line 30-40):

```csharp
// Register services
services.AddSingleton<SaveGameService>();
services.AddSingleton<CombatService>();
// ... other services ...
```

**ADD** these registrations:

```csharp
// Death system services
services.AddSingleton<DeathService>();
services.AddSingleton<HallOfFameService>();
```

---

### 10. `Game/Features/Combat/Commands/AttackEnemy/AttackEnemyHandler.cs` - Integrate Death

**ADD** using statement at top:

```csharp
using Game.Features.Death.Commands;
```

**FIND** where the player dies in combat (look for `if (player.Health <= 0)`):

**REPLACE** the death handling with this:

```csharp
if (player.Health <= 0)
{
    // Player died - send death command
    var deathResult = await _mediator.Send(new HandlePlayerDeathCommand
    {
        Player = player,
        DeathLocation = "Combat Area", // TODO: Get actual location from GameStateService
        Killer = enemy
    }, cancellationToken);
    
    return new AttackEnemyResult
    {
        Success = false,
        Message = deathResult.IsPermadeath 
            ? "Permadeath - Save deleted" 
            : "You have been defeated and respawned",
        Victory = false,
        PlayerDied = true,
        WasPermadeath = deathResult.IsPermadeath
    };
}
```

**ALSO ADD** properties to `AttackEnemyResult` (in AttackEnemyCommand.cs):

```csharp
// Add to the record definition
public bool PlayerDied { get; init; }
public bool WasPermadeath { get; init; }
```

---

### 11. `Game/GameEngine.cs` - Handle Death Result and Item Recovery

**ADD** using statements at top:

```csharp
using Game.Features.Death.Commands;
using Game.Features.Death.Queries;
```

**ADD** field for HallOfFameService:

```csharp
private readonly HallOfFameService _hallOfFameService;
```

**UPDATE** constructor to inject it:

```csharp
public GameEngine(IMediator mediator, SaveGameService saveGameService, HallOfFameService hallOfFameService, ...)
{
    // ... existing code ...
    _hallOfFameService = hallOfFameService;
}
```

**ADD** method to check for dropped items when entering a location:

```csharp
private async Task CheckForDroppedItemsAsync(string location)
{
    var result = await _mediator.Send(new GetDroppedItemsQuery { Location = location });
    
    if (result.HasItems)
    {
        ConsoleUI.ShowWarning($"⚠️  You see your dropped items here! ({result.Items.Count} items)");
        
        if (ConsoleUI.Confirm("Retrieve your items?"))
        {
            // Recover items
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame != null)
            {
                _player.Inventory.AddRange(result.Items);
                saveGame.DroppedItemsAtLocations.Remove(location);
                
                ConsoleUI.ShowSuccess($"Recovered {result.Items.Count} items!");
                await Task.Delay(1500);
            }
        }
    }
}
```

**CALL** this method when player enters/travels to a location (in exploration logic):

```csharp
// After changing location
_currentLocation = newLocation;
await CheckForDroppedItemsAsync(_currentLocation);
```

**ADD** Hall of Fame to main menu. FIND the main menu options:

```csharp
var choice = ConsoleUI.ShowMenu("Main Menu", new[]
{
    "New Game",
    "Load Game",
    "Settings",
    "Exit"
});
```

**REPLACE WITH**:

```csharp
var choice = ConsoleUI.ShowMenu("Main Menu", new[]
{
    "New Game",
    "Load Game",
    "Hall of Fame",
    "Settings",
    "Exit"
});
```

**THEN UPDATE** the switch/if statement handling menu choices to add Hall of Fame case (insert before Settings):

```csharp
case 2: // Hall of Fame
    _hallOfFameService.DisplayHallOfFame();
    break;
case 3: // Settings (was case 2)
    // ... settings code ...
    break;
case 4: // Exit (was case 3)
    // ... exit code ...
    break;
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Normal Mode Death**:
   - [ ] Create Normal character
   - [ ] Die in combat
   - [ ] Verify lost ~10% gold
   - [ ] Verify lost ~25% XP (didn't delevel)
   - [ ] Verify dropped 1 item
   - [ ] Travel to death location
   - [ ] Successfully retrieve dropped item
   - [ ] Verify respawned with full health

2. **Ironman Mode Death**:
   - [ ] Create Ironman character
   - [ ] Accumulate gold and items
   - [ ] Die in combat
   - [ ] Verify auto-save occurred
   - [ ] Verify cannot reload previous save
   - [ ] Verify dropped all items
   - [ ] Return to death location
   - [ ] Retrieve all items

3. **Permadeath Mode**:
   - [ ] Create Permadeath character
   - [ ] Play for a while (get meaningful stats)
   - [ ] Die in combat
   - [ ] Verify save file deleted
   - [ ] Verify Hall of Fame entry created
   - [ ] View Hall of Fame from main menu
   - [ ] Verify character appears with correct stats

4. **Easy Mode**:
   - [ ] Create Easy character
   - [ ] Die in combat
   - [ ] Verify lost only ~5% gold, ~10% XP
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
- [ ] Die in Ironman with full inventory (all dropped)
- [ ] Multiple deaths in same location (items stack correctly)
- [ ] Die, retrieve items, die again at same location

---

## ✅ Completion Checklist

- [ ] Created `HallOfFameEntry.cs` model
- [ ] Created `Features/Death/` folder structure
- [ ] Created `HandlePlayerDeathCommand` and handler
- [ ] Created `GetDroppedItemsQuery` and handler
- [ ] Created `DeathService.cs`
- [ ] Created `HallOfFameService.cs`
- [ ] Registered services in `Program.cs`
- [ ] Integrated death command in `AttackEnemyHandler`
- [ ] Added item recovery in `GameEngine`
- [ ] Added Hall of Fame to main menu
- [ ] Tested all difficulty death penalties
- [ ] Tested permadeath with Hall of Fame
- [ ] Tested item recovery system
- [ ] Verified Ironman auto-save on death
- [ ] Built successfully with `dotnet build`
- [ ] All existing tests still pass (`dotnet test`)

---

## 📊 Completion Status

**Completed**: [Date]  
**Time Taken**: [Duration]  
**Build Status**: ⚪ Not Built  
**Test Results**: ⚪ Not Tested

### Issues Encountered

```text
[List any issues or deviations from the plan]
```

### Notes

```text
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
