using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to learn a spell from a spellbook.
/// </summary>
public record LearnSpellCommand : IRequest<LearnSpellResult>
{
    public required Character Character { get; init; }
    public required string SpellId { get; init; }
}

/// <summary>
/// Result of learning a spell.
/// </summary>
public record LearnSpellResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
    public Spell? SpellLearned { get; init; }
}
