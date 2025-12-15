using Game.Core.Features.Combat;
using Game.Core.Features.Exploration;
using Game.Core.Features.SaveLoad;
using Game.Core.Services;
using Game.Console.UI;
using Game.Console.Orchestrators;
using Game.Data.Repositories;
using MediatR;

namespace Game.Shared.Services;

/// <summary>
/// Service aggregator for GameEngine to reduce constructor complexity.
/// Groups related services into logical categories.
/// </summary>
public class GameEngineServices
{
    // Core infrastructure
    public IMediator Mediator { get; }
    
    // UI services
    public IConsoleUI Console { get; }
    public MenuService Menu { get; }
    public CharacterViewService CharacterView { get; }
    
    // Core game services
    public LevelUpService LevelUpService { get; }
    
    // Feature orchestrators (high-level workflows)
    public CharacterCreationOrchestrator CharacterCreation { get; }
    public CombatOrchestrator Combat { get; }
    public InventoryOrchestrator Inventory { get; }
    
    // Feature services (business logic)
    public SaveGameService SaveGame { get; }
    public LoadGameService LoadGame { get; }
    public CombatService CombatLogic { get; }
    public ExplorationService Exploration { get; }
    public GameplayService Gameplay { get; }
    public HallOfFameRepository HallOfFame { get; }
    
    // Shared services
    public ApocalypseTimer ApocalypseTimer { get; }
    
    // SonarQube: Suppress S107 (too many parameters) for service aggregator pattern
    // This class intentionally centralizes dependencies to reduce complexity in GameEngine
#pragma warning disable S107
    public GameEngineServices(
        IMediator mediator,
        IConsoleUI console,
        CharacterViewService characterView,
        LevelUpService levelUpService,
        CharacterCreationOrchestrator characterCreation,
        CombatOrchestrator combat,
        InventoryOrchestrator inventory,
        SaveGameService saveGame,
        LoadGameService loadGame,
        CombatService combatLogic,
        ExplorationService exploration,
        GameplayService gameplay,
        HallOfFameRepository hallOfFame,
        ApocalypseTimer apocalypseTimer,
        MenuService menu)
#pragma warning restore S107
    {
        Mediator = mediator;
        Console = console;
        CharacterView = characterView;
        LevelUpService = levelUpService;
        CharacterCreation = characterCreation;
        Combat = combat;
        Inventory = inventory;
        SaveGame = saveGame;
        LoadGame = loadGame;
        CombatLogic = combatLogic;
        Exploration = exploration;
        Gameplay = gameplay;
        HallOfFame = hallOfFame;
        ApocalypseTimer = apocalypseTimer;
        Menu = menu;
    }
}
