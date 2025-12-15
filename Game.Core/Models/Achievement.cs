namespace Game.Core.Models;

public class Achievement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏆";
    public AchievementCategory Category { get; set; }
    public int Points { get; set; }
    public bool IsSecret { get; set; }
    
    // Unlock criteria
    public AchievementCriteria Criteria { get; set; } = new();
    
    // State
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}

public class AchievementCriteria
{
    public AchievementType Type { get; set; }
    public int RequiredValue { get; set; }
    public string? RequiredId { get; set; } // For specific quests, enemies, etc.
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
