using Game.Core.Models;
using MediatR;

namespace Game.Core.Features.CharacterCreation.Queries;

/// <summary>
/// Query to get all available character classes.
/// </summary>
public record GetCharacterClassesQuery : IRequest<GetCharacterClassesResult>
{
}

/// <summary>
/// Result containing available character classes.
/// </summary>
public record GetCharacterClassesResult
{
    public required List<CharacterClass> Classes { get; init; }
}
