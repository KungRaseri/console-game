namespace Game.Models;

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
    
    // Quest giver
    public string QuestGiverId { get; set; } = string.Empty;
    public string QuestGiverName { get; set; } = string.Empty;
    
    // Objectives
    public string TargetType { get; set; } = string.Empty; // beast, undead, demon, etc.
    public string TargetName { get; set; } = string.Empty; // "Ancient Red Dragon"
    public int Quantity { get; set; } = 1;
    public string Location { get; set; } = string.Empty;
    
    // Rewards
    public int GoldReward { get; set; }
    public int XpReward { get; set; }
    public List<string> ItemRewards { get; set; } = new();
    
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
}
