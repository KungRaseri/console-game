using Bogus;
using Game.Data.Models;
using Game.Models;
using Game.Services;
using Game.Utilities;
using Serilog;

namespace Game.Generators;

/// <summary>
/// Generates procedural quests using templates, NPC traits, and enemy traits.
/// </summary>
public static class QuestGenerator
{
    private static readonly Random _random = new();
    
    /// <summary>
    /// Generate a random quest from any available template.
    /// </summary>
    public static Quest Generate()
    {
        var faker = new Faker();
        var data = GameDataService.Instance.QuestTemplates;
        
        // Pick random quest type
        var questType = faker.PickRandom(new[] { "Kill", "Fetch", "Escort", "Investigate", "Delivery" });
        
        return GenerateByType(questType);
    }
    
    /// <summary>
    /// Generate a quest of a specific type.
    /// </summary>
    public static Quest GenerateByType(string questType)
    {
        var faker = new Faker();
        var data = GameDataService.Instance.QuestTemplates;
        
        // Pick random difficulty
        var difficulty = faker.PickRandom(new[] { "Easy", "Medium", "Hard" });
        
        return GenerateByTypeAndDifficulty(questType, difficulty);
    }
    
    /// <summary>
    /// Generate a quest with specific type and difficulty.
    /// </summary>
    public static Quest GenerateByTypeAndDifficulty(string questType, string difficulty)
    {
        var faker = new Faker();
        var data = GameDataService.Instance.QuestTemplates;
        
        // Get the appropriate difficulty tier
        QuestDifficultyTierData? tierData = questType switch
        {
            "Kill" => data.Kill,
            "Fetch" => data.Fetch,
            "Escort" => data.Escort,
            "Investigate" => data.Investigate,
            "Delivery" => data.Delivery,
            _ => null
        };
        
        if (tierData == null)
        {
            Log.Warning("Unknown quest type: {QuestType}", questType);
            return GenerateDefaultQuest();
        }
        
        // Get templates for difficulty
        var templates = difficulty switch
        {
            "Easy" => tierData.Easy,
            "Medium" => tierData.Medium,
            "Hard" => tierData.Hard,
            _ => tierData.Easy
        };
        
        if (templates.Count == 0)
        {
            Log.Warning("No templates found for {QuestType} - {Difficulty}", questType, difficulty);
            return GenerateDefaultQuest();
        }
        
        // Pick random template
        var template = GameDataService.GetRandom(templates.Values.ToList());
        
        // Generate quest from template
        return GenerateFromTemplate(template, questType, difficulty, faker);
    }
    
    /// <summary>
    /// Generate a quest from a template.
    /// </summary>
    private static Quest GenerateFromTemplate(QuestTemplateTraitData template, string questType, string difficulty, Faker faker)
    {
        var quest = new Quest
        {
            Title = template.DisplayName,
            QuestType = questType.ToLower(),
            Difficulty = difficulty.ToLower(),
            Type = DetermineQuestType(template, questType) // New Phase 4 field
        };
        
        // Apply all traits from template
        TraitApplicator.ApplyTraits(quest, template.Traits);
        
        // Extract common traits
        if (quest.Traits.ContainsKey("baseGoldReward"))
            quest.GoldReward = quest.Traits["baseGoldReward"].AsInt();
            
        if (quest.Traits.ContainsKey("baseXpReward"))
            quest.XpReward = quest.Traits["baseXpReward"].AsInt();
            
        if (quest.Traits.ContainsKey("timeLimit"))
            quest.TimeLimit = quest.Traits["timeLimit"].AsInt();
            
        if (quest.Traits.ContainsKey("location"))
            quest.Location = quest.Traits["location"].AsString();
        
        // Generate quest-specific details
        switch (questType.ToLower())
        {
            case "kill":
                GenerateKillQuestDetails(quest, faker);
                break;
            case "fetch":
                GenerateFetchQuestDetails(quest, faker);
                break;
            case "escort":
                GenerateEscortQuestDetails(quest, faker);
                break;
            case "investigate":
                GenerateInvestigateQuestDetails(quest, faker);
                break;
            case "delivery":
                GenerateDeliveryQuestDetails(quest, faker);
                break;
        }
        
        // Initialize objectives dictionary (Phase 4 enhancement)
        InitializeObjectives(quest);
        
        // Generate description
        GenerateDescription(quest);
        
        // Assign quest giver
        AssignQuestGiver(quest, faker);
        
        Log.Debug("Generated quest: {Title} ({Type}/{Difficulty}) - Reward: {Gold}g, {Xp}xp", 
            quest.Title, quest.QuestType, quest.Difficulty, quest.GoldReward, quest.XpReward);
        
        return quest;
    }
    
    /// <summary>
    /// Generate kill quest details (target type, quantity, etc.).
    /// </summary>
    private static void GenerateKillQuestDetails(Quest quest, Faker faker)
    {
        // Get target type from traits
        if (quest.Traits.ContainsKey("targetType"))
        {
            quest.TargetType = quest.Traits["targetType"].AsString();
        }
        
        // Determine quantity
        int minQty = quest.Traits.ContainsKey("minQuantity") ? quest.Traits["minQuantity"].AsInt() : 1;
        int maxQty = quest.Traits.ContainsKey("maxQuantity") ? quest.Traits["maxQuantity"].AsInt() : 5;
        quest.Quantity = faker.Random.Int(minQty, maxQty);
        
        // Generate specific enemy name using enemy generator
        if (!string.IsNullOrEmpty(quest.TargetType))
        {
            // Use quest difficulty to determine enemy level
            int baseLevel = quest.Difficulty.ToLower() switch
            {
                "easy" => 5,
                "medium" => 15,
                "hard" => 30,
                _ => 10
            };
            
            var enemy = EnemyGenerator.GenerateByType(
                GetEnemyTypeFromString(quest.TargetType),
                baseLevel,
                GetDifficultyFromQuestDifficulty(quest.Difficulty)
            );
            quest.TargetName = enemy.Name;
            
            // Scale rewards based on enemy strength
            quest.GoldReward = (int)(quest.GoldReward * (1 + enemy.Level * 0.1));
            quest.XpReward = (int)(quest.XpReward * (1 + enemy.Level * 0.1));
        }
        
        // Generate location if not set
        if (string.IsNullOrEmpty(quest.Location))
        {
            quest.Location = GenerateLocation(quest.TargetType, faker);
        }
    }
    
    /// <summary>
    /// Generate fetch quest details (item type, quantity, etc.).
    /// </summary>
    private static void GenerateFetchQuestDetails(Quest quest, Faker faker)
    {
        // Determine quantity
        int minQty = quest.Traits.ContainsKey("minQuantity") ? quest.Traits["minQuantity"].AsInt() : 1;
        int maxQty = quest.Traits.ContainsKey("maxQuantity") ? quest.Traits["maxQuantity"].AsInt() : 10;
        quest.Quantity = faker.Random.Int(minQty, maxQty);
        
        // Generate item names based on item type
        if (quest.Traits.ContainsKey("itemType"))
        {
            var itemType = quest.Traits["itemType"].AsString();
            quest.TargetName = GenerateItemName(itemType, faker);
        }
        
        // Generate location if not set
        if (string.IsNullOrEmpty(quest.Location))
        {
            quest.Location = faker.PickRandom("Forest", "Cave", "Dungeon", "Ruins", "Mountains", "Swamp");
        }
    }
    
    /// <summary>
    /// Generate escort quest details.
    /// </summary>
    private static void GenerateEscortQuestDetails(Quest quest, Faker faker)
    {
        // Generate NPC to escort
        var npc = NpcGenerator.Generate();
        quest.TargetName = npc.Name;
        
        // Set location based on distance
        var distance = quest.Traits.ContainsKey("distance") ? quest.Traits["distance"].AsString() : "short";
        quest.Location = distance switch
        {
            "short" => faker.PickRandom("Nearby Town", "Next Village", "Trading Post"),
            "medium" => faker.PickRandom("Distant City", "Port Town", "Mountain Fortress"),
            "long" => faker.PickRandom("Capital City", "Foreign Kingdom", "Sacred Temple"),
            _ => "Unknown Destination"
        };
    }
    
    /// <summary>
    /// Generate investigate quest details.
    /// </summary>
    private static void GenerateInvestigateQuestDetails(Quest quest, Faker faker)
    {
        // Determine number of clues
        int minClues = quest.Traits.ContainsKey("minClues") ? quest.Traits["minClues"].AsInt() : 3;
        int maxClues = quest.Traits.ContainsKey("maxClues") ? quest.Traits["maxClues"].AsInt() : 8;
        quest.Quantity = faker.Random.Int(minClues, maxClues);
        
        quest.TargetName = "clues";
    }
    
    /// <summary>
    /// Generate delivery quest details.
    /// </summary>
    private static void GenerateDeliveryQuestDetails(Quest quest, Faker faker)
    {
        // Generate package/item name
        quest.TargetName = faker.PickRandom("Package", "Letter", "Crate", "Parcel", "Documents");
        
        // Set destination based on distance
        var distance = quest.Traits.ContainsKey("distance") ? quest.Traits["distance"].AsString() : "short";
        quest.Location = distance switch
        {
            "short" => faker.PickRandom("Nearby Town", "Next Village"),
            "medium" => faker.PickRandom("Distant City", "Trading Hub"),
            "long" => faker.PickRandom("Capital", "Foreign Land", "Remote Outpost"),
            _ => "Unknown Location"
        };
    }
    
    /// <summary>
    /// Generate quest description from template.
    /// </summary>
    private static void GenerateDescription(Quest quest)
    {
        if (quest.Traits.ContainsKey("description"))
        {
            var template = quest.Traits["description"].AsString();
            
            // Replace placeholders
            quest.Description = template
                .Replace("{quantity}", quest.Quantity.ToString())
                .Replace("{target}", quest.TargetName)
                .Replace("{location}", quest.Location)
                .Replace("{itemName}", quest.TargetName);
        }
        else
        {
            quest.Description = $"{quest.Title}: A {quest.Difficulty} {quest.QuestType} quest.";
        }
    }
    
    /// <summary>
    /// Assign a quest giver NPC.
    /// </summary>
    private static void AssignQuestGiver(Quest quest, Faker faker)
    {
        var npc = NpcGenerator.Generate();
        
        // Make sure NPC can give quests (check questGiverChance trait)
        if (npc.Traits.ContainsKey("questGiverChance"))
        {
            var chance = npc.Traits["questGiverChance"].AsInt();
            if (faker.Random.Int(0, 100) > chance)
            {
                // Generate another NPC with better quest giving chance
                npc = NpcGenerator.Generate();
            }
        }
        
        quest.QuestGiverId = npc.Id;
        quest.QuestGiverName = npc.Name;
    }
    
    /// <summary>
    /// Generate a default fallback quest.
    /// </summary>
    private static Quest GenerateDefaultQuest()
    {
        return new Quest
        {
            Title = "Simple Task",
            Description = "Complete a simple task for the locals.",
            QuestType = "fetch",
            Difficulty = "easy",
            GoldReward = 50,
            XpReward = 100,
            Quantity = 1
        };
    }
    
    /// <summary>
    /// Convert quest difficulty to enemy difficulty.
    /// </summary>
    private static EnemyDifficulty GetDifficultyFromQuestDifficulty(string questDifficulty)
    {
        return questDifficulty.ToLower() switch
        {
            "easy" => EnemyDifficulty.Easy,
            "medium" => EnemyDifficulty.Hard,
            "hard" => EnemyDifficulty.Boss,
            _ => EnemyDifficulty.Normal
        };
    }
    
    /// <summary>
    /// Convert target type string to EnemyType enum.
    /// </summary>
    private static EnemyType GetEnemyTypeFromString(string targetType)
    {
        return targetType.ToLower() switch
        {
            "beast" => EnemyType.Beast,
            "undead" => EnemyType.Undead,
            "demon" => EnemyType.Demon,
            "elemental" => EnemyType.Elemental,
            "dragon" => EnemyType.Dragon,
            "humanoid" => EnemyType.Humanoid,
            _ => EnemyType.Beast
        };
    }
    
    /// <summary>
    /// Generate a location based on target type.
    /// </summary>
    private static string GenerateLocation(string targetType, Faker faker)
    {
        return targetType.ToLower() switch
        {
            "beast" => faker.PickRandom("Forest", "Wilderness", "Jungle", "Plains"),
            "undead" => faker.PickRandom("Graveyard", "Crypt", "Ruins", "Haunted Manor"),
            "demon" => faker.PickRandom("Hell Gate", "Demonic Portal", "Corrupted Temple", "Abyss"),
            "elemental" => faker.PickRandom("Elemental Plane", "Volcano", "Storm Peak", "Ancient Ruins"),
            "dragon" => faker.PickRandom("Dragon's Lair", "Mountain Peak", "Ancient Cave", "Sky Fortress"),
            "humanoid" => faker.PickRandom("Bandit Camp", "Fort", "Village", "Arena"),
            _ => faker.PickRandom("Unknown Location", "Mysterious Place", "Dangerous Area")
        };
    }
    
    /// <summary>
    /// Generate an item name based on item type.
    /// </summary>
    private static string GenerateItemName(string itemType, Faker faker)
    {
        return itemType.ToLower() switch
        {
            "consumable" => faker.PickRandom("Healing Herbs", "Mana Crystals", "Antidote Ingredients"),
            "material" => faker.PickRandom("Iron Ore", "Precious Gems", "Rare Wood"),
            "questitem" => faker.PickRandom("Ancient Artifact", "Sacred Relic", "Magical Tome", "Mysterious Orb"),
            "treasure" => faker.PickRandom("Golden Chalice", "Jeweled Crown", "Ancient Coins"),
            "weapon" => faker.PickRandom("Legendary Sword", "Enchanted Bow", "Mystic Staff"),
            _ => faker.PickRandom("Mysterious Item", "Unknown Object")
        };
    }
    
    /// <summary>
    /// Determine quest type (main, side, legendary) based on template and quest type (Phase 4).
    /// </summary>
    private static string DetermineQuestType(QuestTemplateTraitData template, string questType)
    {
        // Check if template has explicit quest type trait
        if (template.Traits.ContainsKey("questCategory"))
        {
            return template.Traits["questCategory"].AsString();
        }
        
        // Hard difficulty quests have a chance to be legendary
        if (template.Traits.ContainsKey("baseGoldReward"))
        {
            var goldReward = template.Traits["baseGoldReward"].AsInt();
            if (goldReward > 500)
            {
                return "legendary";
            }
        }
        
        // Default to side quest
        return "side";
    }
    
    /// <summary>
    /// Initialize objectives dictionary for quest tracking (Phase 4 enhancement).
    /// </summary>
    private static void InitializeObjectives(Quest quest)
    {
        // Convert legacy quest data to objectives format
        switch (quest.QuestType.ToLower())
        {
            case "kill":
                if (!string.IsNullOrEmpty(quest.TargetName))
                {
                    quest.Objectives[$"Kill {quest.TargetName}"] = quest.Quantity;
                    quest.ObjectiveProgress[$"Kill {quest.TargetName}"] = 0;
                }
                break;
                
            case "fetch":
                if (!string.IsNullOrEmpty(quest.TargetName))
                {
                    quest.Objectives[$"Collect {quest.TargetName}"] = quest.Quantity;
                    quest.ObjectiveProgress[$"Collect {quest.TargetName}"] = 0;
                }
                break;
                
            case "escort":
                if (!string.IsNullOrEmpty(quest.TargetName))
                {
                    quest.Objectives[$"Escort {quest.TargetName} to {quest.Location}"] = 1;
                    quest.ObjectiveProgress[$"Escort {quest.TargetName} to {quest.Location}"] = 0;
                }
                break;
                
            case "investigate":
                quest.Objectives[$"Investigate {quest.Location}"] = 1;
                quest.ObjectiveProgress[$"Investigate {quest.Location}"] = 0;
                break;
                
            case "delivery":
                if (!string.IsNullOrEmpty(quest.TargetName))
                {
                    quest.Objectives[$"Deliver to {quest.TargetName}"] = 1;
                    quest.ObjectiveProgress[$"Deliver to {quest.TargetName}"] = 0;
                }
                break;
        }
    }
}

