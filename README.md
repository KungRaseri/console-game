# RealmEngine

A powerful RPG framework built with .NET 9, showcasing modern C# architecture patterns and comprehensive game systems. RealmEngine provides the foundation for creating fantasy adventures, with **RealmForge** as the companion data editor tool.

## 🏗️ Architecture

**Vertical Slice Architecture + CQRS Pattern**

**RealmEngine** consists of multiple libraries working together:
- **Game.Core** - Core game mechanics, combat, inventory, character systems
- **Game.Data** - JSON data management and persistence layer  
- **Game.Shared** - Common utilities, models, and services
- **RealmForge** - WPF desktop application for editing game data (JSON files)

This project uses **Vertical Slice Architecture** with **CQRS** (Command Query Responsibility Segregation) using **MediatR** for a clean, maintainable codebase organized by business features.

- 📐 **[Vertical Slice Migration Summary](./docs/VERTICAL_SLICE_MIGRATION_SUMMARY.md)** - Complete migration details
- 🚀 **[Developer Quick Reference](./docs/VERTICAL_SLICE_QUICK_REFERENCE.md)** - How to add new features

**Key Benefits:**
- ✅ Code organized by **business capability** (Features/Combat, Features/Inventory, etc.)
- ✅ Clear separation: **Commands** (write) vs **Queries** (read)
- ✅ Automatic **validation** and **logging** via MediatR pipeline behaviors
- ✅ **CQRS handlers** across multiple game features (Combat, Inventory, CharacterCreation, SaveLoad, Exploration)

## 📚 Documentation

**Complete documentation is available in the [docs/](./docs/) folder:**

- 🎮 **[Game Design Document (GDD-Main.md)](./docs/GDD-Main.md)** - **START HERE!** Complete game overview, systems, and roadmap
- � **[Vertical Slice Migration Summary](./docs/VERTICAL_SLICE_MIGRATION_SUMMARY.md)** - Complete migration details
- 🚀 **[Developer Quick Reference](./docs/VERTICAL_SLICE_QUICK_REFERENCE.md)** - How to add new features
- 📖 **[Documentation Index](./docs/README.md)** - All guides and implementation notes

**Quick Guide Links:**
- [Game Loop Guide](./docs/guides/GAME_LOOP_GUIDE.md) - Understanding the GameEngine architecture
- [Inventory System Guide](./docs/guides/INVENTORY_GUIDE.md) - Complete item management system
- [Settings Guide](./docs/guides/SETTINGS_GUIDE.md) - Configuration management
- [ConsoleUI Guide](./docs/guides/CONSOLEUI_GUIDE.md) - Using Spectre.Console UI components
- [Save/Load Guide](./docs/guides/SAVE_LOAD_GUIDE.md) - Game persistence system
- [Test Coverage Report](./docs/testing/TEST_COVERAGE_REPORT.md) - Comprehensive test suite with high pass rate

## Quick Start

```powershell
# Build the RealmEngine framework
dotnet build

# Run the test suite
dotnet test

# Launch RealmForge data editor
dotnet run --project RealmForge

# Debug in VS Code
Press F5
```

## ⚡ Features

### Game Engine & Architecture
- **State Machine**: GameEngine with event-driven architecture (MediatR)
- **Error Handling**: Retry logic and resilience patterns (Polly)
- **Settings System**: Microsoft.Extensions.Configuration with validation
- **Logging**: Structured logging (Serilog) to console and files

### Gameplay Features
- **D20 System**: Full attribute system (STR, DEX, CON, INT, WIS, CHA) with derived stats
- **Character Classes**: 6 classes (Warrior, Rogue, Mage, Cleric, Ranger, Paladin) with unique bonuses
- **Turn-Based Combat**: Attack, defend, use items - with dodge, crit, and blocking mechanics
- **Level-Up System**: Interactive attribute allocation and skill learning
- **Skills**: 8 learnable skills that enhance combat and character abilities
- **Inventory System**: Full item management with equipment slots, consumables, and sorting
- **Item Generation**: Random loot drops with 5 rarity tiers (Common to Legendary)
- **Save/Load**: Persistent game state with auto-save and multiple character support
- **Enemy AI**: Procedurally generated enemies with difficulty scaling

### User Interface & Experience
- **Rich Console UI**: Beautiful interactive displays (Spectre.Console)
- **Data Persistence**: Save/load game state (LiteDB)
- **Audio Support**: Background music and sound effects (NAudio)

### Development & Testing  
- **Validation**: Robust input checking (FluentValidation)
- **Procedural Generation**: Random NPCs and items (Bogus)
- **Natural Language**: Number formatting and pluralization (Humanizer)
- **Test Coverage**: Comprehensive test suite with xUnit and FluentAssertions

See the [docs/](./docs/) folder for detailed feature documentation.

## 🔧 RealmForge - Data Editor

**RealmForge** is the companion WPF desktop application for editing RealmEngine's game data:

- **JSON Editor** - Visual editor for all game data files (164+ files)
- **Schema Validation** - Real-time validation against JSON v4.0 standards  
- **Reference System** - Support for v4.1 cross-references between data files
- **Live Preview** - See changes instantly without restarting
- **Data Compliance** - Ensures all JSON follows established patterns

**Launch RealmForge:**
```powershell
dotnet run --project RealmForge
```

**Key Features:**
- Edit abilities, classes, enemies, items, NPCs, and quests
- Material Design UI with dark/light themes
- Drag-and-drop pattern building for names generation
- Automatic backup and version control integration
- Hot reload support for rapid iteration

## Architecture Highlights

🏗️ **Modern Design Patterns**
- **Vertical Slice Architecture** - Features organized by business capability
- **CQRS with MediatR** - Clean separation of commands and queries
- **Event-Driven Architecture** - Loosely coupled components
- **Domain-Driven Design** - Rich domain models and services

🧪 **Quality & Testing**
- **Comprehensive Test Suite** - Unit, integration, and JSON compliance tests
- **High Test Coverage** - Extensive validation of game mechanics
- **Automated CI/CD** - Quality gates and continuous integration
- **JSON Schema Validation** - Data integrity across 164+ game data files

🎯 **Enterprise Patterns**
- **Dependency Injection** - Microsoft.Extensions.DI
- **Configuration Management** - Strongly-typed settings
- **Structured Logging** - Serilog with multiple sinks
- **Resilience Patterns** - Polly for retry logic

## Building the Project

```powershell
# Build the entire RealmEngine solution
dotnet build

# Build specific components
dotnet build Game.Core        # Core engine
dotnet build Game.Data        # Data layer
dotnet build Game.Shared      # Shared utilities
dotnet build RealmForge       # Data editor tool

# Run all tests
dotnet test

# Launch RealmForge
dotnet run --project RealmForge
```

## Development

### Adding New Models
Create classes in the `Models/` folder:
```csharp
namespace Game.Models;

public class Enemy
{
    public string Name { get; set; }
    public int Health { get; set; }
}
```

### Creating Validators
Use FluentValidation in the `Validators/` folder:
```csharp
using FluentValidation;
using Game.Models;

namespace Game.Validators;

public class EnemyValidator : AbstractValidator<Enemy>
{
    public EnemyValidator()
    {
        RuleFor(e => e.Name).NotEmpty();
        RuleFor(e => e.Health).GreaterThan(0);
    }
}
```

### Generating Random Data
Use Bogus generators in the `Generators/` folder:
```csharp
using Bogus;
using Game.Models;

namespace Game.Generators;

public static class EnemyGenerator
{
    private static readonly Faker<Enemy> EnemyFaker = new Faker<Enemy>()
        .RuleFor(e => e.Name, f => f.Name.FirstName())
        .RuleFor(e => e.Health, f => f.Random.Int(50, 200));

    public static Enemy Generate() => EnemyFaker.Generate();
}
```

### Using Events
Define events in `Handlers/GameEvents.cs`:
```csharp
public record EnemyDefeated(string EnemyName, int XpGained) : INotification;
```

Create handlers in `Handlers/EventHandlers.cs`:
```csharp
public class EnemyDefeatedHandler : INotificationHandler<EnemyDefeated>
{
    public Task Handle(EnemyDefeated notification, CancellationToken ct)
    {
        ConsoleUI.ShowSuccess($"Defeated {notification.EnemyName}!");
        return Task.CompletedTask;
    }
}
```

### Saving Data
Use LiteDB repositories in the `Data/` folder:
```csharp
using (var repo = new SaveGameRepository())
{
    var save = new SaveGame { PlayerName = "Hero" };
    repo.Save(save);
}
```

## Libraries Used

- **Spectre.Console** - Rich console UI
- **LiteDB** - NoSQL database
- **Newtonsoft.Json** - JSON serialization
- **NAudio** - Audio playback
- **FluentValidation** - Input validation
- **Bogus** - Fake data generation
- **Humanizer** - Natural language formatting
- **MediatR** - Event-driven patterns
- **Polly** - Resilience patterns
- **Serilog** - Structured logging
- **xUnit** - Unit testing
- **FluentAssertions** - Test assertions

## Testing

The project includes a comprehensive test suite using xUnit and FluentAssertions.

### Running Tests

To run all tests:

```powershell
dotnet test
```

Or run specific test files:

```powershell
dotnet test --filter "FullyQualifiedName~CharacterTests"
```

### Test Structure

```
Game.Tests/
├── Models/                      # Model tests
│   └── CharacterTests.cs       # Character behavior tests
├── Validators/                  # Validation tests
│   └── CharacterValidatorTests.cs
├── Generators/                  # Generator tests
│   ├── ItemGeneratorTests.cs
│   └── NpcGeneratorTests.cs
└── Game.Tests.csproj
```

### Test Coverage

Current test coverage includes:
- **Character Model**: Initialization, XP gain, leveling, stat increases (7 tests)
- **Character Validation**: Name, level, health, mana validation (6 tests)
- **Item Generator**: Item creation, type filtering, unique items (6 tests)
- **NPC Generator**: NPC creation, data variety, realistic data (5 tests)

**All tests passing** ✅

### Writing Tests

Example test with FluentAssertions:

```csharp
[Fact]
public void Character_Should_Level_Up_When_Gaining_100_XP()
{
    // Arrange
    var character = new Character { Level = 1, Experience = 0 };
    
    // Act
    character.GainExperience(100);
    
    // Assert
    character.Level.Should().Be(2);
    character.Experience.Should().Be(0);
}
```

Example validation test:

```csharp
[Fact]
public void Should_Have_Error_When_Name_Is_Empty()
{
    // Arrange
    var validator = new CharacterValidator();
    var character = new Character { Name = "" };
    
    // Act & Assert
    validator.ShouldHaveValidationErrorFor(c => c.Name, character);
}
```

## Development Roadmap

The project follows a feature-driven development approach using vertical slices:

**Core Engine Features**
- Quest system with objectives and rewards
- Magic spell system with mana management
- Shop/economy system with dynamic pricing
- Status effects system (poison, stun, burning, etc.)

**Content & World Building**
- Dungeon zones with procedural generation
- Achievement and statistics tracking
- Equipment enchantment system
- NPC dialogue trees

**Quality of Life**
- Hot reload for JSON data files
- Console game controller support
- Audio system enhancements
- Performance optimizations

## Architecture Resources

**Documentation**
- **[Game Design Document](./docs/GDD-Main.md)** - Complete game specification
- **[Vertical Slice Quick Reference](./docs/VERTICAL_SLICE_QUICK_REFERENCE.md)** - Adding new features
- **[Architecture Documentation](./docs/)** - All implementation guides

**External Resources**
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)

---

**Framework**: .NET 9.0 with C# 13  
**Architecture**: Vertical Slice + CQRS Pattern  
**UI Framework**: Spectre.Console  
**Database**: LiteDB for persistence  
**Testing**: xUnit with FluentAssertions
