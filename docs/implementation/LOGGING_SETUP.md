# Logging Setup Summary

## Overview
Implemented proper dependency injection-based logging across the solution using Microsoft.Extensions.Logging with Serilog as the provider.

## Architecture

### Logging Provider: Serilog
- **Static Logger**: `LoggingService.Initialize()` sets up the global Serilog `Log.Logger`
- **DI Integration**: Serilog is bridged to `ILogger<T>` via `Microsoft.Extensions.Logging`
- **Output Sinks**:
  - Console (with timestamps and log levels)
  - File (rolling daily logs in `logs/game-.txt`)

### Package Dependencies

#### Game.Console (Application Layer)
```xml
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Console" Version="10.0.0" />
```

#### Game.Core (Business Logic Layer)
```xml
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
```

**Note**: Game.Core only references `Abstractions` to avoid coupling to a specific logging implementation.

## Configuration in Program.cs

```csharp
// 1. Initialize Serilog first (before DI)
LoggingService.Initialize();

// 2. Register logging in DI container
services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddSerilog(dispose: true);
});
```

## Usage Patterns

### In Game.Core Services (Recommended)
```csharp
using Microsoft.Extensions.Logging;

public class GameStateService
{
    private readonly ILogger<GameStateService> _logger;
    
    public GameStateService(
        SaveGameService saveGameService,
        ILogger<GameStateService> logger)
    {
        _saveGameService = saveGameService;
        _logger = logger;
    }
    
    public void UpdateLocation(string location)
    {
        _logger.LogInformation("Player visited {Location}", location);
    }
}
```

### In Program.cs or Startup Code (Static Logger)
```csharp
using Serilog;

Log.Information("Application starting");
Log.Debug("Configuration loaded");
Log.Fatal(ex, "Application crashed");
```

### In Tests (Null Logger)
```csharp
using Game.Tests.Helpers;

// Use helper to create null loggers
var logger = LoggerHelper.CreateNullLogger<GameStateService>();
var service = new GameStateService(saveGameService, logger);
```

## Log Levels

- **Verbose**: Detailed tracing (not currently used)
- **Debug**: Development diagnostics (configuration, validation)
- **Information**: General flow (game start, player actions)
- **Warning**: Unexpected but recoverable issues
- **Error**: Errors that don't crash the app
- **Fatal**: Critical errors that terminate the app

## Example Log Output

### Console
```
[10:04:08 INF] Game logging initialized
[10:04:08 INF] Game starting - Version 1.0
[10:04:08 DBG] Configuration loaded from appsettings.json
[10:04:08 DBG] GameSettings validation passed
```

### File (logs/game-20251216.txt)
```
[2025-12-16 10:04:08.123 -05:00 INF] Game logging initialized
[2025-12-16 10:04:08.456 -05:00 INF] Game starting - Version 1.0
[2025-12-16 10:04:08.789 -05:00 DBG] Configuration loaded from appsettings.json
```

## Benefits of This Approach

1. **Testability**: Services can receive `ILogger<T>` which can be mocked/nulled in tests
2. **Structured Logging**: Parameters like `{Location}` are structured data, not string interpolation
3. **Performance**: Lazy evaluation - log messages only formatted if the level is enabled
4. **Flexibility**: Easy to swap Serilog for another provider (NLog, etc.)
5. **Separation of Concerns**: Game.Core doesn't depend on Serilog directly

## Best Practices

### DO:
- ✅ Use `ILogger<T>` in Game.Core services (DI-friendly)
- ✅ Use structured logging with parameters: `_logger.LogInformation("Player {Name} leveled up to {Level}", name, level)`
- ✅ Use static `Log` in startup/shutdown code where DI isn't available
- ✅ Log meaningful events (player actions, state changes, errors)

### DON'T:
- ❌ Don't use string interpolation: `_logger.LogInformation($"Player {name} leveled up")`
- ❌ Don't log sensitive data (passwords, tokens)
- ❌ Don't log in tight loops (performance impact)
- ❌ Don't reference Serilog directly in Game.Core (use abstractions)

## Future Enhancements

1. **Configuration-based log levels**: Read from `appsettings.json`
2. **Additional sinks**: Database, cloud logging (Azure App Insights, AWS CloudWatch)
3. **Enrichment**: Add correlation IDs, session IDs, player context
4. **Performance metrics**: Integration with MediatR `PerformanceBehavior`
5. **Audit trail**: Separate audit log for player actions (for anti-cheat/debugging)

## Files Modified

### Added/Modified
- `Game.Console/Program.cs` - Added `services.AddLogging()` with Serilog
- `Game.Console/Game.Console.csproj` - Added logging packages
- `Game.Core/Game.Core.csproj` - Added `Microsoft.Extensions.Logging.Abstractions`
- `Game.Core/Services/GameStateService.cs` - Converted to use `ILogger<T>`
- `Game.Tests/Helpers/LoggerHelper.cs` - Created helper for test loggers
- `Game.Tests/Services/MenuServiceTests.cs` - Updated to pass logger
- `Game.Tests/Services/CombatOrchestratorTests.cs` - Updated to pass logger
- `Game.Tests/Services/ExplorationServiceTests.cs` - Updated to pass logger

### Already Existed
- `Game.Console/Shared/Services/LoggingService.cs` - Serilog configuration
- `Game.Shared/Behaviors/LoggingBehavior.cs` - MediatR logging pipeline

## Testing

All tests pass with the new logging setup:
```bash
dotnet build Game.sln  # ✅ Success
dotnet test            # ✅ All tests pass
dotnet run --project Game.Console  # ✅ Game runs
```
