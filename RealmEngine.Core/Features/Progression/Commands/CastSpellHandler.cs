using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Handles casting a spell.
/// </summary>
public class CastSpellHandler : IRequestHandler<CastSpellCommand, CastSpellResult>
{
    private readonly SpellCastingService _spellService;

    public CastSpellHandler(SpellCastingService spellService)
    {
        _spellService = spellService ?? throw new ArgumentNullException(nameof(spellService));
    }

    public Task<CastSpellResult> Handle(CastSpellCommand request, CancellationToken cancellationToken)
    {
        var result = _spellService.CastSpell(
            request.Caster,
            request.SpellId,
            request.Target);

        return Task.FromResult(new CastSpellResult
        {
            Success = result.Success,
            Message = result.Message,
            ManaCostPaid = result.ManaCostPaid,
            EffectValue = result.EffectValue,
            WasFizzle = result.WasFizzle,
            SpellCast = result.SpellCast
        });
    }
}
