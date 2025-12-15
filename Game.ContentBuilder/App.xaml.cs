using System.IO;
using System.Windows;
using Serilog;

namespace Game.ContentBuilder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly string LogFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        "logs", 
        $"contentbuilder-{DateTime.Now:yyyyMMdd}.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Ensure logs directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
        
        // Initialize Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(LogFilePath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();
        
        // Log startup
        Log.Information("Application starting...");
        LogMessage("Application starting...");
        
        // Handle any unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] UNHANDLED EXCEPTION\n" +
                             $"Message: {args.Exception.Message}\n" +
                             $"Type: {args.Exception.GetType().FullName}\n" +
                             $"Stack Trace:\n{args.Exception.StackTrace}\n" +
                             $"Inner Exception: {args.Exception.InnerException?.Message ?? "None"}\n" +
                             new string('-', 80);
            
            Log.Error(args.Exception, "Unhandled dispatcher exception");
            LogMessage(errorMessage);
            
            MessageBox.Show(
                $"An error occurred: {args.Exception.Message}\n\n" +
                $"Details logged to: {LogFilePath}\n\n" +
                $"Stack Trace:\n{args.Exception.StackTrace}", 
                "Content Builder Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            
            args.Handled = true;
        };
        
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CRITICAL UNHANDLED EXCEPTION\n" +
                             $"Message: {ex?.Message ?? "Unknown"}\n" +
                             $"Type: {ex?.GetType().FullName ?? "Unknown"}\n" +
                             $"Stack Trace:\n{ex?.StackTrace ?? "No stack trace"}\n" +
                             $"Is Terminating: {args.IsTerminating}\n" +
                             new string('-', 80);
            
            LogMessage(errorMessage);
        };
        
        LogMessage("Application startup complete");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogMessage($"Application exiting with code: {e.ApplicationExitCode}");
        base.OnExit(e);
    }

    private static void LogMessage(string message)
    {
        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(LogFilePath, logEntry);
        }
        catch
        {
            // Ignore logging errors to prevent recursion
        }
    }
}

