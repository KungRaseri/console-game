using RealmEngine.Shared.Models;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Query to get learnable spells for a character based on their magic skills.
/// </summary>
public record GetLearnableSpellsQuery : IRequest<GetLearnableSpellsResult>
{
    public required Character Character { get; init; }
    public MagicalTradition? Tradition { get; init; }
}

/// <summary>
/// Result containing learnable spells.
/// </summary>
public record GetLearnableSpellsResult
{
    public required List<Spell> Spells { get; init; }
    public int TotalCount { get; init; }
}
