# 🏗️ Vertical Slice + CQRS Migration Plan

**Date**: December 8, 2024  
**Pattern**: Vertical Slice Architecture + CQRS with MediatR  
**Estimated Effort**: 16-20 hours (can be done incrementally)  
**Risk Level**: Medium (systematic refactoring with clear rollback points)

---

## � Reference Documents

- **[ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md)** ⭐ - **READ THIS FIRST** - All finalized decisions in one place
- [ORGANIZATION_AND_LAYERS_GUIDE.md](ORGANIZATION_AND_LAYERS_GUIDE.md) - Detailed layer responsibilities
- [FOLDER_STRUCTURE_ANALYSIS.md](FOLDER_STRUCTURE_ANALYSIS.md) - Feature-First vs CQRS-First comparison

---

## �📋 Table of Contents

1. [Overview](#overview)
2. [Target Architecture](#target-architecture)
3. [Migration Phases](#migration-phases)
4. [Detailed Task Breakdown](#detailed-task-breakdown)
5. [Code Examples](#code-examples)
6. [Testing Strategy](#testing-strategy)
7. [Rollback Plan](#rollback-plan)

---

## 🎯 Overview

### Current State

```
Game/
├── Services/           ❌ Mixed orchestrators and domain services
│   ├── CombatOrchestrator.cs
│   ├── CombatService.cs
│   ├── InventoryOrchestrator.cs
│   ├── InventoryService.cs
│   ├── LoadGameService.cs (orchestrator!)
│   ├── GameplayService.cs (orchestrator!)
│   └── ...
├── Models/             ✅ Domain models
├── Handlers/           ⚠️ Only event handlers (no command/query handlers)
└── GameEngine.cs       ⚠️ Thin coordinator (good!) but delegates to orchestrators
```

**Problems**:
- Services and Orchestrators mixed together
- No command/query separation
- Hard to find related code (spread across folders)
- Orchestrators handle too much (UI + coordination + some logic)

---

## ✅ Architecture Decisions (Finalized)

### Decision 1: Feature-First Organization
**Choice**: **Option B - Vertical Slice Architecture (Feature-First)**

**Rationale**:
- High feature cohesion - all Combat code in one folder
- Industry standard (Clean Architecture, Jimmy Bogard)
- Better for team collaboration
- Scales well with many features
- Matches Vertical Slice philosophy

### Decision 2: Folder Organization by Feature
**Choice**: **Case-by-case based on complexity**

| Feature | Structure | Reason |
|---------|-----------|--------|
| **Combat** | Option 3 (folder per command) | Complex, 4-5 commands, needs validators |
| **Inventory** | Option 2 (Commands/Queries subfolders) | Medium complexity, 5-6 commands |
| **Character Creation** | Option 2 (Commands/Queries subfolders) | Medium complexity, 3-4 commands |
| **Save/Load** | Option 2 (Commands/Queries subfolders) | Medium complexity, 4-5 commands |
| **Exploration** | Option 2 (Commands/Queries subfolders) | Simple, 3-4 commands |

### Decision 3: Layer Architecture
**Choice**: **Three-layer model (Orchestrator → Handler → Service)**

**Layers**:
1. **Orchestrator** - UI workflow coordination (shows menus, sends commands)
2. **Handler** - Execute ONE command/query (business logic orchestration)
3. **Service** - Reusable domain calculations (pure logic)

**When to use each**:
- **All 3 layers**: Complex features with UI workflows (Combat, Inventory, CharacterCreation)
- **Handler + Service**: Commands called by other orchestrators
- **Handler only**: Trivial operations (toggle settings, mark complete)

**Rule of Thumb**:
- If used by 2+ handlers → extract to Service
- If has UI workflow → needs Orchestrator
- Always use Handler (core of CQRS)

---

### Target State

```
Game/
├── Features/           ✨ NEW: Organized by vertical slice
│   ├── Combat/
│   │   ├── Commands/
│   │   │   ├── AttackEnemy/
│   │   │   │   ├── AttackEnemyCommand.cs
│   │   │   │   ├── AttackEnemyHandler.cs
│   │   │   │   └── AttackEnemyValidator.cs
│   │   │   ├── DefendAction/
│   │   │   ├── UseCombatItem/
│   │   │   └── FleeFromCombat/
│   │   ├── Queries/
│   │   │   ├── GetCombatState/
│   │   │   └── GetEnemyInfo/
│   │   ├── CombatOrchestrator.cs
│   │   └── CombatService.cs (domain logic)
│   ├── Inventory/
│   │   ├── Commands/
│   │   │   ├── EquipItem/
│   │   │   ├── UnequipItem/
│   │   │   ├── UseItem/
│   │   │   ├── DropItem/
│   │   │   └── SortInventory/
│   │   ├── Queries/
│   │   │   ├── GetInventoryItems/
│   │   │   ├── GetEquippedItems/
│   │   │   └── GetItemDetails/
│   │   ├── InventoryOrchestrator.cs
│   │   └── InventoryService.cs (domain logic)
│   ├── CharacterCreation/
│   │   ├── Commands/
│   │   │   ├── CreateCharacter/
│   │   │   ├── SelectClass/
│   │   │   └── AllocateAttributes/
│   │   ├── Queries/
│   │   │   ├── GetAvailableClasses/
│   │   │   └── GetStartingEquipment/
│   │   ├── CharacterCreationOrchestrator.cs
│   │   └── CharacterCreationService.cs
│   ├── SaveGame/
│   │   ├── Commands/
│   │   │   ├── CreateSave/
│   │   │   ├── LoadSave/
│   │   │   ├── DeleteSave/
│   │   │   └── AutoSave/
│   │   ├── Queries/
│   │   │   ├── GetAllSaves/
│   │   │   ├── GetSaveDetails/
│   │   │   └── GetCurrentSave/
│   │   ├── LoadGameOrchestrator.cs
│   │   └── SaveGameService.cs
│   ├── Exploration/
│   │   ├── Commands/
│   │   │   ├── ExploreLocation/
│   │   │   ├── SearchArea/
│   │   │   └── Rest/
│   │   ├── Queries/
│   │   │   ├── GetLocationInfo/
│   │   │   └── GetAvailableActions/
│   │   ├── ExplorationOrchestrator.cs
│   │   └── ExplorationService.cs
│   └── Gameplay/
│       ├── Commands/
│       │   ├── Rest/
│       │   └── SaveGame/
│       ├── GameplayOrchestrator.cs
│       └── GameplayService.cs
├── Shared/             ✨ NEW: Cross-cutting concerns
│   ├── Services/
│   │   ├── MenuService.cs
│   │   ├── CharacterViewService.cs
│   │   └── GameStateService.cs
│   ├── UI/
│   │   └── ConsoleUI.cs
│   ├── Data/
│   │   ├── Repositories/
│   │   └── Models/
│   ├── Events/         ✨ Moved from root Handlers/
│   │   ├── CharacterCreatedHandler.cs
│   │   ├── PlayerLeveledUpHandler.cs
│   │   └── ...
│   └── Behaviors/      ✨ NEW: MediatR pipeline behaviors
│       ├── LoggingBehavior.cs
│       ├── ValidationBehavior.cs
│       └── PerformanceBehavior.cs
├── Models/             ✅ Keep: Domain models
├── Generators/         ✅ Keep: Procedural generation
├── Validators/         ⚠️ Move to feature-specific folders
├── GameEngine.cs       ✅ Keep: Main coordinator
└── Program.cs          ⚠️ Update: Register commands/queries
```

**Benefits**:
- ✅ All code for a feature in one place
- ✅ Clear command/query separation
- ✅ Thin orchestrators (just UI workflow)
- ✅ Testable handlers (pure business logic)
- ✅ Easy to add new features (copy template)
- ✅ Enforced patterns via MediatR

---

## 🗺️ Target Architecture

### Layering

```
┌─────────────────────────────────────────────────┐
│              GameEngine.cs                      │  ← Coordinator
│         (Thin game loop, state machine)         │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│           Feature Orchestrators                 │  ← UI Workflows
│    (ConsoleUI calls, menu loops, MediatR)       │
│  CombatOrchestrator, InventoryOrchestrator...   │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│          Commands & Queries (MediatR)           │  ← CQRS Layer
│                                                 │
│  Commands              Queries                  │
│  ├─ AttackEnemy        ├─ GetInventoryItems     │
│  ├─ EquipItem          ├─ GetCombatState        │
│  └─ CreateCharacter    └─ GetAllSaves           │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│          Command/Query Handlers                 │  ← Business Logic
│       (Pure logic, no UI, testable)             │
│                                                 │
│  AttackEnemyHandler, EquipItemHandler...        │
│  Uses: Domain Services, Repositories            │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│            Domain Services                      │  ← Reusable Logic
│  CombatService, InventoryService, LevelUp...    │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│         Data Layer (Repositories)               │  ← Persistence
│    SaveGameRepository, CharacterRepository...   │
└─────────────────────────────────────────────────┘
```

---

## 📅 Migration Phases

We'll migrate **incrementally** to minimize risk. Each phase is a complete, testable unit.

### Phase 1: Foundation (2-3 hours)

**Goal**: Set up folder structure and move shared components

- [x] Create new folder structure
- [x] Move shared services to `Shared/`
- [x] Move event handlers to `Shared/Events/`
- [x] Create MediatR behaviors (logging, validation)
- [x] Update namespaces
- [x] Verify build

**Deliverable**: New structure in place, everything builds

---

### Phase 2: Pilot Feature - Combat (4-5 hours)

**Goal**: Fully migrate one feature as a template

**Organization**: Option 3 (folder per command) - Combat is complex enough to justify detailed structure

- [x] Create `Features/Combat/` structure with folder-per-command organization
- [x] Create combat commands (Attack, Defend, Flee, UseItem)
  - Each command in its own folder with Command/Handler/Validator/Result
- [x] Create combat queries (GetCombatState, GetEnemyInfo)
  - Each query in its own folder with Query/Handler/DTO
- [x] Implement handlers with business logic (handlers use CombatService)
- [x] Create validators for commands (FluentValidation)
- [x] Refactor CombatOrchestrator to use MediatR commands (thin UI layer)
- [x] Move CombatService to `Features/Combat/` (reusable domain logic)
- [x] Write tests for handlers (unit test each handler independently)
- [x] Update GameEngine integration
- [x] Verify combat works end-to-end

**Layer Structure**:
- Orchestrator (UI) → sends commands via MediatR
- Handlers (operations) → use CombatService for calculations
- CombatService (domain logic) → pure calculations, reusable

**Deliverable**: Combat feature fully migrated, working, tested, serves as template for other features

---

### Phase 3: Inventory Feature (3-4 hours)

**Goal**: Apply pattern to second feature

**Organization**: Option 2 (Commands/Queries subfolders) - Medium complexity, good balance

- [x] Create `Features/Inventory/` structure with Commands/ and Queries/ subfolders
- [x] Create inventory commands (Equip, Unequip, Use, Drop, Sort)
  - Command/Handler/Validator files in Commands/ folder
- [x] Create inventory queries (GetItems, GetEquipped, GetDetails)
  - Query/Handler files in Queries/ folder
- [x] Implement handlers (handlers use InventoryService for business rules)
- [x] Refactor InventoryOrchestrator to use MediatR (thin UI layer)
- [x] Move InventoryService to `Features/Inventory/` (domain logic)
- [x] Write tests for handlers
- [x] Verify inventory works

**Layer Structure**:
- Orchestrator → sends commands
- Handlers → use InventoryService
- InventoryService → equipment rules, validation logic

**Deliverable**: Inventory feature migrated with simpler folder structure than Combat

---

### Phase 4: Character Creation (2-3 hours)

**Goal**: Migrate character creation workflow

**Organization**: Option 2 (Commands/Queries subfolders) - Medium complexity, straightforward workflow

- [x] Create `Features/CharacterCreation/` structure with Commands/ and Queries/ subfolders
- [x] Create commands (CreateCharacter, SelectClass, AllocateAttributes)
  - Command/Handler/Validator files in Commands/ folder
- [x] Create queries (GetAvailableClasses, GetStartingEquipment)
  - Query/Handler files in Queries/ folder
- [x] Implement handlers (handlers use CharacterCreationService for business rules)
- [x] Refactor CharacterCreationOrchestrator to use MediatR (thin UI layer)
- [x] Move CharacterCreationService to `Features/CharacterCreation/` (domain logic)
- [x] Write tests for handlers
- [x] Verify character creation

**Layer Structure**:
- Orchestrator → sends commands
- Handlers → use CharacterCreationService
- CharacterCreationService → class rules, stat allocation logic

**Deliverable**: Character creation migrated with Option 2 structure

---

### Phase 5: Save/Load Game (2-3 hours)

**Goal**: Migrate save game operations

**Organization**: Option 2 (Commands/Queries subfolders) - Medium complexity, standard CRUD-like operations

- [x] Create `Features/SaveGame/` structure with Commands/ and Queries/ subfolders
- [x] Rename LoadGameService → LoadGameOrchestrator
- [x] Create commands (CreateSave, LoadSave, DeleteSave, AutoSave)
  - Command/Handler/Validator files in Commands/ folder
- [x] Create queries (GetAllSaves, GetSaveDetails, GetCurrentSave)
  - Query/Handler files in Queries/ folder
- [x] Implement handlers (handlers use SaveGameRepository from Shared/Data)
- [x] Refactor orchestrator to use MediatR (thin UI layer)
- [x] Keep SaveGameRepository in Shared/Data/ (cross-cutting data access)
- [x] Write tests for handlers

**Layer Structure**:
- Orchestrator → sends commands
- Handlers → use SaveGameRepository
- SaveGameRepository → LiteDB persistence (in Shared/Data/)

**Deliverable**: Save/load migrated with Option 2 structure

---

### Phase 6: Exploration & Gameplay (2 hours)

**Goal**: Migrate remaining features

**Organization**: Option 2 (Commands/Queries subfolders) - Low-medium complexity, simple operations

**Exploration**:
- [x] Create `Features/Exploration/` structure with Commands/ and Queries/ subfolders
- [x] Create commands (ExploreLocation, SearchArea, Rest)
  - Command/Handler files in Commands/ folder
- [x] Create queries (GetLocationInfo, GetAvailableActions)
  - Query/Handler files in Queries/ folder
- [x] Implement handlers (handlers use ExplorationService for location logic)
- [x] Refactor ExplorationOrchestrator to use MediatR (thin UI layer)

**Gameplay**:
- [x] Create `Features/Gameplay/` structure with Commands/ subfolder
- [x] Rename GameplayService → GameplayOrchestrator
- [x] Create gameplay commands (Rest, SaveGame, ViewStats)
- [x] Implement handlers

**Layer Structure**:
- Orchestrator → sends commands
- Handlers → use feature-specific services
- Services → location logic, stat calculations

**Deliverable**: All features migrated with Option 2 structure

---

### Phase 7: Cleanup & Documentation (1-2 hours)

**Goal**: Polish and document

- [x] Delete old `Services/` folder (everything moved)
- [x] Update README.md with new architecture
- [x] Create ARCHITECTURE.md guide
- [x] Create feature template for future features
- [x] Run full test suite
- [x] Performance testing
- [x] Update copilot-instructions.md

**Deliverable**: Clean, documented codebase

---

## 📝 Detailed Task Breakdown

### Phase 1: Foundation Setup

#### Task 1.1: Create Folder Structure

**Files to Create**:
```
Game/
├── Features/
│   └── .gitkeep
└── Shared/
    ├── Services/
    ├── UI/
    ├── Data/
    ├── Events/
    └── Behaviors/
```

**Commands**:
```powershell
# Run from c:\code\console-game\
New-Item -ItemType Directory -Path "Game\Features" -Force
New-Item -ItemType Directory -Path "Game\Shared\Services" -Force
New-Item -ItemType Directory -Path "Game\Shared\UI" -Force
New-Item -ItemType Directory -Path "Game\Shared\Data" -Force
New-Item -ItemType Directory -Path "Game\Shared\Events" -Force
New-Item -ItemType Directory -Path "Game\Shared\Behaviors" -Force
```

**Validation**: Folders exist

---

#### Task 1.2: Move Shared Services

**Move These Files**:
- `Game/Services/MenuService.cs` → `Game/Shared/Services/MenuService.cs`
- `Game/Services/CharacterViewService.cs` → `Game/Shared/Services/CharacterViewService.cs`
- `Game/Services/GameStateService.cs` → `Game/Shared/Services/GameStateService.cs`
- `Game/Services/AudioService.cs` → `Game/Shared/Services/AudioService.cs`
- `Game/Services/LoggingService.cs` → `Game/Shared/Services/LoggingService.cs`
- `Game/Services/GameDataService.cs` → `Game/Shared/Services/GameDataService.cs`

**Update Namespaces**: 
```csharp
// OLD
namespace Game.Services;

// NEW
namespace Game.Shared.Services;
```

**Commands**:
```powershell
# Move files
Move-Item "Game\Services\MenuService.cs" "Game\Shared\Services\"
Move-Item "Game\Services\CharacterViewService.cs" "Game\Shared\Services\"
Move-Item "Game\Services\GameStateService.cs" "Game\Shared\Services\"
Move-Item "Game\Services\AudioService.cs" "Game\Shared\Services\"
Move-Item "Game\Services\LoggingService.cs" "Game\Shared\Services\"
Move-Item "Game\Services\GameDataService.cs" "Game\Shared\Services\"
```

**Files to Update** (fix using statements):
- `Game/GameEngine.cs` - change `using Game.Services;` to `using Game.Shared.Services;`
- All orchestrators - update using statements
- `Game/Program.cs` - update DI registrations

**Validation**: Build succeeds, no namespace errors

---

#### Task 1.3: Move UI Components

**Move These Files**:
- `Game/UI/ConsoleUI.cs` → `Game/Shared/UI/ConsoleUI.cs`

**Update Namespace**:
```csharp
namespace Game.Shared.UI;
```

**Validation**: Build succeeds

---

#### Task 1.4: Move Event Handlers

**Move These Files**:
- `Game/Handlers/EventHandlers.cs` → `Game/Shared/Events/EventHandlers.cs`

**Update Namespace**:
```csharp
namespace Game.Shared.Events;
```

**Validation**: Build succeeds, events still fire

---

#### Task 1.5: Move Data Components

**Move These Folders**:
- `Game/Data/` → `Game/Shared/Data/` (entire folder)

**Update Namespaces**:
```csharp
// In all repository files
namespace Game.Shared.Data;
namespace Game.Shared.Data.Models;
```

**Validation**: Build succeeds, database works

---

#### Task 1.6: Create MediatR Behaviors

**File**: `Game/Shared/Behaviors/LoggingBehavior.cs`

```csharp
using MediatR;
using Serilog;
using System.Diagnostics;

namespace Game.Shared.Behaviors;

/// <summary>
/// Logs all commands and queries with timing information.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        Log.Information("Executing {RequestName}", requestName);
        Log.Debug("Request: {@Request}", request);

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            Log.Information("Completed {RequestName} in {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Failed {RequestName} after {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

**File**: `Game/Shared/Behaviors/ValidationBehavior.cs`

```csharp
using FluentValidation;
using MediatR;

namespace Game.Shared.Behaviors;

/// <summary>
/// Automatically validates commands using FluentValidation.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

**File**: `Game/Shared/Behaviors/PerformanceBehavior.cs`

```csharp
using MediatR;
using Serilog;
using System.Diagnostics;

namespace Game.Shared.Behaviors;

/// <summary>
/// Logs warnings for slow commands/queries (>500ms).
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            Log.Warning("Slow request detected: {RequestName} took {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
```

**Update**: `Game/Program.cs` - Register behaviors

```csharp
// Register MediatR with behaviors
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add pipeline behaviors (order matters!)
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
});
```

**Validation**: Build succeeds, behaviors registered

---

#### Task 1.7: Verify Phase 1

**Tests**:
```powershell
# Build
dotnet build

# Run tests
dotnet test

# Quick smoke test
dotnet run --project Game
```

**Expected**: All builds, tests pass, game runs normally

---

### Phase 2: Pilot Feature - Combat

#### Task 2.1: Create Combat Folder Structure

```
Game/Features/Combat/
├── Commands/
│   ├── AttackEnemy/
│   ├── DefendAction/
│   ├── UseCombatItem/
│   └── FleeFromCombat/
├── Queries/
│   ├── GetCombatState/
│   └── GetEnemyInfo/
├── CombatOrchestrator.cs
└── CombatService.cs
```

**Commands**:
```powershell
New-Item -ItemType Directory -Path "Game\Features\Combat\Commands\AttackEnemy" -Force
New-Item -ItemType Directory -Path "Game\Features\Combat\Commands\DefendAction" -Force
New-Item -ItemType Directory -Path "Game\Features\Combat\Commands\UseCombatItem" -Force
New-Item -ItemType Directory -Path "Game\Features\Combat\Commands\FleeFromCombat" -Force
New-Item -ItemType Directory -Path "Game\Features\Combat\Queries\GetCombatState" -Force
New-Item -ItemType Directory -Path "Game\Features\Combat\Queries\GetEnemyInfo" -Force
```

---

#### Task 2.2: Create AttackEnemy Command

**File**: `Game/Features/Combat/Commands/AttackEnemy/AttackEnemyCommand.cs`

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Combat.Commands.AttackEnemy;

/// <summary>
/// Command to execute a player attack against an enemy.
/// </summary>
public record AttackEnemyCommand : IRequest<AttackResult>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of an attack command.
/// </summary>
public record AttackResult
{
    public int Damage { get; init; }
    public bool IsCritical { get; init; }
    public bool IsEnemyDefeated { get; init; }
    public int ExperienceGained { get; init; }
    public int GoldGained { get; init; }
}
```

**File**: `Game/Features/Combat/Commands/AttackEnemy/AttackEnemyHandler.cs`

```csharp
using Game.Features.Combat;
using Game.Models;
using MediatR;
using Serilog;

namespace Game.Features.Combat.Commands.AttackEnemy;

/// <summary>
/// Handles the AttackEnemy command.
/// </summary>
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult>
{
    private readonly CombatService _combatService;
    private readonly IMediator _mediator;

    public AttackEnemyHandler(CombatService combatService, IMediator mediator)
    {
        _combatService = combatService;
        _mediator = mediator;
    }

    public async Task<AttackResult> Handle(AttackEnemyCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;
        var combatLog = request.CombatLog;

        // Calculate damage
        var (damage, isCritical) = _combatService.ExecutePlayerAttack(player, enemy);

        // Apply damage
        enemy.Health -= damage;
        
        // Log to combat log
        combatLog?.AddEntry($"{player.Name} attacks for {damage} damage" + 
            (isCritical ? " (CRITICAL!)" : ""));

        // Publish attack event
        await _mediator.Publish(new AttackPerformed(player.Name, enemy.Name, damage), cancellationToken);

        // Check if enemy defeated
        var isDefeated = enemy.Health <= 0;
        int xpGained = 0;
        int goldGained = 0;

        if (isDefeated)
        {
            xpGained = _combatService.CalculateExperienceReward(enemy);
            goldGained = _combatService.CalculateGoldReward(enemy);
            
            player.Experience += xpGained;
            player.Gold += goldGained;

            combatLog?.AddEntry($"{enemy.Name} defeated! Gained {xpGained} XP and {goldGained} gold!");

            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name), cancellationToken);
            await _mediator.Publish(new GoldGained(player.Name, goldGained), cancellationToken);
        }

        Log.Information("Player {PlayerName} attacked {EnemyName} for {Damage} damage (critical: {IsCritical})",
            player.Name, enemy.Name, damage, isCritical);

        return new AttackResult
        {
            Damage = damage,
            IsCritical = isCritical,
            IsEnemyDefeated = isDefeated,
            ExperienceGained = xpGained,
            GoldGained = goldGained
        };
    }
}
```

**File**: `Game/Features/Combat/Commands/AttackEnemy/AttackEnemyValidator.cs`

```csharp
using FluentValidation;

namespace Game.Features.Combat.Commands.AttackEnemy;

/// <summary>
/// Validates the AttackEnemy command.
/// </summary>
public class AttackEnemyValidator : AbstractValidator<AttackEnemyCommand>
{
    public AttackEnemyValidator()
    {
        RuleFor(x => x.Player)
            .NotNull().WithMessage("Player cannot be null");

        RuleFor(x => x.Player.Health)
            .GreaterThan(0).WithMessage("Player must be alive to attack");

        RuleFor(x => x.Enemy)
            .NotNull().WithMessage("Enemy cannot be null");

        RuleFor(x => x.Enemy.Health)
            .GreaterThan(0).WithMessage("Enemy must be alive to be attacked");
    }
}
```

---

#### Task 2.3: Create DefendAction Command

**File**: `Game/Features/Combat/Commands/DefendAction/DefendActionCommand.cs`

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Combat.Commands.DefendAction;

/// <summary>
/// Command to have the player defend (reduce incoming damage).
/// </summary>
public record DefendActionCommand : IRequest<DefendResult>
{
    public required Character Player { get; init; }
    public CombatLog? CombatLog { get; init; }
}

/// <summary>
/// Result of a defend action.
/// </summary>
public record DefendResult
{
    public int DefenseBonus { get; init; }
    public string Message { get; init; } = string.Empty;
}
```

**File**: `Game/Features/Combat/Commands/DefendAction/DefendActionHandler.cs`

```csharp
using MediatR;
using Serilog;

namespace Game.Features.Combat.Commands.DefendAction;

public class DefendActionHandler : IRequestHandler<DefendActionCommand, DefendResult>
{
    public Task<DefendResult> Handle(DefendActionCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var combatLog = request.CombatLog;

        // Apply temporary defense bonus (handled by CombatService in next turn)
        var defenseBonus = player.Constitution / 2;

        combatLog?.AddEntry($"{player.Name} takes a defensive stance! (Defense +{defenseBonus})");
        
        Log.Information("Player {PlayerName} defended (bonus: {DefenseBonus})", 
            player.Name, defenseBonus);

        return Task.FromResult(new DefendResult
        {
            DefenseBonus = defenseBonus,
            Message = $"You brace yourself for impact! Defense +{defenseBonus}"
        });
    }
}
```

---

#### Task 2.4: Create UseCombatItem Command

**File**: `Game/Features/Combat/Commands/UseCombatItem/UseCombatItemCommand.cs`

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Combat.Commands.UseCombatItem;

public record UseCombatItemCommand : IRequest<UseCombatItemResult>
{
    public required Character Player { get; init; }
    public required Item Item { get; init; }
    public CombatLog? CombatLog { get; init; }
}

public record UseCombatItemResult
{
    public bool Success { get; init; }
    public int HealthRestored { get; init; }
    public int ManaRestored { get; init; }
    public string Message { get; init; } = string.Empty;
}
```

**File**: `Game/Features/Combat/Commands/UseCombatItem/UseCombatItemHandler.cs`

```csharp
using MediatR;
using Serilog;

namespace Game.Features.Combat.Commands.UseCombatItem;

public class UseCombatItemHandler : IRequestHandler<UseCombatItemCommand, UseCombatItemResult>
{
    public async Task<UseCombatItemResult> Handle(UseCombatItemCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var item = request.Item;
        var combatLog = request.CombatLog;

        if (item.Type != ItemType.Consumable)
        {
            return new UseCombatItemResult
            {
                Success = false,
                Message = "Item cannot be used in combat"
            };
        }

        // Calculate healing based on rarity
        var healAmount = item.Rarity switch
        {
            ItemRarity.Common => 25,
            ItemRarity.Uncommon => 50,
            ItemRarity.Rare => 75,
            ItemRarity.Epic => 100,
            ItemRarity.Legendary => 150,
            _ => 0
        };

        var healthBefore = player.Health;
        player.Health = Math.Min(player.Health + healAmount, player.MaxHealth);
        var actualHealing = player.Health - healthBefore;

        // Remove item from inventory
        player.Inventory.Remove(item);

        combatLog?.AddEntry($"{player.Name} used {item.Name} and restored {actualHealing} health!");
        
        Log.Information("Player {PlayerName} used {ItemName} in combat (healed: {Healing})",
            player.Name, item.Name, actualHealing);

        return new UseCombatItemResult
        {
            Success = true,
            HealthRestored = actualHealing,
            Message = $"Restored {actualHealing} health!"
        };
    }
}
```

---

#### Task 2.5: Create FleeFromCombat Command

**File**: `Game/Features/Combat/Commands/FleeFromCombat/FleeFromCombatCommand.cs`

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Combat.Commands.FleeFromCombat;

public record FleeFromCombatCommand : IRequest<FleeResult>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
    public CombatLog? CombatLog { get; init; }
}

public record FleeResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
```

**File**: `Game/Features/Combat/Commands/FleeFromCombat/FleeFromCombatHandler.cs`

```csharp
using Game.Features.Combat;
using MediatR;
using Serilog;

namespace Game.Features.Combat.Commands.FleeFromCombat;

public class FleeFromCombatHandler : IRequestHandler<FleeFromCombatCommand, FleeResult>
{
    private readonly CombatService _combatService;

    public FleeFromCombatHandler(CombatService combatService)
    {
        _combatService = combatService;
    }

    public Task<FleeResult> Handle(FleeFromCombatCommand request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;
        var combatLog = request.CombatLog;

        var fleeChance = _combatService.CalculateFleeChance(player, enemy);
        var success = new Random().NextDouble() < fleeChance;

        if (success)
        {
            combatLog?.AddEntry($"{player.Name} successfully fled from {enemy.Name}!");
            Log.Information("Player {PlayerName} fled from {EnemyName}", player.Name, enemy.Name);
            
            return Task.FromResult(new FleeResult
            {
                Success = true,
                Message = "You successfully escaped!"
            });
        }
        else
        {
            combatLog?.AddEntry($"{player.Name} failed to escape!");
            Log.Information("Player {PlayerName} failed to flee from {EnemyName}", player.Name, enemy.Name);
            
            return Task.FromResult(new FleeResult
            {
                Success = false,
                Message = "You couldn't escape!"
            });
        }
    }
}
```

---

#### Task 2.6: Create Combat Queries

**File**: `Game/Features/Combat/Queries/GetCombatState/GetCombatStateQuery.cs`

```csharp
using Game.Models;
using MediatR;

namespace Game.Features.Combat.Queries.GetCombatState;

public record GetCombatStateQuery : IRequest<CombatState>
{
    public required Character Player { get; init; }
    public required Enemy Enemy { get; init; }
}

public record CombatState
{
    public int PlayerHealthPercentage { get; init; }
    public int EnemyHealthPercentage { get; init; }
    public bool PlayerCanFlee { get; init; }
    public bool PlayerHasItems { get; init; }
    public List<string> AvailableActions { get; init; } = new();
}
```

**File**: `Game/Features/Combat/Queries/GetCombatState/GetCombatStateHandler.cs`

```csharp
using MediatR;

namespace Game.Features.Combat.Queries.GetCombatState;

public class GetCombatStateHandler : IRequestHandler<GetCombatStateQuery, CombatState>
{
    public Task<CombatState> Handle(GetCombatStateQuery request, CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;

        var state = new CombatState
        {
            PlayerHealthPercentage = (int)((double)player.Health / player.MaxHealth * 100),
            EnemyHealthPercentage = (int)((double)enemy.Health / enemy.MaxHealth * 100),
            PlayerCanFlee = true, // Always can attempt
            PlayerHasItems = player.Inventory.Any(i => i.Type == ItemType.Consumable),
            AvailableActions = new List<string> { "Attack", "Defend", "Use Item", "Flee" }
        };

        return Task.FromResult(state);
    }
}
```

---

#### Task 2.7: Move CombatService and CombatOrchestrator

**Move Files**:
```powershell
Move-Item "Game\Services\CombatService.cs" "Game\Features\Combat\"
Move-Item "Game\Services\CombatOrchestrator.cs" "Game\Features\Combat\"
```

**Update Namespaces**:
```csharp
namespace Game.Features.Combat;
```

**Update CombatOrchestrator** to use commands:

```csharp
// OLD (direct service calls)
var (damage, isCritical) = _combatService.ExecutePlayerAttack(player, enemy);
enemy.Health -= damage;
// ... more logic

// NEW (send command)
var result = await _mediator.Send(new AttackEnemyCommand 
{ 
    Player = player, 
    Enemy = enemy, 
    CombatLog = combatLog 
});

ConsoleUI.ShowSuccess($"You dealt {result.Damage} damage!" + 
    (result.IsCritical ? " CRITICAL HIT!" : ""));

if (result.IsEnemyDefeated)
{
    ConsoleUI.ShowInfo($"Enemy defeated! +{result.ExperienceGained} XP, +{result.GoldGained} gold");
}
```

---

#### Task 2.8: Update Program.cs

**Register Combat Commands**:
```csharp
// In Program.cs - these are auto-discovered by MediatR!
// Just need to register validators
services.AddSingleton<IValidator<AttackEnemyCommand>, AttackEnemyValidator>();
```

---

#### Task 2.9: Write Tests for Combat

**File**: `Game.Tests/Features/Combat/Commands/AttackEnemyHandlerTests.cs`

```csharp
using FluentAssertions;
using Game.Features.Combat;
using Game.Features.Combat.Commands.AttackEnemy;
using Game.Models;
using MediatR;
using Moq;
using Xunit;

namespace Game.Tests.Features.Combat.Commands;

public class AttackEnemyHandlerTests
{
    private readonly Mock<CombatService> _combatServiceMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AttackEnemyHandler _handler;

    public AttackEnemyHandlerTests()
    {
        _combatServiceMock = new Mock<CombatService>();
        _mediatorMock = new Mock<IMediator>();
        _handler = new AttackEnemyHandler(_combatServiceMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Deal_Damage_To_Enemy()
    {
        // Arrange
        var player = new Character { Name = "Hero", Strength = 10 };
        var enemy = new Enemy { Name = "Goblin", Health = 50, MaxHealth = 50 };
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy))
            .Returns((20, false)); // 20 damage, not critical

        var command = new AttackEnemyCommand { Player = player, Enemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Damage.Should().Be(20);
        result.IsCritical.Should().BeFalse();
        result.IsEnemyDefeated.Should().BeFalse();
        enemy.Health.Should().Be(30);
    }

    [Fact]
    public async Task Handle_Should_Defeat_Enemy_When_Health_Reaches_Zero()
    {
        // Arrange
        var player = new Character { Name = "Hero", Strength = 20, Experience = 0, Gold = 0 };
        var enemy = new Enemy { Name = "Goblin", Health = 10, MaxHealth = 50 };
        
        _combatServiceMock
            .Setup(x => x.ExecutePlayerAttack(player, enemy))
            .Returns((15, false));
        
        _combatServiceMock
            .Setup(x => x.CalculateExperienceReward(enemy))
            .Returns(50);
        
        _combatServiceMock
            .Setup(x => x.CalculateGoldReward(enemy))
            .Returns(25);

        var command = new AttackEnemyCommand { Player = player, Enemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsEnemyDefeated.Should().BeTrue();
        result.ExperienceGained.Should().Be(50);
        result.GoldGained.Should().Be(25);
        player.Experience.Should().Be(50);
        player.Gold.Should().Be(25);
    }
}
```

---

#### Task 2.10: Verify Phase 2

**Tests**:
```powershell
dotnet build
dotnet test --filter "FullyQualifiedName~Combat"
dotnet run --project Game
```

**Manual Test**: Start game, get into combat, verify:
- Attack works
- Defend works
- Use item works
- Flee works
- Logging shows commands being executed

---

### Phase 3-6: Repeat Pattern

The remaining phases follow the same pattern as Phase 2:

1. Create folder structure
2. Create commands and queries
3. Implement handlers
4. Create validators
5. Refactor orchestrator to use commands
6. Move services to feature folder
7. Write tests
8. Verify

I'll provide templates for each feature in separate sections below.

---

## 💡 Code Examples

### Template: New Feature

When creating a new feature, copy this structure:

```
Game/Features/MyFeature/
├── Commands/
│   └── MyCommand/
│       ├── MyCommand.cs
│       ├── MyCommandHandler.cs
│       └── MyCommandValidator.cs
├── Queries/
│   └── MyQuery/
│       ├── MyQuery.cs
│       └── MyQueryHandler.cs
├── MyFeatureOrchestrator.cs
└── MyFeatureService.cs (if needed)
```

**Command Template**:
```csharp
using MediatR;

namespace Game.Features.MyFeature.Commands.MyCommand;

public record MyCommand : IRequest<MyResult>
{
    // Properties
}

public record MyResult
{
    // Result properties
}
```

**Handler Template**:
```csharp
using MediatR;

namespace Game.Features.MyFeature.Commands.MyCommand;

public class MyCommandHandler : IRequestHandler<MyCommand, MyResult>
{
    // Dependencies via constructor

    public async Task<MyResult> Handle(MyCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
        
        // Publish events if needed
        await _mediator.Publish(new SomethingHappened(...), cancellationToken);
        
        return new MyResult { ... };
    }
}
```

**Validator Template**:
```csharp
using FluentValidation;

namespace Game.Features.MyFeature.Commands.MyCommand;

public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.SomeProperty)
            .NotNull()
            .WithMessage("SomeProperty is required");
    }
}
```

---

## 🧪 Testing Strategy

### Unit Tests

**Test Handlers** (not orchestrators):
- Handlers contain business logic → test them
- Orchestrators are thin UI → integration test or skip

**Example**:
```csharp
[Fact]
public async Task EquipItem_Should_Apply_Stat_Bonuses()
{
    // Arrange
    var player = new Character { Strength = 10 };
    var sword = new Item { Type = ItemType.Weapon, BonusStrength = 5 };
    var handler = new EquipItemHandler(_inventoryService);
    
    // Act
    var result = await handler.Handle(new EquipItemCommand { Player = player, Item = sword }, default);
    
    // Assert
    result.Success.Should().BeTrue();
    player.Strength.Should().Be(15);
}
```

### Integration Tests

Test full workflows:
```csharp
[Fact]
public async Task Combat_Should_Award_XP_And_Level_Up()
{
    // Arrange: Player at 90 XP, needs 100 for level 2
    // Act: Kill enemy worth 20 XP
    // Assert: Player is now level 2
}
```

---

## 🔄 Rollback Plan

Each phase is independent and can be rolled back:

**Phase 1**: 
- Delete `Shared/` folder
- Restore `Services/` and `Handlers/` from git
- `git checkout HEAD -- Game/Services Game/Handlers`

**Phase 2-6**:
- Delete `Features/MyFeature/` folder
- Restore old orchestrator/service from git
- `git checkout HEAD -- Game/Services/MyOrchestrator.cs`

**Full Rollback**:
```powershell
git stash
git checkout main
```

---

## 📊 Migration Checklist

### Pre-Migration

- [x] Review architecture document
- [x] Understand CQRS pattern
- [x] Understand Vertical Slices
- [x] Create git branch: `git checkout -b feature/vertical-slice-migration`
- [x] Ensure all tests pass
- [x] Backup database: `Copy-Item savegames.db savegames.backup.db`

### Phase 1: Foundation

- [ ] Create folder structure
- [ ] Move shared services to `Shared/Services/`
- [ ] Move UI to `Shared/UI/`
- [ ] Move event handlers to `Shared/Events/`
- [ ] Move data to `Shared/Data/`
- [ ] Create MediatR behaviors
- [ ] Update Program.cs
- [ ] Fix all namespaces
- [ ] Build succeeds
- [ ] Tests pass
- [ ] Commit: `git commit -m "Phase 1: Foundation structure"`

### Phase 2: Combat

- [ ] Create `Features/Combat/` structure
- [ ] Create AttackEnemy command + handler + validator
- [ ] Create DefendAction command + handler
- [ ] Create UseCombatItem command + handler
- [ ] Create FleeFromCombat command + handler
- [ ] Create GetCombatState query + handler
- [ ] Move CombatService to `Features/Combat/`
- [ ] Move CombatOrchestrator to `Features/Combat/`
- [ ] Refactor orchestrator to use commands
- [ ] Write handler tests
- [ ] Build succeeds
- [ ] Tests pass
- [ ] Manual test combat
- [ ] Commit: `git commit -m "Phase 2: Combat feature migrated"`

### Phase 3: Inventory

- [ ] Create `Features/Inventory/` structure
- [ ] Create EquipItem command + handler + validator
- [ ] Create UnequipItem command + handler
- [ ] Create UseItem command + handler
- [ ] Create DropItem command + handler
- [ ] Create SortInventory command + handler
- [ ] Create GetInventoryItems query + handler
- [ ] Create GetItemDetails query + handler
- [ ] Move InventoryService to `Features/Inventory/`
- [ ] Move InventoryOrchestrator to `Features/Inventory/`
- [ ] Refactor orchestrator
- [ ] Write tests
- [ ] Verify inventory works
- [ ] Commit: `git commit -m "Phase 3: Inventory feature migrated"`

### Phase 4: Character Creation

- [ ] Create `Features/CharacterCreation/` structure
- [ ] Create CreateCharacter command + handler + validator
- [ ] Create SelectClass command + handler
- [ ] Create AllocateAttributes command + handler + validator
- [ ] Create GetAvailableClasses query + handler
- [ ] Move CharacterCreationService to `Features/CharacterCreation/`
- [ ] Move CharacterCreationOrchestrator to `Features/CharacterCreation/`
- [ ] Refactor orchestrator
- [ ] Write tests
- [ ] Verify character creation
- [ ] Commit: `git commit -m "Phase 4: Character creation migrated"`

### Phase 5: Save/Load

- [ ] Create `Features/SaveGame/` structure
- [ ] Rename LoadGameService → LoadGameOrchestrator
- [ ] Create CreateSave command + handler
- [ ] Create LoadSave command + handler
- [ ] Create DeleteSave command + handler
- [ ] Create AutoSave command + handler
- [ ] Create GetAllSaves query + handler
- [ ] Create GetSaveDetails query + handler
- [ ] Move SaveGameService to `Features/SaveGame/` (as repository)
- [ ] Refactor orchestrator
- [ ] Write tests
- [ ] Verify save/load
- [ ] Commit: `git commit -m "Phase 5: Save/Load migrated"`

### Phase 6: Exploration & Gameplay

- [ ] Create `Features/Exploration/` structure
- [ ] Create ExploreLocation command + handler
- [ ] Create SearchArea command + handler
- [ ] Create Rest command + handler
- [ ] Move ExplorationService to `Features/Exploration/`
- [ ] Create `Features/Gameplay/` structure
- [ ] Rename GameplayService → GameplayOrchestrator
- [ ] Create gameplay commands
- [ ] Write tests
- [ ] Verify exploration
- [ ] Commit: `git commit -m "Phase 6: Exploration & Gameplay migrated"`

### Phase 7: Cleanup

- [ ] Delete empty `Services/` folder
- [ ] Delete empty `Handlers/` folder
- [ ] Update README.md
- [ ] Create ARCHITECTURE.md
- [ ] Create feature template in docs/
- [ ] Update copilot-instructions.md
- [ ] Run full test suite
- [ ] Performance test (should be same or better)
- [ ] Code review
- [ ] Commit: `git commit -m "Phase 7: Cleanup and documentation"`
- [ ] Merge to main: `git checkout main && git merge feature/vertical-slice-migration`

---

## 📚 Documentation Updates

### New Files to Create

1. **`docs/ARCHITECTURE.md`** - Architecture overview
2. **`docs/FEATURE_TEMPLATE.md`** - Template for new features
3. **`docs/COMMAND_QUERY_GUIDE.md`** - When to use commands vs queries
4. **Update `README.md`** - Reflect new structure
5. **Update `.github/copilot-instructions.md`** - New patterns

---

## ⏱️ Time Estimates

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| **Phase 1: Foundation** | Setup structure, move shared code | 2-3 hours |
| **Phase 2: Combat** | Full combat migration with tests | 4-5 hours |
| **Phase 3: Inventory** | Full inventory migration with tests | 3-4 hours |
| **Phase 4: Character Creation** | Character creation migration | 2-3 hours |
| **Phase 5: Save/Load** | Save game migration | 2-3 hours |
| **Phase 6: Exploration** | Final features migration | 2 hours |
| **Phase 7: Cleanup** | Documentation, polish | 1-2 hours |
| **TOTAL** | | **16-22 hours** |

Can be done over **2-3 days** working 6-8 hours per day.

---

## ✅ Success Criteria

**Phase Completion**:
- ✅ All code compiles without errors
- ✅ All existing tests pass
- ✅ New handler tests added and passing
- ✅ Manual testing confirms feature works
- ✅ Git commit made

**Final Success**:
- ✅ All features migrated
- ✅ 375+ tests passing (adding handler tests)
- ✅ Game runs normally (no regressions)
- ✅ Code is cleaner and more maintainable
- ✅ Clear architecture documented
- ✅ Performance same or better

---

## 🎯 Next Steps

**Ready to start?** Let's begin with Phase 1!

I recommend:

1. **Review this plan** - any questions?
2. **Create git branch** - `git checkout -b feature/vertical-slice-migration`
3. **Start Phase 1** - I'll guide you through each task
4. **Commit frequently** - after each phase

Should I start with Phase 1, Task 1.1? 🚀
