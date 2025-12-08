using FluentAssertions;
using Game.Models;
using Game.Services;

namespace Game.Tests.Services;

public class CombatServiceTests
{
    private readonly CombatService _combatService;
    
    public CombatServiceTests()
    {
        // Create a SaveGameService with in-memory database for testing
        var saveGameService = new SaveGameService(":memory:");
        
        // Create a test save game with normal difficulty
        var testSave = new SaveGame 
        { 
            DifficultyLevel = "Normal",
            IronmanMode = false,
            PlayerName = "TestPlayer",
            Character = CreateTestPlayer()
        };
        saveGameService.CreateNewGame(testSave.Character, "Normal", false);
        
        // Create CombatService instance with SaveGameService
        _combatService = new CombatService(saveGameService);
    }
    
    [Fact]
    public void ExecutePlayerAttack_Should_Deal_Damage_To_Enemy()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();
        int initialHealth = enemy.Health;
        
        // Act
        var result = _combatService.ExecutePlayerAttack(player, enemy);
        
        // Assert
        result.Success.Should().BeTrue();
        enemy.Health.Should().BeLessThan(initialHealth);
        result.Damage.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void ExecutePlayerAttack_Should_Not_Damage_When_Dodged()
    {
        // Arrange - Create enemy with impossibly high DEX for guaranteed dodge
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();
        enemy.Dexterity = 500; // Very high dodge chance
        
        // Act - Try multiple times since dodge is probabilistic
        bool foundDodge = false;
        for (int i = 0; i < 100; i++)
        {
            enemy.Health = enemy.MaxHealth; // Reset health
            var result = _combatService.ExecutePlayerAttack(player, enemy);
            if (result.IsDodged)
            {
                foundDodge = true;
                result.Damage.Should().Be(0);
                result.Message.Should().Contain("dodged");
                break;
            }
        }
        
        // Assert
        foundDodge.Should().BeTrue("enemy with high DEX should dodge at least once in 100 attempts");
    }
    
    [Fact]
    public void ExecutePlayerAttack_Should_Deal_Double_Damage_On_Critical()
    {
        // Arrange - Create player with high DEX for critical chance
        var player = CreateTestPlayer();
        player.Dexterity = 100; // High crit chance
        var enemy = CreateTestEnemy();
        enemy.Dexterity = 1; // Low dodge chance
        
        // Act - Try multiple times since crit is probabilistic
        bool foundCritical = false;
        for (int i = 0; i < 100; i++)
        {
            var result = _combatService.ExecutePlayerAttack(player, enemy);
            if (result.IsCritical)
            {
                foundCritical = true;
                result.Message.Should().Contain("CRITICAL");
                result.Damage.Should().BeGreaterThan(0);
                break;
            }
        }
        
        // Assert
        foundCritical.Should().BeTrue("player with high DEX should crit at least once in 100 attempts");
    }
    
    [Fact]
    public void ExecutePlayerAttack_Should_Apply_Defense_Reduction()
    {
        // Arrange
        var player = CreateTestPlayer();
        var weakEnemy = CreateTestEnemy();
        weakEnemy.Constitution = 5; // Low defense
        
        var strongEnemy = CreateTestEnemy();
        strongEnemy.Constitution = 20; // High defense
        
        // Act
        var weakResult = _combatService.ExecutePlayerAttack(player, weakEnemy);
        var strongResult = _combatService.ExecutePlayerAttack(player, strongEnemy);
        
        // Assert - Weak enemy should take more damage due to lower defense
        weakResult.Damage.Should().BeGreaterThan(strongResult.Damage);
    }
    
    [Fact]
    public void ExecuteEnemyAttack_Should_Deal_Damage_To_Player()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Dexterity = 1; // Low dodge chance to ensure hit
        var enemy = CreateTestEnemy();
        enemy.BasePhysicalDamage = 20; // Ensure some damage
        
        // Act - Try multiple times since there's still a small chance of dodge
        bool dealtDamage = false;
        for (int i = 0; i < 10; i++)
        {
            player.Health = player.MaxHealth; // Reset health
            var result = _combatService.ExecuteEnemyAttack(enemy, player);
            
            if (!result.IsDodged && player.Health < player.MaxHealth)
            {
                dealtDamage = true;
                result.Success.Should().BeTrue();
                result.Damage.Should().BeGreaterThan(0);
                break;
            }
        }
        
        // Assert
        dealtDamage.Should().BeTrue("enemy should deal damage in at least one of 10 attempts");
    }
    
    [Fact]
    public void ExecuteEnemyAttack_Should_Reduce_Damage_When_Defending()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();
        
        // Set health to max for consistent comparison
        player.Health = player.MaxHealth;
        int healthBefore = player.Health;
        var normalResult = _combatService.ExecuteEnemyAttack(enemy, player);
        int normalDamage = normalResult.Damage;
        
        // Reset and defend
        player.Health = healthBefore;
        var defendResult = _combatService.ExecuteEnemyAttack(enemy, player, isDefending: true);
        
        // Assert - Defending should reduce damage (if not blocked/dodged)
        if (!defendResult.IsBlocked && !defendResult.IsDodged)
        {
            defendResult.Damage.Should().BeLessThan(normalDamage);
        }
    }
    
    [Fact]
    public void ExecuteEnemyAttack_Should_Block_When_Defending()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Dexterity = 1; // Low dodge to isolate block mechanic
        var enemy = CreateTestEnemy();
        
        // Act - Try multiple times since block is 50% when defending
        bool foundBlock = false;
        for (int i = 0; i < 100; i++)
        {
            var result = _combatService.ExecuteEnemyAttack(enemy, player, isDefending: true);
            if (result.IsBlocked)
            {
                foundBlock = true;
                result.Damage.Should().Be(0);
                result.Message.Should().Contain("blocked");
                break;
            }
        }
        
        // Assert
        foundBlock.Should().BeTrue("defending should block at least once in 100 attempts");
    }
    
    [Fact]
    public void AttemptFlee_Should_Have_Higher_Success_With_High_DEX()
    {
        // Arrange
        var fastPlayer = CreateTestPlayer();
        fastPlayer.Dexterity = 20; // High DEX
        
        var slowPlayer = CreateTestPlayer();
        slowPlayer.Dexterity = 5; // Low DEX
        
        var enemy = CreateTestEnemy();
        enemy.Dexterity = 10;
        
        // Act - Try multiple times and count successes
        int fastSuccesses = 0;
        int slowSuccesses = 0;
        
        for (int i = 0; i < 100; i++)
        {
            if (_combatService.AttemptFlee(fastPlayer, enemy).Success)
                fastSuccesses++;
            if (_combatService.AttemptFlee(slowPlayer, enemy).Success)
                slowSuccesses++;
        }
        
        // Assert - Fast player should flee more often
        fastSuccesses.Should().BeGreaterThan(slowSuccesses);
    }
    
    [Fact]
    public void UseItemInCombat_Should_Heal_Player_With_Health_Potion()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Health = 10; // Low health
        
        var healthPotion = new Item 
        { 
            Name = "Health Potion", 
            Type = ItemType.Consumable 
        };
        player.Inventory.Add(healthPotion);
        
        int initialHealth = player.Health;
        
        // Act
        var result = _combatService.UseItemInCombat(player, healthPotion);
        
        // Assert
        result.Success.Should().BeTrue();
        result.Healing.Should().BeGreaterThan(0);
        player.Health.Should().BeGreaterThan(initialHealth);
        player.Inventory.Should().NotContain(healthPotion);
    }
    
    [Fact]
    public void UseItemInCombat_Should_Fail_For_Non_Consumables()
    {
        // Arrange
        var player = CreateTestPlayer();
        var sword = new Item 
        { 
            Name = "Sword", 
            Type = ItemType.Weapon 
        };
        
        // Act
        var result = _combatService.UseItemInCombat(player, sword);
        
        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("cannot be used");
    }
    
    [Fact]
    public void GenerateVictoryOutcome_Should_Award_XP_And_Gold()
    {
        // Arrange
        var player = CreateTestPlayer();
        var enemy = CreateTestEnemy();
        enemy.XPReward = 100;
        enemy.GoldReward = 50;
        
        // Act
        var outcome = _combatService.GenerateVictoryOutcome(player, enemy);
        
        // Assert
        outcome.PlayerVictory.Should().BeTrue();
        outcome.XPGained.Should().Be(100);
        outcome.GoldGained.Should().Be(50);
        outcome.Summary.Should().Contain("Victory");
        outcome.Summary.Should().Contain("100 XP");
        outcome.Summary.Should().Contain("50 Gold");
    }
    
    [Fact]
    public void GenerateVictoryOutcome_Should_Drop_Better_Loot_For_Boss()
    {
        // Arrange
        var player = CreateTestPlayer();
        var boss = CreateTestEnemy();
        boss.Difficulty = EnemyDifficulty.Boss;
        
        // Act - Try multiple times since loot is probabilistic
        bool foundLoot = false;
        for (int i = 0; i < 10; i++) // Boss has 100% drop chance
        {
            var outcome = _combatService.GenerateVictoryOutcome(player, boss);
            if (outcome.LootDropped.Any())
            {
                foundLoot = true;
                outcome.Summary.Should().Contain("Loot");
                break;
            }
        }
        
        // Assert
        foundLoot.Should().BeTrue("boss should drop loot with 100% chance");
    }
    
    [Fact]
    public void GenerateVictoryOutcome_Should_Include_Loot_In_Summary()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Charisma = 20; // High rare find chance
        var enemy = CreateTestEnemy();
        enemy.Difficulty = EnemyDifficulty.Boss; // 100% loot chance
        
        // Act
        var outcome = _combatService.GenerateVictoryOutcome(player, enemy);
        
        // Assert
        if (outcome.LootDropped.Any())
        {
            outcome.Summary.Should().Contain("Loot");
            foreach (var item in outcome.LootDropped)
            {
                outcome.Summary.Should().Contain(item.Name);
            }
        }
    }
    
    [Fact]
    public void Combat_Should_Not_Reduce_Health_Below_Zero()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Health = 1;
        
        var enemy = CreateTestEnemy();
        enemy.BasePhysicalDamage = 1000; // Overkill damage
        
        // Act
        _combatService.ExecuteEnemyAttack(enemy, player);
        
        // Assert
        player.Health.Should().Be(0);
        player.Health.Should().BeGreaterThanOrEqualTo(0);
    }
    
    [Fact]
    public void Combat_Should_Always_Deal_Minimum_Damage()
    {
        // Arrange
        var player = CreateTestPlayer();
        player.Strength = 1; // Very low damage
        
        var enemy = CreateTestEnemy();
        enemy.Constitution = 100; // Very high defense
        
        // Act
        var result = _combatService.ExecutePlayerAttack(player, enemy);
        
        // Assert - Should always deal at least 1 damage
        if (!result.IsDodged)
        {
            result.Damage.Should().BeGreaterThanOrEqualTo(1);
        }
    }
    
    // Helper methods
    private static Character CreateTestPlayer()
    {
        return new Character
        {
            Name = "Test Hero",
            Level = 5,
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            Strength = 12,
            Dexterity = 10,
            Constitution = 11,
            Intelligence = 10,
            Wisdom = 10,
            Charisma = 10,
            EquippedMainHand = new Item 
            { 
                Name = "Sword", 
                Type = ItemType.Weapon, 
                Rarity = ItemRarity.Common 
            }
        };
    }
    
    private static Enemy CreateTestEnemy()
    {
        return new Enemy
        {
            Name = "Test Goblin",
            Level = 5,
            MaxHealth = 50,
            Health = 50,
            BasePhysicalDamage = 8,
            BaseMagicDamage = 2,
            Strength = 8,
            Dexterity = 10,
            Constitution = 8,
            Intelligence = 6,
            Wisdom = 6,
            Charisma = 4,
            Type = EnemyType.Common,
            Difficulty = EnemyDifficulty.Normal,
            XPReward = 50,
            GoldReward = 25
        };
    }
}
