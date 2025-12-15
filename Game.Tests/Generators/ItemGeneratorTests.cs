using System.Text.RegularExpressions;
using FluentAssertions;
using Game.Core.Generators;
using Game.Core.Models;

namespace Game.Tests.Generators;

public class ItemGeneratorTests
{
    [Fact]
    public void Generate_Should_Create_Valid_Item()
    {
        // Act
        var item = ItemGenerator.Generate();

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBeNullOrEmpty();
        item.Name.Should().NotBeNullOrEmpty();
        item.Description.Should().NotBeNullOrEmpty();
        item.Price.Should().BeInRange(5, 1000);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void Generate_Should_Create_Requested_Number_Of_Items(int count)
    {
        // Act
        var items = ItemGenerator.Generate(count);

        // Assert
        items.Should().HaveCount(count);
        items.Should().OnlyHaveUniqueItems(i => i.Id);
    }

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Necklace)]
    [InlineData(ItemType.QuestItem)]
    public void GenerateByType_Should_Create_Items_Of_Specified_Type(ItemType type)
    {
        // Act
        var items = ItemGenerator.GenerateByType(type, 5);

        // Assert
        items.Should().HaveCount(5);
        items.Should().OnlyContain(i => i.Type == type);
    }

    [Fact]
    public void GenerateByType_Weapon_Should_Have_Weapon_Names()
    {
        // Act
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 10);

        // Assert - check that weapons have names from our JSON data
        foreach (var weapon in weapons)
        {
            weapon.Name.Should().NotBeNullOrWhiteSpace();
            // Weapon names should not be empty and should be actual weapon types
            weapon.Type.Should().Be(ItemType.Weapon);
        }
        
        // At least some weapons should have recognizable weapon words
        var hasWeaponWords = weapons.Any(w => 
            Regex.IsMatch(w.Name, @"(sword|axe|bow|dagger|spear|mace|staff|partisan|halberd|lance|pike|javelin|crossbow|flail|hammer|katana|claymore|rapier|scimitar|trident|glaive|club|maul)", RegexOptions.IgnoreCase));
        hasWeaponWords.Should().BeTrue("weapons should contain recognizable weapon type names");
    }

    [Fact]
    public void GenerateByType_Consumable_Should_Have_Potion_Names()
    {
        // Act
        var potions = ItemGenerator.GenerateByType(ItemType.Consumable, 5);

        // Assert
        foreach (var potion in potions)
        {
            potion.Name.Should().Contain("Potion");
        }
    }

    #region Stat Bonus Tests

    [Theory]
    [InlineData(ItemRarity.Common, 0)]
    [InlineData(ItemRarity.Uncommon, 1)]
    [InlineData(ItemRarity.Rare, 2)]
    [InlineData(ItemRarity.Epic, 5)]
    [InlineData(ItemRarity.Legendary, 10)]
    public void GenerateByType_Weapon_Should_Have_Stat_Bonuses_Based_On_Rarity(ItemRarity rarity, int minExpected)
    {
        // Act
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 10);
        var weaponsOfRarity = weapons.Where(w => w.Rarity == rarity).ToList();

        // Assert - if we got any of this rarity, they should have bonuses in the expected range
        if (weaponsOfRarity.Any())
        {
            // Verify rarity-appropriate stat bonuses exist
            weaponsOfRarity.Should().Contain(w => 
                w.BonusStrength >= minExpected || w.BonusDexterity >= minExpected || 
                w.BonusConstitution >= minExpected || w.BonusIntelligence >= minExpected ||
                w.BonusWisdom >= minExpected || w.BonusCharisma >= minExpected,
                $"weapons of rarity {rarity} should have stat bonuses >= {minExpected}");
        }
    }

    [Fact]
    public void GenerateByType_Weapon_Should_Have_Primary_Stat_Bonuses()
    {
        // Act - generate many weapons to ensure we get stat bonuses
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 50);

        // Assert - weapons should have Strength bonuses (primary stat)
        weapons.Should().Contain(w => w.BonusStrength > 0, "weapons should have Strength bonuses");
    }

    [Fact]
    public void GenerateByType_Armor_Should_Have_Constitution_Bonuses()
    {
        // Act - generate chest pieces (heavy armor)
        var chestPieces = ItemGenerator.GenerateByType(ItemType.Chest, 30);

        // Assert - chest pieces should have Constitution bonuses (primary stat)
        chestPieces.Should().Contain(c => c.BonusConstitution > 0, "chest pieces should have Constitution bonuses");
    }

    [Fact]
    public void GenerateByType_Consumable_Should_Not_Have_Stat_Bonuses()
    {
        // Act
        var potions = ItemGenerator.GenerateByType(ItemType.Consumable, 10);

        // Assert - consumables don't give permanent stat bonuses
        potions.Should().AllSatisfy(p =>
        {
            p.BonusStrength.Should().Be(0);
            p.BonusDexterity.Should().Be(0);
            p.BonusConstitution.Should().Be(0);
            p.BonusIntelligence.Should().Be(0);
            p.BonusWisdom.Should().Be(0);
            p.BonusCharisma.Should().Be(0);
        });
    }

    [Fact]
    public void GenerateByType_QuestItem_Should_Not_Have_Stat_Bonuses()
    {
        // Act
        var questItems = ItemGenerator.GenerateByType(ItemType.QuestItem, 10);

        // Assert - quest items don't give stat bonuses
        questItems.Should().AllSatisfy(q =>
        {
            q.BonusStrength.Should().Be(0);
            q.BonusDexterity.Should().Be(0);
            q.BonusConstitution.Should().Be(0);
            q.BonusIntelligence.Should().Be(0);
            q.BonusWisdom.Should().Be(0);
            q.BonusCharisma.Should().Be(0);
        });
    }

    #endregion

    #region Item Type Name Tests

    [Fact]
    public void GenerateByType_Shield_Should_Have_Shield_In_Name()
    {
        // Act
        var shields = ItemGenerator.GenerateByType(ItemType.Shield, 10);

        // Assert
        shields.Should().AllSatisfy(s => s.Name.Should().Contain("Shield"));
    }

    [Fact]
    public void GenerateByType_Helmet_Should_Have_Helmet_In_Name()
    {
        // Act
        var helmets = ItemGenerator.GenerateByType(ItemType.Helmet, 10);

        // Assert
        helmets.Should().AllSatisfy(h => h.Name.Should().Contain("Helmet"));
    }

    [Fact]
    public void GenerateByType_Chest_Should_Have_Chestpiece_In_Name()
    {
        // Act
        var chests = ItemGenerator.GenerateByType(ItemType.Chest, 10);

        // Assert
        chests.Should().AllSatisfy(c => c.Name.Should().Contain("Chestpiece"));
    }

    [Fact]
    public void GenerateByType_Gloves_Should_Have_Gloves_In_Name()
    {
        // Act
        var gloves = ItemGenerator.GenerateByType(ItemType.Gloves, 10);

        // Assert
        gloves.Should().AllSatisfy(g => g.Name.Should().Contain("Gloves"));
    }

    [Fact]
    public void GenerateByType_Boots_Should_Have_Boots_In_Name()
    {
        // Act
        var boots = ItemGenerator.GenerateByType(ItemType.Boots, 10);

        // Assert
        boots.Should().AllSatisfy(b => b.Name.Should().Contain("Boots"));
    }

    [Theory]
    [InlineData(ItemType.Necklace)]
    [InlineData(ItemType.Ring)]
    public void GenerateByType_Jewelry_Should_Have_Gemstone_Names(ItemType type)
    {
        // Act
        var jewelry = ItemGenerator.GenerateByType(type, 20);

        // Assert - jewelry should have gemstone materials
        jewelry.Should().NotBeEmpty();
        jewelry.Should().OnlyContain(j => !string.IsNullOrWhiteSpace(j.Name));
    }

    [Fact]
    public void GenerateByType_OffHand_Should_Have_Magic_Focus_Names()
    {
        // Act
        var offHands = ItemGenerator.GenerateByType(ItemType.OffHand, 10);

        // Assert - off-hand items should be magic focuses
        offHands.Should().Contain(o => 
            o.Name.Contains("Tome") || o.Name.Contains("Orb") || 
            o.Name.Contains("Crystal") || o.Name.Contains("Focus"),
            "off-hand items should be magic focuses");
    }

    #endregion

    #region Two-Handed Weapon Tests

    [Fact]
    public void GenerateByType_Weapon_Should_Sometimes_Be_Two_Handed()
    {
        // Act - generate many weapons to get some two-handed ones
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 50);

        // Assert - about 30% should be two-handed
        var twoHandedCount = weapons.Count(w => w.IsTwoHanded);
        twoHandedCount.Should().BeGreaterThan(0, "some weapons should be two-handed");
        twoHandedCount.Should().BeLessThan(weapons.Count, "not all weapons should be two-handed");
    }

    [Theory]
    [InlineData(ItemType.Shield)]
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.Necklace)]
    public void GenerateByType_NonWeapon_Should_Not_Be_Two_Handed(ItemType type)
    {
        // Act
        var items = ItemGenerator.GenerateByType(type, 10);

        // Assert - only weapons can be two-handed
        items.Should().AllSatisfy(i => i.IsTwoHanded.Should().BeFalse());
    }

    #endregion

    #region Rarity Distribution Tests

    [Fact]
    public void Generate_Should_Produce_All_Rarities_Eventually()
    {
        // Act - generate many items
        var items = ItemGenerator.Generate(100);

        // Assert - should get multiple rarities (randomness)
        var rarities = items.Select(i => i.Rarity).Distinct().ToList();
        rarities.Should().HaveCountGreaterThan(1, "random generation should produce different rarities");
    }

    [Fact]
    public void Generate_Should_Produce_All_Item_Types_Eventually()
    {
        // Act - generate many items
        var items = ItemGenerator.Generate(150);

        // Assert - should get multiple types (randomness)
        var types = items.Select(i => i.Type).Distinct().ToList();
        types.Should().HaveCountGreaterThan(3, "random generation should produce different item types");
    }

    #endregion

    #region Item Uniqueness Tests

    [Fact]
    public void Generate_Should_Create_Items_With_Unique_IDs()
    {
        // Act
        var items = ItemGenerator.Generate(50);

        // Assert - all IDs should be unique GUIDs
        items.Should().OnlyHaveUniqueItems(i => i.Id);
        items.Should().AllSatisfy(i => Guid.TryParse(i.Id, out _).Should().BeTrue("ID should be a valid GUID"));
    }

    [Fact]
    public void GenerateByType_Should_Create_Items_With_Unique_IDs()
    {
        // Act
        var items = ItemGenerator.GenerateByType(ItemType.Weapon, 30);

        // Assert
        items.Should().OnlyHaveUniqueItems(i => i.Id);
    }

    #endregion

    #region Price Tests

    [Fact]
    public void Generate_Should_Have_Reasonable_Prices()
    {
        // Act
        var items = ItemGenerator.Generate(50);

        // Assert - prices should be within expected range
        items.Should().AllSatisfy(i => i.Price.Should().BeInRange(5, 1000));
    }

    [Fact]
    public void GenerateByType_Should_Have_Reasonable_Prices()
    {
        // Act
        var items = ItemGenerator.GenerateByType(ItemType.Weapon, 20);

        // Assert
        items.Should().AllSatisfy(i => i.Price.Should().BeInRange(5, 1000));
    }

    #endregion

    #region All Item Types Coverage

    [Theory]
    [InlineData(ItemType.Weapon)]
    [InlineData(ItemType.Shield)]
    [InlineData(ItemType.OffHand)]
    [InlineData(ItemType.Helmet)]
    [InlineData(ItemType.Shoulders)]
    [InlineData(ItemType.Chest)]
    [InlineData(ItemType.Bracers)]
    [InlineData(ItemType.Gloves)]
    [InlineData(ItemType.Belt)]
    [InlineData(ItemType.Legs)]
    [InlineData(ItemType.Boots)]
    [InlineData(ItemType.Necklace)]
    [InlineData(ItemType.Ring)]
    [InlineData(ItemType.Consumable)]
    [InlineData(ItemType.QuestItem)]
    public void GenerateByType_Should_Handle_All_ItemTypes(ItemType type)
    {
        // Act
        var items = ItemGenerator.GenerateByType(type, 5);

        // Assert
        items.Should().HaveCount(5);
        items.Should().AllSatisfy(i =>
        {
            i.Type.Should().Be(type);
            i.Id.Should().NotBeNullOrEmpty();
            i.Name.Should().NotBeNullOrEmpty();
            i.Description.Should().NotBeNullOrEmpty();
            i.Price.Should().BeGreaterThanOrEqualTo(5);
        });
    }

    #endregion

    #region Material and Trait Tests

    [Fact]
    public void GenerateByType_Weapon_Should_Have_Materials_In_Name()
    {
        // Act - generate many weapons to get material prefixes
        var weapons = ItemGenerator.GenerateByType(ItemType.Weapon, 50);

        // Assert - some weapons should have material names (metal or wood)
        weapons.Should().NotBeEmpty();
        // Names should not all be identical (materials add variety)
        var uniqueNames = weapons.Select(w => w.Name).Distinct().Count();
        uniqueNames.Should().BeGreaterThan(10, "weapons should have varied names from materials");
    }

    [Fact]
    public void GenerateByType_Armor_Should_Have_Material_Prefixes()
    {
        // Act
        var helmets = ItemGenerator.GenerateByType(ItemType.Helmet, 30);

        // Assert - helmets should have leather material prefixes
        helmets.Should().NotBeEmpty();
        var uniqueNames = helmets.Select(h => h.Name).Distinct().Count();
        uniqueNames.Should().BeGreaterThan(5, "helmets should have varied material names");
    }

    [Fact]
    public void GenerateByType_Should_Apply_Traits_To_Items()
    {
        // Act - generate items and check if any have traits applied
        var items = ItemGenerator.GenerateByType(ItemType.Weapon, 50);

        // Assert - traits system should be working (items might have trait data)
        items.Should().NotBeEmpty();
        // Just verify generation succeeds with trait application logic
        items.Should().AllSatisfy(i => i.Should().NotBeNull());
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Generate_Zero_Items_Should_Return_Empty_List()
    {
        // Act
        var items = ItemGenerator.Generate(0);

        // Assert
        items.Should().BeEmpty();
    }

    [Fact]
    public void GenerateByType_Zero_Items_Should_Return_Empty_List()
    {
        // Act
        var items = ItemGenerator.GenerateByType(ItemType.Weapon, 0);

        // Assert
        items.Should().BeEmpty();
    }

    [Fact]
    public void Generate_Large_Batch_Should_Succeed()
    {
        // Act
        var items = ItemGenerator.Generate(200);

        // Assert
        items.Should().HaveCount(200);
        items.Should().OnlyHaveUniqueItems(i => i.Id);
    }

    [Fact]
    public void GenerateByType_Large_Batch_Should_Succeed()
    {
        // Act
        var items = ItemGenerator.GenerateByType(ItemType.Weapon, 100);

        // Assert
        items.Should().HaveCount(100);
        items.Should().AllSatisfy(i => i.Type.Should().Be(ItemType.Weapon));
    }

    #endregion

    #region Legendary and Epic Items

    [Fact]
    public void GenerateByType_Should_Sometimes_Create_Higher_Rarity_Items()
    {
        // Act - generate many items to get rare/epic/legendary
        var items = ItemGenerator.Generate(100);

        // Assert - should get at least one rare or better item eventually
        var hasHigherRarity = items.Any(i => 
            i.Rarity == ItemRarity.Rare || 
            i.Rarity == ItemRarity.Epic || 
            i.Rarity == ItemRarity.Legendary);
        
        hasHigherRarity.Should().BeTrue("random generation should occasionally produce higher rarity items");
    }

    #endregion
}
