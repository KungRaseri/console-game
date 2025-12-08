# GameEngine Modularization Plan

## Overview
The GameEngine.cs file is currently ~1,900 lines with 50+ methods. This document outlines the strategy for extracting functionality into specialized services to improve maintainability, testability, and scalability.

## Current State

### Existing Services ✅
- `SaveGameService` - Save/load game operations
- `CombatService` - Combat mechanics
- `CharacterCreationService` - Character creation logic (static)
- `InventoryService` - Inventory management
- `LevelUpService` - Level-up point allocation
- `GameStateService` - Centralized game state access
- `AudioService` - Audio playback
- `GameDataService` - Data loading
- `LoggingService` - Logging configuration

### New Services Created 🆕
- **MenuService** - All menu navigation and display
- **ExplorationService** - Location tracking, travel, exploration events
- **CharacterViewService** - Character stat display (static utility)

## Extraction Strategy

### Phase 1: Extract High-Level Services ✅ COMPLETED
1. **MenuService** ✅
   - Main menu navigation
   - In-game menu
   - Combat menu
   - Inventory menu
   - Pause menu
   - Item selection menus

2. **ExplorationService** ✅
   - Exploration logic
   - Travel between locations
   - Random encounters
   - Treasure finding

3. **CharacterViewService** ✅
   - Character stat display
   - Equipment display
   - Character summary/review

### Phase 2: Refactor GameEngine (IN PROGRESS)
1. **Update GameEngine constructor** to use dependency injection
   - Inject all services
   - Remove direct instantiation
   - Configure with proper DI container

2. **Replace direct method calls with service calls**
   - `HandleMainMenuAsync()` → `MenuService.HandleMainMenu()`
   - `HandleInGameAsync()` → Use `MenuService.ShowInGameMenu()`
   - `ExploreAsync()` → `ExplorationService.ExploreAsync()`
   - `ViewCharacterAsync()` → `CharacterViewService.ViewCharacter()`
   - `TravelToLocation()` → `ExplorationService.TravelToLocation()`

3. **Move state management to GameStateService**
   - Location tracking (already done)
   - Current save ID management
   - Combat log management

### Phase 3: Further Extraction (FUTURE)
1. **LoadGameService** - Extract save loading/deletion logic
2. **CharacterCreationOrchestrator** - Coordinate character creation flow
3. **CombatOrchestrator** - High-level combat flow coordination
4. **InventoryOrchestrator** - High-level inventory operations

## Service Responsibilities

### MenuService
**Purpose**: Centralize all menu display and user choice handling

**Methods**:
- `HandleMainMenu()` - Display main menu, return next state
- `ShowInGameMenu()` - Display in-game menu, return choice
- `ShowCombatMenu()` - Display combat action menu
- `ShowInventoryMenu()` - Display inventory menu
- `HandlePauseMenu()` - Display pause menu, return next state
- `SelectItemFromList()` - Generic item selection
- `ShowRingSlotMenu()` - Ring slot selection

**Benefits**:
- All menu logic in one place
- Easy to add new menus
- Consistent menu styling
- Testable menu behavior

### ExplorationService
**Purpose**: Handle all world exploration and location-based gameplay

**Methods**:
- `ExploreAsync()` - Perform exploration, return combat flag
- `TravelToLocation()` - Travel between known locations
- `GetKnownLocations()` - Get list of discoverable locations

**Dependencies**:
- `IMediator` - For publishing events
- `GameStateService` - For location tracking
- `SaveGameService` - For visited locations

**Benefits**:
- Isolated exploration mechanics
- Easy to add new locations
- Testable encounter probabilities
- Reusable for different game modes

### CharacterViewService
**Purpose**: Display character information in various contexts

**Methods**:
- `ViewCharacter()` - Full character screen with all stats
- `ReviewCharacter()` - Character creation summary
- `GetEquipmentDisplay()` - Equipment and set bonuses
- `GetRarityColor()` - Color coding utility

**Benefits**:
- Consistent character display
- Static utility (no state)
- Easy to extend with new stat displays
- Shared across multiple views

### GameStateService (Enhanced)
**Purpose**: Centralized access to game state

**Current Methods**:
- `CurrentSave` - Get active save
- `Player` - Get player character
- `DifficultyLevel` - Get difficulty
- `IsIronmanMode` - Check ironman status
- `UpdateLocation()` - Update and track location
- `RecordDeath()` - Record death event

**Future Additions**:
- `CurrentSaveId` - Track current save ID
- `CombatLog` - Track combat log
- `IsInCombat` - Combat state flag

## Dependency Injection Setup

### Program.cs Configuration
```csharp
var services = new ServiceCollection();

// Core services
services.AddSingleton<IMediator, Mediator>();
services.AddSingleton<SaveGameService>();
services.AddSingleton<GameStateService>();

// Game systems
services.AddSingleton<CombatService>();
services.AddTransient<MenuService>();
services.AddTransient<ExplorationService>();

// Engine
services.AddSingleton<GameEngine>();

var serviceProvider = services.BuildServiceProvider();
var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
await gameEngine.RunAsync();
```

### GameEngine Constructor (Refactored)
```csharp
public GameEngine(
    IMediator mediator,
    SaveGameService saveGameService,
    GameStateService gameState,
    CombatService combatService,
    MenuService menuService,
    ExplorationService explorationService)
{
    _mediator = mediator;
    _saveGameService = saveGameService;
    _gameState = gameState;
    _combatService = combatService;
    _menuService = menuService;
    _explorationService = explorationService;
    // ... initialization
}
```

## Benefits of Modularization

### 1. **Maintainability** ⭐⭐⭐⭐⭐
- **Before**: 1,900 line file, hard to navigate
- **After**: Services ~150-300 lines each, single responsibility

### 2. **Testability** ⭐⭐⭐⭐⭐
- **Before**: Hard to test game loop without running entire engine
- **After**: Test `ExplorationService.ExploreAsync()` in isolation

### 3. **Collaboration** ⭐⭐⭐⭐
- **Before**: Merge conflicts on `GameEngine.cs`
- **After**: Work on `MenuService` vs `CombatService` independently

### 4. **Reusability** ⭐⭐⭐⭐
- **Before**: Combat logic tied to GameEngine
- **After**: `CombatService` reusable for arena, PvP, etc.

### 5. **Scalability** ⭐⭐⭐⭐⭐
- **Before**: Adding new features bloats `GameEngine`
- **After**: New `ShopService`, `QuestService`, `CraftingService`

## Migration Checklist

### Completed ✅
- [x] Create `MenuService`
- [x] Create `ExplorationService`
- [x] Create `CharacterViewService`
- [x] Update `GameStateService` with `UpdateLocation()`

### In Progress 🔄
- [ ] Update `GameEngine` to use services
- [ ] Replace all menu logic with `MenuService` calls
- [ ] Replace exploration logic with `ExplorationService` calls
- [ ] Replace character view logic with `CharacterViewService` calls

### Future 📋
- [ ] Set up Dependency Injection in `Program.cs`
- [ ] Extract save loading logic to `LoadGameService`
- [ ] Create `CharacterCreationOrchestrator`
- [ ] Create `CombatOrchestrator`
- [ ] Create `InventoryOrchestrator`
- [ ] Write unit tests for all services
- [ ] Update documentation

## File Size Reduction Estimate

| File | Current Lines | Target Lines | Reduction |
|------|---------------|--------------|-----------|
| GameEngine.cs | 1,912 | ~800 | -58% |
| MenuService.cs | N/A | ~200 | NEW |
| ExplorationService.cs | N/A | ~150 | NEW |
| CharacterViewService.cs | N/A | ~250 | NEW |

**Total**: 1,912 lines → 1,400 lines across 4 files (+600 lines of new code, but better organized)

## Testing Strategy

### MenuService Tests
```csharp
[Fact]
public void ShowInGameMenu_Should_Include_LevelUp_When_Points_Available()
{
    // Arrange
    var player = new Character { UnspentAttributePoints = 3 };
    
    // Act
    var menu = menuService.ShowInGameMenu(player);
    
    // Assert
    menu.Should().Contain("Level Up");
}
```

### ExplorationService Tests
```csharp
[Fact]
public async Task ExploreAsync_Should_Grant_XP_On_Peaceful_Outcome()
{
    // Arrange
    var initialXP = player.Experience;
    
    // Act
    await explorationService.ExploreAsync();
    
    // Assert
    player.Experience.Should().BeGreaterThan(initialXP);
}
```

### CharacterViewService Tests
```csharp
[Fact]
public void GetEquipmentDisplay_Should_Show_Active_Set_Bonuses()
{
    // Arrange
    var player = CreatePlayerWithFullSet("Berserker's Rage");
    
    // Act
    var display = CharacterViewService.GetEquipmentDisplay(player);
    
    // Assert
    display.Should().Contain("Active Equipment Sets");
    display.Should().Contain("Berserker's Rage");
}
```

## Next Steps

1. ✅ **Create new services** - DONE
2. **Refactor GameEngine** - Use services instead of local methods
3. **Set up DI** - Configure dependency injection
4. **Add tests** - Unit tests for each service
5. **Update docs** - Reflect new architecture

## Notes

- Keep `GameEngine` as the **orchestrator** - it coordinates services but doesn't implement logic
- Services should be **stateless where possible** or manage their own state
- Use `IMediator` for cross-service communication (events)
- Consider adding **interfaces** (e.g., `IMenuService`) for better testability
- Performance is not a concern - readability and maintainability are priorities

---

**Status**: Phase 1 Complete ✅ | Phase 2 In Progress 🔄
**Last Updated**: December 8, 2025
