using FluentAssertions;
using Game.Generators;
using Game.Models;
using Xunit;

namespace Game.Tests.Generators;

/// <summary>
/// Tests for EnemyGenerator.
/// </summary>
public class EnemyGeneratorTests
{
    #region Generate Tests

    [Fact]
    public void Generate_Should_Create_Valid_Enemy()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 5);

        // Assert
        enemy.Should().NotBeNull();
        enemy.Name.Should().NotBeEmpty();
        enemy.Description.Should().NotBeEmpty();
        enemy.Level.Should().BeGreaterThan(0);
        enemy.Health.Should().BeGreaterThan(0);
        enemy.MaxHealth.Should().BeGreaterThan(0);
        enemy.Health.Should().Be(enemy.MaxHealth);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public void Generate_Should_Scale_Enemy_To_Player_Level(int playerLevel)
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel);

        // Assert
        enemy.Level.Should().BeGreaterThan(0);
        enemy.Level.Should().BeCloseTo(playerLevel, (uint)(playerLevel / 2 + 1));
    }

    [Fact]
    public void Generate_Should_Create_Normal_Difficulty_By_Default()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 5);

        // Assert
        enemy.Difficulty.Should().Be(EnemyDifficulty.Normal);
    }

    [Theory]
    [InlineData(EnemyDifficulty.Easy)]
    [InlineData(EnemyDifficulty.Normal)]
    [InlineData(EnemyDifficulty.Hard)]
    [InlineData(EnemyDifficulty.Elite)]
    public void Generate_Should_Support_Different_Difficulties(EnemyDifficulty difficulty)
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 5, difficulty);

        // Assert
        enemy.Difficulty.Should().Be(difficulty);
    }

    [Fact]
    public void Generate_Should_Create_Different_Enemies_On_Multiple_Calls()
    {
        // Act
        var enemy1 = EnemyGenerator.Generate(playerLevel: 5);
        var enemy2 = EnemyGenerator.Generate(playerLevel: 5);
        var enemy3 = EnemyGenerator.Generate(playerLevel: 5);

        // Assert - At least one should be different (with high probability)
        var allSame = enemy1.Name == enemy2.Name && enemy2.Name == enemy3.Name;
        allSame.Should().BeFalse("randomness should produce variety");
    }

    #endregion

    #region GenerateByType Tests

    [Theory]
    [InlineData(EnemyType.Beast)]
    [InlineData(EnemyType.Humanoid)]
    [InlineData(EnemyType.Undead)]
    [InlineData(EnemyType.Demon)]
    [InlineData(EnemyType.Dragon)]
    [InlineData(EnemyType.Elemental)]
    [InlineData(EnemyType.Common)]
    public void GenerateByType_Should_Create_Enemy_Of_Specified_Type(EnemyType type)
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(type, playerLevel: 5);

        // Assert
        enemy.Type.Should().Be(type);
        enemy.Name.Should().NotBeEmpty();
        enemy.Description.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateByType_Should_Create_Beast_With_Beast_Characteristics()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Beast, playerLevel: 5);

        // Assert
        enemy.Type.Should().Be(EnemyType.Beast);
        enemy.Name.Should().NotBeEmpty();
        enemy.Strength.Should().BeGreaterThan(0);
        enemy.Constitution.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateByType_Should_Create_Dragon_With_Dragon_Traits()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Dragon, playerLevel: 10);

        // Assert
        enemy.Type.Should().Be(EnemyType.Dragon);
        enemy.Name.Should().NotBeEmpty();
        enemy.Description.Should().NotBeEmpty();
        enemy.Traits.Should().NotBeEmpty("dragons should have traits from dragon color");
    }

    [Fact]
    public void GenerateByType_Should_Create_Undead_With_Undead_Properties()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Undead, playerLevel: 5);

        // Assert
        enemy.Type.Should().Be(EnemyType.Undead);
        enemy.Name.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateByType_Should_Create_Demon_With_High_Attributes()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Demon, playerLevel: 10);

        // Assert
        enemy.Type.Should().Be(EnemyType.Demon);
        enemy.Strength.Should().BeGreaterThan(5);
        enemy.Intelligence.Should().BeGreaterThan(5);
    }

    [Fact]
    public void GenerateByType_Should_Apply_Difficulty_Modifiers()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var easyEnemy = EnemyGenerator.GenerateByType(EnemyType.Beast, playerLevel, EnemyDifficulty.Easy);
        var eliteEnemy = EnemyGenerator.GenerateByType(EnemyType.Beast, playerLevel, EnemyDifficulty.Elite);

        // Assert
        eliteEnemy.Level.Should().BeGreaterThan(easyEnemy.Level);
        eliteEnemy.XPReward.Should().BeGreaterThan(easyEnemy.XPReward);
        eliteEnemy.GoldReward.Should().BeGreaterThan(easyEnemy.GoldReward);
    }

    [Fact]
    public void GenerateByType_Should_Set_Health_Based_On_Constitution_And_Level()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Humanoid, playerLevel: 5);

        // Assert
        enemy.MaxHealth.Should().BeGreaterThan(0);
        enemy.Health.Should().Be(enemy.MaxHealth);
        
        // Health formula: (Constitution * 8) + (Level * 5)
        var expectedHealth = (enemy.Constitution * 8) + (enemy.Level * 5);
        enemy.MaxHealth.Should().Be(expectedHealth);
    }

    [Fact]
    public void GenerateByType_Should_Calculate_XP_And_Gold_Rewards()
    {
        // Act
        var enemy = EnemyGenerator.GenerateByType(EnemyType.Beast, playerLevel: 10);

        // Assert
        enemy.XPReward.Should().BeGreaterThan(0);
        enemy.GoldReward.Should().BeGreaterThan(0);
    }

    #endregion

    #region GenerateBoss Tests

    [Fact]
    public void GenerateBoss_Should_Create_Boss_Type_Enemy()
    {
        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel: 10);

        // Assert
        boss.Type.Should().Be(EnemyType.Boss);
        boss.Difficulty.Should().Be(EnemyDifficulty.Boss);
    }

    [Fact]
    public void GenerateBoss_Should_Have_Higher_Level_Than_Player()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel);

        // Assert
        boss.Level.Should().BeGreaterThan(playerLevel);
        // Formula: (playerLevel * 1.5) + 2
        var expectedLevel = (int)(playerLevel * 1.5) + 2;
        boss.Level.Should().Be(expectedLevel);
    }

    [Fact]
    public void GenerateBoss_Should_Have_High_Attributes()
    {
        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel: 10);

        // Assert
        boss.Strength.Should().BeGreaterThan(15);
        boss.Constitution.Should().BeGreaterThan(15);
        boss.Dexterity.Should().BeGreaterThan(10);
        boss.Intelligence.Should().BeGreaterThan(10);
        boss.Wisdom.Should().BeGreaterThan(10);
        boss.Charisma.Should().BeGreaterThan(10);
    }

    [Fact]
    public void GenerateBoss_Should_Have_Much_Higher_Health_Than_Normal_Enemy()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var normalEnemy = EnemyGenerator.Generate(playerLevel);
        var boss = EnemyGenerator.GenerateBoss(playerLevel);

        // Assert
        boss.MaxHealth.Should().BeGreaterThan(normalEnemy.MaxHealth * 2);
    }

    [Fact]
    public void GenerateBoss_Should_Have_Higher_Damage_Than_Normal_Enemy()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var normalEnemy = EnemyGenerator.Generate(playerLevel);
        var boss = EnemyGenerator.GenerateBoss(playerLevel);

        // Assert
        boss.BasePhysicalDamage.Should().BeGreaterThan(normalEnemy.BasePhysicalDamage);
    }

    [Fact]
    public void GenerateBoss_Should_Have_Much_Higher_Rewards_Than_Normal_Enemy()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var normalEnemy = EnemyGenerator.Generate(playerLevel);
        var boss = EnemyGenerator.GenerateBoss(playerLevel);

        // Assert
        boss.XPReward.Should().BeGreaterThan(normalEnemy.XPReward * 3);
        boss.GoldReward.Should().BeGreaterThan(normalEnemy.GoldReward * 3);
    }

    [Fact]
    public void GenerateBoss_Should_Have_Unique_Name()
    {
        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel: 10);

        // Assert
        boss.Name.Should().NotBeEmpty();
        boss.Name.Should().MatchRegex(@"(Lord|King|Queen|Ancient|Elder|Dark) \w+");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public void GenerateBoss_Should_Scale_With_Player_Level(int playerLevel)
    {
        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel);

        // Assert
        boss.Level.Should().BeGreaterThan(playerLevel);
        boss.MaxHealth.Should().BeGreaterThan(100);
        boss.XPReward.Should().BeGreaterThan(0);
        boss.GoldReward.Should().BeGreaterThan(0);
    }

    #endregion

    #region Enemy Attributes Tests

    [Fact]
    public void Generated_Enemy_Should_Have_All_D20_Attributes()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 5);

        // Assert
        enemy.Strength.Should().BeGreaterThan(0);
        enemy.Dexterity.Should().BeGreaterThan(0);
        enemy.Constitution.Should().BeGreaterThan(0);
        enemy.Intelligence.Should().BeGreaterThan(0);
        enemy.Wisdom.Should().BeGreaterThan(0);
        enemy.Charisma.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generated_Enemy_Should_Have_Damage_Values()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 5);

        // Assert
        enemy.BasePhysicalDamage.Should().BeGreaterThanOrEqualTo(0);
        enemy.BaseMagicDamage.Should().BeGreaterThanOrEqualTo(0);
        (enemy.BasePhysicalDamage + enemy.BaseMagicDamage).Should().BeGreaterThan(0, "enemy should have some damage");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Generate_Should_Handle_Level_1_Player()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 1);

        // Assert
        enemy.Should().NotBeNull();
        enemy.Level.Should().BeGreaterThan(0);
        enemy.Health.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generate_Should_Handle_High_Level_Player()
    {
        // Act
        var enemy = EnemyGenerator.Generate(playerLevel: 100);

        // Assert
        enemy.Should().NotBeNull();
        enemy.Level.Should().BeGreaterThan(50);
        enemy.Health.Should().BeGreaterThan(500);
    }

    [Fact]
    public void GenerateBoss_Should_Handle_Level_1_Player()
    {
        // Act
        var boss = EnemyGenerator.GenerateBoss(playerLevel: 1);

        // Assert
        boss.Should().NotBeNull();
        boss.Type.Should().Be(EnemyType.Boss);
        boss.Level.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Generated_Enemies_Should_Have_Consistent_Health_Formula()
    {
        // Act
        var enemies = Enumerable.Range(1, 10)
            .Select(i => EnemyGenerator.Generate(playerLevel: 5))
            .ToList();

        // Assert - All enemies should follow health formula
        foreach (var enemy in enemies)
        {
            var expectedHealth = (enemy.Constitution * 8) + (enemy.Level * 5);
            enemy.MaxHealth.Should().Be(expectedHealth,
                $"enemy {enemy.Name} should have health matching formula");
        }
    }

    #endregion

    #region Difficulty Scaling Tests

    [Fact]
    public void Elite_Enemy_Should_Be_Stronger_Than_Normal_Enemy()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var normalEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Normal);
        var eliteEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Elite);

        // Assert
        eliteEnemy.Level.Should().BeGreaterThan(normalEnemy.Level);
        eliteEnemy.MaxHealth.Should().BeGreaterThan(normalEnemy.MaxHealth);
    }

    [Fact]
    public void Easy_Enemy_Should_Be_Weaker_Than_Normal_Enemy()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var easyEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Easy);
        var normalEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Normal);

        // Assert
        easyEnemy.Level.Should().BeLessThan(normalEnemy.Level);
    }

    [Fact]
    public void Hard_Enemy_Should_Be_Between_Normal_And_Elite()
    {
        // Arrange
        var playerLevel = 10;

        // Act
        var normalEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Normal);
        var hardEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Hard);
        var eliteEnemy = EnemyGenerator.Generate(playerLevel, EnemyDifficulty.Elite);

        // Assert
        hardEnemy.Level.Should().BeGreaterThan(normalEnemy.Level);
        hardEnemy.Level.Should().BeLessThanOrEqualTo(eliteEnemy.Level);
    }

    #endregion

    #region Enemy Type Variety Tests

    [Fact]
    public void Generate_Multiple_Enemies_Should_Include_Various_Types()
    {
        // Act - Generate 50 enemies
        var enemies = Enumerable.Range(1, 50)
            .Select(_ => EnemyGenerator.Generate(playerLevel: 10))
            .ToList();

        // Assert - Should have at least 3 different types
        var uniqueTypes = enemies.Select(e => e.Type).Distinct().Count();
        uniqueTypes.Should().BeGreaterThanOrEqualTo(3, "random generation should produce variety");
    }

    [Fact]
    public void Dragons_Should_Have_Color_Traits()
    {
        // Act
        var dragon = EnemyGenerator.GenerateByType(EnemyType.Dragon, playerLevel: 10);

        // Assert - Dragons should have traits from dragon color data
        dragon.Traits.Should().NotBeEmpty();
        dragon.Traits.Should().ContainKey("breathWeaponType");
    }

    #endregion
}
