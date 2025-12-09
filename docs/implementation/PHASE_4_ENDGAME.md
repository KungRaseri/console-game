# Phase 4: End-Game System (Quest, Achievement, Victory)

**Status**: ⚪ Ready to Start  
**Prerequisites**: ✅ Phase 1, 2, 3 complete + CQRS/Vertical Slice Architecture  
**Estimated Time**: 4-6 hours  
**Previous Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)  
**Related**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md), [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)

---

## 📋 Overview

Implement the main quest chain, achievement system, victory conditions, and New Game+ mode using **CQRS + Vertical Slice Architecture**. This phase represents the culmination of the game experience with clear progression, goals, and replay value.

**✅ Pre-Phase Foundation Complete:**

- DifficultySettings ready for New Game+ tracking
- SaveGame has QuestsCompleted, ActiveQuests, Achievements fields
- GameStateService provides centralized state access
- MediatR infrastructure for commands/queries ready
- ApocalypseTimer ready for integration with quest bonuses

---

## 🎯 Goals

1. ✅ Create main quest chain with 5-7 major quests
2. ✅ Implement achievement system with unlock mechanics
3. ✅ Add victory screen and game completion celebration
4. ✅ Implement New Game+ with increased difficulty and rewards
5. ✅ Track statistics for Hall of Fame integration
6. ✅ Add quest journal UI
7. ✅ Update this artifact with completion status

---

## 🏗️ Architecture: Features as Vertical Slices

This phase creates **THREE** new features, each with full CQRS structure:

### Feature 1: `Features/Quest/`
- **Commands**: `StartQuestCommand`, `CompleteQuestCommand`, `UpdateQuestProgressCommand`
- **Queries**: `GetActiveQuestsQuery`, `GetMainQuestChainQuery`, `GetQuestByIdQuery`
- **Services**: `MainQuestService` (quest chain logic), `QuestProgressService` (tracking)
- **Orchestrators**: None needed (commands are simple)

### Feature 2: `Features/Achievement/`
- **Commands**: `UnlockAchievementCommand`, `CheckAchievementProgressCommand`
- **Queries**: `GetUnlockedAchievementsQuery`, `GetAchievementProgressQuery`
- **Services**: `AchievementService` (unlock logic, progress tracking)
- **Orchestrators**: None needed (achievements are reactive)

### Feature 3: `Features/Victory/`
- **Commands**: `TriggerVictoryCommand`, `StartNewGamePlusCommand`
- **Queries**: `GetVictoryStatisticsQuery`, `CanStartNewGamePlusQuery`
- **Services**: `VictoryService` (completion logic), `NewGamePlusService` (NG+ setup)
- **Orchestrators**: `VictoryOrchestrator` (multi-step victory sequence and UI)

---

## 📁 Files to Create

### Feature 1: Quest System

#### 1.1 `Game/Models/Quest.cs` (NEW)

```csharp
namespace Game.Models;

/// <summary>
/// Represents a quest in the game.
/// </summary>
public class Quest
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType Type { get; set; }
    public QuestDifficulty Difficulty { get; set; }
    public bool IsMainQuest { get; set; }
    public int OrderInChain { get; set; } // For main quest chain ordering
    
    // Requirements
    public int RequiredLevel { get; set; }
    public List<string> PrerequisiteQuestIds { get; set; } = new();
    
    // Objectives
    public List<QuestObjective> Objectives { get; set; } = new();
    
    // Rewards
    public int XpReward { get; set; }
    public int GoldReward { get; set; }
    public int ApocalypseBonusMinutes { get; set; } // For Apocalypse mode
    public List<string> ItemRewards { get; set; } = new();
    
    // State
    public QuestStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class QuestObjective
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ObjectiveType Type { get; set; }
    public string TargetId { get; set; } = string.Empty; // Enemy ID, location ID, etc.
    public int Required { get; set; }
    public int Current { get; set; }
    
    public bool IsComplete => Current >= Required;
}

public enum QuestType
{
    Main,
    Side,
    Daily,
    Bounty
}

public enum QuestDifficulty
{
    Easy,
    Medium,
    Hard,
    Epic
}

public enum QuestStatus
{
    NotStarted,
    Active,
    Completed,
    Failed
}

public enum ObjectiveType
{
    KillEnemy,
    ReachLocation,
    CollectItem,
    TalkToNpc,
    SurviveTime
}
```

---

#### 1.2 `Game/Features/Quest/Commands/StartQuestCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Quest.Commands;

public record StartQuestCommand(string QuestId) : IRequest<StartQuestResult>;

public record StartQuestResult(bool Success, string Message, Models.Quest? Quest = null);

public class StartQuestHandler : IRequestHandler<StartQuestCommand, StartQuestResult>
{
    private readonly Services.QuestService _questService;
    
    public StartQuestHandler(Services.QuestService questService)
    {
        _questService = questService;
    }
    
    public async Task<StartQuestResult> Handle(StartQuestCommand request, CancellationToken cancellationToken)
    {
        var result = await _questService.StartQuestAsync(request.QuestId);
        
        if (result.Success)
        {
            return new StartQuestResult(true, $"Quest started: {result.Quest!.Title}", result.Quest);
        }
        
        return new StartQuestResult(false, result.ErrorMessage);
    }
}
```

---

#### 1.3 `Game/Features/Quest/Commands/CompleteQuestCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Quest.Commands;

public record CompleteQuestCommand(string QuestId) : IRequest<CompleteQuestResult>;

public record CompleteQuestResult(bool Success, string Message, QuestRewards? Rewards = null);

public record QuestRewards(int Xp, int Gold, int ApocalypseBonus, List<string> Items);

public class CompleteQuestHandler : IRequestHandler<CompleteQuestCommand, CompleteQuestResult>
{
    private readonly Services.QuestService _questService;
    
    public CompleteQuestHandler(Services.QuestService questService)
    {
        _questService = questService;
    }
    
    public async Task<CompleteQuestResult> Handle(CompleteQuestCommand request, CancellationToken cancellationToken)
    {
        var result = await _questService.CompleteQuestAsync(request.QuestId);
        
        if (result.Success)
        {
            var rewards = new QuestRewards(
                result.Quest!.XpReward,
                result.Quest.GoldReward,
                result.Quest.ApocalypseBonusMinutes,
                result.Quest.ItemRewards
            );
            
            return new CompleteQuestResult(true, $"Quest completed: {result.Quest.Title}", rewards);
        }
        
        return new CompleteQuestResult(false, result.ErrorMessage);
    }
}
```

---

#### 1.4 `Game/Features/Quest/Commands/UpdateQuestProgressCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Quest.Commands;

public record UpdateQuestProgressCommand(string QuestId, string ObjectiveId, int Amount) : IRequest<UpdateQuestProgressResult>;

public record UpdateQuestProgressResult(bool Success, bool ObjectiveCompleted, bool QuestCompleted);

public class UpdateQuestProgressHandler : IRequestHandler<UpdateQuestProgressCommand, UpdateQuestProgressResult>
{
    private readonly Services.QuestProgressService _progressService;
    
    public UpdateQuestProgressHandler(Services.QuestProgressService progressService)
    {
        _progressService = progressService;
    }
    
    public async Task<UpdateQuestProgressResult> Handle(UpdateQuestProgressCommand request, CancellationToken cancellationToken)
    {
        var result = await _progressService.UpdateProgressAsync(request.QuestId, request.ObjectiveId, request.Amount);
        
        return new UpdateQuestProgressResult(
            result.Success,
            result.ObjectiveCompleted,
            result.QuestCompleted
        );
    }
}
```

---

#### 1.5 `Game/Features/Quest/Queries/GetActiveQuestsQuery.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Quest.Queries;

public record GetActiveQuestsQuery : IRequest<List<Models.Quest>>;

public class GetActiveQuestsHandler : IRequestHandler<GetActiveQuestsQuery, List<Models.Quest>>
{
    private readonly Services.QuestService _questService;
    
    public GetActiveQuestsHandler(Services.QuestService questService)
    {
        _questService = questService;
    }
    
    public async Task<List<Models.Quest>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken)
    {
        return await _questService.GetActiveQuestsAsync();
    }
}
```

---

#### 1.6 `Game/Features/Quest/Queries/GetMainQuestChainQuery.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Quest.Queries;

public record GetMainQuestChainQuery : IRequest<List<Models.Quest>>;

public class GetMainQuestChainHandler : IRequestHandler<GetMainQuestChainQuery, List<Models.Quest>>
{
    private readonly Services.MainQuestService _mainQuestService;
    
    public GetMainQuestChainHandler(Services.MainQuestService mainQuestService)
    {
        _mainQuestService = mainQuestService;
    }
    
    public async Task<List<Models.Quest>> Handle(GetMainQuestChainQuery request, CancellationToken cancellationToken)
    {
        return await _mainQuestService.GetMainQuestChainAsync();
    }
}
```

---

#### 1.7 `Game/Features/Quest/Services/QuestService.cs` (NEW)

```csharp
using Game.Models;
using Game.Features.SaveLoad;
using Serilog;

namespace Game.Features.Quest.Services;

public class QuestService
{
    private readonly SaveGameService _saveGameService;
    private readonly MainQuestService _mainQuestService;
    
    public QuestService(SaveGameService saveGameService, MainQuestService mainQuestService)
    {
        _saveGameService = saveGameService;
        _mainQuestService = mainQuestService;
    }
    
    public async Task<(bool Success, Models.Quest? Quest, string ErrorMessage)> StartQuestAsync(string questId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, null, "No active save game");
        
        // Check if quest is already active or completed
        if (saveGame.ActiveQuests.Any(q => q.Id == questId))
            return (false, null, "Quest is already active");
        
        if (saveGame.CompletedQuestIds.Contains(questId))
            return (false, null, "Quest is already completed");
        
        // Get quest definition
        var quest = await GetQuestDefinitionAsync(questId);
        if (quest == null)
            return (false, null, "Quest not found");
        
        // Check prerequisites
        if (!CheckPrerequisites(quest, saveGame))
            return (false, null, "Prerequisites not met");
        
        // Check level requirement
        if (saveGame.Character.Level < quest.RequiredLevel)
            return (false, null, $"Requires level {quest.RequiredLevel}");
        
        // Start quest
        quest.Status = QuestStatus.Active;
        quest.StartedAt = DateTime.Now;
        saveGame.ActiveQuests.Add(quest);
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Information("Quest started: {QuestId} - {QuestTitle}", questId, quest.Title);
        
        return (true, quest, string.Empty);
    }
    
    public async Task<(bool Success, Models.Quest? Quest, string ErrorMessage)> CompleteQuestAsync(string questId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, null, "No active save game");
        
        var quest = saveGame.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest == null)
            return (false, null, "Quest is not active");
        
        // Check if all objectives are complete
        if (!quest.Objectives.All(o => o.IsComplete))
            return (false, null, "Not all objectives complete");
        
        // Complete quest
        quest.Status = QuestStatus.Completed;
        quest.CompletedAt = DateTime.Now;
        
        saveGame.ActiveQuests.Remove(quest);
        saveGame.CompletedQuestIds.Add(quest.Id);
        saveGame.QuestsCompleted++;
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Information("Quest completed: {QuestId} - {QuestTitle}", questId, quest.Title);
        
        return (true, quest, string.Empty);
    }
    
    public async Task<List<Models.Quest>> GetActiveQuestsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        return saveGame?.ActiveQuests ?? new List<Models.Quest>();
    }
    
    private async Task<Models.Quest?> GetQuestDefinitionAsync(string questId)
    {
        // Get from main quest service or quest database
        return await _mainQuestService.GetQuestByIdAsync(questId);
    }
    
    private bool CheckPrerequisites(Models.Quest quest, SaveGame saveGame)
    {
        return quest.PrerequisiteQuestIds.All(prereqId => 
            saveGame.CompletedQuestIds.Contains(prereqId));
    }
}
```

---

#### 1.8 `Game/Features/Quest/Services/MainQuestService.cs` (NEW)

```csharp
using Game.Models;

namespace Game.Features.Quest.Services;

/// <summary>
/// Manages the main quest chain and quest definitions.
/// </summary>
public class MainQuestService
{
    private readonly List<Models.Quest> _allQuests;
    
    public MainQuestService()
    {
        _allQuests = InitializeQuestDatabase();
    }
    
    public async Task<List<Models.Quest>> GetMainQuestChainAsync()
    {
        return await Task.FromResult(
            _allQuests.Where(q => q.IsMainQuest)
                      .OrderBy(q => q.OrderInChain)
                      .ToList()
        );
    }
    
    public async Task<Models.Quest?> GetQuestByIdAsync(string questId)
    {
        return await Task.FromResult(_allQuests.FirstOrDefault(q => q.Id == questId));
    }
    
    private List<Models.Quest> InitializeQuestDatabase()
    {
        return new List<Models.Quest>
        {
            // Main Quest 1: The Awakening
            new Models.Quest
            {
                Id = "main_01_awakening",
                Title = "The Awakening",
                Description = "A mysterious voice calls to you. Investigate the ancient shrine.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Easy,
                IsMainQuest = true,
                OrderInChain = 1,
                RequiredLevel = 1,
                PrerequisiteQuestIds = new List<string>(),
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "reach_shrine",
                        Description = "Reach the Ancient Shrine",
                        Type = ObjectiveType.ReachLocation,
                        TargetId = "ancient_shrine",
                        Required = 1,
                        Current = 0
                    }
                },
                XpReward = 100,
                GoldReward = 50,
                ApocalypseBonusMinutes = 15,
                Status = QuestStatus.NotStarted
            },
            
            // Main Quest 2: The First Trial
            new Models.Quest
            {
                Id = "main_02_first_trial",
                Title = "The First Trial",
                Description = "Defeat the guardian of the shrine to prove your worth.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Medium,
                IsMainQuest = true,
                OrderInChain = 2,
                RequiredLevel = 3,
                PrerequisiteQuestIds = new List<string> { "main_01_awakening" },
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "defeat_guardian",
                        Description = "Defeat the Shrine Guardian",
                        Type = ObjectiveType.KillEnemy,
                        TargetId = "shrine_guardian",
                        Required = 1,
                        Current = 0
                    }
                },
                XpReward = 250,
                GoldReward = 100,
                ApocalypseBonusMinutes = 20,
                Status = QuestStatus.NotStarted
            },
            
            // Main Quest 3: Gathering Power
            new Models.Quest
            {
                Id = "main_03_gathering_power",
                Title = "Gathering Power",
                Description = "Collect ancient artifacts to strengthen yourself for the battles ahead.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Medium,
                IsMainQuest = true,
                OrderInChain = 3,
                RequiredLevel = 5,
                PrerequisiteQuestIds = new List<string> { "main_02_first_trial" },
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "collect_artifacts",
                        Description = "Collect Ancient Artifacts",
                        Type = ObjectiveType.CollectItem,
                        TargetId = "ancient_artifact",
                        Required = 3,
                        Current = 0
                    }
                },
                XpReward = 400,
                GoldReward = 200,
                ApocalypseBonusMinutes = 25,
                Status = QuestStatus.NotStarted
            },
            
            // Main Quest 4: The Dark Prophecy
            new Models.Quest
            {
                Id = "main_04_dark_prophecy",
                Title = "The Dark Prophecy",
                Description = "Seek out the oracle and learn of the coming apocalypse.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Hard,
                IsMainQuest = true,
                OrderInChain = 4,
                RequiredLevel = 8,
                PrerequisiteQuestIds = new List<string> { "main_03_gathering_power" },
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "talk_to_oracle",
                        Description = "Speak with the Oracle",
                        Type = ObjectiveType.TalkToNpc,
                        TargetId = "oracle_of_shadows",
                        Required = 1,
                        Current = 0
                    }
                },
                XpReward = 600,
                GoldReward = 300,
                ApocalypseBonusMinutes = 30,
                Status = QuestStatus.NotStarted
            },
            
            // Main Quest 5: Into the Abyss
            new Models.Quest
            {
                Id = "main_05_into_abyss",
                Title = "Into the Abyss",
                Description = "Enter the Abyssal Depths and confront the source of evil.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Hard,
                IsMainQuest = true,
                OrderInChain = 5,
                RequiredLevel = 12,
                PrerequisiteQuestIds = new List<string> { "main_04_dark_prophecy" },
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "reach_abyss",
                        Description = "Reach the Abyssal Depths",
                        Type = ObjectiveType.ReachLocation,
                        TargetId = "abyssal_depths",
                        Required = 1,
                        Current = 0
                    },
                    new QuestObjective
                    {
                        Id = "defeat_demons",
                        Description = "Defeat Abyssal Demons",
                        Type = ObjectiveType.KillEnemy,
                        TargetId = "abyssal_demon",
                        Required = 5,
                        Current = 0
                    }
                },
                XpReward = 1000,
                GoldReward = 500,
                ApocalypseBonusMinutes = 40,
                Status = QuestStatus.NotStarted
            },
            
            // Main Quest 6: The Final Confrontation
            new Models.Quest
            {
                Id = "main_06_final_boss",
                Title = "The Final Confrontation",
                Description = "Defeat the Dark Lord and save the world from destruction.",
                Type = QuestType.Main,
                Difficulty = QuestDifficulty.Epic,
                IsMainQuest = true,
                OrderInChain = 6,
                RequiredLevel = 15,
                PrerequisiteQuestIds = new List<string> { "main_05_into_abyss" },
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "defeat_dark_lord",
                        Description = "Defeat the Dark Lord",
                        Type = ObjectiveType.KillEnemy,
                        TargetId = "dark_lord",
                        Required = 1,
                        Current = 0
                    }
                },
                XpReward = 2000,
                GoldReward = 1000,
                ApocalypseBonusMinutes = 60,
                Status = QuestStatus.NotStarted
            }
        };
    }
}
```

---

#### 1.9 `Game/Features/Quest/Services/QuestProgressService.cs` (NEW)

```csharp
using Game.Features.SaveLoad;
using Serilog;

namespace Game.Features.Quest.Services;

public class QuestProgressService
{
    private readonly SaveGameService _saveGameService;
    
    public QuestProgressService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<(bool Success, bool ObjectiveCompleted, bool QuestCompleted)> UpdateProgressAsync(
        string questId, string objectiveId, int amount)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return (false, false, false);
        
        var quest = saveGame.ActiveQuests.FirstOrDefault(q => q.Id == questId);
        if (quest == null)
            return (false, false, false);
        
        var objective = quest.Objectives.FirstOrDefault(o => o.Id == objectiveId);
        if (objective == null)
            return (false, false, false);
        
        // Update progress
        objective.Current = Math.Min(objective.Current + amount, objective.Required);
        
        var objectiveCompleted = objective.IsComplete;
        var questCompleted = quest.Objectives.All(o => o.IsComplete);
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Debug("Quest progress updated: {QuestId}/{ObjectiveId} = {Current}/{Required}",
            questId, objectiveId, objective.Current, objective.Required);
        
        return await Task.FromResult((true, objectiveCompleted, questCompleted));
    }
}
```

---

### Feature 2: Achievement System

#### 2.1 `Game/Models/Achievement.cs` (NEW)

```csharp
namespace Game.Models;

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
```

---

#### 2.2 `Game/Features/Achievement/Commands/UnlockAchievementCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Achievement.Commands;

public record UnlockAchievementCommand(string AchievementId) : IRequest<UnlockAchievementResult>;

public record UnlockAchievementResult(bool Success, Models.Achievement? Achievement = null);

public class UnlockAchievementHandler : IRequestHandler<UnlockAchievementCommand, UnlockAchievementResult>
{
    private readonly Services.AchievementService _achievementService;
    
    public UnlockAchievementHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }
    
    public async Task<UnlockAchievementResult> Handle(UnlockAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _achievementService.UnlockAchievementAsync(request.AchievementId);
        
        if (achievement != null)
        {
            return new UnlockAchievementResult(true, achievement);
        }
        
        return new UnlockAchievementResult(false);
    }
}
```

---

#### 2.3 `Game/Features/Achievement/Commands/CheckAchievementProgressCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Achievement.Commands;

public record CheckAchievementProgressCommand : IRequest<List<Models.Achievement>>;

public class CheckAchievementProgressHandler : IRequestHandler<CheckAchievementProgressCommand, List<Models.Achievement>>
{
    private readonly Services.AchievementService _achievementService;
    
    public CheckAchievementProgressHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }
    
    public async Task<List<Models.Achievement>> Handle(CheckAchievementProgressCommand request, CancellationToken cancellationToken)
    {
        return await _achievementService.CheckAllAchievementsAsync();
    }
}
```

---

#### 2.4 `Game/Features/Achievement/Queries/GetUnlockedAchievementsQuery.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Achievement.Queries;

public record GetUnlockedAchievementsQuery : IRequest<List<Models.Achievement>>;

public class GetUnlockedAchievementsHandler : IRequestHandler<GetUnlockedAchievementsQuery, List<Models.Achievement>>
{
    private readonly Services.AchievementService _achievementService;
    
    public GetUnlockedAchievementsHandler(Services.AchievementService achievementService)
    {
        _achievementService = achievementService;
    }
    
    public async Task<List<Models.Achievement>> Handle(GetUnlockedAchievementsQuery request, CancellationToken cancellationToken)
    {
        return await _achievementService.GetUnlockedAchievementsAsync();
    }
}
```

---

#### 2.5 `Game/Features/Achievement/Services/AchievementService.cs` (NEW)

```csharp
using Game.Models;
using Game.Features.SaveLoad;
using Game.Shared.UI;
using Serilog;

namespace Game.Features.Achievement.Services;

public class AchievementService
{
    private readonly SaveGameService _saveGameService;
    private readonly List<Models.Achievement> _allAchievements;
    
    public AchievementService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
        _allAchievements = InitializeAchievements();
    }
    
    public async Task<Models.Achievement?> UnlockAchievementAsync(string achievementId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return null;
        
        // Check if already unlocked
        if (saveGame.Achievements.Any(a => a.Id == achievementId && a.IsUnlocked))
            return null;
        
        var achievement = _allAchievements.FirstOrDefault(a => a.Id == achievementId);
        if (achievement == null)
            return null;
        
        // Unlock achievement
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.Now;
        
        saveGame.Achievements.Add(achievement);
        _saveGameService.SaveGame(saveGame);
        
        // Show unlock notification
        ShowAchievementUnlock(achievement);
        
        Log.Information("Achievement unlocked: {AchievementId} - {Title}", achievementId, achievement.Title);
        
        return await Task.FromResult(achievement);
    }
    
    public async Task<List<Models.Achievement>> CheckAllAchievementsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return new List<Models.Achievement>();
        
        var newlyUnlocked = new List<Models.Achievement>();
        
        foreach (var achievement in _allAchievements)
        {
            // Skip if already unlocked
            if (saveGame.Achievements.Any(a => a.Id == achievement.Id && a.IsUnlocked))
                continue;
            
            // Check criteria
            if (CheckCriteria(achievement, saveGame))
            {
                var unlocked = await UnlockAchievementAsync(achievement.Id);
                if (unlocked != null)
                    newlyUnlocked.Add(unlocked);
            }
        }
        
        return newlyUnlocked;
    }
    
    public async Task<List<Models.Achievement>> GetUnlockedAchievementsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        return await Task.FromResult(saveGame?.Achievements.Where(a => a.IsUnlocked).ToList() ?? new List<Models.Achievement>());
    }
    
    private bool CheckCriteria(Models.Achievement achievement, SaveGame saveGame)
    {
        return achievement.Criteria.Type switch
        {
            AchievementType.CompleteQuest => saveGame.CompletedQuestIds.Contains(achievement.Criteria.RequiredId ?? ""),
            AchievementType.DefeatEnemies => saveGame.EnemiesDefeated >= achievement.Criteria.RequiredValue,
            AchievementType.ReachLevel => saveGame.Character.Level >= achievement.Criteria.RequiredValue,
            AchievementType.CollectGold => saveGame.Character.Gold >= achievement.Criteria.RequiredValue,
            AchievementType.SurviveTime => saveGame.PlayTimeMinutes >= achievement.Criteria.RequiredValue,
            AchievementType.CompleteGame => saveGame.CompletedQuestIds.Contains("main_06_final_boss"),
            AchievementType.CompleteDifficulty => saveGame.CompletedQuestIds.Contains("main_06_final_boss") && 
                                                   saveGame.DifficultySettings.Name == achievement.Criteria.RequiredId,
            AchievementType.Deathless => saveGame.DeathCount == 0 && saveGame.CompletedQuestIds.Contains("main_06_final_boss"),
            _ => false
        };
    }
    
    private void ShowAchievementUnlock(Models.Achievement achievement)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        ConsoleUI.ShowSuccess("      ACHIEVEMENT UNLOCKED!            ");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        ConsoleUI.WriteText($"  {achievement.Icon} {achievement.Title}");
        ConsoleUI.WriteText($"  {achievement.Description}");
        ConsoleUI.WriteText($"  Points: {achievement.Points}");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════");
        Thread.Sleep(3000);
    }
    
    private List<Models.Achievement> InitializeAchievements()
    {
        return new List<Models.Achievement>
        {
            new Models.Achievement
            {
                Id = "first_steps",
                Title = "First Steps",
                Description = "Complete your first quest",
                Icon = "🌟",
                Category = AchievementCategory.Quests,
                Points = 10,
                Criteria = new AchievementCriteria { Type = AchievementType.CompleteQuest, RequiredValue = 1 }
            },
            new Models.Achievement
            {
                Id = "slayer",
                Title = "Slayer",
                Description = "Defeat 100 enemies",
                Icon = "⚔️",
                Category = AchievementCategory.Combat,
                Points = 25,
                Criteria = new AchievementCriteria { Type = AchievementType.DefeatEnemies, RequiredValue = 100 }
            },
            new Models.Achievement
            {
                Id = "master",
                Title = "Master",
                Description = "Reach level 20",
                Icon = "👑",
                Category = AchievementCategory.Mastery,
                Points = 50,
                Criteria = new AchievementCriteria { Type = AchievementType.ReachLevel, RequiredValue = 20 }
            },
            new Models.Achievement
            {
                Id = "savior",
                Title = "Savior of the World",
                Description = "Complete the main quest",
                Icon = "🏆",
                Category = AchievementCategory.Quests,
                Points = 100,
                Criteria = new AchievementCriteria { Type = AchievementType.CompleteGame, RequiredValue = 1 }
            },
            new Models.Achievement
            {
                Id = "apocalypse_survivor",
                Title = "Apocalypse Survivor",
                Description = "Complete the game on Apocalypse difficulty",
                Icon = "💀",
                Category = AchievementCategory.Survival,
                Points = 200,
                IsSecret = true,
                Criteria = new AchievementCriteria { Type = AchievementType.CompleteDifficulty, RequiredId = "Apocalypse" }
            },
            new Models.Achievement
            {
                Id = "deathless",
                Title = "Deathless",
                Description = "Complete the game without dying",
                Icon = "✨",
                Category = AchievementCategory.Mastery,
                Points = 500,
                IsSecret = true,
                Criteria = new AchievementCriteria { Type = AchievementType.Deathless, RequiredValue = 1 }
            }
        };
    }
}
```

---

### Feature 3: Victory System

#### 3.1 `Game/Features/Victory/Commands/TriggerVictoryCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Victory.Commands;

public record TriggerVictoryCommand : IRequest<TriggerVictoryResult>;

public record TriggerVictoryResult(bool Success, VictoryStatistics? Statistics = null);

public record VictoryStatistics(
    string PlayerName,
    string ClassName,
    int FinalLevel,
    string Difficulty,
    int PlayTimeMinutes,
    int QuestsCompleted,
    int EnemiesDefeated,
    int DeathCount,
    int AchievementsUnlocked,
    int TotalGoldEarned
);

public class TriggerVictoryHandler : IRequestHandler<TriggerVictoryCommand, TriggerVictoryResult>
{
    private readonly Services.VictoryService _victoryService;
    
    public TriggerVictoryHandler(Services.VictoryService victoryService)
    {
        _victoryService = victoryService;
    }
    
    public async Task<TriggerVictoryResult> Handle(TriggerVictoryCommand request, CancellationToken cancellationToken)
    {
        var statistics = await _victoryService.CalculateVictoryStatisticsAsync();
        
        if (statistics != null)
        {
            await _victoryService.MarkGameCompleteAsync();
            return new TriggerVictoryResult(true, statistics);
        }
        
        return new TriggerVictoryResult(false);
    }
}
```

---

#### 3.2 `Game/Features/Victory/Commands/StartNewGamePlusCommand.cs` (NEW)

```csharp
using MediatR;

namespace Game.Features.Victory.Commands;

public record StartNewGamePlusCommand : IRequest<StartNewGamePlusResult>;

public record StartNewGamePlusResult(bool Success, string Message);

public class StartNewGamePlusHandler : IRequestHandler<StartNewGamePlusCommand, StartNewGamePlusResult>
{
    private readonly Services.NewGamePlusService _ngPlusService;
    
    public StartNewGamePlusHandler(Services.NewGamePlusService ngPlusService)
    {
        _ngPlusService = ngPlusService;
    }
    
    public async Task<StartNewGamePlusResult> Handle(StartNewGamePlusCommand request, CancellationToken cancellationToken)
    {
        var result = await _ngPlusService.StartNewGamePlusAsync();
        
        if (result.Success)
        {
            return new StartNewGamePlusResult(true, "New Game+ started!");
        }
        
        return new StartNewGamePlusResult(false, "Failed to start New Game+");
    }
}
```

---

#### 3.3 `Game/Features/Victory/Services/VictoryService.cs` (NEW)

```csharp
using Game.Features.SaveLoad;
using Game.Features.Victory.Commands;
using Serilog;

namespace Game.Features.Victory.Services;

public class VictoryService
{
    private readonly SaveGameService _saveGameService;
    
    public VictoryService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<VictoryStatistics?> CalculateVictoryStatisticsAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return null;
        
        var statistics = new VictoryStatistics(
            saveGame.Character.Name,
            saveGame.Character.ClassName,
            saveGame.Character.Level,
            saveGame.DifficultySettings.Name,
            saveGame.PlayTimeMinutes,
            saveGame.QuestsCompleted,
            saveGame.EnemiesDefeated,
            saveGame.DeathCount,
            saveGame.Achievements.Count(a => a.IsUnlocked),
            saveGame.TotalGoldEarned
        );
        
        Log.Information("Victory statistics calculated for {PlayerName}", saveGame.Character.Name);
        
        return await Task.FromResult(statistics);
    }
    
    public async Task MarkGameCompleteAsync()
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return;
        
        saveGame.GameCompleted = true;
        saveGame.CompletionDate = DateTime.Now;
        
        _saveGameService.SaveGame(saveGame);
        
        Log.Information("Game marked as completed for {PlayerName}", saveGame.Character.Name);
        
        await Task.CompletedTask;
    }
}
```

---

#### 3.4 `Game/Features/Victory/Services/NewGamePlusService.cs` (NEW)

```csharp
using Game.Features.SaveLoad;
using Game.Models;
using Serilog;

namespace Game.Features.Victory.Services;

public class NewGamePlusService
{
    private readonly SaveGameService _saveGameService;
    
    public NewGamePlusService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    public async Task<(bool Success, SaveGame? NewSave)> StartNewGamePlusAsync()
    {
        var completedSave = _saveGameService.GetCurrentSave();
        if (completedSave == null || !completedSave.GameCompleted)
            return (false, null);
        
        // Create New Game+ character with bonuses
        var ngPlusCharacter = new Character
        {
            Name = completedSave.Character.Name,
            ClassName = completedSave.Character.ClassName,
            Level = 1, // Start at level 1
            MaxHealth = completedSave.Character.MaxHealth + 50, // Bonus HP
            Health = completedSave.Character.MaxHealth + 50,
            MaxMana = completedSave.Character.MaxMana + 50, // Bonus Mana
            Mana = completedSave.Character.MaxMana + 50,
            Strength = completedSave.Character.Strength + 5, // Bonus stats
            Intelligence = completedSave.Character.Intelligence + 5,
            Dexterity = completedSave.Character.Dexterity + 5,
            Gold = 500 // Starting gold bonus
        };
        
        // Create harder difficulty for NG+
        var ngPlusDifficulty = new DifficultySettings
        {
            Name = $"{completedSave.DifficultySettings.Name} NG+",
            Description = "New Game+ difficulty with increased challenge and rewards",
            EnemyHealthMultiplier = completedSave.DifficultySettings.EnemyHealthMultiplier * 1.5,
            EnemyDamageMultiplier = completedSave.DifficultySettings.EnemyDamageMultiplier * 1.5,
            XpMultiplier = completedSave.DifficultySettings.XpMultiplier * 1.25,
            GoldMultiplier = completedSave.DifficultySettings.GoldMultiplier * 1.25,
            ItemDropMultiplier = completedSave.DifficultySettings.ItemDropMultiplier * 1.25,
            IsApocalypse = completedSave.DifficultySettings.IsApocalypse,
            ApocalypseTimeLimitMinutes = completedSave.DifficultySettings.ApocalypseTimeLimitMinutes
        };
        
        // Create new save
        var ngPlusSave = _saveGameService.CreateNewGame(ngPlusCharacter, ngPlusDifficulty);
        ngPlusSave.IsNewGamePlus = true;
        ngPlusSave.NewGamePlusGeneration = (completedSave.NewGamePlusGeneration ?? 0) + 1;
        
        // Carry over achievements
        ngPlusSave.Achievements = completedSave.Achievements;
        
        _saveGameService.SaveGame(ngPlusSave);
        
        Log.Information("New Game+ started. Generation: {Generation}", ngPlusSave.NewGamePlusGeneration);
        
        return await Task.FromResult((true, ngPlusSave));
    }
}
```

---

#### 3.5 `Game/Features/Victory/Orchestrators/VictoryOrchestrator.cs` (NEW)

```csharp
using MediatR;
using Game.Shared.UI;
using Game.Features.Victory.Commands;
using Serilog;

namespace Game.Features.Victory.Orchestrators;

/// <summary>
/// Orchestrates the multi-step victory sequence with UI interactions.
/// </summary>
public class VictoryOrchestrator
{
    private readonly IMediator _mediator;
    
    public VictoryOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<bool> ShowVictorySequenceAsync()
    {
        // Trigger victory command
        var victoryResult = await _mediator.Send(new TriggerVictoryCommand());
        
        if (!victoryResult.Success || victoryResult.Statistics == null)
            return false;
        
        var stats = victoryResult.Statistics;
        
        // Victory sequence
        ConsoleUI.Clear();
        await ShowDramaticVictoryAsync();
        await ShowStatisticsAsync(stats);
        await ShowAchievementsAsync();
        
        // Offer New Game+
        return await OfferNewGamePlusAsync();
    }
    
    private async Task ShowDramaticVictoryAsync()
    {
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(500);
        ConsoleUI.ShowSuccess("        THE DARK LORD HAS FALLEN!              ");
        await Task.Delay(1000);
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(1500);
        
        Console.Clear();
        ConsoleUI.ShowSuccess("Light returns to the world...");
        await Task.Delay(2000);
        
        ConsoleUI.ShowSuccess("The apocalypse has been averted...");
        await Task.Delay(2000);
        
        ConsoleUI.ShowSuccess("You are the savior of the realm!");
        await Task.Delay(2000);
        
        Console.Clear();
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        ConsoleUI.ShowSuccess("                                               ");
        ConsoleUI.ShowSuccess("              🏆 VICTORY! 🏆                   ");
        ConsoleUI.ShowSuccess("                                               ");
        ConsoleUI.ShowSuccess("═══════════════════════════════════════════════");
        await Task.Delay(3000);
    }
    
    private async Task ShowStatisticsAsync(VictoryStatistics stats)
    {
        Console.Clear();
        ConsoleUI.ShowBanner("Final Statistics", $"{stats.PlayerName} the {stats.ClassName}");
        Console.WriteLine();
        
        var headers = new[] { "Stat", "Value" };
        var rows = new List<string[]>
        {
            new[] { "Final Level", stats.FinalLevel.ToString() },
            new[] { "Difficulty", stats.Difficulty },
            new[] { "Play Time", $"{stats.PlayTimeMinutes / 60}h {stats.PlayTimeMinutes % 60}m" },
            new[] { "Quests Completed", stats.QuestsCompleted.ToString() },
            new[] { "Enemies Defeated", stats.EnemiesDefeated.ToString() },
            new[] { "Deaths", stats.DeathCount.ToString() },
            new[] { "Achievements", $"{stats.AchievementsUnlocked} unlocked" },
            new[] { "Total Gold Earned", $"{stats.TotalGoldEarned}g" }
        };
        
        ConsoleUI.ShowTable("Your Journey", headers, rows);
        
        Log.Information("Victory statistics displayed for {PlayerName}", stats.PlayerName);
        
        await Task.Delay(5000);
    }
    
    private async Task ShowAchievementsAsync()
    {
        Console.Clear();
        ConsoleUI.ShowBanner("Achievements", "Your Accomplishments");
        Console.WriteLine();
        
        var query = new Features.Achievement.Queries.GetUnlockedAchievementsQuery();
        var achievements = await _mediator.Send(query);
        
        if (achievements.Any())
        {
            foreach (var achievement in achievements)
            {
                ConsoleUI.WriteText($"{achievement.Icon} {achievement.Title} - {achievement.Points} points");
            }
        }
        else
        {
            ConsoleUI.WriteText("No achievements unlocked yet.");
        }
        
        await Task.Delay(4000);
    }
    
    private async Task<bool> OfferNewGamePlusAsync()
    {
        Console.Clear();
        ConsoleUI.ShowBanner("New Game+", "Continue Your Journey");
        Console.WriteLine();
        
        ConsoleUI.WriteText("You have completed the game!");
        ConsoleUI.WriteText("New Game+ is now available with:");
        ConsoleUI.WriteText("  • Increased enemy difficulty");
        ConsoleUI.WriteText("  • Better rewards (XP, gold, items)");
        ConsoleUI.WriteText("  • Keep your achievements");
        ConsoleUI.WriteText("  • Start with bonus stats");
        Console.WriteLine();
        
        if (ConsoleUI.Confirm("Start New Game+?"))
        {
            var result = await _mediator.Send(new StartNewGamePlusCommand());
            
            if (result.Success)
            {
                ConsoleUI.ShowSuccess(result.Message);
                await Task.Delay(2000);
                return true;
            }
        }
        
        return false;
    }
}
```

---

## 📝 Files to Modify

### `Game/Program.cs` - Register Quest, Achievement, Victory Services

**FIND** the service registration section:

```csharp
// Register feature services
services.AddScoped<CharacterCreationOrchestrator>();
// ... other services ...
```

**ADD** these registrations:

```csharp
// Quest feature
services.AddScoped<Game.Features.Quest.Services.QuestService>();
services.AddScoped<Game.Features.Quest.Services.MainQuestService>();
services.AddScoped<Game.Features.Quest.Services.QuestProgressService>();

// Achievement feature
services.AddScoped<Game.Features.Achievement.Services.AchievementService>();

// Victory feature
services.AddScoped<Game.Features.Victory.Services.VictoryService>();
services.AddScoped<Game.Features.Victory.Services.NewGamePlusService>();
services.AddScoped<Game.Features.Victory.Orchestrators.VictoryOrchestrator>();
```

---

### `Game/Models/SaveGame.cs` - Add New Fields

**ADD** these properties if they don't exist:

```csharp
// Quest tracking
public List<Quest> ActiveQuests { get; set; } = new();
public List<string> CompletedQuestIds { get; set; } = new();
public int QuestsCompleted { get; set; }

// Achievement tracking
public List<Achievement> Achievements { get; set; } = new();

// Game completion
public bool GameCompleted { get; set; }
public DateTime? CompletionDate { get; set; }

// New Game+
public bool IsNewGamePlus { get; set; }
public int? NewGamePlusGeneration { get; set; }

// Statistics
public int EnemiesDefeated { get; set; }
public int DeathCount { get; set; }
public int PlayTimeMinutes { get; set; }
public int TotalGoldEarned { get; set; }
```

---

### `Game/GameEngine.cs` - Integrate Quest, Achievement, Victory Systems

**ADD** using statements:

```csharp
using Game.Features.Quest.Commands;
using Game.Features.Achievement.Commands;
using Game.Features.Victory.Commands;
using Game.Features.Victory.Orchestrators;
```

**ADD** fields:

```csharp
private readonly VictoryOrchestrator _victoryOrchestrator;
```

**UPDATE** constructor:

```csharp
public GameEngine(
    IMediator mediator,
    SaveGameService saveGameService,
    VictoryOrchestrator victoryOrchestrator,
    // ... other parameters
    )
{
    // ... existing code ...
    _victoryOrchestrator = victoryOrchestrator;
}
```

**AFTER** defeating the final boss:

```csharp
private async Task OnEnemyDefeatedAsync(Enemy enemy)
{
    // ... existing logic ...
    
    // Check if final boss
    if (enemy.Id == "dark_lord")
    {
        await HandleFinalBossVictoryAsync();
    }
}

private async Task HandleFinalBossVictoryAsync()
{
    // Complete final quest
    await _mediator.Send(new CompleteQuestCommand("main_06_final_boss"));
    
    // Check achievements
    await _mediator.Send(new CheckAchievementProgressCommand());
    
    // Show victory sequence
    var startedNgPlus = await _victoryOrchestrator.ShowVictorySequenceAsync();
    
    if (startedNgPlus)
    {
        _state = GameState.InGame;
        // Game continues in NG+
    }
    else
    {
        _state = GameState.MainMenu;
    }
}
```

---

## 🧪 Testing Checklist

### Manual Testing

1. **Quest System**:
   - [ ] Start main quest 1
   - [ ] Complete objectives
   - [ ] Receive rewards (XP, gold, apocalypse bonus)
   - [ ] Quest unlocks next quest in chain
   - [ ] Quest journal displays correctly

2. **Achievement System**:
   - [ ] First quest completion unlocks "First Steps"
   - [ ] Reaching level 20 unlocks "Master"
   - [ ] Defeating 100 enemies unlocks "Slayer"
   - [ ] Completing game unlocks "Savior of the World"
   - [ ] Secret achievements unlock correctly

3. **Victory System**:
   - [ ] Defeat final boss (Dark Lord)
   - [ ] Victory sequence plays
   - [ ] Statistics display correctly
   - [ ] Achievements shown
   - [ ] New Game+ offer appears

4. **New Game+**:
   - [ ] NG+ starts with bonus stats
   - [ ] Difficulty increased (enemies harder)
   - [ ] Rewards increased (XP, gold multipliers)
   - [ ] Achievements persist
   - [ ] Can chain multiple NG+ runs

### Edge Cases

- [ ] Try to start quest without meeting prerequisites
- [ ] Try to complete quest with incomplete objectives
- [ ] Achievement unlocks persist across saves
- [ ] Victory triggers only after final boss
- [ ] NG+ can't start without completing game
- [ ] NG+ generation number increments correctly

---

## ✅ Completion Checklist

- [ ] Created `Quest.cs` model with objectives
- [ ] Created `Achievement.cs` model with criteria
- [ ] Created Quest feature (Commands, Queries, Services)
- [ ] Created Achievement feature (Commands, Queries, Service)
- [ ] Created Victory feature (Commands, Services, Orchestrator)
- [ ] Registered all services in `Program.cs`
- [ ] Updated `SaveGame.cs` with new fields
- [ ] Integrated quest completion in `GameEngine`
- [ ] Integrated achievement unlocks in `GameEngine`
- [ ] Integrated victory sequence in `GameEngine`
- [ ] Implemented New Game+ system
- [ ] Tested main quest chain (6 quests)
- [ ] Tested achievement unlocks
- [ ] Tested victory screen
- [ ] Tested New Game+
- [ ] Built successfully with `dotnet build`
- [ ] All tests pass (`dotnet test`)

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
[Any notes about quest design, achievement balance, NG+ difficulty scaling, etc.]
```

---

## 🔗 Navigation

- **Previous Phase**: [Phase 3: Apocalypse Mode](./PHASE_3_APOCALYPSE_MODE.md)
- **Current Phase**: Phase 4 - End-Game System
- **See Also**: [Phase 1: Difficulty](./PHASE_1_DIFFICULTY_FOUNDATION.md), [Phase 2: Death System](./PHASE_2_DEATH_SYSTEM.md)

---

**Ready to implement? Copy this entire artifact into chat to begin Phase 4!** 🚀
