using Game.Models;

namespace Game.Data;

/// <summary>
/// Repository of predefined character classes.
/// </summary>
public static class CharacterClassRepository
{
    public static List<CharacterClass> GetAllClasses()
    {
        return new List<CharacterClass>
        {
            CreateWarriorClass(),
            CreateRogueClass(),
            CreateMageClass(),
            CreateClericClass(),
            CreateRangerClass(),
            CreatePaladinClass()
        };
    }
    
    public static CharacterClass? GetClassByName(string name)
    {
        return GetAllClasses().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    private static CharacterClass CreateWarriorClass()
    {
        return new CharacterClass
        {
            Name = "Warrior",
            Description = "Masters of melee combat, warriors excel in strength and endurance.",
            PrimaryAttributes = new List<string> { "Strength", "Constitution" },
            BonusStrength = 2,
            BonusConstitution = 1,
            StartingHealthBonus = 10,
            StartingEquipment = new List<string>
            {
                "Iron Sword",
                "Wooden Shield",
                "Leather Helmet",
                "Chain Chestpiece",
                "Leather Gloves",
                "Chain Leggings",
                "Leather Boots"
            },
            FlavorText = "\"Steel and valor are my weapons. I stand unbroken against any foe.\""
        };
    }
    
    private static CharacterClass CreateRogueClass()
    {
        return new CharacterClass
        {
            Name = "Rogue",
            Description = "Swift and cunning, rogues rely on dexterity and deception.",
            PrimaryAttributes = new List<string> { "Dexterity", "Charisma" },
            BonusDexterity = 2,
            BonusCharisma = 1,
            StartingHealthBonus = 0,
            StartingManaBonus = 5,
            StartingEquipment = new List<string>
            {
                "Steel Dagger",
                "Leather Hood",
                "Leather Shoulderpads",
                "Leather Chestpiece",
                "Leather Gloves",
                "Leather Belt",
                "Leather Leggings",
                "Leather Boots"
            },
            FlavorText = "\"Shadows are my ally. They'll never see me coming.\""
        };
    }
    
    private static CharacterClass CreateMageClass()
    {
        return new CharacterClass
        {
            Name = "Mage",
            Description = "Wielders of arcane power, mages command devastating spells.",
            PrimaryAttributes = new List<string> { "Intelligence", "Wisdom" },
            BonusIntelligence = 2,
            BonusWisdom = 1,
            StartingHealthBonus = -5,
            StartingManaBonus = 20,
            StartingEquipment = new List<string>
            {
                "Wooden Staff",
                "Tome of Power",
                "Silk Circlet",
                "Silk Robes",
                "Silk Gloves",
                "Silk Belt",
                "Silk Boots"
            },
            FlavorText = "\"Knowledge is the ultimate power. Reality bends to my will.\""
        };
    }
    
    private static CharacterClass CreateClericClass()
    {
        return new CharacterClass
        {
            Name = "Cleric",
            Description = "Holy warriors who heal allies and smite the wicked.",
            PrimaryAttributes = new List<string> { "Wisdom", "Constitution" },
            BonusWisdom = 2,
            BonusConstitution = 1,
            StartingHealthBonus = 5,
            StartingManaBonus = 15,
            StartingEquipment = new List<string>
            {
                "Iron Mace",
                "Wooden Shield",
                "Chain Helmet",
                "Chain Shoulderpads",
                "Chain Chestpiece",
                "Chain Bracers",
                "Chain Gloves",
                "Leather Belt",
                "Chain Leggings",
                "Chain Boots"
            },
            FlavorText = "\"By faith and steel, I will protect the innocent and vanquish evil.\""
        };
    }
    
    private static CharacterClass CreateRangerClass()
    {
        return new CharacterClass
        {
            Name = "Ranger",
            Description = "Expert trackers and archers, rangers are one with nature.",
            PrimaryAttributes = new List<string> { "Dexterity", "Wisdom" },
            BonusDexterity = 2,
            BonusWisdom = 1,
            StartingHealthBonus = 3,
            StartingManaBonus = 10,
            StartingEquipment = new List<string>
            {
                "Longbow",
                "Leather Hood",
                "Leather Shoulderpads",
                "Studded Leather Chestpiece",
                "Leather Bracers",
                "Leather Gloves",
                "Leather Belt",
                "Leather Leggings",
                "Leather Boots"
            },
            FlavorText = "\"The wild is my home. My arrows never miss their mark.\""
        };
    }
    
    private static CharacterClass CreatePaladinClass()
    {
        return new CharacterClass
        {
            Name = "Paladin",
            Description = "Divine champions who blend martial prowess with holy magic.",
            PrimaryAttributes = new List<string> { "Strength", "Charisma" },
            BonusStrength = 1,
            BonusCharisma = 2,
            StartingHealthBonus = 8,
            StartingManaBonus = 12,
            StartingEquipment = new List<string>
            {
                "Steel Longsword",
                "Steel Shield",
                "Plate Helmet",
                "Plate Shoulderpads",
                "Plate Chestpiece",
                "Plate Bracers",
                "Plate Gauntlets",
                "Plate Belt",
                "Plate Leggings",
                "Plate Boots"
            },
            FlavorText = "\"I am the light in the darkness, a beacon of hope for all.\""
        };
    }
}
