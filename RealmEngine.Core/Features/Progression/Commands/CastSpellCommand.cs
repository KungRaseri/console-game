using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Command to cast a spell.
/// </summary>
public record CastSpellCommand : IRequest<CastSpellResult>
{
    public required Character Caster { get; init; }
    public required string SpellId { get; init; }
    public Character? Target { get; init; }
}

/// <summary>
/// Result of casting a spell.
/// </summary>
public record CastSpellResult
{
    public bool Success { get; init; }
    public required string Message { get; init; }
    public int ManaCostPaid { get; init; }
    public string EffectValue { get; init; } = string.Empty;
    public bool WasFizzle { get; init; }
    public Spell? SpellCast { get; init; }
}
