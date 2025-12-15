using Game.Core.Models;
using Game.Core.Features.SaveLoad;
using Game.Core.Abstractions;
using Serilog;

namespace Game.Core.Features.Achievement.Services;

public class AchievementService
{
    private readonly SaveGameService _saveGameService;
    private readonly List<Models.Achievement> _allAchievements;
    private readonly IGameUI _console;
    
    public AchievementService(SaveGameService saveGameService, IGameUI console)
    {
        _saveGameService = saveGameService;
        _console = console;
        _allAchievements = InitializeAchievements();
    }
    
    public async Task<Models.Achievement?> UnlockAchievementAsync(string achievementId)
    {
        var saveGame = _saveGameService.GetCurrentSave();
        if (saveGame == null)
            return null;
        
        // Check if already unlocked using the existing UnlockedAchievements list
        if (saveGame.UnlockedAchievements.Contains(achievementId))
            return null;
        
        var achievement = _allAchievements.FirstOrDefault(a => a.Id == achievementId);
        if (achievement == null)
            return null;
        
        // Unlock achievement
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.Now;
        
        saveGame.UnlockedAchievements.Add(achievementId);
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
            if (saveGame.UnlockedAchievements.Contains(achievement.Id))
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
        if (saveGame == null)
            return new List<Models.Achievement>();
        
        var unlockedIds = saveGame.UnlockedAchievements;
        var unlocked = _allAchievements.Where(a => unlockedIds.Contains(a.Id)).ToList();
        
        foreach (var achievement in unlocked)
        {
            achievement.IsUnlocked = true;
        }
        
        return await Task.FromResult(unlocked);
    }
    
    private bool CheckCriteria(Models.Achievement achievement, SaveGame saveGame)
    {
        return achievement.Criteria.Type switch
        {
            AchievementType.CompleteQuest => saveGame.CompletedQuests.Any(q => q.Id == achievement.Criteria.RequiredId),
            AchievementType.DefeatEnemies => saveGame.TotalEnemiesDefeated >= achievement.Criteria.RequiredValue,
            AchievementType.ReachLevel => saveGame.Character.Level >= achievement.Criteria.RequiredValue,
            AchievementType.CollectGold => saveGame.Character.Gold >= achievement.Criteria.RequiredValue,
            AchievementType.SurviveTime => saveGame.PlayTimeMinutes >= achievement.Criteria.RequiredValue,
            AchievementType.CompleteGame => saveGame.CompletedQuests.Any(q => q.Id == "main_06_final_boss"),
            AchievementType.CompleteDifficulty => saveGame.CompletedQuests.Any(q => q.Id == "main_06_final_boss") && 
                                                   saveGame.DifficultyLevel == achievement.Criteria.RequiredId,
            AchievementType.Deathless => saveGame.DeathCount == 0 && saveGame.CompletedQuests.Any(q => q.Id == "main_06_final_boss"),
            _ => false
        };
    }
    
    private void ShowAchievementUnlock(Models.Achievement achievement)
    {
        _console.Clear();
        _console.ShowSuccess("═══════════════════════════════════════");
        _console.ShowSuccess("      ACHIEVEMENT UNLOCKED!            ");
        _console.ShowSuccess("═══════════════════════════════════════");
        _console.WriteText($"  {achievement.Icon} {achievement.Title}");
        _console.WriteText($"  {achievement.Description}");
        _console.WriteText($"  Points: {achievement.Points}");
        _console.ShowSuccess("═══════════════════════════════════════");
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
