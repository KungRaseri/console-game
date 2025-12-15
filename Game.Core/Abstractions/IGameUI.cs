namespace Game.Core.Abstractions;

/// <summary>
/// Abstraction for game UI operations. Implementations can be console, web, mobile, etc.
/// This allows Game.Core to remain UI-agnostic.
/// </summary>
public interface IGameUI
{
    void ShowMessage(string message);
    void WriteText(string text);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
    void ShowBanner(string title, string subtitle = "");
    void ShowProgress(string title, Action<object> work); // Simplified progress API
    string AskForInput(string prompt);
    int AskForNumber(string prompt, int min, int max);
    bool Confirm(string question);
    string ShowMenu(string title, params string[] options);
    List<string> ShowMultiSelectMenu(string title, params string[] options);
    void ShowTable(string title, string[] headers, List<string[]> rows);
    void ShowPanel(string title, string content, string color = "white");
    void Clear();
    void PressAnyKey(string message = "Press any key to continue...");
}
