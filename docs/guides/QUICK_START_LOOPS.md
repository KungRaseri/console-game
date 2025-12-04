# Quick Start: Game Loops

This guide shows you two ways to create game loops in your console game.

## Option 1: Simple Loop (Recommended for Beginners)

Your current `Program.cs` already uses this approach! Here's a simplified version:

```csharp
using Game.UI;

// Setup
var continueGame = true;
var playerHealth = 100;

// Main game loop
while (continueGame)
{
    var action = ConsoleUI.ShowMenu(
        $"HP: {playerHealth}",
        "Fight",
        "Heal", 
        "Exit"
    );

    switch (action)
    {
        case "Fight":
            playerHealth -= 10;
            ConsoleUI.ShowWarning("You took 10 damage!");
            break;

        case "Heal":
            playerHealth = 100;
            ConsoleUI.ShowSuccess("Fully healed!");
            break;

        case "Exit":
            continueGame = false;
            break;
    }

    // Check game over
    if (playerHealth <= 0)
    {
        ConsoleUI.ShowError("Game Over!");
        continueGame = false;
    }
}
```

**When to use:** Simple games, prototypes, learning

---

## Option 2: GameEngine (Recommended for Production)

For more complex games, use the `GameEngine` class:

```csharp
using Game.Services;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
services.AddSingleton<GameEngine>();

var serviceProvider = services.BuildServiceProvider();

// Run the game
var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
await gameEngine.RunAsync();
```

The `GameEngine` handles:
- ✅ State management (MainMenu, InGame, Combat, etc.)
- ✅ Event system (level ups, gold gained, etc.)
- ✅ Error handling and retry logic
- ✅ Async operations

**When to use:** Complex games, team projects, production code

---

## To Use the GameEngine

### Step 1: Rename files

```powershell
# Backup current Program.cs
Move-Item Game\Program.cs Game\ProgramSimple.cs

# Use the engine version
Move-Item Game\ProgramWithEngine.cs Game\Program.cs
```

### Step 2: Run the game

```powershell
dotnet run --project Game
```

You'll see a state-driven game with:
- Main menu → New Game → Character Creation → In-Game → etc.
- Event logging (Serilog)
- Automatic error recovery (Polly)

---

## Switching Between Approaches

You can keep both files and choose which to use:

### Run Simple Version
```powershell
# Delete current Program.cs temporarily
Remove-Item Game\Program.cs

# Rename simple version
Move-Item Game\ProgramSimple.cs Game\Program.cs

# Run
dotnet run --project Game
```

### Run Engine Version
```powershell
# Same process but with ProgramWithEngine.cs
```

---

## Adding New Features

### Simple Loop Example: Add Combat

```csharp
case "Fight":
    var enemyHealth = 50;
    
    while (enemyHealth > 0 && playerHealth > 0)
    {
        var combatAction = ConsoleUI.ShowMenu(
            $"You: {playerHealth} | Enemy: {enemyHealth}",
            "Attack",
            "Defend"
        );
        
        if (combatAction == "Attack")
        {
            var damage = Random.Shared.Next(10, 20);
            enemyHealth -= damage;
            ConsoleUI.WriteText($"You deal {damage} damage!");
            
            // Enemy attacks back
            if (enemyHealth > 0)
            {
                var enemyDamage = Random.Shared.Next(5, 15);
                playerHealth -= enemyDamage;
                ConsoleUI.ShowWarning($"Enemy deals {enemyDamage} damage!");
            }
        }
    }
    
    if (enemyHealth <= 0)
    {
        ConsoleUI.ShowSuccess("Victory!");
    }
    break;
```

### GameEngine Example: Add Combat State

```csharp
// In GameEngine.cs, add to ProcessGameTickAsync:
case GameState.Combat:
    await HandleCombatAsync();
    break;

// Then implement:
private async Task HandleCombatAsync()
{
    if (_player == null) return;
    
    var enemy = new Character 
    { 
        Name = "Goblin", 
        Health = 50, 
        MaxHealth = 50 
    };
    
    while (enemy.Health > 0 && _player.Health > 0)
    {
        var action = ConsoleUI.ShowMenu(
            $"You: {_player.Health} | {enemy.Name}: {enemy.Health}",
            "Attack",
            "Flee"
        );
        
        if (action == "Attack")
        {
            var damage = Random.Shared.Next(10, 20);
            enemy.Health -= damage;
            await _mediator.Publish(new DamageDealt(_player.Name, enemy.Name, damage));
            
            if (enemy.Health > 0)
            {
                var enemyDamage = Random.Shared.Next(5, 15);
                _player.Health -= enemyDamage;
                await _mediator.Publish(new DamageTaken(_player.Name, enemyDamage));
            }
        }
        else if (action == "Flee")
        {
            ConsoleUI.ShowInfo("You fled!");
            break;
        }
    }
    
    if (enemy.Health <= 0)
    {
        ConsoleUI.ShowSuccess("Victory!");
        var xp = 50;
        _player.GainExperience(xp);
    }
    else if (_player.Health <= 0)
    {
        _state = GameState.GameOver;
    }
    
    _state = GameState.InGame;
}
```

---

## Best Practices

### 1. Always validate input
```csharp
var age = ConsoleUI.AskForNumber("Enter age:", 1, 100);
// Already validated!
```

### 2. Use events for important actions
```csharp
// Simple: Just do it
playerHealth = 100;

// Engine: Publish event
await _mediator.Publish(new PlayerHealed(_player.Name, amountHealed));
```

### 3. Handle errors
```csharp
try
{
    // Your game logic
}
catch (Exception ex)
{
    Log.Error(ex, "Game error");
    ConsoleUI.ShowError("Something went wrong!");
}
```

### 4. Save often
```csharp
case "Save":
    using (var repo = new SaveGameRepository())
    {
        repo.Save(new SaveGame { PlayerName = player.Name, ... });
    }
    ConsoleUI.ShowSuccess("Game saved!");
    break;
```

---

## Next Steps

1. **Try both approaches** - Run each version to see the difference
2. **Pick your style** - Simple for learning, Engine for production
3. **Add features** - Combat, inventory, quests
4. **Test thoroughly** - Use the test project (see `dotnet test`)
5. **Read more** - Check `GAME_LOOP_GUIDE.md` for advanced patterns

---

## Need Help?

- See full examples in `Program.cs` and `GameEngine.cs`
- Read `GAME_LOOP_GUIDE.md` for detailed explanations
- Check test files in `Game.Tests/` for examples
- Look at library usage in `.github/copilot-instructions.md`
