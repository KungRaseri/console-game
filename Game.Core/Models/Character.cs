using Game.Core.Utilities;

namespace Game.Core.Models;

/// <summary>
/// Represents a player character in the game.
/// </summary>
public class Character
{
    public string Name { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;  // Character's class (Warrior, Mage, etc.)
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 50;
    public int MaxMana { get; set; } = 50;
    public int Gold { get; set; } = 0;

    // D20 Attributes (core stats that affect everything)
    public int Strength { get; set; } = 10;      // Melee damage, carry weight
    public int Dexterity { get; set; } = 10;     // Dodge, accuracy, crit chance
    public int Constitution { get; set; } = 10;  // Max HP, physical defense
    public int Intelligence { get; set; } = 10;  // Magic damage, crafting
    public int Wisdom { get; set; } = 10;        // Max Mana, magic defense
    public int Charisma { get; set; } = 10;      // Shop prices, rare loot

    // Level-up tracking
    public int UnspentAttributePoints { get; set; } = 0;
    public int UnspentSkillPoints { get; set; } = 0;
    public List<LevelUpInfo> PendingLevelUps { get; set; } = new();
    public List<Skill> LearnedSkills { get; set; } = new();

    // Inventory
    public List<Item> Inventory { get; set; } = new();

    // Equipment slots (13 total)
    // Weapons & Off-hand
    public Item? EquippedMainHand { get; set; }
    public Item? EquippedOffHand { get; set; }
    
    // Armor pieces (8 slots)
    public Item? EquippedHelmet { get; set; }
    public Item? EquippedShoulders { get; set; }
    public Item? EquippedChest { get; set; }
    public Item? EquippedBracers { get; set; }
    public Item? EquippedGloves { get; set; }
    public Item? EquippedBelt { get; set; }
    public Item? EquippedLegs { get; set; }
    public Item? EquippedBoots { get; set; }
    
    // Jewelry (3 slots)
    public Item? EquippedNecklace { get; set; }
    public Item? EquippedRing1 { get; set; }
    public Item? EquippedRing2 { get; set; }

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
        
        // Recalculate max health and mana from new level
        MaxHealth = GetMaxHealth();
        Health = MaxHealth;  // Fully heal on level up
        MaxMana = GetMaxMana();
        Mana = MaxMana;  // Fully restore mana on level up
        
        // Award attribute and skill points for player to allocate
        int attributePoints = 3; // 3 attribute points per level
        int skillPoints = 1;     // 1 skill point per level
        
        // Every 5 levels, award bonus points
        if (Level % 5 == 0)
        {
            attributePoints += 2; // Bonus at levels 5, 10, 15, etc.
            skillPoints += 1;
        }
        
        UnspentAttributePoints += attributePoints;
        UnspentSkillPoints += skillPoints;
        
        // Queue this level-up for player interaction
        PendingLevelUps.Add(new LevelUpInfo
        {
            NewLevel = Level,
            AttributePointsGained = attributePoints,
            SkillPointsGained = skillPoints,
            IsProcessed = false
        });
    }

    /// <summary>
    /// Calculate maximum health from Constitution and level.
    /// </summary>
    public int GetMaxHealth()
    {
        return (Constitution * 10) + (Level * 5);
    }

    /// <summary>
    /// Calculate maximum mana from Wisdom and level.
    /// </summary>
    public int GetMaxMana()
    {
        int baseMana = (Wisdom * 5) + (Level * 3);
        double skillMultiplier = SkillEffectCalculator.GetMaxManaMultiplier(this);
        return (int)(baseMana * skillMultiplier);
    }

    /// <summary>
    /// Calculate physical damage bonus from Strength.
    /// </summary>
    public int GetPhysicalDamageBonus()
    {
        return Strength;
    }

    /// <summary>
    /// Calculate magic damage bonus from Intelligence.
    /// </summary>
    public int GetMagicDamageBonus()
    {
        return Intelligence;
    }

    /// <summary>
    /// Calculate dodge chance percentage from Dexterity.
    /// </summary>
    public double GetDodgeChance()
    {
        return Dexterity * 0.5;  // 10 DEX = 5% dodge
    }

    /// <summary>
    /// Calculate critical hit chance percentage from Dexterity.
    /// </summary>
    public double GetCriticalChance()
    {
        return Dexterity * 0.3;  // 10 DEX = 3% crit
    }

    /// <summary>
    /// Calculate physical defense from Constitution and equipment.
    /// </summary>
    public int GetPhysicalDefense(List<EquipmentSet>? sets = null)
    {
        int defense = Constitution;  // Base defense from CON
        
        // Add equipment defense bonuses
        defense += EquippedMainHand?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedOffHand?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedHelmet?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedShoulders?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedChest?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedBracers?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedGloves?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedBelt?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedLegs?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedBoots?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedNecklace?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedRing1?.GetTotalBonusConstitution() ?? 0;
        defense += EquippedRing2?.GetTotalBonusConstitution() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Constitution");
            defense += setBonuses.Values.Sum();
        }
        
        return defense;
    }

    /// <summary>
    /// Calculate magic resistance percentage from Wisdom.
    /// </summary>
    public double GetMagicResistance()
    {
        return Wisdom * 0.8;  // 10 WIS = 8% magic resist
    }

    /// <summary>
    /// Calculate shop discount percentage from Charisma.
    /// </summary>
    public double GetShopDiscount()
    {
        return Charisma * 1.0;  // 10 CHA = 10% discount
    }

    /// <summary>
    /// Calculate rare item find chance from Charisma.
    /// </summary>
    public double GetRareItemChance()
    {
        double baseChance = Charisma * 0.5;  // 10 CHA = 5% rare find
        double skillBonus = SkillEffectCalculator.GetRareItemFindBonus(this);
        return baseChance + skillBonus;
    }

    /// <summary>
    /// Check if character is alive (health > 0).
    /// </summary>
    public bool IsAlive()
    {
        return Health > 0;
    }

    /// <summary>
    /// Calculate total strength (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalStrength(List<EquipmentSet>? sets = null)
    {
        int total = Strength;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusStrength() ?? 0;
        total += EquippedOffHand?.GetTotalBonusStrength() ?? 0;
        total += EquippedHelmet?.GetTotalBonusStrength() ?? 0;
        total += EquippedShoulders?.GetTotalBonusStrength() ?? 0;
        total += EquippedChest?.GetTotalBonusStrength() ?? 0;
        total += EquippedBracers?.GetTotalBonusStrength() ?? 0;
        total += EquippedGloves?.GetTotalBonusStrength() ?? 0;
        total += EquippedBelt?.GetTotalBonusStrength() ?? 0;
        total += EquippedLegs?.GetTotalBonusStrength() ?? 0;
        total += EquippedBoots?.GetTotalBonusStrength() ?? 0;
        total += EquippedNecklace?.GetTotalBonusStrength() ?? 0;
        total += EquippedRing1?.GetTotalBonusStrength() ?? 0;
        total += EquippedRing2?.GetTotalBonusStrength() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Strength");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Calculate total dexterity (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalDexterity(List<EquipmentSet>? sets = null)
    {
        int total = Dexterity;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusDexterity() ?? 0;
        total += EquippedOffHand?.GetTotalBonusDexterity() ?? 0;
        total += EquippedHelmet?.GetTotalBonusDexterity() ?? 0;
        total += EquippedShoulders?.GetTotalBonusDexterity() ?? 0;
        total += EquippedChest?.GetTotalBonusDexterity() ?? 0;
        total += EquippedBracers?.GetTotalBonusDexterity() ?? 0;
        total += EquippedGloves?.GetTotalBonusDexterity() ?? 0;
        total += EquippedBelt?.GetTotalBonusDexterity() ?? 0;
        total += EquippedLegs?.GetTotalBonusDexterity() ?? 0;
        total += EquippedBoots?.GetTotalBonusDexterity() ?? 0;
        total += EquippedNecklace?.GetTotalBonusDexterity() ?? 0;
        total += EquippedRing1?.GetTotalBonusDexterity() ?? 0;
        total += EquippedRing2?.GetTotalBonusDexterity() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Dexterity");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Calculate total constitution (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalConstitution(List<EquipmentSet>? sets = null)
    {
        int total = Constitution;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusConstitution() ?? 0;
        total += EquippedOffHand?.GetTotalBonusConstitution() ?? 0;
        total += EquippedHelmet?.GetTotalBonusConstitution() ?? 0;
        total += EquippedShoulders?.GetTotalBonusConstitution() ?? 0;
        total += EquippedChest?.GetTotalBonusConstitution() ?? 0;
        total += EquippedBracers?.GetTotalBonusConstitution() ?? 0;
        total += EquippedGloves?.GetTotalBonusConstitution() ?? 0;
        total += EquippedBelt?.GetTotalBonusConstitution() ?? 0;
        total += EquippedLegs?.GetTotalBonusConstitution() ?? 0;
        total += EquippedBoots?.GetTotalBonusConstitution() ?? 0;
        total += EquippedNecklace?.GetTotalBonusConstitution() ?? 0;
        total += EquippedRing1?.GetTotalBonusConstitution() ?? 0;
        total += EquippedRing2?.GetTotalBonusConstitution() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Constitution");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Calculate total intelligence (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalIntelligence(List<EquipmentSet>? sets = null)
    {
        int total = Intelligence;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedOffHand?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedHelmet?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedShoulders?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedChest?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedBracers?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedGloves?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedBelt?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedLegs?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedBoots?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedNecklace?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedRing1?.GetTotalBonusIntelligence() ?? 0;
        total += EquippedRing2?.GetTotalBonusIntelligence() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Intelligence");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Calculate total wisdom (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalWisdom(List<EquipmentSet>? sets = null)
    {
        int total = Wisdom;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusWisdom() ?? 0;
        total += EquippedOffHand?.GetTotalBonusWisdom() ?? 0;
        total += EquippedHelmet?.GetTotalBonusWisdom() ?? 0;
        total += EquippedShoulders?.GetTotalBonusWisdom() ?? 0;
        total += EquippedChest?.GetTotalBonusWisdom() ?? 0;
        total += EquippedBracers?.GetTotalBonusWisdom() ?? 0;
        total += EquippedGloves?.GetTotalBonusWisdom() ?? 0;
        total += EquippedBelt?.GetTotalBonusWisdom() ?? 0;
        total += EquippedLegs?.GetTotalBonusWisdom() ?? 0;
        total += EquippedBoots?.GetTotalBonusWisdom() ?? 0;
        total += EquippedNecklace?.GetTotalBonusWisdom() ?? 0;
        total += EquippedRing1?.GetTotalBonusWisdom() ?? 0;
        total += EquippedRing2?.GetTotalBonusWisdom() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Wisdom");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Calculate total charisma (base + equipment bonuses + set bonuses).
    /// </summary>
    public int GetTotalCharisma(List<EquipmentSet>? sets = null)
    {
        int total = Charisma;  // Base D20 attribute
        total += EquippedMainHand?.GetTotalBonusCharisma() ?? 0;
        total += EquippedOffHand?.GetTotalBonusCharisma() ?? 0;
        total += EquippedHelmet?.GetTotalBonusCharisma() ?? 0;
        total += EquippedShoulders?.GetTotalBonusCharisma() ?? 0;
        total += EquippedChest?.GetTotalBonusCharisma() ?? 0;
        total += EquippedBracers?.GetTotalBonusCharisma() ?? 0;
        total += EquippedGloves?.GetTotalBonusCharisma() ?? 0;
        total += EquippedBelt?.GetTotalBonusCharisma() ?? 0;
        total += EquippedLegs?.GetTotalBonusCharisma() ?? 0;
        total += EquippedBoots?.GetTotalBonusCharisma() ?? 0;
        total += EquippedNecklace?.GetTotalBonusCharisma() ?? 0;
        total += EquippedRing1?.GetTotalBonusCharisma() ?? 0;
        total += EquippedRing2?.GetTotalBonusCharisma() ?? 0;
        
        // Add set bonuses
        if (sets != null)
        {
            var setBonuses = GetSetBonuses(sets, "Charisma");
            total += setBonuses.Values.Sum();
        }
        
        return total;
    }

    /// <summary>
    /// Get all currently equipped items.
    /// </summary>
    public List<Item> GetEquippedItems()
    {
        var items = new List<Item>();
        
        if (EquippedMainHand != null) items.Add(EquippedMainHand);
        if (EquippedOffHand != null) items.Add(EquippedOffHand);
        if (EquippedHelmet != null) items.Add(EquippedHelmet);
        if (EquippedShoulders != null) items.Add(EquippedShoulders);
        if (EquippedChest != null) items.Add(EquippedChest);
        if (EquippedBracers != null) items.Add(EquippedBracers);
        if (EquippedGloves != null) items.Add(EquippedGloves);
        if (EquippedBelt != null) items.Add(EquippedBelt);
        if (EquippedLegs != null) items.Add(EquippedLegs);
        if (EquippedBoots != null) items.Add(EquippedBoots);
        if (EquippedNecklace != null) items.Add(EquippedNecklace);
        if (EquippedRing1 != null) items.Add(EquippedRing1);
        if (EquippedRing2 != null) items.Add(EquippedRing2);
        
        return items;
    }

    /// <summary>
    /// Detect which equipment sets are partially or fully equipped.
    /// Returns a dictionary of set name -> number of pieces equipped.
    /// </summary>
    public Dictionary<string, int> GetActiveEquipmentSets()
    {
        var equippedItems = GetEquippedItems();
        var sets = new Dictionary<string, int>();
        
        foreach (var item in equippedItems)
        {
            if (!string.IsNullOrEmpty(item.SetName))
            {
                if (sets.ContainsKey(item.SetName))
                    sets[item.SetName]++;
                else
                    sets[item.SetName] = 1;
            }
        }
        
        return sets;
    }

    /// <summary>
    /// Calculate set bonuses from equipped sets.
    /// </summary>
    public Dictionary<string, int> GetSetBonuses(List<EquipmentSet> availableSets, string statType)
    {
        var activeSets = GetActiveEquipmentSets();
        var bonuses = new Dictionary<string, int>();
        
        foreach (var (setName, piecesEquipped) in activeSets)
        {
            var set = availableSets.FirstOrDefault(s => s.Name == setName);
            if (set == null) continue;
            
            // Check which bonuses are active based on pieces equipped
            foreach (var (requiredPieces, bonus) in set.Bonuses)
            {
                if (piecesEquipped >= requiredPieces)
                {
                    var bonusValue = statType switch
                    {
                        "Strength" => bonus.BonusStrength,
                        "Dexterity" => bonus.BonusDexterity,
                        "Constitution" => bonus.BonusConstitution,
                        "Intelligence" => bonus.BonusIntelligence,
                        "Wisdom" => bonus.BonusWisdom,
                        "Charisma" => bonus.BonusCharisma,
                        _ => 0
                    };
                    
                    if (bonusValue > 0)
                    {
                        var key = $"{setName} ({requiredPieces} pieces)";
                        bonuses[key] = bonusValue;
                    }
                }
            }
        }
        
        return bonuses;
    }
}
