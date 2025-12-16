# Console UI Logging Enhancement

## Changes Made

### 1. Added Logging to ConsoleUI Class

**File**: `Game.Console/Shared/UI/ConsoleUI.cs`

Added `ILogger<ConsoleUI>` dependency injection to capture all user interactions:

```csharp
private readonly ILogger<ConsoleUI>? _logger;

public ConsoleUI(IAnsiConsole console, ILogger<ConsoleUI>? logger = null)
{
    _console = console ?? throw new ArgumentNullException(nameof(console));
    _logger = logger;
}
```

### 2. Logging Added to Key UI Methods

#### ShowMenu - Menu Selection
```csharp
public string ShowMenu(string title, params string[] choices)
{
    _logger?.LogDebug("ShowMenu: {Title} with {ChoiceCount} choices", title, choices.Length);
    
    var selection = _console.Prompt(/* ... */);
    
    _logger?.LogInformation("User selected: {Selection} from menu: {Title}", selection, title);
    return selection;
}
```

**Log Output Example**:
```
[10:45:00 DBG] ShowMenu: Main Menu with 5 choices
[10:45:03 INF] User selected: New Game from menu: Main Menu
```

#### AskForInput - Text Input
```csharp
public string AskForInput(string prompt)
{
    _logger?.LogDebug("AskForInput: {Prompt}", prompt);
    var input = _console.Ask<string>(/* ... */);
    _logger?.LogInformation("User input received for '{Prompt}': {InputLength} characters", prompt, input.Length);
    return input;
}
```

**Log Output Example**:
```
[10:45:10 DBG] AskForInput: Enter your character name
[10:45:15 INF] User input received for 'Enter your character name': 8 characters
```

#### AskForNumber - Numeric Input
```csharp
public int AskForNumber(string prompt, int min = int.MinValue, int max = int.MaxValue)
{
    _logger?.LogDebug("AskForNumber: {Prompt} (min: {Min}, max: {Max})", prompt, min, max);
    
    var number = _console.Prompt(/* ... */);
    
    _logger?.LogInformation("User entered number: {Number} for prompt: {Prompt}", number, prompt);
    return number;
}
```

**Log Output Example**:
```
[10:45:20 DBG] AskForNumber: How many points to allocate? (min: 0, max: 10)
[10:45:25 INF] User entered number: 5 for prompt: How many points to allocate?
```

#### Confirm - Yes/No Questions
```csharp
public bool Confirm(string question)
{
    _logger?.LogDebug("Confirm: {Question}", question);
    
    var result = _console.Confirm(/* ... */);
    
    _logger?.LogInformation("User confirmed: {Result} for question: {Question}", result, question);
    return result;
}
```

**Log Output Example**:
```
[10:45:30 DBG] Confirm: Do you want to save your game?
[10:45:32 INF] User confirmed: True for question: Do you want to save your game?
```

### 3. Updated DI Registration

**File**: `Game.Console/Program.cs`

Updated ConsoleUI registration to inject logger:

```csharp
services.AddSingleton<ConsoleUI>(sp => 
    new ConsoleUI(sp.GetRequiredService<IAnsiConsole>(), sp.GetRequiredService<ILogger<ConsoleUI>>()));
services.AddSingleton<IGameUI>(sp => sp.GetRequiredService<ConsoleUI>());
services.AddSingleton<IConsoleUI>(sp => sp.GetRequiredService<ConsoleUI>());
```

## What Gets Logged Now

### Before Enhancement
Only business logic events were logged:
- Game start/stop
- Character creation
- Combat events
- Save/load operations

### After Enhancement
**All user interactions** are now logged:
- ✅ Menu selections (with menu title and selected option)
- ✅ Text input (with prompt and input length - not the actual text for privacy)
- ✅ Numeric input (with prompt, constraints, and entered value)
- ✅ Confirmation dialogs (with question and yes/no response)

## Benefits

### 1. **Debugging & Support**
- See exactly what menu options users selected
- Track user flow through the game
- Identify where users get stuck or confused

### 2. **Analytics**
- Understand which features are used most
- Track user behavior patterns
- Measure engagement with different menu options

### 3. **Audit Trail**
- Complete record of all user actions
- Useful for bug reproduction
- Can correlate UI actions with game state changes

### 4. **Privacy-Conscious**
- Text input logs show length, not content (to avoid logging sensitive data)
- All logging is optional (uses null-conditional operator `?. `)
- Can be disabled by not providing a logger

## Example Log Session

```
[10:00:00 INF] Game logging initialized to C:\code\console-game\Game.Console\bin\Debug\net9.0\logs
[10:00:00 INF] Game starting - Version 1.0
[10:00:01 DBG] ShowMenu: Main Menu with 5 choices
[10:00:03 INF] User selected: New Game from menu: Main Menu
[10:00:03 DBG] ShowMenu: Choose your difficulty with 4 choices
[10:00:05 INF] User selected: Normal from menu: Choose your difficulty
[10:00:05 DBG] Confirm: Enable Ironman mode?
[10:00:06 INF] User confirmed: False for question: Enable Ironman mode?
[10:00:06 DBG] AskForInput: Enter your character name
[10:00:10 INF] User input received for 'Enter your character name': 8 characters
[10:00:10 INF] New character created: TheBrave
[10:00:10 DBG] ShowMenu: Choose your class with 4 choices
[10:00:12 INF] User selected: Warrior from menu: Choose your class
[10:00:12 INF] Character created: TheBrave (Warrior)
[10:00:13 DBG] ShowMenu: What would you like to do? with 3 choices
[10:00:15 INF] User selected: Explore from menu: What would you like to do?
```

## Testing

To test the new logging:

1. **Run the game**:
   ```powershell
   dotnet run --project Game.Console/Game.Console.csproj
   ```

2. **Interact with menus, inputs, and confirmations**

3. **Check the log file**:
   ```powershell
   Get-Content Game.Console/bin/Debug/net9.0/logs/game-20251216.txt -Tail 50
   ```

4. **Filter for UI interactions**:
   ```powershell
   Get-Content Game.Console/bin/Debug/net9.0/logs/game-20251216.txt | Select-String "ShowMenu|AskFor|Confirm|User selected|User input|User entered|User confirmed"
   ```

## Log File Location

All logs (including UI interactions) are now stored in:
```
Game.Console/bin/Debug/net9.0/logs/game-{date}.txt
```

This ensures logs and databases stay in the bin folder, not the solution root.

## Future Enhancements

Potential additions:
- Log ShowTable calls (what data is being displayed)
- Log ShowPanel calls (what information panels are shown)
- Log navigation patterns (menu->submenu tracking)
- Add correlation IDs to track a single user session
- Performance metrics (time spent on each menu)
