namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents an enemy combatant.
/// </summary>
public class Enemy : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this enemy.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the display name of the enemy.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the descriptive text for the enemy.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic properties.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    /// <summary>
    /// Gets or sets the enemy's current level.
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the enemy's current health points.
    /// </summary>
    public int Health { get; set; } = 50;
    
    /// <summary>
    /// Gets or sets the enemy's maximum health points.
    /// </summary>
    public int MaxHealth { get; set; } = 50;

    /// <summary>
    /// Gets or sets the Strength attribute. Affects melee damage and carry weight.
    /// </summary>
    public int Strength { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the Dexterity attribute. Affects dodge, accuracy, and critical hit chance.
    /// </summary>
    public int Dexterity { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the Constitution attribute. Affects max HP and physical defense.
    /// </summary>
    public int Constitution { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the Intelligence attribute. Affects magic damage and crafting.
    /// </summary>
    public int Intelligence { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the Wisdom attribute. Affects max mana and magic defense.
    /// </summary>
    public int Wisdom { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the Charisma attribute. Affects NPC interactions and rare loot drops.
    /// </summary>
    public int Charisma { get; set; } = 10;

    /// <summary>
    /// Gets or sets the base physical damage dealt by this enemy.
    /// </summary>
    public int BasePhysicalDamage { get; set; } = 5;
    
    /// <summary>
    /// Gets or sets the base magic damage dealt by this enemy.
    /// </summary>
    public int BaseMagicDamage { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the experience points rewarded when this enemy is defeated.
    /// </summary>
    public int XPReward { get; set; } = 25;
    
    /// <summary>
    /// Gets or sets the gold rewarded when this enemy is defeated.
    /// </summary>
    public int GoldReward { get; set; } = 10;

    /// <summary>
    /// Gets or sets the classification type of this enemy (Common, Elite, Boss, etc.).
    /// </summary>
    public EnemyType Type { get; set; } = EnemyType.Common;
    
    /// <summary>
    /// Gets or sets the difficulty rating of this enemy (Easy, Medium, Hard, etc.).
    /// </summary>
    public EnemyDifficulty Difficulty { get; set; } = EnemyDifficulty.Easy;

    /// <summary>
    /// Collection of ability IDs this enemy can use in combat.
    /// These are resolved from @abilities JSON references during enemy generation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Resolve IDs to full Ability objects
    /// var abilities = await abilityRepository.GetByIdsAsync(enemy.AbilityIds);
    /// foreach (var ability in abilities)
    /// {
    ///     if (ShouldUseAbility(ability))
    ///         ExecuteAbility(ability, target);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Load enemy abilities for combat
    /// var abilities = []
    /// for ability_id in enemy.AbilityIds:
    ///     var ability = await ability_service.get_by_id(ability_id)
    ///     abilities.append(ability)
    /// enemy.combat_abilities = abilities
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Enemy is a template, not a live combat instance</description></item>
    /// <item><description>Abilities loaded when combat starts</description></item>
    /// <item><description>Memory efficiency - 1000s of enemy templates in catalog</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["basic-attack", "fire-breath", "tail-swipe"]
    /// </example>
    public List<string> AbilityIds { get; set; } = new();

    /// <summary>
    /// Collection of loot table IDs for determining drops when this enemy is defeated.
    /// These are resolved from @loot JSON references during enemy generation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Generate loot on enemy death
    /// var lootTables = await lootTableRepository.GetByIdsAsync(enemy.LootTableIds);
    /// var droppedItems = lootGenerator.GenerateLoot(lootTables, enemy.Level);
    /// player.Inventory.AddRange(droppedItems);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Generate loot when enemy dies
    /// var loot_items = []
    /// for loot_table_id in enemy.LootTableIds:
    ///     var loot_table = await loot_service.get_by_id(loot_table_id)
    ///     var items = loot_generator.roll_loot(loot_table, enemy.Level)
    ///     loot_items.append_array(items)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["common-humanoid-loot", "dragon-hoard"]
    /// </example>
    public List<string> LootTableIds { get; set; } = new();

    /// <summary>
    /// Calculate physical damage bonus from Strength.
    /// </summary>
    public int GetPhysicalDamageBonus()
    {
        return Strength;
    }

    /// <summary>
    /// Calculate magic damage bonus from Intelligence.
    /// </summary>
    public int GetMagicDamageBonus()
    {
        return Intelligence;
    }

    /// <summary>
    /// Calculate dodge chance from Dexterity.
    /// </summary>
    public double GetDodgeChance()
    {
        return Dexterity * 0.5; // 10 DEX = 5% dodge
    }

    /// <summary>
    /// Calculate critical hit chance from Dexterity.
    /// </summary>
    public double GetCriticalChance()
    {
        return Dexterity * 0.3; // 10 DEX = 3% crit
    }

    /// <summary>
    /// Calculate physical defense from Constitution.
    /// </summary>
    public int GetPhysicalDefense()
    {
        return Constitution;
    }

    /// <summary>
    /// Calculate magic resistance from Wisdom.
    /// </summary>
    public double GetMagicResistance()
    {
        return Wisdom * 0.8; // 10 WIS = 8% resist
    }

    /// <summary>
    /// Check if enemy is still alive.
    /// </summary>
    public bool IsAlive()
    {
        return Health > 0;
    }
}

/// <summary>
/// Enemy types for flavor and potential special abilities.
/// </summary>
public enum EnemyType
{
    Common,      // Regular enemies
    Beast,       // Animals and monsters
    Undead,      // Zombies, skeletons
    Demon,       // Hellish creatures
    Elemental,   // Fire, ice, earth, air
    Humanoid,    // Bandits, soldiers
    Dragon,      // Dragons and dragonkin
    Boss         // Special boss enemies
}

/// <summary>
/// Enemy difficulty scaling.
/// </summary>
public enum EnemyDifficulty
{
    Easy,        // 0.5x player level
    Normal,      // 0.8x player level
    Hard,        // 1.0x player level
    Elite,       // 1.2x player level
    Boss         // 1.5x+ player level
}
