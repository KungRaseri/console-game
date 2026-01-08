using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to learn a new ability.
/// </summary>
public record LearnAbilityCommand : IRequest<LearnAbilityResult>
{
    public required Character Character { get; init; }
    public required string AbilityId { get; init; }
}

/// <summary>
/// Result of learning an ability.
/// </summary>
public record LearnAbilityResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
    public Ability? AbilityLearned { get; init; }
}
