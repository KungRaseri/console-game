using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Handles getting learnable spells for a character.
/// </summary>
public class GetLearnableSpellsHandler : IRequestHandler<GetLearnableSpellsQuery, GetLearnableSpellsResult>
{
    private readonly SpellCatalogService _spellService;

    public GetLearnableSpellsHandler(SpellCatalogService spellService)
    {
        _spellService = spellService ?? throw new ArgumentNullException(nameof(spellService));
    }

    public Task<GetLearnableSpellsResult> Handle(GetLearnableSpellsQuery request, CancellationToken cancellationToken)
    {
        var spells = request.Tradition.HasValue
            ? _spellService.GetSpellsByTradition(request.Tradition.Value)
            : _spellService.GetLearnableSpells(request.Character);

        return Task.FromResult(new GetLearnableSpellsResult
        {
            Spells = spells,
            TotalCount = spells.Count
        });
    }
}
