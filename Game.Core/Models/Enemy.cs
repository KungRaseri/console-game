using Game.Shared.Models;

namespace Game.Core.Models;

/// <summary>
/// Represents an enemy combatant.
/// </summary>
public class Enemy : ITraitable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Trait system
    public Dictionary<string, TraitValue> Traits { get; } = new();
    
    // Combat stats
    public int Level { get; set; } = 1;
    public int Health { get; set; } = 50;
    public int MaxHealth { get; set; } = 50;
    
    // D20 Attributes (same as player)
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;
    
    // Combat properties
    public int BasePhysicalDamage { get; set; } = 5;
    public int BaseMagicDamage { get; set; } = 0;
    public int XPReward { get; set; } = 25;
    public int GoldReward { get; set; } = 10;
    
    // Enemy type/difficulty
    public EnemyType Type { get; set; } = EnemyType.Common;
    public EnemyDifficulty Difficulty { get; set; } = EnemyDifficulty.Easy;
    
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
