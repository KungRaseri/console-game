# Phase 4: End-Game System & Victory Conditions

**Status**: � Ready to Start  
**Prerequisites**: ✅ Phase 1, 2, & 3 complete  
**Estimated Time**: 4-5 hours  
**Previous Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)  
**Next Phase**: None (Final Phase)  
**Related**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md), [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)

---

## 📋 Overview

Implement the 4-phase end-game system with main quest chain, victory screens, True Ending unlock conditions, New Game+ system, and comprehensive achievement tracking.

**✅ Pre-Phase Foundation Complete:**
- Quest model enhanced with Type, Prerequisites, Objectives, ObjectiveProgress
- Quest model has IsObjectivesComplete() and UpdateObjectiveProgress() methods
- QuestGenerator updated with InitializeObjectives and DetermineQuestType
- SaveGame has quest tracking fields (ActiveQuests, CompletedQuests, etc.)
- All infrastructure ready for main quest chain implementation

---

## 🎯 Goals

1. ✅ Create main quest chain (10 quests, Lv1-35)
2. ✅ Implement victory screen and statistics display
3. ✅ Add True Ending unlock (Level 50 + 100% completion)
4. ✅ Create New Game+ system with carry-over bonuses
5. ✅ Implement achievement system (50+ achievements)
6. ✅ Add completion percentage tracking
7. ✅ Update this artifact with completion status

---

## 📁 Files to Create

### 1. `Game/Models/Quest.cs` - Quest Model Enhancement

**✅ Note**: Quest model already enhanced during pre-phase improvements! The following properties already exist:
- `Type` - Quest type (main, side, legendary)
- `Prerequisites` - List of quest IDs that must be completed first
- `Objectives` - Dictionary of objectives with target counts
- `ObjectiveProgress` - Dictionary tracking current progress
- `IsObjectivesComplete()` - Method to check completion
- `UpdateObjectiveProgress(objectiveName, increment)` - Method to update progress

**ADD** these additional properties for Phase 4:

```csharp
// Add to existing Quest.cs
public string Difficulty { get; set; } = "medium"; // "easy", "medium", "hard"
public int GoldReward { get; set; }
public int XpReward { get; set; }
public Dictionary<string, int> ItemRewards { get; set; } = new(); // ItemId -> Quantity

public double GetCompletionPercentage()
{
    if (Objectives.Count == 0) return 100.0;
    
    var totalRequired = Objectives.Values.Sum();
    var totalProgress = 0;
    
    foreach (var objective in Objectives.Keys)
    {
        if (ObjectiveProgress.ContainsKey(objective))
            totalProgress += ObjectiveProgress[objective];
    }
    
    return (double)totalProgress / totalRequired * 100.0;
}
```

---

### 2. `Game/Services/MainQuestService.cs` (NEW)

```csharp
using Game.Models;
using Serilog;

namespace Game.Services;

/// <summary>
/// Manages the main quest chain and progression.
/// </summary>
public class MainQuestService
{
    private readonly List<Quest> _mainQuests;
    
    public MainQuestService()
    {
        _mainQuests = CreateMainQuestChain();
    }
    
    /// <summary>
    /// Get all main quests.
    /// </summary>
    public List<Quest> GetMainQuests() => _mainQuests;
    
    /// <summary>
    /// Get next available main quest for player.
    /// </summary>
    public Quest? GetNextMainQuest(SaveGame saveGame, int playerLevel)
    {
        foreach (var quest in _mainQuests)
        {
            // Skip if already completed
            if (saveGame.CompletedQuests.Contains(quest.Id))
                continue;
            
            // Skip if already active
            if (saveGame.ActiveQuests.Any(q => q.Id == quest.Id))
                continue;
            
            // Check level requirement
            if (playerLevel < quest.RequiredLevel)
                continue;
            
            // Check prerequisites
            var prereqsMet = quest.Prerequisites.All(prereq => 
                saveGame.CompletedQuests.Contains(prereq));
            
            if (!prereqsMet)
                continue;
            
            return quest;
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if player has completed main quest chain.
    /// </summary>
    public bool HasCompletedMainQuest(SaveGame saveGame)
    {
        var finalQuestId = "main_10_shadow_lord";
        return saveGame.CompletedQuests.Contains(finalQuestId);
    }
    
    /// <summary>
    /// Get main quest completion percentage.
    /// </summary>
    public double GetMainQuestProgress(SaveGame saveGame)
    {
        var completed = _mainQuests.Count(q => saveGame.CompletedQuests.Contains(q.Id));
        return (double)completed / _mainQuests.Count * 100.0;
    }
    
    /// <summary>
    /// Create the main quest chain.
    /// </summary>
    private List<Quest> CreateMainQuestChain()
    {
        return new List<Quest>
        {
            new Quest
            {
                Id = "main_01_awakening",
                Title = "The Awakening",
                Description = "You awaken in a strange land with no memory. Explore the village and speak to the Elder.",
                Type = "main",
                RequiredLevel = 1,
                GoldReward = 100,
                XpReward = 500,
                Objectives = new Dictionary<string, int>
                {
                    { "Speak to Village Elder", 1 },
                    { "Explore the village", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_02_first_threat",
                Title = "The First Threat",
                Description = "Bandits are terrorizing the village. Defeat them to prove yourself.",
                Type = "main",
                RequiredLevel = 3,
                Prerequisites = new List<string> { "main_01_awakening" },
                GoldReward = 250,
                XpReward = 1000,
                Objectives = new Dictionary<string, int>
                {
                    { "Defeat Bandits", 5 },
                    { "Defeat Bandit Leader", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_03_ancient_ruins",
                Title = "Secrets of the Ruins",
                Description = "The Elder speaks of ancient ruins holding clues to your past. Investigate them.",
                Type = "main",
                RequiredLevel = 7,
                Prerequisites = new List<string> { "main_02_first_threat" },
                GoldReward = 500,
                XpReward = 2000,
                Objectives = new Dictionary<string, int>
                {
                    { "Explore Ancient Ruins", 1 },
                    { "Find Memory Fragment", 1 },
                    { "Defeat Ruin Guardian", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_04_dark_prophecy",
                Title = "The Dark Prophecy",
                Description = "A prophecy foretells the return of the Shadow Lord. Seek the Oracle for guidance.",
                Type = "main",
                RequiredLevel = 12,
                Prerequisites = new List<string> { "main_03_ancient_ruins" },
                GoldReward = 750,
                XpReward = 3000,
                Objectives = new Dictionary<string, int>
                {
                    { "Journey to Mountain Temple", 1 },
                    { "Consult the Oracle", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_05_gathering_allies",
                Title = "Gathering Allies",
                Description = "The Shadow Lord's forces grow. Rally heroes from across the land.",
                Type = "main",
                RequiredLevel = 17,
                Prerequisites = new List<string> { "main_04_dark_prophecy" },
                GoldReward = 1000,
                XpReward = 4000,
                Objectives = new Dictionary<string, int>
                {
                    { "Recruit Warrior", 1 },
                    { "Recruit Mage", 1 },
                    { "Recruit Ranger", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_06_trials",
                Title = "The Trials of Worth",
                Description = "Prove your worth in the ancient trials to unlock legendary power.",
                Type = "main",
                RequiredLevel = 22,
                Prerequisites = new List<string> { "main_05_gathering_allies" },
                GoldReward = 1500,
                XpReward = 5000,
                Objectives = new Dictionary<string, int>
                {
                    { "Trial of Strength", 1 },
                    { "Trial of Wisdom", 1 },
                    { "Trial of Courage", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_07_dark_fortress",
                Title = "Assault on the Dark Fortress",
                Description = "Attack the Shadow Lord's fortress and weaken his defenses.",
                Type = "main",
                RequiredLevel = 27,
                Prerequisites = new List<string> { "main_06_trials" },
                GoldReward = 2000,
                XpReward = 6000,
                Objectives = new Dictionary<string, int>
                {
                    { "Breach the Outer Walls", 1 },
                    { "Defeat Dark Generals", 3 },
                    { "Destroy Shadow Crystals", 5 }
                }
            },
            
            new Quest
            {
                Id = "main_08_betrayal",
                Title = "The Betrayal",
                Description = "A trusted ally reveals their true allegiance. Face the betrayer.",
                Type = "main",
                RequiredLevel = 30,
                Prerequisites = new List<string> { "main_07_dark_fortress" },
                GoldReward = 2500,
                XpReward = 7000,
                Objectives = new Dictionary<string, int>
                {
                    { "Confront the Betrayer", 1 },
                    { "Survive the Ambush", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_09_final_preparations",
                Title = "The Final Hour",
                Description = "Prepare for the final confrontation with the Shadow Lord.",
                Type = "main",
                RequiredLevel = 33,
                Prerequisites = new List<string> { "main_08_betrayal" },
                GoldReward = 3000,
                XpReward = 8000,
                Objectives = new Dictionary<string, int>
                {
                    { "Gather Legendary Weapons", 1 },
                    { "Rally All Allies", 1 },
                    { "Strengthen Defenses", 1 }
                }
            },
            
            new Quest
            {
                Id = "main_10_shadow_lord",
                Title = "Defeat the Shadow Lord",
                Description = "The final battle. Defeat the Shadow Lord and save the world.",
                Type = "main",
                RequiredLevel = 35,
                Prerequisites = new List<string> { "main_09_final_preparations" },
                GoldReward = 10000,
                XpReward = 15000,
                Objectives = new Dictionary<string, int>
                {
                    { "Enter the Shadow Realm", 1 },
                    { "Defeat the Shadow Lord", 1 }
                }
            }
        };
    }
}
```

---

### 3. `Game/Services/AchievementService.cs` (NEW)

```csharp
using Game.Models;
using Serilog;

namespace Game.Services;

/// <summary>
/// Manages achievements and tracks player progress.
/// </summary>
public class AchievementService
{
    private readonly List<Achievement> _allAchievements;
    
    public AchievementService()
    {
        _allAchievements = CreateAchievements();
    }
    
    /// <summary>
    /// Get all achievements.
    /// </summary>
    public List<Achievement> GetAllAchievements() => _allAchievements;
    
    /// <summary>
    /// Check and unlock achievements based on save game state.
    /// </summary>
    public List<Achievement> CheckAchievements(SaveGame saveGame, Character player)
    {
        var newlyUnlocked = new List<Achievement>();
        
        foreach (var achievement in _allAchievements)
        {
            // Skip if already unlocked
            if (saveGame.UnlockedAchievements.Contains(achievement.Id))
                continue;
            
            // Check if conditions are met
            if (IsAchievementUnlocked(achievement, saveGame, player))
            {
                saveGame.UnlockedAchievements.Add(achievement.Id);
                newlyUnlocked.Add(achievement);
                Log.Information("Achievement unlocked: {Title}", achievement.Title);
            }
        }
        
        return newlyUnlocked;
    }
    
    /// <summary>
    /// Get completion percentage (unlocked achievements / total).
    /// </summary>
    public double GetAchievementProgress(SaveGame saveGame)
    {
        return (double)saveGame.UnlockedAchievements.Count / _allAchievements.Count * 100.0;
    }
    
    /// <summary>
    /// Check if achievement conditions are met.
    /// </summary>
    private bool IsAchievementUnlocked(Achievement achievement, SaveGame saveGame, Character player)
    {
        return achievement.Id switch
        {
            // Level Achievements
            "level_10" => player.Level >= 10,
            "level_25" => player.Level >= 25,
            "level_50" => player.Level >= 50,
            "max_level" => player.Level >= 100,
            
            // Quest Achievements
            "complete_10_quests" => saveGame.QuestsCompleted >= 10,
            "complete_50_quests" => saveGame.QuestsCompleted >= 50,
            "complete_100_quests" => saveGame.QuestsCompleted >= 100,
            "main_quest_complete" => saveGame.CompletedQuests.Contains("main_10_shadow_lord"),
            
            // Combat Achievements
            "defeat_100_enemies" => saveGame.TotalEnemiesDefeated >= 100,
            "defeat_500_enemies" => saveGame.TotalEnemiesDefeated >= 500,
            "defeat_1000_enemies" => saveGame.TotalEnemiesDefeated >= 1000,
            "defeat_first_legendary" => saveGame.LegendaryEnemiesDefeated.Count >= 1,
            "defeat_all_legendaries" => saveGame.LegendaryEnemiesDefeated.Count >= 10,
            
            // Wealth Achievements
            "earn_10k_gold" => saveGame.TotalGoldEarned >= 10000,
            "earn_100k_gold" => saveGame.TotalGoldEarned >= 100000,
            "hoarder" => player.Gold >= 50000,
            
            // Death Achievements (ironic!)
            "die_once" => saveGame.TotalDeaths >= 1,
            "die_10_times" => saveGame.TotalDeaths >= 10,
            "survivor" => saveGame.TotalDeaths == 0 && player.Level >= 25,
            "ironman_victory" => saveGame.IronmanMode && saveGame.CompletedQuests.Contains("main_10_shadow_lord"),
            "permadeath_attempt" => saveGame.PermadeathMode, // Just attempting is an achievement!
            
            // Exploration Achievements
            "discover_10_locations" => saveGame.DiscoveredLocations.Count >= 10,
            "discover_all_locations" => saveGame.DiscoveredLocations.Count >= 30,
            "world_traveler" => saveGame.VisitedLocations.Count >= 25,
            
            // NPC Achievements
            "meet_20_npcs" => saveGame.KnownNPCs.Count >= 20,
            "max_friendship" => saveGame.NPCRelationships.Values.Any(r => r >= 100),
            "max_hatred" => saveGame.NPCRelationships.Values.Any(r => r <= -100),
            
            // Misc Achievements
            "complete_game_100%" => saveGame.GetCompletionPercentage() >= 100.0,
            "speed_runner" => saveGame.ApocalypseMode && saveGame.CompletedQuests.Contains("main_10_shadow_lord"),
            "true_ending" => saveGame.CompletedQuests.Contains("true_ending_quest"),
            "new_game_plus" => saveGame.NewGamePlusCount >= 1,
            
            _ => false
        };
    }
    
    /// <summary>
    /// Create all achievements.
    /// </summary>
    private List<Achievement> CreateAchievements()
    {
        return new List<Achievement>
        {
            // Level Achievements
            new Achievement("level_10", "Apprentice Hero", "Reach level 10", "progression", 10),
            new Achievement("level_25", "Veteran Hero", "Reach level 25", "progression", 25),
            new Achievement("level_50", "Master Hero", "Reach level 50", "progression", 50),
            new Achievement("max_level", "Legendary Hero", "Reach level 100", "progression", 100),
            
            // Quest Achievements
            new Achievement("complete_10_quests", "Quest Starter", "Complete 10 quests", "quests", 10),
            new Achievement("complete_50_quests", "Quest Master", "Complete 50 quests", "quests", 50),
            new Achievement("complete_100_quests", "Questaholic", "Complete 100 quests", "quests", 100),
            new Achievement("main_quest_complete", "Shadow Slayer", "Defeat the Shadow Lord", "quests", 200),
            
            // Combat Achievements
            new Achievement("defeat_100_enemies", "Monster Slayer", "Defeat 100 enemies", "combat", 20),
            new Achievement("defeat_500_enemies", "Demon Hunter", "Defeat 500 enemies", "combat", 50),
            new Achievement("defeat_1000_enemies", "Army Crusher", "Defeat 1000 enemies", "combat", 100),
            new Achievement("defeat_first_legendary", "Legend Killer", "Defeat your first legendary enemy", "combat", 50),
            new Achievement("defeat_all_legendaries", "Bane of Legends", "Defeat all legendary enemies", "combat", 200),
            
            // Wealth Achievements
            new Achievement("earn_10k_gold", "Entrepreneur", "Earn 10,000 gold", "wealth", 15),
            new Achievement("earn_100k_gold", "Tycoon", "Earn 100,000 gold", "wealth", 75),
            new Achievement("hoarder", "Dragon's Hoard", "Possess 50,000 gold at once", "wealth", 50),
            
            // Death Achievements
            new Achievement("die_once", "Mortal", "Die for the first time", "death", 5),
            new Achievement("die_10_times", "Stubborn", "Die 10 times and keep going", "death", 10),
            new Achievement("survivor", "Untouchable", "Reach level 25 without dying", "death", 100),
            new Achievement("ironman_victory", "Iron Will", "Complete the main quest in Ironman mode", "death", 200),
            new Achievement("permadeath_attempt", "Brave Soul", "Attempt Permadeath mode", "death", 50),
            
            // Exploration Achievements
            new Achievement("discover_10_locations", "Explorer", "Discover 10 locations", "exploration", 15),
            new Achievement("discover_all_locations", "Cartographer", "Discover all locations", "exploration", 100),
            new Achievement("world_traveler", "Wanderer", "Visit 25 different locations", "exploration", 50),
            
            // NPC Achievements
            new Achievement("meet_20_npcs", "Social Butterfly", "Meet 20 NPCs", "social", 20),
            new Achievement("max_friendship", "Best Friend", "Max out relationship with an NPC", "social", 50),
            new Achievement("max_hatred", "Nemesis", "Min out relationship with an NPC", "social", 50),
            
            // Special Achievements
            new Achievement("complete_game_100%", "Perfectionist", "Achieve 100% completion", "special", 250),
            new Achievement("speed_runner", "Against the Clock", "Beat Apocalypse mode", "special", 200),
            new Achievement("true_ending", "Truth Seeker", "Unlock the True Ending", "special", 300),
            new Achievement("new_game_plus", "Never Enough", "Start New Game+", "special", 100)
        };
    }
}

public class Achievement
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public int Points { get; set; }
    
    public Achievement(string id, string title, string description, string category, int points)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        Points = points;
    }
}
```

---

### 4. `Game/Services/VictoryService.cs` (NEW)

```csharp
using Game.Models;
using Game.UI;
using Spectre.Console;
using Serilog;

namespace Game.Services;

/// <summary>
/// Handles victory screens and end-game scenarios.
/// </summary>
public class VictoryService
{
    private readonly MainQuestService _mainQuestService;
    private readonly AchievementService _achievementService;
    
    public VictoryService(MainQuestService mainQuestService, AchievementService achievementService)
    {
        _mainQuestService = mainQuestService;
        _achievementService = achievementService;
    }
    
    /// <summary>
    /// Show main quest victory screen.
    /// </summary>
    public async Task ShowMainQuestVictory(SaveGame saveGame, Character player)
    {
        Console.Clear();
        
        // Victory animation
        await ShowVictoryAnimation();
        
        // Victory screen
        ConsoleUI.Clear();
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        ConsoleUI.ShowSuccess("                                                 ");
        ConsoleUI.ShowSuccess("              VICTORY!                           ");
        ConsoleUI.ShowSuccess("                                                 ");
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        
        Thread.Sleep(2000);
        Console.WriteLine();
        
        ConsoleUI.WriteText("The Shadow Lord falls before you.");
        ConsoleUI.WriteText("Light returns to the land.");
        ConsoleUI.WriteText("Peace is restored... for now.");
        
        Thread.Sleep(3000);
        Console.WriteLine();
        
        // Show statistics
        ShowVictoryStatistics(saveGame, player);
        
        Thread.Sleep(2000);
        
        // Check for True Ending unlock
        var canUnlockTrueEnding = player.Level >= 50 && saveGame.GetCompletionPercentage() >= 100.0;
        
        if (canUnlockTrueEnding && !saveGame.CompletedQuests.Contains("true_ending_quest"))
        {
            await ShowTrueEndingUnlock();
        }
        else
        {
            // Offer New Game+
            await ShowNewGamePlusOffer(saveGame);
        }
        
        Log.Information("Main quest victory screen shown for {PlayerName}", player.Name);
    }
    
    /// <summary>
    /// Show victory animation.
    /// </summary>
    private async Task ShowVictoryAnimation()
    {
        var frames = new[]
        {
            "The Shadow Lord staggers...",
            "His power wanes...",
            "He falls to one knee...",
            "\"Impossible...\" he whispers...",
            "A blinding flash of light!",
            "Silence...",
            "The darkness fades..."
        };
        
        foreach (var frame in frames)
        {
            Console.Clear();
            ConsoleUI.WriteText(frame);
            await Task.Delay(1500);
        }
    }
    
    /// <summary>
    /// Show victory statistics.
    /// </summary>
    private void ShowVictoryStatistics(SaveGame saveGame, Character player)
    {
        ConsoleUI.ShowBanner("Victory Statistics", "═");
        
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[cyan]Statistic[/]").LeftAligned());
        table.AddColumn(new TableColumn("[yellow]Value[/]").RightAligned());
        
        table.AddRow("Final Level", player.Level.ToString());
        table.AddRow("Total Playtime", FormatPlaytime(saveGame.TotalPlaytimeMinutes));
        table.AddRow("Quests Completed", saveGame.QuestsCompleted.ToString());
        table.AddRow("Enemies Defeated", saveGame.TotalEnemiesDefeated.ToString());
        table.AddRow("Legendary Enemies", saveGame.LegendaryEnemiesDefeated.Count.ToString());
        table.AddRow("Gold Earned", saveGame.TotalGoldEarned.ToString("N0"));
        table.AddRow("Deaths", saveGame.TotalDeaths.ToString());
        table.AddRow("Locations Discovered", saveGame.DiscoveredLocations.Count.ToString());
        table.AddRow("Achievements Unlocked", $"{saveGame.UnlockedAchievements.Count} / {_achievementService.GetAllAchievements().Count}");
        table.AddRow("Completion", $"{saveGame.GetCompletionPercentage():F1}%");
        
        AnsiConsole.Write(table);
        Console.WriteLine();
    }
    
    /// <summary>
    /// Show True Ending unlock message.
    /// </summary>
    private async Task ShowTrueEndingUnlock()
    {
        Console.Clear();
        
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        ConsoleUI.ShowSuccess("           TRUE ENDING UNLOCKED!                 ");
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        
        await Task.Delay(2000);
        Console.WriteLine();
        
        ConsoleUI.WriteText("But wait...");
        await Task.Delay(1500);
        
        ConsoleUI.WriteText("Something stirs in the darkness...");
        await Task.Delay(1500);
        
        ConsoleUI.WriteText("A new quest has appeared: 'The True Enemy'");
        await Task.Delay(2000);
        
        Console.WriteLine();
        ConsoleUI.ShowInfo("You have unlocked the True Ending path!");
        ConsoleUI.WriteText("Return to the game to face the ultimate challenge.");
        
        Thread.Sleep(3000);
    }
    
    /// <summary>
    /// Offer New Game+ to the player.
    /// </summary>
    private async Task ShowNewGamePlusOffer(SaveGame saveGame)
    {
        Console.WriteLine();
        ConsoleUI.ShowInfo("═════════════════════════════════════════════════");
        ConsoleUI.ShowInfo("           NEW GAME+ AVAILABLE                   ");
        ConsoleUI.ShowInfo("═════════════════════════════════════════════════");
        
        Console.WriteLine();
        ConsoleUI.WriteText("Start a new adventure with bonuses:");
        ConsoleUI.WriteText("  • Keep all achievements");
        ConsoleUI.WriteText("  • +5 to all attributes");
        ConsoleUI.WriteText("  • +25% gold and XP rewards");
        ConsoleUI.WriteText("  • Enemies are stronger and smarter");
        ConsoleUI.WriteText("  • New legendary enemies appear");
        
        Console.WriteLine();
        
        if (ConsoleUI.Confirm("Start New Game+?"))
        {
            ConsoleUI.ShowSuccess("New Game+ will be available in the main menu!");
        }
        
        await Task.Delay(1000);
    }
    
    /// <summary>
    /// Format playtime in human-readable format.
    /// </summary>
    private string FormatPlaytime(int totalMinutes)
    {
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        
        if (hours > 0)
            return $"{hours}h {minutes}m";
        
        return $"{minutes}m";
    }
    
    /// <summary>
    /// Show True Ending victory screen.
    /// </summary>
    public async Task ShowTrueEndingVictory(SaveGame saveGame, Character player)
    {
        Console.Clear();
        
        // Epic victory sequence
        await ShowTrueEndingAnimation();
        
        ConsoleUI.Clear();
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        ConsoleUI.ShowSuccess("                                                 ");
        ConsoleUI.ShowSuccess("           TRUE ENDING ACHIEVED!                 ");
        ConsoleUI.ShowSuccess("                                                 ");
        ConsoleUI.ShowSuccess("═════════════════════════════════════════════════");
        
        Thread.Sleep(3000);
        Console.WriteLine();
        
        ConsoleUI.WriteText("You have defeated the Ancient Evil.");
        ConsoleUI.WriteText("The true enemy has been vanquished.");
        ConsoleUI.WriteText("The world is saved... truly, this time.");
        
        Thread.Sleep(4000);
        Console.WriteLine();
        
        ConsoleUI.ShowSuccess("Congratulations! You have achieved 100% completion!");
        
        Thread.Sleep(2000);
        
        // Show final statistics
        ShowVictoryStatistics(saveGame, player);
        
        // Still offer New Game+
        await ShowNewGamePlusOffer(saveGame);
        
        Log.Information("True Ending victory shown for {PlayerName}", player.Name);
    }
    
    /// <summary>
    /// Show True Ending animation.
    /// </summary>
    private async Task ShowTrueEndingAnimation()
    {
        var frames = new[]
        {
            "The Ancient Evil reveals its true form...",
            "Power beyond comprehension...",
            "You draw upon all your strength...",
            "The final clash!",
            "Light versus Darkness...",
            "An explosion of energy!",
            "...",
            "Silence.",
            "You stand victorious.",
            "The nightmare is over."
        };
        
        foreach (var frame in frames)
        {
            Console.Clear();
            ConsoleUI.WriteText(frame);
            await Task.Delay(2000);
        }
    }
}
```

---

## 📝 Files to Modify

### 5. `Game/Models/SaveGame.cs` - Add New Game+ Fields

**ADD** properties:

```csharp
// New Game+ tracking
public int NewGamePlusCount { get; set; } = 0;
public bool IsNewGamePlus { get; set; } = false;
public List<string> NewGamePlusAchievements { get; set; } = new(); // Carried over
public int NewGamePlusBonusStats { get; set; } = 0; // +5 per NG+ cycle
```

**MODIFY** `GetCompletionPercentage` method:

```csharp
public double GetCompletionPercentage()
{
    // This is a comprehensive formula considering all aspects
    var totalWeight = 0.0;
    var achievedWeight = 0.0;
    
    // Main quest (40% weight)
    if (CompletedQuests.Contains("main_10_shadow_lord"))
    {
        achievedWeight += 40.0;
    }
    totalWeight += 40.0;
    
    // All quests (20% weight)
    var questProgress = QuestsCompleted / Math.Max(1.0, QuestsCompleted + ActiveQuests.Count + AvailableQuests.Count);
    achievedWeight += questProgress * 20.0;
    totalWeight += 20.0;
    
    // Achievements (20% weight)
    var achievementProgress = UnlockedAchievements.Count / 30.0; // Assuming ~30 achievements
    achievedWeight += Math.Min(achievementProgress, 1.0) * 20.0;
    totalWeight += 20.0;
    
    // Locations (10% weight)
    var locationProgress = DiscoveredLocations.Count / 30.0; // Assuming 30 locations
    achievedWeight += Math.Min(locationProgress, 1.0) * 10.0;
    totalWeight += 10.0;
    
    // Legendary enemies (10% weight)
    var legendaryProgress = LegendaryEnemiesDefeated.Count / 10.0; // Assuming 10 legendary enemies
    achievedWeight += Math.Min(legendaryProgress, 1.0) * 10.0;
    totalWeight += 10.0;
    
    return (achievedWeight / totalWeight) * 100.0;
}
```

---

### 6. `Game/Services/SaveGameService.cs` - Add New Game+ Method

**ADD** method:

```csharp
/// <summary>
/// Create a New Game+ save from an existing completed save.
/// </summary>
public SaveGame CreateNewGamePlus(SaveGame completedSave, Character newCharacter)
{
    var ngPlusSave = CreateNewGame(
        newCharacter,
        completedSave.Difficulty,
        isNewGamePlus: true
    );
    
    // Carry over achievements
    ngPlusSave.NewGamePlusAchievements = new List<string>(completedSave.UnlockedAchievements);
    ngPlusSave.UnlockedAchievements = new List<string>(completedSave.UnlockedAchievements);
    
    // Increment NG+ counter
    ngPlusSave.NewGamePlusCount = completedSave.NewGamePlusCount + 1;
    ngPlusSave.IsNewGamePlus = true;
    
    // Bonus stats (+5 per cycle)
    ngPlusSave.NewGamePlusBonusStats = 5 * ngPlusSave.NewGamePlusCount;
    
    Log.Information("Created New Game+ save. Cycle: {Cycle}, Bonus Stats: {BonusStats}",
        ngPlusSave.NewGamePlusCount, ngPlusSave.NewGamePlusBonusStats);
    
    return ngPlusSave;
}

/// <summary>
/// Update CreateNewGame signature to accept isNewGamePlus parameter.
/// </summary>
public SaveGame CreateNewGame(Character character, DifficultySettings difficulty, bool isNewGamePlus = false)
{
    // ... existing code ...
    
    saveGame.IsNewGamePlus = isNewGamePlus;
    
    return saveGame;
}
```

---

### 7. `Game/GameEngine.cs` - Integrate End-Game Systems

**ADD** fields:

```csharp
private readonly MainQuestService _mainQuestService;
private readonly AchievementService _achievementService;
private readonly VictoryService _victoryService;
```

**In constructor**:

```csharp
_mainQuestService = new MainQuestService();
_achievementService = new AchievementService();
_victoryService = new VictoryService(_mainQuestService, _achievementService);
```

**After quest completion**:

```csharp
private async Task OnQuestCompleted(Quest quest)
{
    // Award rewards
    _player.Gold += quest.GoldReward;
    _player.GainExperience(quest.XpReward);
    
    // Check achievements
    var newAchievements = _achievementService.CheckAchievements(_saveGameService.GetCurrentSave()!, _player);
    
    foreach (var achievement in newAchievements)
    {
        ConsoleUI.ShowSuccess($"🏆 Achievement Unlocked: {achievement.Title}");
        ConsoleUI.WriteText($"   {achievement.Description} (+{achievement.Points} points)");
        Thread.Sleep(2000);
    }
    
    // Check for victory conditions
    if (quest.Id == "main_10_shadow_lord")
    {
        await _victoryService.ShowMainQuestVictory(_saveGameService.GetCurrentSave()!, _player);
    }
    else if (quest.Id == "true_ending_quest")
    {
        await _victoryService.ShowTrueEndingVictory(_saveGameService.GetCurrentSave()!, _player);
    }
    
    // Make next main quest available
    var nextMainQuest = _mainQuestService.GetNextMainQuest(_saveGameService.GetCurrentSave()!, _player.Level);
    if (nextMainQuest != null)
    {
        _saveGameService.AddAvailableQuest(nextMainQuest);
        ConsoleUI.ShowInfo($"New main quest available: {nextMainQuest.Title}");
    }
}
```

**In main menu**:

```csharp
private async Task ShowMainMenu()
{
    var options = new List<string> { "New Game", "Load Game", "Settings", "Exit" };
    
    // Check if player has completed main quest for NG+ option
    var saves = _saveGameService.GetAllSaves();
    var hasCompletedSave = saves.Any(s => s.CompletedQuests.Contains("main_10_shadow_lord"));
    
    if (hasCompletedSave)
    {
        options.Insert(1, "New Game+");
    }
    
    var choice = ConsoleUI.ShowMenu("Main Menu", options.ToArray());
    
    // Handle choice...
    if (choice == "New Game+")
    {
        await StartNewGamePlus();
    }
}

private async Task StartNewGamePlus()
{
    // Select which completed save to use as base
    var completedSaves = _saveGameService.GetAllSaves()
        .Where(s => s.CompletedQuests.Contains("main_10_shadow_lord"))
        .ToList();
    
    if (completedSaves.Count == 0)
    {
        ConsoleUI.ShowError("No completed saves found!");
        return;
    }
    
    var saveNames = completedSaves.Select(s => $"{s.CharacterName} (Lv{s.CharacterLevel}, NG+{s.NewGamePlusCount})").ToArray();
    var selectedSave = ConsoleUI.ShowMenu("Select save for New Game+:", saveNames);
    var baseSave = completedSaves[Array.IndexOf(saveNames, selectedSave)];
    
    // Create new character
    var newCharacter = await _characterCreationService.CreateCharacterAsync();
    
    // Apply NG+ bonuses
    var bonusStats = 5 * (baseSave.NewGamePlusCount + 1);
    newCharacter.Strength += bonusStats;
    newCharacter.Dexterity += bonusStats;
    newCharacter.Intelligence += bonusStats;
    newCharacter.Constitution += bonusStats;
    
    ConsoleUI.ShowSuccess($"New Game+ bonuses applied: +{bonusStats} to all stats!");
    
    // Create NG+ save
    var ngPlusSave = _saveGameService.CreateNewGamePlus(baseSave, newCharacter);
    _player = newCharacter;
    
    // Start game
    _state = GameState.InGame;
    await GameLoopAsync();
}
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Main Quest Chain**:
   - [ ] Start new game
   - [ ] Verify first quest appears
   - [ ] Complete first quest
   - [ ] Verify next quest unlocks
   - [ ] Verify prerequisites block future quests
   - [ ] Complete entire quest chain
   - [ ] Verify victory screen displays

2. **Victory Screens**:
   - [ ] Defeat Shadow Lord
   - [ ] Verify victory animation plays
   - [ ] Verify statistics display correctly
   - [ ] Check completion percentage calculation

3. **True Ending**:
   - [ ] Reach Level 50
   - [ ] Achieve 100% completion
   - [ ] Defeat Shadow Lord
   - [ ] Verify True Ending quest unlocks
   - [ ] Complete True Ending quest
   - [ ] Verify True Ending victory screen

4. **New Game+**:
   - [ ] Complete main quest
   - [ ] Verify "New Game+" appears in main menu
   - [ ] Start New Game+
   - [ ] Verify achievements carry over
   - [ ] Verify +5 stat bonus applied
   - [ ] Verify NG+ counter increments
   - [ ] Complete NG+ and start NG++ (verify +10 stats)

5. **Achievements**:
   - [ ] Reach level 10 → verify achievement unlocks
   - [ ] Complete 10 quests → verify achievement
   - [ ] Defeat 100 enemies → verify achievement
   - [ ] Die once → verify achievement
   - [ ] Check achievement notifications display
   - [ ] Verify achievement progress tracking

6. **Completion Percentage**:
   - [ ] Start new game → verify 0%
   - [ ] Complete main quest → verify ~40%
   - [ ] Unlock achievements → verify percentage increases
   - [ ] Discover locations → verify percentage increases
   - [ ] Defeat legendary enemies → verify percentage increases
   - [ ] Reach 100% → verify True Ending unlocks

---

## ✅ Completion Checklist

- [ ] Created `Quest.cs` model
- [ ] Created `MainQuestService.cs` with 10-quest chain
- [ ] Created `AchievementService.cs` with 30+ achievements
- [ ] Created `VictoryService.cs` with victory screens
- [ ] Modified `SaveGame.cs` with NG+ fields
- [ ] Modified `SaveGameService.cs` with `CreateNewGamePlus`
- [ ] Modified `GameEngine.cs` with end-game integration
- [ ] Integrated achievement checks
- [ ] Integrated victory conditions
- [ ] Implemented True Ending unlock
- [ ] Implemented New Game+ system
- [ ] Tested main quest progression
- [ ] Tested victory screens
- [ ] Tested True Ending
- [ ] Tested New Game+ bonuses
- [ ] Built successfully with no errors
- [ ] All existing tests still pass

---

## 📊 Completion Status

**Completed**: [Date]  
**Time Taken**: [Duration]  
**Build Status**: ⚪ Not Built  
**Test Results**: ⚪ Not Tested

### Issues Encountered

```text
[List any issues or deviations from the plan]
```

### Notes

```text
[Any additional notes about quest balance, achievement difficulty, etc.]
```

---

## 🔗 Navigation

- **Previous Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)
- **Current Phase**: Phase 4 - End-Game System (FINAL PHASE)
- **Next Phase**: None (implementation complete!)
- **See Also**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md), [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)

---

**Ready to implement? Copy this entire artifact into chat to begin Phase 4!** 🚀

**Once all 4 phases are complete, you'll have a fully-featured game loop from start to finish!** 🎮
