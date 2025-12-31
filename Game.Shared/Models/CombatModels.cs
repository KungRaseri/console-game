namespace Game.Shared.Models;

/// <summary>
/// Represents a combat action taken by a combatant.
/// </summary>
public class CombatAction
{
    public CombatActionType Type { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string? TargetName { get; set; }
    public Item? ItemUsed { get; set; }
}

/// <summary>
/// Types of actions available in combat.
/// </summary>
public enum CombatActionType
{
    Attack,      // Physical or magic attack
    Defend,      // Reduce incoming damage
    UseItem,     // Use consumable item
    Flee         // Attempt to escape combat
}

/// <summary>
/// Result of a combat action.
/// </summary>
public class CombatResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Damage { get; set; } = 0;
    public int Healing { get; set; } = 0;
    public bool IsCritical { get; set; } = false;
    public bool IsDodged { get; set; } = false;
    public bool IsBlocked { get; set; } = false;
    public CombatEffect Effect { get; set; } = CombatEffect.None;
}

/// <summary>
/// Special combat effects.
/// </summary>
public enum CombatEffect
{
    None,
    Stunned,
    Poisoned,
    Burning,
    Frozen,
    Bleeding
}

/// <summary>
/// Outcome of a combat encounter.
/// </summary>
public class CombatOutcome
{
    public bool PlayerVictory { get; set; }
    public int XPGained { get; set; }
    public int GoldGained { get; set; }
    public List<Item> LootDropped { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}
