using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Handles getting available abilities for a character.
/// </summary>
public class GetAvailableAbilitiesHandler : IRequestHandler<GetAvailableAbilitiesQuery, GetAvailableAbilitiesResult>
{
    private readonly AbilityCatalogService _abilityService;

    public GetAvailableAbilitiesHandler(AbilityCatalogService abilityService)
    {
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
    }

    public Task<GetAvailableAbilitiesResult> Handle(GetAvailableAbilitiesQuery request, CancellationToken cancellationToken)
    {
        var abilities = request.Tier.HasValue
            ? _abilityService.GetAbilitiesByTier(request.Tier.Value)
            : _abilityService.GetUnlockableAbilities(request.ClassName, request.Level);

        return Task.FromResult(new GetAvailableAbilitiesResult
        {
            Abilities = abilities,
            TotalCount = abilities.Count
        });
    }
}
