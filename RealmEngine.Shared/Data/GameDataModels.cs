namespace RealmEngine.Shared.Data.Models;

// Item-related data models

/// <summary>
/// Data models for deserializing JSON game data files.
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
    /// <summary>Gets or sets the value.</summary>
    public string Value { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the rarity weight.</summary>
    public int RarityWeight { get; set; } = 10;
}

/// <summary>
/// Pattern template for weapon name generation
/// </summary>
public class WeaponPattern
{
    /// <summary>Gets or sets the template.</summary>
    public string? Template { get; set; }
    
    /// <summary>Gets or sets the pattern (alternative field name).</summary>
    public string? Pattern { get; set; }
    
    /// <summary>Gets or sets the weight.</summary>
    public int Weight { get; set; } = 10;
    
    /// <summary>Gets or sets the rarity weight (alternative field name).</summary>
    public int RarityWeight { get; set; } = 10;
    
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    
    /// <summary>Gets or sets the example (alternative field name).</summary>
    public string? Example { get; set; }

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
/// <summary>
/// Enemy name generation data.
/// </summary>
public class EnemyNameData
{
    /// <summary>Gets or sets the prefixes.</summary>
    public List<string> Prefixes { get; set; } = new();
    
    /// <summary>Gets or sets the creatures.</summary>
    public List<string> Creatures { get; set; } = new();
    
    /// <summary>Gets or sets the variants.</summary>
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

/// <summary>
/// Dragon name generation data.
/// </summary>
public class DragonNameData
{
    /// <summary>Gets or sets the prefixes.</summary>
    public List<string> Prefixes { get; set; } = new();
    
    /// <summary>Gets or sets the colors.</summary>
    public List<string> Colors { get; set; } = new();
    
    /// <summary>Gets or sets the types.</summary>
    public List<string> Types { get; set; } = new();
    
    /// <summary>Gets or sets the variants.</summary>
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

/// <summary>
/// Humanoid name generation data.
/// </summary>
public class HumanoidNameData
{
    /// <summary>Gets or sets the professions.</summary>
    public List<string> Professions { get; set; } = new();
    
    /// <summary>Gets or sets the roles.</summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>Gets or sets the factions.</summary>
    public List<string> Factions { get; set; } = new();
    
    /// <summary>Gets or sets the variants.</summary>
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

// NPC-related data models
/// <summary>
/// Fantasy name generation data.
/// </summary>
public class FantasyNameData
{
    /// <summary>Gets or sets the male names.</summary>
    public List<string> Male { get; set; } = new();
    
    /// <summary>Gets or sets the female names.</summary>
    public List<string> Female { get; set; } = new();
    
    /// <summary>Gets or sets the surnames.</summary>
    public List<string> Surnames { get; set; } = new();
}

/// <summary>
/// Dialogue template data for NPCs.
/// </summary>
public class DialogueTemplateData
{
    /// <summary>Gets or sets the greetings.</summary>
    public List<string> Greetings { get; set; } = new();
    
    /// <summary>Gets or sets the merchant dialogues.</summary>
    public List<string> Merchants { get; set; } = new();
    
    /// <summary>Gets or sets the quest dialogues.</summary>
    public List<string> Quests { get; set; } = new();
    
    /// <summary>Gets or sets the rumor dialogues.</summary>
    public List<string> Rumors { get; set; } = new();
    
    /// <summary>Gets or sets the farewells.</summary>
    public List<string> Farewells { get; set; } = new();
    
    /// <summary>Gets or sets the hostile dialogues.</summary>
    public List<string> Hostile { get; set; } = new();
    
    /// <summary>Gets or sets the friendly dialogues.</summary>
    public List<string> Friendly { get; set; } = new();
}

// General data models
/// <summary>
/// Adjective data for name generation.
/// </summary>
public class AdjectiveData
{
    /// <summary>Gets or sets the positive adjectives.</summary>
    public List<string> Positive { get; set; } = new();
    
    /// <summary>Gets or sets the negative adjectives.</summary>
    public List<string> Negative { get; set; } = new();
    
    /// <summary>Gets or sets the size adjectives.</summary>
    public List<string> Size { get; set; } = new();
    
    /// <summary>Gets or sets the appearance adjectives.</summary>
    public List<string> Appearance { get; set; } = new();
    
    /// <summary>Gets or sets the condition adjectives.</summary>
    public List<string> Condition { get; set; } = new();
}

/// <summary>
/// Material data for item generation.
/// </summary>
public class MaterialData
{
    /// <summary>Gets or sets the metal materials.</summary>
    public List<string> Metals { get; set; } = new();
    
    /// <summary>Gets or sets the precious materials.</summary>
    public List<string> Precious { get; set; } = new();
    
    /// <summary>Gets or sets the natural materials.</summary>
    public List<string> Natural { get; set; } = new();
    
    /// <summary>Gets or sets the magical materials.</summary>
    public List<string> Magical { get; set; } = new();
}
