using MediatR;
using RealmEngine.Shared.Models;
using Serilog;

namespace RealmEngine.Core.Features.CharacterCreation.Commands;

/// <summary>
/// Handles creating a new character with full initialization (abilities, spells, equipment).
/// </summary>
public class CreateCharacterHandler : IRequestHandler<CreateCharacterCommand, CreateCharacterResult>
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCharacterHandler"/> class.
    /// </summary>
    /// <param name="mediator">The mediator for sending commands.</param>
    public CreateCharacterHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Handles the create character command and returns the result.
    /// </summary>
    /// <param name="request">The create character command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the creation result.</returns>
    public async Task<CreateCharacterResult> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the character with base stats from class
            var character = CreateCharacterFromClass(request.CharacterName, request.CharacterClass);
            
            Log.Information("Created new character: {CharacterName} ({ClassName})", 
                request.CharacterName, request.CharacterClass.Name);

            // Initialize starting abilities
            var abilitiesCommand = new InitializeStartingAbilitiesCommand
            {
                Character = character,
                ClassName = request.CharacterClass.Name
            };
            
            var abilitiesResult = await _mediator.Send(abilitiesCommand, cancellationToken);
            
            if (!abilitiesResult.Success)
            {
                Log.Warning("Failed to initialize starting abilities for {CharacterName}: {Message}",
                    request.CharacterName, abilitiesResult.Message);
            }

            // Initialize starting spells
            var spellsCommand = new InitializeStartingSpellsCommand
            {
                Character = character,
                ClassName = request.CharacterClass.Name
            };
            
            var spellsResult = await _mediator.Send(spellsCommand, cancellationToken);
            
            if (!spellsResult.Success)
            {
                Log.Warning("Failed to initialize starting spells for {CharacterName}: {Message}",
                    request.CharacterName, spellsResult.Message);
            }

            Log.Information("Character creation complete: {CharacterName} with {AbilityCount} abilities and {SpellCount} spells",
                request.CharacterName, abilitiesResult.AbilitiesLearned, spellsResult.SpellsLearned);

            return new CreateCharacterResult
            {
                Character = character,
                Success = true,
                Message = $"Character {request.CharacterName} created successfully",
                AbilitiesLearned = abilitiesResult.AbilitiesLearned,
                SpellsLearned = spellsResult.SpellsLearned
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating character {CharacterName}", request.CharacterName);
            return new CreateCharacterResult
            {
                Character = null,
                Success = false,
                Message = $"Failed to create character: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new character object with base stats from the character class.
    /// </summary>
    private Character CreateCharacterFromClass(string name, CharacterClass characterClass)
    {
        var character = new Character
        {
            Name = name,
            ClassName = characterClass.Name,
            Level = 1,
            Experience = 0,
            
            // Apply class attribute bonuses
            Strength = 10 + characterClass.BonusStrength,
            Dexterity = 10 + characterClass.BonusDexterity,
            Constitution = 10 + characterClass.BonusConstitution,
            Intelligence = 10 + characterClass.BonusIntelligence,
            Wisdom = 10 + characterClass.BonusWisdom,
            Charisma = 10 + characterClass.BonusCharisma,
            
            // Set starting resources from class
            Health = characterClass.StartingHealth,
            MaxHealth = characterClass.StartingHealth,
            Mana = characterClass.StartingMana,
            MaxMana = characterClass.StartingMana,
            
            // Initialize collections
            Inventory = new List<Item>(),
            LearnedAbilities = new Dictionary<string, CharacterAbility>(),
            LearnedSpells = new Dictionary<string, CharacterSpell>(),
            Skills = new Dictionary<string, CharacterSkill>(),
            PendingLevelUps = new List<LevelUpInfo>()
        };

        return character;
    }
}
