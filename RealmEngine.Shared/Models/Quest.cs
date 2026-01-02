namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a quest that can be given to the player.
/// Implements ITraitable to support quest-specific traits from templates.
/// </summary>
public class Quest : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this quest.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the quest title displayed to the player.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the descriptive text explaining the quest objectives.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quest type (e.g., "kill", "fetch", "escort", "investigate", "delivery").
    /// </summary>
    public string QuestType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the difficulty rating (e.g., "easy", "medium", "hard").
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quest category ("main", "side", "legendary").
    /// </summary>
    public string Type { get; set; } = "side";

    /// <summary>
    /// Gets or sets the unique identifier of the NPC who gives this quest.
    /// </summary>
    public string QuestGiverId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of the quest giver NPC.
    /// </summary>
    public string QuestGiverName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of target for the objective (e.g., "beast", "undead", "demon").
    /// </summary>
    public string TargetType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the specific name of the target (e.g., "Ancient Red Dragon").
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the required quantity to complete the objective.
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the location where the quest objective must be completed.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of prerequisite quest IDs that must be completed before this quest becomes available.
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary mapping objective names to their required completion counts.
    /// </summary>
    public Dictionary<string, int> Objectives { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary tracking current progress towards each objective.
    /// </summary>
    public Dictionary<string, int> ObjectiveProgress { get; set; } = new();

    /// <summary>
    /// Gets or sets the gold reward for completing this quest.
    /// </summary>
    public int GoldReward { get; set; }
    
    /// <summary>
    /// Gets or sets the experience points reward for completing this quest.
    /// </summary>
    public int XpReward { get; set; }
    
    /// <summary>
    /// Gets or sets the bonus time (in minutes) awarded in Apocalypse mode for completing this quest.
    /// </summary>
    public int ApocalypseBonusMinutes { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the collection of item reward IDs resolved from @items references in JSON data.
    /// </summary>
    public List<string> ItemRewardIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of ability reward IDs resolved from @abilities references in JSON data.
    /// </summary>
    public List<string> AbilityRewardIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of location IDs for quest objectives resolved from @locations references.
    /// </summary>
    public List<string> ObjectiveLocationIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of NPC IDs for quest objectives resolved from @npcs references.
    /// </summary>
    public List<string> ObjectiveNpcIds { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the collection of enemy IDs for quest objectives resolved from @enemies references.
    /// </summary>
    public List<string> ObjectiveEnemyIds { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this quest is currently active in the player's quest log.
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether this quest has been completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the current progress count towards the quest objective.
    /// </summary>
    public int Progress { get; set; } = 0;

    /// <summary>
    /// Gets or sets the time limit for this quest in hours (0 = no time limit).
    /// </summary>
    public int TimeLimit { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the timestamp when this quest was started.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic quest properties.
    /// Implements ITraitable interface.
    /// </summary>
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
