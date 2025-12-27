using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Serilog;
using Xunit;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Shared fixture for all UI tests in the collection.
/// Launches the application once before all tests and closes it after all tests complete.
/// </summary>
public class UITestCollectionFixture : IDisposable
{
    public Application? App { get; private set; }
    public UIA3Automation? Automation { get; private set; }
    public Window? MainWindow { get; private set; }
    private bool _disposed;

    public UITestCollectionFixture()
    {
        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("cb-test.log", rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();

        Log.Information("UITestCollectionFixture: Launching application...");
        LaunchApplication();
    }

    private void LaunchApplication()
    {
        try
        {
            // Kill any existing instances
            var existingProcesses = Process.GetProcessesByName("Game.ContentBuilder");
            foreach (var proc in existingProcesses)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit(2000);
                }
                catch { /* ignore */ }
            }

            Thread.Sleep(500);

            // Find the executable (walk up from test bin folder to solution root)
            var currentDir = Directory.GetCurrentDirectory();
            var solutionRoot = currentDir;

            // Walk up until we find Game.sln
            while (!File.Exists(Path.Combine(solutionRoot, "Game.sln")) && Directory.GetParent(solutionRoot) != null)
            {
                solutionRoot = Directory.GetParent(solutionRoot)!.FullName;
            }

            var exePath = Path.Combine(solutionRoot, "Game.ContentBuilder", "bin", "Debug", "net9.0-windows", "Game.ContentBuilder.exe");

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException($"Could not find Game.ContentBuilder.exe at: {exePath}. Current directory: {currentDir}, Solution root: {solutionRoot}");
            }

            Log.Information($"Launching from: {exePath}");

            // Launch application
            Automation = new UIA3Automation();
            App = Application.Launch(exePath);

            // Wait for main window with timeout
            var stopwatch = Stopwatch.StartNew();
            var timeout = TimeSpan.FromSeconds(15);

            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(2));
                    if (MainWindow != null && !MainWindow.IsOffscreen)
                    {
                        Log.Information("Main window found and ready");
                        Thread.Sleep(1000); // Additional stabilization
                        return;
                    }
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            throw new TimeoutException("Failed to get main window within timeout period");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch application");
            Cleanup();
            throw;
        }
    }

    private void Cleanup()
    {
        try
        {
            MainWindow?.Close();
            Thread.Sleep(500);
        }
        catch { /* ignore */ }

        try
        {
            App?.Close();
            Thread.Sleep(500);
        }
        catch { /* ignore */ }

        try
        {
            Automation?.Dispose();
        }
        catch { /* ignore */ }

        // Force kill any remaining processes
        var remainingProcesses = Process.GetProcessesByName("Game.ContentBuilder");
        foreach (var proc in remainingProcesses)
        {
            try
            {
                proc.Kill();
                proc.WaitForExit(2000);
            }
            catch { /* ignore */ }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Log.Information("UITestCollectionFixture: Closing application...");
        Cleanup();
    }
}

/// <summary>
/// Defines the UI test collection that shares the UITestCollectionFixture
/// </summary>
[CollectionDefinition("UI Tests")]
public class UITestCollection : ICollectionFixture<UITestCollectionFixture>
{
    // This class is never instantiated, it's just used to define the collection
}
