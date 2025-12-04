using Serilog;
using Serilog.Events;

namespace Game.Services;

/// <summary>
/// Configures and manages Serilog logging for the game.
/// </summary>
public static class LoggingService
{
    /// <summary>
    /// Initialize Serilog with console and file sinks.
    /// </summary>
    public static void Initialize()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/game-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Game logging initialized");
    }

    /// <summary>
    /// Close and flush the logger.
    /// </summary>
    public static void Shutdown()
    {
        Log.Information("Game shutting down");
        Log.CloseAndFlush();
    }
}
