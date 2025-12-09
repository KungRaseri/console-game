using Game.Models;
using Game.Services;
using Game.Shared.Data;
using Game.Features.CharacterCreation;
using Game.Features.SaveLoad;
using MediatR;
using Serilog;

namespace Game.Features.CharacterCreation.Commands;

/// <summary>
/// Handles the CreateCharacter command.
/// </summary>
public class CreateCharacterHandler : IRequestHandler<CreateCharacterCommand, CreateCharacterResult>
{
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;

    public CreateCharacterHandler(IMediator mediator, SaveGameService saveGameService)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
    }

    public async Task<CreateCharacterResult> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        // Create the character using the service
        var character = CharacterCreationService.CreateCharacter(
            request.PlayerName,
            request.ClassName,
            request.AttributeAllocation);

        // Publish character created event
        await _mediator.Publish(new CharacterCreated(character.Name), cancellationToken);

        // Create save game with the new character (using Normal difficulty as default for backward compatibility)
        var saveGame = _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        Log.Information("Character created: {CharacterName} ({ClassName})",
            character.Name, character.ClassName);

        return new CreateCharacterResult
        {
            Success = true,
            Message = $"Welcome, {character.Name} the {character.ClassName}!",
            Character = character,
            SaveGameId = saveGame.Id
        };
    }
}
