using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data;

/// <summary>
/// Root data model for quest rewards (items, gold, experience).
/// Loaded from quests/rewards.json.
/// </summary>
public class QuestRewardsData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public QuestRewardsMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the components.</summary>
    [JsonPropertyName("components")]
    public QuestRewardsComponents Components { get; set; } = new();
}

/// <summary>
/// Metadata for quest rewards data.
/// </summary>
public class QuestRewardsMetadata
{
    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the last updated date.</summary>
    [JsonPropertyName("lastUpdated")]
    public string LastUpdated { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the notes.</summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets the total item rewards.</summary>
    [JsonPropertyName("total_item_rewards")]
    public int TotalItemRewards { get; set; }

    /// <summary>Gets or sets the total gold tiers.</summary>
    [JsonPropertyName("total_gold_tiers")]
    public int TotalGoldTiers { get; set; }

    /// <summary>Gets or sets the total XP tiers.</summary>
    [JsonPropertyName("total_xp_tiers")]
    public int TotalXpTiers { get; set; }

    /// <summary>Gets or sets the scaling formulas.</summary>
    [JsonPropertyName("scaling_formulas")]
    public ScalingFormulas ScalingFormulas { get; set; } = new();

    /// <summary>Gets or sets the bonus multipliers.</summary>
    [JsonPropertyName("bonus_multipliers")]
    public BonusMultipliers BonusMultipliers { get; set; } = new();

    /// <summary>Gets or sets the usage.</summary>
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    /// <summary>Gets or sets the categories.</summary>
    [JsonPropertyName("categories")]
    public QuestRewardsCategories Categories { get; set; } = new();
}

/// <summary>
/// Formulas for scaling rewards based on level.
/// </summary>
public class ScalingFormulas
{
    /// <summary>Gets or sets the gold scaling formula.</summary>
    [JsonPropertyName("gold")]
    public string Gold { get; set; } = string.Empty;

    /// <summary>Gets or sets the experience scaling formula.</summary>
    [JsonPropertyName("experience")]
    public string Experience { get; set; } = string.Empty;

    /// <summary>Gets or sets the item level scaling formula.</summary>
    [JsonPropertyName("item_level")]
    public string ItemLevel { get; set; } = string.Empty;
}

/// <summary>
/// Multipliers for bonus rewards.
/// </summary>
public class BonusMultipliers
{
    /// <summary>Gets or sets the secondary objective multiplier.</summary>
    [JsonPropertyName("secondary_objective")]
    public string SecondaryObjective { get; set; } = string.Empty;

    /// <summary>Gets or sets the hidden objective multiplier.</summary>
    [JsonPropertyName("hidden_objective")]
    public string HiddenObjective { get; set; } = string.Empty;

    /// <summary>Gets or sets the first time completion multiplier.</summary>
    [JsonPropertyName("first_time_completion")]
    public string FirstTimeCompletion { get; set; } = string.Empty;

    /// <summary>Gets or sets the speed bonus multiplier.</summary>
    [JsonPropertyName("speed_bonus")]
    public string SpeedBonus { get; set; } = string.Empty;

    /// <summary>Gets or sets the perfect completion multiplier.</summary>
    [JsonPropertyName("perfect_completion")]
    public string PerfectCompletion { get; set; } = string.Empty;
}

/// <summary>
/// Categories of quest rewards.
/// </summary>
public class QuestRewardsCategories
{
    /// <summary>Gets or sets the item categories.</summary>
    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = new();

    /// <summary>Gets or sets the gold categories.</summary>
    [JsonPropertyName("gold")]
    public List<string> Gold { get; set; } = new();

    /// <summary>Gets or sets the experience categories.</summary>
    [JsonPropertyName("experience")]
    public List<string> Experience { get; set; } = new();
}

/// <summary>
/// Components containing reward collections.
/// </summary>
public class QuestRewardsComponents
{
    /// <summary>Gets or sets the item rewards.</summary>
    [JsonPropertyName("items")]
    public ItemRewardsCollection Items { get; set; } = new();

    /// <summary>Gets or sets the gold rewards.</summary>
    [JsonPropertyName("gold")]
    public GoldRewardsCollection Gold { get; set; } = new();

    /// <summary>Gets or sets the experience rewards.</summary>
    [JsonPropertyName("experience")]
    public ExperienceRewardsCollection Experience { get; set; } = new();
}

// Item Rewards
/// <summary>
/// Collection of item rewards by rarity.
/// </summary>
public class ItemRewardsCollection
{
    /// <summary>Gets or sets the consumable rewards.</summary>
    [JsonPropertyName("consumable_rewards")]
    public List<ItemReward> ConsumableRewards { get; set; } = new();

    /// <summary>Gets or sets the common equipment.</summary>
    [JsonPropertyName("common_equipment")]
    public List<ItemReward> CommonEquipment { get; set; } = new();

    /// <summary>Gets or sets the uncommon equipment.</summary>
    [JsonPropertyName("uncommon_equipment")]
    public List<ItemReward> UncommonEquipment { get; set; } = new();

    /// <summary>Gets or sets the rare equipment.</summary>
    [JsonPropertyName("rare_equipment")]
    public List<ItemReward> RareEquipment { get; set; } = new();

    /// <summary>Gets or sets the epic equipment.</summary>
    [JsonPropertyName("epic_equipment")]
    public List<ItemReward> EpicEquipment { get; set; } = new();

    /// <summary>Gets or sets the legendary equipment.</summary>
    [JsonPropertyName("legendary_equipment")]
    public List<ItemReward> LegendaryEquipment { get; set; } = new();

    /// <summary>Gets or sets the mythic equipment.</summary>
    [JsonPropertyName("mythic_equipment")]
    public List<ItemReward> MythicEquipment { get; set; } = new();

    /// <summary>Gets or sets the choice rewards.</summary>
    [JsonPropertyName("choice_rewards")]
    public List<ItemReward> ChoiceRewards { get; set; } = new();
}

/// <summary>
/// Item reward definition with configurable properties.
/// </summary>
public class ItemReward
{
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the rarity.</summary>
    [JsonPropertyName("rarity")]
    public string Rarity { get; set; } = string.Empty;

    /// <summary>Gets or sets the category.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the reward type.</summary>
    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "item";

    /// <summary>Gets or sets the minimum quantity.</summary>
    [JsonPropertyName("minQuantity")]
    public int? MinQuantity { get; set; }

    /// <summary>Gets or sets the maximum quantity.</summary>
    [JsonPropertyName("maxQuantity")]
    public int? MaxQuantity { get; set; }

    /// <summary>Gets or sets the item type.</summary>
    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    /// <summary>Gets or sets the equipment slot.</summary>
    [JsonPropertyName("slot")]
    public string? Slot { get; set; }

    /// <summary>Gets or sets the stat bonus.</summary>
    [JsonPropertyName("statBonus")]
    public string? StatBonus { get; set; }

    /// <summary>Gets or sets the decoration type.</summary>
    [JsonPropertyName("decorationType")]
    public string? DecorationType { get; set; }

    /// <summary>Gets or sets the effect type.</summary>
    [JsonPropertyName("effectType")]
    public string? EffectType { get; set; }

    /// <summary>Gets or sets the skill type.</summary>
    [JsonPropertyName("skillType")]
    public string? SkillType { get; set; }

    /// <summary>Gets or sets the title rarity.</summary>
    [JsonPropertyName("titleRarity")]
    public string? TitleRarity { get; set; }

    /// <summary>Gets or sets the mount type.</summary>
    [JsonPropertyName("mountType")]
    public string? MountType { get; set; }

    /// <summary>Gets or sets the pet type.</summary>
    [JsonPropertyName("petType")]
    public string? PetType { get; set; }

    /// <summary>Gets or sets a value indicating whether this is quest specific.</summary>
    [JsonPropertyName("questSpecific")]
    public bool? QuestSpecific { get; set; }

    /// <summary>Gets or sets the relic type.</summary>
    [JsonPropertyName("relicType")]
    public string? RelicType { get; set; }

    /// <summary>Gets or sets the passive bonus.</summary>
    [JsonPropertyName("passiveBonus")]
    public string? PassiveBonus { get; set; }

    /// <summary>Gets or sets the treasure type.</summary>
    [JsonPropertyName("treasureType")]
    public string? TreasureType { get; set; }

    /// <summary>Gets or sets the number of choices.</summary>
    [JsonPropertyName("choices")]
    public int? Choices { get; set; }

    /// <summary>Gets or sets the choice types.</summary>
    [JsonPropertyName("choiceTypes")]
    public List<string>? ChoiceTypes { get; set; }
}

// Gold Rewards
/// <summary>
/// Collection of gold rewards by tier.
/// </summary>
public class GoldRewardsCollection
{
    /// <summary>Gets or sets the trivial gold rewards.</summary>
    [JsonPropertyName("trivial")]
    public List<GoldReward> Trivial { get; set; } = new();

    /// <summary>Gets or sets the low gold rewards.</summary>
    [JsonPropertyName("low")]
    public List<GoldReward> Low { get; set; } = new();

    /// <summary>Gets or sets the medium gold rewards.</summary>
    [JsonPropertyName("medium")]
    public List<GoldReward> Medium { get; set; } = new();

    /// <summary>Gets or sets the high gold rewards.</summary>
    [JsonPropertyName("high")]
    public List<GoldReward> High { get; set; } = new();

    /// <summary>Gets or sets the very high gold rewards.</summary>
    [JsonPropertyName("very_high")]
    public List<GoldReward> VeryHigh { get; set; } = new();

    /// <summary>Gets or sets the epic gold rewards.</summary>
    [JsonPropertyName("epic")]
    public List<GoldReward> Epic { get; set; } = new();

    /// <summary>Gets or sets the legendary gold rewards.</summary>
    [JsonPropertyName("legendary")]
    public List<GoldReward> Legendary { get; set; } = new();

    /// <summary>Gets or sets the mythic gold rewards.</summary>
    [JsonPropertyName("mythic")]
    public List<GoldReward> Mythic { get; set; } = new();

    /// <summary>Gets or sets the ancient gold rewards.</summary>
    [JsonPropertyName("ancient")]
    public List<GoldReward> Ancient { get; set; } = new();
}

/// <summary>
/// Gold reward definition.
/// </summary>
public class GoldReward
{
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the minimum amount.</summary>
    [JsonPropertyName("minAmount")]
    public int MinAmount { get; set; }

    /// <summary>Gets or sets the maximum amount.</summary>
    [JsonPropertyName("maxAmount")]
    public int MaxAmount { get; set; }

    /// <summary>Gets or sets the tier.</summary>
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the reward type.</summary>
    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "gold";
}

// Experience Rewards
/// <summary>
/// Collection of experience rewards by tier.
/// </summary>
public class ExperienceRewardsCollection
{
    /// <summary>Gets or sets the trivial experience rewards.</summary>
    [JsonPropertyName("trivial")]
    public List<ExperienceReward> Trivial { get; set; } = new();

    /// <summary>Gets or sets the low experience rewards.</summary>
    [JsonPropertyName("low")]
    public List<ExperienceReward> Low { get; set; } = new();

    /// <summary>Gets or sets the medium experience rewards.</summary>
    [JsonPropertyName("medium")]
    public List<ExperienceReward> Medium { get; set; } = new();

    /// <summary>Gets or sets the high experience rewards.</summary>
    [JsonPropertyName("high")]
    public List<ExperienceReward> High { get; set; } = new();

    /// <summary>Gets or sets the very high experience rewards.</summary>
    [JsonPropertyName("very_high")]
    public List<ExperienceReward> VeryHigh { get; set; } = new();

    /// <summary>Gets or sets the epic experience rewards.</summary>
    [JsonPropertyName("epic")]
    public List<ExperienceReward> Epic { get; set; } = new();

    /// <summary>Gets or sets the legendary experience rewards.</summary>
    [JsonPropertyName("legendary")]
    public List<ExperienceReward> Legendary { get; set; } = new();

    /// <summary>Gets or sets the mythic experience rewards.</summary>
    [JsonPropertyName("mythic")]
    public List<ExperienceReward> Mythic { get; set; } = new();

    /// <summary>Gets or sets the ancient experience rewards.</summary>
    [JsonPropertyName("ancient")]
    public List<ExperienceReward> Ancient { get; set; } = new();
}

/// <summary>
/// Experience reward definition.
/// </summary>
public class ExperienceReward
{
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the minimum amount.</summary>
    [JsonPropertyName("minAmount")]
    public int? MinAmount { get; set; }

    /// <summary>Gets or sets the maximum amount.</summary>
    [JsonPropertyName("maxAmount")]
    public int? MaxAmount { get; set; }

    /// <summary>Gets or sets the tier.</summary>
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the reward type.</summary>
    [JsonPropertyName("rewardType")]
    public string RewardType { get; set; } = "experience";

    /// <summary>Gets or sets a value indicating whether the amount is dynamic.</summary>
    [JsonPropertyName("dynamicAmount")]
    public bool? DynamicAmount { get; set; }
}
