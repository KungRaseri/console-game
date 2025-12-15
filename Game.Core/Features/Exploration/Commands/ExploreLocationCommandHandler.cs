using Game.Core.Models;
using Game.Core.Abstractions;
using Game.Shared.Services;
using Game.Core.Generators;
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
            // TODO: Restore progress bar - temporarily disabled
            /*
            _console.ShowProgress("Exploring...", task =>
            {
                task.MaxValue = 100;
                for (int i = 0; i <= 100; i += 10)
                {
                    task.Value = i;
                    Thread.Sleep(100);
                }
            });
            */

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
                var foundItem = ItemGenerator.Generate();
                
                player.Inventory.Add(foundItem);
                await _mediator.Publish(new ItemAcquired(player.Name, foundItem.Name), cancellationToken);
                
                var rarityColor = GetRarityColor(foundItem.Rarity);
                _console.ShowSuccess($"Found: {rarityColor}{foundItem.Name} ({foundItem.Rarity})[/]!");
                itemFound = foundItem.Name;
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
