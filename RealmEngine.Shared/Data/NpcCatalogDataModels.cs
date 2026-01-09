using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

/// <summary>
/// Data models for NPC catalog system (backgrounds, occupations, traits, names).
/// These models map to the new v4.0 JSON structure.
/// </summary>

#region Catalog Models

/// <summary>
/// Root data model for NPC catalog (backgrounds and occupations).
/// </summary>
public class NpcCatalogData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public CatalogMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the backgrounds by social class.</summary>
    [JsonPropertyName("backgrounds")]
    public Dictionary<string, List<BackgroundItem>> Backgrounds { get; set; } = new();

    /// <summary>Gets or sets the occupations by social class.</summary>
    [JsonPropertyName("occupations")]
    public Dictionary<string, List<OccupationItem>> Occupations { get; set; } = new();
}

/// <summary>
/// Metadata for NPC catalog.
/// </summary>
public class CatalogMetadata
{
    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the total backgrounds.</summary>
    [JsonPropertyName("totalBackgrounds")]
    public int TotalBackgrounds { get; set; }

    /// <summary>Gets or sets the total occupations.</summary>
    [JsonPropertyName("totalOccupations")]
    public int TotalOccupations { get; set; }
}

/// <summary>
/// NPC background definition.
/// </summary>
public class BackgroundItem
{
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the social class.</summary>
    [JsonPropertyName("socialClass")]
    public string SocialClass { get; set; } = string.Empty;

    /// <summary>Gets or sets the starting gold.</summary>
    [JsonPropertyName("startingGold")]
    public int StartingGold { get; set; }

    /// <summary>Gets or sets the shop modifiers.</summary>
    [JsonPropertyName("shopModifiers")]
    public ShopModifiers? ShopModifiers { get; set; }
}

/// <summary>
/// NPC occupation definition.
/// </summary>
public class OccupationItem
{
    /// <summary>Gets or sets the name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the social class.</summary>
    [JsonPropertyName("socialClass")]
    public string SocialClass { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this is a merchant occupation.</summary>
    [JsonPropertyName("isMerchant")]
    public bool IsMerchant { get; set; }

    /// <summary>Gets or sets the shop configuration.</summary>
    [JsonPropertyName("shop")]
    public ShopConfig? Shop { get; set; }

    /// <summary>Gets or sets the bank gold formula.</summary>
    [JsonPropertyName("bankGoldFormula")]
    public BankGoldFormula? BankGoldFormula { get; set; }
}

/// <summary>
/// Configuration for merchant shops.
/// </summary>
public class ShopConfig
{
    /// <summary>Gets or sets the inventory type.</summary>
    [JsonPropertyName("inventoryType")]
    public string InventoryType { get; set; } = string.Empty;

    /// <summary>Gets or sets the core items.</summary>
    [JsonPropertyName("coreItems")]
    public List<CoreItemConfig>? CoreItems { get; set; }

    /// <summary>Gets or sets the dynamic categories.</summary>
    [JsonPropertyName("dynamicCategories")]
    public List<string>? DynamicCategories { get; set; }

    /// <summary>Gets or sets the dynamic count configuration.</summary>
    [JsonPropertyName("dynamicCount")]
    public DynamicCountConfig? DynamicCount { get; set; }

    /// <summary>Gets or sets the refresh schedule.</summary>
    [JsonPropertyName("refreshSchedule")]
    public string RefreshSchedule { get; set; } = string.Empty;

    /// <summary>Gets or sets the player trading policy.</summary>
    [JsonPropertyName("playerTradingPolicy")]
    public PlayerTradingPolicy? PlayerTradingPolicy { get; set; }
}

/// <summary>
/// Configuration for core shop items.
/// </summary>
public class CoreItemConfig
{
    /// <summary>Gets or sets the category.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the subcategory.</summary>
    [JsonPropertyName("subcategory")]
    public string? Subcategory { get; set; }

    /// <summary>Gets or sets the minimum quantity.</summary>
    [JsonPropertyName("minQuantity")]
    public int MinQuantity { get; set; }

    /// <summary>Gets or sets the maximum quantity.</summary>
    [JsonPropertyName("maxQuantity")]
    public int MaxQuantity { get; set; }
}

/// <summary>
/// Configuration for dynamic item counts by rarity.
/// </summary>
public class DynamicCountConfig
{
    /// <summary>Gets or sets the common item count.</summary>
    [JsonPropertyName("common")]
    public int Common { get; set; }

    /// <summary>Gets or sets the uncommon item count.</summary>
    [JsonPropertyName("uncommon")]
    public int Uncommon { get; set; }

    /// <summary>Gets or sets the rare item count.</summary>
    [JsonPropertyName("rare")]
    public int Rare { get; set; }
}

/// <summary>
/// Policy for player item trading.
/// </summary>
public class PlayerTradingPolicy
{
    /// <summary>Gets or sets a value indicating whether the merchant accepts player items.</summary>
    [JsonPropertyName("acceptsPlayerItems")]
    public bool AcceptsPlayerItems { get; set; }

    /// <summary>Gets or sets the buy price percentage.</summary>
    [JsonPropertyName("buyPricePercentage")]
    public int BuyPricePercentage { get; set; }

    /// <summary>Gets or sets the resell price percentage.</summary>
    [JsonPropertyName("resellPricePercentage")]
    public int ResellPricePercentage { get; set; }

    /// <summary>Gets or sets the decay days.</summary>
    [JsonPropertyName("decayDays")]
    public int DecayDays { get; set; }

    /// <summary>Gets or sets the decay rate per day.</summary>
    [JsonPropertyName("decayRatePerDay")]
    public int DecayRatePerDay { get; set; }

    /// <summary>Gets or sets a value indicating whether the merchant retains item properties.</summary>
    [JsonPropertyName("retainsItemProperties")]
    public bool RetainsItemProperties { get; set; }
}

/// <summary>
/// Modifiers for shop prices and behavior.
/// </summary>
public class ShopModifiers
{
    /// <summary>Gets or sets the price multiplier.</summary>
    [JsonPropertyName("priceMultiplier")]
    public double? PriceMultiplier { get; set; }

    /// <summary>Gets or sets the buy price multiplier.</summary>
    [JsonPropertyName("buyPriceMultiplier")]
    public double? BuyPriceMultiplier { get; set; }

    /// <summary>Gets or sets the quality bonus.</summary>
    [JsonPropertyName("qualityBonus")]
    public int? QualityBonus { get; set; }

    /// <summary>Gets or sets the reputation discount.</summary>
    [JsonPropertyName("reputationDiscount")]
    public int? ReputationDiscount { get; set; }
}

/// <summary>
/// Formula for calculating NPC bank gold.
/// </summary>
public class BankGoldFormula
{
    /// <summary>Gets or sets the base multiplier.</summary>
    [JsonPropertyName("baseMultiplier")]
    public int BaseMultiplier { get; set; }

    /// <summary>Gets or sets the background multiplier.</summary>
    [JsonPropertyName("backgroundMultiplier")]
    public int BackgroundMultiplier { get; set; }

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

#endregion

#region Traits Models

public class NpcTraitsData
{
    [JsonPropertyName("metadata")]
    public TraitsMetadata Metadata { get; set; } = new();

    [JsonPropertyName("personality_traits")]
    public Dictionary<string, List<TraitItem>> PersonalityTraits { get; set; } = new();

    [JsonPropertyName("quirks")]
    public Dictionary<string, List<QuirkItem>> Quirks { get; set; } = new();
}

public class TraitsMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("totalTraits")]
    public int TotalTraits { get; set; }

    [JsonPropertyName("totalQuirks")]
    public int TotalQuirks { get; set; }
}

public class TraitItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("shopModifiers")]
    public TraitShopModifiers? ShopModifiers { get; set; }
}

public class TraitShopModifiers
{
    [JsonPropertyName("priceMultiplier")]
    public double? PriceMultiplier { get; set; }

    [JsonPropertyName("buyPriceMultiplier")]
    public double? BuyPriceMultiplier { get; set; }

    [JsonPropertyName("qualityBonus")]
    public int? QualityBonus { get; set; }

    [JsonPropertyName("reputationRequired")]
    public int? ReputationRequired { get; set; }

    [JsonPropertyName("dialogueStyle")]
    public string? DialogueStyle { get; set; }
}

public class QuirkItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }
}

#endregion

#region Names Models

public class NpcNamesData
{
    [JsonPropertyName("metadata")]
    public NamesMetadata Metadata { get; set; } = new();

    [JsonPropertyName("components")]
    public NameComponents Components { get; set; } = new();

    [JsonPropertyName("patterns")]
    public List<NamePattern> Patterns { get; set; } = new();
}

public class NamesMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class NameComponents
{
    [JsonPropertyName("title")]
    public List<TitleComponent>? Title { get; set; }

    [JsonPropertyName("first_name")]
    public FirstNameComponents? FirstName { get; set; }

    [JsonPropertyName("surname")]
    public Dictionary<string, List<NameComponent>>? Surname { get; set; }

    [JsonPropertyName("suffix")]
    public List<NameComponent>? Suffix { get; set; }
}

public class TitleComponent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("preferredSocialClass")]
    public string? PreferredSocialClass { get; set; }

    [JsonPropertyName("weightMultiplier")]
    public Dictionary<string, double>? WeightMultiplier { get; set; }
}

public class FirstNameComponents
{
    [JsonPropertyName("male")]
    public Dictionary<string, List<NameComponent>>? Male { get; set; }

    [JsonPropertyName("female")]
    public Dictionary<string, List<NameComponent>>? Female { get; set; }
}

public class NameComponent
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }
}

public class NamePattern
{
    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("socialClass")]
    public List<string>? SocialClass { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("requiresTitle")]
    public bool? RequiresTitle { get; set; }

    [JsonPropertyName("excludeTitles")]
    public bool? ExcludeTitles { get; set; }
}

#endregion
