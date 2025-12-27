using MediatR;
using Game.Console.UI;
using Game.Core.Models;
using Serilog;

namespace Game.Console.Events;

/// <summary>
/// Handles the CharacterCreated event.
/// </summary>
public class CharacterCreatedHandler : INotificationHandler<CharacterCreated>
{
    private readonly IConsoleUI _console;

    public CharacterCreatedHandler(IConsoleUI console)
    {
        _console = console;
    }

    public Task Handle(CharacterCreated notification, CancellationToken cancellationToken)
    {
        _console.WriteColoredText($"[green]⚔️ {notification.PlayerName} enters the world![/]");
        Log.Information("New character created: {PlayerName}", notification.PlayerName);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the PlayerLeveledUp event.
/// </summary>
public class PlayerLeveledUpHandler : INotificationHandler<PlayerLeveledUp>
{
    private readonly IConsoleUI _console;

    public PlayerLeveledUpHandler(IConsoleUI console)
    {
        _console = console;
    }
    public Task Handle(PlayerLeveledUp notification, CancellationToken cancellationToken)
    {
        _console.ShowSuccess($"🎉 {notification.PlayerName} reached level {notification.NewLevel}!");
        _console.WriteColoredText($"[yellow]★[/] Congratulations! You are now more powerful!");
        Log.Information("{PlayerName} leveled up to {Level}", notification.PlayerName, notification.NewLevel);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the GoldGained event.
/// </summary>
public class GoldGainedHandler : INotificationHandler<GoldGained>
{
    private readonly IConsoleUI _console;

    public GoldGainedHandler(IConsoleUI console)
    {
        _console = console;
    }

    public Task Handle(GoldGained notification, CancellationToken cancellationToken)
    {
        _console.WriteColoredText($"[yellow]💰 +{notification.Amount} gold[/]");
        Log.Debug("{PlayerName} gained {Gold} gold", notification.PlayerName, notification.Amount);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the DamageTaken event.
/// </summary>
public class DamageTakenHandler : INotificationHandler<DamageTaken>
{
    private readonly IConsoleUI _console;

    public DamageTakenHandler(IConsoleUI console)
    {
        _console = console;
    }

    public Task Handle(DamageTaken notification, CancellationToken cancellationToken)
    {
        _console.WriteColoredText($"[red]❤️ -{notification.Amount} health[/]");
        Log.Debug("{PlayerName} took {Damage} damage", notification.PlayerName, notification.Amount);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the ItemAcquired event.
/// </summary>
public class ItemAcquiredHandler : INotificationHandler<ItemAcquired>
{
    private readonly IConsoleUI _console;

    public ItemAcquiredHandler(IConsoleUI console)
    {
        _console = console;
    }

    public Task Handle(ItemAcquired notification, CancellationToken cancellationToken)
    {
        _console.WriteColoredText($"[green]📦 Acquired: {notification.ItemName}[/]");
        Log.Information("{PlayerName} acquired item: {ItemName}", notification.PlayerName, notification.ItemName);
        return Task.CompletedTask;
    }
}
