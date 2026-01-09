using MediatR;
using RealmEngine.Shared.Models;

namespace RealmEngine.Core.Features.Combat.Commands.EncounterBoss;

/// <summary>
/// Command to initiate a boss encounter.
/// Returns special information about the boss including enhanced rewards and difficulty warnings.
/// </summary>
public record EncounterBossCommand : IRequest<BossEncounterResult>
{
    public required string BossCategory { get; init; }
    public required string BossName { get; init; }
}

/// <summary>
/// Result of a boss encounter command with detailed boss information.
/// </summary>
public record BossEncounterResult
{
    public bool Success { get; init; }
    public Enemy? Boss { get; init; }
    public string? ErrorMessage { get; init; }
    public BossInfo? Info { get; init; }
}

/// <summary>
/// Detailed information about a boss enemy.
/// </summary>
public record BossInfo
{
    public string Name { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Level { get; init; }
    public int RecommendedPlayerLevel { get; init; }
    public EnemyDifficulty Difficulty { get; init; }
    public int EstimatedXP { get; init; }
    public int EstimatedGold { get; init; }
    public int HealthTotal { get; init; }
    public int AttackPower { get; init; }
    public int DefenseRating { get; init; }
    public List<string> Abilities { get; init; } = new();
    public List<string> SpecialTraits { get; init; } = new();
    public string WarningMessage { get; init; } = string.Empty;
}
