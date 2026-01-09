namespace RealmEngine.Shared.Models;

/// <summary>
/// Tracks combat events in a rolling log.
/// </summary>
public class CombatLog
{
    private readonly List<CombatLogEntry> _entries = new();
    private readonly int _maxEntries;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatLog"/> class.
    /// </summary>
    /// <param name="maxEntries">Maximum number of entries to retain in the log.</param>
    public CombatLog(int maxEntries = 15)
    {
        _maxEntries = maxEntries;
    }

    /// <summary>Gets the read-only list of combat log entries.</summary>
    public IReadOnlyList<CombatLogEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// Adds a new entry to the combat log.
    /// Automatically trims old entries if max is exceeded.
    /// </summary>
    public void AddEntry(string message, CombatLogType type = CombatLogType.Info)
    {
        _entries.Add(new CombatLogEntry
        {
            Message = message,
            Type = type,
            Timestamp = DateTime.Now
        });

        // Keep only the most recent entries
        if (_entries.Count > _maxEntries)
        {
            _entries.RemoveAt(0);
        }
    }

    /// <summary>
    /// Clears all log entries.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }

    /// <summary>
    /// Gets formatted log entries for display.
    /// </summary>
    public List<string> GetFormattedEntries()
    {
        var formatted = new List<string>();

        foreach (var entry in _entries)
        {
            var color = entry.Type switch
            {
                CombatLogType.PlayerAttack => "green",
                CombatLogType.EnemyAttack => "red",
                CombatLogType.Critical => "orange1",
                CombatLogType.Dodge => "yellow",
                CombatLogType.Heal => "cyan",
                CombatLogType.Defend => "blue",
                CombatLogType.ItemUse => "purple",
                CombatLogType.Victory => "lime",
                CombatLogType.Defeat => "red",
                _ => "dim"
            };

            formatted.Add($"[{color}]{entry.Message}[/]");
        }

        return formatted;
    }
}

/// <summary>
/// Represents a single entry in the combat log.
/// </summary>
public class CombatLogEntry
{
    /// <summary>Gets or initializes the log message.</summary>
    public required string Message { get; init; }
    /// <summary>Gets or initializes the type of combat log entry.</summary>
    public required CombatLogType Type { get; init; }
    /// <summary>Gets or initializes the timestamp when the entry was created.</summary>
    public required DateTime Timestamp { get; init; }
}

/// <summary>
/// Types of combat log entries.
/// </summary>
public enum CombatLogType
{
    /// <summary>Informational message.</summary>
    Info,
    /// <summary>Player attack action.</summary>
    PlayerAttack,
    /// <summary>Enemy attack action.</summary>
    EnemyAttack,
    /// <summary>Critical hit.</summary>
    Critical,
    /// <summary>Dodge action.</summary>
    Dodge,
    /// <summary>Healing action.</summary>
    Heal,
    /// <summary>Defend action.</summary>
    Defend,
    /// <summary>Item use action.</summary>
    ItemUse,
    /// <summary>Ability use action.</summary>
    AbilityUse,
    /// <summary>Spell cast action.</summary>
    SpellCast,
    /// <summary>Victory message.</summary>
    Victory,
    /// <summary>Defeat message.</summary>
    Defeat
}
