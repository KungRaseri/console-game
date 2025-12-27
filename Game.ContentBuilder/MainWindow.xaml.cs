using System.Windows;
using System.Collections.ObjectModel;
using Game.ContentBuilder.ViewModels;
using Game.ContentBuilder.Models;

namespace Game.ContentBuilder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    private System.Text.StringBuilder _consoleBuffer = new System.Text.StringBuilder();

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;

        // Add initial log
        AddLog("Application started");
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainViewModel viewModel && e.NewValue is CategoryNode selectedNode)
        {
            viewModel.SelectedCategory = selectedNode;
        }
    }

    public static void AddLog(string message)
    {
        if (Instance != null)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Application.Current.Dispatcher.Invoke(() =>
            {
                var logLine = $"[{timestamp}] {message}\n";
                Instance._consoleBuffer.Append(logLine);

                // Keep only last ~50KB of logs (approximately 1000 lines)
                if (Instance._consoleBuffer.Length > 50000)
                {
                    var text = Instance._consoleBuffer.ToString();
                    var lines = text.Split('\n');
                    var keepLines = lines.Skip(Math.Max(0, lines.Length - 800)).ToArray();
                    Instance._consoleBuffer.Clear();
                    Instance._consoleBuffer.Append(string.Join("\n", keepLines));
                }

                // Update TextBox if console is open
                if (Instance.ConsoleTextBox != null)
                {
                    Instance.ConsoleTextBox.Text = Instance._consoleBuffer.ToString();
                    Instance.ConsoleTextBox.ScrollToEnd();
                }
            });
        }
    }

    private void ToggleConsole(object sender, RoutedEventArgs e)
    {
        ConsolePanel.Visibility = ConsolePanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (ConsolePanel.Visibility == Visibility.Visible)
        {
            // Refresh console content
            if (ConsoleTextBox != null)
            {
                ConsoleTextBox.Text = _consoleBuffer.ToString();
                ConsoleTextBox.ScrollToEnd();
            }
            AddLog("Console opened");
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.F12)
        {
            ToggleConsole(sender, e);
            e.Handled = true;
        }
        else if (e.Key == System.Windows.Input.Key.F5)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.RefreshCommand.Execute(null);
            }
            e.Handled = true;
        }
    }

    private void ToggleConsole_Click(object sender, RoutedEventArgs e)
    {
        ToggleConsole(sender, e);
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Game Content Builder\n\n" +
            "Version 1.0.0\n" +
            "Built with .NET 9.0 and Material Design\n\n" +
            "A visual editor for game content JSON files.\n" +
            "Use F12 to toggle the debug console.",
            "About Game Content Builder",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
