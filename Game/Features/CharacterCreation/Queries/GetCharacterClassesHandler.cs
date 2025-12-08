using Game.Shared.Data;
using MediatR;

namespace Game.Features.CharacterCreation.Queries;

/// <summary>
/// Handles the GetCharacterClasses query.
/// </summary>
public class GetCharacterClassesHandler : IRequestHandler<GetCharacterClassesQuery, GetCharacterClassesResult>
{
    public Task<GetCharacterClassesResult> Handle(GetCharacterClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = CharacterClassRepository.GetAllClasses();

        return Task.FromResult(new GetCharacterClassesResult
        {
            Classes = classes
        });
    }
}
