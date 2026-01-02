namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a character class/archetype with starting bonuses and proficiencies.
/// Maps to v4.1 JSON classes/catalog.json structure.
/// </summary>
public class CharacterClass
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name shown to players (may differ from internal name).
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Rarity weight for procedural generation (lower = more common).
    /// </summary>
    public int RarityWeight { get; set; } = 1;

    /// <summary>
    /// Whether this is a subclass/specialization of another class.
    /// </summary>
    public bool IsSubclass { get; set; } = false;

    /// <summary>
    /// Parent class ID if this is a subclass (resolved from @classes reference).
    /// </summary>
    public string? ParentClassId { get; set; }

    /// <summary>
    /// Primary attributes for this class (used for recommendations).
    /// </summary>
    public List<string> PrimaryAttributes { get; set; } = new();

    /// <summary>
    /// Starting attribute bonuses applied at character creation.
    /// Maps to startingStats in JSON.
    /// </summary>
    public int StartingHealth { get; set; } = 100;
    public int StartingMana { get; set; } = 50;
    public int BonusStrength { get; set; } = 10;
    public int BonusDexterity { get; set; } = 10;
    public int BonusConstitution { get; set; } = 10;
    public int BonusIntelligence { get; set; } = 10;
    public int BonusWisdom { get; set; } = 10;
    public int BonusCharisma { get; set; } = 10;

    /// <summary>
    /// Collection of starting ability IDs that characters of this class begin with.
    /// These are resolved from @abilities JSON references during character creation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Resolve IDs to full Ability objects
    /// var abilities = await abilityRepository.GetByIdsAsync(characterClass.StartingAbilityIds);
    /// character.LearnedSkills.AddRange(abilities);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Load ability data from IDs
    /// var abilities = []
    /// for ability_id in character_class.StartingAbilityIds:
    ///     var ability = await ability_service.get_by_id(ability_id)
    ///     abilities.append(ability)
    /// character.learned_skills.append_array(abilities)
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Memory efficiency - templates don't duplicate full objects</description></item>
    /// <item><description>Lazy loading - resolve only when creating a character</description></item>
    /// <item><description>Cross-domain references - abilities live in separate catalog</description></item>
    /// <item><description>Save file optimization - serialize IDs instead of full object graphs</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["basic-attack", "power-strike", "defensive-stance"]
    /// </example>
    public List<string> StartingAbilityIds { get; set; } = new();

    /// <summary>
    /// Collection of starting equipment item IDs that characters of this class begin with.
    /// These are resolved from @items JSON references during character creation.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Resolve IDs to full Item objects
    /// var equipment = await itemRepository.GetByIdsAsync(characterClass.StartingEquipmentIds);
    /// character.Inventory.AddRange(equipment);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Load equipment data from IDs
    /// for item_id in character_class.StartingEquipmentIds:
    ///     var item = await item_service.get_by_id(item_id)
    ///     character.inventory.add_item(item)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["iron-sword", "wooden-shield", "leather-armor"]
    /// </example>
    public List<string> StartingEquipmentIds { get; set; } = new();

    /// <summary>
    /// Traits/properties specific to this class (bonus damage types, resistances, etc.).
    /// Values may be strings, numbers, or resolved references.
    /// </summary>
    public Dictionary<string, object> Traits { get; set; } = new();

    /// <summary>
    /// Progression data (stat growth, ability unlocks, etc.).
    /// </summary>
    public ClassProgression? Progression { get; set; }

    /// <summary>
    /// Flavor text for class selection.
    /// </summary>
    public string FlavorText { get; set; } = string.Empty;
}

/// <summary>
/// Class progression data (stat growth, ability unlocks).
/// </summary>
public class ClassProgression
{
    /// <summary>
    /// Stat growth per level.
    /// </summary>
    public Dictionary<string, double> StatGrowth { get; set; } = new();

    /// <summary>
    /// Ability unlock schedule mapping level to ability IDs.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Get abilities unlocked at level 5
    /// if (progression.AbilityUnlocks.TryGetValue(5, out var abilityIds))
    /// {
    ///     var unlockedAbilities = await abilityRepository.GetByIdsAsync(abilityIds);
    ///     character.LearnedSkills.AddRange(unlockedAbilities);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Check for abilities at current level
    /// if progression.AbilityUnlocks.has(character.level):
    ///     var ability_ids = progression.AbilityUnlocks[character.level]
    ///     for ability_id in ability_ids:
    ///         var ability = await ability_service.get_by_id(ability_id)
    ///         character.learned_skills.append(ability)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example: { 5: ["power-strike", "cleave"], 10: ["whirlwind", "battle-cry"] }
    /// </example>
    public Dictionary<int, List<string>> AbilityUnlocks { get; set; } = new();
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
