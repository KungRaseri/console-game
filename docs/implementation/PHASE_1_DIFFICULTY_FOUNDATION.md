# Phase 1: Difficulty System Foundation

**Status**: � Ready to Start  
**Prerequisites**: ✅ All pre-phase improvements complete  
**Estimated Time**: 2-3 hours  
**Next Phase**: [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)  
**Related Phases**: [Phase 3](./PHASE_3_APOCALYPSE_MODE.md) | [Phase 4](./PHASE_4_ENDGAME.md)

---

## 📋 Overview

Create the foundational difficulty system with 7 difficulty modes: Easy, Normal, Hard, Expert, Ironman, Permadeath, and Apocalypse. This phase establishes the data models and applies combat multipliers.

**✅ Pre-Phase Foundation Complete:**
- CombatService is now instance-based with SaveGameService injection
- Enemy scaling hook (InitializeCombat) already exists
- GameStateService provides centralized difficulty access
- All infrastructure ready for multiplier implementation

---

## 🎯 Goals

1. ✅ Create `DifficultySettings` model with all 7 modes
2. ✅ Add difficulty selection to character creation flow
3. ✅ Update `SaveGame` to store difficulty settings
4. ✅ Apply difficulty multipliers to combat calculations
5. ✅ Test all difficulty modes in combat
6. ✅ Update this artifact with completion status

---

## 📁 Files to Create

### 1. `Game/Models/DifficultySettings.cs` (NEW)

```csharp
namespace Game.Models;

/// <summary>
/// Defines difficulty settings and modifiers for different game modes.
/// </summary>
public class DifficultySettings
{
    public string Name { get; set; } = "Normal";
    public string Description { get; set; } = string.Empty;
    
    // Combat modifiers
    public double PlayerDamageMultiplier { get; set; } = 1.0;
    public double EnemyDamageMultiplier { get; set; } = 1.0;
    public double EnemyHealthMultiplier { get; set; } = 1.0;
    public double GoldXPMultiplier { get; set; } = 1.0;
    
    // Save system behavior
    public bool AutoSaveOnly { get; set; } = false;
    public bool IsPermadeath { get; set; } = false;
    public bool IsApocalypse { get; set; } = false;
    public int ApocalypseTimeLimitMinutes { get; set; } = 240; // 4 hours default
    
    // Death penalties (used in Phase 2)
    public double GoldLossPercentage { get; set; } = 0.10; // 10%
    public double XPLossPercentage { get; set; } = 0.25; // 25%
    public bool DropAllInventoryOnDeath { get; set; } = false;
    public int ItemsDroppedOnDeath { get; set; } = 1;
    
    /// <summary>
    /// Easy mode - Story-focused with reduced challenge.
    /// </summary>
    public static DifficultySettings Easy => new()
    {
        Name = "Easy",
        Description = "Story Mode - Experience the adventure with reduced challenge",
        PlayerDamageMultiplier = 1.5,
        EnemyDamageMultiplier = 0.75,
        EnemyHealthMultiplier = 0.75,
        GoldXPMultiplier = 1.5,
        GoldLossPercentage = 0.05,
        XPLossPercentage = 0.10,
        ItemsDroppedOnDeath = 0
    };
    
    /// <summary>
    /// Normal mode - Balanced, intended experience (default).
    /// </summary>
    public static DifficultySettings Normal => new()
    {
        Name = "Normal",
        Description = "Balanced Experience - The intended gameplay (Recommended)",
        PlayerDamageMultiplier = 1.0,
        EnemyDamageMultiplier = 1.0,
        EnemyHealthMultiplier = 1.0,
        GoldXPMultiplier = 1.0,
        GoldLossPercentage = 0.10,
        XPLossPercentage = 0.25,
        ItemsDroppedOnDeath = 1
    };
    
    /// <summary>
    /// Hard mode - For experienced players seeking challenge.
    /// </summary>
    public static DifficultySettings Hard => new()
    {
        Name = "Hard",
        Description = "Experienced Players - Tactical combat, meaningful penalties",
        PlayerDamageMultiplier = 0.75,
        EnemyDamageMultiplier = 1.25,
        EnemyHealthMultiplier = 1.25,
        GoldXPMultiplier = 1.0,
        GoldLossPercentage = 0.20,
        XPLossPercentage = 0.50,
        DropAllInventoryOnDeath = true
    };
    
    /// <summary>
    /// Expert mode - Brutal challenge for masters.
    /// </summary>
    public static DifficultySettings Expert => new()
    {
        Name = "Expert",
        Description = "Brutal Challenge - For veterans only, very punishing",
        PlayerDamageMultiplier = 0.50,
        EnemyDamageMultiplier = 1.50,
        EnemyHealthMultiplier = 1.50,
        GoldXPMultiplier = 1.0,
        GoldLossPercentage = 0.30,
        XPLossPercentage = 0.75,
        DropAllInventoryOnDeath = true
    };
    
    /// <summary>
    /// Ironman mode - No save scumming, every choice permanent.
    /// </summary>
    public static DifficultySettings Ironman => new()
    {
        Name = "Ironman",
        Description = "No Reloading Saves - Every choice is permanent",
        PlayerDamageMultiplier = 0.75,
        EnemyDamageMultiplier = 1.25,
        EnemyHealthMultiplier = 1.25,
        GoldXPMultiplier = 1.0,
        AutoSaveOnly = true,
        GoldLossPercentage = 0.25,
        XPLossPercentage = 0.50,
        DropAllInventoryOnDeath = true
    };
    
    /// <summary>
    /// Permadeath mode - Death deletes save permanently.
    /// </summary>
    public static DifficultySettings Permadeath => new()
    {
        Name = "Permadeath",
        Description = "Death Deletes Save - One life. One chance. Hall of Fame glory.",
        PlayerDamageMultiplier = 0.50,
        EnemyDamageMultiplier = 1.50,
        EnemyHealthMultiplier = 1.50,
        GoldXPMultiplier = 1.0,
        AutoSaveOnly = true,
        IsPermadeath = true,
        // Death penalties don't matter - save is deleted
        GoldLossPercentage = 1.0,
        XPLossPercentage = 1.0,
        DropAllInventoryOnDeath = true
    };
    
    /// <summary>
    /// Apocalypse mode - 4-hour speed run challenge.
    /// </summary>
    public static DifficultySettings Apocalypse => new()
    {
        Name = "Apocalypse",
        Description = "4-Hour Speed Run - Race against time to save the world",
        PlayerDamageMultiplier = 1.0,
        EnemyDamageMultiplier = 1.0,
        EnemyHealthMultiplier = 1.0,
        GoldXPMultiplier = 1.0,
        IsApocalypse = true,
        ApocalypseTimeLimitMinutes = 240,
        GoldLossPercentage = 0.10,
        XPLossPercentage = 0.25,
        ItemsDroppedOnDeath = 1
    };
    
    /// <summary>
    /// Get difficulty settings by name.
    /// </summary>
    public static DifficultySettings GetByName(string name)
    {
        return name switch
        {
            "Easy" => Easy,
            "Normal" => Normal,
            "Hard" => Hard,
            "Expert" => Expert,
            "Ironman" => Ironman,
            "Permadeath" => Permadeath,
            "Apocalypse" => Apocalypse,
            _ => Normal
        };
    }
    
    /// <summary>
    /// Get all available difficulty options.
    /// </summary>
    public static List<DifficultySettings> GetAll()
    {
        return new List<DifficultySettings>
        {
            Easy,
            Normal,
            Hard,
            Expert,
            Ironman,
            Permadeath,
            Apocalypse
        };
    }
}
```

---

## 📝 Files to Modify

### 2. `Game/Models/SaveGame.cs` - Add Difficulty Fields

**Location**: Add after `DifficultyLevel` property (around line 60)

```csharp
// Existing:
public string DifficultyLevel { get; set; } = "Normal";
public bool IronmanMode { get; set; } = false;

// ADD THESE:
public bool PermadeathMode { get; set; } = false;
public bool ApocalypseMode { get; set; } = false;
public DateTime? ApocalypseStartTime { get; set; }
public int ApocalypseBonusMinutes { get; set; } = 0;

// For tracking dropped items on death (Phase 2)
public Dictionary<string, List<Item>> DroppedItemsAtLocations { get; set; } = new();
```

---

### 3. `Game/Services/CharacterCreationService.cs` - Add Difficulty Selection

**Location**: Add new method after `CreateCharacter`

```csharp
/// <summary>
/// Show difficulty selection menu and return chosen difficulty.
/// </summary>
public static DifficultySettings SelectDifficulty()
{
    ConsoleUI.Clear();
    ConsoleUI.ShowBanner("Select Difficulty", "Choose your challenge level");
    
    var difficulties = DifficultySettings.GetAll();
    var options = difficulties.Select((d, i) => 
        $"[{i + 1}] {d.Name,-15} - {d.Description}"
    ).ToArray();
    
    var choice = ConsoleUI.ShowMenu("Select difficulty level:", options);
    var selected = difficulties[choice];
    
    // Show confirmation for challenging modes
    if (selected.Name is "Ironman" or "Permadeath" or "Apocalypse")
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowWarning($"You selected: {selected.Name.ToUpper()}");
        Console.WriteLine();
        ConsoleUI.WriteText("This mode features:");
        
        if (selected.AutoSaveOnly)
            ConsoleUI.WriteText("  • Auto-save after every action");
        if (selected.IsPermadeath)
            ConsoleUI.WriteText("  • Death permanently deletes your save");
        if (selected.IsApocalypse)
            ConsoleUI.WriteText($"  • {selected.ApocalypseTimeLimitMinutes / 60}-hour time limit to complete main story");
        if (selected.Name == "Ironman")
            ConsoleUI.WriteText("  • Cannot reload previous saves");
        
        Console.WriteLine();
        var confirm = ConsoleUI.Confirm($"Are you sure you want {selected.Name} mode?");
        
        if (!confirm)
        {
            ConsoleUI.ShowWarning("Returning to difficulty selection...");
            Thread.Sleep(1000);
            return SelectDifficulty(); // Recursive call
        }
    }
    
    ConsoleUI.ShowSuccess($"Difficulty set to: {selected.Name}");
    Thread.Sleep(1000);
    
    return selected;
}
```

---

### 4. `Game/Services/SaveGameService.cs` - Update CreateNewGame

**Location**: Modify existing `CreateNewGame` method (around line 23)

**OLD**:
```csharp
public SaveGame CreateNewGame(Character player, string difficultyLevel = "Normal", bool ironmanMode = false)
{
    _currentSave = new SaveGame
    {
        PlayerName = player.Name,
        Character = player,
        CreationDate = DateTime.Now,
        SaveDate = DateTime.Now,
        DifficultyLevel = difficultyLevel,
        IronmanMode = ironmanMode,
        PlayTimeMinutes = 0
    };
```

**NEW**:
```csharp
public SaveGame CreateNewGame(Character player, DifficultySettings difficulty)
{
    _currentSave = new SaveGame
    {
        PlayerName = player.Name,
        Character = player,
        CreationDate = DateTime.Now,
        SaveDate = DateTime.Now,
        DifficultyLevel = difficulty.Name,
        IronmanMode = difficulty.AutoSaveOnly,
        PermadeathMode = difficulty.IsPermadeath,
        ApocalypseMode = difficulty.IsApocalypse,
        ApocalypseStartTime = difficulty.IsApocalypse ? DateTime.Now : null,
        ApocalypseBonusMinutes = 0,
        PlayTimeMinutes = 0
    };
```

**ALSO ADD** helper method:
```csharp
/// <summary>
/// Get difficulty settings from current save.
/// </summary>
public DifficultySettings GetDifficultySettings()
{
    if (_currentSave == null)
        return DifficultySettings.Normal;
    
    return DifficultySettings.GetByName(_currentSave.DifficultyLevel);
}
```

---

### 5. `Game/Services/CombatService.cs` - Apply Difficulty Multipliers

**✅ Pre-Phase Note**: CombatService is already an instance class with SaveGameService injected! The InitializeCombat method already exists for enemy health scaling.

**Location**: Update existing `InitializeCombat` method to use DifficultySettings

**CURRENT CODE** (from pre-phase improvements):
```csharp
public void InitializeCombat(Enemy enemy)
{
    var difficulty = _saveGameService.GetDifficultySettings();
    
    // Scale enemy health
    enemy.MaxHealth = (int)(enemy.MaxHealth * difficulty.EnemyHealthMultiplier);
    enemy.Health = enemy.MaxHealth;
}
```

**NO CHANGES NEEDED** - This method already applies enemy health multipliers!

**ADD** player and enemy damage multipliers to combat methods:

**Modify `ExecutePlayerAttack` method** - Apply player damage multiplier:
```csharp
public CombatResult ExecutePlayerAttack(Character player, Enemy enemy)
{
    // ... existing damage calculation ...
    
    // Apply difficulty multiplier to player damage
    var difficulty = _saveGameService.GetDifficultySettings();
    damage = (int)(damage * difficulty.PlayerDamageMultiplier);
    
    // ... rest of method ...
}
```

**Modify `ExecuteEnemyAttack` method** - Apply enemy damage multiplier:
```csharp
public CombatResult ExecuteEnemyAttack(Enemy enemy, Character player, bool playerDefending)
{
    // ... existing damage calculation ...
    
    // Apply difficulty multiplier to enemy damage
    var difficulty = _saveGameService.GetDifficultySettings();
    damage = (int)(damage * difficulty.EnemyDamageMultiplier);
    
    // ... rest of method ...
}
```

---

### 6. `Game/GameEngine.cs` - Integrate Difficulty Selection

**Location**: In `HandleCharacterCreationAsync` method (around line 230)

**FIND** (near end of character creation):
```csharp
// Step 4: Create character
var newCharacter = Services.CharacterCreationService.CreateCharacter(playerName, selectedClass.Name, allocation);

// Step 5: Review character
ReviewCharacter(newCharacter, selectedClass);

ConsoleUI.ShowSuccess($"Welcome, {newCharacter.Name} the {newCharacter.ClassName}!");
await Task.Delay(500);

// Publish character created event
await _mediator.Publish(new CharacterCreated(newCharacter.Name));

// Create save game with the new character
var saveGame = _saveGameService.CreateNewGame(newCharacter);
_currentSaveId = saveGame.Id;
```

**REPLACE WITH**:
```csharp
// Step 4: Create character
var newCharacter = Services.CharacterCreationService.CreateCharacter(playerName, selectedClass.Name, allocation);

// Step 5: Review character
ReviewCharacter(newCharacter, selectedClass);

// Step 6: Select difficulty
var difficulty = Services.CharacterCreationService.SelectDifficulty();

ConsoleUI.ShowSuccess($"Welcome, {newCharacter.Name} the {newCharacter.ClassName}!");
await Task.Delay(500);

// Publish character created event
await _mediator.Publish(new CharacterCreated(newCharacter.Name));

// Create save game with the new character and difficulty settings
var saveGame = _saveGameService.CreateNewGame(newCharacter, difficulty);
_currentSaveId = saveGame.Id;
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Difficulty Selection**:
   - [ ] Start new game
   - [ ] See all 7 difficulty options
   - [ ] Select each difficulty and confirm it's saved
   - [ ] Verify warnings appear for Ironman/Permadeath/Apocalypse
   - [ ] Test canceling difficult mode selection

2. **Combat Multipliers**:
   - [ ] Create Easy mode character → verify enemies die faster
   - [ ] Create Normal mode character → baseline
   - [ ] Create Hard mode character → verify fights are harder
   - [ ] Create Expert mode character → verify extreme difficulty
   - [ ] Verify player damage changes match difficulty
   - [ ] Verify enemy damage changes match difficulty

3. **Save/Load**:
   - [ ] Create save with difficulty
   - [ ] Load save
   - [ ] Verify difficulty persists
   - [ ] Verify multipliers still apply

### Unit Tests (Optional)

Create `Game.Tests/Models/DifficultySettingsTests.cs`:

```csharp
public class DifficultySettingsTests
{
    [Fact]
    public void GetByName_Should_Return_Correct_Difficulty()
    {
        var easy = DifficultySettings.GetByName("Easy");
        easy.Name.Should().Be("Easy");
        easy.PlayerDamageMultiplier.Should().Be(1.5);
    }
    
    [Fact]
    public void GetAll_Should_Return_Seven_Difficulties()
    {
        var all = DifficultySettings.GetAll();
        all.Should().HaveCount(7);
    }
    
    [Theory]
    [InlineData("Easy", 1.5)]
    [InlineData("Normal", 1.0)]
    [InlineData("Hard", 0.75)]
    public void Difficulty_Should_Have_Correct_Player_Damage(string name, double expectedDamage)
    {
        var difficulty = DifficultySettings.GetByName(name);
        difficulty.PlayerDamageMultiplier.Should().Be(expectedDamage);
    }
}
```

---

## ✅ Completion Checklist

- [ ] Created `DifficultySettings.cs` with all 7 modes
- [ ] Updated `SaveGame.cs` with difficulty fields
- [ ] Added `SelectDifficulty()` to `CharacterCreationService.cs`
- [ ] Updated `SaveGameService.CreateNewGame()` to accept `DifficultySettings`
- [ ] Added `GetDifficultySettings()` helper to `SaveGameService`
- [ ] Applied player damage multiplier in `CombatService`
- [ ] Applied enemy damage multiplier in `CombatService`
- [ ] Applied enemy health multiplier in `CombatService.StartCombat()`
- [ ] Integrated difficulty selection in `GameEngine.NewGameAsync()`
- [ ] Tested all 7 difficulty modes
- [ ] Verified combat multipliers work correctly
- [ ] Verified save/load preserves difficulty
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
[Any additional notes or observations]
```

---

## 🔗 Navigation

- **Current Phase**: Phase 1 - Difficulty Foundation
- **Next Phase**: [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)
- **See Also**: 
  - [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)
  - [Phase 4: End-Game System](./PHASE_4_ENDGAME.md)
  - [Project Overview](../README.md)

---

**Ready to implement? Copy this entire artifact into chat to begin Phase 1!** 🚀
