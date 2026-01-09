using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get learnable spells for a character based on their magic skills.
/// </summary>
public record GetLearnableSpellsQuery : IRequest<GetLearnableSpellsResult>
{
    /// <summary>Gets the character to get learnable spells for.</summary>
    public required Character Character { get; init; }
    /// <summary>Gets the optional magical tradition filter.</summary>
    public MagicalTradition? Tradition { get; init; }
}

/// <summary>
/// Result containing learnable spells.
/// </summary>
public record GetLearnableSpellsResult
{
    /// <summary>Gets the list of learnable spells.</summary>
    public required List<Spell> Spells { get; init; }
    /// <summary>Gets the total count of spells.</summary>
    public int TotalCount { get; init; }
}
