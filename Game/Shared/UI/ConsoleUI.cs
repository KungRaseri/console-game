using Spectre.Console;
using Spectre.Console.Rendering;
using Game.Models;

namespace Game.Shared.UI;

/// <summary>
/// Comprehensive wrapper for Spectre.Console functionality.
/// Provides rich console UI features with security best practices.
/// All user input is automatically escaped to prevent markup injection.
/// </summary>
public static class ConsoleUI
{
    #region Private Helpers
    
    /// <summary>
    /// Escapes markup characters in a string to prevent markup injection.
    /// </summary>
    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }

    /// <summary>
    /// Parses a color string to a Spectre.Console Color.
    /// </summary>
    private static Color ParseColor(string colorName)
    {
        return colorName.ToLower() switch
        {
            "red" => Color.Red,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "yellow" => Color.Yellow,
            "cyan" => Color.Cyan,
            "magenta" => Color.Magenta,
            "white" => Color.White,
            "grey" or "gray" => Color.Grey,
            "orange" => Color.Orange1,
            "purple" => Color.Purple,
            "pink" => Color.Pink1,
            "lime" => Color.Lime,
            _ => Color.Default
        };
    }
    
    
    #endregion

    #region Banner & Text Output
    
    /// <summary>
    /// Displays a large ASCII art title using FigletText.
    /// </summary>
    public static void ShowTitle(string title, Color? color = null)
    {
        var figlet = new FigletText(EscapeMarkup(title))
            .Centered()
            .Color(color ?? Color.Yellow);
        
        AnsiConsole.Write(figlet);
        AnsiConsole.WriteLine();
    }
    
    /// <summary>
    /// Displays a horizontal rule with optional title.
    /// </summary>
    public static void ShowRule(string? title = null, string style = "blue")
    {
        var rule = title != null 
            ? new Rule($"[{style}]{EscapeMarkup(title)}[/]")
            : new Rule();
        
        rule.RuleStyle(style);
        AnsiConsole.Write(rule);
    }
    
    /// <summary>
    /// Displays a welcome banner with styled text.
    /// </summary>
    public static void ShowBanner(string title, string subtitle = "")
    {
        var rule = new Rule($"[bold yellow]{EscapeMarkup(title)}[/]");
        rule.RuleStyle("blue");
        AnsiConsole.Write(rule);

        if (!string.IsNullOrEmpty(subtitle))
        {
            AnsiConsole.MarkupLine($"[dim]{EscapeMarkup(subtitle)}[/]");
        }

        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Displays colored text with markup support.
    /// Note: This method does NOT escape markup - use it only with trusted/controlled text.
    /// For user input, use WriteText() instead.
    /// </summary>
    public static void WriteColoredText(string text)
    {
        AnsiConsole.MarkupLine(text);
    }

    /// <summary>
    /// Displays plain text safely (escapes markup characters).
    /// Use this for displaying user input or untrusted text.
    /// </summary>
    public static void WriteText(string text)
    {
        AnsiConsole.WriteLine(EscapeMarkup(text));
    }

    /// <summary>
    /// Asks the user for text input with a prompt.
    /// </summary>
    public static string AskForInput(string prompt)
    {
        return AnsiConsole.Ask<string>($"[green]{EscapeMarkup(prompt)}[/]");
    }

    /// <summary>
    /// Asks the user for numeric input with validation.
    /// </summary>
    public static int AskForNumber(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        // Escape markup in prompt to prevent injection
        var safePrompt = prompt.Replace("[", "[[").Replace("]", "]]");
        
        return AnsiConsole.Prompt(
            new TextPrompt<int>($"[green]{safePrompt}[/]")
                .ValidationErrorMessage($"[red]Please enter a number between {min} and {max}[/]")
                .Validate(n => n >= min && n <= max
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Number must be between {min} and {max}"))
        );
    }

    /// <summary>
    /// Shows a selection menu and returns the user's choice.
    /// </summary>
    public static string ShowMenu(string title, params string[] choices)
    {
        // Escape markup in title to prevent injection
        var safeTitle = title.Replace("[", "[[").Replace("]", "]]");
        
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{safeTitle}[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices(choices)
        );
    }

    /// <summary>
    /// Shows a multi-selection menu and returns the user's choices.
    /// </summary>
    public static List<string> ShowMultiSelectMenu(string title, params string[] choices)
    {
        // Escape markup in title to prevent injection
        var safeTitle = title.Replace("[", "[[").Replace("]", "]]");
        
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"[yellow]{safeTitle}[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                .AddChoices(choices)
        );
    }

    /// <summary>
    /// Asks a yes/no confirmation question.
    /// </summary>
    public static bool Confirm(string question)
    {
        // Escape markup in question to prevent injection
        var safeQuestion = question.Replace("[", "[[").Replace("]", "]]");
        return AnsiConsole.Confirm($"[yellow]{safeQuestion}[/]");
    }

    /// <summary>
    /// Displays a simple table with headers and rows.
    /// </summary>
    public static void ShowTable(string title, string[] headers, List<string[]> rows)
    {
        // Escape markup in title to prevent injection
        var safeTitle = title.Replace("[", "[[").Replace("]", "]]");
        
        var table = new Table();
        table.Title = new TableTitle($"[bold yellow]{safeTitle}[/]");
        table.Border = TableBorder.Rounded;

        foreach (var header in headers)
        {
            // Escape markup in headers to prevent injection
            var safeHeader = header.Replace("[", "[[").Replace("]", "]]");
            table.AddColumn($"[bold cyan]{safeHeader}[/]");
        }

        foreach (var row in rows)
        {
            // Escape markup in row data to prevent injection
            var safeRow = row.Select(cell => cell.Replace("[", "[[").Replace("]", "]]")).ToArray();
            table.AddRow(safeRow);
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Displays text inside a styled panel.
    /// Note: Content supports markup. Use EscapeMarkup for user input if needed.
    /// </summary>
    public static void ShowPanel(string title, string content, string borderColor = "blue")
    {
        var color = borderColor.ToLower() switch
        {
            "red" => Spectre.Console.Color.Red,
            "green" => Spectre.Console.Color.Green,
            "yellow" => Spectre.Console.Color.Yellow,
            "cyan" => Spectre.Console.Color.Cyan,
            "magenta" => Spectre.Console.Color.Magenta,
            "white" => Spectre.Console.Color.White,
            _ => Spectre.Console.Color.Blue
        };

        var panel = new Panel(content)
        {
            Header = new PanelHeader($"[bold]{EscapeMarkup(title)}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(color)
        };

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Displays a success message.
    /// </summary>
    public static void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {EscapeMarkup(message)}");
    }

    /// <summary>
    /// Displays an error message.
    /// </summary>
    public static void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗[/] {EscapeMarkup(message)}");
    }

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    public static void ShowWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]⚠[/] {EscapeMarkup(message)}");
    }

    /// <summary>
    /// Displays an info message.
    /// </summary>
    public static void ShowInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ[/] {EscapeMarkup(message)}");
    }

    /// <summary>
    /// Shows a progress bar while executing an action.
    /// </summary>
    public static void ShowProgress(string description, Action<ProgressTask> action)
    {
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[green]{description}[/]");
                action(task);
            });
    }

    #endregion

    #region Advanced UI Elements

    /// <summary>
    /// Shows a status spinner while executing an async operation.
    /// </summary>
    public static async Task<T> ShowSpinnerAsync<T>(string message, Func<Task<T>> asyncAction)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[yellow]{EscapeMarkup(message)}[/]", async ctx => 
            {
                return await asyncAction();
            });
    }

    /// <summary>
    /// Shows a status spinner (void) while executing an async operation.
    /// </summary>
    public static async Task ShowSpinnerAsync(string message, Func<Task> asyncAction)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[yellow]{EscapeMarkup(message)}[/]", async ctx => 
            {
                await asyncAction();
            });
    }

    /// <summary>
    /// Displays a tree structure for hierarchical data.
    /// </summary>
    public static void ShowTree(string rootLabel, Action<IHasTreeNodes> buildTree)
    {
        var tree = new Tree(EscapeMarkup(rootLabel));
        buildTree(tree);
        AnsiConsole.Write(tree);
    }

    /// <summary>
    /// Displays a bar chart.
    /// </summary>
    public static void ShowBarChart(string title, Dictionary<string, double> data, int width = 50)
    {
        var chart = new BarChart()
            .Width(width)
            .Label($"[bold yellow]{EscapeMarkup(title)}[/]");

        foreach (var item in data)
        {
            chart.AddItem(EscapeMarkup(item.Key), item.Value, Color.Green);
        }

        AnsiConsole.Write(chart);
    }

    /// <summary>
    /// Displays a breakdown chart (percentages).
    /// </summary>
    public static void ShowBreakdownChart(string title, Dictionary<string, double> data)
    {
        var chart = new BreakdownChart()
            .Width(60);

        foreach (var (key, value) in data)
        {
            chart.AddItem(EscapeMarkup(key), value, Color.Green);
        }

        AnsiConsole.Write(new Panel(chart)
        {
            Header = new PanelHeader($"[bold yellow]{EscapeMarkup(title)}[/]"),
            Border = BoxBorder.Rounded
        });
    }

    /// <summary>
    /// Displays a calendar for a specific month/year.
    /// </summary>
    public static void ShowCalendar(int year, int month, string title = "Calendar")
    {
        var calendar = new Calendar(year, month)
        {
            Border = TableBorder.Rounded,
            HeaderStyle = new Style(Color.Blue, Color.Default, Decoration.Bold)
        };

        AnsiConsole.Write(new Panel(calendar)
        {
            Header = new PanelHeader($"[bold]{EscapeMarkup(title)}[/]")
        });
    }

    /// <summary>
    /// Displays an exception with nice formatting.
    /// </summary>
    public static void ShowException(Exception ex)
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    }

    /// <summary>
    /// Creates a live display area that can be updated in real-time.
    /// </summary>
    public static async Task ShowLiveDisplayAsync(IRenderable initialDisplay, Func<LiveDisplayContext, Task> updateAction)
    {
        await AnsiConsole.Live(initialDisplay)
            .StartAsync(updateAction);
    }

    #endregion

    #region Layout & Columns

    /// <summary>
    /// Displays content in multiple columns.
    /// </summary>
    public static void ShowColumns(params IRenderable[] columns)
    {
        var columnLayout = new Columns(columns);
        AnsiConsole.Write(columnLayout);
    }

    /// <summary>
    /// Displays a grid layout with rows and columns.
    /// </summary>
    public static void ShowGrid(Action<Grid> buildGrid)
    {
        var grid = new Grid();
        buildGrid(grid);
        AnsiConsole.Write(grid);
    }

    /// <summary>
    /// Displays a 2-column combat layout with main content on left and combat log on right.
    /// </summary>
    public static void ShowCombatLayout(IRenderable mainContent, List<string> logEntries, string logTitle = "Combat Log", int maxLogLines = 15)
    {
        // Pad log entries to always show maxLogLines (fill with empty lines if needed)
        var paddedEntries = new List<string>();
        
        // Add existing entries
        paddedEntries.AddRange(logEntries);
        
        // Fill remaining space with empty lines to maintain consistent height
        while (paddedEntries.Count < maxLogLines)
        {
            paddedEntries.Add("[dim] [/]"); // Empty line with minimal markup
        }
        
        // Create the log content with fixed height and width
        var logContent = string.Join("\n", paddedEntries);

        var logPanel = new Panel(new Markup(logContent))
        {
            Header = new PanelHeader($"[bold yellow]{EscapeMarkup(logTitle)}[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Height = maxLogLines + 2, // +2 for panel borders
            Width = 50, // Fixed width to prevent shifting
            Padding = new Padding(1, 0, 1, 0) // Left/Right padding only
        };

        // Create columns with fixed widths
        var table = new Table()
        {
            Border = TableBorder.None,
            ShowHeaders = false,
            Expand = false // Don't expand to fill terminal width
        };

        // Use fixed column widths instead of percentages
        table.AddColumn(new TableColumn("").NoWrap());
        table.AddColumn(new TableColumn("").Width(50).NoWrap());

        table.AddRow(mainContent, logPanel);

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Shows a two-column layout (label: value) for displaying character stats, etc.
    /// </summary>
    public static void ShowKeyValueList(string title, Dictionary<string, string> items, string borderColor = "blue")
    {
        var table = new Table()
        {
            Border = TableBorder.Rounded,
            BorderStyle = new Style(ParseColor(borderColor))
        };

        if (!string.IsNullOrEmpty(title))
        {
            table.Title = new TableTitle($"[bold yellow]{EscapeMarkup(title)}[/]");
        }

        table.AddColumn(new TableColumn("[bold cyan]Property[/]").Width(20));
        table.AddColumn(new TableColumn("[bold green]Value[/]").Width(30));

        foreach (var (key, value) in items)
        {
            table.AddRow(EscapeMarkup(key), EscapeMarkup(value));
        }

        AnsiConsole.Write(table);
    }

    #endregion

    #region Character Stats Display

    /// <summary>
    /// Displays character stats using a Character model.
    /// </summary>
    public static void ShowCharacterStats(Character character)
    {
        ShowCharacterStats(
            character.Name,
            character.Level,
            character.Health,
            character.MaxHealth,
            character.Mana,
            character.MaxMana,
            character.Gold,
            character.Experience
        );
    }

    /// <summary>
    /// Displays character stats in a beautiful panel.
    /// </summary>
    public static void ShowCharacterStats(string name, int level, int health, int maxHealth, 
        int mana, int maxMana, int gold, int experience)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();

        // Health bar
        var healthPercent = (double)health / maxHealth * 100;
        string healthColor = "red";
        if (healthPercent > 50) healthColor = "green";
        else if (healthPercent > 25) healthColor = "yellow";
        
        grid.AddRow(
            new Markup("[bold cyan]Health:[/]"),
            new Markup($"[{healthColor}]{health}/{maxHealth}[/]")
        );

        // Mana bar  
        grid.AddRow(
            new Markup("[bold cyan]Mana:[/]"),
            new Markup($"[blue]{mana}/{maxMana}[/]")
        );

        // Level
        grid.AddRow(
            new Markup("[bold cyan]Level:[/]"),
            new Markup($"[yellow]{level}[/]")
        );

        // Experience
        grid.AddRow(
            new Markup("[bold cyan]Experience:[/]"),
            new Markup($"[magenta]{experience} XP[/]")
        );

        // Gold
        grid.AddRow(
            new Markup("[bold cyan]Gold:[/]"),
            new Markup($"[gold1]{gold} ⚜[/]")
        );

        var panel = new Panel(grid)
        {
            Header = new PanelHeader($"[bold green]{EscapeMarkup(name)}[/]"),
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Shows a health bar with percentage.
    /// </summary>
    public static void ShowHealthBar(int current, int max, string label = "Health", int width = 40)
    {
        var percentage = (double)current / max;
        Color color = Color.Red;
        if (percentage > 0.5) color = Color.Green;
        else if (percentage > 0.25) color = Color.Yellow;

        var bar = new BreakdownChart()
            .Width(width)
            .AddItem(label, current, color)
            .AddItem("", max - current, Color.Grey);

        AnsiConsole.Write(bar);
        AnsiConsole.MarkupLine($"[{color.ToString().ToLower()}]{current}/{max} ({percentage:P0})[/]");
    }

    #endregion

    #region Prompts & Dialogs

    /// <summary>
    /// Shows a menu with custom item objects (displays a property as the label).
    /// </summary>
    public static T ShowObjectMenu<T>(string title, IEnumerable<T> items, Func<T, string> labelSelector) where T : notnull
    {
        var safeTitle = EscapeMarkup(title);
        
        return AnsiConsole.Prompt(
            new SelectionPrompt<T>()
                .Title($"[yellow]{safeTitle}[/]")
                .PageSize(10)
                .UseConverter(item => EscapeMarkup(labelSelector(item)))
                .AddChoices(items)
        );
    }

    /// <summary>
    /// Shows a password input prompt (masked).
    /// </summary>
    public static string AskForPassword(string prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]{EscapeMarkup(prompt)}[/]")
                .PromptStyle("red")
                .Secret()
        );
    }

    /// <summary>
    /// Shows a text input with default value.
    /// </summary>
    public static string AskForInputWithDefault(string prompt, string defaultValue)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]{EscapeMarkup(prompt)}[/]")
                .DefaultValue(defaultValue)
                .ShowDefaultValue(true)
        );
    }

    /// <summary>
    /// Shows a validated text input with custom validation.
    /// </summary>
    public static string AskForInputWithValidation(string prompt, Func<string, ValidationResult> validator)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[green]{EscapeMarkup(prompt)}[/]")
                .Validate(validator)
        );
    }

    #endregion

    #region Utility

    /// <summary>
    /// Clears the console screen.
    /// </summary>
    public static void Clear()
    {
        AnsiConsole.Clear();
    }

    /// <summary>
    /// Waits for the user to press any key.
    /// </summary>
    public static void PressAnyKey(string message = "Press any key to continue...")
    {
        AnsiConsole.MarkupLine($"[dim]{message}[/]");
        Console.ReadKey(true);
    }

    /// <summary>
    /// Writes a blank line.
    /// </summary>
    public static void WriteLine()
    {
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Writes multiple blank lines.
    /// </summary>
    public static void WriteLines(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AnsiConsole.WriteLine();
        }
    }

    /// <summary>
    /// Sets the console title.
    /// </summary>
    public static void SetTitle(string title)
    {
        Console.Title = title;
    }

    /// <summary>
    /// Shows a divider line.
    /// </summary>
    public static void ShowDivider(string character = "─", string color = "grey")
    {
        var width = Console.WindowWidth;
        AnsiConsole.MarkupLine($"[{color}]{new string(character[0], width)}[/]");
    }

    #endregion
}
