using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

// Data models for NPC catalog system (backgrounds, occupations, traits, names).
// These models map to the new v4.0 JSON structure.

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

/// <summary>
/// Root data model for NPC traits (personality traits and quirks).
/// </summary>
public class NpcTraitsData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public TraitsMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the personality traits by category.</summary>
    [JsonPropertyName("personality_traits")]
    public Dictionary<string, List<TraitItem>> PersonalityTraits { get; set; } = new();

    /// <summary>Gets or sets the quirks by category.</summary>
    [JsonPropertyName("quirks")]
    public Dictionary<string, List<QuirkItem>> Quirks { get; set; } = new();
}

/// <summary>
/// Metadata for NPC traits data.
/// </summary>
public class TraitsMetadata
{
    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the total traits.</summary>
    [JsonPropertyName("totalTraits")]
    public int TotalTraits { get; set; }

    /// <summary>Gets or sets the total quirks.</summary>
    [JsonPropertyName("totalQuirks")]
    public int TotalQuirks { get; set; }
}

/// <summary>
/// Personality trait definition.
/// </summary>
public class TraitItem
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

    /// <summary>Gets or sets the shop modifiers.</summary>
    [JsonPropertyName("shopModifiers")]
    public TraitShopModifiers? ShopModifiers { get; set; }
}

/// <summary>
/// Shop modifiers for personality traits.
/// </summary>
public class TraitShopModifiers
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

    /// <summary>Gets or sets the reputation required.</summary>
    [JsonPropertyName("reputationRequired")]
    public int? ReputationRequired { get; set; }

    /// <summary>Gets or sets the dialogue style.</summary>
    [JsonPropertyName("dialogueStyle")]
    public string? DialogueStyle { get; set; }
}

/// <summary>
/// Quirk definition for NPCs.
/// </summary>
public class QuirkItem
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
}

#endregion

#region Names Models

/// <summary>
/// Root data model for NPC names (components and patterns).
/// </summary>
public class NpcNamesData
{
    /// <summary>Gets or sets the metadata.</summary>
    [JsonPropertyName("metadata")]
    public NamesMetadata Metadata { get; set; } = new();

    /// <summary>Gets or sets the name components.</summary>
    [JsonPropertyName("components")]
    public NameComponents Components { get; set; } = new();

    /// <summary>Gets or sets the name patterns.</summary>
    [JsonPropertyName("patterns")]
    public List<NamePattern> Patterns { get; set; } = new();
}

/// <summary>
/// Metadata for NPC names data.
/// </summary>
public class NamesMetadata
{
    /// <summary>Gets or sets the version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Components for generating NPC names.
/// </summary>
public class NameComponents
{
    /// <summary>Gets or sets the title components.</summary>
    [JsonPropertyName("title")]
    public List<TitleComponent>? Title { get; set; }

    /// <summary>Gets or sets the first name components.</summary>
    [JsonPropertyName("first_name")]
    public FirstNameComponents? FirstName { get; set; }

    /// <summary>Gets or sets the surname components by social class.</summary>
    [JsonPropertyName("surname")]
    public Dictionary<string, List<NameComponent>>? Surname { get; set; }

    /// <summary>Gets or sets the suffix components.</summary>
    [JsonPropertyName("suffix")]
    public List<NameComponent>? Suffix { get; set; }
}

/// <summary>
/// Title component with social class preferences.
/// </summary>
public class TitleComponent
{
    /// <summary>Gets or sets the value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the gender.</summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>Gets or sets the preferred social class.</summary>
    [JsonPropertyName("preferredSocialClass")]
    public string? PreferredSocialClass { get; set; }

    /// <summary>Gets or sets the weight multiplier by social class.</summary>
    [JsonPropertyName("weightMultiplier")]
    public Dictionary<string, double>? WeightMultiplier { get; set; }
}

/// <summary>
/// First name components by gender and social class.
/// </summary>
public class FirstNameComponents
{
    /// <summary>Gets or sets the male first names by social class.</summary>
    [JsonPropertyName("male")]
    public Dictionary<string, List<NameComponent>>? Male { get; set; }

    /// <summary>Gets or sets the female first names by social class.</summary>
    [JsonPropertyName("female")]
    public Dictionary<string, List<NameComponent>>? Female { get; set; }
}

/// <summary>
/// Basic name component with value and rarity weight.
/// </summary>
public class NameComponent
{
    /// <summary>Gets or sets the value.</summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }
}

/// <summary>
/// Pattern template for generating NPC names.
/// </summary>
public class NamePattern
{
    /// <summary>Gets or sets the template.</summary>
    [JsonPropertyName("template")]
    public string Template { get; set; } = string.Empty;

    /// <summary>Gets or sets the rarity weight.</summary>
    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    /// <summary>Gets or sets the social class filter.</summary>
    [JsonPropertyName("socialClass")]
    public List<string>? SocialClass { get; set; }

    /// <summary>Gets or sets the description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether a title is required.</summary>
    [JsonPropertyName("requiresTitle")]
    public bool? RequiresTitle { get; set; }

    /// <summary>Gets or sets a value indicating whether titles are excluded.</summary>
    [JsonPropertyName("excludeTitles")]
    public bool? ExcludeTitles { get; set; }
}

#endregion
