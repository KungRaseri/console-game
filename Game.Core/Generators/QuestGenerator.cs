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
        
        // Calculate and apply rewards (Phase 4)
        CalculateRewards(quest, playerLevel: 1);
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
    /// Select a primary objective matching quest type and difficulty using v4.0 catalog.
    /// </summary>
    private static QuestObjective? SelectPrimaryObjective(string questType, string difficulty)
    {
        var catalog = GameDataService.Instance.QuestObjectives;
        if (catalog?.Components?.Primary == null) return null;

        // Get all primary objectives
        var allPrimary = new List<QuestObjective>();
        
        if (catalog.Components.Primary.Combat != null)
            allPrimary.AddRange(catalog.Components.Primary.Combat);
        if (catalog.Components.Primary.Retrieval != null)
            allPrimary.AddRange(catalog.Components.Primary.Retrieval);
        if (catalog.Components.Primary.Rescue != null)
            allPrimary.AddRange(catalog.Components.Primary.Rescue);
        if (catalog.Components.Primary.Purification != null)
            allPrimary.AddRange(catalog.Components.Primary.Purification);
        if (catalog.Components.Primary.Defense != null)
            allPrimary.AddRange(catalog.Components.Primary.Defense);
        if (catalog.Components.Primary.Social != null)
            allPrimary.AddRange(catalog.Components.Primary.Social);
        if (catalog.Components.Primary.Timed != null)
            allPrimary.AddRange(catalog.Components.Primary.Timed);

        // Filter by difficulty
        var matchingObjectives = allPrimary
            .Where(o => o.Difficulty?.Equals(difficulty, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        if (!matchingObjectives.Any())
            matchingObjectives = allPrimary; // Fallback to any if no difficulty match

        return WeightedSelector.SelectByRarityWeight(matchingObjectives);
    }

    /// <summary>
    /// Select a secondary objective matching difficulty using v4.0 catalog.
    /// </summary>
    private static QuestObjective? SelectSecondaryObjective(string difficulty)
    {
        var catalog = GameDataService.Instance.QuestObjectives;
        if (catalog?.Components?.Secondary == null) return null;

        var allSecondary = new List<QuestObjective>();
        
        if (catalog.Components.Secondary.Stealth != null)
            allSecondary.AddRange(catalog.Components.Secondary.Stealth);
        if (catalog.Components.Secondary.Survival != null)
            allSecondary.AddRange(catalog.Components.Secondary.Survival);
        if (catalog.Components.Secondary.Speed != null)
            allSecondary.AddRange(catalog.Components.Secondary.Speed);
        if (catalog.Components.Secondary.Collection != null)
            allSecondary.AddRange(catalog.Components.Secondary.Collection);
        if (catalog.Components.Secondary.Mercy != null)
            allSecondary.AddRange(catalog.Components.Secondary.Mercy);
        if (catalog.Components.Secondary.Combat != null)
            allSecondary.AddRange(catalog.Components.Secondary.Combat);
        if (catalog.Components.Secondary.Precision != null)
            allSecondary.AddRange(catalog.Components.Secondary.Precision);
        if (catalog.Components.Secondary.Social != null)
            allSecondary.AddRange(catalog.Components.Secondary.Social);

        // Prefer matching difficulty, but allow any if none match
        var matchingObjectives = allSecondary
            .Where(o => o.Difficulty?.Equals(difficulty, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        if (!matchingObjectives.Any())
            matchingObjectives = allSecondary;

        return WeightedSelector.SelectByRarityWeight(matchingObjectives);
    }

    /// <summary>
    /// Select a hidden objective (discovery-based bonus) using v4.0 catalog.
    /// </summary>
    private static QuestObjective? SelectHiddenObjective()
    {
        var catalog = GameDataService.Instance.QuestObjectives;
        if (catalog?.Components?.Hidden == null) return null;

        var allHidden = new List<QuestObjective>();
        
        if (catalog.Components.Hidden.Exploration != null)
            allHidden.AddRange(catalog.Components.Hidden.Exploration);
        if (catalog.Components.Hidden.Lore != null)
            allHidden.AddRange(catalog.Components.Hidden.Lore);
        if (catalog.Components.Hidden.Combat != null)
            allHidden.AddRange(catalog.Components.Hidden.Combat);
        if (catalog.Components.Hidden.Branching != null)
            allHidden.AddRange(catalog.Components.Hidden.Branching);
        if (catalog.Components.Hidden.Collection != null)
            allHidden.AddRange(catalog.Components.Hidden.Collection);
        if (catalog.Components.Hidden.Puzzle != null)
            allHidden.AddRange(catalog.Components.Hidden.Puzzle);
        if (catalog.Components.Hidden.Rescue != null)
            allHidden.AddRange(catalog.Components.Hidden.Rescue);
        if (catalog.Components.Hidden.Diplomacy != null)
            allHidden.AddRange(catalog.Components.Hidden.Diplomacy);
        if (catalog.Components.Hidden.Timed != null)
            allHidden.AddRange(catalog.Components.Hidden.Timed);
        if (catalog.Components.Hidden.Purification != null)
            allHidden.AddRange(catalog.Components.Hidden.Purification);

        return WeightedSelector.SelectByRarityWeight(allHidden);
    }
    
    /// <summary>
    /// Calculate and apply rewards (gold, XP, items) to quest using v4.0 rewards catalog (Phase 4).
    /// </summary>
    private static void CalculateRewards(Quest quest, int playerLevel = 1)
    {
        var catalog = GameDataService.Instance.QuestRewards;
        if (catalog?.Components == null)
        {
            // Keep template's base rewards if catalog not loaded
            return;
        }

        // Calculate gold with player level scaling: base * (1 + level * 0.05)
        var goldMultiplier = 1.0 + (playerLevel * 0.05);
        quest.GoldReward = (int)(quest.GoldReward * goldMultiplier);

        // Calculate XP with difficulty and level scaling
        var difficultyMultiplier = quest.Difficulty?.ToLower() switch
        {
            "easy" => 1.0,
            "medium" => 1.5,
            "hard" => 2.0,
            _ => 1.0
        };
        var xpMultiplier = difficultyMultiplier * (1.0 + (playerLevel * 0.1));
        quest.XpReward = (int)(quest.XpReward * xpMultiplier);

        // Apply bonus multipliers for objectives
        var bonusMultiplier = 1.0;
        
        // Secondary objective: +25-50% bonus
        if (quest.Traits.ContainsKey("secondaryObjective"))
        {
            bonusMultiplier += 0.25 + (_random.NextDouble() * 0.25); // 25-50%
        }
        
        // Hidden objective: +50-100% bonus
        if (quest.Traits.ContainsKey("hiddenObjective"))
        {
            bonusMultiplier += 0.50 + (_random.NextDouble() * 0.50); // 50-100%
        }

        // Apply bonus multipliers
        if (bonusMultiplier > 1.0)
        {
            quest.GoldReward = (int)(quest.GoldReward * bonusMultiplier);
            quest.XpReward = (int)(quest.XpReward * bonusMultiplier);
        }

        // Select item rewards based on difficulty and objectives
        SelectItemRewards(quest);
    }

    /// <summary>
    /// Select item rewards based on quest difficulty and completion objectives.
    /// </summary>
    private static void SelectItemRewards(Quest quest)
    {
        var catalog = GameDataService.Instance.QuestRewards;
        if (catalog?.Components?.Items == null) return;

        // Determine reward tier based on difficulty
        var rewardTier = quest.Difficulty?.ToLower() switch
        {
            "easy" => 1,    // Common/Uncommon
            "medium" => 2,  // Uncommon/Rare
            "hard" => 3,    // Rare/Epic
            _ => 1
        };

        // Upgrade tier if hidden objective present (+1 tier)
        if (quest.Traits.ContainsKey("hiddenObjective"))
        {
            rewardTier++;
        }

        // Legendary quests get better rewards
        if (quest.Type == "legendary")
        {
            rewardTier++;
        }

        // Select items based on tier (capped at mythic = tier 6)
        rewardTier = Math.Min(rewardTier, 6);

        var selectedItems = new List<string>();
        
        // Get appropriate item pool
        var itemPool = new List<ItemReward>();
        
        switch (rewardTier)
        {
            case 1: // Common
                if (catalog.Components.Items.ConsumableRewards != null)
                    itemPool.AddRange(catalog.Components.Items.ConsumableRewards);
                if (catalog.Components.Items.CommonEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.CommonEquipment);
                break;
                
            case 2: // Uncommon
                if (catalog.Components.Items.UncommonEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.UncommonEquipment);
                if (catalog.Components.Items.ConsumableRewards != null)
                    itemPool.AddRange(catalog.Components.Items.ConsumableRewards);
                break;
                
            case 3: // Rare
                if (catalog.Components.Items.RareEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.RareEquipment);
                if (catalog.Components.Items.UncommonEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.UncommonEquipment);
                break;
                
            case 4: // Epic
                if (catalog.Components.Items.EpicEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.EpicEquipment);
                if (catalog.Components.Items.RareEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.RareEquipment);
                break;
                
            case 5: // Legendary
                if (catalog.Components.Items.LegendaryEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.LegendaryEquipment);
                if (catalog.Components.Items.EpicEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.EpicEquipment);
                break;
                
            case 6: // Mythic
                if (catalog.Components.Items.MythicEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.MythicEquipment);
                if (catalog.Components.Items.LegendaryEquipment != null)
                    itemPool.AddRange(catalog.Components.Items.LegendaryEquipment);
                break;
        }

        // Select 1-2 items using weighted selection
        var itemCount = _random.Next(1, 3); // 1 or 2 items
        
        for (int i = 0; i < itemCount && itemPool.Any(); i++)
        {
            var selectedReward = WeightedSelector.SelectByRarityWeight(itemPool);
            if (selectedReward != null)
            {
                selectedItems.Add(selectedReward.DisplayName);
                // Remove from pool to avoid duplicates
                itemPool.Remove(selectedReward);
            }
        }

        // Store item rewards
        quest.ItemRewards = selectedItems;
        
        // Store reward tier in traits for reference
        quest.Traits["rewardTier"] = new TraitValue(rewardTier, TraitType.Number);
    }
    
    /// <summary>
    /// Initialize objectives dictionary for quest tracking using v4.0 objectives catalog (Phase 3).
    /// </summary>
    private static void InitializeObjectives(Quest quest)
    {
        var catalog = GameDataService.Instance.QuestObjectives;
        
        if (catalog?.Components != null)
        {
            // V4.0: Use weighted objective selection from catalog
            
            // Select primary objective (required - always added)
            var primaryObjective = SelectPrimaryObjective(quest.QuestType, quest.Difficulty);
            if (primaryObjective != null)
            {
                quest.Objectives[primaryObjective.DisplayName] = quest.Quantity;
                quest.ObjectiveProgress[primaryObjective.DisplayName] = 0;
                
                // Store objective metadata in traits for later reference
                quest.Traits["primaryObjective"] = new TraitValue(primaryObjective.Name, TraitType.String);
            }

            // 25% chance to add secondary objective (optional bonus)
            if (_random.Next(100) < 25)
            {
                var secondaryObjective = SelectSecondaryObjective(quest.Difficulty);
                if (secondaryObjective != null)
                {
                    quest.Objectives[$"[BONUS] {secondaryObjective.DisplayName}"] = 1;
                    quest.ObjectiveProgress[$"[BONUS] {secondaryObjective.DisplayName}"] = 0;
                    quest.Traits["secondaryObjective"] = new TraitValue(secondaryObjective.Name, TraitType.String);
                }
            }

            // 10% chance to add hidden objective (discovery-based)
            if (_random.Next(100) < 10)
            {
                var hiddenObjective = SelectHiddenObjective();
                if (hiddenObjective != null)
                {
                    // Hidden objectives are not visible initially
                    quest.Objectives[$"[HIDDEN] {hiddenObjective.DisplayName}"] = 1;
                    quest.ObjectiveProgress[$"[HIDDEN] {hiddenObjective.DisplayName}"] = 0;
                    quest.Traits["hiddenObjective"] = new TraitValue(hiddenObjective.Name, TraitType.String);
                }
            }
        }
        else
        {
            // Fallback: Convert legacy quest data to objectives format
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
}

