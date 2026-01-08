# .NET Core Console Game Application

This is a .NET Core Console application in C# for building a console-based game.

## Project Type

- **Language**: C#
- **Framework**: .NET Core (.NET 9.0)
- **Type**: Console Application (Game)
- **UI Library**: Spectre.Console v0.54.0

## Project Structure

```
console-game/
├── Game/
│   ├── ConsoleUI.cs           # Spectre.Console wrapper with best practices
│   ├── Program.cs             # Main entry point
│   └── Game.csproj            # Project file with Spectre.Console dependency
├── .vscode/
│   ├── launch.json            # Debug configuration
│   └── tasks.json             # Build tasks
├── .github/
│   └── copilot-instructions.md
├── docs/
│   └── standards/json/        # JSON v4.0 standards
│       ├── NAMES_JSON_STANDARD.md
│       ├── CATALOG_JSON_STANDARD.md
│       ├── CBCONFIG_STANDARD.md
│       └── README.md
├── SPECTRE_BEST_PRACTICES.md  # Security and usage guidelines
└── README.md
```

## Completed Setup

- ✅ Created project structure with `dotnet new console`
- ✅ Added Spectre.Console v0.54.0 package
- ✅ Added Spectre.Console.Cli v0.53.1 for command-line parsing
- ✅ Added LiteDB v5.0.21 for game data persistence
- ✅ Added Newtonsoft.Json v13.0.4 for JSON serialization
- ✅ Added NAudio v2.2.1 for sound effects and music
- ✅ Added FluentValidation v12.1.1 for input validation
- ✅ Added Bogus v35.6.5 for procedural content generation
- ✅ Added Humanizer v3.0.1 for natural language formatting
- ✅ Added MediatR v14.0.0 for event-driven architecture
- ✅ Added Polly v8.6.5 for resilience patterns
- ✅ Added Serilog v4.3.0 with Console and File sinks for logging
- ✅ Added xUnit v2.9.3 and FluentAssertions v8.8.0 for testing
- ✅ Created `ConsoleUI` wrapper class with security best practices
- ✅ Compiled successfully with `dotnet build`
- ✅ Created VS Code build and debug tasks
- ✅ Added README.md and best practices documentation
- ✅ Created Game.Tests project with comprehensive test coverage
- ✅ All 38 tests passing (Character, Validation, Generators)
- ✅ Established JSON v4.0 standards for all game data files
- ✅ Created RealmForge WPF application for JSON editing
- ✅ Integrated JSON v4.1 reference system into ContentBuilder
- ✅ All 35 integration tests passing (ReferenceResolverService)
- ✅ All 33 unit tests passing (ReferenceResolverService)
- ✅ Created 857 JSON compliance tests for 164 data files
- ✅ Fixed ContentBuilder startup crash (duplicate dictionary key)
- ✅ Consolidated abilities to category-level catalogs (v4.2)
- ✅ Fixed all socketable generators for dual-path support
- ✅ **Core.Tests**: 846/846 passing (100%)
- ✅ **Shared.Tests**: 665/665 passing (100%)
- ✅ **Data.Tests**: 5,250/5,250 passing (100%)

## JSON Data Standards (v4.0 + v4.1 References)

**All game data files follow strict standards documented in `docs/standards/json/`:**

### JSON Reference System v4.1

**Purpose**: Unified system for linking game data across domains to eliminate duplication

**Reference Syntax**: `@domain/path/category:item-name[filters]?.property.nested`

**Common Reference Patterns**:
- Abilities: `@abilities/active/offensive:basic-attack`
- Classes: `@classes/warriors:fighter`
- Items: `@items/weapons/swords:iron-longsword`
- Enemies: `@enemies/humanoid:goblin-warrior`
- NPCs: `@npcs/merchants:blacksmith`
- Quests: `@quests/main-story:chapter-1`

**Features**:
- Direct references: Link to specific items
- Property access: Use dot notation (`.property.nested`)
- Wildcard selection: `:*` for random item respecting rarityWeight
- Optional references: `?` suffix returns null instead of error
- Filtering: Support for operators (=, !=, <, <=, >, >=, EXISTS, NOT EXISTS, MATCHES)

**Documentation**: `docs/standards/json/JSON_REFERENCE_STANDARDS.md`

### names.json Standard (Pattern Generation)

**Required Fields:**
- `version`: "4.0"
- `type`: "pattern_generation"
- `supportsTraits`: true or false
- `lastUpdated`: ISO date string
- `description`: Purpose of the file
- `patterns[]`: Array with `rarityWeight` (NOT "weight")
- `components{}`: Component arrays (prefix, suffix, etc.)

**Pattern Syntax:**
- Component tokens: `{base}`, `{prefix}`, `{suffix}`, `{quality}`
- External references: Use v4.1 syntax `@items/materials/metals` instead of old `[@materialRef/weapon]`
- NO "example" fields allowed

### catalog.json Standard (Item/Enemy Definitions)

**Required Metadata:**
- `description`, `version`, `lastUpdated`, `type` (ends with "_catalog")

**Structure:**
- All items MUST have `name` and `rarityWeight`
- Physical "weight" allowed (item weight in lbs)
- Use references instead of hardcoded names (e.g., `@abilities/...` not "Basic Attack")

### .cbconfig.json Standard (ContentBuilder UI)

**Required Fields:**
- `icon`: MaterialDesign icon name (NOT emojis)
- `sortOrder`: Integer for tree position

### Compliance Status

✅ **JSON v4.1 Reference System (December 28, 2025)**
- **classes/catalog.json**: ✅ All abilities and parentClass use references
- **classes/progression.json**: ✅ Merged into catalog.json (deleted)

✅ **JSON v4.0 Standards Compliance (December 29, 2025)**
- **Phase 5 Comprehensive Testing**: 857 automated tests created
- **Total Files**: 164 (61 catalogs + 38 names + 65 configs)
- **Overall Compliance**: 164/164 files (100%) ✅
  - ✅ **.cbconfig.json**: 65/65 (100% compliant)
  - ✅ **names.json**: 38/38 (100% compliant)
  - ✅ **catalog.json**: 61/61 (100% compliant)

**All JSON data files are now fully compliant with v4.0 standards!**

**See**: [JSON_DATA_COMPLIANCE_REPORT.md](../docs/JSON_DATA_COMPLIANCE_REPORT.md)

**Standards Documentation:**
- `docs/standards/json/JSON_REFERENCE_STANDARDS.md` - **NEW v4.1**
- `docs/standards/json/NAMES_JSON_STANDARD.md`
- `docs/standards/json/CATALOG_JSON_STANDARD.md`
- `docs/standards/json/CBCONFIG_STANDARD.md`
- `docs/standards/json/README.md`

## Dependencies

### UI & Console
- **Spectre.Console v0.54.0** - Rich console UI with colors, tables, prompts, progress bars
- **Spectre.Console.Cli v0.53.1** - Command-line argument parsing and subcommands

### Data & Persistence
- **LiteDB v5.0.21** - Lightweight NoSQL database for save files and game data
- **Newtonsoft.Json v13.0.4** - JSON serialization for configuration and data storage

### Audio
- **NAudio v2.2.1** - Sound effects and background music playback

### Validation & Data Generation
- **FluentValidation v12.1.1** - Robust input validation with custom rules
- **Bogus v35.6.5** - Realistic fake data generation for NPCs, items, and content

### Utilities
- **Humanizer v3.0.1** - Natural language text formatting (numbers to words, pluralization)
- **MediatR v14.0.0** - Event-driven architecture and command/query separation
- **Polly v8.6.5** - Resilience patterns (retry logic, circuit breakers)

### Logging
- **Serilog v4.3.0** - Structured logging framework
- **Serilog.Sinks.Console v6.1.1** - Console output for logs
- **Serilog.Sinks.File v7.0.0** - File-based logging

### Testing
- **xUnit v2.9.3** - Unit testing framework
- **xUnit.runner.visualstudio v3.1.5** - Visual Studio test runner integration
- **FluentAssertions v8.8.0** - Readable and expressive test assertions

## How to Use

- **Build**: Press `Ctrl+Shift+B` or run `dotnet build`
- **Run**: Use `dotnet run --project Game`
- **Debug**: Press `F5` in VS Code (uses integrated terminal for color support)
- **Watch Mode**: Run the "watch" task for auto-reload during development
- **Run Tests**: Use `dotnet test` to run all 38 unit tests

## Testing

### Test Project Structure

```
Game.Tests/
├── Models/                      # Model tests
│   └── CharacterTests.cs       # 7 tests
├── Validators/                  # Validation tests
│   └── CharacterValidatorTests.cs  # 6 tests
├── Generators/                  # Generator tests
│   ├── ItemGeneratorTests.cs   # 6 tests
│   └── NpcGeneratorTests.cs    # 5 tests
└── Game.Tests.csproj
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~CharacterTests"

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage (38 tests ✅)

- **CharacterTests**: Model behavior, XP gain, leveling, stat increases
- **CharacterValidatorTests**: Name, level, health, mana validation
- **ItemGeneratorTests**: Item creation, type filtering, uniqueness
- **NpcGeneratorTests**: NPC generation, data variety, realism

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

Example validation test using FluentValidation.TestHelper:

```csharp
[Fact]
public void Should_Have_Error_When_Name_Is_Empty()
{
    // Arrange
    var validator = new CharacterValidator();
    
    // Act & Assert
    validator.ShouldHaveValidationErrorFor(c => c.Name, new Character { Name = "" });
}
```

## Spectre.Console Capabilities

### Available Features in ConsoleUI Wrapper

The `ConsoleUI` class provides secure, easy-to-use methods for:

#### Text Output
- `WriteColoredText(string)` - Markup-enabled colored text (trusted content only)
- `WriteText(string)` - Safe plain text (auto-escapes markup)
- `ShowBanner(string, string)` - Styled title banners with rules
- `ShowSuccess/Error/Warning/Info(string)` - Status messages with icons

#### User Input
- `AskForInput(string)` - Text input with prompt
- `AskForNumber(string, int, int)` - Numeric input with validation
- `ShowMenu(string, params string[])` - Single-selection menu (keyboard navigation)
- `ShowMultiSelectMenu(string, params string[])` - Multi-selection menu
- `Confirm(string)` - Yes/No confirmation prompt

#### Layout & Display
- `ShowTable(string, string[], List<string[]>)` - Tables with headers and rows
- `ShowPanel(string, string, string)` - Bordered panels with titles
- `ShowProgress(string, Action<ProgressTask>)` - Progress bars
- `Clear()` - Clear console screen
- `PressAnyKey(string)` - Wait for key press

### Advanced Spectre.Console Features
Beyond the wrapper, Spectre.Console supports:

- **Charts**: Bar charts, breakdown charts
- **Tree Views**: Hierarchical data display
- **Live Display**: Real-time updating content
- **Calendars**: Month/date displays
- **FigletText**: ASCII art text
- **Canvas**: Image rendering in terminal
- **Status Spinners**: Animated loading indicators
- **Grid Layouts**: Complex multi-column layouts
- **Exceptions**: Pretty exception formatting
- **JSON**: Syntax-highlighted JSON display

### Using Advanced Features

To use features not in the wrapper, import directly:
```csharp
using Spectre.Console;

// Example: FigletText for large ASCII art
AnsiConsole.Write(
    new FigletText("GAME TITLE")
        .Centered()
        .Color(Color.Green));

// Example: Tree view
var root = new Tree("Inventory");
root.AddNode("[yellow]Weapons[/]")
    .AddNode("Sword")
    .AddNode("Bow");
root.AddNode("[blue]Potions[/]")
    .AddNode("Health Potion x5");
AnsiConsole.Write(root);

// Example: Live display (auto-updating)
await AnsiConsole.Live(new Panel("Loading..."))
    .StartAsync(async ctx =>
    {
        // Update display in real-time
        ctx.UpdateTarget(new Panel("50%..."));
        await Task.Delay(500);
        ctx.UpdateTarget(new Panel("100%!"));
    });
```

## Security Best Practices

⚠️ **IMPORTANT**: Always escape user input to prevent markup injection!

### Safe Methods (Auto-Escape)
- `ConsoleUI.WriteText()` - Safe for any input
- `ConsoleUI.ShowSuccess/Error/Warning/Info()` - Messages are escaped
- `ConsoleUI.ShowBanner()` - Title and subtitle are escaped
- `ConsoleUI.AskForInput()` - Prompt is escaped

### Unsafe Methods (No Escaping)
- `ConsoleUI.WriteColoredText()` - Use ONLY with trusted/static text
- `ConsoleUI.ShowPanel()` - Content supports markup (escape manually if needed)
- `ConsoleUI.ShowTable()` - Table data is not escaped

### Manual Escaping
For advanced usage outside the wrapper:
```csharp
// BAD - vulnerable to injection
string userInput = "[red]hacked[/]";
AnsiConsole.MarkupLine(userInput); // Would render as red text!

// GOOD - safe from injection
string userInput = "[red]hacked[/]";
string safe = userInput.Replace("[", "[[").Replace("]", "]]");
AnsiConsole.MarkupLine(safe); // Displays literal brackets
```

## Color Support

Spectre.Console automatically detects terminal capabilities:
- **True Color** (24-bit RGB): Modern terminals
- **256 Colors**: Standard terminals
- **16 Colors**: Basic terminals
- **No Color**: Fallback mode

Test with different profiles:
- `AnsiConsole.Profile.Capabilities.ColorSystem`

## Development Guidelines

### For Game Development
1. **Use `ShowMenu()` for all player choices** - keyboard accessible
2. **Validate numeric input** with `AskForNumber()` - prevents crashes
3. **Use progress bars** for loading/long operations
4. **Display game stats** in tables or panels
5. **Use colored text** for game states (health, mana, etc.)
6. **Create immersive title screens** with FigletText
7. **Show inventory** with tree views or tables
8. **Always escape player names** and user-generated content

### Code Style
- Use the `ConsoleUI` wrapper for common operations
- For complex layouts, use Spectre.Console directly
- Keep UI code separate from game logic
- Test with different terminal sizes
- Consider adding a "Safe Mode" for minimal color support

## Debugging

- **Launch Configuration**: Set to use `integratedTerminal` for proper color support
- **Console Input**: Works correctly in VS Code integrated terminal
- **Breakpoints**: Full debugger support with F5
- **Watch Variables**: Inspect game state during debugging

## Resources

- **Spectre.Console Docs**: https://spectreconsole.net/
- **GitHub Repository**: https://github.com/spectreconsole/spectre.console
- **Best Practices**: See `SPECTRE_BEST_PRACTICES.md` in workspace
- **Examples**: https://spectreconsole.net/examples

## Performance Notes

- Rendering is fast for most use cases
- Live displays can impact performance if updated too frequently
- Large tables should use pagination
- Complex layouts should be pre-rendered when possible

## Library Usage Guide

### Spectre.Console.Cli - Command-line Arguments
```csharp
// Create commands for different game modes
public class NewGameCommand : Command<NewGameSettings>
{
    public override int Execute(CommandContext context, NewGameSettings settings)
    {
        // Start new game with settings
        return 0;
    }
}

// Configure app with commands
var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<NewGameCommand>("new");
    config.AddCommand<LoadGameCommand>("load");
    config.AddCommand<SettingsCommand>("settings");
});
```

### LiteDB - Game Data Persistence
```csharp
// Save game data
using var db = new LiteDatabase("game.db");
var saves = db.GetCollection<SaveGame>("saves");
saves.Insert(new SaveGame { PlayerName = "Hero", Level = 5 });

// Query saved games
var allSaves = saves.FindAll().ToList();
```

### NAudio - Sound Effects & Music
```csharp
// Play background music
using var audioFile = new AudioFileReader("music.mp3");
using var outputDevice = new WaveOutEvent();
outputDevice.Init(audioFile);
outputDevice.Play();

// Play sound effect
using var sfx = new AudioFileReader("sword.wav");
using var output = new WaveOutEvent();
output.Init(sfx);
output.Play();
```

### FluentValidation - Input Validation
```csharp
// Define validation rules
public class CharacterValidator : AbstractValidator<Character>
{
    public CharacterValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .Length(3, 20)
            .Matches("^[a-zA-Z]+$");
        
        RuleFor(c => c.Level)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

// Validate
var validator = new CharacterValidator();
var result = validator.Validate(character);
if (!result.IsValid)
{
    foreach (var error in result.Errors)
        ConsoleUI.ShowError(error.ErrorMessage);
}
```

### Bogus - Procedural Content Generation
```csharp
// Generate random NPCs
var npcFaker = new Faker<NPC>()
    .RuleFor(n => n.Name, f => f.Name.FullName())
    .RuleFor(n => n.Age, f => f.Random.Int(18, 80))
    .RuleFor(n => n.Occupation, f => f.Name.JobTitle())
    .RuleFor(n => n.Gold, f => f.Random.Int(10, 500));

var npcs = npcFaker.Generate(10);

// Generate random items
var itemFaker = new Faker<Item>()
    .RuleFor(i => i.Name, f => f.Commerce.ProductName())
    .RuleFor(i => i.Price, f => f.Random.Int(5, 100))
    .RuleFor(i => i.Rarity, f => f.PickRandom<Rarity>());

var loot = itemFaker.Generate(50);
```

### Humanizer - Natural Language Formatting
```csharp
// Convert numbers to words
int gold = 1234;
ConsoleUI.WriteText($"You have {gold.ToWords()} gold coins");
// Output: "You have one thousand two hundred and thirty-four gold coins"

// Pluralization
int enemies = 5;
ConsoleUI.WriteText($"{enemies} {"enemy".ToQuantity(enemies)}");
// Output: "5 enemies"

// Time formatting
TimeSpan elapsed = TimeSpan.FromMinutes(75);
ConsoleUI.WriteText($"Play time: {elapsed.Humanize()}");
// Output: "Play time: 1 hour, 15 minutes"

// Ordinal numbers
int position = 3;
ConsoleUI.WriteText($"You finished in {position.Ordinalize()} place");
// Output: "You finished in 3rd place"
```

### MediatR - Event-Driven Architecture
```csharp
// Define events
public record PlayerLeveledUp(int NewLevel) : INotification;

// Handle events
public class LevelUpHandler : INotificationHandler<PlayerLeveledUp>
{
    public Task Handle(PlayerLeveledUp notification, CancellationToken ct)
    {
        ConsoleUI.ShowSuccess($"Level up! You are now level {notification.NewLevel}!");
        return Task.CompletedTask;
    }
}

// Publish events
await mediator.Publish(new PlayerLeveledUp(6));
```

### Polly - Resilience & Retry Logic
```csharp
// Retry file operations
var retryPolicy = Policy
    .Handle<IOException>()
    .WaitAndRetry(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

retryPolicy.Execute(() =>
{
    File.WriteAllText("save.json", gameData);
});

// Circuit breaker for external services
var circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1));
```

### Serilog - Structured Logging
```csharp
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/game-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Use structured logging
Log.Information("Player {PlayerName} started new game", playerName);
Log.Warning("Low health: {Health}/{MaxHealth}", health, maxHealth);
Log.Error(ex, "Failed to save game for player {PlayerId}", playerId);

// Context enrichment
using (LogContext.PushProperty("SessionId", sessionId))
{
    Log.Information("Battle started");
}
```

### xUnit & FluentAssertions - Testing
```csharp
// Test with FluentAssertions
public class CharacterTests
{
    [Fact]
    public void Character_Should_Level_Up_Correctly()
    {
        // Arrange
        var character = new Character { Level = 1, Experience = 0 };
        
        // Act
        character.GainExperience(100);
        
        // Assert
        character.Level.Should().Be(2);
        character.Experience.Should().Be(0);
    }
    
    [Theory]
    [InlineData(10, 20, 30)]
    [InlineData(5, 5, 10)]
    public void Inventory_Should_Stack_Items(int count1, int count2, int expected)
    {
        // Arrange
        var inventory = new Inventory();
        
        // Act
        inventory.AddItem("Potion", count1);
        inventory.AddItem("Potion", count2);
        
        // Assert
        inventory.GetItemCount("Potion").Should().Be(expected);
    }
}
```

## Architecture Recommendations

### Project Organization
```
Game/
├── Commands/          # CLI commands (using Spectre.Console.Cli)
├── Models/            # Game entities (Character, Item, Enemy, etc.)
├── Services/          # Game logic services
├── Handlers/          # MediatR event handlers
├── Validators/        # FluentValidation validators
├── Data/              # LiteDB repositories
├── Audio/             # NAudio sound management
├── Generators/        # Bogus data generators
└── ConsoleUI.cs       # Spectre.Console wrapper
```

### Best Practices
1. **Separation of Concerns**: Keep game logic separate from UI code
2. **Event-Driven**: Use MediatR for decoupled game events
3. **Validation**: Validate all user input with FluentValidation
4. **Logging**: Log important game events with Serilog
5. **Testing**: Write unit tests for game mechanics with xUnit
6. **Persistence**: Use LiteDB for simple data, consider adding indexes
7. **Audio**: Dispose of audio resources properly to prevent memory leaks
8. **Error Handling**: Use Polly for retry logic on I/O operations
