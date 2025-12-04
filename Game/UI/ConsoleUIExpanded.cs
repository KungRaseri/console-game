using Spectre.Console;
using Spectre.Console.Rendering;
using Game.Models;

namespace Game.UI;

/// <summary>
/// Comprehensive wrapper for Spectre.Console functionality.
/// Provides rich console UI features with security best practices.
/// All user input is automatically escaped to prevent markup injection.
/// </summary>
public static class ConsoleUIExpanded
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

    #region Advanced UI Elements

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

    #region Character Display

    /// <summary>
    /// Displays character stats using a Character model.
    /// </summary>
    public static void ShowCharacterStats(Character character)
    {
        ShowCharacterStatsDetailed(
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
    private static void ShowCharacterStatsDetailed(
        string name, int level, int health, int maxHealth, 
        int mana, int maxMana, int gold, int experience)
    {
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();

        // Health bar
        var healthPercent = (double)health / maxHealth * 100;
        var healthColor = GetHealthColor(healthPercent);
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

    private static string GetHealthColor(double healthPercent)
    {
        if (healthPercent > 50)
            return "green";
        if (healthPercent > 25)
            return "yellow";
        return "red";
    }

    /// <summary>
    /// Shows a health bar with percentage.
    /// </summary>
    public static void ShowHealthBar(int current, int max, string label = "Health", int width = 40)
    {
        var percentage = (double)current / max;
        var color = GetHealthColorEnum(percentage);

        var bar = new BreakdownChart()
            .Width(width)
            .AddItem(label, current, color)
            .AddItem("", max - current, Color.Grey);

        AnsiConsole.Write(bar);
        AnsiConsole.MarkupLine($"[{color.ToString().ToLower()}]{current}/{max} ({percentage:P0})[/]");
    }

    private static Color GetHealthColorEnum(double percentage)
    {
        if (percentage > 0.5)
            return Color.Green;
        if (percentage > 0.25)
            return Color.Yellow;
        return Color.Red;
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

    /// <summary>
    /// Shows a horizontal rule with optional title.
    /// </summary>
    public static void ShowRule(string? title = null, string style = "blue")
    {
        var rule = title != null 
            ? new Rule($"[{style}]{EscapeMarkup(title)}[/]")
            : new Rule();
        
        rule.RuleStyle(style);
        AnsiConsole.Write(rule);
    }

    #endregion
}
