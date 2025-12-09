using Game.Models;
using Game.Shared.UI;
using Game.Shared.Services;
using Game.Features.SaveLoad;
using Game.Generators;
using MediatR;
using Serilog;

namespace Game.Features.Exploration;

/// <summary>
/// Service for handling exploration, travel, and location-based events.
/// </summary>
public class ExplorationService
{
    private readonly IMediator _mediator;
    private readonly GameStateService _gameState;
    private readonly SaveGameService _saveGameService;
    
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
    
    public ExplorationService(IMediator mediator, GameStateService gameState, SaveGameService saveGameService)
    {
        _mediator = mediator;
        _gameState = gameState;
        _saveGameService = saveGameService;
    }
    
    /// <summary>
    /// Perform exploration at the current location.
    /// Returns true if combat should be initiated.
    /// </summary>
    public async Task<bool> ExploreAsync()
    {
        var player = _gameState.Player;
        
        ConsoleUI.ShowInfo($"Exploring {_gameState.CurrentLocation}...");

        // Simulate exploration
        ConsoleUI.ShowProgress("Exploring...", task =>
        {
            task.MaxValue = 100;
            for (int i = 0; i <= 100; i += 10)
            {
                task.Value = i;
                Thread.Sleep(100);
            }
        });

        // 60% chance of combat encounter, 40% chance of peaceful exploration
        var encounterRoll = Random.Shared.Next(100);
        
        if (encounterRoll < 60)
        {
            // Combat encounter!
            ConsoleUI.ShowWarning("You encounter an enemy!");
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

        ConsoleUI.ShowSuccess($"Gained {xpGained} XP!");

        // Find gold
        var goldFound = Random.Shared.Next(5, 25);
        player.Gold += goldFound;
        await _mediator.Publish(new GoldGained(player.Name, goldFound));

        // Random chance to find an item (30% chance)
        if (Random.Shared.Next(100) < 30)
        {
            var foundItem = ItemGenerator.Generate();
            
            player.Inventory.Add(foundItem);
            await _mediator.Publish(new ItemAcquired(player.Name, foundItem.Name));
            
            var rarityColor = GetRarityColor(foundItem.Rarity);
            ConsoleUI.ShowSuccess($"Found: {rarityColor}{foundItem.Name} ({foundItem.Rarity})[/]!");
        }
        
        return false; // No combat
    }
    
    /// <summary>
    /// Allow player to travel to a different location.
    /// </summary>
    public void TravelToLocation()
    {
        var availableLocations = _knownLocations
            .Where(loc => loc != _gameState.CurrentLocation)
            .ToList();

        if (!availableLocations.Any())
        {
            ConsoleUI.ShowInfo("No other locations available.");
            return;
        }

        var choice = ConsoleUI.ShowMenu(
            $"Current Location: {_gameState.CurrentLocation}\n\nWhere would you like to travel?",
            availableLocations.Concat(new[] { "Cancel" }).ToArray()
        );

        if (choice == "Cancel")
            return;

        _gameState.UpdateLocation(choice);
        
        ConsoleUI.ShowSuccess($"Traveled to {_gameState.CurrentLocation}");
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
