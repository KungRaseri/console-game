# Console Game

A .NET Core Console application written in C# for building feature-rich console-based games.

## 📚 Documentation

**Complete documentation is available in the [docs/](./docs/) folder:**

- 📖 **[User Guides](./docs/guides/)** - How to use the game systems and libraries
- 🔧 **[Implementation Notes](./docs/implementation/)** - Technical decisions and summaries
- 🧪 **[Testing](./docs/testing/)** - Test coverage reports and guidelines

**Quick Links:**
- [Game Loop Guide](./docs/guides/GAME_LOOP_GUIDE.md) - Understanding the GameEngine architecture
- [Inventory System Guide](./docs/guides/INVENTORY_GUIDE.md) - Complete item management system
- [Settings Guide](./docs/guides/SETTINGS_GUIDE.md) - Configuration management
- [ConsoleUI Guide](./docs/guides/CONSOLEUI_GUIDE.md) - Using Spectre.Console UI components
- [Test Coverage Report](./docs/testing/TEST_COVERAGE_REPORT.md) - 176 tests (100% coverage)

## Quick Start

```powershell
# Run the game
dotnet run --project Game

# Run tests (176 tests ✅)
dotnet test Game.Tests

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
- **Inventory System**: Full item management with equipment slots, consumables, and sorting
- **Item Generation**: Random loot drops with 5 rarity tiers (Common to Legendary)
- **Character Progression**: XP gain, leveling, stat increases
- **Exploration**: Adventure and find treasures

### User Interface & Experience
- **Rich Console UI**: Beautiful interactive displays (Spectre.Console)
- **Data Persistence**: Save/load game state (LiteDB)
- **Audio Support**: Background music and sound effects (NAudio)

### Development & Testing  
- **Validation**: Robust input checking (FluentValidation)
- **Procedural Generation**: Random NPCs and items (Bogus)
- **Natural Language**: Number formatting and pluralization (Humanizer)
- **100% Test Coverage**: 176 tests with xUnit and FluentAssertions

See the [docs/](./docs/) folder for detailed feature documentation.

## What's New - Inventory System! 🎒

**Version 1.1 adds a complete inventory management system:**

✨ **Find Items** - 30% chance to discover items while exploring  
🎒 **Manage Inventory** - View, sort, and organize your items  
⚔️ **Equip Gear** - Weapon, armor, and accessory slots  
💊 **Use Consumables** - Health and mana potions with rarity-based effects  
📊 **Smart Sorting** - Sort by name, type, rarity, or value  
🎲 **Procedural Loot** - Random generation with 5 rarity tiers  

See the [Inventory Guide](./docs/guides/INVENTORY_GUIDE.md) for complete details!

## Building the Project

To build the project, run:

```powershell
dotnet build
```

Or use the VS Code build task (Press `Ctrl+Shift+B`).

## Running the Project

To run the application, use:

```powershell
dotnet run --project Game
```

## Debugging

To debug the application:
1. Press `F5` or go to Run and Debug view
2. Select ".NET Core Launch (console)" configuration
3. Press the green play button

The application will start in debug mode with full color support in the integrated terminal.

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

**Total: 38 passing tests** ✅

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

## Next Steps

1. ✅ ~~Inventory system with item management~~ **COMPLETED!**
2. Implement combat system with turn-based battles
3. Add integration tests for save/load functionality
4. Create quest system with objectives and rewards
5. Add command-line interface with Spectre.Console.Cli
6. Integrate audio files for immersive experience

## Resources

- **📚 [Full Documentation](./docs/)** - Complete guides and references
- **🎒 [Inventory Guide](./docs/guides/INVENTORY_GUIDE.md)** - Full inventory system documentation
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [xUnit Documentation](https://xunit.net/)

---

**Last Updated**: December 5, 2025  
**Test Coverage**: 176 tests passing ✅ (100% coverage)  
**Framework**: .NET 9.0  
**New in v1.1**: Complete Inventory System 🎒
