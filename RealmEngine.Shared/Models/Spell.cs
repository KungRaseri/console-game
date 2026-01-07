using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Models;

/// <summary>
/// Defines a spell's properties and requirements.
/// Maps to spells/catalog.json structure in JSON v4.2.
/// </summary>
public class Spell
{
    /// <summary>
    /// Unique identifier matching JSON catalog (e.g., "fireball", "heal", "force-missile").
    /// </summary>
    public required string SpellId { get; set; }
    
    /// <summary>
    /// Internal name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full description of spell effects.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Magical tradition (Arcane, Divine, Occult, Primal).
    /// </summary>
    public MagicalTradition Tradition { get; set; }
    
    /// <summary>
    /// Spell rank: 0 (Cantrip) through 10.
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// Minimum tradition skill rank required to cast effectively (0-100).
    /// Characters can attempt spells below this rank but with reduced success rate.
    /// </summary>
    public int MinimumSkillRank { get; set; }
    
    /// <summary>
    /// Base mana cost before skill modifiers (0 for cantrips).
    /// </summary>
    public int ManaCost { get; set; }
    
    /// <summary>
    /// Spell effect type (Damage, Heal, Buff, Summon, Control, Utility).
    /// </summary>
    public SpellEffectType EffectType { get; set; }
    
    /// <summary>
    /// Base effect value (damage dice, healing amount, etc.).
    /// Examples: "8d6", "1d8+WIS", "2d10+5"
    /// </summary>
    public string BaseEffectValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Range in feet/meters (null or 0 for self/touch).
    /// </summary>
    public int Range { get; set; }
    
    /// <summary>
    /// Area of effect radius in feet (0 for single target).
    /// </summary>
    public int AreaOfEffect { get; set; } = 0;
    
    /// <summary>
    /// Duration in turns (0 for instant effects).
    /// </summary>
    public int Duration { get; set; } = 0;
    
    /// <summary>
    /// Cooldown in turns before spell can be cast again (0 for no cooldown).
    /// </summary>
    public int Cooldown { get; set; } = 0;
    
    /// <summary>
    /// Selection weight for spellbook/scroll generation (1-1000).
    /// Lower = more common, higher = more rare.
    /// </summary>
    public int SelectionWeight { get; set; } = 50;
    
    /// <summary>
    /// Traits/properties specific to this spell (damage type, saving throw, etc.).
    /// </summary>
    public Dictionary<string, object> Traits { get; set; } = new();
}

/// <summary>
/// Magical tradition categories based on Pathfinder 2e.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MagicalTradition
{
    /// <summary>
    /// INT-based: Force, transmutation, teleportation, raw magical power.
    /// </summary>
    Arcane,
    
    /// <summary>
    /// WIS-based: Healing, holy power, protection, faith magic.
    /// </summary>
    Divine,
    
    /// <summary>
    /// CHA-based: Mind control, illusion, psychic, shadow magic.
    /// </summary>
    Occult,
    
    /// <summary>
    /// WIS-based: Elements, beasts, nature, weather.
    /// </summary>
    Primal
}

/// <summary>
/// Spell effect type categories.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpellEffectType
{
    /// <summary>
    /// Deals damage to targets.
    /// </summary>
    Damage,
    
    /// <summary>
    /// Restores health or removes negative conditions.
    /// </summary>
    Heal,
    
    /// <summary>
    /// Provides positive bonuses to stats or abilities.
    /// </summary>
    Buff,
    
    /// <summary>
    /// Applies negative effects or reduces effectiveness.
    /// </summary>
    Debuff,
    
    /// <summary>
    /// Summons creatures or creates objects.
    /// </summary>
    Summon,
    
    /// <summary>
    /// Restricts movement or actions.
    /// </summary>
    Control,
    
    /// <summary>
    /// Non-combat or situational effects.
    /// </summary>
    Utility,
    
    /// <summary>
    /// Protective barriers or shields.
    /// </summary>
    Protection
}
