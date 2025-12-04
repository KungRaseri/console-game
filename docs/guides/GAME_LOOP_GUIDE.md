# Game Loop Architecture

This document explains how to create and use game loops in your console game using the libraries you have set up.

## Overview

A **game loop** is the core heartbeat of your game. It continuously processes player input, updates game state, and renders output until the game exits.

## Architecture Patterns

### 1. Simple While Loop (Current Program.cs)

The simplest approach - a `while` loop that runs until a flag is set:

```csharp
var continueGame = true;

while (continueGame)
{
    // Show menu
    var action = ConsoleUI.ShowMenu("What do you want to do?", ...);
    
    // Process action
    switch (action)
    {
        case "Exit":
            continueGame = false;
            break;
        // ... other cases
    }
}
```

**Pros:**
- Simple and straightforward
- Easy to understand
- Good for menu-driven games

**Cons:**
- Harder to manage complex state
- Difficult to add async operations
- Limited error handling

---

### 2. State Machine Game Engine (GameEngine.cs)

A more robust approach using state management and event-driven architecture:

```csharp
public enum GameState
{
    MainMenu,
    CharacterCreation,
    InGame,
    Combat,
    Inventory,
    Paused,
    GameOver
}

public class GameEngine
{
    private GameState _state;
    private bool _isRunning;
    
    public async Task RunAsync()
    {
        _isRunning = true;
        
        while (_isRunning)
        {
            await ProcessGameTickAsync();
        }
    }
    
    private async Task ProcessGameTickAsync()
    {
        switch (_state)
        {
            case GameState.MainMenu:
                await HandleMainMenuAsync();
                break;
            case GameState.InGame:
                await HandleInGameAsync();
                break;
            // ... other states
        }
    }
}
```

**Pros:**
- Clear state management
- Easier to add new states
- Supports async operations
- Better error handling
- Event-driven with MediatR
- Resilience with Polly

**Cons:**
- More complex initially
- Requires dependency injection setup

---

## Using the GameEngine

### Setup with Dependency Injection

The `GameEngine` uses MediatR for events, so you need to set up DI:

```csharp
// In Program.cs
var services = new ServiceCollection();

// Register MediatR
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// Register GameEngine
services.AddSingleton<GameEngine>();

var serviceProvider = services.BuildServiceProvider();

// Run the game
var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
await gameEngine.RunAsync();
```

### Game States

The engine manages different states:

1. **MainMenu** - New game, load game, settings, exit
2. **CharacterCreation** - Create your player character
3. **InGame** - Main gameplay loop (explore, inventory, etc.)
4. **Combat** - Turn-based combat (TODO)
5. **Inventory** - Manage items (TODO)
6. **Paused** - Pause menu
7. **GameOver** - Player died

### Event-Driven Updates

The engine publishes events using MediatR:

```csharp
// Character created
await _mediator.Publish(new CharacterCreated(playerName));

// Level up
await _mediator.Publish(new PlayerLeveledUp(playerName, newLevel));

// Gold gained
await _mediator.Publish(new GoldGained(playerName, amount));
```

Event handlers can respond to these:

```csharp
public class PlayerLeveledUpHandler : INotificationHandler<PlayerLeveledUp>
{
    public Task Handle(PlayerLeveledUp notification, CancellationToken ct)
    {
        ConsoleUI.ShowSuccess($"🎉 Level up! You are now level {notification.NewLevel}!");
        Log.Information("{Player} leveled up to {Level}", 
            notification.PlayerName, notification.NewLevel);
        return Task.CompletedTask;
    }
}
```

### Error Handling with Polly

The engine uses Polly for resilience:

```csharp
_resiliencePipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(100),
        BackoffType = DelayBackoffType.Exponential
    })
    .Build();

// Execute with automatic retry on failure
await _resiliencePipeline.ExecuteAsync(async ct =>
{
    await ProcessGameTickAsync();
});
```

---

## Comparison: Simple vs Engine

### When to Use Simple Loop

✅ Small projects
✅ Menu-driven games
✅ Prototyping
✅ Learning

### When to Use GameEngine

✅ Complex state management
✅ Multiple game screens/modes
✅ Need async operations
✅ Event-driven architecture
✅ Production-ready code
✅ Team collaboration

---

## Advanced Patterns

### Turn-Based Combat Loop

```csharp
private async Task HandleCombatAsync()
{
    var enemy = GenerateEnemy();
    var combatActive = true;
    
    while (combatActive && _player.Health > 0 && enemy.Health > 0)
    {
        // Player turn
        var action = ConsoleUI.ShowMenu(
            $"HP: {_player.Health}/{_player.MaxHealth} vs {enemy.Name}",
            "Attack",
            "Defend",
            "Use Item",
            "Flee"
        );
        
        switch (action)
        {
            case "Attack":
                var damage = CalculateDamage(_player, enemy);
                enemy.Health -= damage;
                await _mediator.Publish(new DamageDealt(_player.Name, enemy.Name, damage));
                break;
            // ... other actions
        }
        
        // Enemy turn
        if (enemy.Health > 0)
        {
            var enemyDamage = CalculateDamage(enemy, _player);
            _player.Health -= enemyDamage;
            await _mediator.Publish(new DamageTaken(_player.Name, enemyDamage));
        }
        
        // Check win/lose
        if (enemy.Health <= 0)
        {
            await _mediator.Publish(new EnemyDefeated(enemy.Name));
            combatActive = false;
        }
        else if (_player.Health <= 0)
        {
            _state = GameState.GameOver;
            combatActive = false;
        }
    }
}
```

### Real-Time Loop (with Delta Time)

For action games that need frame timing:

```csharp
private async Task RunRealtimeLoopAsync()
{
    var lastUpdate = DateTime.Now;
    
    while (_isRunning)
    {
        var now = DateTime.Now;
        var deltaTime = (now - lastUpdate).TotalSeconds;
        lastUpdate = now;
        
        // Update game state with delta time
        await UpdateAsync(deltaTime);
        
        // Render
        RenderFrame();
        
        // Cap to ~60 FPS
        var frameTime = (DateTime.Now - now).TotalMilliseconds;
        var targetFrameTime = 1000.0 / 60.0;
        if (frameTime < targetFrameTime)
        {
            await Task.Delay((int)(targetFrameTime - frameTime));
        }
    }
}
```

### Progress Bar Loop

For long-running operations:

```csharp
private async Task ProcessLongOperationAsync()
{
    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var task = ctx.AddTask("[green]Processing...[/]");
            
            while (!task.IsFinished)
            {
                // Do work
                await DoWorkAsync();
                
                // Update progress
                task.Increment(1.0);
            }
        });
}
```

---

## Best Practices

### 1. **Separate Concerns**
- Keep game logic separate from UI code
- Use services for reusable functionality
- Use events for decoupled communication

### 2. **Handle Errors Gracefully**
```csharp
try
{
    await ProcessGameTickAsync();
}
catch (Exception ex)
{
    Log.Error(ex, "Error in game loop");
    ConsoleUI.ShowError($"An error occurred: {ex.Message}");
    
    // Ask if player wants to continue
    if (!ConsoleUI.Confirm("Continue playing?"))
    {
        _isRunning = false;
    }
}
```

### 3. **Use Async/Await Properly**
```csharp
// ✅ Good - awaits async operations
await SaveGameAsync();

// ❌ Bad - blocks the thread
SaveGameAsync().Wait();
```

### 4. **Log Important Events**
```csharp
Log.Information("Game loop started");
Log.Debug("Processing state: {State}", _state);
Log.Error(ex, "Failed to process game tick");
```

### 5. **Validate State Transitions**
```csharp
private void TransitionTo(GameState newState)
{
    var validTransitions = GetValidTransitions(_state);
    
    if (!validTransitions.Contains(newState))
    {
        Log.Warning("Invalid state transition: {From} -> {To}", _state, newState);
        return;
    }
    
    Log.Information("State transition: {From} -> {To}", _state, newState);
    _state = newState;
}
```

---

## Testing Game Loops

### Unit Testing State Transitions

```csharp
[Fact]
public async Task Should_Transition_From_MainMenu_To_CharacterCreation()
{
    // Arrange
    var mediator = Mock.Of<IMediator>();
    var engine = new GameEngine(mediator);
    
    // Act
    // Simulate selecting "New Game"
    
    // Assert
    engine.CurrentState.Should().Be(GameState.CharacterCreation);
}
```

### Integration Testing

```csharp
[Fact]
public async Task Full_Game_Loop_Should_Complete_Without_Errors()
{
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var engine = serviceProvider.GetRequiredService<GameEngine>();
    
    // Act & Assert
    await engine.RunAsync();
    
    // Verify no errors logged
}
```

---

## Examples in This Project

### 1. **Program.cs** - Simple loop approach
- Traditional while loop
- Menu-driven
- Good for learning

### 2. **ProgramWithEngine.cs** - Engine approach
- Uses dependency injection
- State machine pattern
- Event-driven
- Production-ready

### 3. **GameEngine.cs** - Full implementation
- State management
- Error handling
- Event publishing
- Async operations

---

## Next Steps

1. **Choose Your Approach**
   - Start with simple loop for prototyping
   - Migrate to GameEngine for production

2. **Implement Missing Features**
   - Combat system
   - Inventory management
   - Save/Load with LiteDB

3. **Add More States**
   - Shop/Trading
   - Quests
   - Map/World

4. **Enhance Events**
   - More event types
   - Event handlers for UI updates
   - Event logging

5. **Test Thoroughly**
   - Unit tests for state logic
   - Integration tests for full loops
   - Error scenario testing

---

## Resources

- **MediatR Docs**: https://github.com/jbogard/MediatR
- **Polly Docs**: https://www.pollydocs.org/
- **Game Loop Patterns**: https://gameprogrammingpatterns.com/game-loop.html
- **State Machine Pattern**: https://refactoring.guru/design-patterns/state
