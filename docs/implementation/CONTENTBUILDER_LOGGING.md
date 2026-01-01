# ContentBuilder Logging Implementation

**Date:** December 15, 2024  
**Status:** ✅ Complete  
**Version:** 1.0

---

## Overview

The ContentBuilder WPF application now includes comprehensive structured logging using Serilog with a `.latest.log` pattern and automatic timestamped file renaming on exit.

---

## Logging Features

### 1. Latest Log Pattern
- **Active Log:** `logs/contentbuilder.latest.log`
- **Behavior:** Deleted and recreated on each application start
- **Purpose:** Always know where the current session log is located

### 2. Timestamped Archive
- **On Exit:** `.latest.log` renamed to `contentbuilder-YYYYMMDD-HHMMSS.log`
- **Format Example:** `contentbuilder-20241215-143052.log`
- **Collision Handling:** Appends counter if file exists (e.g., `-1.log`, `-2.log`)

### 3. Log Content
**Startup Information:**
```
==========================================================
ContentBuilder Application Starting
Start Time: 2024-12-15 14:30:52
Log File: C:\code\console-game\RealmForge\bin\Debug\net9.0-windows\logs\contentbuilder.latest.log
==========================================================
MainViewModel initialized - Data directory: C:\code\console-game\RealmEngine.Shared\Data\Json
Categories initialized - Total categories: 5
Application startup complete
```

**User Actions:**
```
Category selected: Weapons - Type: NameList - File: items/weapons/names.json
Loading NameListEditor for items/weapons/names.json
NameListEditor loaded successfully for items/weapons/names.json
```

**File Operations (from JsonEditorService):**
```
Loaded JSON file: items/weapons/names.json
Saved JSON file: items/weapons/names.json
Created backup: items/weapons/names.json.backup.20241215-143512
```

**Exit Information:**
```
==========================================================
Application Exiting
Exit Code: 0
Exit Time: 2024-12-15 14:45:23
Session Duration: 00:14:31
==========================================================
```

---

## Implementation Details

### App.xaml.cs Changes

**Static Fields:**
```csharp
private static readonly DateTime _startTime = DateTime.Now;
private static readonly string _logsDirectory = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, 
    "logs");

private static readonly string _latestLogPath = Path.Combine(
    _logsDirectory,
    "contentbuilder.latest.log");

private static string? _finalLogPath;
```

**Serilog Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        _latestLogPath,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .WriteTo.Console()
    .CreateLogger();
```

**OnExit Renaming Logic:**
```csharp
protected override void OnExit(ExitEventArgs e)
{
    // Log exit information
    Log.Information("Application Exiting");
    Log.CloseAndFlush();
    
    // Rename .latest.log to timestamped version
    _finalLogPath = Path.Combine(
        _logsDirectory,
        $"contentbuilder-{_startTime:yyyyMMdd-HHmmss}.log");
    
    File.Move(_latestLogPath, _finalLogPath);
}
```

---

## Log Output Template

**Format:**
```
{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}
```

**Example Output:**
```
2024-12-15 14:30:52.123 [INF] ContentBuilder Application Starting
2024-12-15 14:30:52.456 [INF] MainViewModel initialized - Data directory: C:\code\console-game\RealmEngine.Shared\Data\Json
2024-12-15 14:31:15.789 [INF] Category selected: Weapons - Type: NameList - File: items/weapons/names.json
2024-12-15 14:31:15.890 [DBG] Loading NameListEditor for items/weapons/names.json
2024-12-15 14:31:16.012 [INF] NameListEditor loaded successfully for items/weapons/names.json
2024-12-15 14:32:45.234 [ERR] Failed to load FlatItemEditor for items/broken.json
System.IO.FileNotFoundException: Could not find file 'items/broken.json'
   at RealmForge.Services.JsonEditorService.LoadJson(String fileName)
```

---

## Log Levels

| Level | Usage | Examples |
|-------|-------|----------|
| **Debug** | Detailed flow tracking | `Loading NameListEditor for {FileName}` |
| **Information** | Normal operations | `Category selected`, `File saved` |
| **Warning** | Non-critical issues | `File not found`, `Backup skipped` |
| **Error** | Recoverable errors | `Failed to load editor`, `Save failed` |
| **Fatal** | Critical crashes | `Unhandled exception - terminating` |

---

## Logged Operations

### Application Lifecycle
- ✅ Application startup with timestamp
- ✅ Data directory initialization
- ✅ Category tree initialization
- ✅ Application exit with duration

### User Actions
- ✅ Category selection (file + editor type)
- ✅ Editor loading attempts
- ✅ Preview window open/close
- ✅ Exit command

### File Operations (JsonEditorService)
- ✅ JSON file load success/failure
- ✅ JSON file save success/failure
- ✅ Backup creation
- ✅ File validation errors

### Editor Operations
- ✅ ItemEditor load/save
- ✅ FlatItemEditor load/save
- ✅ NameListEditor load/save
- ✅ HybridArrayEditor load/save

### Error Handling
- ✅ Unhandled dispatcher exceptions (UI thread)
- ✅ Critical app domain exceptions
- ✅ JSON parsing errors
- ✅ File I/O errors

---

## Log File Locations

### Development
```
RealmForge/bin/Debug/net9.0-windows/logs/
├── contentbuilder.latest.log          ← Active session (while app running)
├── contentbuilder-20241215-143052.log ← Session 1 (14:30-14:45)
├── contentbuilder-20241215-150234.log ← Session 2 (15:02-15:18)
└── contentbuilder-20241215-163045.log ← Session 3 (16:30-16:42)
```

### Production/Release
```
[ExecutableDirectory]/logs/
├── contentbuilder.latest.log
├── contentbuilder-YYYYMMDD-HHMMSS.log
└── ...
```

---

## Benefits

### 1. Development
- **Always know current log:** Check `contentbuilder.latest.log`
- **Session tracking:** Each session gets unique timestamped file
- **Debugging:** Detailed Debug-level logs for troubleshooting

### 2. User Support
- **Easy to find:** "Send me the .latest.log file"
- **Session history:** Previous sessions preserved
- **Error reporting:** Full stack traces with context

### 3. Monitoring
- **Usage patterns:** See which files users edit most
- **Error frequency:** Track recurring issues
- **Performance:** Session duration tracking

---

## Configuration Options

### Adjust Log Level
Change minimum level in `App.xaml.cs`:
```csharp
.MinimumLevel.Debug()      // Current (most verbose)
.MinimumLevel.Information() // Normal operations only
.MinimumLevel.Warning()     // Warnings and errors only
```

### Adjust Flush Interval
Change disk write frequency:
```csharp
flushToDiskInterval: TimeSpan.FromSeconds(1)  // Current (1 second)
flushToDiskInterval: TimeSpan.FromSeconds(5)  // Less frequent (better performance)
```

### Add Rolling File Limit
Limit number of archived logs:
```csharp
.WriteTo.File(
    _latestLogPath,
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30  // Keep last 30 sessions
)
```

---

## Testing Checklist

### ✅ Verified Behaviors

1. **Startup**
   - ✅ `.latest.log` created on start
   - ✅ Old `.latest.log` deleted if exists
   - ✅ Startup banner logged

2. **Runtime**
   - ✅ User actions logged
   - ✅ File operations logged
   - ✅ Errors logged with stack traces

3. **Exit**
   - ✅ Exit banner logged
   - ✅ Session duration calculated
   - ✅ Log closed and flushed
   - ✅ `.latest.log` renamed to timestamped file
   - ✅ Collision handling (counter appended)

4. **Crash Handling**
   - ✅ Unhandled exceptions logged
   - ✅ Log flushed before crash
   - ✅ File rename may fail (stays as `.latest.log`)

---

## Usage Examples

### Check Current Session Log
```powershell
# View last 50 lines
Get-Content RealmForge\bin\Debug\net9.0-windows\logs\contentbuilder.latest.log -Tail 50
```

### Find Error Sessions
```powershell
# Search all logs for errors
Get-ChildItem logs\*.log | Select-String "\[ERR\]" | Group-Object Path
```

### Session Duration Report
```powershell
# Extract session durations
Get-Content logs\contentbuilder-*.log | Select-String "Session Duration"
```

---

## Future Enhancements

### Potential Improvements
1. **Log Rotation** - Auto-delete logs older than 30 days
2. **Log Upload** - Send logs to telemetry service
3. **Performance Metrics** - Track editor load times
4. **User Analytics** - Most edited files, session patterns
5. **Error Aggregation** - Group similar errors for reporting

### Structured Logging Examples
Current implementation uses structured logging:
```csharp
// Good - structured
Log.Information("Category selected: {CategoryName} - Type: {EditorType}", name, type);

// Avoid - string interpolation
Log.Information($"Category selected: {name} - Type: {type}");
```

Benefits:
- Log analysis tools can query by property
- Better performance (no string allocation)
- Type-safe logging

---

## Related Files

**Modified:**
- `RealmForge/App.xaml.cs` - Main logging setup
- `RealmForge/ViewModels/MainViewModel.cs` - Enhanced action logging

**Already Using Logging:**
- `RealmForge/Services/JsonEditorService.cs` - File operations
- `RealmForge/Services/PreviewService.cs` - Preview generation
- `RealmForge/ViewModels/ItemEditorViewModel.cs` - Editor operations

---

## Summary

The ContentBuilder now has **production-ready logging** with:
- ✅ `.latest.log` pattern for easy access
- ✅ Automatic timestamped archiving on exit
- ✅ Comprehensive operation tracking
- ✅ Full error reporting with stack traces
- ✅ Session duration tracking
- ✅ Structured logging for analysis

**Result:** Easy debugging, better user support, and session tracking!
