using RealmEngine.Shared.Abstractions;
using MediatR;

namespace RealmEngine.Core.Features.CharacterCreation.Queries;

/// <summary>
/// Handles the GetCharacterClasses query.
/// </summary>
public class GetCharacterClassesHandler : IRequestHandler<GetCharacterClassesQuery, GetCharacterClassesResult>
{
    private readonly ICharacterClassRepository _repository;

    public GetCharacterClassesHandler(ICharacterClassRepository repository)
    {
        _repository = repository;
    }

    public Task<GetCharacterClassesResult> Handle(GetCharacterClassesQuery request, CancellationToken cancellationToken)
    {
        var classes = _repository.GetAllClasses();

        return Task.FromResult(new GetCharacterClassesResult
        {
            Classes = classes
        });
    }
}