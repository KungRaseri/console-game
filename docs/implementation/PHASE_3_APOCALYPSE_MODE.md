# Phase 3: Apocalypse Mode & Timer System

**Status**: 🟡 Not Started  
**Estimated Time**: 2-3 hours  
**Previous Phase**: [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)  
**Next Phase**: [Phase 4: End-Game System](./PHASE_4_ENDGAME.md)  
**Related**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md)

---

## 📋 Overview

Implement the Apocalypse mode with a 4-hour real-time countdown timer, bonus time rewards for completing quests, and a dramatic game-over when time expires.

---

## 🎯 Goals

1. ✅ Create `ApocalypseTimer` class for countdown management
2. ✅ Display timer prominently on screen during gameplay
3. ✅ Award bonus time for completing quests
4. ✅ Implement time-based game over with dramatic ending
5. ✅ Pause timer during menus/saves
6. ✅ Add warnings at critical time thresholds
7. ✅ Update this artifact with completion status

---

## 📁 Files to Create

### 1. `Game/Services/ApocalypseTimer.cs` (NEW)

```csharp
using Game.UI;
using Serilog;

namespace Game.Services;

/// <summary>
/// Manages the countdown timer for Apocalypse mode.
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
        Log.Information("Apocalypse timer started. {TotalMinutes} minutes until world end.", _totalMinutes);
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
        if (_isPaused)
        {
            // Calculate as if we're still paused
            var elapsed = (_pauseStartTime!.Value - _startTime) - _pausedDuration;
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
}
```

---

## 📝 Files to Modify

### 2. `Game/GameEngine.cs` - Initialize and Use Timer

**ADD** field at top of class:
```csharp
private ApocalypseTimer? _apocalypseTimer;
```

**In `NewGameAsync` or when starting game** after difficulty selection:
```csharp
// After creating save with difficulty
if (difficulty.IsApocalypse)
{
    _apocalypseTimer = new ApocalypseTimer();
    _apocalypseTimer.Start();
    
    ConsoleUI.Clear();
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    ConsoleUI.ShowWarning("      APOCALYPSE MODE ACTIVE           ");
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    ConsoleUI.WriteText("  The world will end in 4 hours!");
    ConsoleUI.WriteText("  Complete the main quest before time runs out.");
    ConsoleUI.WriteText("  Completing quests will award bonus time.");
    ConsoleUI.ShowWarning("═══════════════════════════════════════");
    Thread.Sleep(4000);
}
```

**In main game loop** (check timer every action):
```csharp
private async Task GameLoopAsync()
{
    while (_state == GameState.InGame)
    {
        // Check apocalypse timer
        if (_apocalypseTimer != null)
        {
            _apocalypseTimer.CheckTimeWarnings();
            
            if (_apocalypseTimer.IsExpired())
            {
                await HandleApocalypseGameOver();
                return;
            }
        }
        
        // Display timer in UI
        DisplayGameHUD();
        
        // ... rest of game loop ...
    }
}
```

---

### 3. `Game/GameEngine.cs` - Add HUD Display

**CREATE** new method to display game HUD with timer:
```csharp
private void DisplayGameHUD()
{
    Console.Clear();
    
    // Top bar with character info and timer
    var leftInfo = $"[cyan]{_player.Name}[/] | Level {_player.Level} {_player.ClassName}";
    var centerInfo = $"[green]❤ {_player.Health}/{_player.MaxHealth}[/]  [blue]⚡ {_player.Mana}/{_player.MaxMana}[/]  [yellow]💰 {_player.Gold}g[/]";
    var rightInfo = "";
    
    if (_apocalypseTimer != null)
    {
        rightInfo = _apocalypseTimer.GetColoredTimeDisplay();
    }
    
    // Calculate spacing
    var leftLen = ConsoleUI.StripMarkup(leftInfo).Length;
    var centerLen = ConsoleUI.StripMarkup(centerInfo).Length;
    var rightLen = ConsoleUI.StripMarkup(rightInfo).Length;
    
    var totalWidth = Console.WindowWidth;
    var centerPadding = (totalWidth - centerLen) / 2;
    var rightPadding = totalWidth - leftLen - centerLen - rightLen - 4;
    
    // Display HUD
    Console.WriteLine("═".Repeat(totalWidth));
    AnsiConsole.MarkupLine($"{leftInfo}  {centerInfo}{new string(' ', Math.Max(0, rightPadding))}{rightInfo}");
    Console.WriteLine("═".Repeat(totalWidth));
    Console.WriteLine();
}

// Helper extension
public static class StringExtensions
{
    public static string Repeat(this string s, int count)
    {
        return new string(s[0], count);
    }
}
```

**Note**: ConsoleUI.StripMarkup needs to be added if not exists:
```csharp
// In ConsoleUI.cs
public static string StripMarkup(string text)
{
    return System.Text.RegularExpressions.Regex.Replace(text, @"\[.*?\]", "");
}
```

---

### 4. `Game/GameEngine.cs` - Pause/Resume Timer

**When entering menus**:
```csharp
private void ShowInventoryMenu()
{
    _apocalypseTimer?.Pause();
    
    // ... menu code ...
    
    _apocalypseTimer?.Resume();
}

private void ShowCharacterSheet()
{
    _apocalypseTimer?.Pause();
    
    // ... character sheet code ...
    
    _apocalypseTimer?.Resume();
}
```

**Do the same for**:
- Quest journal
- Save menu
- Settings
- Any menu that pauses gameplay

---

### 5. `Game/GameEngine.cs` - Award Bonus Time on Quest Complete

**In quest completion handler**:
```csharp
private void CompleteQuest(Quest quest)
{
    _saveGameService.CompleteQuest(quest.Id);
    
    // Award rewards
    _player.Gold += quest.GoldReward;
    _player.GainExperience(quest.XpReward);
    
    ConsoleUI.ShowSuccess($"Quest Complete: {quest.Title}");
    ConsoleUI.WriteText($"Rewards: {quest.GoldReward}g, {quest.XpReward} XP");
    
    // Award bonus time in Apocalypse mode
    if (_apocalypseTimer != null)
    {
        var bonusMinutes = quest.Difficulty switch
        {
            "easy" => 10,
            "medium" => 20,
            "hard" => 30,
            _ => 15
        };
        
        _apocalypseTimer.AddBonusTime(bonusMinutes, $"Completed quest: {quest.Title}");
    }
    
    Thread.Sleep(2000);
}
```

---

### 6. `Game/GameEngine.cs` - Apocalypse Game Over

**CREATE** apocalypse game over handler:
```csharp
private async Task HandleApocalypseGameOver()
{
    ConsoleUI.Clear();
    
    // Dramatic apocalypse sequence
    ConsoleUI.ShowError("═══════════════════════════════════════");
    Thread.Sleep(500);
    ConsoleUI.ShowError("        TIME HAS RUN OUT...            ");
    Thread.Sleep(1000);
    ConsoleUI.ShowError("═══════════════════════════════════════");
    Thread.Sleep(1500);
    
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
    Thread.Sleep(2000);
    
    Console.WriteLine();
    ConsoleUI.WriteText("The world crumbles into eternal darkness...");
    ConsoleUI.WriteText("You failed to stop the inevitable.");
    Console.WriteLine();
    
    // Show final statistics
    var saveGame = _saveGameService.GetCurrentSave();
    if (saveGame != null)
    {
        ConsoleUI.WriteText($"Time Survived: {_apocalypseTimer!.GetElapsedMinutes() / 60}h {_apocalypseTimer.GetElapsedMinutes() % 60}m");
        ConsoleUI.WriteText($"Final Level: {_player.Level}");
        ConsoleUI.WriteText($"Quests Completed: {saveGame.QuestsCompleted} / {saveGame.QuestsCompleted + saveGame.ActiveQuests.Count}");
        ConsoleUI.WriteText($"Enemies Defeated: {saveGame.TotalEnemiesDefeated}");
        Console.WriteLine();
        
        // Check if they were close to completing main quest
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
    // Count main quest completions
    // This assumes you have a way to identify main quests
    // For now, use total quest completion as proxy
    var totalQuests = saveGame.QuestsCompleted + saveGame.ActiveQuests.Count + saveGame.AvailableQuests.Count;
    if (totalQuests == 0) return 0;
    
    return (double)saveGame.QuestsCompleted / totalQuests;
}
```

---

### 7. `Game/Models/SaveGame.cs` - Add Timer State

**ADD** methods to save/restore timer state:
```csharp
/// <summary>
/// Get apocalypse timer remaining minutes for display.
/// </summary>
public int GetApocalypseTimeRemaining()
{
    if (!ApocalypseMode || !ApocalypseStartTime.HasValue)
        return 0;
    
    var elapsed = (DateTime.Now - ApocalypseStartTime.Value).TotalMinutes;
    var total = 240 + ApocalypseBonusMinutes; // 4 hours + bonuses
    
    return Math.Max(0, (int)(total - elapsed));
}
```

**In `GameEngine.cs` when loading Apocalypse save**:
```csharp
if (saveGame.ApocalypseMode && saveGame.ApocalypseStartTime.HasValue)
{
    _apocalypseTimer = new ApocalypseTimer();
    // Restore timer state
    var elapsed = (DateTime.Now - saveGame.ApocalypseStartTime.Value).TotalMinutes;
    // Note: May need to add RestoreState method to ApocalypseTimer
    // Or check time on load and show warning if close to expiring
    
    _apocalypseTimer.Start(); // Continue from where left off
    
    if (_apocalypseTimer.IsExpired())
    {
        await HandleApocalypseGameOver();
        return;
    }
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
   - [ ] Verify warnings only show once

5. **Game Over**:
   - [ ] Let timer expire
   - [ ] Verify apocalypse sequence plays
   - [ ] Verify statistics display correctly
   - [ ] Verify game returns to main menu

6. **Save/Load**:
   - [ ] Save game in Apocalypse mode
   - [ ] Quit and reload
   - [ ] Verify timer continues from save point
   - [ ] Verify bonus time persists

### Edge Cases

- [ ] Start Apocalypse mode, immediately save/load
- [ ] Complete many quests quickly (verify bonuses stack)
- [ ] Pause for extended period (verify time tracking)
- [ ] Let timer expire while in menu
- [ ] Load Apocalypse save with expired timer

---

## ✅ Completion Checklist

- [ ] Created `ApocalypseTimer.cs` class
- [ ] Added timer initialization in `GameEngine`
- [ ] Implemented HUD display with timer
- [ ] Added pause/resume for all menus
- [ ] Implemented bonus time on quest completion
- [ ] Created apocalypse game over sequence
- [ ] Added time warnings at thresholds
- [ ] Implemented save/load timer state
- [ ] Tested timer countdown accuracy
- [ ] Tested all bonus time awards
- [ ] Tested apocalypse game over
- [ ] Built successfully with no errors
- [ ] All existing tests still pass

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
