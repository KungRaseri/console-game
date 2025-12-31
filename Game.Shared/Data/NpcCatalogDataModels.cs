using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Data models for NPC catalog system (backgrounds, occupations, traits, names).
/// These models map to the new v4.0 JSON structure.
/// </summary>

#region Catalog Models

public class NpcCatalogData
{
    [JsonPropertyName("metadata")]
    public CatalogMetadata Metadata { get; set; } = new();

    [JsonPropertyName("backgrounds")]
    public Dictionary<string, List<BackgroundItem>> Backgrounds { get; set; } = new();

    [JsonPropertyName("occupations")]
    public Dictionary<string, List<OccupationItem>> Occupations { get; set; } = new();
}

public class CatalogMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("totalBackgrounds")]
    public int TotalBackgrounds { get; set; }

    [JsonPropertyName("totalOccupations")]
    public int TotalOccupations { get; set; }
}

public class BackgroundItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("socialClass")]
    public string SocialClass { get; set; } = string.Empty;

    [JsonPropertyName("startingGold")]
    public int StartingGold { get; set; }

    [JsonPropertyName("shopModifiers")]
    public ShopModifiers? ShopModifiers { get; set; }
}

public class OccupationItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; }

    [JsonPropertyName("socialClass")]
    public string SocialClass { get; set; } = string.Empty;

    [JsonPropertyName("isMerchant")]
    public bool IsMerchant { get; set; }

    [JsonPropertyName("shop")]
    public ShopConfig? Shop { get; set; }

    [JsonPropertyName("bankGoldFormula")]
    public BankGoldFormula? BankGoldFormula { get; set; }
}

public class ShopConfig
{
    [JsonPropertyName("inventoryType")]
    public string InventoryType { get; set; } = string.Empty;

    [JsonPropertyName("coreItems")]
    public List<CoreItemConfig>? CoreItems { get; set; }

    [JsonPropertyName("dynamicCategories")]
    public List<string>? DynamicCategories { get; set; }

    [JsonPropertyName("dynamicCount")]
    public DynamicCountConfig? DynamicCount { get; set; }

    [JsonPropertyName("refreshSchedule")]
    public string RefreshSchedule { get; set; } = string.Empty;

    [JsonPropertyName("playerTradingPolicy")]
    public PlayerTradingPolicy? PlayerTradingPolicy { get; set; }
}

public class CoreItemConfig
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("subcategory")]
    public string? Subcategory { get; set; }

    [JsonPropertyName("minQuantity")]
    public int MinQuantity { get; set; }

    [JsonPropertyName("maxQuantity")]
    public int MaxQuantity { get; set; }
}

public class DynamicCountConfig
{
    [JsonPropertyName("common")]
    public int Common { get; set; }

    [JsonPropertyName("uncommon")]
    public int Uncommon { get; set; }

    [JsonPropertyName("rare")]
    public int Rare { get; set; }
}

public class PlayerTradingPolicy
{
    [JsonPropertyName("acceptsPlayerItems")]
    public bool AcceptsPlayerItems { get; set; }

    [JsonPropertyName("buyPricePercentage")]
    public int BuyPricePercentage { get; set; }

    [JsonPropertyName("resellPricePercentage")]
    public int ResellPricePercentage { get; set; }

    [JsonPropertyName("decayDays")]
    public int DecayDays { get; set; }

    [JsonPropertyName("decayRatePerDay")]
    public int DecayRatePerDay { get; set; }

    [JsonPropertyName("retainsItemProperties")]
    public bool RetainsItemProperties { get; set; }
}

public class ShopModifiers
{
    [JsonPropertyName("priceMultiplier")]
    public double? PriceMultiplier { get; set; }

    [JsonPropertyName("buyPriceMultiplier")]
    public double? BuyPriceMultiplier { get; set; }

    [JsonPropertyName("qualityBonus")]
    public int? QualityBonus { get; set; }

    [JsonPropertyName("reputationDiscount")]
    public int? ReputationDiscount { get; set; }
}

public class BankGoldFormula
{
    [JsonPropertyName("baseMultiplier")]
    public int BaseMultiplier { get; set; }

    [JsonPropertyName("backgroundMultiplier")]
    public int BackgroundMultiplier { get; set; }

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
