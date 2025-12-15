using System.IO;
using System.Windows;
using Serilog;

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
    
    private static readonly string _latestLogPath = Path.Combine(
        _logsDirectory,
        "contentbuilder.latest.log");
    
    private static string? _finalLogPath;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Ensure logs directory exists
        Directory.CreateDirectory(_logsDirectory);
        
        // Delete existing .latest.log if it exists
        if (File.Exists(_latestLogPath))
        {
            try
            {
                File.Delete(_latestLogPath);
            }
            catch
            {
                // Ignore if we can't delete the old log
            }
        }
        
        // Initialize Serilog with the .latest.log file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                _latestLogPath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.Console()
            .CreateLogger();
        
        // Log startup
        Log.Information("==========================================================");
        Log.Information("ContentBuilder Application Starting");
        Log.Information("Start Time: {StartTime}", _startTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Log.Information("Log File: {LogFile}", _latestLogPath);
        Log.Information("==========================================================");
        
        // Handle any unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "Unhandled dispatcher exception");
            
            MessageBox.Show(
                $"An error occurred: {args.Exception.Message}\n\n" +
                $"Details logged to: {_latestLogPath}\n\n" +
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
        
        Log.Information("==========================================================");
        Log.Information("Application Exiting");
        Log.Information("Exit Code: {ExitCode}", e.ApplicationExitCode);
        Log.Information("Exit Time: {ExitTime}", exitTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Log.Information("Session Duration: {Duration}", duration.ToString(@"hh\:mm\:ss"));
        Log.Information("==========================================================");
        
        // Flush and close Serilog
        Log.CloseAndFlush();
        
        // Rename the .latest.log file to timestamped version
        try
        {
            if (File.Exists(_latestLogPath))
            {
                _finalLogPath = Path.Combine(
                    _logsDirectory,
                    $"contentbuilder-{_startTime:yyyyMMdd-HHmmss}.log");
                
                // If a file with this name already exists, add a counter
                var counter = 1;
                var basePath = _finalLogPath;
                while (File.Exists(_finalLogPath))
                {
                    _finalLogPath = Path.Combine(
                        _logsDirectory,
                        $"contentbuilder-{_startTime:yyyyMMdd-HHmmss}-{counter}.log");
                    counter++;
                }
                
                File.Move(_latestLogPath, _finalLogPath);
            }
        }
        catch (Exception ex)
        {
            // If we can't rename, just leave it as .latest.log
            // We can't log this since Serilog is already closed
            System.Diagnostics.Debug.WriteLine($"Failed to rename log file: {ex.Message}");
        }
        
        base.OnExit(e);
    }
}

