using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data;

/// <summary>
/// Root data model for quest catalog (templates + locations).
/// Loaded from quests/catalog.json.
/// </summary>
public class QuestCatalogData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public QuestCatalogMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the components.</summary>
    [JsonPropertyName("components")]
    public QuestCatalogComponents Components { get; set; } = new();
}

/// <summary>
/// Metadata for quest catalog.
/// </summary>
public class QuestCatalogMetadata
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

    /// <summary>Gets or sets the total templates.</summary>
    [JsonPropertyName("total_templates")]
    public int TotalTemplates { get; set; }

    /// <summary>Gets or sets the total locations.</summary>
    [JsonPropertyName("total_locations")]
    public int TotalLocations { get; set; }

    /// <summary>Gets or sets the quest types.</summary>
    [JsonPropertyName("quest_types")]
    public List<string> QuestTypes { get; set; } = new();

    /// <summary>Gets or sets the difficulty levels.</summary>
    [JsonPropertyName("difficulty_levels")]
    public List<string> DifficultyLevels { get; set; } = new();

    /// <summary>Gets or sets the usage.</summary>
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;

    /// <summary>Gets or sets the categories.</summary>
    [JsonPropertyName("categories")]
    public QuestCatalogCategories Categories { get; set; } = new();
}

/// <summary>
/// Categories in quest catalog.
/// </summary>
public class QuestCatalogCategories
{
    /// <summary>Gets or sets the templates.</summary>
    [JsonPropertyName("templates")]
    public List<string> Templates { get; set; } = new();

    /// <summary>Gets or sets the locations.</summary>
    [JsonPropertyName("locations")]
    public List<string> Locations { get; set; } = new();
}

/// <summary>
/// Components of quest catalog (templates and locations).
/// </summary>
public class QuestCatalogComponents
{
    /// <summary>Gets or sets the templates.</summary>
    [JsonPropertyName("templates")]
    public QuestTemplatesCollection Templates { get; set; } = new();

    /// <summary>Gets or sets the locations.</summary>
    [JsonPropertyName("locations")]
    public QuestLocationsCollection Locations { get; set; } = new();
}

// Quest Templates
/// <summary>
/// Collection of quest templates by type.
/// </summary>
public class QuestTemplatesCollection
{
    /// <summary>Gets or sets the fetch quest templates.</summary>
    [JsonPropertyName("fetch")]
    public FetchQuestTemplates Fetch { get; set; } = new();

    /// <summary>Gets or sets the kill quest templates.</summary>
    [JsonPropertyName("kill")]
    public KillQuestTemplates Kill { get; set; } = new();

    /// <summary>Gets or sets the escort quest templates.</summary>
    [JsonPropertyName("escort")]
    public EscortQuestTemplates Escort { get; set; } = new();

    /// <summary>Gets or sets the delivery quest templates.</summary>
    [JsonPropertyName("delivery")]
    public DeliveryQuestTemplates Delivery { get; set; } = new();

    /// <summary>Gets or sets the investigate quest templates.</summary>
    [JsonPropertyName("investigate")]
    public InvestigateQuestTemplates Investigate { get; set; } = new();
}

/// <summary>
/// Fetch quest templates by difficulty.
/// </summary>
public class FetchQuestTemplates
{
    /// <summary>Gets or sets the easy fetch quests.</summary>
    [JsonPropertyName("easy_fetch")]
    public List<QuestTemplate> EasyFetch { get; set; } = new();

    /// <summary>Gets or sets the medium fetch quests.</summary>
    [JsonPropertyName("medium_fetch")]
    public List<QuestTemplate> MediumFetch { get; set; } = new();

    /// <summary>Gets or sets the hard fetch quests.</summary>
    [JsonPropertyName("hard_fetch")]
    public List<QuestTemplate> HardFetch { get; set; } = new();
}

/// <summary>
/// Kill/combat quest templates by difficulty.
/// </summary>
public class KillQuestTemplates
{
    /// <summary>Gets or sets the easy combat quests.</summary>
    [JsonPropertyName("easy_combat")]
    public List<QuestTemplate> EasyCombat { get; set; } = new();

    /// <summary>Gets or sets the medium combat quests.</summary>
    [JsonPropertyName("medium_combat")]
    public List<QuestTemplate> MediumCombat { get; set; } = new();

    /// <summary>Gets or sets the hard combat quests.</summary>
    [JsonPropertyName("hard_combat")]
    public List<QuestTemplate> HardCombat { get; set; } = new();
}

/// <summary>
/// Escort quest templates by difficulty.
/// </summary>
public class EscortQuestTemplates
{
    /// <summary>Gets or sets the easy escort quests.</summary>
    [JsonPropertyName("easy_escort")]
    public List<QuestTemplate> EasyEscort { get; set; } = new();

    /// <summary>Gets or sets the medium escort quests.</summary>
    [JsonPropertyName("medium_escort")]
    public List<QuestTemplate> MediumEscort { get; set; } = new();

    /// <summary>Gets or sets the hard escort quests.</summary>
    [JsonPropertyName("hard_escort")]
    public List<QuestTemplate> HardEscort { get; set; } = new();
}

/// <summary>
/// Delivery quest templates by difficulty.
/// </summary>
public class DeliveryQuestTemplates
{
    /// <summary>Gets or sets the easy delivery quests.</summary>
    [JsonPropertyName("easy_delivery")]
    public List<QuestTemplate> EasyDelivery { get; set; } = new();

    /// <summary>Gets or sets the medium delivery quests.</summary>
    [JsonPropertyName("medium_delivery")]
    public List<QuestTemplate> MediumDelivery { get; set; } = new();

    /// <summary>Gets or sets the hard delivery quests.</summary>
    [JsonPropertyName("hard_delivery")]
    public List<QuestTemplate> HardDelivery { get; set; } = new();
}

/// <summary>
/// Investigation quest templates by difficulty.
/// </summary>
public class InvestigateQuestTemplates
{
    /// <summary>Gets or sets the easy investigation quests.</summary>
    [JsonPropertyName("easy_investigation")]
    public List<QuestTemplate> EasyInvestigation { get; set; } = new();

    /// <summary>Gets or sets the medium investigation quests.</summary>
    [JsonPropertyName("medium_investigation")]
    public List<QuestTemplate> MediumInvestigation { get; set; } = new();

    /// <summary>Gets or sets the hard investigation quests.</summary>
    [JsonPropertyName("hard_investigation")]
    public List<QuestTemplate> HardInvestigation { get; set; } = new();
}

/// <summary>
/// Template for generating quests with configurable properties.
/// </summary>
public class QuestTemplate
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

    /// <summary>Gets or sets the quest type.</summary>
    [JsonPropertyName("questType")]
    public string QuestType { get; set; } = string.Empty;

    /// <summary>Gets or sets the difficulty.</summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the base gold reward.</summary>
    [JsonPropertyName("baseGoldReward")]
    public int BaseGoldReward { get; set; }

    /// <summary>Gets or sets the base XP reward.</summary>
    [JsonPropertyName("baseXpReward")]
    public int BaseXpReward { get; set; }

    // Optional properties (vary by quest type)
    /// <summary>Gets or sets the item type (for fetch quests).</summary>
    [JsonPropertyName("itemType")]
    public string? ItemType { get; set; }

    /// <summary>Gets or sets the item rarity (for fetch quests).</summary>
    [JsonPropertyName("itemRarity")]
    public string? ItemRarity { get; set; }

    /// <summary>Gets or sets the minimum quantity.</summary>
    [JsonPropertyName("minQuantity")]
    public int? MinQuantity { get; set; }

    /// <summary>Gets or sets the maximum quantity.</summary>
    [JsonPropertyName("maxQuantity")]
    public int? MaxQuantity { get; set; }

    /// <summary>Gets or sets the target type (for kill quests).</summary>
    [JsonPropertyName("targetType")]
    public string? TargetType { get; set; }

    /// <summary>Gets or sets the NPC type (for escort/delivery quests).</summary>
    [JsonPropertyName("npcType")]
    public string? NpcType { get; set; }

    /// <summary>Gets or sets the location.</summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>Gets or sets the distance.</summary>
    [JsonPropertyName("distance")]
    public string? Distance { get; set; }

    /// <summary>Gets or sets the time limit (in minutes).</summary>
    [JsonPropertyName("timeLimit")]
    public int? TimeLimit { get; set; }

    /// <summary>Gets or sets the minimum clues (for investigation quests).</summary>
    [JsonPropertyName("minClues")]
    public int? MinClues { get; set; }

    /// <summary>Gets or sets the maximum clues (for investigation quests).</summary>
    [JsonPropertyName("maxClues")]
    public int? MaxClues { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is fragile (for delivery quests).</summary>
    [JsonPropertyName("itemFragile")]
    public bool? ItemFragile { get; set; }

    /// <summary>Gets or sets a value indicating whether the quest is urgent.</summary>
    [JsonPropertyName("urgent")]
    public bool? Urgent { get; set; }

    /// <summary>Gets or sets a value indicating whether there are guardians.</summary>
    [JsonPropertyName("guardians")]
    public bool? Guardians { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a boss fight.</summary>
    [JsonPropertyName("bossFight")]
    public bool? BossFight { get; set; }

    /// <summary>Gets or sets a value indicating whether the quest is legendary.</summary>
    [JsonPropertyName("legendary")]
    public bool? Legendary { get; set; }

    /// <summary>Gets or sets the stealth required level.</summary>
    [JsonPropertyName("stealthRequired")]
    public int? StealthRequired { get; set; }

    /// <summary>Gets or sets the moral choice description.</summary>
    [JsonPropertyName("moralChoice")]
    public string? MoralChoice { get; set; }

    /// <summary>Gets or sets the failure consequence description.</summary>
    [JsonPropertyName("failureConsequence")]
    public string? FailureConsequence { get; set; }

    /// <summary>Gets or sets the bonus reward description.</summary>
    [JsonPropertyName("bonusReward")]
    public string? BonusReward { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a PvP quest.</summary>
    [JsonPropertyName("pvpQuest")]
    public bool? PvpQuest { get; set; }

    /// <summary>Gets or sets the ambush chance percentage.</summary>
    [JsonPropertyName("ambushChance")]
    public int? AmbushChance { get; set; }

    /// <summary>Gets or sets a value indicating whether the NPC can die (for escort quests).</summary>
    [JsonPropertyName("npcCanDie")]
    public bool? NpcCanDie { get; set; }

    /// <summary>Gets or sets a value indicating whether there are bandits.</summary>
    [JsonPropertyName("bandits")]
    public bool? Bandits { get; set; }

    /// <summary>Gets or sets a value indicating whether the quest is cursed.</summary>
    [JsonPropertyName("cursed")]
    public bool? Cursed { get; set; }

    /// <summary>Gets or sets a value indicating whether the player is pursued.</summary>
    [JsonPropertyName("pursued")]
    public bool? Pursued { get; set; }

    /// <summary>Gets or sets a value indicating whether combat is optional.</summary>
    [JsonPropertyName("combatOptional")]
    public bool? CombatOptional { get; set; }

    /// <summary>Gets or sets a value indicating whether there are multiple endings.</summary>
    [JsonPropertyName("multipleEndings")]
    public bool? MultipleEndings { get; set; }

    /// <summary>Gets or sets the stealth level.</summary>
    [JsonPropertyName("stealth")]
    public int? Stealth { get; set; }

    /// <summary>Gets or sets a value indicating whether there is betrayal.</summary>
    [JsonPropertyName("betrayal")]
    public bool? Betrayal { get; set; }
}

// Quest Locations
/// <summary>
/// Collection of quest locations organized by type.
/// </summary>
public class QuestLocationsCollection
{
    /// <summary>Gets or sets the wilderness locations.</summary>
    [JsonPropertyName("wilderness")]
    public WildernessLocations Wilderness { get; set; } = new();

    /// <summary>Gets or sets the town locations.</summary>
    [JsonPropertyName("towns")]
    public TownLocations Towns { get; set; } = new();

    /// <summary>Gets or sets the dungeon locations.</summary>
    [JsonPropertyName("dungeons")]
    public DungeonLocations Dungeons { get; set; } = new();
}

/// <summary>
/// Wilderness locations by danger level.
/// </summary>
public class WildernessLocations
{
    /// <summary>Gets or sets the low danger locations.</summary>
    [JsonPropertyName("low_danger")]
    public List<QuestLocation> LowDanger { get; set; } = new();

    /// <summary>Gets or sets the medium danger locations.</summary>
    [JsonPropertyName("medium_danger")]
    public List<QuestLocation> MediumDanger { get; set; } = new();

    /// <summary>Gets or sets the high danger locations.</summary>
    [JsonPropertyName("high_danger")]
    public List<QuestLocation> HighDanger { get; set; } = new();

    /// <summary>Gets or sets the very high danger locations.</summary>
    [JsonPropertyName("very_high_danger")]
    public List<QuestLocation> VeryHighDanger { get; set; } = new();
}

/// <summary>
/// Town and settlement locations by size.
/// </summary>
public class TownLocations
{
    /// <summary>Gets or sets the outpost locations.</summary>
    [JsonPropertyName("outposts")]
    public List<QuestLocation> Outposts { get; set; } = new();

    /// <summary>Gets or sets the village locations.</summary>
    [JsonPropertyName("villages")]
    public List<QuestLocation> Villages { get; set; } = new();

    /// <summary>Gets or sets the town locations.</summary>
    [JsonPropertyName("towns")]
    public List<QuestLocation> Towns { get; set; } = new();

    /// <summary>Gets or sets the city locations.</summary>
    [JsonPropertyName("cities")]
    public List<QuestLocation> Cities { get; set; } = new();

    /// <summary>Gets or sets the capital locations.</summary>
    [JsonPropertyName("capitals")]
    public List<QuestLocation> Capitals { get; set; } = new();

    /// <summary>Gets or sets the special locations.</summary>
    [JsonPropertyName("special_locations")]
    public List<QuestLocation> SpecialLocations { get; set; } = new();
}

/// <summary>
/// Dungeon locations by difficulty.
/// </summary>
public class DungeonLocations
{
    /// <summary>Gets or sets the easy dungeons.</summary>
    [JsonPropertyName("easy_dungeons")]
    public List<QuestLocation> EasyDungeons { get; set; } = new();

    /// <summary>Gets or sets the medium dungeons.</summary>
    [JsonPropertyName("medium_dungeons")]
    public List<QuestLocation> MediumDungeons { get; set; } = new();

    /// <summary>Gets or sets the hard dungeons.</summary>
    [JsonPropertyName("hard_dungeons")]
    public List<QuestLocation> HardDungeons { get; set; } = new();

    /// <summary>Gets or sets the very hard dungeons.</summary>
    [JsonPropertyName("very_hard_dungeons")]
    public List<QuestLocation> VeryHardDungeons { get; set; } = new();

    /// <summary>Gets or sets the epic dungeons.</summary>
    [JsonPropertyName("epic_dungeons")]
    public List<QuestLocation> EpicDungeons { get; set; } = new();

    /// <summary>Gets or sets the legendary dungeons.</summary>
    [JsonPropertyName("legendary_dungeons")]
    public List<QuestLocation> LegendaryDungeons { get; set; } = new();
}

/// <summary>
/// A specific location where quests can take place.
/// </summary>
public class QuestLocation
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

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the location type.</summary>
    [JsonPropertyName("locationType")]
    public string LocationType { get; set; } = string.Empty;

    // Wilderness properties
    /// <summary>Gets or sets the terrain type (for wilderness locations).</summary>
    [JsonPropertyName("terrain")]
    public string? Terrain { get; set; }

    /// <summary>Gets or sets the danger level (for wilderness locations).</summary>
    [JsonPropertyName("danger")]
    public string? Danger { get; set; }

    // Settlement properties
    /// <summary>Gets or sets the settlement size (for town locations).</summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }

    /// <summary>Gets or sets the population (for town locations).</summary>
    [JsonPropertyName("population")]
    public string? Population { get; set; }

    // Dungeon properties
    /// <summary>Gets or sets the dungeon type.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>Gets or sets the difficulty (for dungeons).</summary>
    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }

    /// <summary>Gets or sets the enemy types (for dungeons).</summary>
    [JsonPropertyName("enemy_types")]
    public List<string>? EnemyTypes { get; set; }
}
