using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data;

/// <summary>
/// Root data model for quest objectives (primary, secondary, hidden).
/// Loaded from quests/objectives.json.
/// </summary>
public class QuestObjectivesData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public QuestObjectivesMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the components.</summary>
    [JsonPropertyName("components")]
    public QuestObjectivesComponents Components { get; set; } = new();
}

/// <summary>
/// Metadata for quest objectives data.
/// </summary>
public class QuestObjectivesMetadata
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

    /// <summary>Gets or sets the total objectives.</summary>
    [JsonPropertyName("total_objectives")]
    public int TotalObjectives { get; set; }

    /// <summary>Gets or sets the objective types.</summary>
    [JsonPropertyName("objective_types")]
    public List<string> ObjectiveTypes { get; set; } = new();

    /// <summary>Gets or sets the usage.</summary>
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    /// <summary>Gets or sets the categories.</summary>
    [JsonPropertyName("categories")]
    public QuestObjectivesCategories Categories { get; set; } = new();
}

/// <summary>
/// Categories of quest objectives.
/// </summary>
public class QuestObjectivesCategories
{
    /// <summary>Gets or sets the primary objective categories.</summary>
    [JsonPropertyName("primary")]
    public List<string> Primary { get; set; } = new();

    /// <summary>Gets or sets the secondary objective categories.</summary>
    [JsonPropertyName("secondary")]
    public List<string> Secondary { get; set; } = new();

    /// <summary>Gets or sets the hidden objective categories.</summary>
    [JsonPropertyName("hidden")]
    public List<string> Hidden { get; set; } = new();
}

/// <summary>
/// Components containing quest objective collections.
/// </summary>
public class QuestObjectivesComponents
{
    /// <summary>Gets or sets the primary objectives.</summary>
    [JsonPropertyName("primary")]
    public PrimaryObjectivesCollection Primary { get; set; } = new();

    /// <summary>Gets or sets the secondary objectives.</summary>
    [JsonPropertyName("secondary")]
    public SecondaryObjectivesCollection Secondary { get; set; } = new();

    /// <summary>Gets or sets the hidden objectives.</summary>
    [JsonPropertyName("hidden")]
    public HiddenObjectivesCollection Hidden { get; set; } = new();
}

// Primary Objectives
/// <summary>
/// Collection of primary quest objectives by category.
/// </summary>
public class PrimaryObjectivesCollection
{
    /// <summary>Gets or sets the combat objectives.</summary>
    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    /// <summary>Gets or sets the retrieval objectives.</summary>
    [JsonPropertyName("retrieval")]
    public List<QuestObjective> Retrieval { get; set; } = new();

    /// <summary>Gets or sets the rescue objectives.</summary>
    [JsonPropertyName("rescue")]
    public List<QuestObjective> Rescue { get; set; } = new();

    /// <summary>Gets or sets the purification objectives.</summary>
    [JsonPropertyName("purification")]
    public List<QuestObjective> Purification { get; set; } = new();

    /// <summary>Gets or sets the defense objectives.</summary>
    [JsonPropertyName("defense")]
    public List<QuestObjective> Defense { get; set; } = new();

    /// <summary>Gets or sets the social objectives.</summary>
    [JsonPropertyName("social")]
    public List<QuestObjective> Social { get; set; } = new();

    /// <summary>Gets or sets the timed objectives.</summary>
    [JsonPropertyName("timed")]
    public List<QuestObjective> Timed { get; set; } = new();
}

// Secondary Objectives
/// <summary>
/// Collection of secondary quest objectives by category.
/// </summary>
public class SecondaryObjectivesCollection
{
    /// <summary>Gets or sets the stealth objectives.</summary>
    [JsonPropertyName("stealth")]
    public List<QuestObjective> Stealth { get; set; } = new();

    /// <summary>Gets or sets the survival objectives.</summary>
    [JsonPropertyName("survival")]
    public List<QuestObjective> Survival { get; set; } = new();

    /// <summary>Gets or sets the speed objectives.</summary>
    [JsonPropertyName("speed")]
    public List<QuestObjective> Speed { get; set; } = new();

    /// <summary>Gets or sets the collection objectives.</summary>
    [JsonPropertyName("collection")]
    public List<QuestObjective> Collection { get; set; } = new();

    /// <summary>Gets or sets the mercy objectives.</summary>
    [JsonPropertyName("mercy")]
    public List<QuestObjective> Mercy { get; set; } = new();

    /// <summary>Gets or sets the combat objectives.</summary>
    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    /// <summary>Gets or sets the precision objectives.</summary>
    [JsonPropertyName("precision")]
    public List<QuestObjective> Precision { get; set; } = new();

    /// <summary>Gets or sets the social objectives.</summary>
    [JsonPropertyName("social")]
    public List<QuestObjective> Social { get; set; } = new();
}

// Hidden Objectives
/// <summary>
/// Collection of hidden quest objectives by category.
/// </summary>
public class HiddenObjectivesCollection
{
    /// <summary>Gets or sets the exploration objectives.</summary>
    [JsonPropertyName("exploration")]
    public List<QuestObjective> Exploration { get; set; } = new();

    /// <summary>Gets or sets the lore objectives.</summary>
    [JsonPropertyName("lore")]
    public List<QuestObjective> Lore { get; set; } = new();

    /// <summary>Gets or sets the combat objectives.</summary>
    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    /// <summary>Gets or sets the branching objectives.</summary>
    [JsonPropertyName("branching")]
    public List<QuestObjective> Branching { get; set; } = new();

    /// <summary>Gets or sets the collection objectives.</summary>
    [JsonPropertyName("collection")]
    public List<QuestObjective> Collection { get; set; } = new();

    /// <summary>Gets or sets the puzzle objectives.</summary>
    [JsonPropertyName("puzzle")]
    public List<QuestObjective> Puzzle { get; set; } = new();

    /// <summary>Gets or sets the rescue objectives.</summary>
    [JsonPropertyName("rescue")]
    public List<QuestObjective> Rescue { get; set; } = new();

    /// <summary>Gets or sets the diplomacy objectives.</summary>
    [JsonPropertyName("diplomacy")]
    public List<QuestObjective> Diplomacy { get; set; } = new();

    /// <summary>Gets or sets the timed objectives.</summary>
    [JsonPropertyName("timed")]
    public List<QuestObjective> Timed { get; set; } = new();

    /// <summary>Gets or sets the purification objectives.</summary>
    [JsonPropertyName("purification")]
    public List<QuestObjective> Purification { get; set; } = new();
}

/// <summary>
/// Definition of a quest objective with configurable properties.
/// </summary>
public class QuestObjective
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

    /// <summary>Gets or sets the category.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the difficulty.</summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the objective type.</summary>
    [JsonPropertyName("objectiveType")]
    public string ObjectiveType { get; set; } = string.Empty;

    /// <summary>Gets or sets the bonus reward.</summary>
    [JsonPropertyName("bonusReward")]
    public string? BonusReward { get; set; }

    // Optional properties for specific objective types
    /// <summary>Gets or sets the minimum kills (for combat objectives).</summary>
    [JsonPropertyName("minKills")]
    public int? MinKills { get; set; }

    /// <summary>Gets or sets the maximum kills (for combat objectives).</summary>
    [JsonPropertyName("maxKills")]
    public int? MaxKills { get; set; }

    /// <summary>Gets or sets the enemy type (for combat objectives).</summary>
    [JsonPropertyName("enemyType")]
    public string? EnemyType { get; set; }

    /// <summary>Gets or sets a value indicating whether a boss is required.</summary>
    [JsonPropertyName("bossRequired")]
    public bool? BossRequired { get; set; }

    /// <summary>Gets or sets a value indicating whether an item is required.</summary>
    [JsonPropertyName("itemRequired")]
    public bool? ItemRequired { get; set; }

    /// <summary>Gets or sets the target type (for rescue/retrieval objectives).</summary>
    [JsonPropertyName("targetType")]
    public string? TargetType { get; set; }

    /// <summary>Gets or sets the minimum targets.</summary>
    [JsonPropertyName("minTargets")]
    public int? MinTargets { get; set; }

    /// <summary>Gets or sets the maximum targets.</summary>
    [JsonPropertyName("maxTargets")]
    public int? MaxTargets { get; set; }

    /// <summary>Gets or sets the area type.</summary>
    [JsonPropertyName("areaType")]
    public string? AreaType { get; set; }

    /// <summary>Gets or sets the corruption source (for purification objectives).</summary>
    [JsonPropertyName("corruptionSource")]
    public string? CorruptionSource { get; set; }

    /// <summary>Gets or sets the minimum waves (for defense objectives).</summary>
    [JsonPropertyName("minWaves")]
    public int? MinWaves { get; set; }

    /// <summary>Gets or sets the maximum waves (for defense objectives).</summary>
    [JsonPropertyName("maxWaves")]
    public int? MaxWaves { get; set; }

    /// <summary>Gets or sets the allowed losses (for defense objectives).</summary>
    [JsonPropertyName("allowedLosses")]
    public int? AllowedLosses { get; set; }

    /// <summary>Gets or sets the dialogue challenge (for social objectives).</summary>
    [JsonPropertyName("dialogueChallenge")]
    public string? DialogueChallenge { get; set; }

    /// <summary>Gets or sets the consequence of failure.</summary>
    [JsonPropertyName("consequenceOfFailure")]
    public string? ConsequenceOfFailure { get; set; }

    /// <summary>Gets or sets the time limit (in minutes).</summary>
    [JsonPropertyName("timeLimit")]
    public int? TimeLimit { get; set; }

    /// <summary>Gets or sets the penalty for failure.</summary>
    [JsonPropertyName("penaltyForFailure")]
    public string? PenaltyForFailure { get; set; }

    /// <summary>Gets or sets the detection allowed count (for stealth objectives).</summary>
    [JsonPropertyName("detectionAllowed")]
    public int? DetectionAllowed { get; set; }

    /// <summary>Gets or sets the equipment restriction (for survival objectives).</summary>
    [JsonPropertyName("equipmentRestriction")]
    public string? EquipmentRestriction { get; set; }

    /// <summary>Gets or sets a value indicating whether consumables are allowed.</summary>
    [JsonPropertyName("consumablesAllowed")]
    public bool? ConsumablesAllowed { get; set; }

    /// <summary>Gets or sets the minimum completion percentage.</summary>
    [JsonPropertyName("minCompletion")]
    public int? MinCompletion { get; set; }

    /// <summary>Gets or sets the resource type (for collection objectives).</summary>
    [JsonPropertyName("resourceType")]
    public string? ResourceType { get; set; }

    /// <summary>Gets or sets the minimum resource count.</summary>
    [JsonPropertyName("minResource")]
    public int? MinResource { get; set; }

    /// <summary>Gets or sets the maximum resource count.</summary>
    [JsonPropertyName("maxResource")]
    public int? MaxResource { get; set; }

    /// <summary>Gets or sets the rare item chance percentage.</summary>
    [JsonPropertyName("rareChance")]
    public int? RareChance { get; set; }

    /// <summary>Gets or sets a value indicating whether healing is allowed.</summary>
    [JsonPropertyName("allowHealing")]
    public bool? AllowHealing { get; set; }

    /// <summary>Gets or sets the minimum accuracy percentage (for precision objectives).</summary>
    [JsonPropertyName("minAccuracy")]
    public int? MinAccuracy { get; set; }

    /// <summary>Gets or sets the weapon restriction.</summary>
    [JsonPropertyName("weapon")]
    public string? Weapon { get; set; }

    /// <summary>Gets or sets the special target.</summary>
    [JsonPropertyName("specialTarget")]
    public string? SpecialTarget { get; set; }

    /// <summary>Gets or sets the persuasion check level (for social objectives).</summary>
    [JsonPropertyName("persuasionCheck")]
    public int? PersuasionCheck { get; set; }

    /// <summary>Gets or sets the minimum areas (for exploration objectives).</summary>
    [JsonPropertyName("minAreas")]
    public int? MinAreas { get; set; }

    /// <summary>Gets or sets the maximum areas (for exploration objectives).</summary>
    [JsonPropertyName("maxAreas")]
    public int? MaxAreas { get; set; }

    /// <summary>Gets or sets the discovery type.</summary>
    [JsonPropertyName("discoveryType")]
    public string? DiscoveryType { get; set; }

    /// <summary>Gets or sets the minimum lore items (for lore objectives).</summary>
    [JsonPropertyName("minLore")]
    public int? MinLore { get; set; }

    /// <summary>Gets or sets the maximum lore items (for lore objectives).</summary>
    [JsonPropertyName("maxLore")]
    public int? MaxLore { get; set; }

    /// <summary>Gets or sets the lore type.</summary>
    [JsonPropertyName("loreType")]
    public string? LoreType { get; set; }

    /// <summary>Gets or sets the reveal condition (for hidden objectives).</summary>
    [JsonPropertyName("revealCondition")]
    public string? RevealCondition { get; set; }

    /// <summary>Gets or sets the choices (for branching objectives).</summary>
    [JsonPropertyName("choices")]
    public List<string>? Choices { get; set; }

    /// <summary>Gets or sets the outcome.</summary>
    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }

    /// <summary>Gets or sets the collectible type.</summary>
    [JsonPropertyName("collectibleType")]
    public string? CollectibleType { get; set; }

    /// <summary>Gets or sets the puzzle type.</summary>
    [JsonPropertyName("puzzleType")]
    public string? PuzzleType { get; set; }

    /// <summary>Gets or sets the reward tier.</summary>
    [JsonPropertyName("rewardTier")]
    public string? RewardTier { get; set; }

    /// <summary>Gets or sets the failure consequence.</summary>
    [JsonPropertyName("failureConsequence")]
    public string? FailureConsequence { get; set; }

    /// <summary>Gets or sets the faction involved.</summary>
    [JsonPropertyName("factionInvolved")]
    public string? FactionInvolved { get; set; }

    /// <summary>Gets or sets the reputation change.</summary>
    [JsonPropertyName("reputationChange")]
    public int? ReputationChange { get; set; }

    /// <summary>Gets or sets the urgency level.</summary>
    [JsonPropertyName("urgency")]
    public string? Urgency { get; set; }

    /// <summary>Gets or sets the penalty per minute.</summary>
    [JsonPropertyName("penaltyPerMinute")]
    public int? PenaltyPerMinute { get; set; }

    /// <summary>Gets or sets the minimum cleansed count (for purification objectives).</summary>
    [JsonPropertyName("minCleansed")]
    public int? MinCleansed { get; set; }

    /// <summary>Gets or sets the maximum cleansed count (for purification objectives).</summary>
    [JsonPropertyName("maxCleansed")]
    public int? MaxCleansed { get; set; }

    /// <summary>Gets or sets the cleansed type.</summary>
    [JsonPropertyName("cleansedType")]
    public string? CleansedType { get; set; }

    /// <summary>Gets or sets a value indicating whether a ritual is required.</summary>
    [JsonPropertyName("ritualRequired")]
    public bool? RitualRequired { get; set; }
}
