using Game.Core.Abstractions;
using Game.Shared.Models;
using MediatR;
using Serilog;

using Game.Core.Services;
namespace Game.Core.Features.Exploration.Commands;

/// <summary>
/// Handler for ExploreLocationCommand.
/// </summary>
public class ExploreLocationCommandHandler : IRequestHandler<ExploreLocationCommand, ExploreLocationResult>
{
    private readonly IMediator _mediator;
    private readonly GameStateService _gameState;
    private readonly IGameUI _console;

    public ExploreLocationCommandHandler(IMediator mediator, GameStateService gameState, IGameUI console)
    {
        _mediator = mediator;
        _gameState = gameState;
        _console = console;
    }

    public async Task<ExploreLocationResult> Handle(ExploreLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var player = _gameState.Player;

            _console.ShowInfo($"Exploring {_gameState.CurrentLocation}...");

            // Simulate exploration
            _console.ShowMessage("Exploring...");
            await Task.Delay(500); // Brief pause for immersion

            // 60% chance of combat encounter, 40% chance of peaceful exploration
            var encounterRoll = Random.Shared.Next(100);

            if (encounterRoll < 60)
            {
                // Combat encounter!
                _console.ShowWarning("You encounter an enemy!");
                await Task.Delay(300);
                return new ExploreLocationResult(true, CombatTriggered: true);
            }

            // Peaceful exploration - gain some XP
            var xpGained = Random.Shared.Next(10, 30);
            var oldLevel = player.Level;
            player.GainExperience(xpGained);

            // Check if leveled up
            if (player.Level > oldLevel)
            {
                await _mediator.Publish(new PlayerLeveledUp(player.Name, player.Level), cancellationToken);
            }

            _console.ShowSuccess($"Gained {xpGained} XP!");

            // Find gold
            var goldFound = Random.Shared.Next(5, 25);
            player.Gold += goldFound;
            await _mediator.Publish(new GoldGained(player.Name, goldFound), cancellationToken);

            string? itemFound = null;

            // Random chance to find an item (30% chance)
            if (Random.Shared.Next(100) < 30)
            {
                // TODO: Modernize - // TODO: Modernize - var foundItem = ItemGenerator.Generate();

                // TODO: Modernize - player.Inventory.Add(foundItem);
                // TODO: Modernize - await _mediator.Publish(new ItemAcquired(player.Name, foundItem.Name), cancellationToken);

                // TODO: Modernize - var rarityColor = GetRarityColor(foundItem.Rarity);
                // TODO: Modernize - _console.ShowSuccess($"Found: {rarityColor}{foundItem.Name} ({foundItem.Rarity})[/]!");
        // TODO: Modernize - itemFound = foundItem.Name;
            }

            return new ExploreLocationResult(
                Success: true,
                CombatTriggered: false,
                ExperienceGained: xpGained,
                GoldGained: goldFound,
                ItemFound: itemFound
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during exploration");
            return new ExploreLocationResult(false, false, ErrorMessage: ex.Message);
        }
    }

    private static string GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "[white]",
            ItemRarity.Uncommon => "[green]",
            ItemRarity.Rare => "[blue]",
            ItemRarity.Epic => "[purple]",
            ItemRarity.Legendary => "[orange1]",
            _ => "[grey]"
        };
    }
}