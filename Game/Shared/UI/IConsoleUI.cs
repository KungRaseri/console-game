using Spectre.Console;
using Spectre.Console.Rendering;
using Game.Models;

namespace Game.Shared.UI;

/// <summary>
/// Interface for console UI operations, enabling dependency injection and testing.
/// Provides rich console UI features with security best practices.
/// All user input is automatically escaped to prevent markup injection.
/// </summary>
public interface IConsoleUI
{
    #region Utility Methods
    
    /// <summary>
    /// Strip Spectre.Console markup from text for length calculations.
    /// </summary>
    string StripMarkup(string text);
    
    #endregion
    
    #region Display Methods
    
    /// <summary>
    /// Displays a large centered title using FigletText.
    /// </summary>
    void ShowTitle(string title, Color? color = null);
    
    /// <summary>
    /// Displays a horizontal rule with optional title.
    /// </summary>
    void ShowRule(string? title = null, string style = "blue");
    
    /// <summary>
    /// Displays a banner with title and subtitle.
    /// </summary>
    void ShowBanner(string title, string subtitle = "");
    
    /// <summary>
    /// Writes colored text with markup support (TRUSTED CONTENT ONLY).
    /// </summary>
    void WriteColoredText(string text);
    
    /// <summary>
    /// Writes plain text (auto-escapes markup).
    /// </summary>
    void WriteText(string text);
    
    #endregion
    
    #region Input Methods
    
    /// <summary>
    /// Asks for text input with a prompt.
    /// </summary>
    string AskForInput(string prompt);
    
    /// <summary>
    /// Asks for numeric input with validation.
    /// </summary>
    int AskForNumber(string prompt, int min = int.MinValue, int max = int.MaxValue);
    
    /// <summary>
    /// Displays a menu and returns the selected choice.
    /// </summary>
    string ShowMenu(string title, params string[] choices);
    
    /// <summary>
    /// Displays a multi-select menu and returns selected choices.
    /// </summary>
    List<string> ShowMultiSelectMenu(string title, params string[] choices);
    
    /// <summary>
    /// Asks for yes/no confirmation.
    /// </summary>
    bool Confirm(string question);
    
    /// <summary>
    /// Displays a menu of objects and returns the selected item.
    /// </summary>
    T ShowObjectMenu<T>(string title, IEnumerable<T> items, Func<T, string> labelSelector) where T : notnull;
    
    /// <summary>
    /// Asks for password input (hidden characters).
    /// </summary>
    string AskForPassword(string prompt);
    
    /// <summary>
    /// Asks for input with a default value.
    /// </summary>
    string AskForInputWithDefault(string prompt, string defaultValue);
    
    /// <summary>
    /// Asks for input with custom validation.
    /// </summary>
    string AskForInputWithValidation(string prompt, Func<string, ValidationResult> validator);
    
    #endregion
    
    #region Layout & Display
    
    /// <summary>
    /// Displays a table with headers and rows.
    /// </summary>
    void ShowTable(string title, string[] headers, List<string[]> rows);
    
    /// <summary>
    /// Displays a bordered panel with title and content.
    /// </summary>
    void ShowPanel(string title, string content, string borderColor = "blue");
    
    /// <summary>
    /// Displays a success message.
    /// </summary>
    void ShowSuccess(string message);
    
    /// <summary>
    /// Displays an error message.
    /// </summary>
    void ShowError(string message);
    
    /// <summary>
    /// Displays a warning message.
    /// </summary>
    void ShowWarning(string message);
    
    /// <summary>
    /// Displays an info message.
    /// </summary>
    void ShowInfo(string message);
    
    /// <summary>
    /// Displays a progress bar while executing an action.
    /// </summary>
    void ShowProgress(string description, Action<ProgressTask> action);
    
    /// <summary>
    /// Displays a spinner while executing an async action.
    /// </summary>
    Task<T> ShowSpinnerAsync<T>(string message, Func<Task<T>> asyncAction);
    
    /// <summary>
    /// Displays a spinner while executing an async action.
    /// </summary>
    Task ShowSpinnerAsync(string message, Func<Task> asyncAction);
    
    /// <summary>
    /// Displays a tree structure.
    /// </summary>
    void ShowTree(string rootLabel, Action<IHasTreeNodes> buildTree);
    
    /// <summary>
    /// Displays a bar chart.
    /// </summary>
    void ShowBarChart(string title, Dictionary<string, double> data, int width = 50);
    
    /// <summary>
    /// Displays a breakdown chart.
    /// </summary>
    void ShowBreakdownChart(string title, Dictionary<string, double> data);
    
    /// <summary>
    /// Displays a calendar for a specific month/year.
    /// </summary>
    void ShowCalendar(int year, int month, string title = "Calendar");
    
    /// <summary>
    /// Displays formatted exception information.
    /// </summary>
    void ShowException(Exception ex);
    
    /// <summary>
    /// Displays live updating content.
    /// </summary>
    Task ShowLiveDisplayAsync(IRenderable initialDisplay, Func<LiveDisplayContext, Task> updateAction);
    
    /// <summary>
    /// Displays multiple columns side-by-side.
    /// </summary>
    void ShowColumns(params IRenderable[] columns);
    
    /// <summary>
    /// Displays a customizable grid.
    /// </summary>
    void ShowGrid(Action<Grid> buildGrid);
    
    /// <summary>
    /// Displays a combat layout with main content and log.
    /// </summary>
    void ShowCombatLayout(IRenderable mainContent, List<string> logEntries, string logTitle = "Combat Log", int maxLogLines = 15);
    
    /// <summary>
    /// Displays a list of key-value pairs in a panel.
    /// </summary>
    void ShowKeyValueList(string title, Dictionary<string, string> items, string borderColor = "blue");
    
    #endregion
    
    #region Game-Specific Display
    
    /// <summary>
    /// Displays character stats using Character model.
    /// </summary>
    void ShowCharacterStats(Character character);
    
    /// <summary>
    /// Displays character stats using individual parameters.
    /// </summary>
    void ShowCharacterStats(string name, int level, int health, int maxHealth, 
        int mana, int maxMana, int gold, int experience);
    
    /// <summary>
    /// Displays a health bar.
    /// </summary>
    void ShowHealthBar(int current, int max, string label = "Health", int width = 40);
    
    #endregion
    
    #region Console Control
    
    /// <summary>
    /// Clears the console screen.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Waits for any key press.
    /// </summary>
    void PressAnyKey(string message = "Press any key to continue...");
    
    /// <summary>
    /// Writes a blank line.
    /// </summary>
    void WriteLine();
    
    /// <summary>
    /// Writes multiple blank lines.
    /// </summary>
    void WriteLines(int count);
    
    /// <summary>
    /// Sets the console window title.
    /// </summary>
    void SetTitle(string title);
    
    /// <summary>
    /// Shows a horizontal divider line.
    /// </summary>
    void ShowDivider(string character = "─", string color = "grey");
    
    #endregion
}
