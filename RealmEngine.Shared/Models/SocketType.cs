namespace RealmEngine.Shared.Models;

/// <summary>
/// Types of sockets available for items.
/// Each type accepts specific socketable content (gems, essences, runes, etc.).
/// </summary>
public enum SocketType
{
    /// <summary>
    /// Physical gems providing stat bonuses (Ruby, Sapphire, etc.).
    /// </summary>
    Gem,
    
    /// <summary>
    /// Magical essences providing elemental effects (Fire Essence, Shadow Essence, etc.).
    /// </summary>
    Essence,
    
    /// <summary>
    /// Inscribed runes providing skill modifiers (Flame Rune, Protection Rune, etc.).
    /// </summary>
    Rune,
    
    /// <summary>
    /// Energy crystals providing resource effects (Mana Crystal, Life Crystal, etc.).
    /// </summary>
    Crystal,
    
    /// <summary>
    /// Skill orbs providing ability enhancements (Combat Orb, Magic Orb, etc.).
    /// </summary>
    Orb
}
