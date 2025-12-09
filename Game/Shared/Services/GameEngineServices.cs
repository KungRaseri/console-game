using Game.Features.CharacterCreation;
using Game.Features.Combat;
using Game.Features.Exploration;
using Game.Features.Inventory;
using Game.Features.SaveLoad;
using Game.Features.Death;
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
    public HallOfFameService HallOfFame { get; }
    
    // Shared services
    public ApocalypseTimer ApocalypseTimer { get; }
    
    // UI services
    public MenuService Menu { get; }
    
    // SonarQube: Suppress S107 (too many parameters) for service aggregator pattern
    // This class intentionally centralizes dependencies to reduce complexity in GameEngine
#pragma warning disable S107
    public GameEngineServices(
        IMediator mediator,
        CharacterCreationOrchestrator characterCreation,
        CombatOrchestrator combat,
        InventoryOrchestrator inventory,
        SaveGameService saveGame,
        LoadGameService loadGame,
        CombatService combatLogic,
        ExplorationService exploration,
        GameplayService gameplay,
        HallOfFameService hallOfFame,
        ApocalypseTimer apocalypseTimer,
        MenuService menu)
#pragma warning restore S107
    {
        Mediator = mediator;
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
