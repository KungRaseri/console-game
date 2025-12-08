using MediatR;
using Game.Shared.UI;
using Game.Models;
using Serilog;

namespace Game.Shared.Events;

/// <summary>
/// Handles the CharacterCreated event.
/// </summary>
public class CharacterCreatedHandler : INotificationHandler<CharacterCreated>
{
    public Task Handle(CharacterCreated notification, CancellationToken cancellationToken)
    {
        ConsoleUI.WriteColoredText($"[green]⚔️ {notification.PlayerName} enters the world![/]");
        Log.Information("New character created: {PlayerName}", notification.PlayerName);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the PlayerLeveledUp event.
/// </summary>
public class PlayerLeveledUpHandler : INotificationHandler<PlayerLeveledUp>
{
    public Task Handle(PlayerLeveledUp notification, CancellationToken cancellationToken)
    {
        ConsoleUI.ShowSuccess($"🎉 {notification.PlayerName} reached level {notification.NewLevel}!");
        ConsoleUI.WriteColoredText($"[yellow]★[/] Congratulations! You are now more powerful!");
        Log.Information("{PlayerName} leveled up to {Level}", notification.PlayerName, notification.NewLevel);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the GoldGained event.
/// </summary>
public class GoldGainedHandler : INotificationHandler<GoldGained>
{
    public Task Handle(GoldGained notification, CancellationToken cancellationToken)
    {
        ConsoleUI.WriteColoredText($"[yellow]💰 +{notification.Amount} gold[/]");
        Log.Debug("{PlayerName} gained {Gold} gold", notification.PlayerName, notification.Amount);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the DamageTaken event.
/// </summary>
public class DamageTakenHandler : INotificationHandler<DamageTaken>
{
    public Task Handle(DamageTaken notification, CancellationToken cancellationToken)
    {
        ConsoleUI.WriteColoredText($"[red]❤️ -{notification.Amount} health[/]");
        Log.Debug("{PlayerName} took {Damage} damage", notification.PlayerName, notification.Amount);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles the ItemAcquired event.
/// </summary>
public class ItemAcquiredHandler : INotificationHandler<ItemAcquired>
{
    public Task Handle(ItemAcquired notification, CancellationToken cancellationToken)
    {
        ConsoleUI.WriteColoredText($"[green]📦 Acquired: {notification.ItemName}[/]");
        Log.Information("{PlayerName} acquired item: {ItemName}", notification.PlayerName, notification.ItemName);
        return Task.CompletedTask;
    }
}
