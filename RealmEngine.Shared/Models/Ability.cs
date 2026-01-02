using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Models;

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

    /// <summary>
    /// Collection of item IDs required to use this ability (spell components, weapons, etc.).
    /// These are resolved from @items JSON references when checking if ability can be used.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Check if player has required items to use ability
    /// var requiredItems = await itemRepository.GetByIdsAsync(ability.RequiredItemIds);
    /// bool canUse = requiredItems.All(item => character.Inventory.Contains(item));
    /// if (canUse && character.Mana >= ability.ManaCost)
    /// {
    ///     ExecuteAbility(ability, target);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Validate ability requirements
    /// var can_use = true
    /// for item_id in ability.RequiredItemIds:
    ///     if not player.inventory.has_item(item_id):
    ///         can_use = false
    ///         show_error("Missing required item: " + item_id)
    ///         break
    /// if can_use:
    ///     cast_ability(ability)
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Prevents circular dependency (Ability → Item → Ability)</description></item>
    /// <item><description>Validation at use-time - check if player has items</description></item>
    /// <item><description>Dynamic requirements - items may change based on game state</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@items/consumables/reagents:mana-crystal", "@items/weapons/staves:magic-staff"]
    /// </example>
    public List<string> RequiredItemIds { get; set; } = new();
    
    /// <summary>
    /// Collection of ability IDs that must be learned before this ability becomes available (prerequisites).
    /// These are resolved from @abilities JSON references when checking if ability can be learned.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Check if player meets prerequisites to learn ability
    /// var prerequisites = await abilityRepository.GetByIdsAsync(ability.RequiredAbilityIds);
    /// bool meetsRequirements = prerequisites.All(req => character.LearnedSkills.Any(s => s.Id == req.Id));
    /// if (meetsRequirements && character.Level >= ability.RequiredLevel)
    /// {
    ///     character.LearnedSkills.Add(ability);
    ///     ShowAbilityLearned(ability);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Validate prerequisite abilities
    /// var has_prerequisites = true
    /// for prereq_id in ability.RequiredAbilityIds:
    ///     if not player.has_ability(prereq_id):
    ///         has_prerequisites = false
    ///         show_error("Must learn prerequisite: " + prereq_id)
    ///         break
    /// if has_prerequisites:
    ///     player.learn_ability(ability)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["basic-attack", "power-strike"] (learn basic attacks before advanced ones)
    /// </example>
    public List<string> RequiredAbilityIds { get; set; } = new();
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
