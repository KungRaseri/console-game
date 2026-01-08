using MediatR;
using RealmEngine.Core.Features.Progression.Commands;
using Serilog;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Handles initializing starting spells for a new character.
/// </summary>
public class InitializeStartingSpellsHandler : IRequestHandler<InitializeStartingSpellsCommand, InitializeStartingSpellsResult>
{
    private readonly IMediator _mediator;

    public InitializeStartingSpellsHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<InitializeStartingSpellsResult> Handle(InitializeStartingSpellsCommand request, CancellationToken cancellationToken)
    {
        var spellsLearned = 0;
        var spellIds = new List<string>();
        
        // Get starting spells for class
        var startingSpells = GetStartingSpellsForClass(request.ClassName);
        
        if (startingSpells.Count == 0)
        {
            // Not all classes are spellcasters
            Log.Debug("No starting spells for class {ClassName} (non-spellcaster)", request.ClassName);
            return new InitializeStartingSpellsResult
            {
                Success = true,
                SpellsLearned = 0,
                Message = $"No starting spells for {request.ClassName}"
            };
        }

        // Learn each spell
        foreach (var spellId in startingSpells)
        {
            try
            {
                var command = new LearnSpellCommand
                {
                    Character = request.Character,
                    SpellId = spellId
                };

                var result = await _mediator.Send(command, cancellationToken);
                
                if (result.Success)
                {
                    spellsLearned++;
                    spellIds.Add(spellId);
                    Log.Information("Character {CharacterName} learned starting spell: {SpellId}", 
                        request.Character.Name, spellId);
                }
                else
                {
                    Log.Warning("Failed to learn starting spell {SpellId} for {CharacterName}: {Message}",
                        spellId, request.Character.Name, result.Message);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error learning starting spell {SpellId} for {CharacterName}", 
                    spellId, request.Character.Name);
            }
        }

        return new InitializeStartingSpellsResult
        {
            Success = true,
            SpellsLearned = spellsLearned,
            SpellIds = spellIds,
            Message = $"Learned {spellsLearned} starting spells"
        };
    }

    /// <summary>
    /// Get starting spells for a class based on class name.
    /// Only spellcasting classes have starting spells.
    /// </summary>
    private List<string> GetStartingSpellsForClass(string className)
    {
        return className.ToLower() switch
        {
            "mage" => new List<string>
            {
                "magic-missile",
                "ray-of-frost",
                "shield"
            },
            "cleric" => new List<string>
            {
                "sacred-flame",
                "cure-wounds",
                "bless"
            },
            "paladin" => new List<string>
            {
                "sacred-flame",
                "cure-wounds"
            },
            _ => new List<string>() // Warrior, Rogue, Ranger are not primary spellcasters
        };
    }
}
