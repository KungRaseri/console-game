using Game.Core.Abstractions;
using MediatR;

namespace Game.Core.Features.CharacterCreation.Queries;

/// <summary>
/// Handles the GetCharacterClass query.
/// </summary>
public class GetCharacterClassHandler : IRequestHandler<GetCharacterClassQuery, GetCharacterClassResult>
{
    private readonly ICharacterClassRepository _repository;

    public GetCharacterClassHandler(ICharacterClassRepository repository)
    {
        _repository = repository;
    }

    public Task<GetCharacterClassResult> Handle(GetCharacterClassQuery request, CancellationToken cancellationToken)
    {
        var characterClass = _repository.GetClassByName(request.ClassName);

        return Task.FromResult(new GetCharacterClassResult
        {
            Found = characterClass != null,
            CharacterClass = characterClass
        });
    }
}
