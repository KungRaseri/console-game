namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents the current state of the game.
/// </summary>
public enum GameState
{
    /// <summary>Main menu state.</summary>
    MainMenu,
    /// <summary>Character creation state.</summary>
    CharacterCreation,
    /// <summary>In-game state.</summary>
    InGame,
    /// <summary>Combat state.</summary>
    Combat,
    /// <summary>Inventory state.</summary>
    Inventory,
    /// <summary>Paused state.</summary>
    Paused,
    /// <summary>Game over state.</summary>
    GameOver
}
