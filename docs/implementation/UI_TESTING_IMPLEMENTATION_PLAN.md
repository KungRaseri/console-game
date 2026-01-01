# UI Testing Implementation Plan

**Date**: December 9, 2025  
**Goal**: Enable unit testing of UI-dependent methods using Spectre.Console.Testing  
**Issue**: 4 tests are currently skipped due to UI dependencies

---

## 🎯 Objective

Refactor the ConsoleUI wrapper to support dependency injection so we can:
1. Test UI-dependent service methods with mocked console input
2. Verify console output in unit tests
3. Un-skip the 4 currently skipped tests
4. Test interactive prompts (menus, confirmations, text input)

---

## 📊 Current Situation

### Current ConsoleUI Implementation

**File**: `Game/Shared/UI/ConsoleUI.cs`  
**Type**: Static class  
**Problem**: Directly calls `AnsiConsole.MarkupLine()`, `AnsiConsole.Prompt()`, etc.  
**Impact**: Cannot be mocked or tested

```csharp
public static class ConsoleUI
{
    public static string AskForInput(string prompt)
    {
        return AnsiConsole.Ask<string>($"[green]{EscapeMarkup(prompt)}[/]");
    }
    
    public static string ShowMenu(string title, params string[] choices)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{EscapeMarkup(title)}[/]")
                .AddChoices(choices));
    }
}
```

### Skipped Tests (4 total)

1. **LoadGameServiceTests** (3 tests):
   - `LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist`
   - `LoadGameAsync_Should_Display_Available_Saves_When_Saves_Exist`
   - `DeleteSaveAsync_Should_Delete_Save_With_Confirmation`

2. **ExplorationServiceTests** (1 test):
   - `TravelToLocation_Should_Update_Current_Location`

**Reason**: All call ConsoleUI methods (ShowMenu, ShowTable, Confirm)

---

## 🔧 Solution: Refactor ConsoleUI for Dependency Injection

### Step 1: Install Spectre.Console.Testing Package

```powershell
dotnet add Game.Tests package Spectre.Console.Testing
```

### Step 2: Create IConsoleUI Interface

**File**: `Game/Shared/UI/IConsoleUI.cs` (NEW)

```csharp
namespace RealmEngine.Shared.UI;

/// <summary>
/// Interface for console UI operations, enabling dependency injection and testing
/// </summary>
public interface IConsoleUI
{
    // Text Output
    void WriteColoredText(string text);
    void WriteText(string text);
    void ShowBanner(string title, string? subtitle = null);
    
    // User Input
    string AskForInput(string prompt);
    int AskForNumber(string prompt, int min, int max);
    string ShowMenu(string title, params string[] choices);
    List<string> ShowMultiSelectMenu(string title, params string[] choices);
    bool Confirm(string question);
    
    // Display
    void ShowTable(string title, string[] headers, List<string[]> rows);
    void ShowPanel(string title, string content, string borderColor = "blue");
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
    void ShowProgress(string taskName, Action<ProgressTask> action);
    
    // Utility
    void Clear();
    void PressAnyKey(string? message = null);
}
```

### Step 3: Refactor ConsoleUI to Implement Interface

**File**: `Game/Shared/UI/ConsoleUI.cs` (REFACTOR)

**Before**:
```csharp
public static class ConsoleUI
{
    public static string AskForInput(string prompt) { ... }
}
```

**After**:
```csharp
public class ConsoleUI : IConsoleUI
{
    private readonly IAnsiConsole _console;
    
    // Constructor for dependency injection
    public ConsoleUI(IAnsiConsole console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }
    
    // Default constructor uses real AnsiConsole
    public ConsoleUI() : this(AnsiConsole.Console)
    {
    }
    
    public string AskForInput(string prompt)
    {
        string safePrompt = EscapeMarkup(prompt);
        return _console.Ask<string>($"[green]{safePrompt}[/]");
    }
    
    public string ShowMenu(string title, params string[] choices)
    {
        string safeTitle = EscapeMarkup(title);
        return _console.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]{safeTitle}[/]")
                .AddChoices(choices));
    }
    
    // ... all other methods updated similarly
}
```

### Step 4: Update Service Constructors for DI

**Services to Update**:
- LoadGameService
- ExplorationService  
- GameplayService
- InventoryOrchestrator
- CombatOrchestrator
- Any service calling ConsoleUI

**Example - LoadGameService**:

**Before**:
```csharp
public class LoadGameService
{
    private readonly SaveGameRepository _repository;
    
    public LoadGameService(SaveGameRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<(SaveGame?, bool)> LoadGameAsync()
    {
        var saves = _repository.GetAll();
        if (!saves.Any())
        {
            ConsoleUI.ShowError("No saved games found!");
            return (null, false);
        }
        
        // Display saves
        ConsoleUI.ShowTable(...);
        var choice = ConsoleUI.ShowMenu(...);
        // ...
    }
}
```

**After**:
```csharp
public class LoadGameService
{
    private readonly SaveGameRepository _repository;
    private readonly IConsoleUI _console;
    
    public LoadGameService(SaveGameRepository repository, IConsoleUI console)
    {
        _repository = repository;
        _console = console;
    }
    
    public async Task<(SaveGame?, bool)> LoadGameAsync()
    {
        var saves = _repository.GetAll();
        if (!saves.Any())
        {
            _console.ShowError("No saved games found!");
            return (null, false);
        }
        
        // Display saves
        _console.ShowTable(...);
        var choice = _console.ShowMenu(...);
        // ...
    }
}
```

### Step 5: Register IConsoleUI in DI Container

**File**: `Game/Program.cs`

```csharp
// Register ConsoleUI as singleton
services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);
services.AddSingleton<IConsoleUI, ConsoleUI>();
```

### Step 6: Create TestConsoleUI Helper

**File**: `Game.Tests/Helpers/TestConsoleHelper.cs` (NEW)

```csharp
using Spectre.Console;
using Spectre.Console.Testing;

namespace Game.Tests.Helpers;

public static class TestConsoleHelper
{
    /// <summary>
    /// Creates a test console with interactive mode enabled
    /// </summary>
    public static TestConsole CreateInteractiveConsole()
    {
        var console = new TestConsole();
        console.Interactive();
        return console;
    }
    
    /// <summary>
    /// Simulates menu selection by index (0-based)
    /// </summary>
    public static void SelectMenuOption(TestConsole console, int index)
    {
        // Navigate down to the option
        for (int i = 0; i < index; i++)
        {
            console.Input.PushKey(ConsoleKey.DownArrow);
        }
        console.Input.PushKey(ConsoleKey.Enter);
    }
    
    /// <summary>
    /// Simulates entering text and pressing Enter
    /// </summary>
    public static void EnterText(TestConsole console, string text)
    {
        console.Input.PushTextWithEnter(text);
    }
    
    /// <summary>
    /// Simulates confirming a yes/no prompt (y or n)
    /// </summary>
    public static void ConfirmPrompt(TestConsole console, bool confirm)
    {
        console.Input.PushTextWithEnter(confirm ? "y" : "n");
    }
}
```

### Step 7: Update Skipped Tests

**Example: LoadGameServiceTests**

**Before** (Skipped):
```csharp
[Fact(Skip = "LoadGameAsync requires interactive terminal")]
public async Task LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist()
{
    await Task.CompletedTask;
}
```

**After** (Testable):
```csharp
[Fact]
public async Task LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist()
{
    // Arrange
    var testConsole = TestConsoleHelper.CreateInteractiveConsole();
    var consoleUI = new ConsoleUI(testConsole);
    var repository = new SaveGameRepository(new LiteDatabase(":memory:"));
    var service = new LoadGameService(repository, consoleUI);
    
    // Act
    var (save, success) = await service.LoadGameAsync();
    
    // Assert
    save.Should().BeNull();
    success.Should().BeFalse();
    testConsole.Output.Should().Contain("No saved games found!");
}

[Fact]
public async Task LoadGameAsync_Should_Display_Available_Saves_When_Saves_Exist()
{
    // Arrange
    var testConsole = TestConsoleHelper.CreateInteractiveConsole();
    var consoleUI = new ConsoleUI(testConsole);
    var repository = new SaveGameRepository(new LiteDatabase(":memory:"));
    var service = new LoadGameService(repository, consoleUI);
    
    // Create test saves
    repository.Insert(new SaveGame { CharacterName = "Hero1", Level = 5 });
    repository.Insert(new SaveGame { CharacterName = "Hero2", Level = 10 });
    
    // Simulate user selecting first save (index 0)
    TestConsoleHelper.SelectMenuOption(testConsole, 0);
    
    // Act
    var (save, success) = await service.LoadGameAsync();
    
    // Assert
    save.Should().NotBeNull();
    save!.CharacterName.Should().Be("Hero1");
    success.Should().BeTrue();
    testConsole.Output.Should().Contain("Hero1");
    testConsole.Output.Should().Contain("Hero2");
}
```

---

## 📋 Implementation Checklist

### Phase 1: Foundation (1-2 hours)
- [ ] Install Spectre.Console.Testing package
- [ ] Create IConsoleUI interface
- [ ] Refactor ConsoleUI class to implement IConsoleUI
- [ ] Add IAnsiConsole constructor parameter
- [ ] Keep default parameterless constructor
- [ ] Update all static methods to instance methods

### Phase 2: Dependency Injection (1-2 hours)
- [ ] Register IConsoleUI in Program.cs DI container
- [ ] Update LoadGameService constructor
- [ ] Update ExplorationService constructor
- [ ] Update GameplayService constructor
- [ ] Update InventoryOrchestrator constructor
- [ ] Update CombatOrchestrator constructor
- [ ] Update CharacterCreationOrchestrator constructor
- [ ] Update all other services using ConsoleUI

### Phase 3: Test Infrastructure (30 mins)
- [ ] Create TestConsoleHelper utility class
- [ ] Add menu selection helper
- [ ] Add text input helper
- [ ] Add confirmation helper

### Phase 4: Update Tests (1-2 hours)
- [ ] Update LoadGameServiceTests (3 tests)
- [ ] Update ExplorationServiceTests (1 test)
- [ ] Update any other affected service tests
- [ ] Verify all 379 tests pass

### Phase 5: Documentation (30 mins)
- [ ] Update TEST_COVERAGE_REPORT.md
- [ ] Update GDD-Main.md (remove "skipped" note)
- [ ] Create UI_TESTING_GUIDE.md
- [ ] Update CONSOLEUI_GUIDE.md with DI examples

---

## 🎯 Benefits

### Before
- ❌ 4 tests skipped
- ❌ UI methods untestable
- ❌ Cannot verify console output
- ❌ Cannot mock user input
- ❌ Manual testing only for UI flows

### After
- ✅ All 379 tests passing
- ✅ UI methods fully testable
- ✅ Console output verified in tests
- ✅ User input mocked for automation
- ✅ Comprehensive test coverage
- ✅ Easier to find UI bugs
- ✅ Faster feedback during development

---

## 🚨 Breaking Changes & Migration

### Services Using ConsoleUI (Need Updates)

**Old Code**:
```csharp
ConsoleUI.ShowMenu("Choose action", "Fight", "Run");
```

**New Code**:
```csharp
_consoleUI.ShowMenu("Choose action", "Fight", "Run");
```

### DI Registration Pattern

All services will need constructor injection:

```csharp
public class MyService
{
    private readonly IConsoleUI _console;
    
    public MyService(IConsoleUI console)
    {
        _console = console;
    }
}
```

---

## 📊 Estimated Impact

| Area | Files Changed | Effort |
|------|---------------|--------|
| ConsoleUI refactor | 2 files | 1 hour |
| Service updates | ~15 files | 2 hours |
| Test updates | ~5 files | 1 hour |
| Documentation | 3 files | 30 mins |
| **Total** | **~25 files** | **4-5 hours** |

---

## 🧪 Testing Strategy

### Test Scenarios to Add

1. **Menu Navigation**:
   - Select first option
   - Select last option
   - Navigate with arrow keys

2. **Text Input**:
   - Valid input
   - Empty input
   - Special characters

3. **Confirmations**:
   - Accept (y)
   - Decline (n)

4. **Output Verification**:
   - Table rendering
   - Panel rendering
   - Colored text
   - Error messages

### Example Test Pattern

```csharp
[Theory]
[InlineData(0, "Fight")]
[InlineData(1, "Defend")]
[InlineData(2, "Use Item")]
[InlineData(3, "Flee")]
public void CombatService_Should_Handle_All_Menu_Options(int index, string expected)
{
    // Arrange
    var testConsole = TestConsoleHelper.CreateInteractiveConsole();
    var consoleUI = new ConsoleUI(testConsole);
    var service = new CombatService(consoleUI, ...);
    
    TestConsoleHelper.SelectMenuOption(testConsole, index);
    
    // Act
    var action = service.GetPlayerAction();
    
    // Assert
    action.Should().Be(expected);
    testConsole.Output.Should().Contain(expected);
}
```

---

## 📖 Related Resources

- **Spectre.Console.Testing NuGet**: https://www.nuget.org/packages/Spectre.Console.Testing/
- **Unit Testing Guide**: https://spectreconsole.net/unit-testing
- **CLI Unit Testing**: https://spectreconsole.net/cli/unit-testing
- **TestConsole API**: https://spectreconsole.net/api/Spectre.Console.Testing/TestConsole/

---

## 🎉 Success Criteria

1. ✅ All 379 tests passing (0 skipped)
2. ✅ UI methods covered by unit tests
3. ✅ No breaking changes to gameplay
4. ✅ Documentation updated
5. ✅ CI/CD pipeline green
6. ✅ Code review approved

---

**Next Steps**: 
1. Review this plan
2. Get approval for breaking changes
3. Create feature branch: `feature/ui-testing-support`
4. Implement Phase 1 (Foundation)
5. Test incrementally
6. Update documentation

**Estimated Completion**: 1-2 days (with testing)
