using RealmEngine.Shared.Models;
using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

/// <summary>
/// Weapon prefix with display name and traits.
/// </summary>
public class WeaponPrefixTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

/// <summary>
/// Weapon prefix collection by rarity tier.
/// </summary>
public class WeaponPrefixData
{
    /// <summary>Gets or sets the common prefixes.</summary>
    public Dictionary<string, WeaponPrefixTraitData> Common { get; set; } = new();
    
    /// <summary>Gets or sets the uncommon prefixes.</summary>
    public Dictionary<string, WeaponPrefixTraitData> Uncommon { get; set; } = new();
    
    /// <summary>Gets or sets the rare prefixes.</summary>
    public Dictionary<string, WeaponPrefixTraitData> Rare { get; set; } = new();
    
    /// <summary>Gets or sets the epic prefixes.</summary>
    public Dictionary<string, WeaponPrefixTraitData> Epic { get; set; } = new();
    
    /// <summary>Gets or sets the legendary prefixes.</summary>
    public Dictionary<string, WeaponPrefixTraitData> Legendary { get; set; } = new();
}

// Armor material with traits
/// <summary>
/// Armor material with display name and traits.
/// </summary>
public class ArmorMaterialTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

/// <summary>
/// Armor material collection by rarity tier.
/// </summary>
public class ArmorMaterialData
{
    /// <summary>Gets or sets the common materials.</summary>
    public Dictionary<string, ArmorMaterialTraitData> Common { get; set; } = new();
    
    /// <summary>Gets or sets the uncommon materials.</summary>
    public Dictionary<string, ArmorMaterialTraitData> Uncommon { get; set; } = new();
    
    /// <summary>Gets or sets the rare materials.</summary>
    public Dictionary<string, ArmorMaterialTraitData> Rare { get; set; } = new();
    
    /// <summary>Gets or sets the epic materials.</summary>
    public Dictionary<string, ArmorMaterialTraitData> Epic { get; set; } = new();
    
    /// <summary>Gets or sets the legendary materials.</summary>
    public Dictionary<string, ArmorMaterialTraitData> Legendary { get; set; } = new();
}

// Enchantment suffix with traits
/// <summary>
/// Enchantment suffix with display name and traits.
/// </summary>
public class EnchantmentSuffixTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

/// <summary>
/// Enchantment suffix collection by category.
/// </summary>
public class EnchantmentSuffixData
{
    /// <summary>Gets or sets the power suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Power { get; set; } = new();
    
    /// <summary>Gets or sets the protection suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Protection { get; set; } = new();
    
    /// <summary>Gets or sets the wisdom suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Wisdom { get; set; } = new();
    
    /// <summary>Gets or sets the agility suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Agility { get; set; } = new();
    
    /// <summary>Gets or sets the magic suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Magic { get; set; } = new();
    
    /// <summary>Gets or sets the fire suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Fire { get; set; } = new();
    
    /// <summary>Gets or sets the ice suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Ice { get; set; } = new();
    
    /// <summary>Gets or sets the lightning suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Lightning { get; set; } = new();
    
    /// <summary>Gets or sets the life suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Life { get; set; } = new();
    
    /// <summary>Gets or sets the death suffixes.</summary>
    public Dictionary<string, EnchantmentSuffixTraitData> Death { get; set; } = new();
}

// Material property trait data (for metals, leathers, woods, gemstones)
/// <summary>
/// Material property with display name and traits.
/// </summary>
public class MaterialTraitData
{
    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the traits in JSON format.</summary>
    [JsonPropertyName("traits")]
    public Dictionary<string, JsonTraitValue> JsonTraits { get; set; } = new();

    /// <summary>
    /// Convert JSON traits to TraitValue dictionary.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, TraitValue> Traits
    {
        get
        {
            var result = new Dictionary<string, TraitValue>();
            foreach (var kvp in JsonTraits)
            {
                result[kvp.Key] = kvp.Value.ToTraitValue();
            }
            return result;
        }
    }
}

// Material data collections (flat dictionaries, not tiered)
/// <summary>
/// Metal material collection.
/// </summary>
public class MetalData : Dictionary<string, MaterialTraitData> { }

/// <summary>
/// Leather material collection.
/// </summary>
public class LeatherData : Dictionary<string, MaterialTraitData> { }

/// <summary>
/// Wood material collection.
/// </summary>
public class WoodData : Dictionary<string, MaterialTraitData> { }

/// <summary>
/// Gemstone material collection.
/// </summary>
public class GemstoneData : Dictionary<string, MaterialTraitData> { }
