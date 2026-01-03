namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an NPC (Non-Player Character) in the game.
/// </summary>
public class NPC : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this NPC.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the display name of the NPC.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base name of the NPC without titles (e.g., "Garrick", "Elara").
    /// </summary>
    public string? BaseName { get; set; }
    
    /// <summary>
    /// Gets or sets the title prefix (e.g., "Master", "Apprentice", "Lord").
    /// </summary>
    public string? TitlePrefix { get; set; }
    
    /// <summary>
    /// Gets or sets the title suffix (e.g., "the Wise", "of Stormwind").
    /// </summary>
    public string? TitleSuffix { get; set; }
    
    /// <summary>
    /// Gets or sets the age of the NPC in years.
    /// </summary>
    public int Age { get; set; }
    
    /// <summary>
    /// Gets or sets the NPC's profession or role (e.g., "Blacksmith", "Merchant", "Guard").
    /// </summary>
    public string Occupation { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the amount of gold this NPC possesses.
    /// </summary>
    public int Gold { get; set; }
    
    /// <summary>
    /// Gets or sets the default dialogue text for this NPC.
    /// </summary>
    public string Dialogue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether this NPC is friendly to the player.
    /// Hostile NPCs may initiate combat.
    /// </summary>
    public bool IsFriendly { get; set; } = true;

    /// <summary>
    /// Collection of dialogue line IDs for this NPC's conversations.
    /// These are resolved from @dialogue JSON references when player interacts with NPC.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Load dialogue when player talks to NPC
    /// var dialogueLines = await dialogueRepository.GetByIdsAsync(npc.DialogueIds);
    /// var greeting = dialogueLines.FirstOrDefault(d => d.Type == "greeting");
    /// DisplayDialogue(greeting);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Show dialogue options
    /// var dialogue_options = []
    /// for dialogue_id in npc.DialogueIds:
    ///     var dialogue = await dialogue_service.get_by_id(dialogue_id)
    ///     dialogue_options.append(dialogue)
    /// show_dialogue_ui(dialogue_options)
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Lazy loading - only load dialogue when player interacts</description></item>
    /// <item><description>Memory efficiency - dialogue not needed until conversation starts</description></item>
    /// <item><description>Dynamic dialogue - can change based on quest state</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["greeting-friendly", "shop-browse", "quest-offer"]
    /// </example>
    public List<string> DialogueIds { get; set; } = new();
    
    /// <summary>
    /// Collection of ability reference IDs (v4.1 format) this NPC can use if hostile or in combat.
    /// Each ID is a JSON reference like "@abilities/active/offensive:staff-strike".
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Resolve abilities if NPC becomes hostile
    /// if (!npc.IsFriendly)
    /// {
    ///     var resolver = new ReferenceResolverService(dataCache);
    ///     var abilities = new List&lt;Ability&gt;();
    ///     foreach (var refId in npc.AbilityIds)
    ///     {
    ///         var abilityJson = await resolver.ResolveToObjectAsync(refId);
    ///         var ability = abilityJson.ToObject&lt;Ability&gt;();
    ///         abilities.Add(ability);
    ///     }
    ///     npc.CombatAbilities = abilities;
    /// }
    /// </code>
    /// <code>
    /// // GDScript - Resolve abilities in Godot
    /// if not npc.is_friendly:
    ///     var resolver = ReferenceResolverService.new(data_cache)
    ///     for ref_id in npc.AbilityIds:
    ///         var ability_data = await resolver.ResolveToObjectAsync(ref_id)
    ///         npc.add_combat_ability(ability_data)
    /// </code>
    /// <para><strong>Use cases:</strong></para>
    /// <list type="bullet">
    /// <item><description>Guards who attack when player steals</description></item>
    /// <item><description>Quest NPCs who fight alongside player</description></item>
    /// <item><description>Arena trainers for combat tutorials</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example ability reference IDs:
    /// <code>
    /// [
    ///   "@abilities/active/offensive:sword-slash",
    ///   "@abilities/active/support:healing-word"
    /// ]
    /// </code>
    /// </example>
    public List<string> AbilityIds { get; set; } = new();
    
    /// <summary>
    /// Collection of inventory item reference IDs (v4.1 format) this NPC possesses.
    /// Used for merchant shops, trading, or NPC looting. Supports wildcard references.
    /// </summary>
    /// <remarks>
    /// <para><strong>✅ HOW TO RESOLVE - Use ReferenceResolverService:</strong></para>
    /// <code>
    /// // C# - Resolve shop inventory
    /// var resolver = new ReferenceResolverService(dataCache);
    /// var shopItems = new List&lt;Item&gt;();
    /// foreach (var refId in npc.InventoryIds)
    /// {
    ///     var itemJson = await resolver.ResolveToObjectAsync(refId);
    ///     var item = itemJson.ToObject&lt;Item&gt;();
    ///     shopItems.Add(item);
    /// }
    /// DisplayShopInventory(shopItems, npc.Occupation);
    /// </code>
    /// <code>
    /// // GDScript - Resolve inventory in Godot
    /// var resolver = ReferenceResolverService.new(data_cache)
    /// var shop_items = []
    /// for ref_id in npc.InventoryIds:
    ///     var item_data = await resolver.ResolveToObjectAsync(ref_id)
    ///     shop_items.append(item_data)
    /// show_shop_ui(shop_items)
    /// </code>
    /// <para><strong>Wildcard support for random stock:</strong></para>
    /// <list type="bullet">
    /// <item><description>"@items/consumables/potions:*" - Random potion types</description></item>
    /// <item><description>"@items/weapons/swords:*" - Random swords in stock</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example inventory reference IDs:
    /// <code>
    /// [
    ///   \"@items/consumables/potions:health-potion\",
    ///   \"@items/weapons/swords:*\",
    ///   \"@items/armor/chest:leather-armor\"
    /// ]
    /// </code>
    /// </example>
    public List<string> InventoryIds { get; set; } = new();

    /// <summary>
    /// Fully resolved DialogueLine objects for this NPC's conversations.
    /// Populated by NpcGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in DialogueIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property when player interacts with NPC</description></item>
    /// <item><description>Already resolved - no need to call ReferenceResolverService</description></item>
    /// <item><description>Null if NPC loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<DialogueLine> Dialogues { get; set; } = new();

    /// <summary>
    /// Fully resolved Ability objects for this NPC's combat actions (if hostile).
    /// Populated by NpcGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in AbilityIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property if NPC enters combat</description></item>
    /// <item><description>Already resolved - no need to call ReferenceResolverService</description></item>
    /// <item><description>Null if NPC loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Ability> Abilities { get; set; } = new();

    /// <summary>
    /// Fully resolved Item objects for this NPC's inventory (merchant stock, tradeable items).
    /// Populated by NpcGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in InventoryIds instead).
    /// </summary>
    /// <remarks>
    /// <para><strong>For Runtime Use:</strong></para>
    /// <list type="bullet">
    /// <item><description>Use this property for shop UI and trading</description></item>
    /// <item><description>Already resolved with wildcard selection applied</description></item>
    /// <item><description>Null if NPC loaded from template without hydration</description></item>
    /// </list>
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Item> Inventory { get; set; } = new();

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic properties.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    /// <summary>
    /// Composes the NPC name from individual naming components.
    /// Useful for rebuilding names, localization, or debugging.
    /// </summary>
    /// <returns>The composed name string.</returns>
    public string ComposeNameFromComponents()
    {
        var parts = new List<string>();
        
        // Order: [TitlePrefix] [Base] [TitleSuffix]
        // Examples: "Master Garrick", "Elara the Wise", "Lord Marcus of Stormwind"
        if (!string.IsNullOrWhiteSpace(TitlePrefix)) parts.Add(TitlePrefix);
        if (!string.IsNullOrWhiteSpace(BaseName)) parts.Add(BaseName);
        if (!string.IsNullOrWhiteSpace(TitleSuffix)) parts.Add(TitleSuffix);
        
        return string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
