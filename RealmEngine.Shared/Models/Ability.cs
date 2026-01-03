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
    /// Gets or sets the base name of the ability without modifiers (e.g., "Fireball", "Shield").
    /// </summary>
    public string? BaseAbilityName { get; set; }
    
    /// <summary>
    /// Gets or sets the power modifier prefix (e.g., "Greater", "Lesser", "Supreme").
    /// </summary>
    public string? PowerPrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the elemental or school modifier (e.g., "Frost", "Holy", "Shadow").
    /// </summary>
    public string? SchoolPrefix { get; set; }

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
    /// Collection of item reference IDs (v4.1 format) required to use this ability.
    /// Each ID is a JSON reference like "@items/consumables/reagents:mana-crystal".
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Validate required items before using ability
    /// var resolver = new ReferenceResolverService(dataCache);
    /// var requiredItems = new List&lt;Item&gt;();
    /// foreach (var refId in ability.RequiredItemIds)
    /// {
    ///     var itemJson = await resolver.ResolveToObjectAsync(refId);
    ///     var item = itemJson.ToObject&lt;Item&gt;();
    ///     requiredItems.Add(item);
    /// }
    /// bool canUse = requiredItems.All(item => character.Inventory.Contains(item.Name));
    /// if (canUse &amp;&amp; character.Mana >= ability.ManaCost)
    /// {
    ///     ExecuteAbility(ability, target);
    /// }
    /// </code>
    /// <code>
    /// // GDScript - Validate requirements in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// var can_use = true
    /// for ref_id in ability.RequiredItemIds:
    ///     var item_data = await resolver.ResolveToObjectAsync(ref_id)
    ///     if not player.inventory.has_item(item_data.name):
    ///         can_use = false
    ///         show_error("Missing: " + item_data.name)
    ///         break
    /// if can_use:
    ///     cast_ability(ability)
    /// </code>
    /// <para><strong>Why reference IDs instead of embedded objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Prevents circular dependency (Ability → Item → Ability)</description></item>
    /// <item><description>Validation at use-time - check if player has items</description></item>
    /// <item><description>Dynamic requirements - items may change based on game state</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example required item reference IDs:
    /// <code>
    /// [
    ///   "@items/consumables/reagents:mana-crystal",
    ///   "@items/weapons/staves:magic-staff"
    /// ]
    /// </code>
    /// </example>
    public List<string> RequiredItemIds { get; set; } = new();

    /// <summary>
    /// Collection of ability reference IDs (v4.1 format) that must be learned first (prerequisites).
    /// Each ID is a JSON reference like "@abilities/active/offensive:basic-attack".
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Validate prerequisites before learning ability
    /// var resolver = new ReferenceResolverService(dataCache);
    /// var prerequisites = new List&lt;Ability&gt;();
    /// foreach (var refId in ability.RequiredAbilityIds)
    /// {
    ///     var abilityJson = await resolver.ResolveToObjectAsync(refId);
    ///     var prereq = abilityJson.ToObject&lt;Ability&gt;();
    ///     prerequisites.Add(prereq);
    /// }
    /// bool meetsRequirements = prerequisites.All(req => 
    ///     character.LearnedSkills.Any(s => s.Name == req.Name));
    /// if (meetsRequirements &amp;&amp; character.Level >= ability.RequiredLevel)
    /// {
    ///     character.LearnedSkills.Add(ability);
    ///     ShowAbilityLearned(ability);
    /// }
    /// </code>
    /// <code>
    /// // GDScript - Validate prerequisites in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// var has_prereqs = true
    /// for ref_id in ability.RequiredAbilityIds:
    ///     var prereq_data = await resolver.ResolveToObjectAsync(ref_id)
    ///     if not player.has_ability(prereq_data.name):
    ///         has_prereqs = false
    ///         show_error("Must learn: " + prereq_data.display_name)
    ///         break
    /// if has_prereqs:
    ///     player.learn_ability(ability)
    /// </code>
    /// <para><strong>Typical prerequisites:</strong></para>
    /// <list type="bullet">
    /// <item><description>Advanced abilities require basic versions first</description></item>
    /// <item><description>Talent tree progression (Tier 1 → Tier 2 → Tier 3)</description></item>
    /// <item><description>Combo abilities requiring mastery of base skills</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example prerequisite reference IDs:
    /// <code>
    /// [
    ///   "@abilities/active/offensive:basic-attack",
    ///   "@abilities/passive/defensive:armor-proficiency"
    /// ]
    /// </code>
    /// </example>
    public List<string> RequiredAbilityIds { get; set; } = new();

    /// <summary>
    /// Fully resolved Item objects required to use this ability.
    /// Populated by AbilityGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in RequiredItemIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property to validate ability can be used</description></item>
    /// <item><description>Already resolved - no need to call ReferenceResolverService</description></item>
    /// <item><description>Null if ability loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Item> RequiredItems { get; set; } = new();

    /// <summary>
    /// Fully resolved Ability objects required before learning this ability (prerequisites).
    /// Populated by AbilityGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in RequiredAbilityIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property to check if player meets prerequisites</description></item>
    /// <item><description>Already resolved - no need to call ReferenceResolverService</description></item>
    /// <item><description>Null if ability loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Ability> RequiredAbilities { get; set; } = new();
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
