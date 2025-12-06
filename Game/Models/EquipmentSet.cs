namespace Game.Models;

/// <summary>
/// Represents a set of equipment that provides bonuses when multiple pieces are worn.
/// </summary>
public class EquipmentSet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Item names that belong to this set.
    /// </summary>
    public List<string> SetItemNames { get; set; } = new();
    
    /// <summary>
    /// Set bonuses indexed by number of pieces equipped.
    /// Example: { 2 => "+10 Defense", 4 => "+20% Fire Resistance", 6 => "+50 Strength" }
    /// </summary>
    public Dictionary<int, SetBonus> Bonuses { get; set; } = new();
}

/// <summary>
/// Represents a bonus granted when wearing a certain number of set pieces.
/// </summary>
public class SetBonus
{
    public int PiecesRequired { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // D20 Attribute Bonuses
    public int BonusStrength { get; set; } = 0;
    public int BonusDexterity { get; set; } = 0;
    public int BonusConstitution { get; set; } = 0;
    public int BonusIntelligence { get; set; } = 0;
    public int BonusWisdom { get; set; } = 0;
    public int BonusCharisma { get; set; } = 0;
    
    // Special effects (for future implementation)
    public string? SpecialEffect { get; set; }
}
