# 🎯 Architecture Decisions - Vertical Slice + CQRS Migration

**Date**: December 8, 2024  
**Status**: ✅ **FINALIZED**  
**Migration Plan**: `VERTICAL_SLICE_CQRS_MIGRATION_PLAN.md`

---

## 📋 Summary of Decisions

This document captures the architectural decisions made for the Vertical Slice + CQRS migration. All implementation MUST follow these guidelines for consistency.

---

## Decision 1: Feature-First Organization (Vertical Slices)

**Choice**: ✅ **Option B - Feature-First (Vertical Slice Architecture)**

**Structure**:
```
Game/Features/
├── Combat/          ← All Combat code here
├── Inventory/       ← All Inventory code here
├── CharacterCreation/
└── SaveGame/
```

**NOT**:
```
Game/
├── Commands/        ❌ CQRS-First (rejected)
├── Queries/         ❌ Horizontal slicing (rejected)
└── Features/
```

**Rationale**:
- ✅ High feature cohesion - all related code together
- ✅ Industry standard (Clean Architecture, Vertical Slice Architecture)
- ✅ Better for team collaboration (no merge conflicts)
- ✅ Easier to find code (one folder per feature)
- ✅ Clean feature deletion (just delete folder)
- ✅ Scales well (linear growth vs 3× explosion)

**References**:
- `docs/FOLDER_STRUCTURE_ANALYSIS.md` - Full comparison (10/12 criteria favored Feature-First)
- Jimmy Bogard's Vertical Slice Architecture
- Microsoft eShopOnContainers pattern
- Clean Architecture (Uncle Bob)

---

## Decision 2: Folder Organization by Feature Complexity

**Choice**: ✅ **Case-by-case basis (pragmatic approach)**

### Organization Matrix

| Feature | Complexity | Commands | Queries | Structure | Reason |
|---------|-----------|----------|---------|-----------|--------|
| **Combat** | High | 4-5 | 2-3 | **Option 3** (folder per command) | Complex logic, validators, DTOs needed |
| **Inventory** | Medium | 5-6 | 2-3 | **Option 2** (Commands/Queries subfolders) | Good balance, not overly complex |
| **Character Creation** | Medium | 3-4 | 2 | **Option 2** (Commands/Queries subfolders) | Straightforward workflow |
| **Save/Load** | Medium | 4-5 | 3-4 | **Option 2** (Commands/Queries subfolders) | Standard CRUD-like operations |
| **Exploration** | Low | 3-4 | 2 | **Option 2** (Commands/Queries subfolders) | Simple operations |

### Option 2: Commands/Queries Subfolders (Default)

**Use for**: Most features (medium complexity)

```
Features/Inventory/
├── Commands/
│   ├── EquipItemCommand.cs
│   ├── EquipItemHandler.cs
│   ├── EquipItemValidator.cs
│   ├── UseItemCommand.cs
│   ├── UseItemHandler.cs
│   └── DropItemCommand.cs
├── Queries/
│   ├── GetInventoryQuery.cs
│   ├── GetInventoryHandler.cs
│   ├── GetItemDetailsQuery.cs
│   └── GetItemDetailsHandler.cs
├── InventoryOrchestrator.cs
└── InventoryService.cs
```

**Namespaces**:
```csharp
namespace Game.Features.Inventory.Commands;
namespace Game.Features.Inventory.Queries;
```

---

### Option 3: Folder Per Command (Complex Features)

**Use for**: Combat (pilot feature), complex features with validators/DTOs

```
Features/Combat/
├── Commands/
│   ├── AttackEnemy/
│   │   ├── AttackEnemyCommand.cs
│   │   ├── AttackEnemyHandler.cs
│   │   ├── AttackEnemyValidator.cs
│   │   └── AttackResult.cs
│   ├── DefendAction/
│   │   ├── DefendActionCommand.cs
│   │   ├── DefendActionHandler.cs
│   │   └── DefendResult.cs
│   └── UseCombatItem/
│       ├── UseCombatItemCommand.cs
│       ├── UseCombatItemHandler.cs
│       └── UseCombatItemResult.cs
├── Queries/
│   ├── GetCombatState/
│   │   ├── GetCombatStateQuery.cs
│   │   ├── GetCombatStateHandler.cs
│   │   └── CombatState.cs
│   └── GetEnemyInfo/
│       ├── GetEnemyInfoQuery.cs
│       ├── GetEnemyInfoHandler.cs
│       └── EnemyInfo.cs
├── CombatOrchestrator.cs
└── CombatService.cs
```

**Namespaces**:
```csharp
namespace Game.Features.Combat.Commands.AttackEnemy;
namespace Game.Features.Combat.Queries.GetCombatState;
```

**When to use**:
- ✅ Command needs validator
- ✅ Command needs result DTO
- ✅ Feature has 30+ operations
- ✅ Maximum clarity desired

---

## Decision 3: Three-Layer Architecture

**Choice**: ✅ **Orchestrator → Handler → Service**

### Layer Definitions

```
┌─────────────────────────────────┐
│     Orchestrator (UI Layer)      │  ← Shows menus, gets input, sends commands
└─────────────────────────────────┘
              ↓ MediatR.Send()
┌─────────────────────────────────┐
│     Handler (CQRS Layer)         │  ← Executes ONE operation, publishes events
└─────────────────────────────────┘
              ↓ uses
┌─────────────────────────────────┐
│     Service (Domain Layer)       │  ← Pure calculations, reusable logic
└─────────────────────────────────┘
```

---

### Layer 1: Orchestrator

**Purpose**: UI workflow coordination

**File**: `{Feature}Orchestrator.cs` (e.g., `CombatOrchestrator.cs`)

**Responsibilities**:
- ✅ Display UI (ConsoleUI calls)
- ✅ Show menus and get user input (MenuService)
- ✅ Send commands/queries via MediatR
- ✅ Coordinate multi-step workflows
- ✅ Handle menu loops and UI state

**Must NOT**:
- ❌ Contain business logic
- ❌ Calculate damage, stats, etc.
- ❌ Directly modify domain models
- ❌ Publish domain events (handlers do that)

**Example**:
```csharp
public class CombatOrchestrator
{
    private readonly IMediator _mediator;
    private readonly MenuService _menuService;

    public async Task RunCombatAsync(Character player, Enemy enemy)
    {
        while (inCombat)
        {
            // 1. Display UI
            ConsoleUI.ShowPanel("Combat", $"{player.Name} vs {enemy.Name}");
            
            // 2. Get user choice
            var choice = _menuService.ShowMenu("Choose action:", "Attack", "Defend");
            
            // 3. Send command
            var result = await _mediator.Send(new AttackEnemyCommand 
            { 
                Player = player, 
                Enemy = enemy 
            });
            
            // 4. Display result
            ConsoleUI.ShowSuccess($"You dealt {result.Damage} damage!");
        }
    }
}
```

---

### Layer 2: Handler

**Purpose**: Execute ONE command or query

**File**: `{CommandName}Handler.cs` (e.g., `AttackEnemyHandler.cs`)

**Responsibilities**:
- ✅ Execute ONE operation (single responsibility)
- ✅ Use services for calculations
- ✅ Apply business rules
- ✅ Modify domain models (apply results)
- ✅ Publish domain events via MediatR
- ✅ Return structured results

**Must NOT**:
- ❌ Display UI (no ConsoleUI calls)
- ❌ Show menus
- ❌ Contain reusable calculations (delegate to service)
- ❌ Handle multiple operations

**Example**:
```csharp
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult>
{
    private readonly CombatService _combatService;
    private readonly IMediator _mediator;

    public async Task<AttackResult> Handle(AttackEnemyCommand request, ...)
    {
        // 1. Use service for calculation
        var (damage, isCritical) = _combatService.CalculateDamage(
            request.Player.Strength,
            request.Enemy.Defense
        );
        
        // 2. Apply result
        request.Enemy.Health -= damage;
        
        // 3. Publish event
        await _mediator.Publish(new AttackPerformed(
            request.Player.Name, 
            request.Enemy.Name, 
            damage
        ));
        
        // 4. Check defeat
        if (request.Enemy.Health <= 0)
        {
            var xp = _combatService.CalculateExperienceReward(request.Enemy.Level);
            request.Player.Experience += xp;
            await _mediator.Publish(new EnemyDefeated(...));
        }
        
        // 5. Return result
        return new AttackResult 
        { 
            Damage = damage, 
            IsCritical = isCritical 
        };
    }
}
```

---

### Layer 3: Service

**Purpose**: Reusable domain logic

**File**: `{Feature}Service.cs` (e.g., `CombatService.cs`)

**Responsibilities**:
- ✅ Pure calculations (no side effects)
- ✅ Domain rules enforcement
- ✅ Reusable across handlers
- ✅ Stateless (or manages specific state like Random)

**Must NOT**:
- ❌ Display UI
- ❌ Publish events
- ❌ Modify domain models directly (return values instead)
- ❌ Call MediatR

**Example**:
```csharp
public class CombatService
{
    private readonly Random _random = new();

    /// <summary>
    /// Pure calculation - no side effects.
    /// </summary>
    public (int damage, bool isCritical) CalculateDamage(int strength, int defense)
    {
        var baseDamage = strength * 2;
        var isCritical = _random.NextDouble() < 0.15;
        var damage = isCritical ? baseDamage * 2 : baseDamage;
        
        return (Math.Max(1, damage - defense), isCritical);
    }

    /// <summary>
    /// Reusable reward calculation.
    /// </summary>
    public int CalculateExperienceReward(int enemyLevel)
    {
        return enemyLevel * 50;
    }
}
```

---

## Decision 4: When to Use Each Layer

### Decision Matrix

| Scenario | Orchestrator | Handler | Service | Example |
|----------|-------------|---------|---------|---------|
| **Feature with UI workflow** | ✅ Yes | ✅ Yes | ✅ Yes | Combat, Inventory, CharacterCreation |
| **Command from orchestrator** | ❌ No | ✅ Yes | ✅ Maybe | AttackEnemy, EquipItem |
| **Utility command** | ❌ No | ✅ Yes | ❌ No | ToggleSetting, MarkComplete |
| **Simple query** | ❌ No | ✅ Yes | ❌ No | GetPlayerName |
| **Complex calculation** | ❌ No | ✅ Yes | ✅ Yes | CalculateDamage (reused) |

### Rules of Thumb

**Always use Handler**:
- Handlers are the core of CQRS
- Every command/query needs a handler
- Benefits: logging, validation, consistency

**Extract to Service when**:
- Logic is used by 2+ handlers
- Calculation is complex (> 10 lines)
- Domain rule is reusable

**Create Orchestrator when**:
- Feature has UI workflow (menus, loops)
- Coordinates multiple commands
- Manages UI state

---

## Decision 5: MediatR Pipeline Behaviors

**Choice**: ✅ **Use pipeline behaviors for cross-cutting concerns**

### Behaviors to Implement

1. **LoggingBehavior** - Log all commands/queries with timing
2. **ValidationBehavior** - Automatic FluentValidation
3. **PerformanceBehavior** - Warn on slow operations (> 500ms)

**Location**: `Game/Shared/Behaviors/`

**Registration** (in `Program.cs`):
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Order matters! Logging → Validation → Performance
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
});
```

**Benefits**:
- Automatic logging of all operations
- Automatic validation (no manual validation calls)
- Performance monitoring built-in
- Consistent across all features

---

## Decision 6: Shared Components Organization

**Choice**: ✅ **Move cross-cutting concerns to `Shared/`**

### Shared Folder Structure

```
Game/Shared/
├── Services/           ← Cross-cutting services
│   ├── MenuService.cs
│   ├── CharacterViewService.cs
│   ├── GameStateService.cs
│   ├── AudioService.cs
│   └── LoggingService.cs
├── UI/                 ← UI primitives
│   └── ConsoleUI.cs
├── Data/               ← Repositories
│   ├── Repositories/
│   │   ├── SaveGameRepository.cs
│   │   ├── CharacterClassRepository.cs
│   │   └── EquipmentSetRepository.cs
│   └── Models/
├── Events/             ← Domain event handlers
│   └── EventHandlers.cs
└── Behaviors/          ← MediatR pipeline behaviors
    ├── LoggingBehavior.cs
    ├── ValidationBehavior.cs
    └── PerformanceBehavior.cs
```

**What goes in Shared**:
- ✅ Services used by multiple features
- ✅ UI primitives (ConsoleUI, MenuService)
- ✅ Data access (repositories)
- ✅ MediatR behaviors
- ✅ Domain event handlers

**What stays in Features**:
- ✅ Feature-specific commands/queries
- ✅ Feature-specific handlers
- ✅ Feature orchestrators
- ✅ Feature domain services

---

## Decision 7: Naming Conventions

### Files

- **Commands**: `{Action}{Entity}Command.cs` (e.g., `AttackEnemyCommand.cs`)
- **Handlers**: `{Action}{Entity}Handler.cs` (e.g., `AttackEnemyHandler.cs`)
- **Validators**: `{Action}{Entity}Validator.cs` (e.g., `AttackEnemyValidator.cs`)
- **Results**: `{Action}Result.cs` or `{Entity}.cs` (DTOs)
- **Queries**: `Get{Entity}{Details}Query.cs` (e.g., `GetCombatStateQuery.cs`)
- **Orchestrators**: `{Feature}Orchestrator.cs` (e.g., `CombatOrchestrator.cs`)
- **Services**: `{Feature}Service.cs` (e.g., `CombatService.cs`)

### Namespaces

**Option 2 (Commands/Queries subfolders)**:
```csharp
namespace Game.Features.Inventory.Commands;
namespace Game.Features.Inventory.Queries;
```

**Option 3 (Folder per command)**:
```csharp
namespace Game.Features.Combat.Commands.AttackEnemy;
namespace Game.Features.Combat.Queries.GetCombatState;
```

**Shared**:
```csharp
namespace RealmEngine.Shared.Services;
namespace RealmEngine.Shared.UI;
namespace RealmEngine.Shared.Data;
namespace RealmEngine.Shared.Events;
namespace RealmEngine.Shared.Behaviors;
```

---

## Decision 8: Testing Strategy

### Test Organization

Mirror production structure:

```
Game.Tests/Features/
├── Combat/
│   ├── Commands/
│   │   └── AttackEnemyHandlerTests.cs
│   └── Queries/
│       └── GetCombatStateHandlerTests.cs
└── Inventory/
    └── Commands/
        └── EquipItemHandlerTests.cs
```

### What to Test

**Always test**:
- ✅ Handlers (unit tests - mock services)
- ✅ Services (unit tests - pure logic)
- ✅ Validators (unit tests)

**Integration test**:
- ✅ Full workflows (orchestrator → handler → service)
- ✅ MediatR pipeline behaviors

**Don't test**:
- ❌ Orchestrators in isolation (too much UI mocking)
- ❌ Trivial commands (just property setters)

---

## Decision 9: Migration Approach

**Choice**: ✅ **Incremental, phase-by-phase migration**

### Migration Phases

1. **Phase 1**: Foundation (2-3h) - Folder structure, move shared code
2. **Phase 2**: Combat (4-5h) - Pilot feature with Option 3 structure
3. **Phase 3**: Inventory (3-4h) - Apply Option 2 structure
4. **Phase 4**: Character Creation (2-3h) - Apply pattern
5. **Phase 5**: Save/Load (2-3h) - Apply pattern
6. **Phase 6**: Exploration & Gameplay (2h) - Final features
7. **Phase 7**: Cleanup (1-2h) - Documentation, polish

**Benefits**:
- ✅ Low risk (rollback after each phase)
- ✅ Can test at every step
- ✅ Learn from pilot (Combat) before applying to others
- ✅ Team can continue working (minimal disruption)

---

## Decision 10: Git Strategy

### Branch Strategy

```
main
  ├─ feature/vertical-slice-migration
      ├─ commit: Phase 1 - Foundation
      ├─ commit: Phase 2 - Combat feature
      ├─ commit: Phase 3 - Inventory feature
      ├─ commit: Phase 4 - Character creation
      ├─ commit: Phase 5 - Save/Load
      ├─ commit: Phase 6 - Exploration
      └─ commit: Phase 7 - Cleanup
```

**Commit after each phase** for easy rollback

---

## 🎯 Summary Checklist

When implementing ANY feature:

- [ ] Feature folder in `Game/Features/{FeatureName}/`
- [ ] Choose structure: Option 2 (default) or Option 3 (complex)
- [ ] Orchestrator (if has UI workflow)
- [ ] Commands with handlers (one operation each)
- [ ] Queries with handlers (read operations)
- [ ] Service (if logic reused by 2+ handlers)
- [ ] Validators for commands (FluentValidation)
- [ ] Tests for handlers
- [ ] Follow naming conventions
- [ ] Use proper namespaces

---

## 📚 Reference Documents

- `docs/VERTICAL_SLICE_CQRS_MIGRATION_PLAN.md` - Full migration plan
- `docs/ORGANIZATION_AND_LAYERS_GUIDE.md` - Detailed organization options
- `docs/FOLDER_STRUCTURE_ANALYSIS.md` - Feature-First vs CQRS-First comparison

---

**Last Updated**: December 8, 2024  
**Status**: ✅ Finalized and ready for implementation
