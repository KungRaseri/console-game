using RealmEngine.Core.Abstractions;
using RealmEngine.Shared.Models;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Core.Features.Death.Queries;
using MediatR;

using RealmEngine.Core.Services;
namespace RealmEngine.Core.Features.Exploration;

/// <summary>
/// Service for handling exploration, travel, and location-based events.
/// </summary>
public class ExplorationService
{
    private readonly IMediator _mediator;
    private readonly GameStateService _gameState;
    private readonly SaveGameService _saveGameService;
    private readonly IGameUI _console;

    private readonly List<string> _knownLocations = new()
    {
        "Hub Town",
        "Dark Forest",
        "Ancient Ruins",
        "Dragon's Lair",
        "Cursed Graveyard",
        "Mountain Peak",
        "Coastal Village",
        "Underground Caverns"
    };

    public ExplorationService(IMediator mediator, GameStateService gameState, SaveGameService saveGameService, IGameUI console)
    {
        _mediator = mediator;
        _gameState = gameState;
        _saveGameService = saveGameService;
        _console = console;
    }

    /// <summary>
    /// Perform exploration at the current location.
    /// Returns true if combat should be initiated.
    /// </summary>
    public async Task<bool> ExploreAsync()
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
            return true; // Indicates combat should start
        }

        // Peaceful exploration - gain some XP
        var xpGained = Random.Shared.Next(10, 30);
        player.GainExperience(xpGained);

        // Check if leveled up
        var newLevel = player.Level;
        if (newLevel > player.Level - 1)
        {
            await _mediator.Publish(new PlayerLeveledUp(player.Name, newLevel));
        }

        _console.ShowSuccess($"Gained {xpGained} XP!");

        // Find gold
        var goldFound = Random.Shared.Next(5, 25);
        player.Gold += goldFound;
        await _mediator.Publish(new GoldGained(player.Name, goldFound));

        // Random chance to find an item (30% chance)
        if (Random.Shared.Next(100) < 30)
        {
            // TODO: Modernize - // TODO: Modernize - var foundItem = ItemGenerator.Generate();

            // TODO: Modernize - player.Inventory.Add(foundItem);
            // TODO: Modernize - await _mediator.Publish(new ItemAcquired(player.Name, foundItem.Name));

            // TODO: Modernize - var rarityColor = GetRarityColor(foundItem.Rarity);
            // TODO: Modernize - _console.ShowSuccess($"Found: {rarityColor}{foundItem.Name} ({foundItem.Rarity})[/]!");
        }

        return false; // No combat
    }

    /// <summary>
    /// Allow player to travel to a different location.
    /// </summary>
    public async Task TravelToLocation()
    {
        var availableLocations = _knownLocations
            .Where(loc => loc != _gameState.CurrentLocation)
            .ToList();

        if (!availableLocations.Any())
        {
            _console.ShowInfo("No other locations available.");
            return;
        }

        var choice = _console.ShowMenu(
            $"Current Location: {_gameState.CurrentLocation}\n\nWhere would you like to travel?",
            availableLocations.Concat(new[] { "Cancel" }).ToArray()
        );

        if (choice == "Cancel")
            return;

        _gameState.UpdateLocation(choice);

        _console.ShowSuccess($"Traveled to {_gameState.CurrentLocation}");

        // Check for dropped items at the new location
        await CheckForDroppedItemsAsync(choice);
    }

    /// <summary>
    /// Check for dropped items at the current location and allow player to recover them.
    /// </summary>
    private async Task CheckForDroppedItemsAsync(string location)
    {
        var result = await _mediator.Send(new GetDroppedItemsQuery { Location = location });

        if (result.HasItems)
        {
            _console.ShowWarning($"\n⚠️  You see your dropped items here! ({result.Items.Count} items)");

            if (_console.Confirm("Retrieve your items?"))
            {
                // Recover items
                var saveGame = _saveGameService.GetCurrentSave();
                if (saveGame != null && saveGame.Character != null)
                {
                    saveGame.Character.Inventory.AddRange(result.Items);
                    saveGame.DroppedItemsAtLocations.Remove(location);

                    _console.ShowSuccess($"Recovered {result.Items.Count} items!");
                    await Task.Delay(1500);
                }
            }
        }
    }

    /// <summary>
    /// Get all known locations.
    /// </summary>
    public IReadOnlyList<string> GetKnownLocations() => _knownLocations.AsReadOnly();

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