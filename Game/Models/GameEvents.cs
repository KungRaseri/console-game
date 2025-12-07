using MediatR;

namespace Game.Models;

/// <summary>
/// Event raised when a character is created.
/// </summary>
public record CharacterCreated(string PlayerName) : INotification;

/// <summary>
/// Event raised when a player levels up.
/// </summary>
public record PlayerLeveledUp(string PlayerName, int NewLevel) : INotification;

/// <summary>
/// Event raised when player gains gold.
/// </summary>
public record GoldGained(string PlayerName, int Amount) : INotification;

/// <summary>
/// Event raised when player takes damage.
/// </summary>
public record DamageTaken(string PlayerName, int Amount) : INotification;

/// <summary>
/// Event raised when combat starts.
/// </summary>
public record CombatStarted(string PlayerName, string EnemyName) : INotification;

/// <summary>
/// Event raised when an attack is performed.
/// </summary>
public record AttackPerformed(string AttackerName, string DefenderName, int Damage) : INotification;

/// <summary>
/// Event raised when an enemy is defeated.
/// </summary>
public record EnemyDefeated(string PlayerName, string EnemyName) : INotification;

/// <summary>
/// Event raised when a player is defeated in combat.
/// </summary>
public record PlayerDefeated(string PlayerName, string EnemyName) : INotification;

/// <summary>
/// Event raised when combat ends.
/// </summary>
public record CombatEnded(string PlayerName, bool Victory) : INotification;

/// <summary>
/// Event raised when an item is acquired.
/// </summary>
public record ItemAcquired(string PlayerName, string ItemName) : INotification;
