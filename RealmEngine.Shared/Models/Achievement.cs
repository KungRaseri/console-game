namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an achievement that can be earned by the player.
/// </summary>
public class Achievement
{
    /// <summary>
    /// Gets or sets the unique identifier for this achievement.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the achievement title displayed to the player.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description explaining how to earn this achievement.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the icon displayed for this achievement.
    /// </summary>
    public string Icon { get; set; } = "🏆";
    
    /// <summary>
    /// Gets or sets the achievement category (Combat, Exploration, Collection, etc.).
    /// </summary>
    public AchievementCategory Category { get; set; }
    
    /// <summary>
    /// Gets or sets the point value of this achievement.
    /// </summary>
    public int Points { get; set; }
    
    /// <summary>
    /// Gets or sets whether this is a secret achievement (hidden until unlocked).
    /// </summary>
    public bool IsSecret { get; set; }

    /// <summary>
    /// Gets or sets the criteria required to unlock this achievement.
    /// </summary>
    public AchievementCriteria Criteria { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the player has unlocked this achievement.
    /// </summary>
    public bool IsUnlocked { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this achievement was unlocked.
    /// </summary>
    public DateTime? UnlockedAt { get; set; }
}

/// <summary>
/// Represents the criteria required to unlock an achievement.
/// </summary>
public class AchievementCriteria
{
    /// <summary>
    /// Gets or sets the type of achievement (Kill, Collect, Complete, Explore, etc.).
    /// </summary>
    public AchievementType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the required value to reach (e.g., number of kills, items collected).
    /// </summary>
    public int RequiredValue { get; set; }
    
    /// <summary>
    /// Gets or sets the specific ID required (for quest completion, specific enemy kills, etc.).
    /// </summary>
    public string? RequiredId { get; set; }
}

public enum AchievementCategory
{
    Combat,
    Exploration,
    Quests,
    Survival,
    Mastery,
    Secret
}

public enum AchievementType
{
    CompleteQuest,
    DefeatEnemies,
    ReachLevel,
    CollectGold,
    SurviveTime,
    CompleteGame,
    CompleteDifficulty,
    Deathless
}
