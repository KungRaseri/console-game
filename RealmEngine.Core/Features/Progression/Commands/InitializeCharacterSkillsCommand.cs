using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to initialize all skills for a new character.
/// </summary>
public record InitializeCharacterSkillsCommand : IRequest<InitializeCharacterSkillsResult>
{
    public required Character Character { get; init; }
}

/// <summary>
/// Result of initializing character skills.
/// </summary>
public record InitializeCharacterSkillsResult
{
    public int SkillsInitialized { get; init; }
    public List<string> SkillIds { get; init; } = new();
}
