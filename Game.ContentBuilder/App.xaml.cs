using System.IO;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using Serilog.Events;
using Game.ContentBuilder.Services;

namespace Game.ContentBuilder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly DateTime _startTime = DateTime.Now;
    private static readonly string _logsDirectory = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "logs");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure logs directory exists
        Directory.CreateDirectory(_logsDirectory);

        // Clean up old log files on startup
        CleanupOldLogs("contentbuilder-*.log", retainCount: 10);
        CleanupOldLogs("contentbuilder-error-*.log", retainCount: 3);

        // Configure Serilog with dual rolling file appenders
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(_logsDirectory, "contentbuilder-.log"),
                rollingInterval: RollingInterval.Minute,
                retainedFileCountLimit: 10,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                Path.Combine(_logsDirectory, "contentbuilder-error-.log"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Minute,
                retainedFileCountLimit: 3,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.Console()
            .WriteTo.Sink(new ConsoleSink())
            .CreateLogger();

        // Log startup
        Log.Information("ContentBuilder Application Starting - {StartTime}", _startTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Log.Information("Logs Directory: {LogsDir}", _logsDirectory);

        // Setup global exception handlers
        SetupGlobalExceptionHandlers();

        Log.Information("Application startup complete");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var exitTime = DateTime.Now;
        var duration = exitTime - _startTime;

        Log.Information("Application Exiting - Exit Code: {ExitCode}, Duration: {Duration}",
            e.ApplicationExitCode,
            duration.ToString(@"hh\:mm\:ss"));

        // Flush and close Serilog
        Log.CloseAndFlush();

        base.OnExit(e);
    }

    private static void CleanupOldLogs(string searchPattern, int retainCount)
    {
        try
        {
            var logFiles = Directory.GetFiles(_logsDirectory, searchPattern)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();

            if (logFiles.Count > retainCount)
            {
                var filesToDelete = logFiles.Skip(retainCount);
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        System.Diagnostics.Debug.WriteLine($"Deleted old log: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to delete {file.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to cleanup logs ({searchPattern}): {ex.Message}");
        }
    }

    /// <summary>
    /// Setup global exception handlers to catch all unhandled exceptions
    /// </summary>
    private void SetupGlobalExceptionHandlers()
    {
        // Catch unhandled exceptions on UI thread
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Catch unhandled exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Catch unobserved task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        Log.Information("Global exception handlers configured");
    }

    /// <summary>
    /// Handles unhandled exceptions on the UI thread (Dispatcher)
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI thread exception");

        var errorMsg = $"An error occurred on the UI thread:\n\n{e.Exception.Message}\n\n" +
                      $"Type: {e.Exception.GetType().Name}\n\n" +
                      $"Check the error log for details:\n{_logsDirectory}";

        MessageBox.Show(errorMsg, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);

        // Prevent application crash (mark as handled)
        e.Handled = true;
    }

    /// <summary>
    /// Handles unhandled exceptions on background threads
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        Log.Fatal(exception, "Unhandled background thread exception - IsTerminating: {IsTerminating}", e.IsTerminating);

        if (e.IsTerminating)
        {
            // Try to flush logs before termination
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Handles unobserved task exceptions
    /// </summary>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");

        // Prevent the exception from terminating the application
        e.SetObserved();
    }
}

