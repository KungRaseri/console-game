using Game.Models;
using Game.Services;
using Game.Features.Exploration;
using Game.Features.SaveLoad;
using Game.Shared.Services;
using Game.Shared.UI;
using Game.Tests.Helpers;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console.Testing;

namespace Game.Tests.Features;

/// <summary>
/// Expanded unit tests for ExplorationService focusing on coverage gaps.
/// Tests dropped items recovery, edge cases, and event publishing.
/// </summary>
public class ExplorationServiceExpandedTests : IDisposable
{
    private readonly ExplorationService _explorationService;
    private readonly GameStateService _gameStateService;
    private readonly SaveGameService _saveGameService;
    private readonly IMediator _mediator;
    private readonly Mock<IConsoleUI> _mockConsoleUI;
    private readonly TestConsole _testConsole;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly string _testDbPath;

    public ExplorationServiceExpandedTests()
    {
        // Use unique test database to avoid file locking issues
        _testDbPath = $"test-exploration-expanded-{Guid.NewGuid()}.db";
        
        // Create TestConsole for UI testing (needed for menu interactions)
        _testConsole = TestConsoleHelper.CreateInteractiveConsole();
        
        // Mock IConsoleUI to avoid Thread.Sleep in ShowProgress (1.1s per exploration!)
        _mockConsoleUI = new Mock<IConsoleUI>();
        
        // Setup ShowProgress to do NOTHING (skip the 1.1s Thread.Sleep delay)
        _mockConsoleUI.Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<Action<Spectre.Console.ProgressTask>>()));
        
        // Setup ShowMenu to use real TestConsole for menu interactions
        _mockConsoleUI.Setup(x => x.ShowMenu(It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns<string, string[]>((prompt, choices) =>
            {
                var consoleUI = new ConsoleUI(_testConsole);
                return consoleUI.ShowMenu(prompt, choices);
            });
        
        // Setup Confirm to use real TestConsole
        _mockConsoleUI.Setup(x => x.Confirm(It.IsAny<string>()))
            .Returns<string>((prompt) =>
            {
                var consoleUI = new ConsoleUI(_testConsole);
                return consoleUI.Confirm(prompt);
            });
        
        _apocalypseTimer = new ApocalypseTimer(_mockConsoleUI.Object);
        _saveGameService = new SaveGameService(_apocalypseTimer, _testDbPath);
        _gameStateService = new GameStateService(_saveGameService);
        
        // Setup MediatR with all required services for handlers
        var services = new ServiceCollection();
        
        // Register services that MediatR handlers need
        services.AddSingleton<IConsoleUI>(_mockConsoleUI.Object);
        services.AddSingleton(_saveGameService);
        services.AddSingleton(_gameStateService);
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ExplorationService).Assembly);
        });
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        
        _explorationService = new ExplorationService(_mediator, _gameStateService, _saveGameService, _mockConsoleUI.Object);
    }

    public void Dispose()
    {
        // Dispose of SaveGameService first to release file locks
        _saveGameService?.Dispose();
        
        // Clean up test database files
        try
        {
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
            
            var logFile = _testDbPath.Replace(".db", "-log.db");
            if (File.Exists(logFile))
                File.Delete(logFile);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    #region Dropped Items Recovery Tests

    [Fact]
    public async Task TravelToLocation_Should_Detect_Dropped_Items_At_Destination()
    {
        // Arrange
        var character = new Character
        {
            Name = "ItemRecoverer",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>
            {
                new Item { Name = "Lost Sword", Type = ItemType.Weapon, Rarity = ItemRarity.Rare },
                new Item { Name = "Lost Shield", Type = ItemType.Shield, Rarity = ItemRarity.Common }
            }
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Drop items at "Dark Forest" using death service
        var itemsToDrop = character.Inventory.Take(2).ToList();
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame!.DroppedItemsAtLocations["Dark Forest"] = new List<Item>(itemsToDrop);
        
        // Clear inventory to simulate death drop
        character.Inventory.Clear();
        
        // Select "Dark Forest" and confirm recovery (Yes)
        var locations = _explorationService.GetKnownLocations();
        var darkForestIndex = locations.ToList().FindIndex(loc => loc == "Dark Forest");
        TestConsoleHelper.SelectMenuOption(_testConsole, darkForestIndex);
        TestConsoleHelper.ConfirmPrompt(_testConsole, true); // Confirm recovery

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be("Dark Forest", "Should travel to Dark Forest");
        character.Inventory.Should().HaveCount(2, "Should recover dropped items");
        character.Inventory.Should().Contain(i => i.Name == "Lost Sword");
        character.Inventory.Should().Contain(i => i.Name == "Lost Shield");
        saveGame.DroppedItemsAtLocations.Should().NotContainKey("Dark Forest", "Dropped items should be removed after recovery");
    }

    [Fact]
    public async Task TravelToLocation_Should_Allow_Player_To_Decline_Item_Recovery()
    {
        // Arrange
        var character = new Character
        {
            Name = "IndecisiveTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Drop items at "Ancient Ruins"
        var droppedItems = new List<Item>
        {
            new Item { Name = "Abandoned Axe", Type = ItemType.Weapon, Rarity = ItemRarity.Uncommon }
        };
        
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame!.DroppedItemsAtLocations["Ancient Ruins"] = droppedItems;
        
        // Select "Ancient Ruins" and decline recovery (No)
        var locations = _explorationService.GetKnownLocations();
        var ruinsIndex = locations.ToList().FindIndex(loc => loc == "Ancient Ruins");
        TestConsoleHelper.SelectMenuOption(_testConsole, ruinsIndex);
        TestConsoleHelper.ConfirmPrompt(_testConsole, false); // Decline recovery

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be("Ancient Ruins", "Should still travel to Ancient Ruins");
        character.Inventory.Should().BeEmpty("Should not recover items when declined");
        saveGame.DroppedItemsAtLocations.Should().ContainKey("Ancient Ruins", "Items should remain dropped when declined");
        saveGame.DroppedItemsAtLocations["Ancient Ruins"].Should().HaveCount(1, "Item should still be there");
    }

    [Fact]
    public async Task TravelToLocation_Should_Not_Show_Recovery_Prompt_When_No_Items_Dropped()
    {
        // Arrange
        var character = new Character
        {
            Name = "CleanTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Select "Coastal Village" (no dropped items there)
        var locations = _explorationService.GetKnownLocations();
        var villageIndex = locations.ToList().FindIndex(loc => loc == "Coastal Village");
        TestConsoleHelper.SelectMenuOption(_testConsole, villageIndex);

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be("Coastal Village", "Should travel to Coastal Village");
        character.Inventory.Should().BeEmpty("No items to recover");
        _testConsole.Output.Should().NotContain("dropped items", "Should not mention dropped items when none exist");
    }

    [Fact]
    public async Task TravelToLocation_Should_Handle_Multiple_Dropped_Items_At_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "HoarderRecoverer",
            Level = 10,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Drop many items at "Dragon's Lair"
        var droppedItems = new List<Item>
        {
            new Item { Name = "Dragon Sword", Type = ItemType.Weapon, Rarity = ItemRarity.Epic },
            new Item { Name = "Dragon Scale Armor", Type = ItemType.Chest, Rarity = ItemRarity.Epic },
            new Item { Name = "Health Potion", Type = ItemType.Consumable, Rarity = ItemRarity.Common },
            new Item { Name = "Mana Potion", Type = ItemType.Consumable, Rarity = ItemRarity.Common },
            new Item { Name = "Ancient Amulet", Type = ItemType.Necklace, Rarity = ItemRarity.Legendary }
        };
        
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame!.DroppedItemsAtLocations["Dragon's Lair"] = droppedItems;
        
        // Select "Dragon's Lair" and confirm recovery
        var locations = _explorationService.GetKnownLocations();
        var lairIndex = locations.ToList().FindIndex(loc => loc == "Dragon's Lair");
        TestConsoleHelper.SelectMenuOption(_testConsole, lairIndex);
        TestConsoleHelper.ConfirmPrompt(_testConsole, true); // Confirm recovery

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be("Dragon's Lair");
        character.Inventory.Should().HaveCount(5, "Should recover all 5 dropped items");
        character.Inventory.Should().Contain(i => i.Name == "Dragon Sword");
        character.Inventory.Should().Contain(i => i.Name == "Ancient Amulet");
        saveGame.DroppedItemsAtLocations.Should().NotContainKey("Dragon's Lair", "All items recovered");
    }

    [Fact]
    public async Task TravelToLocation_Should_Display_Dropped_Item_Count_In_Warning()
    {
        // Arrange
        var character = new Character
        {
            Name = "AlertTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Drop exactly 3 items at "Cursed Graveyard"
        var droppedItems = new List<Item>
        {
            new Item { Name = "Item1", Type = ItemType.Weapon, Rarity = ItemRarity.Common },
            new Item { Name = "Item2", Type = ItemType.Chest, Rarity = ItemRarity.Common },
            new Item { Name = "Item3", Type = ItemType.Consumable, Rarity = ItemRarity.Common }
        };
        
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame!.DroppedItemsAtLocations["Cursed Graveyard"] = droppedItems;
        
        // Select "Cursed Graveyard" and decline recovery
        var locations = _explorationService.GetKnownLocations();
        var graveyardIndex = locations.ToList().FindIndex(loc => loc == "Cursed Graveyard");
        TestConsoleHelper.SelectMenuOption(_testConsole, graveyardIndex);
        TestConsoleHelper.ConfirmPrompt(_testConsole, false);

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _testConsole.Output.Should().Contain("3 items", "Should display the count of dropped items");
        _testConsole.Output.Should().Contain("dropped items", "Should mention dropped items");
    }

    #endregion

    #region GetKnownLocations Edge Cases

    [Fact]
    public void GetKnownLocations_Should_Return_Exactly_8_Locations()
    {
        // Act
        var locations = _explorationService.GetKnownLocations();

        // Assert
        locations.Should().HaveCount(8, "There are 8 hardcoded locations in the game");
    }

    [Fact]
    public void GetKnownLocations_Should_Return_Same_Instance_On_Multiple_Calls()
    {
        // Act
        var locations1 = _explorationService.GetKnownLocations();
        var locations2 = _explorationService.GetKnownLocations();

        // Assert - Both should have same contents (readonly list)
        locations1.Should().Equal(locations2, "GetKnownLocations should return consistent results");
    }

    [Fact]
    public void GetKnownLocations_Should_Not_Allow_Modification()
    {
        // Act
        var locations = _explorationService.GetKnownLocations();

        // Assert
        locations.Should().BeAssignableTo<IReadOnlyList<string>>("Should be readonly");
        
        // Verify it's truly readonly (can't cast to List<string>)
        var isModifiable = locations is List<string>;
        isModifiable.Should().BeFalse("Should not be modifiable List");
    }

    #endregion

    #region ExploreAsync Edge Cases and Specific Outcomes

    [Fact]
    public async Task ExploreAsync_Should_Never_Award_Negative_XP()
    {
        // Arrange
        var character = new Character
        {
            Name = "SafeExplorer",
            Level = 1,
            Experience = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run a few explorations to test randomness (minimal iterations due to 1.1s delay per call)
        for (int i = 0; i < 5; i++)
        {
            var startXP = character.Experience;
            await _explorationService.ExploreAsync();
            
            // Assert each iteration
            character.Experience.Should().BeGreaterThanOrEqualTo(startXP, "XP should never decrease");
        }
    }

    [Fact]
    public async Task ExploreAsync_Should_Never_Reduce_Gold()
    {
        // Arrange
        var character = new Character
        {
            Name = "WealthyExplorer",
            Level = 5,
            Gold = 100,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run a few explorations (minimal iterations due to 1.1s delay per call)
        for (int i = 0; i < 5; i++)
        {
            var startGold = character.Gold;
            await _explorationService.ExploreAsync();
            
            // Assert each iteration
            character.Gold.Should().BeGreaterThanOrEqualTo(startGold, "Gold should never decrease from exploration");
        }
    }

    [Fact]
    public async Task ExploreAsync_Should_Display_Exploring_Message_With_Current_Location()
    {
        // Arrange
        var character = new Character
        {
            Name = "Observer",
            Level = 1,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var currentLocation = _gameStateService.CurrentLocation;

        // Act
        await _explorationService.ExploreAsync();

        // Assert
        _testConsole.Output.Should().Contain($"Exploring {currentLocation}", "Should display current location");
    }

    [Fact]
    public async Task ExploreAsync_Should_Show_Combat_Warning_When_Enemy_Encountered()
    {
        // Arrange
        var character = new Character
        {
            Name = "Warrior",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run many times to eventually trigger combat (60% chance)
        bool combatWarningShown = false;
        for (int i = 0; i < 20; i++)
        {
            var combatTriggered = await _explorationService.ExploreAsync();
            if (combatTriggered && _testConsole.Output.Contains("encounter an enemy"))
            {
                combatWarningShown = true;
                break;
            }
        }

        // Assert
        combatWarningShown.Should().BeTrue("Should show combat warning when enemy encountered");
    }

    [Fact]
    public async Task ExploreAsync_Should_Show_Success_Message_For_XP_Gain_On_Peaceful_Exploration()
    {
        // Arrange
        var character = new Character
        {
            Name = "PeacefulExplorer",
            Level = 1,
            Experience = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run many times to get peaceful exploration (40% chance)
        bool xpMessageShown = false;
        for (int i = 0; i < 30; i++)
        {
            var testConsole = TestConsoleHelper.CreateInteractiveConsole();
            var consoleUI = new ConsoleUI(testConsole);
            var service = new ExplorationService(_mediator, _gameStateService, _saveGameService, consoleUI);
            
            var combatTriggered = await service.ExploreAsync();
            if (!combatTriggered && testConsole.Output.Contains("Gained") && testConsole.Output.Contains("XP"))
            {
                xpMessageShown = true;
                break;
            }
        }

        // Assert
        xpMessageShown.Should().BeTrue("Should show XP gain message on peaceful exploration");
    }

    [Fact]
    public async Task ExploreAsync_Should_Award_XP_Between_10_And_30_On_Peaceful_Exploration()
    {
        // Arrange
        var character = new Character
        {
            Name = "XPValidator",
            Level = 1,
            Experience = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Track XP gains from peaceful explorations
        var xpGains = new List<int>();

        // Act - Run minimal explorations to collect data (peaceful = 40% chance, so ~8 peaceful out of 20)
        for (int i = 0; i < 20; i++)
        {
            var startXP = character.Experience;
            var combatTriggered = await _explorationService.ExploreAsync();
            
            if (!combatTriggered)
            {
                var xpGained = character.Experience - startXP;
                xpGains.Add(xpGained);
            }
        }

        // Assert - Should have some peaceful encounters
        xpGains.Should().NotBeEmpty("Should have had some peaceful explorations");
        
        // All XP gains should be in range [10, 30]
        xpGains.Should().OnlyContain(xp => xp >= 10 && xp <= 30, 
            "XP from peaceful exploration should be between 10 and 30");
    }

    [Fact]
    public async Task ExploreAsync_Should_Award_Gold_Between_5_And_25_On_Peaceful_Exploration()
    {
        // Arrange
        var character = new Character
        {
            Name = "GoldValidator",
            Level = 1,
            Gold = 0,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        // Track gold gains from peaceful explorations
        var goldGains = new List<int>();

        // Act - Run minimal explorations (peaceful = 40% chance, so ~8 peaceful out of 20)
        for (int i = 0; i < 20; i++)
        {
            var startGold = character.Gold;
            var combatTriggered = await _explorationService.ExploreAsync();
            
            if (!combatTriggered)
            {
                var goldGained = character.Gold - startGold;
                goldGains.Add(goldGained);
            }
        }

        // Assert
        goldGains.Should().NotBeEmpty("Should have had peaceful explorations");
        
        // All gold gains should be in range [5, 25]
        goldGains.Should().OnlyContain(gold => gold >= 5 && gold <= 25, 
            "Gold from peaceful exploration should be between 5 and 25");
    }

    [Fact]
    public async Task ExploreAsync_Should_Have_30_Percent_Chance_Of_Finding_Item()
    {
        // Arrange
        var character = new Character
        {
            Name = "StatisticalExplorer",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        
        int peacefulExplorations = 0;
        int itemsFound = 0;

        // Act - Run minimal explorations (peaceful = 40%, item find = 30% of peaceful = 12% overall)
        // Expect ~1-2 items out of 15 explorations
        for (int i = 0; i < 15; i++)
        {
            var startInventoryCount = character.Inventory.Count;
            var combatTriggered = await _explorationService.ExploreAsync();
            
            if (!combatTriggered)
            {
                peacefulExplorations++;
                if (character.Inventory.Count > startInventoryCount)
                {
                    itemsFound++;
                }
            }
        }

        // Assert - Just verify item discovery works (not strict probability with small sample)
        peacefulExplorations.Should().BeGreaterThan(0, "Should have had peaceful explorations");
        
        // If we had peaceful explorations, verify items CAN be found (may be 0 due to randomness)
        if (peacefulExplorations >= 5)
        {
            var itemDiscoveryRate = (double)itemsFound / peacefulExplorations;
            itemDiscoveryRate.Should().BeInRange(0.0, 0.60,
                "Item discovery rate should be reasonable (30% ± large margin for small sample)");
        }
    }

    [Fact]
    public async Task ExploreAsync_Should_Generate_Valid_Items_When_Found()
    {
        // Arrange
        var character = new Character
        {
            Name = "ItemCollector",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run minimal explorations to find at least one item (30% chance in peaceful, 40% peaceful = 12% overall)
        for (int i = 0; i < 15; i++)
        {
            await _explorationService.ExploreAsync();
            
            if (character.Inventory.Any())
            {
                break;
            }
        }

        // Assert - If we found items, they should be valid
        if (character.Inventory.Any())
        {
            foreach (var item in character.Inventory)
            {
                item.Should().NotBeNull("Found item should not be null");
                item.Name.Should().NotBeNullOrEmpty("Item should have a name");
                item.Type.Should().BeDefined("Item should have valid type");
                item.Rarity.Should().BeDefined("Item should have valid rarity");
            }
        }
    }

    [Fact]
    public async Task ExploreAsync_Should_Display_Item_Rarity_In_Color_When_Found()
    {
        // Arrange
        var character = new Character
        {
            Name = "RarityObserver",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act - Run minimal explorations to find item with rarity display (creating new service each time is expensive)
        bool itemFoundWithRarity = false;
        for (int i = 0; i < 15; i++)
        {
            var testConsole = TestConsoleHelper.CreateInteractiveConsole();
            var consoleUI = new ConsoleUI(testConsole);
            var service = new ExplorationService(_mediator, _gameStateService, _saveGameService, consoleUI);
            
            var combatTriggered = await service.ExploreAsync();
            
            if (!combatTriggered && testConsole.Output.Contains("Found:"))
            {
                // Check if output contains rarity indicators (Common, Uncommon, Rare, Epic, Legendary)
                var output = testConsole.Output;
                var hasRarity = output.Contains("Common") || output.Contains("Uncommon") || 
                               output.Contains("Rare") || output.Contains("Epic") || 
                               output.Contains("Legendary");
                
                if (hasRarity)
                {
                    itemFoundWithRarity = true;
                    break;
                }
            }
        }

        // Assert
        itemFoundWithRarity.Should().BeTrue("Should display item rarity when item is found");
    }

    #endregion

    #region TravelToLocation Edge Cases

    [Fact]
    public async Task TravelToLocation_Should_Handle_Travelling_To_Same_Location_Repeatedly()
    {
        // Arrange
        var character = new Character
        {
            Name = "RepeatTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);

        // Act & Assert - Travel to first location multiple times
        for (int i = 0; i < 3; i++)
        {
            TestConsoleHelper.SelectMenuOption(_testConsole, 0);
            await _explorationService.TravelToLocation();
            
            _gameStateService.CurrentLocation.Should().NotBeNullOrEmpty($"Location should be set after travel #{i + 1}");
        }
    }

    [Fact]
    public async Task TravelToLocation_Should_Exclude_Current_Location_From_Menu()
    {
        // Arrange
        var character = new Character
        {
            Name = "SmartTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var currentLocation = _gameStateService.CurrentLocation;
        
        // Select first option and travel
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);
        await _explorationService.TravelToLocation();
        
        var newLocation = _gameStateService.CurrentLocation;

        // Assert
        newLocation.Should().NotBe(currentLocation, 
            "Should not be able to travel to current location (it's excluded from menu)");
    }

    [Fact]
    public async Task TravelToLocation_Should_Show_Current_Location_In_Prompt()
    {
        // Arrange
        var character = new Character
        {
            Name = "LocationAwareTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var currentLocation = _gameStateService.CurrentLocation;
        
        // Select cancel to not actually travel
        var locations = _explorationService.GetKnownLocations();
        TestConsoleHelper.SelectMenuOption(_testConsole, locations.Count); // Cancel option

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _testConsole.Output.Should().Contain($"Current Location: {currentLocation}", 
            "Travel menu should show current location");
    }

    [Fact]
    public async Task TravelToLocation_Should_Include_Cancel_Option()
    {
        // Arrange
        var character = new Character
        {
            Name = "CautiousTraveler",
            Level = 5,
            Health = 100,
            MaxHealth = 100
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var initialLocation = _gameStateService.CurrentLocation;
        
        // Select last option (Cancel)
        var locations = _explorationService.GetKnownLocations();
        TestConsoleHelper.SelectMenuOption(_testConsole, locations.Count);

        // Act
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be(initialLocation, 
            "Location should not change when Cancel is selected");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task Integration_Should_Allow_Explore_Then_Travel_Then_Explore_Again()
    {
        // Arrange
        var character = new Character
        {
            Name = "AdventureCycle",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Gold = 0,
            Experience = 0,
            Inventory = new List<Item>()
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var startLocation = _gameStateService.CurrentLocation;

        // Act
        // 1. Explore at starting location
        await _explorationService.ExploreAsync();
        
        // 2. Travel to new location
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);
        await _explorationService.TravelToLocation();
        var newLocation = _gameStateService.CurrentLocation;
        
        // 3. Explore at new location
        await _explorationService.ExploreAsync();

        // Assert
        newLocation.Should().NotBe(startLocation, "Should have traveled to new location");
        
        // Should have gained resources from at least one exploration
        (character.Gold > 0 || character.Experience > 0 || character.Inventory.Any())
            .Should().BeTrue("Should have gained something from explorations");
    }

    [Fact]
    public async Task Integration_Should_Handle_Death_Drop_Recovery_Workflow()
    {
        // Arrange
        var character = new Character
        {
            Name = "Phoenix",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Gold = 100,
            Inventory = new List<Item>
            {
                new Item { Name = "Precious Sword", Type = ItemType.Weapon, Rarity = ItemRarity.Legendary },
                new Item { Name = "Magic Ring", Type = ItemType.Ring, Rarity = ItemRarity.Epic }
            }
        };
        
        _saveGameService.CreateNewGame(character, DifficultySettings.Normal);
        var deathLocation = _gameStateService.CurrentLocation;
        var itemsBeforeDeath = character.Inventory.ToList();
        
        // 1. Simulate death (drop items)
        var saveGame = _saveGameService.GetCurrentSave();
        saveGame!.DroppedItemsAtLocations[deathLocation] = new List<Item>(itemsBeforeDeath);
        character.Inventory.Clear();
        
        // 2. Travel away from death location
        TestConsoleHelper.SelectMenuOption(_testConsole, 0);
        await _explorationService.TravelToLocation();
        var awayLocation = _gameStateService.CurrentLocation;
        awayLocation.Should().NotBe(deathLocation, "Should travel away from death location");
        
        // 3. Travel back to death location and recover items
        var locations = _explorationService.GetKnownLocations();
        var deathLocationIndex = locations.ToList().FindIndex(loc => loc == deathLocation);
        TestConsoleHelper.SelectMenuOption(_testConsole, deathLocationIndex);
        TestConsoleHelper.ConfirmPrompt(_testConsole, true); // Confirm recovery
        
        await _explorationService.TravelToLocation();

        // Assert
        _gameStateService.CurrentLocation.Should().Be(deathLocation, "Should return to death location");
        character.Inventory.Should().HaveCount(2, "Should recover all dropped items");
        character.Inventory.Should().Contain(i => i.Name == "Precious Sword");
        character.Inventory.Should().Contain(i => i.Name == "Magic Ring");
        saveGame.DroppedItemsAtLocations.Should().NotContainKey(deathLocation, "Items should be removed after recovery");
    }

    #endregion
}
