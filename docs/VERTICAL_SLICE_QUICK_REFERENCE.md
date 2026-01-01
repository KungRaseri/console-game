# Vertical Slice Architecture - Developer Quick Reference

## 🏗️ Project Structure

```
Game/
├── Features/              ← Organize by BUSINESS CAPABILITY
│   ├── Combat/
│   ├── Inventory/
│   ├── CharacterCreation/
│   ├── SaveLoad/
│   └── Exploration/
├── Shared/                ← Cross-cutting concerns
│   ├── Behaviors/         ← MediatR pipeline behaviors
│   ├── Services/          ← Shared services
│   ├── UI/                ← UI components
│   └── Events/            ← Domain events
├── Services/              ← Utility services (static/cross-cutting)
│   └── LevelUpService.cs
├── Models/                ← Domain models
├── Generators/            ← Random generation
└── Validators/            ← FluentValidation validators
```

## 📋 Adding a New Feature

### Step 1: Create Feature Folder Structure
```bash
Game/Features/YourFeature/
├── Commands/              ← Write operations (change state)
├── Queries/               ← Read operations (no side effects)
├── YourFeatureService.cs  ← Business logic
└── YourFeatureOrchestrator.cs  ← UI workflow
```

### Step 2: Create a Command

**Command Definition:**
```csharp
using MediatR;

namespace Game.Features.YourFeature.Commands;

public record YourCommand(
    // Command parameters
    string Parameter1,
    int Parameter2
) : IRequest<YourCommandResult>;

public record YourCommandResult(
    bool Success,
    string? ErrorMessage = null
);
```

**Command Handler:**
```csharp
using MediatR;
using Serilog;

namespace Game.Features.YourFeature.Commands;

public class YourCommandHandler : IRequestHandler<YourCommand, YourCommandResult>
{
    private readonly YourFeatureService _service;
    private readonly IMediator _mediator;

    public YourCommandHandler(YourFeatureService service, IMediator mediator)
    {
        _service = service;
        _mediator = mediator;
    }

    public async Task<YourCommandResult> Handle(YourCommand request, CancellationToken ct)
    {
        try
        {
            // 1. Validate (if needed - ValidationBehavior handles this automatically)
            
            // 2. Execute business logic
            var result = await _service.DoSomethingAsync(request.Parameter1);
            
            // 3. Publish events (optional)
            await _mediator.Publish(new SomethingHappened(result), ct);
            
            // 4. Return result
            return new YourCommandResult(Success: true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in YourCommand");
            return new YourCommandResult(Success: false, ErrorMessage: ex.Message);
        }
    }
}
```

**Command Validator (Optional):**
```csharp
using FluentValidation;

namespace Game.Features.YourFeature.Commands;

public class YourCommandValidator : AbstractValidator<YourCommand>
{
    public YourCommandValidator()
    {
        RuleFor(x => x.Parameter1)
            .NotEmpty()
            .WithMessage("Parameter1 is required");
            
        RuleFor(x => x.Parameter2)
            .GreaterThan(0)
            .WithMessage("Parameter2 must be positive");
    }
}
```

### Step 3: Create a Query

**Query Definition:**
```csharp
using MediatR;

namespace Game.Features.YourFeature.Queries;

public record YourQuery(int Id) : IRequest<YourQueryResult>;

public record YourQueryResult(
    bool Success,
    YourData? Data = null,
    string? ErrorMessage = null
);
```

**Query Handler:**
```csharp
using MediatR;
using Serilog;

namespace Game.Features.YourFeature.Queries;

public class YourQueryHandler : IRequestHandler<YourQuery, YourQueryResult>
{
    private readonly YourFeatureService _service;

    public YourQueryHandler(YourFeatureService service)
    {
        _service = service;
    }

    public Task<YourQueryResult> Handle(YourQuery request, CancellationToken ct)
    {
        try
        {
            var data = _service.GetData(request.Id);
            return Task.FromResult(new YourQueryResult(Success: true, Data: data));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in YourQuery");
            return Task.FromResult(new YourQueryResult(Success: false, ErrorMessage: ex.Message));
        }
    }
}
```

### Step 4: Register Services in Program.cs

```csharp
// In Program.cs ConfigureServices section:

// Register your feature service
services.AddTransient<YourFeatureService>();
services.AddTransient<YourFeatureOrchestrator>();

// MediatR automatically discovers handlers in the assembly
// No need to register handlers manually!
```

### Step 5: Use in Code

```csharp
// Inject IMediator
private readonly IMediator _mediator;

public YourClass(IMediator mediator)
{
    _mediator = mediator;
}

// Send a command
var command = new YourCommand("value", 42);
var result = await _mediator.Send(command);

if (result.Success)
{
    ConsoleUI.ShowSuccess("Operation completed!");
}

// Send a query
var query = new YourQuery(123);
var queryResult = await _mediator.Send(query);

if (queryResult.Success && queryResult.Data != null)
{
    // Use data
}
```

## 🎯 CQRS Patterns

### When to Use Commands
- **Changing state** (create, update, delete)
- **Side effects** (send email, save to database)
- **Business operations** (process payment, apply discount)
- Examples: `CreateCharacterCommand`, `AttackEnemyCommand`, `SaveGameCommand`

### When to Use Queries
- **Reading data** (no state changes)
- **Retrieving information**
- **No side effects**
- Examples: `GetInventoryItemsQuery`, `GetCharacterClassesQuery`, `GetCombatStateQuery`

### Command/Query Naming
- **Commands:** `VerbNounCommand` (e.g., `CreateCharacter`, `AttackEnemy`, `SaveGame`)
- **Queries:** `GetNounQuery` or `GetNounByIdQuery` (e.g., `GetInventoryItems`, `GetCharacterClass`)

## 🔧 MediatR Pipeline Behaviors

All commands and queries pass through these behaviors automatically:

1. **LoggingBehavior** - Logs request name and execution time
2. **ValidationBehavior** - Validates commands using FluentValidation
3. **PerformanceBehavior** - Warns about slow operations (>500ms)

### Example Log Output
```
[INF] Handling AttackEnemyCommand
[WRN] AttackEnemyCommand took 523ms to execute
[INF] AttackEnemyCommand completed in 523ms
```

## 📐 Three-Layer Architecture

Each feature follows this pattern:

```
1. Orchestrator (GameEngine, UI)
   ↓ Sends Command/Query via MediatR
2. Handler (Business Logic Coordinator)
   ↓ Delegates to
3. Service (Domain Logic, Calculations)
```

**Example:**
```csharp
// Layer 1: Orchestrator (GameEngine.cs)
var command = new AttackEnemyCommand(Player, _currentEnemy, isDefending);
var result = await _mediator.Send(command);

// Layer 2: Handler (AttackEnemyHandler.cs)
public async Task<AttackResult> Handle(AttackEnemyCommand request, CancellationToken ct)
{
    var result = await _combatService.ExecutePlayerAttackAsync(...);
    await _mediator.Publish(new DamageTaken(...), ct);
    return result;
}

// Layer 3: Service (CombatService.cs)
public async Task<AttackResult> ExecutePlayerAttackAsync(...)
{
    // Pure business logic
    var damage = CalculateDamage(...);
    enemy.Health -= damage;
    return new AttackResult(...);
}
```

## 📝 Best Practices

### ✅ DO
- Use **commands** for state changes
- Use **queries** for reading data
- Keep handlers **small and focused**
- Use **dependency injection** for services
- Publish **events** for side effects
- Return **result objects** (not exceptions)
- Use **async/await** consistently
- Add **validation** to commands using FluentValidation

### ❌ DON'T
- Don't change state in queries
- Don't put business logic in handlers (use services)
- Don't catch exceptions unless you handle them
- Don't return null (use result objects with Success flag)
- Don't use static methods in handlers

## 🧪 Testing

### Testing Handlers
```csharp
[Fact]
public async Task Handler_Should_Do_Something()
{
    // Arrange
    var mockService = new Mock<YourFeatureService>();
    mockService.Setup(x => x.DoSomething()).Returns("result");
    
    var handler = new YourCommandHandler(mockService.Object);
    var command = new YourCommand("test");
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Success.Should().BeTrue();
    mockService.Verify(x => x.DoSomething(), Times.Once);
}
```

## 📚 Examples from Codebase

### Simple Command (No Validator)
- `RestCommand` - Simple command, no validation needed
- Handler directly updates character state

### Command with Validator
- `AttackEnemyCommand` - Validates player and enemy not null
- `CreateCharacterCommand` - Validates character name, class, attributes

### Simple Query
- `GetCurrentLocationQuery` - Returns current location from GameStateService
- `GetInventoryItemsQuery` - Returns character's inventory

### Complex Handler
- `AttackEnemyHandler` - Delegates to CombatService, publishes multiple events
- `CreateCharacterHandler` - Creates character, saves game, publishes event

## 🔍 Namespaces

```csharp
// Feature commands/queries
using Game.Features.Combat.Commands;
using Game.Features.Combat.Queries;

// Shared components
using RealmEngine.Shared.Services;
using RealmEngine.Shared.UI;
using RealmEngine.Shared.Events;
using RealmEngine.Shared.Behaviors;

// Models
using Game.Models;

// MediatR
using MediatR;

// Validation
using FluentValidation;

// Logging
using Serilog;
```

## 🚀 Quick Commands

```bash
# Build project
dotnet build Game/Game.csproj

# Run tests
dotnet test Game.Tests/Game.Tests.csproj

# Run game
dotnet run --project Game/Game.csproj

# Watch mode (auto-reload)
dotnet watch run --project Game/Game.csproj
```

## 📖 Further Reading

- [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)

---

**Last Updated:** December 8, 2025  
**Architecture:** Vertical Slice + CQRS with MediatR
