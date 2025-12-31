using System.Text.Json.Serialization;

namespace Game.Shared.Models;

/// <summary>
/// Represents a character ability (offensive, defensive, support, passive, etc.).
/// Maps to v4.1 JSON ability catalog structure.
/// </summary>
public class Ability
{
    /// <summary>
    /// Unique identifier for this ability (kebab-case, e.g., "basic-attack").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Internal name used in references (kebab-case).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown to players.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the ability does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Rarity weight for procedural generation (lower = more common).
    /// </summary>
    public int RarityWeight { get; set; } = 1;

    /// <summary>
    /// Base damage dice (e.g., "2d6", "4d8+2").
    /// </summary>
    public string? BaseDamage { get; set; }

    /// <summary>
    /// Cooldown in turns/seconds.
    /// </summary>
    public int Cooldown { get; set; } = 0;

    /// <summary>
    /// Range in feet/meters (null for melee/self).
    /// </summary>
    public int? Range { get; set; }

    /// <summary>
    /// Mana/resource cost to use this ability.
    /// </summary>
    public int ManaCost { get; set; } = 0;

    /// <summary>
    /// Duration in turns/seconds (null for instant).
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Ability type/category (offensive, defensive, support, passive, utility, etc.).
    /// </summary>
    public AbilityTypeEnum Type { get; set; } = AbilityTypeEnum.Offensive;

    /// <summary>
    /// Traits/properties specific to this ability (damage type, effects, etc.).
    /// Values may be strings, numbers, or resolved references.
    /// </summary>
    public Dictionary<string, object> Traits { get; set; } = new();

    /// <summary>
    /// Whether this is a passive ability (always active).
    /// </summary>
    public bool IsPassive { get; set; } = false;

    /// <summary>
    /// Level requirement to unlock this ability.
    /// </summary>
    public int RequiredLevel { get; set; } = 1;

    /// <summary>
    /// Class restrictions (empty = available to all classes).
    /// </summary>
    public List<string> AllowedClasses { get; set; } = new();
}

/// <summary>
/// Ability type categories matching v4.1 JSON structure.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AbilityTypeEnum
{
    Offensive,
    Defensive,
    Support,
    Passive,
    Utility,
    Mobility,
    Summon,
    Transformation,
    Buff,
    Debuff,
    Healing,
    Crowd_Control
}
