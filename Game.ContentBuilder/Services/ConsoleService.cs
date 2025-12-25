using System.Collections.ObjectModel;
using System.Windows;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Global console service for logging across the application
/// </summary>
public static class ConsoleService
{
    public static ObservableCollection<string> Logs { get; } = new();
    public static event EventHandler? ConsoleToggled;

    public static void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        Application.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add($"[{timestamp}] {message}");
            
            // Keep only last 1000 messages
            while (Logs.Count > 1000)
            {
                Logs.RemoveAt(0);
            }
        });
    }

    public static void ToggleConsole()
    {
        ConsoleToggled?.Invoke(null, EventArgs.Empty);
    }
}
