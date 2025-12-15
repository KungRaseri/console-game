using FluentAssertions;
using Game.Core.Models;

namespace Game.Tests.Models;

/// <summary>
/// Comprehensive tests for EquipmentSet model.
/// Target: 0% -> 100% coverage.
/// </summary>
public class EquipmentSetTests
{
    #region Initialization Tests

    [Fact]
    public void EquipmentSet_Should_Initialize_With_Default_Values()
    {
        // Act
        var set = new EquipmentSet();

        // Assert
        set.Id.Should().NotBeEmpty();
        set.Name.Should().BeEmpty();
        set.Description.Should().BeEmpty();
        set.SetItemNames.Should().NotBeNull();
        set.SetItemNames.Should().BeEmpty();
        set.Bonuses.Should().NotBeNull();
        set.Bonuses.Should().BeEmpty();
    }

    [Fact]
    public void EquipmentSet_Should_Generate_Unique_Ids()
    {
        // Act
        var set1 = new EquipmentSet();
        var set2 = new EquipmentSet();

        // Assert
        set1.Id.Should().NotBe(set2.Id);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void EquipmentSet_Should_Allow_Name_Assignment()
    {
        // Arrange
        var set = new EquipmentSet();

        // Act
        set.Name = "Dragon Armor Set";

        // Assert
        set.Name.Should().Be("Dragon Armor Set");
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Description_Assignment()
    {
        // Arrange
        var set = new EquipmentSet();

        // Act
        set.Description = "Forged from dragon scales";

        // Assert
        set.Description.Should().Be("Forged from dragon scales");
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Adding_Set_Items()
    {
        // Arrange
        var set = new EquipmentSet();

        // Act
        set.SetItemNames.Add("Dragon Helm");
        set.SetItemNames.Add("Dragon Chestplate");
        set.SetItemNames.Add("Dragon Greaves");

        // Assert
        set.SetItemNames.Should().HaveCount(3);
        set.SetItemNames.Should().Contain("Dragon Helm");
        set.SetItemNames.Should().Contain("Dragon Chestplate");
        set.SetItemNames.Should().Contain("Dragon Greaves");
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Adding_Bonuses()
    {
        // Arrange
        var set = new EquipmentSet();
        var twoSetBonus = new SetBonus
        {
            PiecesRequired = 2,
            Description = "+10 Defense",
            BonusConstitution = 2
        };
        var fourSetBonus = new SetBonus
        {
            PiecesRequired = 4,
            Description = "+20 Strength",
            BonusStrength = 4
        };

        // Act
        set.Bonuses[2] = twoSetBonus;
        set.Bonuses[4] = fourSetBonus;

        // Assert
        set.Bonuses.Should().HaveCount(2);
        set.Bonuses[2].Should().Be(twoSetBonus);
        set.Bonuses[4].Should().Be(fourSetBonus);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void EquipmentSet_Should_Create_Complete_Armor_Set()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Dragonscale Armor",
            Description = "Legendary armor crafted from ancient dragon scales",
            SetItemNames = new List<string>
            {
                "Dragonscale Helm",
                "Dragonscale Chestplate",
                "Dragonscale Gauntlets",
                "Dragonscale Greaves",
                "Dragonscale Boots",
                "Dragonscale Cape"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                {
                    2, new SetBonus
                    {
                        PiecesRequired = 2,
                        Description = "+15 Defense",
                        BonusConstitution = 3
                    }
                },
                {
                    4, new SetBonus
                    {
                        PiecesRequired = 4,
                        Description = "+20% Fire Resistance",
                        BonusIntelligence = 5,
                        SpecialEffect = "FireResistance"
                    }
                },
                {
                    6, new SetBonus
                    {
                        PiecesRequired = 6,
                        Description = "Dragon's Fury - +50 All Stats",
                        BonusStrength = 10,
                        BonusDexterity = 10,
                        BonusConstitution = 10,
                        BonusIntelligence = 10,
                        BonusWisdom = 10,
                        BonusCharisma = 10,
                        SpecialEffect = "DragonFury"
                    }
                }
            }
        };

        // Assert
        set.Name.Should().Be("Dragonscale Armor");
        set.SetItemNames.Should().HaveCount(6);
        set.Bonuses.Should().HaveCount(3);
        set.Bonuses[2].BonusConstitution.Should().Be(3);
        set.Bonuses[4].SpecialEffect.Should().Be("FireResistance");
        set.Bonuses[6].BonusStrength.Should().Be(10);
    }

    [Fact]
    public void EquipmentSet_Should_Create_Minimal_Set()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Basic Leather Set",
            SetItemNames = new List<string> { "Leather Tunic", "Leather Pants" },
            Bonuses = new Dictionary<int, SetBonus>
            {
                {
                    2, new SetBonus
                    {
                        PiecesRequired = 2,
                        Description = "+5 Dexterity",
                        BonusDexterity = 1
                    }
                }
            }
        };

        // Assert
        set.Name.Should().Be("Basic Leather Set");
        set.SetItemNames.Should().HaveCount(2);
        set.Bonuses.Should().HaveCount(1);
        set.Bonuses[2].BonusDexterity.Should().Be(1);
    }

    [Fact]
    public void EquipmentSet_Should_Support_Empty_Set_Items()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Incomplete Set"
        };

        // Assert
        set.SetItemNames.Should().BeEmpty();
    }

    [Fact]
    public void EquipmentSet_Should_Support_Empty_Bonuses()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "No Bonus Set",
            SetItemNames = new List<string> { "Item1", "Item2" }
        };

        // Assert
        set.Bonuses.Should().BeEmpty();
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Multiple_Items_In_Set()
    {
        // Arrange
        var set = new EquipmentSet();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            set.SetItemNames.Add($"Item {i}");
        }

        // Assert
        set.SetItemNames.Should().HaveCount(10);
    }

    [Fact]
    public void EquipmentSet_Should_Support_Non_Sequential_Bonus_Tiers()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Strange Set",
            Bonuses = new Dictionary<int, SetBonus>
            {
                { 2, new SetBonus { PiecesRequired = 2, BonusStrength = 2 } },
                { 5, new SetBonus { PiecesRequired = 5, BonusStrength = 5 } },
                { 8, new SetBonus { PiecesRequired = 8, BonusStrength = 8 } }
            }
        };

        // Assert
        set.Bonuses.Should().HaveCount(3);
        set.Bonuses.Should().ContainKeys(2, 5, 8);
        set.Bonuses[2].BonusStrength.Should().Be(2);
        set.Bonuses[5].BonusStrength.Should().Be(5);
        set.Bonuses[8].BonusStrength.Should().Be(8);
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Updating_Set_Items()
    {
        // Arrange
        var set = new EquipmentSet
        {
            SetItemNames = new List<string> { "Item1", "Item2" }
        };

        // Act
        set.SetItemNames.Add("Item3");
        set.SetItemNames.Remove("Item1");

        // Assert
        set.SetItemNames.Should().HaveCount(2);
        set.SetItemNames.Should().Contain("Item2");
        set.SetItemNames.Should().Contain("Item3");
        set.SetItemNames.Should().NotContain("Item1");
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Updating_Bonuses()
    {
        // Arrange
        var set = new EquipmentSet();
        set.Bonuses[2] = new SetBonus { PiecesRequired = 2, BonusStrength = 5 };

        // Act
        set.Bonuses[2] = new SetBonus { PiecesRequired = 2, BonusStrength = 10 };

        // Assert
        set.Bonuses[2].BonusStrength.Should().Be(10);
    }

    [Fact]
    public void EquipmentSet_Should_Support_Large_Set()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Complete Adventurer Set",
            SetItemNames = new List<string>
            {
                "Helm", "Chestplate", "Gauntlets", "Greaves", "Boots",
                "Cape", "Belt", "Ring1", "Ring2", "Amulet", "Weapon", "Shield"
            }
        };

        // Assert
        set.SetItemNames.Should().HaveCount(12);
    }

    [Fact]
    public void EquipmentSet_Should_Allow_Removing_Bonuses()
    {
        // Arrange
        var set = new EquipmentSet();
        set.Bonuses[2] = new SetBonus { PiecesRequired = 2 };
        set.Bonuses[4] = new SetBonus { PiecesRequired = 4 };

        // Act
        set.Bonuses.Remove(2);

        // Assert
        set.Bonuses.Should().HaveCount(1);
        set.Bonuses.Should().ContainKey(4);
        set.Bonuses.Should().NotContainKey(2);
    }

    [Fact]
    public void EquipmentSet_Should_Create_Weapon_Set()
    {
        // Arrange & Act
        var set = new EquipmentSet
        {
            Name = "Legendary Weapons of Power",
            Description = "A collection of powerful weapons",
            SetItemNames = new List<string>
            {
                "Sword of Light",
                "Axe of Thunder",
                "Bow of Wind",
                "Staff of Fire"
            },
            Bonuses = new Dictionary<int, SetBonus>
            {
                {
                    2, new SetBonus
                    {
                        PiecesRequired = 2,
                        Description = "Elemental Mastery I",
                        BonusStrength = 3,
                        BonusIntelligence = 3
                    }
                },
                {
                    4, new SetBonus
                    {
                        PiecesRequired = 4,
                        Description = "Elemental Mastery II",
                        BonusStrength = 10,
                        BonusIntelligence = 10,
                        SpecialEffect = "ElementalMastery"
                    }
                }
            }
        };

        // Assert
        set.Name.Should().Be("Legendary Weapons of Power");
        set.SetItemNames.Should().HaveCount(4);
        set.Bonuses.Should().HaveCount(2);
    }

    #endregion
}
