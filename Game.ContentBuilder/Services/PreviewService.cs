using Game.Shared.Models;
using Serilog;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for generating preview content using the game's generators.
/// This allows users to see how their JSON edits affect generated content.
/// </summary>
public class PreviewService
{
    /// <summary>
    /// Generate sample items using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateItemPreviews(int count = 10)
    {
        try
        {
            var items = ItemGenerator.Generate(count);
            return items.Select(item => new PreviewItem
            {
                Category = "Item",
                Name = item.Name,
                Details = $"Type: {item.Type}, Rarity: {item.Rarity}, Price: {item.Price}g",
                FullDescription = item.Description ?? "No description"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate item previews");
            return new List<PreviewItem>
            {
                new PreviewItem
                {
                    Category = "Error",
                    Name = "Generation Failed",
                    Details = ex.Message,
                    FullDescription = ex.ToString()
                }
            };
        }
    }

    /// <summary>
    /// Generate sample weapons using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateWeaponPreviews(int count = 10)
    {
        try
        {
            var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, count);
            return weapons.Select(weapon => new PreviewItem
            {
                Category = "Weapon",
                Name = weapon.Name,
                Details = $"Rarity: {weapon.Rarity}, Price: {weapon.Price}g, Two-Handed: {weapon.IsTwoHanded}",
                FullDescription = $"{weapon.Description}\n\nStr: +{weapon.BonusStrength}, Dex: +{weapon.BonusDexterity}, Con: +{weapon.BonusConstitution}"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate weapon previews");
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample consumables using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateConsumablePreviews(int count = 10)
    {
        try
        {
            var consumables = ItemGenerator.GenerateByType(ItemType.Consumable, count);
            return consumables.Select(item => new PreviewItem
            {
                Category = "Consumable",
                Name = item.Name,
                Details = $"Rarity: {item.Rarity}, Price: {item.Price}g",
                FullDescription = item.Description ?? "No description"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate consumable previews");
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample enemies using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateEnemyPreviews(int playerLevel = 5, int count = 10)
    {
        try
        {
            var enemies = new List<Enemy>();
            for (int i = 0; i < count; i++)
            {
                enemies.Add(EnemyGenerator.Generate(playerLevel));
            }

            return enemies.Select(enemy => new PreviewItem
            {
                Category = "Enemy",
                Name = enemy.Name,
                Details = $"Type: {enemy.Type}, Level: {enemy.Level}, HP: {enemy.Health}/{enemy.MaxHealth}",
                FullDescription = $"Str: {enemy.Strength}, Dex: {enemy.Dexterity}, Con: {enemy.Constitution}, XP: {enemy.XPReward}, Gold: {enemy.GoldReward}"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate enemy previews");
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample enemies by type using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateEnemyPreviewsByType(EnemyType type, int playerLevel = 5, int count = 10)
    {
        try
        {
            var enemies = new List<Enemy>();
            for (int i = 0; i < count; i++)
            {
                enemies.Add(EnemyGenerator.GenerateByType(type, playerLevel));
            }

            return enemies.Select(enemy => new PreviewItem
            {
                Category = $"Enemy ({type})",
                Name = enemy.Name,
                Details = $"Level: {enemy.Level}, HP: {enemy.Health}/{enemy.MaxHealth}",
                FullDescription = $"Str: {enemy.Strength}, Dex: {enemy.Dexterity}, Con: {enemy.Constitution}, XP: {enemy.XPReward}, Gold: {enemy.GoldReward}"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate {Type} enemy previews", type);
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample NPCs using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateNpcPreviews(int count = 10)
    {
        try
        {
            var npcs = NpcGenerator.Generate(count);
            return npcs.Select(npc => new PreviewItem
            {
                Category = "NPC",
                Name = npc.Name,
                Details = $"Occupation: {npc.Occupation}, Age: {npc.Age}, Gold: {npc.Gold}",
                FullDescription = $"Friendly: {npc.IsFriendly}\nDialogue: {npc.Dialogue ?? "No dialogue"}"
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate NPC previews");
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample quests using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateQuestPreviews(int count = 10)
    {
        try
        {
            var quests = new List<Quest>();
            for (int i = 0; i < count; i++)
            {
                quests.Add(QuestGenerator.Generate());
            }

            return quests.Select(quest => new PreviewItem
            {
                Category = "Quest",
                Name = quest.Title,
                Details = $"Type: {quest.Type}, Difficulty: {quest.Difficulty}, Reward: {quest.GoldReward}g / {quest.XpReward}xp",
                FullDescription = quest.Description
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate quest previews");
            return CreateErrorPreview(ex);
        }
    }

    /// <summary>
    /// Generate sample quests by type using current JSON data
    /// </summary>
    public List<PreviewItem> GenerateQuestPreviewsByType(string questType, int count = 10)
    {
        try
        {
            var quests = new List<Quest>();
            for (int i = 0; i < count; i++)
            {
                quests.Add(QuestGenerator.GenerateByType(questType));
            }

            return quests.Select(quest => new PreviewItem
            {
                Category = $"Quest ({questType})",
                Name = quest.Title,
                Details = $"Difficulty: {quest.Difficulty}, Reward: {quest.GoldReward}g / {quest.XpReward}xp",
                FullDescription = quest.Description
            }).ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to generate {Type} quest previews", questType);
            return CreateErrorPreview(ex);
        }
    }

    private static List<PreviewItem> CreateErrorPreview(Exception ex)
    {
        return new List<PreviewItem>
        {
            new PreviewItem
            {
                Category = "Error",
                Name = "Generation Failed",
                Details = ex.Message,
                FullDescription = ex.ToString()
            }
        };
    }
}

/// <summary>
/// Model for displaying preview items in the UI
/// </summary>
public class PreviewItem
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
}
