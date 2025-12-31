using Game.Shared.Models;
using MediatR;

namespace Game.Core.Features.CharacterCreation.Queries;

/// <summary>
/// Query to get a character class by name.
/// </summary>
public record GetCharacterClassQuery : IRequest<GetCharacterClassResult>
{
    public required string ClassName { get; init; }
}

/// <summary>
/// Result containing character class details.
/// </summary>
public record GetCharacterClassResult
{
    public required bool Found { get; init; }
    public CharacterClass? CharacterClass { get; init; }
}