namespace Game.Shared.Data.Models;

/// <summary>
/// Data models for deserializing JSON game data files.
/// </summary>

// Item-related data models
/// <summary>
/// Weapon names data using v4 pattern-based structure.
/// Includes components and patterns for procedural name generation with reference support.
/// </summary>
public class WeaponNameData
{
    /// <summary>
    /// Components for pattern-based name generation (e.g., "base", "prefix", "suffix")
    /// Key = component group name, Value = list of component values with weights
    /// </summary>
    public Dictionary<string, List<WeaponComponent>> Components { get; set; } = new();

    /// <summary>
    /// Patterns for generating weapon names (e.g., "@materialRef/weapon + {base}")
    /// </summary>
    public List<WeaponPattern> Patterns { get; set; } = new();
}

/// <summary>
/// Component value for pattern-based generation
/// </summary>
public class WeaponComponent
{
    public string Value { get; set; } = string.Empty;
    public int RarityWeight { get; set; } = 10;
}

/// <summary>
/// Pattern template for weapon name generation
/// </summary>
public class WeaponPattern
{
    public string? Template { get; set; }
    public string? Pattern { get; set; } // Alternative field name
    public int Weight { get; set; } = 10;
    public int RarityWeight { get; set; } = 10; // Alternative field name
    public string? Description { get; set; }
    public string? Example { get; set; } // Alternative field name

    /// <summary>
    /// Get the pattern template (handles both field names)
    /// </summary>
    public string GetTemplate() => Template ?? Pattern ?? string.Empty;

    /// <summary>
    /// Get the weight (handles both field names)
    /// </summary>
    public int GetWeight() => Weight > 0 ? Weight : RarityWeight;
}

// Enemy-related data models
public class EnemyNameData
{
    public List<string> Prefixes { get; set; } = new();
    public List<string> Creatures { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

public class DragonNameData
{
    public List<string> Prefixes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

public class HumanoidNameData
{
    public List<string> Professions { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Factions { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

// NPC-related data models
public class FantasyNameData
{
    public List<string> Male { get; set; } = new();
    public List<string> Female { get; set; } = new();
    public List<string> Surnames { get; set; } = new();
}

public class DialogueTemplateData
{
    public List<string> Greetings { get; set; } = new();
    public List<string> Merchants { get; set; } = new();
    public List<string> Quests { get; set; } = new();
    public List<string> Rumors { get; set; } = new();
    public List<string> Farewells { get; set; } = new();
    public List<string> Hostile { get; set; } = new();
    public List<string> Friendly { get; set; } = new();
}

// General data models
public class AdjectiveData
{
    public List<string> Positive { get; set; } = new();
    public List<string> Negative { get; set; } = new();
    public List<string> Size { get; set; } = new();
    public List<string> Appearance { get; set; } = new();
    public List<string> Condition { get; set; } = new();
}

public class MaterialData
{
    public List<string> Metals { get; set; } = new();
    public List<string> Precious { get; set; } = new();
    public List<string> Natural { get; set; } = new();
    public List<string> Magical { get; set; } = new();
}
