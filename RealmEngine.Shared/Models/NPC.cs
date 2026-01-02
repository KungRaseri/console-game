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
    /// Collection of ability IDs this NPC can use (if hostile or in combat).
    /// These are resolved from @abilities JSON references if NPC enters combat.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Load abilities if NPC becomes hostile
    /// if (!npc.IsFriendly)
    /// {
    ///     var abilities = await abilityRepository.GetByIdsAsync(npc.AbilityIds);
    ///     npc.CombatAbilities = abilities;
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # NPC turned hostile, load combat abilities
    /// if not npc.is_friendly:
    ///     for ability_id in npc.AbilityIds:
    ///         var ability = await ability_service.get_by_id(ability_id)
    ///         npc.add_combat_ability(ability)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["guard-strike", "call-for-help"]
    /// </example>
    public List<string> AbilityIds { get; set; } = new();
    
    /// <summary>
    /// Collection of inventory item IDs this NPC possesses (for shops, trading, or looting).
    /// These are resolved from @items JSON references when accessing NPC inventory.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Load shop inventory when player opens shop UI
    /// var shopItems = await itemRepository.GetByIdsAsync(npc.InventoryIds);
    /// DisplayShopInventory(shopItems, npc.Occupation);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Open merchant shop
    /// var shop_items = []
    /// for item_id in npc.InventoryIds:
    ///     var item = await item_service.get_by_id(item_id)
    ///     shop_items.append(item)
    /// show_shop_ui(shop_items)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@items/consumables/potions:health-potion", "@items/weapons/swords:iron-sword"]
    /// </example>
    public List<string> InventoryIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic properties.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();
}
