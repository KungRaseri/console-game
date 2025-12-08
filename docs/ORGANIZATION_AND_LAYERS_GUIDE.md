# 🎯 Feature-First Organization Options & CQRS Layer Responsibilities

**Date**: December 8, 2024  
**Topic**: Feature organization patterns and CQRS layer design

---

## Part 1: Feature-First Organization Options

### Option 1: Flat Feature Structure (Simple)

**Best for**: Small features (< 10 commands/queries)

```
Game/Features/
├── Combat/
│   ├── AttackEnemyCommand.cs
│   ├── AttackEnemyHandler.cs
│   ├── AttackEnemyValidator.cs
│   ├── DefendActionCommand.cs
│   ├── DefendActionHandler.cs
│   ├── GetCombatStateQuery.cs
│   ├── GetCombatStateHandler.cs
│   ├── CombatOrchestrator.cs
│   └── CombatService.cs
├── Inventory/
│   ├── EquipItemCommand.cs
│   ├── EquipItemHandler.cs
│   ├── UseItemCommand.cs
│   ├── UseItemHandler.cs
│   ├── GetInventoryQuery.cs
│   ├── GetInventoryHandler.cs
│   ├── InventoryOrchestrator.cs
│   └── InventoryService.cs
└── CharacterCreation/
    └── ...
```

**Namespaces**:
```csharp
namespace Game.Features.Combat;

public record AttackEnemyCommand : IRequest<AttackResult> { }
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult> { }
```

**Pros**:
- ✅ Simplest structure
- ✅ Easy to navigate (single folder)
- ✅ Quick to create new commands

**Cons**:
- ❌ Can get crowded with many commands (15+ files)
- ❌ Hard to distinguish commands from queries at a glance
- ❌ No separation between command/query concerns

**Use when**: Feature has < 10 operations total

---

### Option 2: Commands/Queries Subfolders (Organized)

**Best for**: Medium features (10-30 commands/queries)

```
Game/Features/
├── Combat/
│   ├── Commands/
│   │   ├── AttackEnemyCommand.cs
│   │   ├── AttackEnemyHandler.cs
│   │   ├── AttackEnemyValidator.cs
│   │   ├── DefendActionCommand.cs
│   │   ├── DefendActionHandler.cs
│   │   ├── UseCombatItemCommand.cs
│   │   └── UseCombatItemHandler.cs
│   ├── Queries/
│   │   ├── GetCombatStateQuery.cs
│   │   ├── GetCombatStateHandler.cs
│   │   ├── GetEnemyInfoQuery.cs
│   │   └── GetEnemyInfoHandler.cs
│   ├── CombatOrchestrator.cs
│   └── CombatService.cs
└── Inventory/
    ├── Commands/
    ├── Queries/
    ├── InventoryOrchestrator.cs
    └── InventoryService.cs
```

**Namespaces**:
```csharp
namespace Game.Features.Combat.Commands;

public record AttackEnemyCommand : IRequest<AttackResult> { }
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult> { }
```

**Pros**:
- ✅ Clear separation of commands and queries
- ✅ Easy to find all actions (Commands/) or reads (Queries/)
- ✅ Still co-located in feature folder
- ✅ Good balance of organization and simplicity

**Cons**:
- ⚠️ Related files (command + handler + validator) in same folder
- ⚠️ Can still get crowded with 20+ commands

**Use when**: Feature has 10-30 operations ⭐ **RECOMMENDED for most features**

---

### Option 3: One Folder Per Command/Query (Detailed)

**Best for**: Large features (30+ commands/queries) OR complex commands

```
Game/Features/
├── Combat/
│   ├── Commands/
│   │   ├── AttackEnemy/
│   │   │   ├── AttackEnemyCommand.cs
│   │   │   ├── AttackEnemyHandler.cs
│   │   │   ├── AttackEnemyValidator.cs
│   │   │   └── AttackEnemyResult.cs
│   │   ├── DefendAction/
│   │   │   ├── DefendActionCommand.cs
│   │   │   ├── DefendActionHandler.cs
│   │   │   └── DefendActionResult.cs
│   │   ├── UseCombatItem/
│   │   │   ├── UseCombatItemCommand.cs
│   │   │   ├── UseCombatItemHandler.cs
│   │   │   ├── UseCombatItemValidator.cs
│   │   │   └── UseCombatItemResult.cs
│   │   └── FleeFromCombat/
│   │       ├── FleeFromCombatCommand.cs
│   │       ├── FleeFromCombatHandler.cs
│   │       └── FleeResult.cs
│   ├── Queries/
│   │   ├── GetCombatState/
│   │   │   ├── GetCombatStateQuery.cs
│   │   │   ├── GetCombatStateHandler.cs
│   │   │   └── CombatState.cs (DTO)
│   │   └── GetEnemyInfo/
│   │       ├── GetEnemyInfoQuery.cs
│   │       ├── GetEnemyInfoHandler.cs
│   │       └── EnemyInfo.cs (DTO)
│   ├── CombatOrchestrator.cs
│   └── CombatService.cs
└── Inventory/
    └── ...
```

**Namespaces**:
```csharp
namespace Game.Features.Combat.Commands.AttackEnemy;

public record AttackEnemyCommand : IRequest<AttackResult> { }
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult> { }
public class AttackEnemyValidator : AbstractValidator<AttackEnemyCommand> { }
public record AttackResult { }
```

**Pros**:
- ✅ Ultimate organization - everything for one operation in one folder
- ✅ Easy to add supporting files (DTOs, mappings, etc.)
- ✅ Each command is self-documenting
- ✅ Great for complex operations with multiple files
- ✅ Matches modern .NET architecture (Jimmy Bogard, Clean Architecture)

**Cons**:
- ⚠️ More folders (can feel like "folder overload")
- ⚠️ Slightly more navigation for simple commands

**Use when**: 
- Feature has 30+ operations
- Commands are complex (need DTOs, mappings, etc.)
- You want maximum clarity ⭐ **RECOMMENDED for large features**

---

### Option 4: Hybrid Approach (Pragmatic)

**Best for**: Real-world projects with mixed complexity

```
Game/Features/
├── Combat/                         ← Large feature
│   ├── Commands/
│   │   ├── AttackEnemy/           ← Complex command (folder)
│   │   │   ├── AttackEnemyCommand.cs
│   │   │   ├── AttackEnemyHandler.cs
│   │   │   ├── AttackEnemyValidator.cs
│   │   │   └── AttackResult.cs
│   │   ├── DefendActionCommand.cs  ← Simple command (flat)
│   │   ├── DefendActionHandler.cs
│   │   ├── FleeCommand.cs          ← Simple command (flat)
│   │   └── FleeHandler.cs
│   ├── Queries/
│   │   ├── GetCombatStateQuery.cs
│   │   └── GetCombatStateHandler.cs
│   ├── CombatOrchestrator.cs
│   └── CombatService.cs
├── Inventory/                      ← Medium feature
│   ├── Commands/
│   │   ├── EquipItemCommand.cs    ← All flat (simpler)
│   │   ├── EquipItemHandler.cs
│   │   ├── UseItemCommand.cs
│   │   └── UseItemHandler.cs
│   ├── Queries/
│   │   └── ...
│   └── InventoryOrchestrator.cs
└── CharacterCreation/              ← Small feature
    ├── CreateCharacterCommand.cs   ← Completely flat (simple)
    ├── CreateCharacterHandler.cs
    ├── CreateCharacterValidator.cs
    └── CharacterCreationOrchestrator.cs
```

**Decision Matrix**:
- **Complex command** (validator + DTOs + multiple files) → **Folder per command**
- **Simple command** (just command + handler) → **Flat files**
- **Feature with 30+ commands** → **Folder per command**
- **Feature with < 10 commands** → **Flat or Commands/Queries folders**

**Pros**:
- ✅ Pragmatic - adapts to actual complexity
- ✅ Avoids over-engineering simple commands
- ✅ Provides structure for complex commands
- ✅ Most flexible

**Cons**:
- ⚠️ Inconsistent structure across features
- ⚠️ Need clear guidelines for when to use folders

**Use when**: You want flexibility based on actual complexity ⭐ **RECOMMENDED for pragmatic teams**

---

### Recommendation for Your Game

Based on your current codebase:

| Feature | Commands | Queries | Recommendation |
|---------|----------|---------|----------------|
| **Combat** | 4-5 (Attack, Defend, UseItem, Flee) | 2-3 | **Option 3** (folder per command) |
| **Inventory** | 5-6 (Equip, Unequip, Use, Drop, Sort) | 2-3 | **Option 2** (Commands/Queries subfolders) |
| **Character Creation** | 3-4 | 2 | **Option 2** (Commands/Queries subfolders) |
| **Save/Load** | 4-5 | 3-4 | **Option 2** (Commands/Queries subfolders) |
| **Exploration** | 3-4 | 2 | **Option 2** (Commands/Queries subfolders) |

**My recommendation**: **Start with Option 3 (folder per command)** for Combat as the pilot, then use **Option 2** for simpler features.

**Why?**
- Combat is complex enough to justify folders
- Sets a good template for future features
- Easy to flatten later if too much overhead
- Matches the migration plan I already created

---

## Part 2: Services vs Orchestrators vs Handlers

This is a **critical question** that many CQRS beginners struggle with!

### The Three-Layer Model

```
┌─────────────────────────────────────────┐
│         Orchestrator Layer               │  ← UI Workflow Coordination
│  (CombatOrchestrator, InventoryOrch...)  │
└─────────────────────────────────────────┘
                   ↓ sends
┌─────────────────────────────────────────┐
│         Handler Layer (CQRS)             │  ← Business Logic Execution
│  (AttackEnemyHandler, EquipItemHandler)  │
└─────────────────────────────────────────┘
                   ↓ uses
┌─────────────────────────────────────────┐
│         Service Layer                    │  ← Reusable Domain Logic
│  (CombatService, InventoryService)       │
└─────────────────────────────────────────┘
```

---

### Layer 1: Orchestrator

**Purpose**: UI workflow coordination

**Responsibilities**:
- ✅ Display UI (ConsoleUI calls)
- ✅ Show menus and get user input
- ✅ Coordinate multiple commands/queries in a workflow
- ✅ Handle UI state (menu loops)
- ✅ Send commands/queries via MediatR

**Does NOT**:
- ❌ Contain business logic
- ❌ Directly modify domain models
- ❌ Calculate damage, validate rules, etc.

**Example**:
```csharp
public class CombatOrchestrator
{
    private readonly IMediator _mediator;
    private readonly MenuService _menuService;

    public async Task RunCombatAsync(Character player, Enemy enemy)
    {
        var inCombat = true;
        var combatLog = new CombatLog();

        while (inCombat)
        {
            // 1. Display UI
            ConsoleUI.ShowPanel("Combat", $"{player.Name} vs {enemy.Name}", "red");
            ConsoleUI.WriteText($"Player HP: {player.Health}/{player.MaxHealth}");
            ConsoleUI.WriteText($"Enemy HP: {enemy.Health}/{enemy.MaxHealth}");

            // 2. Get user choice
            var choice = _menuService.ShowMenu("Choose action:", 
                "Attack", "Defend", "Use Item", "Flee");

            // 3. Send command based on choice
            switch (choice)
            {
                case "Attack":
                    var attackResult = await _mediator.Send(new AttackEnemyCommand 
                    { 
                        Player = player, 
                        Enemy = enemy,
                        CombatLog = combatLog 
                    });
                    
                    // 4. Display result
                    ConsoleUI.ShowSuccess($"You dealt {attackResult.Damage} damage!");
                    
                    if (attackResult.IsEnemyDefeated)
                    {
                        ConsoleUI.ShowInfo($"Victory! +{attackResult.ExperienceGained} XP");
                        inCombat = false;
                    }
                    break;
                    
                case "Defend":
                    var defendResult = await _mediator.Send(new DefendActionCommand 
                    { 
                        Player = player,
                        CombatLog = combatLog 
                    });
                    ConsoleUI.ShowInfo(defendResult.Message);
                    break;
                    
                // ... more cases
            }

            // 5. Enemy turn (if still alive)
            if (inCombat && enemy.Health > 0)
            {
                var enemyAttack = await _mediator.Send(new EnemyAttackCommand 
                { 
                    Enemy = enemy, 
                    Player = player 
                });
                ConsoleUI.ShowWarning($"Enemy attacks for {enemyAttack.Damage} damage!");
                
                if (player.Health <= 0)
                {
                    ConsoleUI.ShowError("You have been defeated!");
                    inCombat = false;
                }
            }
        }

        // 6. Show combat summary
        combatLog.Display();
    }
}
```

**Key Points**:
- Thin layer - just coordinates
- Sends commands, displays results
- No business logic calculations

---

### Layer 2: Handler (CQRS)

**Purpose**: Execute a single business operation

**Responsibilities**:
- ✅ Execute ONE command or query
- ✅ Orchestrate domain services to fulfill request
- ✅ Apply business rules
- ✅ Publish domain events
- ✅ Return results

**Does NOT**:
- ❌ Display UI
- ❌ Contain reusable calculations (delegates to service)
- ❌ Handle multiple operations (one handler = one operation)

**Example**:
```csharp
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackResult>
{
    private readonly CombatService _combatService;
    private readonly IMediator _mediator;

    public AttackEnemyHandler(CombatService combatService, IMediator mediator)
    {
        _combatService = combatService;
        _mediator = mediator;
    }

    public async Task<AttackResult> Handle(
        AttackEnemyCommand request, 
        CancellationToken cancellationToken)
    {
        var player = request.Player;
        var enemy = request.Enemy;
        var combatLog = request.CombatLog;

        // 1. Use service to calculate damage
        var (damage, isCritical) = _combatService.CalculateDamage(
            player.Strength, 
            player.Dexterity, 
            enemy.Defense
        );

        // 2. Apply damage to enemy
        enemy.Health -= damage;
        
        // 3. Log the attack
        combatLog?.AddEntry($"{player.Name} attacks for {damage} damage" + 
            (isCritical ? " (CRITICAL!)" : ""));

        // 4. Publish event
        await _mediator.Publish(
            new AttackPerformed(player.Name, enemy.Name, damage), 
            cancellationToken
        );

        // 5. Check if enemy defeated
        var isDefeated = enemy.Health <= 0;
        int xpGained = 0;
        int goldGained = 0;

        if (isDefeated)
        {
            // Use service to calculate rewards
            xpGained = _combatService.CalculateExperienceReward(enemy.Level);
            goldGained = _combatService.CalculateGoldReward(enemy.Level);
            
            // Apply rewards
            player.Experience += xpGained;
            player.Gold += goldGained;

            combatLog?.AddEntry($"{enemy.Name} defeated! +{xpGained} XP, +{goldGained} gold");

            // Publish events
            await _mediator.Publish(new EnemyDefeated(player.Name, enemy.Name), cancellationToken);
            await _mediator.Publish(new GoldGained(player.Name, goldGained), cancellationToken);
        }

        // 6. Return result
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

**Key Points**:
- ONE operation (attack enemy)
- Delegates calculations to CombatService
- Applies results to domain models
- Publishes events
- Returns structured result

---

### Layer 3: Service (Domain Logic)

**Purpose**: Reusable domain calculations and rules

**Responsibilities**:
- ✅ Pure business logic calculations
- ✅ Domain rules enforcement
- ✅ Reusable across multiple handlers
- ✅ Stateless (or manages specific state)

**Does NOT**:
- ❌ Display UI
- ❌ Publish events (handlers do that)
- ❌ Modify domain models directly (returns values instead)

**Example**:
```csharp
public class CombatService
{
    private readonly Random _random = new();

    /// <summary>
    /// Calculate damage for an attack.
    /// Pure calculation - no side effects.
    /// </summary>
    public (int damage, bool isCritical) CalculateDamage(
        int attackerStrength, 
        int attackerDexterity, 
        int defenderDefense)
    {
        // Base damage from strength
        var baseDamage = attackerStrength * 2;

        // Critical hit chance based on dexterity
        var critChance = attackerDexterity / 100.0;
        var isCritical = _random.NextDouble() < critChance;

        // Apply critical multiplier
        var damage = isCritical ? baseDamage * 2 : baseDamage;

        // Apply defense reduction
        damage = Math.Max(1, damage - defenderDefense);

        return (damage, isCritical);
    }

    /// <summary>
    /// Calculate XP reward for defeating enemy.
    /// </summary>
    public int CalculateExperienceReward(int enemyLevel)
    {
        return enemyLevel * 50;
    }

    /// <summary>
    /// Calculate gold reward for defeating enemy.
    /// </summary>
    public int CalculateGoldReward(int enemyLevel)
    {
        return enemyLevel * 10 + _random.Next(5, 20);
    }

    /// <summary>
    /// Check if player can flee from combat.
    /// </summary>
    public double CalculateFleeChance(Character player, Enemy enemy)
    {
        // Higher dexterity = better flee chance
        var baseFlee = 0.5;
        var dexBonus = player.Dexterity / 200.0;
        var levelPenalty = (enemy.Level - player.Level) * 0.05;
        
        return Math.Clamp(baseFlee + dexBonus - levelPenalty, 0.1, 0.9);
    }

    /// <summary>
    /// Determine combat initiative (who goes first).
    /// </summary>
    public bool PlayerGoesFirst(Character player, Enemy enemy)
    {
        var playerInitiative = player.Dexterity + _random.Next(1, 20);
        var enemyInitiative = enemy.Dexterity + _random.Next(1, 20);
        
        return playerInitiative >= enemyInitiative;
    }
}
```

**Key Points**:
- Pure calculations
- No side effects (doesn't modify parameters)
- Returns values for handlers to use
- Reusable across multiple handlers

---

## Do You Need All Three Layers?

### Short Answer: **Not always, but usually yes for your game**

Let me explain different scenarios:

---

### Scenario 1: Simple CRUD with UI (All 3 Layers)

**Example**: Equip Item

**Orchestrator**: Shows inventory menu, gets user choice
```csharp
var item = _menuService.SelectItem(player.Inventory);
var result = await _mediator.Send(new EquipItemCommand { Player = player, Item = item });
ConsoleUI.ShowSuccess(result.Message);
```

**Handler**: Executes equip logic, publishes events
```csharp
public async Task<EquipResult> Handle(EquipItemCommand request, ...)
{
    var (canEquip, reason) = _inventoryService.CanEquipItem(request.Player, request.Item);
    if (!canEquip) return new EquipResult { Success = false, Message = reason };
    
    _inventoryService.EquipItem(request.Player, request.Item);
    await _mediator.Publish(new ItemEquipped(request.Player.Name, request.Item.Name));
    
    return new EquipResult { Success = true, Message = "Item equipped!" };
}
```

**Service**: Business rules
```csharp
public (bool canEquip, string reason) CanEquipItem(Character player, Item item)
{
    if (item.Type != ItemType.Weapon && item.Type != ItemType.Armor)
        return (false, "Item is not equippable");
    
    if (item.RequiredLevel > player.Level)
        return (false, $"Requires level {item.RequiredLevel}");
    
    return (true, "");
}
```

**Verdict**: ✅ Need all 3 layers

---

### Scenario 2: Simple Query, No UI Loop (Handler + Service Only)

**Example**: Get Combat State (called by orchestrator)

**Handler**: Fetches and formats data
```csharp
public Task<CombatState> Handle(GetCombatStateQuery request, ...)
{
    var state = new CombatState
    {
        PlayerHealth = request.Player.Health,
        PlayerMaxHealth = request.Player.MaxHealth,
        EnemyHealth = request.Enemy.Health,
        EnemyMaxHealth = request.Enemy.MaxHealth,
        PlayerCanFlee = _combatService.CalculateFleeChance(request.Player, request.Enemy) > 0.3
    };
    
    return Task.FromResult(state);
}
```

**Service**: Calculation only
```csharp
public double CalculateFleeChance(Character player, Enemy enemy) { ... }
```

**No Orchestrator Needed**: This query is called BY an orchestrator, doesn't need its own

**Verdict**: ✅ Handler + Service (no dedicated orchestrator)

---

### Scenario 3: Super Simple Command (Handler Only)

**Example**: Toggle Setting

**Handler**: No complex logic, no service needed
```csharp
public Task<Unit> Handle(ToggleSoundCommand request, ...)
{
    request.Settings.SoundEnabled = !request.Settings.SoundEnabled;
    return Task.FromResult(Unit.Value);
}
```

**No Service**: Logic is trivial
**No Orchestrator**: Called from settings menu orchestrator

**Verdict**: ✅ Handler only

---

### Scenario 4: Complex Workflow with Multiple Commands (Orchestrator + Handlers + Services)

**Example**: Combat Turn

**Orchestrator**: Coordinates full turn
```csharp
public async Task ExecuteTurnAsync(Character player, Enemy enemy)
{
    // Player action
    var playerAction = _menuService.ShowMenu(...);
    var playerResult = await _mediator.Send(GetCommandForAction(playerAction));
    DisplayResult(playerResult);
    
    if (enemy.Health <= 0) return;
    
    // Enemy action
    var enemyAction = await _mediator.Send(new GetEnemyActionQuery { Enemy = enemy });
    var enemyResult = await _mediator.Send(enemyAction);
    DisplayResult(enemyResult);
    
    // Check for status effects
    await _mediator.Send(new ApplyStatusEffectsCommand { Player = player, Enemy = enemy });
}
```

**Handlers**: Each command has handler (AttackEnemy, DefendAction, etc.)

**Services**: Reusable logic (damage calc, initiative, etc.)

**Verdict**: ✅ Need all 3 layers

---

## Decision Matrix: When to Use Each Layer

| Scenario | Orchestrator | Handler | Service | Example |
|----------|-------------|---------|---------|---------|
| **UI workflow with menu loop** | ✅ Yes | ✅ Yes | ✅ Yes | Combat, Inventory |
| **Single command from orchestrator** | ❌ No | ✅ Yes | ✅ Maybe | AttackEnemy |
| **Simple query** | ❌ No | ✅ Yes | ❌ No | GetPlayerName |
| **Complex calculation** | ❌ No | ✅ Yes | ✅ Yes | CalculateDamage |
| **Trivial operation** | ❌ No | ✅ Yes | ❌ No | ToggleSetting |
| **Multi-step workflow** | ✅ Yes | ✅ Yes | ✅ Yes | Character Creation |

---

## Recommended Patterns for Your Game

### Pattern 1: Feature with Orchestrator (Combat, Inventory, Character Creation)

```
Features/Combat/
├── Commands/
│   ├── AttackEnemy/
│   │   ├── AttackEnemyCommand.cs
│   │   ├── AttackEnemyHandler.cs    ← Uses CombatService
│   │   └── AttackEnemyValidator.cs
│   └── DefendAction/
│       ├── DefendActionCommand.cs
│       └── DefendActionHandler.cs   ← Uses CombatService
├── Queries/
│   └── GetCombatState/
│       ├── GetCombatStateQuery.cs
│       └── GetCombatStateHandler.cs ← Uses CombatService
├── CombatOrchestrator.cs            ← Sends commands via MediatR
└── CombatService.cs                 ← Reusable calculations
```

**All 3 layers needed** ✅

---

### Pattern 2: Feature without Orchestrator (Utilities, Helpers)

```
Features/LevelUp/
├── Commands/
│   └── ApplyLevelUp/
│       ├── ApplyLevelUpCommand.cs
│       └── ApplyLevelUpHandler.cs   ← Uses LevelUpService
├── Queries/
│   └── CalculateNextLevel/
│       ├── CalculateNextLevelQuery.cs
│       └── CalculateNextLevelHandler.cs ← Uses LevelUpService
└── LevelUpService.cs                ← XP calculations, stat increases

(No orchestrator - called from other orchestrators)
```

**Handlers + Service only** ✅

---

## Benefits of Each Layer

### Orchestrator Benefits

1. **Separation of UI from Logic**
   - Easy to change UI without touching business logic
   - Could swap ConsoleUI for WebUI or GraphQL

2. **Testability**
   - Test handlers in isolation (no UI)
   - Integration test orchestrators (UI workflow)

3. **Workflow Coordination**
   - Natural place for multi-step workflows
   - Menu loops, state machines

**Example**: CombatOrchestrator coordinates Attack → Enemy Turn → Status Effects → Check Win

---

### Handler Benefits

1. **Single Responsibility**
   - ONE handler = ONE operation
   - Easy to understand, test, maintain

2. **MediatR Pipeline Benefits**
   - Automatic logging (LoggingBehavior)
   - Automatic validation (ValidationBehavior)
   - Performance monitoring (PerformanceBehavior)

3. **Auditability**
   - Every command is logged
   - Can track all state changes
   - Replay possible (event sourcing later)

**Example**: AttackEnemyHandler ONLY handles attacking, nothing else

---

### Service Benefits

1. **Reusability**
   - CalculateDamage used by AttackEnemyHandler, EnemyAttackHandler, etc.
   - Don't duplicate logic

2. **Testability**
   - Pure functions easy to test
   - No dependencies on UI or database

3. **Domain Modeling**
   - Encapsulates business rules
   - Single source of truth for calculations

**Example**: CombatService.CalculateDamage used by 5 different handlers

---

## Can You Skip Layers?

### ❌ Don't Skip Handlers

**Why**: Handlers are the **core of CQRS**. Every command/query needs a handler.

Even if trivial:
```csharp
public class ToggleSoundHandler : IRequestHandler<ToggleSoundCommand, Unit>
{
    public Task<Unit> Handle(ToggleSoundCommand request, ...)
    {
        request.Settings.SoundEnabled = !request.Settings.SoundEnabled;
        return Task.FromResult(Unit.Value);
    }
}
```

**Benefit**: Consistent pattern, automatic logging/validation via MediatR

---

### ✅ Can Skip Service (for simple operations)

If handler logic is trivial and not reused:

```csharp
public class MarkQuestCompleteHandler : IRequestHandler<MarkQuestCompleteCommand, Unit>
{
    public async Task<Unit> Handle(MarkQuestCompleteCommand request, ...)
    {
        request.Quest.IsCompleted = true;
        request.Quest.CompletedDate = DateTime.UtcNow;
        
        await _mediator.Publish(new QuestCompleted(request.Quest.Name));
        
        return Unit.Value;
    }
}
```

**No service needed** - logic is simple and specific to this command

**Rule of Thumb**: If logic is used by 2+ handlers, extract to service

---

### ✅ Can Skip Orchestrator (for commands called by other orchestrators)

**Example**: SaveGame command doesn't need its own orchestrator

```csharp
// Called FROM GameplayOrchestrator
public async Task HandleRestAsync()
{
    ConsoleUI.ShowInfo("Resting...");
    
    // Restore health
    await _mediator.Send(new RestoreHealthCommand { Player = player });
    
    // Auto-save
    await _mediator.Send(new SaveGameCommand { Player = player });
    
    ConsoleUI.ShowSuccess("Health restored and game saved!");
}
```

**SaveGameCommand** has handler but no orchestrator (it's a "service command")

---

## Final Recommendation for Your Game

### Use This Pattern:

```
Features/Combat/                         ← Feature root
├── Commands/                            ← All commands
│   ├── AttackEnemy/                     ← One folder per command
│   │   ├── AttackEnemyCommand.cs        ← Command DTO
│   │   ├── AttackEnemyHandler.cs        ← Handler (uses Service)
│   │   ├── AttackEnemyValidator.cs      ← FluentValidation
│   │   └── AttackResult.cs              ← Result DTO
│   ├── DefendAction/
│   ├── UseCombatItem/
│   └── FleeFromCombat/
├── Queries/                             ← All queries
│   ├── GetCombatState/
│   │   ├── GetCombatStateQuery.cs
│   │   ├── GetCombatStateHandler.cs
│   │   └── CombatState.cs (DTO)
│   └── GetEnemyInfo/
├── CombatOrchestrator.cs                ← UI workflow (uses MediatR)
└── CombatService.cs                     ← Reusable domain logic
```

**Why**:
- ✅ Clear separation: Orchestrator (UI) → Handler (operation) → Service (logic)
- ✅ Testable at every layer
- ✅ Reusable services
- ✅ MediatR pipeline benefits
- ✅ Scales well

**When to use all 3**:
- Combat ✅ (complex workflow)
- Inventory ✅ (complex workflow)
- Character Creation ✅ (complex workflow)

**When to skip orchestrator**:
- Individual save/load commands (called from other orchestrators)
- Utility commands (called programmatically)

**When to skip service**:
- Trivial operations (toggle boolean)
- One-off logic not reused

---

## Quick Reference

**Orchestrator**: "What menu should I show? What command should I send?"
**Handler**: "Execute this ONE operation. Return result."
**Service**: "Here's how to calculate damage. Here's how to check if item can equip."

**Orchestrator → Handler → Service** = UI → Operation → Logic

---

## Questions?

1. Should I start the migration with Option 3 (folder per command)?
2. Any specific features you want organized differently?
3. Ready to begin Phase 1 (Foundation setup)?

Let me know! 🚀
