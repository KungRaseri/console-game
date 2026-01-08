using Xunit;
using FluentAssertions;
using RealmEngine.Core.Features.Combat.Services;
using RealmEngine.Shared.Models;
using System.Collections.Generic;

namespace RealmEngine.Core.Tests.Features.Combat;

public class EnemyAbilityAIServiceTests
{
    private readonly EnemyAbilityAIService _aiService;

    public EnemyAbilityAIServiceTests()
    {
        _aiService = new EnemyAbilityAIService();
    }

    [Fact]
    public void DecideAbilityUsage_Should_Return_Null_When_Enemy_Has_No_Abilities()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Goblin",
            Health = 50,
            MaxHealth = 50,
            Abilities = new List<Ability>()
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>();

        // Act
        var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DecideAbilityUsage_Should_Return_Null_When_All_Abilities_On_Cooldown()
    {
        // Arrange
        var ability1 = new Ability
        {
            Id = "fireball",
            Name = "Fireball",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 3
        };
        var ability2 = new Ability
        {
            Id = "heal",
            Name = "Heal",
            Type = AbilityTypeEnum.Healing,
            Cooldown = 5
        };

        var enemy = new Enemy
        {
            Name = "Wizard",
            Health = 80,
            MaxHealth = 100,
            Abilities = new List<Ability> { ability1, ability2 }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>
        {
            { "fireball", 2 },  // Still on cooldown
            { "heal", 3 }       // Still on cooldown
        };

        // Act
        var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);

        // Assert
        result.Should().BeNull("all abilities are on cooldown");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Prefer_Defensive_Abilities_When_Low_Health()
    {
        // Arrange
        var healAbility = new Ability
        {
            Id = "emergency-heal",
            Name = "Emergency Heal",
            Description = "Restore health to the caster",
            Type = AbilityTypeEnum.Healing,
            Cooldown = 5
        };
        var attackAbility = new Ability
        {
            Id = "attack",
            Name = "Attack",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 0
        };

        var enemy = new Enemy
        {
            Name = "Cleric",
            Health = 15,  // 15% health
            MaxHealth = 100,
            Abilities = new List<Ability> { healAbility, attackAbility }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>();

        // Act - Run multiple times since it's probabilistic (70% chance)
        var healUsedCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            if (result == "emergency-heal")
            {
                healUsedCount++;
            }
        }

        // Assert - Should use heal ability majority of the time (at least 50% due to high priority)
        healUsedCount.Should().BeGreaterThan(50, "defensive abilities should be preferred when health is low");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Prefer_Offensive_Abilities_When_High_Health()
    {
        // Arrange
        var attackAbility = new Ability
        {
            Id = "fireball",
            Name = "Fireball",
            Description = "Deal massive damage",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 2
        };
        var healAbility = new Ability
        {
            Id = "heal",
            Name = "Heal",
            Type = AbilityTypeEnum.Healing,
            Cooldown = 5
        };

        var enemy = new Enemy
        {
            Name = "Wizard",
            Health = 90,  // 90% health
            MaxHealth = 100,
            Abilities = new List<Ability> { attackAbility, healAbility }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>();

        // Act - Run multiple times since it's probabilistic (40% chance for offensive)
        var attackUsedCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            if (result == "fireball")
            {
                attackUsedCount++;
            }
        }

        // Assert - Should use attack ability at least 20% of the time
        attackUsedCount.Should().BeGreaterThan(20, "offensive abilities should be used when health is high");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Prefer_Buffs_At_Start_Of_Combat()
    {
        // Arrange
        var buffAbility = new Ability
        {
            Id = "strengthen",
            Name = "Strengthen",
            Description = "Increase attack power",
            Type = AbilityTypeEnum.Buff,
            Cooldown = 0
        };
        var attackAbility = new Ability
        {
            Id = "slash",
            Name = "Slash",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 0
        };

        var enemy = new Enemy
        {
            Name = "Warrior",
            Health = 100,  // Full health
            MaxHealth = 100,
            Abilities = new List<Ability> { buffAbility, attackAbility }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>();

        // Act - Run multiple times (50% chance for buffs at full health)
        var buffUsedCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            if (result == "strengthen")
            {
                buffUsedCount++;
            }
        }

        // Assert - Should use buff at least 30% of the time at full health
        buffUsedCount.Should().BeGreaterThan(30, "buff abilities should be preferred at start of combat");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Consider_Debuffs_When_Player_Strong()
    {
        // Arrange
        var debuffAbility = new Ability
        {
            Id = "weaken",
            Name = "Weaken",
            Description = "Reduce enemy defense",
            Type = AbilityTypeEnum.Debuff,
            Cooldown = 3
        };

        var enemy = new Enemy
        {
            Name = "Necromancer",
            Health = 60,
            MaxHealth = 100,
            Abilities = new List<Ability> { debuffAbility }
        };
        var player = new Character 
        { 
            Name = "Hero", 
            Health = 90,  // Strong player (>70%)
            MaxHealth = 100 
        };
        var abilityStates = new Dictionary<string, int>();

        // Act - Run multiple times (45% chance)
        var debuffUsedCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            if (result == "weaken")
            {
                debuffUsedCount++;
            }
        }

        // Assert - Should use debuff at least 25% of the time
        debuffUsedCount.Should().BeGreaterThan(25, "debuff abilities should be considered when player is strong");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Skip_Abilities_On_Cooldown()
    {
        // Arrange
        var onCooldownAbility = new Ability
        {
            Id = "big-spell",
            Name = "Big Spell",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 5
        };
        var availableAbility = new Ability
        {
            Id = "small-spell",
            Name = "Small Spell",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 1
        };

        var enemy = new Enemy
        {
            Name = "Mage",
            Health = 80,
            MaxHealth = 100,
            Abilities = new List<Ability> { onCooldownAbility, availableAbility }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>
        {
            { "big-spell", 3 }  // On cooldown
        };

        // Act - Run multiple times
        var results = new List<string?>();
        for (int i = 0; i < 50; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            results.Add(result);
        }

        // Assert - Should never use the ability on cooldown
        results.Should().NotContain("big-spell", "abilities on cooldown should not be selected");
    }

    [Fact]
    public void DecideAbilityUsage_Should_Sometimes_Use_Basic_Attack()
    {
        // Arrange
        var ability = new Ability
        {
            Id = "weak-spell",
            Name = "Weak Spell",
            Type = AbilityTypeEnum.Offensive,
            Cooldown = 1
        };

        var enemy = new Enemy
        {
            Name = "Apprentice",
            Health = 50,
            MaxHealth = 100,
            Abilities = new List<Ability> { ability }
        };
        var player = new Character { Name = "Hero", Health = 100, MaxHealth = 100 };
        var abilityStates = new Dictionary<string, int>();

        // Act - Run many times to catch the basic attack fallback
        var basicAttackCount = 0;
        for (int i = 0; i < 200; i++)
        {
            var result = _aiService.DecideAbilityUsage(enemy, player, abilityStates);
            if (result == null)
            {
                basicAttackCount++;
            }
        }

        // Assert - Should use basic attack sometimes (not use abilities every time)
        basicAttackCount.Should().BeGreaterThan(0, "AI should sometimes choose basic attack");
    }
}
