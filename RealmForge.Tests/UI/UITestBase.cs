using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Serilog;

namespace RealmForge.Tests.UI;

/// <summary>
/// Base class for UI tests with proper timeout handling and cleanup
/// Ensures applications are always closed, even if tests hang or fail
/// </summary>
public abstract class UITestBase : IDisposable
{
    protected Application? _app;
    protected UIA3Automation? _automation;
    protected Window? _mainWindow;
    private readonly CancellationTokenSource _testTimeoutCts;
    private readonly TimeSpan _testTimeout;
    private readonly Stopwatch _testStopwatch;
    private bool _disposed;

    /// <summary>
    /// Creates a new UI test base with default 30 second timeout per test
    /// </summary>
    protected UITestBase() : this(TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Creates a new UI test base with custom timeout
    /// </summary>
    /// <param name="testTimeout">Maximum time allowed for test execution</param>
    protected UITestBase(TimeSpan testTimeout)
    {
        _testTimeout = testTimeout;
        _testTimeoutCts = new CancellationTokenSource();
        _testStopwatch = Stopwatch.StartNew();

        // Configure logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("cb-test.log", rollingInterval: Serilog.RollingInterval.Day)
            .CreateLogger();
    }

    /// <summary>
    /// Launches the ContentBuilder application with timeout protection
    /// </summary>
    protected void LaunchApplication(TimeSpan? launchTimeout = null)
    {
        var timeout = launchTimeout ?? TimeSpan.FromSeconds(15);

        try
        {
            var testAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;
            var exePath = Path.Combine(
                testAssemblyPath,
                "..", "..", "..", "..",
                "RealmForge", "bin", "Debug", "net9.0-windows",
                "RealmForge.exe"
            );

            var fullExePath = Path.GetFullPath(exePath);

            if (!File.Exists(fullExePath))
            {
                throw new FileNotFoundException(
                    $"ContentBuilder executable not found at: {fullExePath}. " +
                    "Please build the Game.ContentBuilder project first.");
            }

            Log.Information("Launching ContentBuilder from: {ExePath}", fullExePath);

            _automation = new UIA3Automation();
            _app = Application.Launch(fullExePath);

            // Wait for main window with timeout
            var task = Task.Run(() =>
            {
                _mainWindow = _app.GetMainWindow(_automation, timeout);
                return _mainWindow;
            }, _testTimeoutCts.Token);

            if (!task.Wait(timeout.Add(TimeSpan.FromSeconds(2))))
            {
                Log.Error("Application launch timed out after {Timeout}s", timeout.TotalSeconds);
                ForceCleanup();
                throw new TimeoutException($"Application failed to launch within {timeout.TotalSeconds} seconds");
            }

            if (_mainWindow == null)
            {
                Log.Error("Main window is null after launch");
                ForceCleanup();
                throw new InvalidOperationException("Main window failed to load");
            }

            Log.Information("Application launched successfully. PID: {ProcessId}", _app.ProcessId);

            // Give UI time to stabilize
            Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch application");
            ForceCleanup();
            throw;
        }
    }

    /// <summary>
    /// Checks if the test has exceeded its timeout
    /// </summary>
    protected void CheckTimeout()
    {
        if (_testStopwatch.Elapsed > _testTimeout)
        {
            var message = $"Test exceeded timeout of {_testTimeout.TotalSeconds}s (actual: {_testStopwatch.Elapsed.TotalSeconds:F1}s)";
            Log.Error(message);
            throw new TimeoutException(message);
        }

        if (_testTimeoutCts.Token.IsCancellationRequested)
        {
            throw new OperationCanceledException("Test was cancelled due to timeout");
        }
    }

    /// <summary>
    /// Executes an action with timeout protection
    /// </summary>
    protected T ExecuteWithTimeout<T>(Func<T> action, TimeSpan? timeout = null, string? operationName = null)
    {
        CheckTimeout();

        var actualTimeout = timeout ?? TimeSpan.FromSeconds(10);
        var task = Task.Run(action, _testTimeoutCts.Token);

        if (!task.Wait((int)actualTimeout.TotalMilliseconds, _testTimeoutCts.Token))
        {
            var name = operationName ?? "Operation";
            Log.Error("{OperationName} timed out after {Timeout}s", name, actualTimeout.TotalSeconds);
            throw new TimeoutException($"{name} timed out after {actualTimeout.TotalSeconds} seconds");
        }

        return task.Result;
    }

    /// <summary>
    /// Executes an action with timeout protection (void return)
    /// </summary>
    protected void ExecuteWithTimeout(Action action, TimeSpan? timeout = null, string? operationName = null)
    {
        ExecuteWithTimeout<object?>(() =>
        {
            action();
            return null;
        }, timeout, operationName);
    }

    /// <summary>
    /// Forces immediate cleanup of all resources
    /// </summary>
    private void ForceCleanup()
    {
        if (_disposed) return;

        Log.Information("Starting force cleanup...");

        try
        {
            // Cancel any pending operations
            _testTimeoutCts.Cancel();

            // Try to close application gracefully
            if (_app != null && !_app.HasExited)
            {
                try
                {
                    Log.Information("Attempting graceful shutdown of PID {ProcessId}", _app.ProcessId);
                    _app.Close();
                    Thread.Sleep(2000); // Wait 2 seconds for graceful shutdown
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Graceful shutdown failed for PID {ProcessId}", _app?.ProcessId);
                }

                // If still running, force kill
                if (!_app.HasExited)
                {
                    try
                    {
                        Log.Warning("Force killing application PID {ProcessId}", _app.ProcessId);
                        _app.Kill();
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to kill application PID {ProcessId}", _app?.ProcessId);
                    }
                }
            }

            // Clean up automation
            _automation?.Dispose();
            _automation = null;

            // Clean up cancellation token
            _testTimeoutCts?.Dispose();

            _disposed = true;
            Log.Information("Force cleanup completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during force cleanup");
        }
    }

    /// <summary>
    /// Disposes resources - can be overridden for custom cleanup
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            Log.Information("Test completed in {ElapsedMs}ms", _testStopwatch.ElapsedMilliseconds);
            _testStopwatch.Stop();
        }

        ForceCleanup();
    }

    /// <summary>
    /// Disposes resources and ensures application is closed
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure cleanup even if Dispose is not called
    /// </summary>
    ~UITestBase()
    {
        Log.Warning("Finalizer called for UITestBase - Dispose was not called explicitly!");
        Dispose(false);
    }
}
