using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get a character's skill progress for display.
/// </summary>
public record GetSkillProgressQuery : IRequest<GetSkillProgressResult>
{
    public required Character Character { get; init; }
    public required string SkillId { get; init; }
}

/// <summary>
/// Result containing skill progress information.
/// </summary>
public record GetSkillProgressResult
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
