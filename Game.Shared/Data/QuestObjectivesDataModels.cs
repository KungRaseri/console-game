using System.Text.Json.Serialization;

namespace Game.Shared.Data;

/// <summary>
/// Root data model for quest objectives (primary, secondary, hidden).
/// Loaded from quests/objectives.json.
/// </summary>
public class QuestObjectivesData
{
    [JsonPropertyName("metadata")]
    public QuestObjectivesMetadata Metadata { get; set; } = new();

    [JsonPropertyName("components")]
    public QuestObjectivesComponents Components { get; set; } = new();
}

public class QuestObjectivesMetadata
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

    [JsonPropertyName("total_objectives")]
    public int TotalObjectives { get; set; }

    [JsonPropertyName("objective_types")]
    public List<string> ObjectiveTypes { get; set; } = new();

    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public QuestObjectivesCategories Categories { get; set; } = new();
}

public class QuestObjectivesCategories
{
    [JsonPropertyName("primary")]
    public List<string> Primary { get; set; } = new();

    [JsonPropertyName("secondary")]
    public List<string> Secondary { get; set; } = new();

    [JsonPropertyName("hidden")]
    public List<string> Hidden { get; set; } = new();
}

public class QuestObjectivesComponents
{
    [JsonPropertyName("primary")]
    public PrimaryObjectivesCollection Primary { get; set; } = new();

    [JsonPropertyName("secondary")]
    public SecondaryObjectivesCollection Secondary { get; set; } = new();

    [JsonPropertyName("hidden")]
    public HiddenObjectivesCollection Hidden { get; set; } = new();
}

// Primary Objectives
public class PrimaryObjectivesCollection
{
    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    [JsonPropertyName("retrieval")]
    public List<QuestObjective> Retrieval { get; set; } = new();

    [JsonPropertyName("rescue")]
    public List<QuestObjective> Rescue { get; set; } = new();

    [JsonPropertyName("purification")]
    public List<QuestObjective> Purification { get; set; } = new();

    [JsonPropertyName("defense")]
    public List<QuestObjective> Defense { get; set; } = new();

    [JsonPropertyName("social")]
    public List<QuestObjective> Social { get; set; } = new();

    [JsonPropertyName("timed")]
    public List<QuestObjective> Timed { get; set; } = new();
}

// Secondary Objectives
public class SecondaryObjectivesCollection
{
    [JsonPropertyName("stealth")]
    public List<QuestObjective> Stealth { get; set; } = new();

    [JsonPropertyName("survival")]
    public List<QuestObjective> Survival { get; set; } = new();

    [JsonPropertyName("speed")]
    public List<QuestObjective> Speed { get; set; } = new();

    [JsonPropertyName("collection")]
    public List<QuestObjective> Collection { get; set; } = new();

    [JsonPropertyName("mercy")]
    public List<QuestObjective> Mercy { get; set; } = new();

    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    [JsonPropertyName("precision")]
    public List<QuestObjective> Precision { get; set; } = new();

    [JsonPropertyName("social")]
    public List<QuestObjective> Social { get; set; } = new();
}

// Hidden Objectives
public class HiddenObjectivesCollection
{
    [JsonPropertyName("exploration")]
    public List<QuestObjective> Exploration { get; set; } = new();

    [JsonPropertyName("lore")]
    public List<QuestObjective> Lore { get; set; } = new();

    [JsonPropertyName("combat")]
    public List<QuestObjective> Combat { get; set; } = new();

    [JsonPropertyName("branching")]
    public List<QuestObjective> Branching { get; set; } = new();

    [JsonPropertyName("collection")]
    public List<QuestObjective> Collection { get; set; } = new();

    [JsonPropertyName("puzzle")]
    public List<QuestObjective> Puzzle { get; set; } = new();

    [JsonPropertyName("rescue")]
    public List<QuestObjective> Rescue { get; set; } = new();

    [JsonPropertyName("diplomacy")]
    public List<QuestObjective> Diplomacy { get; set; } = new();

    [JsonPropertyName("timed")]
    public List<QuestObjective> Timed { get; set; } = new();

    [JsonPropertyName("purification")]
    public List<QuestObjective> Purification { get; set; } = new();
}

public class QuestObjective
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("objectiveType")]
    public string ObjectiveType { get; set; } = string.Empty;

    [JsonPropertyName("bonusReward")]
    public string? BonusReward { get; set; }

    // Optional properties for specific objective types
    [JsonPropertyName("minKills")]
    public int? MinKills { get; set; }

    [JsonPropertyName("maxKills")]
    public int? MaxKills { get; set; }

    [JsonPropertyName("enemyType")]
    public string? EnemyType { get; set; }

    [JsonPropertyName("bossRequired")]
    public bool? BossRequired { get; set; }

    [JsonPropertyName("itemRequired")]
    public bool? ItemRequired { get; set; }

    [JsonPropertyName("targetType")]
    public string? TargetType { get; set; }

    [JsonPropertyName("minTargets")]
    public int? MinTargets { get; set; }

    [JsonPropertyName("maxTargets")]
    public int? MaxTargets { get; set; }

    [JsonPropertyName("areaType")]
    public string? AreaType { get; set; }

    [JsonPropertyName("corruptionSource")]
    public string? CorruptionSource { get; set; }

    [JsonPropertyName("minWaves")]
    public int? MinWaves { get; set; }

    [JsonPropertyName("maxWaves")]
    public int? MaxWaves { get; set; }

    [JsonPropertyName("allowedLosses")]
    public int? AllowedLosses { get; set; }

    [JsonPropertyName("dialogueChallenge")]
    public string? DialogueChallenge { get; set; }

    [JsonPropertyName("consequenceOfFailure")]
    public string? ConsequenceOfFailure { get; set; }

    [JsonPropertyName("timeLimit")]
    public int? TimeLimit { get; set; }

    [JsonPropertyName("penaltyForFailure")]
    public string? PenaltyForFailure { get; set; }

    [JsonPropertyName("detectionAllowed")]
    public int? DetectionAllowed { get; set; }

    [JsonPropertyName("equipmentRestriction")]
    public string? EquipmentRestriction { get; set; }

    [JsonPropertyName("consumablesAllowed")]
    public bool? ConsumablesAllowed { get; set; }

    [JsonPropertyName("minCompletion")]
    public int? MinCompletion { get; set; }

    [JsonPropertyName("resourceType")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("minResource")]
    public int? MinResource { get; set; }

    [JsonPropertyName("maxResource")]
    public int? MaxResource { get; set; }

    [JsonPropertyName("rareChance")]
    public int? RareChance { get; set; }

    [JsonPropertyName("allowHealing")]
    public bool? AllowHealing { get; set; }

    [JsonPropertyName("minAccuracy")]
    public int? MinAccuracy { get; set; }

    [JsonPropertyName("weapon")]
    public string? Weapon { get; set; }

    [JsonPropertyName("specialTarget")]
    public string? SpecialTarget { get; set; }

    [JsonPropertyName("persuasionCheck")]
    public int? PersuasionCheck { get; set; }

    [JsonPropertyName("minAreas")]
    public int? MinAreas { get; set; }

    [JsonPropertyName("maxAreas")]
    public int? MaxAreas { get; set; }

    [JsonPropertyName("discoveryType")]
    public string? DiscoveryType { get; set; }

    [JsonPropertyName("minLore")]
    public int? MinLore { get; set; }

    [JsonPropertyName("maxLore")]
    public int? MaxLore { get; set; }

    [JsonPropertyName("loreType")]
    public string? LoreType { get; set; }

    [JsonPropertyName("revealCondition")]
    public string? RevealCondition { get; set; }

    [JsonPropertyName("choices")]
    public List<string>? Choices { get; set; }

    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }

    [JsonPropertyName("collectibleType")]
    public string? CollectibleType { get; set; }

    [JsonPropertyName("puzzleType")]
    public string? PuzzleType { get; set; }

    [JsonPropertyName("rewardTier")]
    public string? RewardTier { get; set; }

    [JsonPropertyName("failureConsequence")]
    public string? FailureConsequence { get; set; }

    [JsonPropertyName("factionInvolved")]
    public string? FactionInvolved { get; set; }

    [JsonPropertyName("reputationChange")]
    public int? ReputationChange { get; set; }

    [JsonPropertyName("urgency")]
    public string? Urgency { get; set; }

    [JsonPropertyName("penaltyPerMinute")]
    public int? PenaltyPerMinute { get; set; }

    [JsonPropertyName("minCleansed")]
    public int? MinCleansed { get; set; }

    [JsonPropertyName("maxCleansed")]
    public int? MaxCleansed { get; set; }

    [JsonPropertyName("cleansedType")]
    public string? CleansedType { get; set; }

    [JsonPropertyName("ritualRequired")]
    public bool? RitualRequired { get; set; }
}
