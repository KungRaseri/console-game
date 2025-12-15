using Spectre.Console;
using Spectre.Console.Testing;

namespace Game.Tests.Helpers;

/// <summary>
/// Helper class for testing console interactions using Spectre.Console.Testing.
/// Simplifies TestConsole setup and input simulation.
/// </summary>
public static class TestConsoleHelper
{
    /// <summary>
    /// Creates a TestConsole instance for interactive testing.
    /// TestConsole captures output and allows simulating user input.
    /// </summary>
    public static TestConsole CreateInteractiveConsole()
    {
        return new TestConsole()
            .Width(120)
            .Height(40)
            .Interactive()
            .EmitAnsiSequences();
    }

    /// <summary>
    /// Simulates selecting a menu option by index (0-based).
    /// For Spectre.Console menus, this sends arrow keys and Enter.
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <param name="index">The 0-based index of the menu option to select</param>
    public static void SelectMenuOption(TestConsole console, int index)
    {
        // Move down 'index' times (arrow key navigation)
        for (int i = 0; i < index; i++)
        {
            console.Input.PushKey(ConsoleKey.DownArrow);
        }
        
        // Press Enter to confirm selection
        console.Input.PushKey(ConsoleKey.Enter);
    }

    /// <summary>
    /// Simulates entering text input and pressing Enter.
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <param name="text">The text to enter</param>
    public static void EnterText(TestConsole console, string text)
    {
        console.Input.PushTextWithEnter(text);
    }

    /// <summary>
    /// Simulates answering a yes/no confirmation prompt.
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <param name="confirm">True for Yes, False for No</param>
    public static void ConfirmPrompt(TestConsole console, bool confirm)
    {
        if (confirm)
        {
            console.Input.PushKey(ConsoleKey.Enter); // Default is usually "Yes"
        }
        else
        {
            console.Input.PushKey(ConsoleKey.LeftArrow); // Move to "No"
            console.Input.PushKey(ConsoleKey.Enter);
        }
    }

    /// <summary>
    /// Simulates pressing any key (for PressAnyKey prompts).
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    public static void PressAnyKey(TestConsole console)
    {
        console.Input.PushKey(ConsoleKey.Enter);
    }

    /// <summary>
    /// Simulates selecting multiple items from a multi-select menu.
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <param name="indices">Array of 0-based indices to select</param>
    public static void SelectMultipleMenuOptions(TestConsole console, params int[] indices)
    {
        foreach (var index in indices)
        {
            // Navigate to the option
            for (int i = 0; i < index; i++)
            {
                console.Input.PushKey(ConsoleKey.DownArrow);
            }
            
            // Toggle selection with Spacebar
            console.Input.PushKey(ConsoleKey.Spacebar);
            
            // Move back to start for next selection
            for (int i = 0; i < index; i++)
            {
                console.Input.PushKey(ConsoleKey.UpArrow);
            }
        }
        
        // Confirm with Enter
        console.Input.PushKey(ConsoleKey.Enter);
    }

    /// <summary>
    /// Gets the output text from the TestConsole, useful for assertions.
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <returns>The complete console output as a string</returns>
    public static string GetOutput(TestConsole console)
    {
        return console.Output;
    }

    /// <summary>
    /// Checks if the console output contains the expected text (case-insensitive).
    /// </summary>
    /// <param name="console">The TestConsole instance</param>
    /// <param name="expectedText">The text to search for</param>
    /// <returns>True if the output contains the text</returns>
    public static bool OutputContains(TestConsole console, string expectedText)
    {
        return console.Output.Contains(expectedText, StringComparison.OrdinalIgnoreCase);
    }
}
