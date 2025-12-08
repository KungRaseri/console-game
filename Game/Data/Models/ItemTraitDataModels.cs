using Game.Models;
using System.Text.Json.Serialization;

namespace Game.Data.Models;

/// <summary>
/// Data models for item traits from JSON.
/// </summary>

// Weapon prefix with traits
public class WeaponPrefixTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
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

public class WeaponPrefixData
{
    public Dictionary<string, WeaponPrefixTraitData> Common { get; set; } = new();
    public Dictionary<string, WeaponPrefixTraitData> Uncommon { get; set; } = new();
    public Dictionary<string, WeaponPrefixTraitData> Rare { get; set; } = new();
    public Dictionary<string, WeaponPrefixTraitData> Epic { get; set; } = new();
    public Dictionary<string, WeaponPrefixTraitData> Legendary { get; set; } = new();
}

// Armor material with traits
public class ArmorMaterialTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
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

public class ArmorMaterialData
{
    public Dictionary<string, ArmorMaterialTraitData> Common { get; set; } = new();
    public Dictionary<string, ArmorMaterialTraitData> Uncommon { get; set; } = new();
    public Dictionary<string, ArmorMaterialTraitData> Rare { get; set; } = new();
    public Dictionary<string, ArmorMaterialTraitData> Epic { get; set; } = new();
    public Dictionary<string, ArmorMaterialTraitData> Legendary { get; set; } = new();
}

// Enchantment suffix with traits
public class EnchantmentSuffixTraitData
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
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

public class EnchantmentSuffixData
{
    public Dictionary<string, EnchantmentSuffixTraitData> Power { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Protection { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Wisdom { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Agility { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Magic { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Fire { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Ice { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Lightning { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Life { get; set; } = new();
    public Dictionary<string, EnchantmentSuffixTraitData> Death { get; set; } = new();
}
