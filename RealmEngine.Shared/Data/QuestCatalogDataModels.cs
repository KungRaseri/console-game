using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data;

/// <summary>
/// Root data model for quest catalog (templates + locations).
/// Loaded from quests/catalog.json.
/// </summary>
public class QuestCatalogData
{
    [JsonPropertyName("metadata")]
    public QuestCatalogMetadata Metadata { get; set; } = new();

    [JsonPropertyName("components")]
    public QuestCatalogComponents Components { get; set; } = new();
}

public class QuestCatalogMetadata
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

    [JsonPropertyName("total_templates")]
    public int TotalTemplates { get; set; }

    [JsonPropertyName("total_locations")]
    public int TotalLocations { get; set; }

    [JsonPropertyName("quest_types")]
    public List<string> QuestTypes { get; set; } = new();

    [JsonPropertyName("difficulty_levels")]
    public List<string> DifficultyLevels { get; set; } = new();

    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public QuestCatalogCategories Categories { get; set; } = new();
}

public class QuestCatalogCategories
{
    [JsonPropertyName("templates")]
    public List<string> Templates { get; set; } = new();

    [JsonPropertyName("locations")]
    public List<string> Locations { get; set; } = new();
}

public class QuestCatalogComponents
{
    [JsonPropertyName("templates")]
    public QuestTemplatesCollection Templates { get; set; } = new();

    [JsonPropertyName("locations")]
    public QuestLocationsCollection Locations { get; set; } = new();
}

// Quest Templates
public class QuestTemplatesCollection
{
    [JsonPropertyName("fetch")]
    public FetchQuestTemplates Fetch { get; set; } = new();

    [JsonPropertyName("kill")]
    public KillQuestTemplates Kill { get; set; } = new();

    [JsonPropertyName("escort")]
    public EscortQuestTemplates Escort { get; set; } = new();

    [JsonPropertyName("delivery")]
    public DeliveryQuestTemplates Delivery { get; set; } = new();

    [JsonPropertyName("investigate")]
    public InvestigateQuestTemplates Investigate { get; set; } = new();
}

public class FetchQuestTemplates
{
    [JsonPropertyName("easy_fetch")]
    public List<QuestTemplate> EasyFetch { get; set; } = new();

    [JsonPropertyName("medium_fetch")]
    public List<QuestTemplate> MediumFetch { get; set; } = new();

    [JsonPropertyName("hard_fetch")]
    public List<QuestTemplate> HardFetch { get; set; } = new();
}

public class KillQuestTemplates
{
    [JsonPropertyName("easy_combat")]
    public List<QuestTemplate> EasyCombat { get; set; } = new();

    [JsonPropertyName("medium_combat")]
    public List<QuestTemplate> MediumCombat { get; set; } = new();

    [JsonPropertyName("hard_combat")]
    public List<QuestTemplate> HardCombat { get; set; } = new();
}

public class EscortQuestTemplates
{
    [JsonPropertyName("easy_escort")]
    public List<QuestTemplate> EasyEscort { get; set; } = new();

    [JsonPropertyName("medium_escort")]
    public List<QuestTemplate> MediumEscort { get; set; } = new();

    [JsonPropertyName("hard_escort")]
    public List<QuestTemplate> HardEscort { get; set; } = new();
}

public class DeliveryQuestTemplates
{
    [JsonPropertyName("easy_delivery")]
    public List<QuestTemplate> EasyDelivery { get; set; } = new();

    [JsonPropertyName("medium_delivery")]
    public List<QuestTemplate> MediumDelivery { get; set; } = new();

    [JsonPropertyName("hard_delivery")]
    public List<QuestTemplate> HardDelivery { get; set; } = new();
}

public class InvestigateQuestTemplates
{
    [JsonPropertyName("easy_investigation")]
    public List<QuestTemplate> EasyInvestigation { get; set; } = new();

    [JsonPropertyName("medium_investigation")]
    public List<QuestTemplate> MediumInvestigation { get; set; } = new();

    [JsonPropertyName("hard_investigation")]
    public List<QuestTemplate> HardInvestigation { get; set; } = new();
}

public class QuestTemplate
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("questType")]
    public string QuestType { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("baseGoldReward")]
    public int BaseGoldReward { get; set; }

    [JsonPropertyName("baseXpReward")]
    public int BaseXpReward { get; set; }

    // Optional properties (vary by quest type)
    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    [JsonPropertyName("itemRarity")]
    public string? ItemRarity { get; set; }

    [JsonPropertyName("minQuantity")]
    public int? MinQuantity { get; set; }

    [JsonPropertyName("maxQuantity")]
    public int? MaxQuantity { get; set; }

    [JsonPropertyName("targetType")]
    public string? TargetType { get; set; }

    [JsonPropertyName("npcType")]
    public string? NpcType { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("distance")]
    public string? Distance { get; set; }

    [JsonPropertyName("timeLimit")]
    public int? TimeLimit { get; set; }

    [JsonPropertyName("minClues")]
    public int? MinClues { get; set; }

    [JsonPropertyName("maxClues")]
    public int? MaxClues { get; set; }

    [JsonPropertyName("itemFragile")]
    public bool? ItemFragile { get; set; }

    [JsonPropertyName("urgent")]
    public bool? Urgent { get; set; }

    [JsonPropertyName("guardians")]
    public bool? Guardians { get; set; }

    [JsonPropertyName("bossFight")]
    public bool? BossFight { get; set; }

    [JsonPropertyName("legendary")]
    public bool? Legendary { get; set; }

    [JsonPropertyName("stealthRequired")]
    public int? StealthRequired { get; set; }

    [JsonPropertyName("moralChoice")]
    public string? MoralChoice { get; set; }

    [JsonPropertyName("failureConsequence")]
    public string? FailureConsequence { get; set; }

    [JsonPropertyName("bonusReward")]
    public string? BonusReward { get; set; }

    [JsonPropertyName("pvpQuest")]
    public bool? PvpQuest { get; set; }

    [JsonPropertyName("ambushChance")]
    public int? AmbushChance { get; set; }

    [JsonPropertyName("npcCanDie")]
    public bool? NpcCanDie { get; set; }

    [JsonPropertyName("bandits")]
    public bool? Bandits { get; set; }

    [JsonPropertyName("cursed")]
    public bool? Cursed { get; set; }

    [JsonPropertyName("pursued")]
    public bool? Pursued { get; set; }

    [JsonPropertyName("combatOptional")]
    public bool? CombatOptional { get; set; }

    [JsonPropertyName("multipleEndings")]
    public bool? MultipleEndings { get; set; }

    [JsonPropertyName("stealth")]
    public int? Stealth { get; set; }

    [JsonPropertyName("betrayal")]
    public bool? Betrayal { get; set; }
}

// Quest Locations
public class QuestLocationsCollection
{
    [JsonPropertyName("wilderness")]
    public WildernessLocations Wilderness { get; set; } = new();

    [JsonPropertyName("towns")]
    public TownLocations Towns { get; set; } = new();

    [JsonPropertyName("dungeons")]
    public DungeonLocations Dungeons { get; set; } = new();
}

public class WildernessLocations
{
    [JsonPropertyName("low_danger")]
    public List<QuestLocation> LowDanger { get; set; } = new();

    [JsonPropertyName("medium_danger")]
    public List<QuestLocation> MediumDanger { get; set; } = new();

    [JsonPropertyName("high_danger")]
    public List<QuestLocation> HighDanger { get; set; } = new();

    [JsonPropertyName("very_high_danger")]
    public List<QuestLocation> VeryHighDanger { get; set; } = new();
}

public class TownLocations
{
    [JsonPropertyName("outposts")]
    public List<QuestLocation> Outposts { get; set; } = new();

    [JsonPropertyName("villages")]
    public List<QuestLocation> Villages { get; set; } = new();

    [JsonPropertyName("towns")]
    public List<QuestLocation> Towns { get; set; } = new();

    [JsonPropertyName("cities")]
    public List<QuestLocation> Cities { get; set; } = new();

    [JsonPropertyName("capitals")]
    public List<QuestLocation> Capitals { get; set; } = new();

    [JsonPropertyName("special_locations")]
    public List<QuestLocation> SpecialLocations { get; set; } = new();
}

public class DungeonLocations
{
    [JsonPropertyName("easy_dungeons")]
    public List<QuestLocation> EasyDungeons { get; set; } = new();

    [JsonPropertyName("medium_dungeons")]
    public List<QuestLocation> MediumDungeons { get; set; } = new();

    [JsonPropertyName("hard_dungeons")]
    public List<QuestLocation> HardDungeons { get; set; } = new();

    [JsonPropertyName("very_hard_dungeons")]
    public List<QuestLocation> VeryHardDungeons { get; set; } = new();

    [JsonPropertyName("epic_dungeons")]
    public List<QuestLocation> EpicDungeons { get; set; } = new();

    [JsonPropertyName("legendary_dungeons")]
    public List<QuestLocation> LegendaryDungeons { get; set; } = new();
}

public class QuestLocation
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("locationType")]
    public string LocationType { get; set; } = string.Empty;

    // Wilderness properties
    [JsonPropertyName("terrain")]
    public string? Terrain { get; set; }

    [JsonPropertyName("danger")]
    public string? Danger { get; set; }

    // Settlement properties
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("population")]
    public string? Population { get; set; }

    // Dungeon properties
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }

    [JsonPropertyName("enemy_types")]
    public List<string>? EnemyTypes { get; set; }
}
