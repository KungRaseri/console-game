# RealmEngine Godot Logging Integration

This guide shows how to integrate RealmEngine's Serilog-based logging with Godot's `GD.Print()` system.

## Quick Setup

### 1. Copy the RealmEngineManager

Copy `docs/godot/RealmEngineManager.cs` into your Godot project and attach it to a Node in your main scene.

### 2. Add to Group

In the Godot editor, select the Node with RealmEngineManager and add it to a group called `RealmEngineManager`.

### 3. Use RealmEngine Services

From any other script in your project:

```csharp
public partial class GameController : Node
{
    public override void _Ready()
    {
        // Get RealmEngine services
        var (dataCache, referenceResolver) = this.GetRealmEngineServices();
        
        // Create a logger for this class
        var logger = this.CreateLogger<GameController>();
        
        // Now all RealmEngine logs will appear in Godot console!
        logger.LogInformation("Game controller initialized");
    }
}
```

## Manual Setup

If you prefer more control, you can set up logging manually:

```csharp
public override void _Ready()
{
    // Initialize RealmEngine logging
    var loggerFactory = GodotLogger.Initialize(
        minimumLevel: Serilog.Events.LogEventLevel.Debug,
        includeConsole: false
    );

    // Subscribe to log events
    GodotLogger.Subscribe((level, message) => 
    {
        switch (level)
        {
            case LogLevel.Error:
            case LogLevel.Fatal:
                GD.PrintErr($"[RealmEngine] {message}");
                break;
            case LogLevel.Warning:
                GD.Print($"[RealmEngine] ⚠️ {message}");
                break;
            default:
                GD.Print($"[RealmEngine] {message}");
                break;
        }
    });

    // Now initialize your RealmEngine services with the logger factory
    var dataCache = new GameDataCache("Data/Json");
    var logger = loggerFactory.CreateLogger<ReferenceResolverService>();
    var referenceResolver = new ReferenceResolverService(dataCache, logger);
}

public override void _ExitTree()
{
    // Clean up when scene is destroyed
    GodotLogger.Shutdown();
}
```

## Log Levels

The integration maps Serilog levels to appropriate Godot output:

| Serilog Level | LogLevel Enum | Godot Output | Icon |
|---------------|---------------|--------------|------|
| Verbose | Trace | `GD.Print()` | 🔍 |
| Debug | Debug | `GD.Print()` | 🐛 |
| Information | Info | `GD.Print()` | ℹ️ |
| Warning | Warning | `GD.Print()` | ⚠️ |
| Error | Error | `GD.PrintErr()` | ❌ |
| Fatal | Fatal | `GD.PrintErr()` | 💀 |

## Features

### ✅ Structured Logging
All RealmEngine components now provide detailed, structured log messages instead of basic `Console.WriteLine()` calls.

### ✅ Contextual Information
Log messages include relevant context like:
- File paths for missing catalogs
- Reference IDs for failed resolutions  
- Generator names and parameters
- Exception details with stack traces

### ✅ Easy Filtering
Set minimum log level to control verbosity:
```csharp
// Only show warnings and errors
GodotLogger.Initialize(Serilog.Events.LogEventLevel.Warning);

// Show everything including debug info
GodotLogger.Initialize(Serilog.Events.LogEventLevel.Debug);
```

### ✅ No Performance Impact
When no subscribers are attached, logging events are not processed, so there's minimal overhead.

## Example Output

When you run your Godot game with RealmEngine, you'll see structured logs like:

```
[RealmEngine] ℹ️ [21:45:12 INF] Loaded classes catalog with 6 categories
[RealmEngine] ⚠️ [21:45:13 WRN] Could not resolve ability reference '@abilities/active/support:heal' for class 'Priest' - item may not exist in catalog
[RealmEngine] ⚠️ [21:45:13 WRN] Item not found: heal in active/support (reference: @abilities/active/support:heal)
[RealmEngine] ℹ️ [21:45:14 INF] Generated 3 enemies from category 'beasts'
```

This makes it immediately clear what's working and what needs attention in your RealmEngine data!

## Troubleshooting

### Logger Already Initialized
If you see "GodotLogger is already initialized", call `GodotLogger.Shutdown()` before reinitializing.

### No Logs Appearing
1. Check that you called `GodotLogger.Initialize()`
2. Verify you subscribed to log events with `GodotLogger.Subscribe()`
3. Make sure your minimum log level isn't too restrictive

### Memory Leaks
Always call `GodotLogger.Shutdown()` in `_ExitTree()` to clean up event subscriptions and Serilog resources.