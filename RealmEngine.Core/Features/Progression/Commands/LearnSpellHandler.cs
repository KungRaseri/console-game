using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Handles learning a spell from a spellbook.
/// </summary>
public class LearnSpellHandler : IRequestHandler<LearnSpellCommand, LearnSpellResult>
{
    private readonly SpellCastingService _spellService;

    public LearnSpellHandler(SpellCastingService spellService)
    {
        _spellService = spellService ?? throw new ArgumentNullException(nameof(spellService));
    }

    public Task<LearnSpellResult> Handle(LearnSpellCommand request, CancellationToken cancellationToken)
    {
        var result = _spellService.LearnSpell(request.Character, request.SpellId);

        return Task.FromResult(new LearnSpellResult
        {
            Success = result.Success,
            Message = result.Message,
            SpellLearned = result.SpellLearned
        });
    }
}
