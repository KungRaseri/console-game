namespace Game.Shared.Models;

/// <summary>
/// Represents a saved game state with all player progress and game world state.
/// </summary>
public class SaveGame
{
    // === Save Metadata ===
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PlayerName { get; set; } = string.Empty;
    public DateTime SaveDate { get; set; } = DateTime.Now;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public int PlayTimeMinutes { get; set; }
    public string GameVersion { get; set; } = "1.0.0";

    // === Character Data ===
    public Character Character { get; set; } = new();

    // === Quest System ===
    public List<Quest> ActiveQuests { get; set; } = new();
    public List<Quest> CompletedQuests { get; set; } = new();
    public List<Quest> FailedQuests { get; set; } = new();
    public List<Quest> AvailableQuests { get; set; } = new(); // Quests offered but not yet accepted

    // === World State ===
    public List<NPC> KnownNPCs { get; set; } = new(); // NPCs the player has met
    public Dictionary<string, int> NPCRelationships { get; set; } = new(); // NPC ID -> relationship value (-100 to 100)
    public List<string> VisitedLocations { get; set; } = new();
    public List<string> DiscoveredLocations { get; set; } = new(); // Locations on map but not visited

    // === Combat History ===
    public int TotalEnemiesDefeated { get; set; }
    public Dictionary<string, int> EnemiesDefeatedByType { get; set; } = new(); // "dragon" -> 5, "beast" -> 12
    public List<Enemy> LegendaryEnemiesDefeated { get; set; } = new(); // Keep record of legendary kills

    // === Statistics ===
    public int TotalGoldEarned { get; set; }
    public int TotalGoldSpent { get; set; }
    public int QuestsCompleted { get; set; }
    public int QuestsFailed { get; set; }
    public int ItemsCrafted { get; set; }
    public int ItemsSold { get; set; }
    public int DeathCount { get; set; }
    public string LastDeathLocation { get; set; } = string.Empty;
    public DateTime? LastDeathDate { get; set; }

    // === Achievements / Flags ===
    public List<string> UnlockedAchievements { get; set; } = new();
    public Dictionary<string, bool> GameFlags { get; set; } = new(); // Story flags, events, etc.

    // === Difficulty / Settings ===
    public string DifficultyLevel { get; set; } = "Normal"; // Easy, Normal, Hard, Expert
    public bool IronmanMode { get; set; } = false; // No reload, single save file

    // New difficulty system fields (Phase 1)
    public bool PermadeathMode { get; set; } = false;
    public bool ApocalypseMode { get; set; } = false;
    public DateTime? ApocalypseStartTime { get; set; }
    public int ApocalypseBonusMinutes { get; set; } = 0;

    // For tracking dropped items on death (Phase 2)
    public Dictionary<string, List<Item>> DroppedItemsAtLocations { get; set; } = new();

    /// <summary>
    /// Get a summary of the save game for display in load menu.
    /// </summary>
    public string GetSummary()
    {
        return $"{PlayerName} - Level {Character.Level} {Character.ClassName} - {PlayTimeMinutes / 60}h {PlayTimeMinutes % 60}m - {ActiveQuests.Count} active quests";
    }

    /// <summary>
    /// Calculate completion percentage based on various metrics.
    /// </summary>
    public double GetCompletionPercentage()
    {
        double total = 0;
        double completed = 0;

        // Quest completion (40% weight)
        if (CompletedQuests.Count + ActiveQuests.Count + FailedQuests.Count > 0)
        {
            total += 40;
            completed += (CompletedQuests.Count / (double)(CompletedQuests.Count + ActiveQuests.Count + FailedQuests.Count)) * 40;
        }

        // Location discovery (30% weight)
        if (DiscoveredLocations.Count + VisitedLocations.Count > 0)
        {
            total += 30;
            completed += (VisitedLocations.Count / (double)(DiscoveredLocations.Count + VisitedLocations.Count)) * 30;
        }

        // Achievement completion (30% weight)
        // Assuming 50 total achievements for now
        total += 30;
        completed += (UnlockedAchievements.Count / 50.0) * 30;

        return total > 0 ? (completed / total) * 100 : 0;
    }
}
