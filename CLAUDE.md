# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 9.0 console-based RPG with a WPF Content Builder tool for editing game data. The console game uses **Vertical Slice Architecture + CQRS** via MediatR, organized by business capability rather than technical layer.

**Solution Structure:**
- **Game.Console** - Main console game application (executable)
- **Game.Core** - Business logic, CQRS handlers, and game features
- **Game.Shared** - Shared models and type definitions
- **Game.Data** - JSON game content, LiteDB repositories, and data access
- **Game.ContentBuilder** - WPF tool for editing JSON game content
- **Game.Tests** - xUnit tests for Game.Core/Console (84 test files)
- **Game.ContentBuilder.Tests** - xUnit tests for ContentBuilder (16 test files)

## Common Commands

### Build & Run
```powershell
# Build entire solution
dotnet build

# Run the console game
dotnet run --project Game.Console

# Run the Content Builder (WPF)
dotnet run --project Game.ContentBuilder

# Debug in VS Code
Press F5
```

### Testing
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test Game.Tests
dotnet test Game.ContentBuilder.Tests

# Run specific test class
dotnet test --filter "FullyQualifiedName~CharacterTests"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Project References
When adding new dependencies between projects, use relative paths:
```powershell
dotnet add Game.Console reference Game.Core
```

## Architecture: Vertical Slice + CQRS

### Core Principle
Code is organized by **business feature** (vertical slice), not technical layer. Each feature contains all its code: Commands (write), Queries (read), Services, and Orchestrators.

### Feature Structure
```
Game.Core/Features/
├── Combat/              - Turn-based combat system
├── Inventory/           - Item management
├── CharacterCreation/   - Character creation flow
├── SaveLoad/            - Game persistence
├── Exploration/         - World exploration
├── Quest/               - Quest management
├── Achievement/         - Achievement tracking
├── Victory/             - Victory conditions
└── Death/               - Death handling
```

### Inside a Feature Folder
```
Features/YourFeature/
├── Commands/            - Write operations (IRequest<TResult>)
│   ├── DoSomethingCommand.cs
│   ├── DoSomethingHandler.cs
│   └── DoSomethingValidator.cs  (FluentValidation)
├── Queries/             - Read operations (IRequest<TResult>)
│   ├── GetSomethingQuery.cs
│   └── GetSomethingHandler.cs
├── YourFeatureService.cs       - Business logic
└── YourFeatureOrchestrator.cs  - UI workflow (in Game.Console)
```

### Adding a New Feature

1. **Create folder structure** in `Game.Core/Features/YourFeature/`
2. **Define Command/Query** as `record` implementing `IRequest<TResult>`
3. **Create Handler** implementing `IRequestHandler<TRequest, TResult>`
4. **Add Validator** (optional) extending `AbstractValidator<TRequest>`
5. **Create Service** for business logic
6. **Create Orchestrator** in `Game.Console/Orchestrators/` for UI flow

**MediatR Behaviors** (automatically applied to all handlers):
- `ValidationBehavior` - FluentValidation integration
- `LoggingBehavior` - Structured logging with Serilog
- `PerformanceBehavior` - Performance tracking

### Example Command
```csharp
// Command
public record AttackEnemyCommand(string SaveId, int Damage) : IRequest<AttackEnemyResult>;
public record AttackEnemyResult(bool Success, int DamageDealt, bool EnemyDefeated);

// Handler
public class AttackEnemyHandler : IRequestHandler<AttackEnemyCommand, AttackEnemyResult>
{
    public async Task<AttackEnemyResult> Handle(AttackEnemyCommand request, CancellationToken ct)
    {
        // Business logic here
        return new AttackEnemyResult(true, 25, false);
    }
}

// Validator (optional)
public class AttackEnemyValidator : AbstractValidator<AttackEnemyCommand>
{
    public AttackEnemyValidator()
    {
        RuleFor(x => x.Damage).GreaterThan(0);
    }
}
```

## Game.Data: JSON Data System

Game content lives in `Game.Data/Data/Json/` with the following structure:

```
Data/Json/
├── enemies/          - Enemy definitions by type (beasts, demons, dragons, etc.)
│   └── [type]/
│       ├── abilities.json    - Special abilities
│       ├── catalog.json      - Enemy catalog entries
│       └── names.json        - Name generation data
├── items/            - Item definitions
├── npcs/             - NPC definitions
├── quests/           - Quest definitions
└── general/          - General game data (character classes, traits, etc.)
```

**JSON files are copied to output during build** - edits require rebuild or use ContentBuilder.

**Note:** Models that define JSON schemas live in `Game.Shared/Models/`, but the actual JSON data files are in `Game.Data/Data/Json/`.

## Game.ContentBuilder: WPF Editor

A WPF application for editing JSON game content with real-time validation.

**Tech Stack:**
- WPF with .NET 9.0-windows
- MVVM pattern (CommunityToolkit.Mvvm)
- Material Design themes (MaterialDesignThemes)
- FluentValidation for JSON validation

**ViewModels:**
- `AbilitiesEditorViewModel` - Edit enemy abilities
- `CatalogEditorViewModel` - Edit enemy catalogs
- `NameListEditorViewModel` - Edit name generation data
- (More editors to be added)

**Running ContentBuilder:**
```powershell
dotnet run --project Game.ContentBuilder
```

**Testing ContentBuilder UI:**
- Uses xUnit with UI automation tests
- Test files in `Game.ContentBuilder.Tests/`

## Game.Data: Repositories & Persistence

**LiteDB-based repositories** for game persistence:
- `SaveGameRepository` - Player save files
- `HallOfFameRepository` - High scores
- `CharacterClassRepository` - Character class data (JSON-backed)
- `EquipmentSetRepository` - Equipment sets (JSON-backed)

Repositories implement interfaces from `Game.Core/Abstractions/` for dependency injection.

## Key Libraries & Patterns

### Console Game (Game.Console, Game.Core)
- **Spectre.Console** - Rich console UI (menus, tables, progress bars)
- **MediatR** - CQRS and event handling
- **FluentValidation** - Input validation
- **Serilog** - Structured logging to console and file
- **LiteDB** - NoSQL database for save files
- **Polly** - Resilience patterns (retry logic)
- **Bogus** - Procedural generation (NPCs, items)
- **Humanizer** - Natural language formatting

### Content Builder (Game.ContentBuilder)
- **CommunityToolkit.Mvvm** - MVVM helpers (`ObservableObject`, `RelayCommand`)
- **MaterialDesignThemes** - Material Design UI components
- **Extended.Wpf.Toolkit** - Additional WPF controls
- **FluentValidation** - JSON data validation

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Expressive assertions

## Important Conventions

### CQRS Naming
- Commands: `{Verb}{Noun}Command.cs` (e.g., `AttackEnemyCommand.cs`)
- Queries: `Get{Noun}Query.cs` (e.g., `GetInventoryQuery.cs`)
- Handlers: `{CommandName}Handler.cs`
- Results: `{CommandName}Result` as nested record in command file

### File Organization
- **One handler per file** (handler + command + result in same file for simple cases)
- **Validators in separate files** when complex
- **Services** hold business logic, handlers orchestrate
- **Orchestrators** (in Game.Console) handle UI workflow and user input

### Testing Patterns
- Arrange-Act-Assert structure
- Use FluentAssertions for readable assertions
- Mock repositories using interfaces from `Game.Core/Abstractions/`
- Test files mirror source structure: `Game.Core/Features/Combat/` → `Game.Tests/Features/Combat/`

### JSON Structure
All JSON files follow standardized schemas defined in `Game.Shared/Models/`. When adding new JSON:
1. Define model in `Game.Shared/Models/`
2. Add JSON file in `Game.Data/Data/Json/`
3. Create editor in `Game.ContentBuilder` (ViewModel + View)
4. Add validation rules with FluentValidation

## Documentation

Comprehensive documentation in `docs/`:
- **docs/README.md** - Documentation index
- **docs/VERTICAL_SLICE_QUICK_REFERENCE.md** - How to add features
- **docs/ARCHITECTURE_DECISIONS.md** - Architectural rationale
- **docs/guides/** - Feature guides (inventory, save/load, etc.)
- **docs/testing/** - Test coverage reports

**Always consult existing documentation before making architectural changes.**

## Anti-Patterns to Avoid

1. **Don't organize by technical layer** (Commands/, Queries/, Services/ at root)
2. **Don't put shared code in Features/** - use `Game.Core/Behaviors/` or `Game.Core/Services/`
3. **Don't bypass validation** - let `ValidationBehavior` handle it
4. **Don't use Bash for file operations** - use Read/Write/Edit tools
5. **Don't create documentation files** unless explicitly requested
6. **Don't add comments to unchanged code** - only document new, complex logic

## Working with Git

This project uses conventional commit messages and includes pre-commit hooks. When creating commits:
- Focus on "why" not "what" in commit messages
- Include project context (e.g., "Game.Core:", "ContentBuilder:")
- Let automated tooling add co-author footers

## Getting Started Checklist

When working on this codebase:
1. Run `dotnet build` to ensure everything compiles
2. Run `dotnet test` to verify tests pass (375+ tests, 98%+ pass rate)
3. Read `docs/VERTICAL_SLICE_QUICK_REFERENCE.md` for feature development
4. Check `docs/README.md` for relevant guides
5. Follow the Vertical Slice pattern for all new features
