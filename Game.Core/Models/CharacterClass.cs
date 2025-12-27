namespace Game.Core.Models;

/// <summary>
/// Represents a character class/archetype with starting bonuses and proficiencies.
/// </summary>
public class CharacterClass
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Primary attributes for this class (used for recommendations).
    /// </summary>
    public List<string> PrimaryAttributes { get; set; } = new();

    /// <summary>
    /// Starting attribute bonuses applied at character creation.
    /// </summary>
    public int BonusStrength { get; set; } = 0;
    public int BonusDexterity { get; set; } = 0;
    public int BonusConstitution { get; set; } = 0;
    public int BonusIntelligence { get; set; } = 0;
    public int BonusWisdom { get; set; } = 0;
    public int BonusCharisma { get; set; } = 0;

    /// <summary>
    /// Starting health bonus (added to base health calculation).
    /// </summary>
    public int StartingHealthBonus { get; set; } = 0;

    /// <summary>
    /// Starting mana bonus (added to base mana calculation).
    /// </summary>
    public int StartingManaBonus { get; set; } = 0;

    /// <summary>
    /// Starting equipment item names for this class.
    /// </summary>
    public List<string> StartingEquipment { get; set; } = new();

    /// <summary>
    /// Flavor text for class selection.
    /// </summary>
    public string FlavorText { get; set; } = string.Empty;
}

/// <summary>
/// Attribute point allocation for character creation.
/// </summary>
public class AttributeAllocation
{
    public int Strength { get; set; } = 8;
    public int Dexterity { get; set; } = 8;
    public int Constitution { get; set; } = 8;
    public int Intelligence { get; set; } = 8;
    public int Wisdom { get; set; } = 8;
    public int Charisma { get; set; } = 8;

    private const int MinAttributeValue = 8;
    private const int MaxAttributeValue = 15;
    private const int TotalPoints = 27;

    /// <summary>
    /// Calculate the point cost to increase an attribute from current to target value.
    /// Point buy rules: 8-13 costs 1 point per level, 14 costs 2 points, 15 costs 2 points.
    /// </summary>
    public static int GetPointCost(int fromValue, int toValue)
    {
        if (toValue <= fromValue) return 0;

        int cost = 0;
        for (int i = fromValue; i < toValue; i++)
        {
            if (i >= 13)
                cost += 2; // 13→14 and 14→15 cost 2 points each
            else
                cost += 1; // 8→9 through 12→13 cost 1 point each
        }
        return cost;
    }

    /// <summary>
    /// Get the total points spent on all attributes.
    /// </summary>
    public int GetPointsSpent()
    {
        int total = 0;
        total += GetPointCost(MinAttributeValue, Strength);
        total += GetPointCost(MinAttributeValue, Dexterity);
        total += GetPointCost(MinAttributeValue, Constitution);
        total += GetPointCost(MinAttributeValue, Intelligence);
        total += GetPointCost(MinAttributeValue, Wisdom);
        total += GetPointCost(MinAttributeValue, Charisma);
        return total;
    }

    /// <summary>
    /// Get remaining points to allocate.
    /// </summary>
    public int GetRemainingPoints()
    {
        return TotalPoints - GetPointsSpent();
    }

    /// <summary>
    /// Check if the allocation is valid (within limits and points budget).
    /// </summary>
    public bool IsValid()
    {
        return Strength >= MinAttributeValue && Strength <= MaxAttributeValue &&
               Dexterity >= MinAttributeValue && Dexterity <= MaxAttributeValue &&
               Constitution >= MinAttributeValue && Constitution <= MaxAttributeValue &&
               Intelligence >= MinAttributeValue && Intelligence <= MaxAttributeValue &&
               Wisdom >= MinAttributeValue && Wisdom <= MaxAttributeValue &&
               Charisma >= MinAttributeValue && Charisma <= MaxAttributeValue &&
               GetPointsSpent() <= TotalPoints;
    }

    /// <summary>
    /// Check if we can increase an attribute value.
    /// </summary>
    public bool CanIncrease(string attribute)
    {
        var currentValue = GetAttributeValue(attribute);
        if (currentValue >= MaxAttributeValue) return false;

        var costToIncrease = GetPointCost(currentValue, currentValue + 1);
        return GetRemainingPoints() >= costToIncrease;
    }

    /// <summary>
    /// Check if we can decrease an attribute value.
    /// </summary>
    public bool CanDecrease(string attribute)
    {
        return GetAttributeValue(attribute) > MinAttributeValue;
    }

    /// <summary>
    /// Get attribute value by name.
    /// </summary>
    public int GetAttributeValue(string attribute)
    {
        return attribute switch
        {
            "Strength" => Strength,
            "Dexterity" => Dexterity,
            "Constitution" => Constitution,
            "Intelligence" => Intelligence,
            "Wisdom" => Wisdom,
            "Charisma" => Charisma,
            _ => 0
        };
    }

    /// <summary>
    /// Set attribute value by name.
    /// </summary>
    public void SetAttributeValue(string attribute, int value)
    {
        switch (attribute)
        {
            case "Strength": Strength = value; break;
            case "Dexterity": Dexterity = value; break;
            case "Constitution": Constitution = value; break;
            case "Intelligence": Intelligence = value; break;
            case "Wisdom": Wisdom = value; break;
            case "Charisma": Charisma = value; break;
        }
    }
}
