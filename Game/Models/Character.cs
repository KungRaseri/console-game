namespace Game.Models;

/// <summary>
/// Represents a player character in the game.
/// </summary>
public class Character
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 50;
    public int MaxMana { get; set; } = 50;
    public int Gold { get; set; } = 0;

    /// <summary>
    /// Award experience points to the character.
    /// </summary>
    public void GainExperience(int amount)
    {
        Experience += amount;
        
        // Simple leveling logic - can be enhanced
        while (Experience >= ExperienceForNextLevel())
        {
            Experience -= ExperienceForNextLevel();
            LevelUp();
        }
    }

    /// <summary>
    /// Calculate experience needed for next level.
    /// </summary>
    private int ExperienceForNextLevel()
    {
        return Level * 100; // Simple formula: Level 1 needs 100, Level 2 needs 200, etc.
    }

    /// <summary>
    /// Increase character level and stats.
    /// </summary>
    private void LevelUp()
    {
        Level++;
        MaxHealth += 10;
        Health = MaxHealth;
        MaxMana += 5;
        Mana = MaxMana;
    }
}
