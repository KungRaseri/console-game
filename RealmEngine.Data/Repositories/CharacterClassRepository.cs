using RealmEngine.Shared.Models;
using RealmEngine.Shared.Abstractions;

namespace RealmEngine.Data.Repositories;

/// <summary>
/// Repository of predefined character classes.
/// </summary>
public class CharacterClassRepository : ICharacterClassRepository
{
    public List<CharacterClass> GetAllClasses()
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

    public CharacterClass? GetClassByName(string name)
    {
        return GetAllClasses().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // Interface implementation
    public CharacterClass? GetById(string id) => GetByName(id); // ID is the name for character classes
    public CharacterClass? GetByName(string name) => GetClassByName(name);
    public List<CharacterClass> GetAll() => GetAllClasses();
    public void Add(CharacterClass entity) => throw new NotSupportedException("Character classes are predefined");
    public void Update(CharacterClass entity) => throw new NotSupportedException("Character classes are predefined");
    public void Delete(string id) => throw new NotSupportedException("Character classes are predefined");
    public void Dispose() { } // No resources to dispose

    private CharacterClass CreateWarriorClass()
    {
        return new CharacterClass
        {
            Name = "Warrior",
            Description = "Masters of melee combat, warriors excel in strength and endurance.",
            PrimaryAttributes = new List<string> { "Strength", "Constitution" },
            BonusStrength = 2,
            BonusConstitution = 1,
            StartingHealth = 10,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/swords:longsword",
                "@items/armor/shields:wooden-shield",
                "@items/armor/head:helmet",
                "@items/armor/chest:chainmail",
                "@items/armor/hands:leather-gloves",
                "@items/armor/legs:chain-leggings",
                "@items/armor/feet:leather-boots"
            },
            FlavorText = "\"Steel and valor are my weapons. I stand unbroken against any foe.\""
        };
    }

    private CharacterClass CreateRogueClass()
    {
        return new CharacterClass
        {
            Name = "Rogue",
            Description = "Swift and cunning, rogues rely on dexterity and deception.",
            PrimaryAttributes = new List<string> { "Dexterity", "Charisma" },
            BonusDexterity = 2,
            BonusCharisma = 1,
            StartingHealth = 0,
            StartingMana = 5,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/daggers:dagger",
                "@items/armor/head:hood",
                "@items/armor/shoulders:leather-shoulders",
                "@items/armor/chest:leather-armor",
                "@items/armor/hands:leather-gloves",
                "@items/armor/waist:leather-belt",
                "@items/armor/legs:leather-leggings",
                "@items/armor/feet:leather-boots"
            },
            FlavorText = "\"Shadows are my ally. They'll never see me coming.\""
        };
    }

    private CharacterClass CreateMageClass()
    {
        return new CharacterClass
        {
            Name = "Mage",
            Description = "Wielders of arcane power, mages command devastating spells.",
            PrimaryAttributes = new List<string> { "Intelligence", "Wisdom" },
            BonusIntelligence = 2,
            BonusWisdom = 1,
            StartingHealth = -5,
            StartingMana = 20,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/staves:quarterstaff",
                "@items/accessories/jewelry:amulet",
                "@items/armor/head:circlet",
                "@items/armor/chest:robes",
                "@items/armor/hands:silk-gloves",
                "@items/armor/waist:cloth-belt",
                "@items/armor/feet:cloth-boots"
            },
            FlavorText = "\"Knowledge is the ultimate power. Reality bends to my will.\""
        };
    }

    private CharacterClass CreateClericClass()
    {
        return new CharacterClass
        {
            Name = "Cleric",
            Description = "Holy warriors who heal allies and smite the wicked.",
            PrimaryAttributes = new List<string> { "Wisdom", "Constitution" },
            BonusWisdom = 2,
            BonusConstitution = 1,
            StartingHealth = 5,
            StartingMana = 15,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/maces:mace",
                "@items/armor/shields:wooden-shield",
                "@items/armor/head:helmet",
                "@items/armor/shoulders:chain-shoulders",
                "@items/armor/chest:chainmail",
                "@items/armor/arms:chain-bracers",
                "@items/armor/hands:chain-gloves",
                "@items/armor/waist:leather-belt",
                "@items/armor/legs:chain-leggings",
                "@items/armor/feet:chain-boots"
            },
            FlavorText = "\"By faith and steel, I will protect the innocent and vanquish evil.\""
        };
    }

    private CharacterClass CreateRangerClass()
    {
        return new CharacterClass
        {
            Name = "Ranger",
            Description = "Expert trackers and archers, rangers are one with nature.",
            PrimaryAttributes = new List<string> { "Dexterity", "Wisdom" },
            BonusDexterity = 2,
            BonusWisdom = 1,
            StartingHealth = 3,
            StartingMana = 10,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/bows:longbow",
                "@items/armor/head:hood",
                "@items/armor/shoulders:leather-shoulders",
                "@items/armor/chest:studded-leather",
                "@items/armor/arms:leather-bracers",
                "@items/armor/hands:leather-gloves",
                "@items/armor/waist:leather-belt",
                "@items/armor/legs:leather-leggings",
                "@items/armor/feet:leather-boots"
            },
            FlavorText = "\"The wild is my home. My arrows never miss their mark.\""
        };
    }

    private CharacterClass CreatePaladinClass()
    {
        return new CharacterClass
        {
            Name = "Paladin",
            Description = "Divine champions who blend martial prowess with holy magic.",
            PrimaryAttributes = new List<string> { "Strength", "Charisma" },
            BonusStrength = 1,
            BonusCharisma = 2,
            StartingHealth = 8,
            StartingMana = 12,
            StartingEquipmentIds = new List<string>
            {
                "@items/weapons/swords:longsword",
                "@items/armor/shields:steel-shield",
                "@items/armor/head:great-helm",
                "@items/armor/shoulders:plate-shoulders",
                "@items/armor/chest:plate-armor",
                "@items/armor/arms:plate-bracers",
                "@items/armor/hands:plate-gauntlets",
                "@items/armor/waist:plate-belt",
                "@items/armor/legs:plate-leggings",
                "@items/armor/feet:plate-boots"
            },
            FlavorText = "\"I am the light in the darkness, a beacon of hope for all.\""
        };
    }
}
