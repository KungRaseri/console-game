using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to award XP to a character's skill.
/// </summary>
public record AwardSkillXPCommand : IRequest<AwardSkillXPResult>
{
    public required Character Character { get; init; }
    public required string SkillId { get; init; }
    public required int XPAmount { get; init; }
    public string ActionSource { get; init; } = string.Empty;
}

/// <summary>
/// Result of awarding skill XP.
/// </summary>
public record AwardSkillXPResult
{
    public required string SkillId { get; init; }
    public int NewRank { get; init; }
    public int RanksGained { get; init; }
    public List<string> Notifications { get; init; } = new();
    public bool DidRankUp => RanksGained > 0;
}
