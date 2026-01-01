using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Root catalog data for classes/catalog.json (v4.1 format).
/// </summary>
public class ClassCatalogData
{
    [JsonPropertyName("metadata")]
    public CatalogMetadata Metadata { get; set; } = new();

    [JsonPropertyName("class_types")]
    public Dictionary<string, ClassTypeCategory> ClassTypes { get; set; } = new();
}

/// <summary>
/// Category of classes (warrior, mage, rogue, etc.) with metadata and items.
/// </summary>
public class ClassTypeCategory
{
    [JsonPropertyName("metadata")]
    public ClassCategoryMetadata? Metadata { get; set; }

    [JsonPropertyName("items")]
    public List<ClassItemData> Items { get; set; } = new();
}

/// <summary>
/// Metadata for a class category.
/// </summary>
public class ClassCategoryMetadata
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("primaryStats")]
    public List<string> PrimaryStats { get; set; } = new();

    [JsonPropertyName("armorProficiency")]
    public List<string> ArmorProficiency { get; set; } = new();
}

/// <summary>
/// Individual class data from JSON.
/// </summary>
public class ClassItemData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("rarityWeight")]
    public int RarityWeight { get; set; } = 1;

    [JsonPropertyName("isSubclass")]
    public bool IsSubclass { get; set; } = false;

    [JsonPropertyName("parentClass")]
    public string? ParentClass { get; set; }

    [JsonPropertyName("startingStats")]
    public ClassStartingStats? StartingStats { get; set; }

    [JsonPropertyName("startingAbilities")]
    public List<string>? StartingAbilities { get; set; }

    [JsonPropertyName("traits")]
    public Dictionary<string, object>? Traits { get; set; }

    [JsonPropertyName("progression")]
    public Dictionary<string, object>? Progression { get; set; }
}

/// <summary>
/// Starting stats from JSON.
/// </summary>
public class ClassStartingStats
{
    [JsonPropertyName("health")]
    public int Health { get; set; } = 100;

    [JsonPropertyName("mana")]
    public int Mana { get; set; } = 50;

    [JsonPropertyName("strength")]
    public int Strength { get; set; } = 10;

    [JsonPropertyName("dexterity")]
    public int Dexterity { get; set; } = 10;

    [JsonPropertyName("constitution")]
    public int Constitution { get; set; } = 10;

    [JsonPropertyName("intelligence")]
    public int Intelligence { get; set; } = 10;

    [JsonPropertyName("wisdom")]
    public int Wisdom { get; set; } = 10;

    [JsonPropertyName("charisma")]
    public int Charisma { get; set; } = 10;
}
