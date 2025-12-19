using Bogus;
using Game.Shared.Data;
using Game.Shared.Data.Models;
using Game.Core.Models;
using Game.Core.Services;
using Game.Core.Utilities;
using Game.Shared.Models;
using Serilog;

namespace Game.Core.Generators;

/// <summary>
/// Generates procedural quests using v4.0 catalog system with weighted selection.
/// Supports quest templates, locations, objectives, and scaled rewards.
/// </summary>
public static class QuestGenerator
{
    private static readonly Random _random = Random.Shared;
    
    /// <summary>
    /// Generate a random quest from any available template using v4.0 catalog system.
    /// Uses weighted selection based on rarityWeight.
    /// </summary>
    public static Quest Generate()
    {
        var faker = new Faker();
        
        // Pick random quest type and difficulty
        var questType = faker.PickRandom("fetch", "kill", "escort", "delivery", "investigate");
        var difficulty = faker.PickRandom("easy", "medium", "hard");
        
        return GenerateByTypeAndDifficulty(questType, difficulty);
    }
    
    /// <summary>
    /// Generate a quest of a specific type using v4.0 catalog system.
    /// </summary>
    public static Quest GenerateByType(string questType)
    {
        var faker = new Faker();
        
        // Pick random difficulty
        var difficulty = faker.PickRandom("easy", "medium", "hard");
        
        return GenerateByTypeAndDifficulty(questType, difficulty);
    }
    
    /// <summary>
    /// Generate a quest with specific type and difficulty using v4.0 catalog system.
    /// Uses weighted selection for templates, locations, objectives, and rewards.
    /// </summary>
    public static Quest GenerateByTypeAndDifficulty(string questType, string difficulty)
    {
        var catalog = GameDataService.Instance.QuestCatalog;
        var faker = new Faker();
        
        // Normalize inputs
        questType = questType.ToLower();
        difficulty = difficulty.ToLower();
        
        // Select quest template using weighted selection
        var template = SelectQuestTemplate(catalog, questType, difficulty);
        if (template == null)
        {
            Log.Warning("No templates found for {QuestType} - {Difficulty}, using default quest", questType, difficulty);
            return GenerateDefaultQuest();
        }
        
        // Select appropriate location
        var location = SelectQuestLocation(catalog, difficulty);
        
        // Create quest from template
        var quest = new Quest
        {
            Title = template.DisplayName,
            Description = template.Description,
            QuestType = questType,
            Difficulty = difficulty,
            Type = DetermineQuestCategory(template),
            GoldReward = template.BaseGoldReward,
            XpReward = template.BaseXpReward,
            Location = location?.DisplayName ?? "Unknown Location"
        };
        
        // Apply template-specific properties
        ApplyTemplateProperties(quest, template, faker);
        
        // Generate quest-specific details
        GenerateQuestDetails(quest, template, location, faker);
        
        // Assign quest giver
        AssignQuestGiver(quest, faker);
        
        // Populate description variables
        PopulateDescriptionVariables(quest);
        
        Log.Debug("Generated quest: {Title} ({Type}/{Difficulty}) - Reward: {Gold}g, {Xp}xp at {Location}", 
            quest.Title, quest.QuestType, quest.Difficulty, quest.GoldReward, quest.XpReward, quest.Location);
        
        return quest;
    }
    
    /// <summary>
    /// Select a quest template using weighted selection based on type and difficulty.
    /// </summary>
    private static QuestTemplate? SelectQuestTemplate(QuestCatalogData catalog, string questType, string difficulty)
    {
        try
        {
            var templates = (questType, difficulty) switch
            {
                ("fetch", "easy") => catalog.Components.Templates.Fetch.EasyFetch,
                ("fetch", "medium") => catalog.Components.Templates.Fetch.MediumFetch,
                ("fetch", "hard") => catalog.Components.Templates.Fetch.HardFetch,
                
                ("kill", "easy") => catalog.Components.Templates.Kill.EasyCombat,
                ("kill", "medium") => catalog.Components.Templates.Kill.MediumCombat,
                ("kill", "hard") => catalog.Components.Templates.Kill.HardCombat,
                
                ("escort", "easy") => catalog.Components.Templates.Escort.EasyEscort,
                ("escort", "medium") => catalog.Components.Templates.Escort.MediumEscort,
                ("escort", "hard") => catalog.Components.Templates.Escort.HardEscort,
                
                ("delivery", "easy") => catalog.Components.Templates.Delivery.EasyDelivery,
                ("delivery", "medium") => catalog.Components.Templates.Delivery.MediumDelivery,
                ("delivery", "hard") => catalog.Components.Templates.Delivery.HardDelivery,
                
                ("investigate", "easy") => catalog.Components.Templates.Investigate.EasyInvestigation,
                ("investigate", "medium") => catalog.Components.Templates.Investigate.MediumInvestigation,
                ("investigate", "hard") => catalog.Components.Templates.Investigate.HardInvestigation,
                
                _ => null
            };
            
            if (templates == null || templates.Count == 0)
            {
                return null;
            }
            
            return WeightedSelector.SelectByRarityWeight(templates);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to select quest template for {QuestType}/{Difficulty}", questType, difficulty);
            return null;
        }
    }
    
    /// <summary>
    /// Select an appropriate quest location based on difficulty.
    /// </summary>
    private static QuestLocation? SelectQuestLocation(QuestCatalogData catalog, string difficulty)
    {
        try
        {
            // Select location category based on difficulty
            var faker = new Faker();
            var locationType = faker.PickRandom("wilderness", "settlement", "dungeon");
            
            var locations = (locationType, difficulty) switch
            {
                ("wilderness", "easy") => catalog.Components.Locations.Wilderness.LowDanger,
                ("wilderness", "medium") => catalog.Components.Locations.Wilderness.MediumDanger,
                ("wilderness", "hard") => catalog.Components.Locations.Wilderness.HighDanger.Concat(
                    catalog.Components.Locations.Wilderness.VeryHighDanger).ToList(),
                
                ("settlement", "easy") => catalog.Components.Locations.Towns.Outposts.Concat(
                    catalog.Components.Locations.Towns.Villages).ToList(),
                ("settlement", "medium") => catalog.Components.Locations.Towns.Towns.Concat(
                    catalog.Components.Locations.Towns.Cities).ToList(),
                ("settlement", "hard") => catalog.Components.Locations.Towns.Capitals.Concat(
                    catalog.Components.Locations.Towns.SpecialLocations).ToList(),
                
                ("dungeon", "easy") => catalog.Components.Locations.Dungeons.EasyDungeons,
                ("dungeon", "medium") => catalog.Components.Locations.Dungeons.MediumDungeons.Concat(
                    catalog.Components.Locations.Dungeons.HardDungeons).ToList(),
                ("dungeon", "hard") => catalog.Components.Locations.Dungeons.VeryHardDungeons.Concat(
                    catalog.Components.Locations.Dungeons.EpicDungeons).Concat(
                    catalog.Components.Locations.Dungeons.LegendaryDungeons).ToList(),
                
                _ => null
            };
            
            if (locations == null || locations.Count == 0)
            {
                return null;
            }
            
            return WeightedSelector.SelectByRarityWeight(locations);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to select quest location for difficulty {Difficulty}", difficulty);
            return null;
        }
    }
    
    /// <summary>
    /// Apply template-specific properties to the quest.
    /// </summary>
    private static void ApplyTemplateProperties(Quest quest, QuestTemplate template, Faker faker)
    {
        // Apply quantity ranges
        if (template.MinQuantity.HasValue && template.MaxQuantity.HasValue)
        {
            quest.Quantity = faker.Random.Int(template.MinQuantity.Value, template.MaxQuantity.Value);
        }
        
        // Apply time limit
        if (template.TimeLimit.HasValue)
        {
            quest.TimeLimit = template.TimeLimit.Value;
        }
        
        // Store template name for reference
        quest.Traits["templateName"] = new TraitValue(template.Name, TraitType.String);
    }
    
    /// <summary>
    /// Determine quest category (main, side, legendary) from template.
    /// </summary>
    private static string DetermineQuestCategory(QuestTemplate template)
    {
        // Check for legendary indicators
        if (template.Legendary.HasValue && template.Legendary.Value)
        {
            return "legendary";
        }
        
        // Check reward thresholds
        if (template.BaseGoldReward > 500 || template.BaseXpReward > 1000)
        {
            return "legendary";
        }
        
        // Default to side quest
        return "side";
    }
    
    /// <summary>
    /// Generate quest-specific details based on template and type.
    /// </summary>
    private static void GenerateQuestDetails(Quest quest, QuestTemplate template, QuestLocation? location, Faker faker)
    {
        switch (quest.QuestType.ToLower())
        {
            case "kill":
                GenerateKillQuestDetailsV4(quest, template, faker);
                break;
            case "fetch":
                GenerateFetchQuestDetailsV4(quest, template, faker);
                break;
            case "escort":
                GenerateEscortQuestDetailsV4(quest, template, location, faker);
                break;
            case "investigate":
                GenerateInvestigateQuestDetailsV4(quest, template, faker);
                break;
            case "delivery":
                GenerateDeliveryQuestDetailsV4(quest, template, location, faker);
                break;
        }
        
        // Initialize objectives dictionary
        InitializeObjectives(quest);
    }
    
    /// <summary>
    /// Generate kill quest details using v4.0 template data.
    /// </summary>
    private static void GenerateKillQuestDetailsV4(Quest quest, QuestTemplate template, Faker faker)
    {
        // Get target type from template
        quest.TargetType = template.TargetType ?? "beast";
        
        // Generate specific enemy name
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
    
    /// <summary>
    /// Generate fetch quest details using v4.0 template data.
    /// </summary>
    private static void GenerateFetchQuestDetailsV4(Quest quest, QuestTemplate template, Faker faker)
    {
        // Get item type from template
        var itemType = template.ItemType ?? "questitem";
        var itemRarity = template.ItemRarity ?? "common";
        
        // Generate item name
        quest.TargetName = GenerateItemName(itemType, faker);
        quest.Traits["itemRarity"] = new TraitValue(itemRarity, TraitType.String);
    }
    
    /// <summary>
    /// Generate escort quest details using v4.0 template data.
    /// </summary>
    private static void GenerateEscortQuestDetailsV4(Quest quest, QuestTemplate template, QuestLocation? location, Faker faker)
    {
        // Generate NPC to escort
        var npcType = template.NpcType ?? "merchant";
        var npc = NpcGenerator.Generate();
        quest.TargetName = npc.Name;
        quest.Traits["npcType"] = new TraitValue(npcType, TraitType.String);
        
        // Use provided location or generate one
        if (location != null)
        {
            quest.Location = location.DisplayName;
        }
    }
    
    /// <summary>
    /// Generate investigate quest details using v4.0 template data.
    /// </summary>
    private static void GenerateInvestigateQuestDetailsV4(Quest quest, QuestTemplate template, Faker faker)
    {
        // Quantity represents number of clues
        quest.TargetName = "clues";
        
        // Store investigation type
        if (!string.IsNullOrEmpty(template.Location))
        {
            quest.Traits["investigationType"] = new TraitValue(template.Location, TraitType.String);
        }
    }
    
    /// <summary>
    /// Generate delivery quest details using v4.0 template data.
    /// </summary>
    private static void GenerateDeliveryQuestDetailsV4(Quest quest, QuestTemplate template, QuestLocation? location, Faker faker)
    {
        // Generate package name
        quest.TargetName = faker.PickRandom("Package", "Letter", "Crate", "Parcel", "Documents", "Sealed Box");
        
        // Mark if urgent or fragile
        if (template.Urgent.HasValue && template.Urgent.Value)
        {
            quest.Traits["urgent"] = new TraitValue(true, TraitType.Boolean);
            quest.TimeLimit = faker.Random.Int(2, 6); // 2-6 hours for urgent deliveries
        }
        
        if (template.ItemFragile.HasValue && template.ItemFragile.Value)
        {
            quest.Traits["fragile"] = new TraitValue(true, TraitType.Boolean);
        }
        
        // Use provided location
        if (location != null)
        {
            quest.Location = location.DisplayName;
        }
    }
    
    /// <summary>
    /// Populate variables in quest description (e.g., {quantity}, {target}, {location}).
    /// </summary>
    private static void PopulateDescriptionVariables(Quest quest)
    {
        if (string.IsNullOrEmpty(quest.Description))
        {
            return;
        }
        
        quest.Description = quest.Description
            .Replace("{quantity}", quest.Quantity.ToString())
            .Replace("{target}", quest.TargetName)
            .Replace("{location}", quest.Location)
            .Replace("{itemName}", quest.TargetName)
            .Replace("{npcName}", quest.TargetName);
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

