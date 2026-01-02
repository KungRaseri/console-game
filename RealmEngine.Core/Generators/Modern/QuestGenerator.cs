using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generates Quest instances from quests catalog JSON files.
/// Supports various quest types (kill, collect, escort, explore) with objectives, rewards, and prerequisites.
/// </summary>
public class QuestGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the QuestGenerator class.
    /// </summary>
    /// <param name="dataCache">The game data cache for accessing quest catalog files.</param>
    /// <param name="referenceResolver">The reference resolver for resolving JSON references.</param>
    public QuestGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates a list of random quests of a specific type.
    /// </summary>
    /// <param name="questType">The quest type (e.g., "kill", "collect", "escort", "explore").</param>
    /// <param name="count">The number of quests to generate (default: 3).</param>
    /// <returns>A list of generated Quest instances.</returns>
    public async Task<List<Quest>> GenerateQuestsAsync(string questType, int count = 3)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"quests/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return new List<Quest>();
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog, questType);
            
            if (items == null || !items.Any())
            {
                return new List<Quest>();
            }

            var result = new List<Quest>();

            for (int i = 0; i < count; i++)
            {
                var randomQuest = GetRandomWeightedItem(items);
                if (randomQuest != null)
                {
                    var quest = await ConvertToQuestAsync(randomQuest, questType);
                    if (quest != null)
                    {
                        result.Add(quest);
                    }
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating quests for type {questType}: {ex.Message}");
            return new List<Quest>();
        }
    }

    /// <summary>
    /// Generates a specific quest by name from a quest type category.
    /// </summary>
    /// <param name="questType">The quest type category to search in.</param>
    /// <param name="questName">The name or display name of the quest to generate.</param>
    /// <returns>The generated Quest instance, or null if not found.</returns>
    public async Task<Quest?> GenerateQuestByNameAsync(string questType, string questName)
    {
        try
        {
            var catalogFile = _dataCache.GetFile($"quests/catalog.json");
            if (catalogFile?.JsonData == null)
            {
                return null;
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog, questType);
            
            var catalogQuest = items?.FirstOrDefault(q => 
                string.Equals(GetStringProperty(q, "name"), questName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetStringProperty(q, "displayName"), questName, StringComparison.OrdinalIgnoreCase));

            if (catalogQuest != null)
            {
                return await ConvertToQuestAsync(catalogQuest, questType);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating quest {questName} from type {questType}: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<JToken>? GetItemsFromCatalog(JToken catalog, string questType)
    {
        // Navigate to quest_types -> specific type -> items
        var questTypes = catalog["quest_types"];
        if (questTypes == null) return null;

        var typeData = questTypes[questType];
        if (typeData == null) return null;

        var items = typeData["items"];
        return items?.Children();
    }

    private async Task<Quest?> ConvertToQuestAsync(JToken catalogQuest, string questType)
    {
        try
        {
            var quest = new Quest
            {
                Id = $"{questType}:{GetStringProperty(catalogQuest, "name")}",
                Title = GetStringProperty(catalogQuest, "displayName") ?? GetStringProperty(catalogQuest, "name") ?? "Unknown Quest",
                Description = GetStringProperty(catalogQuest, "description") ?? "A mysterious task awaits",
                QuestType = GetStringProperty(catalogQuest, "questType") ?? questType,
                Difficulty = GetStringProperty(catalogQuest, "difficulty") ?? "medium",
                Type = GetBoolProperty(catalogQuest, "legendary", false) ? "legendary" : "side",
                
                // Set rewards
                GoldReward = GetIntProperty(catalogQuest, "baseGoldReward", 50),
                XpReward = GetIntProperty(catalogQuest, "baseXpReward", 100),
                
                // Set target info for kill/fetch quests
                TargetName = GetStringProperty(catalogQuest, "targetType") ?? "",
                Quantity = GetIntProperty(catalogQuest, "minQuantity", 1),
                Location = GetStringProperty(catalogQuest, "location") ?? "Unknown",
                
                // Initialize as not active/completed
                IsActive = false,
                IsCompleted = false
            };

            // Resolve item reward references
            if (catalogQuest["itemRewards"] is JArray itemRewards)
            {
                quest.ItemRewardIds = await ResolveReferencesAsync(itemRewards);
            }

            // Resolve ability reward references
            if (catalogQuest["abilityRewards"] is JArray abilityRewards)
            {
                quest.AbilityRewardIds = await ResolveReferencesAsync(abilityRewards);
            }

            // Resolve objective location references
            if (catalogQuest["locations"] is JArray locations)
            {
                quest.ObjectiveLocationIds = await ResolveReferencesAsync(locations);
            }

            // Resolve objective NPC references
            if (catalogQuest["npcs"] is JArray npcs)
            {
                quest.ObjectiveNpcIds = await ResolveReferencesAsync(npcs);
            }

            // Resolve objective enemy references
            if (catalogQuest["enemies"] is JArray enemies)
            {
                quest.ObjectiveEnemyIds = await ResolveReferencesAsync(enemies);
            }

            // Handle objectives - create simple objectives from quest data
            var objectives = new Dictionary<string, int>();
            
            if (quest.QuestType == "kill" && !string.IsNullOrEmpty(quest.TargetName))
            {
                objectives[$"Defeat {quest.Quantity} {quest.TargetName}"] = quest.Quantity;
            }
            else if (quest.QuestType == "fetch")
            {
                var itemType = GetStringProperty(catalogQuest, "itemType");
                if (!string.IsNullOrEmpty(itemType))
                {
                    objectives[$"Collect {quest.Quantity} {itemType}"] = quest.Quantity;
                }
            }
            else if (quest.QuestType == "escort")
            {
                var npcType = GetStringProperty(catalogQuest, "npcType");
                objectives[$"Escort {npcType ?? "NPC"} safely"] = 1;
            }
            else if (quest.QuestType == "investigate")
            {
                var minClues = GetIntProperty(catalogQuest, "minClues", 3);
                objectives[$"Find {minClues} clues"] = minClues;
            }
            else if (quest.QuestType == "delivery")
            {
                objectives[$"Deliver package to {quest.Location}"] = 1;
            }
            
            quest.Objectives = objectives;
            quest.ObjectiveProgress = objectives.Keys.ToDictionary(k => k, v => 0);

            return quest;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting catalog quest to Quest: {ex.Message}");
            return null;
        }
    }

    private async Task<List<string>> ResolveReferencesAsync(JArray? referenceArray)
    {
        var resolvedIds = new List<string>();
        if (referenceArray == null) return resolvedIds;

        foreach (var item in referenceArray)
        {
            var reference = item.ToString();
            
            if (reference.StartsWith("@"))
            {
                var resolvedId = await _referenceResolver.ResolveAsync(reference);
                if (resolvedId != null)
                {
                    resolvedIds.Add(resolvedId.ToString() ?? string.Empty);
                }
            }
        }

        return resolvedIds;
    }

    private JToken? GetRandomWeightedItem(IEnumerable<JToken> items)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) return null;

        var totalWeight = itemList.Sum(item => GetIntProperty(item, "rarityWeight", 1));
        var randomValue = _random.Next(1, totalWeight + 1);

        int currentWeight = 0;
        foreach (var item in itemList)
        {
            currentWeight += GetIntProperty(item, "rarityWeight", 1);
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }

        return itemList.First();
    }

    private static int GetIntProperty(JToken obj, string propertyName, int defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            return value != null ? value.Value<int>() : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string? GetStringProperty(JToken obj, string propertyName)
    {
        try
        {
            var value = obj[propertyName];
            return value?.Value<string>();
        }
        catch
        {
            return null;
        }
    }

    private static bool GetBoolProperty(JToken obj, string propertyName, bool defaultValue)
    {
        try
        {
            var value = obj[propertyName];
            return value != null ? value.Value<bool>() : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
