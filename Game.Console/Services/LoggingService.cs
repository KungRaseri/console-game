using Serilog;
using Serilog.Events;

namespace Game.Console.Services;

/// <summary>
/// Configures and manages Serilog logging for the game.
/// </summary>
public static class LoggingService
{
    /// <summary>
    /// Initialize Serilog with console and file sinks.
    /// Logs are written to the application's base directory (bin folder when running).
    /// </summary>
    public static void Initialize()
    {
        // Use AppContext.BaseDirectory to ensure logs go to bin folder
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "game-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Game logging initialized to {LogPath}", Path.GetDirectoryName(logPath));
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
