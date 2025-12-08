using Game.Shared.Data;
using MediatR;

namespace Game.Features.CharacterCreation.Queries;

/// <summary>
/// Handles the GetCharacterClass query.
/// </summary>
public class GetCharacterClassHandler : IRequestHandler<GetCharacterClassQuery, GetCharacterClassResult>
{
    public Task<GetCharacterClassResult> Handle(GetCharacterClassQuery request, CancellationToken cancellationToken)
    {
        var characterClass = CharacterClassRepository.GetClassByName(request.ClassName);

        return Task.FromResult(new GetCharacterClassResult
        {
            Found = characterClass != null,
            CharacterClass = characterClass
        });
    }
}
