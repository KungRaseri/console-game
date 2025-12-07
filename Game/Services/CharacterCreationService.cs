using Game.Data;
using Game.Models;

namespace Game.Services;

/// <summary>
/// Service for creating new characters with class selection and attribute allocation.
/// </summary>
public static class CharacterCreationService
{
    /// <summary>
    /// Create a new character with the specified name, class, and attribute allocation.
    /// </summary>
    public static Character CreateCharacter(string name, string className, AttributeAllocation allocation)
    {
        var characterClass = CharacterClassRepository.GetClassByName(className);
        if (characterClass == null)
        {
            throw new ArgumentException($"Unknown class: {className}");
        }
        
        if (!allocation.IsValid())
        {
            throw new ArgumentException("Invalid attribute allocation. Check points and limits.");
        }
        
        // Create character with allocated attributes
        var character = new Character
        {
            Name = name,
            ClassName = characterClass.Name,
            Strength = allocation.Strength + characterClass.BonusStrength,
            Dexterity = allocation.Dexterity + characterClass.BonusDexterity,
            Constitution = allocation.Constitution + characterClass.BonusConstitution,
            Intelligence = allocation.Intelligence + characterClass.BonusIntelligence,
            Wisdom = allocation.Wisdom + characterClass.BonusWisdom,
            Charisma = allocation.Charisma + characterClass.BonusCharisma,
            Level = 1,
            Experience = 0,
            Gold = 100 // Starting gold
        };
        
        // Calculate initial max health and mana
        character.MaxHealth = character.GetMaxHealth() + characterClass.StartingHealthBonus;
        character.Health = character.MaxHealth;
        
        character.MaxMana = character.GetMaxMana() + characterClass.StartingManaBonus;
        character.Mana = character.MaxMana;
        
        // Give starting equipment
        GiveStartingEquipment(character, characterClass);
        
        return character;
    }
    
    /// <summary>
    /// Give the character their class-appropriate starting equipment.
    /// </summary>
    private static void GiveStartingEquipment(Character character, CharacterClass characterClass)
    {
        foreach (var itemName in characterClass.StartingEquipment)
        {
            var item = CreateStartingItem(itemName);
            if (item != null)
            {
                character.Inventory.Add(item);
            }
        }
    }
    
    /// <summary>
    /// Create a basic starting item by name (no stats, just the item shell).
    /// </summary>
    private static Item? CreateStartingItem(string name)
    {
        // Determine item type and rarity based on name
        var (type, rarity) = DetermineItemProperties(name);
        
        return new Item
        {
            Name = name,
            Description = $"A basic {name.ToLower()} for starting adventurers.",
            Type = type,
            Rarity = ItemRarity.Common,
            Price = 10,
            // Starting items have minimal bonuses
            BonusStrength = type == ItemType.Weapon ? 1 : 0,
            BonusConstitution = IsArmorType(type) ? 1 : 0,
            BonusDexterity = type == ItemType.Weapon && name.Contains("Bow") ? 1 : 0
        };
    }
    
    /// <summary>
    /// Determine item type and rarity from name.
    /// </summary>
    private static (ItemType type, ItemRarity rarity) DetermineItemProperties(string name)
    {
        var nameLower = name.ToLower();
        
        // Weapons
        if (nameLower.Contains("sword") || nameLower.Contains("axe") || nameLower.Contains("mace") 
            || nameLower.Contains("dagger") || nameLower.Contains("bow") || nameLower.Contains("staff"))
            return (ItemType.Weapon, ItemRarity.Common);
        
        // Shields
        if (nameLower.Contains("shield"))
            return (ItemType.Shield, ItemRarity.Common);
        
        // Off-hand items
        if (nameLower.Contains("tome") || nameLower.Contains("orb") || nameLower.Contains("focus"))
            return (ItemType.OffHand, ItemRarity.Common);
        
        // Armor pieces
        if (nameLower.Contains("helmet") || nameLower.Contains("helm") || nameLower.Contains("hood") || nameLower.Contains("circlet"))
            return (ItemType.Helmet, ItemRarity.Common);
        
        if (nameLower.Contains("shoulder") || nameLower.Contains("pauldron") || nameLower.Contains("mantle"))
            return (ItemType.Shoulders, ItemRarity.Common);
        
        if (nameLower.Contains("chest") || nameLower.Contains("robe") || nameLower.Contains("plate") && !nameLower.Contains("bracer"))
            return (ItemType.Chest, ItemRarity.Common);
        
        if (nameLower.Contains("bracer") || nameLower.Contains("wristguard"))
            return (ItemType.Bracers, ItemRarity.Common);
        
        if (nameLower.Contains("glove") || nameLower.Contains("gauntlet") || nameLower.Contains("hand"))
            return (ItemType.Gloves, ItemRarity.Common);
        
        if (nameLower.Contains("belt") || nameLower.Contains("girdle"))
            return (ItemType.Belt, ItemRarity.Common);
        
        if (nameLower.Contains("legging") || nameLower.Contains("greave") || nameLower.Contains("pant"))
            return (ItemType.Legs, ItemRarity.Common);
        
        if (nameLower.Contains("boot") || nameLower.Contains("shoe") || nameLower.Contains("sabatons"))
            return (ItemType.Boots, ItemRarity.Common);
        
        // Default
        return (ItemType.Consumable, ItemRarity.Common);
    }
    
    /// <summary>
    /// Check if an item type is armor.
    /// </summary>
    private static bool IsArmorType(ItemType type)
    {
        return type == ItemType.Helmet || type == ItemType.Shoulders || type == ItemType.Chest 
            || type == ItemType.Bracers || type == ItemType.Gloves || type == ItemType.Belt 
            || type == ItemType.Legs || type == ItemType.Boots || type == ItemType.Shield;
    }
    
    /// <summary>
    /// Get a quick-start character for testing (auto-allocated attributes).
    /// </summary>
    public static Character CreateQuickStartCharacter(string name, string className)
    {
        var characterClass = CharacterClassRepository.GetClassByName(className);
        if (characterClass == null)
        {
            throw new ArgumentException($"Unknown class: {className}");
        }
        
        // Auto-allocate attributes based on class primary stats
        var allocation = new AttributeAllocation();
        
        // Put more points in primary attributes
        foreach (var primary in characterClass.PrimaryAttributes)
        {
            allocation.SetAttributeValue(primary, 14); // High in primary stats
        }
        
        // Distribute remaining points evenly
        var remaining = allocation.GetRemainingPoints();
        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" }
            .Where(a => !characterClass.PrimaryAttributes.Contains(a))
            .ToList();
        
        foreach (var attr in attributes)
        {
            if (remaining <= 0) break;
            var current = allocation.GetAttributeValue(attr);
            if (current < 10)
            {
                allocation.SetAttributeValue(attr, 10);
                remaining = allocation.GetRemainingPoints();
            }
        }
        
        return CreateCharacter(name, className, allocation);
    }
}
