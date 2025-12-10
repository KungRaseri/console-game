using Game.Shared.UI;
using Serilog;

namespace Game.Shared.Services;

/// <summary>
/// Manages the countdown timer for Apocalypse mode.
/// This is a shared service, not a feature, as it's infrastructure.
/// </summary>
public class ApocalypseTimer
{
    private readonly IConsoleUI _console;
    private DateTime _startTime;
    private int _totalMinutes = 240; // 4 hours = 240 minutes
    private int _bonusMinutes = 0;
    private bool _isPaused = false;
    private TimeSpan _pausedDuration = TimeSpan.Zero;
    private DateTime? _pauseStartTime = null;
    private bool _hasShownOneHourWarning = false;
    private bool _hasShownThirtyMinWarning = false;
    private bool _hasShownTenMinWarning = false;
    
    public ApocalypseTimer(IConsoleUI console)
    {
        _console = console;
    }
    
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
        
        _console.Clear();
        _console.ShowSuccess("═══════════════════════════════════════");
        _console.ShowSuccess("      BONUS TIME AWARDED!              ");
        _console.ShowSuccess("═══════════════════════════════════════");
        _console.WriteText($"  Reason: {reason}");
        _console.WriteText($"  Bonus: +{minutes} minutes");
        _console.WriteText($"  New Total: {GetRemainingMinutes()} minutes remaining");
        _console.ShowSuccess("═══════════════════════════════════════");
        
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
        _console.Clear();
        _console.ShowWarning("═══════════════════════════════════════");
        _console.ShowWarning($"      {title}");
        _console.ShowWarning("═══════════════════════════════════════");
        _console.WriteText($"  {message}");
        _console.WriteText($"  Time Left: {GetFormattedTimeRemaining()}");
        _console.ShowWarning("═══════════════════════════════════════");
        
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
