namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents the current state of the game.
/// </summary>
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
