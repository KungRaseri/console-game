using System.IO;
using System.Windows;
using Serilog;
using Serilog.Events;

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
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                Path.Combine(_logsDirectory, "contentbuilder-error-.log"),
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.Console()
            .CreateLogger();
        
        // Log startup
        Log.Information("ContentBuilder Application Starting - {StartTime}", _startTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Log.Information("Logs Directory: {LogsDir}", _logsDirectory);
        
        // Handle any unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "Unhandled dispatcher exception");
            
            MessageBox.Show(
                $"An error occurred: {args.Exception.Message}\n\n" +
                $"Check error log in: {_logsDirectory}\n\n" +
                $"Stack Trace:\n{args.Exception.StackTrace}", 
                "Content Builder Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            
            args.Handled = true;
        };
        
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            Log.Fatal(ex, "Critical unhandled exception - Is Terminating: {IsTerminating}", args.IsTerminating);
        };
        
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
}

