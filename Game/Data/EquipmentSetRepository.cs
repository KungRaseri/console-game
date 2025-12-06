using Game.Models;

namespace Game.Data;

/// <summary>
/// Repository of predefined equipment sets.
/// </summary>
public static class EquipmentSetRepository
{
    public static List<EquipmentSet> GetAllSets()
    {
        return new List<EquipmentSet>
        {
            CreateDragonbornSet(),
            CreateShadowAssassinSet(),
            CreateArcaneScholarSet(),
            CreateIronGuardianSet(),
            CreateStormcallerSet()
        };
    }

    private static EquipmentSet CreateDragonbornSet()
    {
        return new EquipmentSet
        {
            Name = "Dragonborn",
            Description = "Forged from dragon scales, this armor grants incredible strength and vitality.",
            SetItemNames = new List<string>
            {
                "Dragonborn Helmet",
                "Dragonborn Shoulders",
                "Dragonborn Chestpiece",
                "Dragonborn Gauntlets",
                "Dragonborn Greaves",
                "Dragonborn Boots"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, Description = "+15 Strength", BonusStrength = 15 } },
                { 4, new SetBonus { PiecesRequired = 4, Description = "+30 Vitality", BonusVitality = 30 } },
                { 6, new SetBonus { PiecesRequired = 6, Description = "+50 Strength, +25 Defense", BonusStrength = 50, BonusDefense = 25 } }
            }
        };
    }

    private static EquipmentSet CreateShadowAssassinSet()
    {
        return new EquipmentSet
        {
            Name = "Shadow Assassin",
            Description = "Crafted for stealth and precision, favored by rogues and assassins.",
            SetItemNames = new List<string>
            {
                "Shadow Assassin Hood",
                "Shadow Assassin Cloak",
                "Shadow Assassin Gloves",
                "Shadow Assassin Leggings",
                "Shadow Assassin Boots"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, Description = "+20 Agility", BonusAgility = 20 } },
                { 4, new SetBonus { PiecesRequired = 4, Description = "+15 Strength, +30 Agility", BonusStrength = 15, BonusAgility = 30 } },
                { 5, new SetBonus { PiecesRequired = 5, Description = "+40 Agility, +25 Defense", BonusAgility = 40, BonusDefense = 25, SpecialEffect = "Critical Strike Chance +10%" } }
            }
        };
    }

    private static EquipmentSet CreateArcaneScholarSet()
    {
        return new EquipmentSet
        {
            Name = "Arcane Scholar",
            Description = "Imbued with magical energy, perfect for spellcasters.",
            SetItemNames = new List<string>
            {
                "Arcane Scholar Circlet",
                "Arcane Scholar Robes",
                "Arcane Scholar Gloves",
                "Arcane Scholar Belt",
                "Arcane Scholar Boots"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, Description = "+25 Intelligence", BonusIntelligence = 25 } },
                { 3, new SetBonus { PiecesRequired = 3, Description = "+40 Intelligence", BonusIntelligence = 40 } },
                { 5, new SetBonus { PiecesRequired = 5, Description = "+60 Intelligence, +20 Vitality", BonusIntelligence = 60, BonusVitality = 20, SpecialEffect = "Spell Power +15%" } }
            }
        };
    }

    private static EquipmentSet CreateIronGuardianSet()
    {
        return new EquipmentSet
        {
            Name = "Iron Guardian",
            Description = "Heavy plate armor providing maximum protection.",
            SetItemNames = new List<string>
            {
                "Iron Guardian Helm",
                "Iron Guardian Pauldrons",
                "Iron Guardian Plate",
                "Iron Guardian Gauntlets",
                "Iron Guardian Greaves",
                "Iron Guardian Sabatons"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, Description = "+20 Defense", BonusDefense = 20 } },
                { 4, new SetBonus { PiecesRequired = 4, Description = "+40 Defense, +15 Vitality", BonusDefense = 40, BonusVitality = 15 } },
                { 6, new SetBonus { PiecesRequired = 6, Description = "+70 Defense, +30 Vitality", BonusDefense = 70, BonusVitality = 30, SpecialEffect = "Damage Reduction +20%" } }
            }
        };
    }

    private static EquipmentSet CreateStormcallerSet()
    {
        return new EquipmentSet
        {
            Name = "Stormcaller",
            Description = "Channels the power of storms, balancing magic and agility.",
            SetItemNames = new List<string>
            {
                "Stormcaller Crown",
                "Stormcaller Mantle",
                "Stormcaller Vestments",
                "Stormcaller Bindings",
                "Stormcaller Treads"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, Description = "+15 Intelligence, +10 Agility", BonusIntelligence = 15, BonusAgility = 10 } },
                { 3, new SetBonus { PiecesRequired = 3, Description = "+25 Intelligence, +20 Agility", BonusIntelligence = 25, BonusAgility = 20 } },
                { 5, new SetBonus { PiecesRequired = 5, Description = "+45 Intelligence, +35 Agility", BonusIntelligence = 45, BonusAgility = 35, SpecialEffect = "Lightning Damage +25%" } }
            }
        };
    }
}
