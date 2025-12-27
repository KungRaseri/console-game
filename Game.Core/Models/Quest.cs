using Game.Shared.Models;

namespace Game.Core.Models;

/// <summary>
/// Represents a quest that can be given to the player.
/// Implements ITraitable to support quest-specific traits from templates.
/// </summary>
public class Quest : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string QuestType { get; set; } = string.Empty; // kill, fetch, escort, investigate, delivery
    public string Difficulty { get; set; } = string.Empty; // easy, medium, hard
    public string Type { get; set; } = "side"; // "main", "side", "legendary" - For quest categorization in Phase 4

    // Quest giver
    public string QuestGiverId { get; set; } = string.Empty;
    public string QuestGiverName { get; set; } = string.Empty;

    // Objectives - Enhanced for Phase 4
    public string TargetType { get; set; } = string.Empty; // beast, undead, demon, etc.
    public string TargetName { get; set; } = string.Empty; // "Ancient Red Dragon"
    public int Quantity { get; set; } = 1;
    public string Location { get; set; } = string.Empty;

    // Phase 4: Quest prerequisites and objectives tracking
    public List<string> Prerequisites { get; set; } = new(); // Quest IDs that must be completed first
    public Dictionary<string, int> Objectives { get; set; } = new(); // Objective name -> required count
    public Dictionary<string, int> ObjectiveProgress { get; set; } = new(); // Objective name -> current count

    // Rewards
    public int GoldReward { get; set; }
    public int XpReward { get; set; }
    public List<string> ItemRewards { get; set; } = new();
    public int ApocalypseBonusMinutes { get; set; } = 0; // Bonus time for Apocalypse mode (Phase 4)

    // Status
    public bool IsActive { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public int Progress { get; set; } = 0; // Current count towards objective

    // Time limit (in hours, 0 = no limit)
    public int TimeLimit { get; set; } = 0;
    public DateTime? StartTime { get; set; }

    // Traits from quest template
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    /// <summary>
    /// Check if the quest has expired (time limit reached).
    /// </summary>
    public bool IsExpired()
    {
        if (TimeLimit <= 0 || !StartTime.HasValue)
            return false;

        var elapsed = DateTime.Now - StartTime.Value;
        return elapsed.TotalHours >= TimeLimit;
    }

    /// <summary>
    /// Get the formatted time remaining.
    /// </summary>
    public string GetTimeRemaining()
    {
        if (TimeLimit <= 0 || !StartTime.HasValue)
            return "No time limit";

        var elapsed = DateTime.Now - StartTime.Value;
        var remaining = TimeSpan.FromHours(TimeLimit) - elapsed;

        if (remaining.TotalHours < 0)
            return "Expired";

        if (remaining.TotalHours < 1)
            return $"{remaining.Minutes} minutes";

        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays} days, {remaining.Hours} hours";

        return $"{(int)remaining.TotalHours} hours";
    }

    /// <summary>
    /// Check if all quest objectives are complete (Phase 4 enhancement).
    /// </summary>
    public bool IsObjectivesComplete()
    {
        // If no objectives defined, fall back to legacy Progress check
        if (!Objectives.Any())
        {
            return Progress >= Quantity;
        }

        // Check all objectives are met
        foreach (var objective in Objectives)
        {
            if (!ObjectiveProgress.ContainsKey(objective.Key) ||
                ObjectiveProgress[objective.Key] < objective.Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Update progress for a specific objective (Phase 4 enhancement).
    /// </summary>
    public void UpdateObjectiveProgress(string objectiveName, int progressIncrement = 1)
    {
        if (!Objectives.ContainsKey(objectiveName))
        {
            return;
        }

        if (!ObjectiveProgress.ContainsKey(objectiveName))
        {
            ObjectiveProgress[objectiveName] = 0;
        }

        ObjectiveProgress[objectiveName] += progressIncrement;

        // Also update legacy Progress field for backwards compatibility
        Progress = ObjectiveProgress.Values.Sum();
    }
}
