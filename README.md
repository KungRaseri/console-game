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
- [Save/Load Guide](./docs/guides/SAVE_LOAD_GUIDE.md) - Game persistence system
- [Test Coverage Report](./docs/testing/TEST_COVERAGE_REPORT.md) - 286 tests (100% coverage)

## Quick Start

```powershell
# Run the game
dotnet run --project Game

# Run tests (286 tests ✅)
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
- **100% Test Coverage**: 286 tests with xUnit and FluentAssertions

See the [docs/](./docs/) folder for detailed feature documentation.

## What's New - Save/Load & Skills! 💾⚔️

**Version 1.3 adds persistence and functional skill system:**

💾 **Save/Load System** - Persistent game state with LiteDB  
🔄 **Auto-Save** - Never lose progress after combat victories  
📂 **Multiple Saves** - Create and manage multiple characters  
⚔️ **Functional Skills** - 8 skills that enhance your character  
📊 **Skill Bonuses** - Damage, defense, dodge, crit, and utility boosts  
🎯 **Level-Up Rewards** - Learn skills and allocate attribute points  
🎮 **Enhanced Character View** - See all stats, skills, and bonuses  

See the [Save/Load Guide](./docs/guides/SAVE_LOAD_GUIDE.md) for complete details!

## Recent Updates

### Version 1.3 - Save/Load & Skills (December 6, 2025)
- ✅ Complete save/load system with auto-save
- ✅ 8 functional skills affecting combat and stats
- ✅ Enhanced character view with skill display
- ✅ 13 new save/load tests (286 total tests)

### Version 1.2 - Combat & Leveling (December 5, 2025)
- ✅ Turn-based combat system with enemies
- ✅ Level-up with interactive attribute allocation
- ✅ Skill learning system with 8 skills
- ✅ Enemy generation with difficulty scaling

### Version 1.1 - Inventory System (December 5, 2025)
- ✅ Complete inventory management
- ✅ Equipment slots and item stats
- ✅ Item generation and loot drops

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

**Total: 286 passing tests** ✅

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
2. ✅ ~~Combat system with turn-based battles~~ **COMPLETED!**
3. ✅ ~~Level-up and skill system~~ **COMPLETED!**
4. ✅ ~~Save/Load functionality~~ **COMPLETED!**
5. Create quest system with objectives and rewards
6. Add magic spell system (use Arcane Knowledge bonus)
7. Implement shop/economy system
8. Add dungeon zones with procedural generation
9. Create status effects (poison, stun, burning)
10. Add achievements and statistics tracking

## Resources

- **📚 [Full Documentation](./docs/)** - Complete guides and references
- **🎒 [Inventory Guide](./docs/guides/INVENTORY_GUIDE.md)** - Full inventory system documentation
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [.NET 9 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [xUnit Documentation](https://xunit.net/)

---

**Last Updated**: December 6, 2025  
**Current Version**: v1.3 - Save/Load & Skills System  
**Test Coverage**: 286 tests passing ✅ (100% coverage)  
**Framework**: .NET 9.0  
**New Features**: Save/Load System 💾 + Functional Skills ⚔️
