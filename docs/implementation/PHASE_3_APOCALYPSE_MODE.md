# Phase 3: Apocalypse Mode & Timer System

**Status**: ⚪ Ready to Start  
**Prerequisites**: ✅ Phase 1 & 2 complete + CQRS/Vertical Slice Architecture  
**Estimated Time**: 2-3 hours  
**Previous Phase**: [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)  
**Next Phase**: [Phase 4: End-Game System](./PHASE_4_ENDGAME.md)  
**Related**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md)

---

## 📋 Overview

Implement the Apocalypse mode with a 4-hour real-time countdown timer, bonus time rewards for completing quests, and a dramatic game-over when time expires using **CQRS + Vertical Slice Architecture**.

**✅ Pre-Phase Foundation Complete:**

- DifficultySettings already has IsApocalypse and ApocalypseTimeLimitMinutes properties
- SaveGame has ApocalypseMode, ApocalypseStartTime, ApocalypseBonusMinutes fields
- GameStateService provides centralized state access in `Shared/Services/`
- Location tracking ready for apocalypse game-over location
- CQRS infrastructure with MediatR ready

---

## 🎯 Goals

1. ✅ Create `ApocalypseTimer` service for countdown management
2. ✅ Display timer prominently on HUD during gameplay
3. ✅ Award bonus time for completing quests (via command)
4. ✅ Implement time-based game over with dramatic ending
5. ✅ Pause timer during menus/saves
6. ✅ Add warnings at critical time thresholds
7. ✅ Update this artifact with completion status

---

## 📁 Files to Create

### 1. `Game/Shared/Services/ApocalypseTimer.cs` (NEW)

**Note**: Timer goes in `Shared/Services/` because it's infrastructure, not a business feature.

```csharp
using Game.Shared.UI;
using Serilog;

namespace Game.Shared.Services;

/// <summary>
/// Manages the countdown timer for Apocalypse mode.
/// This is a shared service, not a feature, as it's infrastructure.
/// </summary>
public class ApocalypseTimer
{
    private DateTime _startTime;
    private int _totalMinutes = 240; // 4 hours = 240 minutes
    private int _bonusMinutes = 0;
    private bool _isPaused = false;
    private TimeSpan _pausedDuration = TimeSpan.Zero;
    private DateTime? _pauseStartTime = null;
    private bool _hasShownOneHourWarning = false;
    private bool _hasShownThirtyMinWarning = false;
    private bool _hasShownTenMinWarning = false;
    
    /// <summary>
    /// Start the apocalypse timer.
    /// </summary>
    public void Start()
    {
        _startTime = DateTime.Now;
        _isPaused = false;
        _hasShownOneHourWarning = false;
        _hasShownThirtyMinWarning = false;
        _hasShownTenMinWarning = false;
        
        Log.Information("Apocalypse timer started. {TotalMinutes} minutes until world end.", _totalMinutes);
    }
    
    /// <summary>
    /// Start timer from a saved state (for loading saves).
    /// </summary>
    public void StartFromSave(DateTime startTime, int bonusMinutes)
    {
        _startTime = startTime;
        _bonusMinutes = bonusMinutes;
        _isPaused = false;
        
        Log.Information("Apocalypse timer restored from save. Started at: {StartTime}, Bonus: {BonusMinutes}",
            startTime, bonusMinutes);
    }
    
    /// <summary>
    /// Pause the timer (during menus, saves, etc.).
    /// </summary>
    public void Pause()
    {
        if (!_isPaused)
        {
            _isPaused = true;
            _pauseStartTime = DateTime.Now;
            Log.Debug("Apocalypse timer paused");
        }
    }
    
    /// <summary>
    /// Resume the timer.
    /// </summary>
    public void Resume()
    {
        if (_isPaused && _pauseStartTime.HasValue)
        {
            _pausedDuration += DateTime.Now - _pauseStartTime.Value;
            _isPaused = false;
            _pauseStartTime = null;
            Log.Debug("Apocalypse timer resumed. Total paused time: {PausedMinutes} minutes",
                _pausedDuration.TotalMinutes);
        }
    }
    
    /// <summary>
    /// Get remaining minutes on the timer.
    /// </summary>
    public int GetRemainingMinutes()
    {
        if (_isPaused && _pauseStartTime.HasValue)
        {
            // Calculate as if we're still paused
            var elapsed = (_pauseStartTime.Value - _startTime) - _pausedDuration;
            return Math.Max(0, (int)(_totalMinutes + _bonusMinutes - elapsed.TotalMinutes));
        }
        
        var totalElapsed = (DateTime.Now - _startTime) - _pausedDuration;
        return Math.Max(0, (int)(_totalMinutes + _bonusMinutes - totalElapsed.TotalMinutes));
    }
    
    /// <summary>
    /// Check if timer has expired.
    /// </summary>
    public bool IsExpired()
    {
        return GetRemainingMinutes() <= 0;
    }
    
    /// <summary>
    /// Add bonus minutes to the timer.
    /// </summary>
    public void AddBonusTime(int minutes, string reason = "Quest completed")
    {
        _bonusMinutes += minutes;
        
        ConsoleUI.Clear();
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        ConsoleUI.ShowSuccess("      BONUS TIME AWARDED!              ");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        ConsoleUI.WriteText($"  Reason: {reason}");
        ConsoleUI.WriteText($"  Bonus: +{minutes} minutes");
        ConsoleUI.WriteText($"  New Total: {GetRemainingMinutes()} minutes remaining");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        
        Log.Information("Bonus time awarded: {Minutes} minutes. Reason: {Reason}. Remaining: {Remaining}",
            minutes, reason, GetRemainingMinutes());
        
        Thread.Sleep(2000);
    }
    
    /// <summary>
    /// Get formatted time remaining string.
    /// </summary>
    public string GetFormattedTimeRemaining()
    {
        var remaining = GetRemainingMinutes();
        var hours = remaining / 60;
        var mins = remaining % 60;
        
        return $"{hours}h {mins}m";
    }
    
    /// <summary>
    /// Get colored time display for UI.
    /// </summary>
    public string GetColoredTimeDisplay()
    {
        var remaining = GetRemainingMinutes();
        var formatted = GetFormattedTimeRemaining();
        
        var color = remaining switch
        {
            < 10 => "red",
            < 30 => "yellow",
            < 60 => "orange",
            _ => "green"
        };
        
        return $"[{color}]⏱ {formatted}[/]";
    }
    
    /// <summary>
    /// Check and show time warnings.
    /// </summary>
    public void CheckTimeWarnings()
    {
        var remaining = GetRemainingMinutes();
        
        if (remaining <= 60 && !_hasShownOneHourWarning)
        {
            _hasShownOneHourWarning = true;
            ShowTimeWarning("1 HOUR REMAINING!", "The apocalypse draws near...");
        }
        else if (remaining <= 30 && !_hasShownThirtyMinWarning)
        {
            _hasShownThirtyMinWarning = true;
            ShowTimeWarning("30 MINUTES REMAINING!", "Time is running out!");
        }
        else if (remaining <= 10 && !_hasShownTenMinWarning)
        {
            _hasShownTenMinWarning = true;
            ShowTimeWarning("10 MINUTES REMAINING!", "The end is imminent!");
        }
    }
    
    /// <summary>
    /// Show a time warning to the player.
    /// </summary>
    private void ShowTimeWarning(string title, string message)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowWarning("═══════════════════════════════════════");
        ConsoleUI.ShowWarning($"      {title}");
        ConsoleUI.ShowWarning("═══════════════════════════════════════");
        ConsoleUI.WriteText($"  {message}");
        ConsoleUI.WriteText($"  Time Left: {GetFormattedTimeRemaining()}");
        ConsoleUI.ShowWarning("═══════════════════════════════════════");
        
        Log.Warning("Apocalypse timer warning: {Title}", title);
        Thread.Sleep(3000);
    }
    
    /// <summary>
    /// Get total time limit with bonuses.
    /// </summary>
    public int GetTotalTimeLimit()
    {
        return _totalMinutes + _bonusMinutes;
    }
    
    /// <summary>
    /// Get time elapsed.
    /// </summary>
    public int GetElapsedMinutes()
    {
        var totalElapsed = (DateTime.Now - _startTime) - _pausedDuration;
        return (int)totalElapsed.TotalMinutes;
    }
    
    /// <summary>
    /// Get bonus minutes awarded so far (for save persistence).
    /// </summary>
    public int GetBonusMinutes()
    {
        return _bonusMinutes;
    }
}
```

---

### 2. `Game/Shared/UI/ConsoleUI.cs` - Add StripMarkup Helper

**ADD** this method to ConsoleUI if it doesn't already exist:

```csharp
/// <summary>
/// Strip Spectre.Console markup from text for length calculations.
/// </summary>
public static string StripMarkup(string text)
{
    return System.Text.RegularExpressions.Regex.Replace(text, @"\[.*?\]", "");
}
```

---

## 📝 Files to Modify

### 3. `Game/Program.cs` - Register ApocalypseTimer

**FIND** the service registration section:

```csharp
// Register shared services
services.AddSingleton<GameStateService>();
services.AddSingleton<MenuService>();
// ... other services ...
```

**ADD** this registration:

```csharp
// Apocalypse timer (shared service)
services.AddSingleton<ApocalypseTimer>();
```

---

### 4. `Game/GameEngine.cs` - Initialize and Use Timer

**ADD** using statement at top:

```csharp
using Game.Shared.Services;
```

**ADD** field for timer:

```csharp
private readonly ApocalypseTimer? _apocalypseTimer;
```

**UPDATE** constructor to inject it:

```csharp
public GameEngine(
    IMediator mediator,
    SaveGameService saveGameService,
    HallOfFameService hallOfFameService,
    ApocalypseTimer apocalypseTimer,
    // ... other parameters
    )
{
    // ... existing code ...
    _apocalypseTimer = apocalypseTimer;
}
```

**AFTER** difficulty selection in character creation:

**FIND** where the save game is created with difficulty:

```csharp
// Create save game with the new character and difficulty settings
var saveGame = _saveGameService.CreateNewGame(newCharacter, difficulty);
_currentSaveId = saveGame.Id;
```

**ADD AFTER** this:

```csharp
// Start apocalypse timer if applicable
if (difficulty.IsApocalypse && _apocalypseTimer != null)
{
    _apocalypseTimer.Start();
    
    ConsoleUI.Clear();
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    ConsoleUI.ShowWarning("      APOCALYPSE MODE ACTIVE           ");
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    ConsoleUI.WriteText("  The world will end in 4 hours!");
    ConsoleUI.WriteText("  Complete the main quest before time runs out.");
    ConsoleUI.WriteText("  Completing quests will award bonus time.");
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    await Task.Delay(4000);
}
```

**IN** the main game loop, ADD timer checks:

**FIND** your main game loop method (might be `RunGameAsync()` or `GameLoopAsync()`):

```csharp
private async Task GameLoopAsync()
{
    while (_state == GameState.InGame)
    {
        // ... existing code ...
    }
}
```

**ADD** at the beginning of the loop:

```csharp
private async Task GameLoopAsync()
{
    while (_state == GameState.InGame)
    {
        // Check apocalypse timer
        if (_apocalypseTimer != null)
        {
            var saveGame = _saveGameService.GetCurrentSave();
            if (saveGame != null && saveGame.ApocalypseMode)
            {
                _apocalypseTimer.CheckTimeWarnings();
                
                if (_apocalypseTimer.IsExpired())
                {
                    await HandleApocalypseGameOverAsync();
                    return;
                }
            }
        }
        
        // Display HUD with timer
        DisplayGameHUD();
        
        // ... rest of game loop ...
    }
}
```

---

### 5. `Game/GameEngine.cs` - Add HUD Display with Timer

**ADD** this method:

```csharp
private void DisplayGameHUD()
{
    Console.Clear();
    
    // Top bar with character info and timer
    var leftInfo = $"[cyan]{_player.Name}[/] | Level {_player.Level} {_player.ClassName}";
    var centerInfo = $"[green]❤ {_player.Health}/{_player.MaxHealth}[/]  [blue]⚡ {_player.Mana}/{_player.MaxMana}[/]  [yellow]💰 {_player.Gold}g[/]";
    var rightInfo = "";
    
    // Add timer if in Apocalypse mode
    var saveGame = _saveGameService.GetCurrentSave();
    if (saveGame != null && saveGame.ApocalypseMode && _apocalypseTimer != null)
    {
        rightInfo = _apocalypseTimer.GetColoredTimeDisplay();
    }
    
    // Calculate spacing for centered layout
    var leftLen = ConsoleUI.StripMarkup(leftInfo).Length;
    var centerLen = ConsoleUI.StripMarkup(centerInfo).Length;
    var rightLen = ConsoleUI.StripMarkup(rightInfo).Length;
    
    var totalWidth = Console.WindowWidth;
    var spacing = Math.Max(2, (totalWidth - leftLen - centerLen - rightLen) / 2);
    
    // Display HUD
    Console.WriteLine(new string('═', totalWidth));
    AnsiConsole.MarkupLine($"{leftInfo}{new string(' ', spacing)}{centerInfo}{new string(' ', spacing)}{rightInfo}");
    Console.WriteLine(new string('═', totalWidth));
    Console.WriteLine();
}
```

---

### 6. `Game/GameEngine.cs` - Pause/Resume Timer in Menus

For **EACH** menu method (inventory, character sheet, quest journal, save menu, settings), wrap with pause/resume:

**EXAMPLE** for inventory:

```csharp
private async Task ShowInventoryAsync()
{
    _apocalypseTimer?.Pause();
    
    try
    {
        // ... existing inventory code ...
    }
    finally
    {
        _apocalypseTimer?.Resume();
    }
}
```

**Apply the same pattern to**:

- `ShowCharacterSheetAsync()` or similar
- `ShowQuestJournalAsync()` or similar
- `ShowSaveMenuAsync()` or similar
- `ShowSettingsAsync()` or similar
- Any other menu that pauses gameplay

---

### 7. `Game/GameEngine.cs` - Award Bonus Time on Quest Complete

**FIND** where quests are completed (likely in a quest completion handler):

**ADD** bonus time logic:

```csharp
private async Task OnQuestCompletedAsync(Quest quest)
{
    // Award rewards
    _player.Gold += quest.GoldReward;
    _player.GainExperience(quest.XpReward);
    
    ConsoleUI.ShowSuccess($"Quest Complete: {quest.Title}");
    ConsoleUI.WriteText($"Rewards: {quest.GoldReward}g, {quest.XpReward} XP");
    
    // Award bonus time in Apocalypse mode
    var saveGame = _saveGameService.GetCurrentSave();
    if (saveGame != null && saveGame.ApocalypseMode && _apocalypseTimer != null)
    {
        var bonusMinutes = quest.Difficulty switch
        {
            "easy" => 10,
            "medium" => 20,
            "hard" => 30,
            _ => 15
        };
        
        _apocalypseTimer.AddBonusTime(bonusMinutes, $"Completed quest: {quest.Title}");
        
        // Update save with new bonus time
        saveGame.ApocalypseBonusMinutes = _apocalypseTimer.GetBonusMinutes();
        _saveGameService.SaveGame(saveGame);
    }
    
    await Task.Delay(2000);
}
```

---

### 8. `Game/GameEngine.cs` - Apocalypse Game Over Handler

**ADD** this method:

```csharp
private async Task HandleApocalypseGameOverAsync()
{
    ConsoleUI.Clear();
    
    // Dramatic apocalypse sequence
    ConsoleUI.ShowError("═══════════════════════════════════════");
    await Task.Delay(500);
    ConsoleUI.ShowError("        TIME HAS RUN OUT...            ");
    await Task.Delay(1000);
    ConsoleUI.ShowError("═══════════════════════════════════════");
    await Task.Delay(1500);
    
    Console.Clear();
    ConsoleUI.ShowError("The world trembles...");
    await Task.Delay(2000);
    
    ConsoleUI.ShowError("The sky darkens...");
    await Task.Delay(2000);
    
    ConsoleUI.ShowError("Reality fractures...");
    await Task.Delay(2000);
    
    Console.Clear();
    ConsoleUI.ShowError("═════════════════════════════════════════════════");
    ConsoleUI.ShowError("                                                 ");
    ConsoleUI.ShowError("         THE APOCALYPSE HAS COME                 ");
    ConsoleUI.ShowError("                                                 ");
    ConsoleUI.ShowError("═════════════════════════════════════════════════");
    await Task.Delay(2000);
    
    Console.WriteLine();
    ConsoleUI.WriteText("The world crumbles into eternal darkness...");
    ConsoleUI.WriteText("You failed to stop the inevitable.");
    Console.WriteLine();
    
    // Show final statistics
    var saveGame = _saveGameService.GetCurrentSave();
    if (saveGame != null && _apocalypseTimer != null)
    {
        var elapsed = _apocalypseTimer.GetElapsedMinutes();
        ConsoleUI.WriteText($"Time Survived: {elapsed / 60}h {elapsed % 60}m");
        ConsoleUI.WriteText($"Final Level: {_player.Level}");
        ConsoleUI.WriteText($"Quests Completed: {saveGame.QuestsCompleted}");
        ConsoleUI.WriteText($"Enemies Defeated: {saveGame.EnemiesDefeated}");
        Console.WriteLine();
        
        // Check progress
        var mainQuestProgress = CalculateMainQuestProgress(saveGame);
        if (mainQuestProgress >= 0.8) // 80% complete
        {
            ConsoleUI.ShowWarning("You were so close... Only a few quests remained.");
        }
        else if (mainQuestProgress >= 0.5)
        {
            ConsoleUI.ShowWarning("You made significant progress, but it wasn't enough.");
        }
        else
        {
            ConsoleUI.ShowWarning("You barely scratched the surface of what was needed.");
        }
    }
    
    Console.WriteLine();
    ConsoleUI.ShowError("═════════════════════════════════════════════════");
    ConsoleUI.ShowError("                  GAME OVER                      ");
    ConsoleUI.ShowError("═════════════════════════════════════════════════");
    
    Log.Warning("Apocalypse game over. Player failed to complete main quest in time.");
    
    await Task.Delay(5000);
    
    // Ask if they want to try again
    if (ConsoleUI.Confirm("Try again?"))
    {
        _state = GameState.MainMenu;
    }
    else
    {
        _state = GameState.Quitting;
    }
}

private double CalculateMainQuestProgress(SaveGame saveGame)
{
    // Simple calculation - can be enhanced in Phase 4 with actual main quest tracking
    var totalQuests = Math.Max(1, saveGame.QuestsCompleted + saveGame.ActiveQuests.Count);
    return (double)saveGame.QuestsCompleted / totalQuests;
}
```

---

### 9. `Game/GameEngine.cs` - Restore Timer on Load

**FIND** where saves are loaded:

**ADD** timer restoration logic:

```csharp
private async Task LoadSaveAsync(SaveGame saveGame)
{
    // ... existing load logic ...
    
    // Restore apocalypse timer if applicable
    if (saveGame.ApocalypseMode && saveGame.ApocalypseStartTime.HasValue && _apocalypseTimer != null)
    {
        _apocalypseTimer.StartFromSave(saveGame.ApocalypseStartTime.Value, saveGame.ApocalypseBonusMinutes);
        
        // Check if time expired while they were away
        if (_apocalypseTimer.IsExpired())
        {
            ConsoleUI.ShowError("Time has run out! The apocalypse occurred while you were gone.");
            await Task.Delay(3000);
            await HandleApocalypseGameOverAsync();
            return;
        }
        
        // Show time remaining
        var remaining = _apocalypseTimer.GetRemainingMinutes();
        ConsoleUI.ShowWarning($"Apocalypse Mode: {remaining} minutes remaining!");
        
        if (remaining < 60)
        {
            ConsoleUI.ShowError("WARNING: Less than 1 hour remaining!");
        }
        
        await Task.Delay(2000);
    }
    
    // ... rest of load logic ...
}
```

---

### 10. `Game/Features/SaveLoad/SaveGameService.cs` - Update Save to Store Timer State

**FIND** the `SaveGame()` method:

**ADD** logic to save timer state:

```csharp
public void SaveGame(SaveGame saveGame)
{
    // Update apocalypse timer state if applicable
    if (saveGame.ApocalypseMode && _apocalypseTimer != null)
    {
        saveGame.ApocalypseBonusMinutes = _apocalypseTimer.GetBonusMinutes();
        // ApocalypseStartTime is already set during game creation
    }
    
    // ... existing save logic ...
}
```

**Note**: You'll need to inject `ApocalypseTimer` into `SaveGameService` constructor:

```csharp
private readonly ApocalypseTimer? _apocalypseTimer;

public SaveGameService(ApocalypseTimer? apocalypseTimer = null)
{
    _apocalypseTimer = apocalypseTimer;
}
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Timer Functionality**:
   - [ ] Create Apocalypse mode character
   - [ ] Verify timer starts at 240 minutes (4 hours)
   - [ ] Verify timer counts down during gameplay
   - [ ] Verify timer displays on HUD
   - [ ] Verify timer color changes (green → orange → yellow → red)

2. **Pause/Resume**:
   - [ ] Open inventory menu → verify timer pauses
   - [ ] Close menu → verify timer resumes
   - [ ] Test with quest journal, character sheet, save menu
   - [ ] Verify paused time doesn't count against total

3. **Bonus Time**:
   - [ ] Complete an easy quest → verify +10 minutes
   - [ ] Complete a medium quest → verify +20 minutes
   - [ ] Complete a hard quest → verify +30 minutes
   - [ ] Verify bonus time message displays
   - [ ] Verify total time increases

4. **Time Warnings**:
   - [ ] Play until 60 minutes remain → verify 1-hour warning
   - [ ] Play until 30 minutes remain → verify 30-min warning
   - [ ] Play until 10 minutes remain → verify 10-min warning
   - [ ] Verify warnings only show once per session

5. **Game Over**:
   - [ ] Let timer expire
   - [ ] Verify apocalypse sequence plays
   - [ ] Verify statistics display correctly
   - [ ] Verify game returns to main menu or quits

6. **Save/Load**:
   - [ ] Save game in Apocalypse mode
   - [ ] Quit and reload
   - [ ] Verify timer continues from save point
   - [ ] Verify bonus time persists
   - [ ] Load save after time expired → verify game over

### Edge Cases

- [ ] Start Apocalypse mode, immediately save/load
- [ ] Complete many quests quickly (verify bonuses stack)
- [ ] Pause for extended period (verify time tracking accurate)
- [ ] Let timer expire while in menu (should not expire until resumed)
- [ ] Load Apocalypse save with 1 minute left
- [ ] Warnings after load (should not re-show if already shown)

---

## ✅ Completion Checklist

- [ ] Created `ApocalypseTimer.cs` in `Shared/Services/`
- [ ] Added `StripMarkup` method to `ConsoleUI`
- [ ] Registered `ApocalypseTimer` in `Program.cs`
- [ ] Injected timer into `GameEngine` constructor
- [ ] Added timer initialization after character creation
- [ ] Implemented HUD display with timer
- [ ] Added pause/resume for all menu methods
- [ ] Implemented bonus time on quest completion
- [ ] Created apocalypse game over sequence
- [ ] Implemented timer restore on load
- [ ] Updated `SaveGameService` to persist timer state
- [ ] Tested timer countdown accuracy
- [ ] Tested all bonus time awards
- [ ] Tested apocalypse game over
- [ ] Tested save/load timer persistence
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
[Any additional notes about timer accuracy, game balance, etc.]
```

---

## 🔗 Navigation

- **Previous Phase**: [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)
- **Current Phase**: Phase 3 - Apocalypse Mode
- **Next Phase**: [Phase 4: End-Game System](./PHASE_4_ENDGAME.md)
- **See Also**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md)

---

**Ready to implement? Copy this entire artifact into chat to begin Phase 3!** 🚀
