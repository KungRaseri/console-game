namespace Game.Models;

/// <summary>
/// Records a character's final moments for the Hall of Fame.
/// Used primarily for permadeath mode but can track any character death.
/// </summary>
public class HallOfFameEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CharacterName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int PlayTimeMinutes { get; set; }
    public int TotalEnemiesDefeated { get; set; }
    public int QuestsCompleted { get; set; }
    public int DeathCount { get; set; }
    public string DeathReason { get; set; } = "Unknown";
    public string DeathLocation { get; set; } = "Unknown";
    public DateTime DeathDate { get; set; } = DateTime.Now;
    public int AchievementsUnlocked { get; set; }
    public bool IsPermadeath { get; set; }
    public string DifficultyLevel { get; set; } = "Normal";
    
    /// <summary>
    /// Calculate a "fame score" for ranking.
    /// </summary>
    public int GetFameScore()
    {
        var score = Level * 100;
        score += QuestsCompleted * 50;
        score += TotalEnemiesDefeated * 5;
        score += AchievementsUnlocked * 200;
        
        if (IsPermadeath)
            score *= 2; // Double points for permadeath runs
        
        return score;
    }
    
    /// <summary>
    /// Get formatted playtime string.
    /// </summary>
    public string GetPlaytimeFormatted()
    {
        var hours = PlayTimeMinutes / 60;
        var minutes = PlayTimeMinutes % 60;
        return $"{hours}h {minutes}m";
    }
}
