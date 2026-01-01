using System.Text.Json.Serialization;

namespace Game.Shared.Data;

/// <summary>
/// Root data model for quest rewards (items, gold, experience).
/// Loaded from quests/rewards.json.
/// </summary>
public class QuestRewardsData
{
    [JsonPropertyName("metadata")]
    public QuestRewardsMetadata Metadata { get; set; } = new();

    [JsonPropertyName("components")]
    public QuestRewardsComponents Components { get; set; } = new();
}

public class QuestRewardsMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("total_item_rewards")]
    public int TotalItemRewards { get; set; }

    [JsonPropertyName("total_gold_tiers")]
    public int TotalGoldTiers { get; set; }

    [JsonPropertyName("total_xp_tiers")]
    public int TotalXpTiers { get; set; }

    [JsonPropertyName("scaling_formulas")]
    public ScalingFormulas ScalingFormulas { get; set; } = new();

    [JsonPropertyName("bonus_multipliers")]
    public BonusMultipliers BonusMultipliers { get; set; } = new();

    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public QuestRewardsCategories Categories { get; set; } = new();
}

public class ScalingFormulas
{
    [JsonPropertyName("gold")]
    public string Gold { get; set; } = string.Empty;

    [JsonPropertyName("experience")]
    public string Experience { get; set; } = string.Empty;

    [JsonPropertyName("item_level")]
    public string ItemLevel { get; set; } = string.Empty;
}

public class BonusMultipliers
{
    [JsonPropertyName("secondary_objective")]
    public string SecondaryObjective { get; set; } = string.Empty;

    [JsonPropertyName("hidden_objective")]
    public string HiddenObjective { get; set; } = string.Empty;

    [JsonPropertyName("first_time_completion")]
    public string FirstTimeCompletion { get; set; } = string.Empty;

    [JsonPropertyName("speed_bonus")]
    public string SpeedBonus { get; set; } = string.Empty;

    [JsonPropertyName("perfect_completion")]
    public string PerfectCompletion { get; set; } = string.Empty;
}

public class QuestRewardsCategories
{
    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();

    [JsonPropertyName("gold")]
    public List<string> Gold { get; set; } = new();

    [JsonPropertyName("experience")]
    public List<string> Experience { get; set; } = new();
}

public class QuestRewardsComponents
{
    [JsonPropertyName("items")]
    public ItemRewardsCollection Items { get; set; } = new();

    [JsonPropertyName("gold")]
    public GoldRewardsCollection Gold { get; set; } = new();

    [JsonPropertyName("experience")]
    public ExperienceRewardsCollection Experience { get; set; } = new();
}

// Item Rewards
public class ItemRewardsCollection
{
    [JsonPropertyName("consumable_rewards")]
    public List<ItemReward> ConsumableRewards { get; set; } = new();

    [JsonPropertyName("common_equipment")]
    public List<ItemReward> CommonEquipment { get; set; } = new();

    [JsonPropertyName("uncommon_equipment")]
    public List<ItemReward> UncommonEquipment { get; set; } = new();

    [JsonPropertyName("rare_equipment")]
    public List<ItemReward> RareEquipment { get; set; } = new();

    [JsonPropertyName("epic_equipment")]
    public List<ItemReward> EpicEquipment { get; set; } = new();

    [JsonPropertyName("legendary_equipment")]
    public List<ItemReward> LegendaryEquipment { get; set; } = new();

    [JsonPropertyName("mythic_equipment")]
    public List<ItemReward> MythicEquipment { get; set; } = new();

    [JsonPropertyName("choice_rewards")]
    public List<ItemReward> ChoiceRewards { get; set; } = new();
}

public class ItemReward
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "item";

    [JsonPropertyName("minQuantity")]
    public int? MinQuantity { get; set; }

    [JsonPropertyName("maxQuantity")]
    public int? MaxQuantity { get; set; }

    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    [JsonPropertyName("slot")]
    public string? Slot { get; set; }

    [JsonPropertyName("statBonus")]
    public string? StatBonus { get; set; }

    [JsonPropertyName("decorationType")]
    public string? DecorationType { get; set; }

    [JsonPropertyName("effectType")]
    public string? EffectType { get; set; }

    [JsonPropertyName("skillType")]
    public string? SkillType { get; set; }

    [JsonPropertyName("titleRarity")]
    public string? TitleRarity { get; set; }

    [JsonPropertyName("mountType")]
    public string? MountType { get; set; }

    [JsonPropertyName("petType")]
    public string? PetType { get; set; }

    [JsonPropertyName("questSpecific")]
    public bool? QuestSpecific { get; set; }

    [JsonPropertyName("relicType")]
    public string? RelicType { get; set; }

    [JsonPropertyName("passiveBonus")]
    public string? PassiveBonus { get; set; }

    [JsonPropertyName("treasureType")]
    public string? TreasureType { get; set; }

    [JsonPropertyName("choices")]
    public int? Choices { get; set; }

    [JsonPropertyName("choiceTypes")]
    public List<string>? ChoiceTypes { get; set; }
}

// Gold Rewards
public class GoldRewardsCollection
{
    [JsonPropertyName("trivial")]
    public List<GoldReward> Trivial { get; set; } = new();

    [JsonPropertyName("low")]
    public List<GoldReward> Low { get; set; } = new();

    [JsonPropertyName("medium")]
    public List<GoldReward> Medium { get; set; } = new();

    [JsonPropertyName("high")]
    public List<GoldReward> High { get; set; } = new();

    [JsonPropertyName("very_high")]
    public List<GoldReward> VeryHigh { get; set; } = new();

    [JsonPropertyName("epic")]
    public List<GoldReward> Epic { get; set; } = new();

    [JsonPropertyName("legendary")]
    public List<GoldReward> Legendary { get; set; } = new();

    [JsonPropertyName("mythic")]
    public List<GoldReward> Mythic { get; set; } = new();

    [JsonPropertyName("ancient")]
    public List<GoldReward> Ancient { get; set; } = new();
}

public class GoldReward
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("minAmount")]
    public int MinAmount { get; set; }

    [JsonPropertyName("maxAmount")]
    public int MaxAmount { get; set; }

    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "gold";
}

// Experience Rewards
public class ExperienceRewardsCollection
{
    [JsonPropertyName("trivial")]
    public List<ExperienceReward> Trivial { get; set; } = new();

    [JsonPropertyName("low")]
    public List<ExperienceReward> Low { get; set; } = new();

    [JsonPropertyName("medium")]
    public List<ExperienceReward> Medium { get; set; } = new();

    [JsonPropertyName("high")]
    public List<ExperienceReward> High { get; set; } = new();

    [JsonPropertyName("very_high")]
    public List<ExperienceReward> VeryHigh { get; set; } = new();

    [JsonPropertyName("epic")]
    public List<ExperienceReward> Epic { get; set; } = new();

    [JsonPropertyName("legendary")]
    public List<ExperienceReward> Legendary { get; set; } = new();

    [JsonPropertyName("mythic")]
    public List<ExperienceReward> Mythic { get; set; } = new();

    [JsonPropertyName("ancient")]
    public List<ExperienceReward> Ancient { get; set; } = new();
}

public class ExperienceReward
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("minAmount")]
    public int? MinAmount { get; set; }

    [JsonPropertyName("maxAmount")]
    public int? MaxAmount { get; set; }

    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "experience";

    [JsonPropertyName("dynamicAmount")]
    public bool? DynamicAmount { get; set; }
}
