using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get all skills progress for character sheet.
/// </summary>
public record GetAllSkillsProgressQuery : IRequest<GetAllSkillsProgressResult>
{
    public required Character Character { get; init; }
}

/// <summary>
/// Result containing all skills progress.
/// </summary>
public record GetAllSkillsProgressResult
{
    public required List<SkillProgressDisplay> Skills { get; init; }
}

/// <summary>
/// Display model for skill progress in character sheet.
/// </summary>
public record SkillProgressDisplay
{
    public required string SkillId { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public int CurrentRank { get; init; }
    public int CurrentXP { get; init; }
    public int XPToNextRank { get; init; }
    public double ProgressPercent { get; init; }
    public required string CurrentEffect { get; init; }
    public required string NextRankEffect { get; init; }
}
