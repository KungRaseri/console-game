using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealmEngine.Core.Features.Combat.Commands;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Combat.Commands;

[Trait("Category", "Feature")]
public class ProcessStatusEffectsTests
{
    private readonly Mock<ILogger<ProcessStatusEffectsHandler>> _mockLogger;
    private readonly ProcessStatusEffectsHandler _handler;

    public ProcessStatusEffectsTests()
    {
        _mockLogger = new Mock<ILogger<ProcessStatusEffectsHandler>>();
        _handler = new ProcessStatusEffectsHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Should_Apply_DoT_Damage_To_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Poison",
                    RemainingDuration = 3,
                    TickDamage = 5,
                    DamageType = "poison"
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(5);
        character.Health.Should().Be(95);
        character.ActiveStatusEffects[0].RemainingDuration.Should().Be(2);
    }

    [Fact]
    public async Task Should_Apply_HoT_Healing_To_Character()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 50,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "regen1",
                    Type = StatusEffectType.Regenerating,
                    Category = StatusEffectCategory.HealOverTime,
                    Name = "Regeneration",
                    RemainingDuration = 4,
                    TickHealing = 8
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalHealingReceived.Should().Be(8);
        character.Health.Should().Be(58);
    }

    [Fact]
    public async Task Should_Not_Heal_Above_Max_Health()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 98,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "regen1",
                    Type = StatusEffectType.Regenerating,
                    Category = StatusEffectCategory.HealOverTime,
                    Name = "Regeneration",
                    RemainingDuration = 2,
                    TickHealing = 10
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalHealingReceived.Should().Be(2);
        character.Health.Should().Be(100);
    }

    [Fact]
    public async Task Should_Apply_Stacked_DoT_Damage()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Orc",
            Health = 50,
            MaxHealth = 50,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "burn1",
                    Type = StatusEffectType.Burning,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Burning",
                    RemainingDuration = 2,
                    TickDamage = 3,
                    StackCount = 3,
                    DamageType = "fire"
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetEnemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(9); // 3 damage * 3 stacks
        enemy.Health.Should().Be(41);
    }

    [Fact]
    public async Task Should_Remove_Expired_Effects()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Poison",
                    RemainingDuration = 1,
                    TickDamage = 5
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.EffectsExpired.Should().Be(1);
        result.ExpiredEffectTypes.Should().Contain(StatusEffectType.Poisoned);
        character.ActiveStatusEffects.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Process_Multiple_Effects_Simultaneously()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 80,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Poison",
                    RemainingDuration = 3,
                    TickDamage = 5
                },
                new StatusEffect
                {
                    Id = "burn1",
                    Type = StatusEffectType.Burning,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Burning",
                    RemainingDuration = 2,
                    TickDamage = 3
                },
                new StatusEffect
                {
                    Id = "regen1",
                    Type = StatusEffectType.Regenerating,
                    Category = StatusEffectCategory.HealOverTime,
                    Name = "Regeneration",
                    RemainingDuration = 4,
                    TickHealing = 7
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(8); // 5 + 3
        result.TotalHealingReceived.Should().Be(7);
        character.Health.Should().Be(79); // 80 - 8 + 7
    }

    [Fact]
    public async Task Should_Accumulate_Stat_Modifiers()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "buff1",
                    Type = StatusEffectType.Strengthened,
                    Category = StatusEffectCategory.Buff,
                    Name = "Strength Buff",
                    RemainingDuration = 3,
                    StatModifiers = new Dictionary<string, int> { { "attack", 10 } }
                },
                new StatusEffect
                {
                    Id = "debuff1",
                    Type = StatusEffectType.Weakened,
                    Category = StatusEffectCategory.Debuff,
                    Name = "Weakness",
                    RemainingDuration = 2,
                    StatModifiers = new Dictionary<string, int> { { "attack", -5 } }
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalStatModifiers.Should().ContainKey("attack");
        result.TotalStatModifiers["attack"].Should().Be(5); // 10 - 5
    }

    [Fact]
    public async Task Should_Decrement_Duration_On_All_Effects()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Poison",
                    RemainingDuration = 3,
                    TickDamage = 5
                },
                new StatusEffect
                {
                    Id = "shield1",
                    Type = StatusEffectType.Shielded,
                    Category = StatusEffectCategory.Buff,
                    Name = "Shield",
                    RemainingDuration = 5
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        character.ActiveStatusEffects[0].RemainingDuration.Should().Be(2);
        character.ActiveStatusEffects[1].RemainingDuration.Should().Be(4);
    }

    [Fact]
    public async Task Should_Handle_Enemy_With_No_Effects()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Goblin",
            Health = 30,
            MaxHealth = 30,
            ActiveStatusEffects = new List<StatusEffect>()
        };

        var command = new ProcessStatusEffectsCommand { TargetEnemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(0);
        result.TotalHealingReceived.Should().Be(0);
        result.EffectsExpired.Should().Be(0);
        result.ActiveEffectTypes.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Not_Reduce_Health_Below_Zero()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 5,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Deadly Poison",
                    RemainingDuration = 2,
                    TickDamage = 20
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(20);
        character.Health.Should().Be(0);
    }

    [Fact]
    public async Task Should_Generate_Combat_Log_Messages()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "burn1",
                    Type = StatusEffectType.Burning,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Burning",
                    RemainingDuration = 1,
                    TickDamage = 8
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Messages.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Messages.Should().Contain(m => m.Contains("Burning"));
    }

    [Fact]
    public async Task Should_Track_Active_Effect_Types()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Orc",
            Health = 50,
            MaxHealth = 50,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "poison1",
                    Type = StatusEffectType.Poisoned,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Poison",
                    RemainingDuration = 3,
                    TickDamage = 5
                },
                new StatusEffect
                {
                    Id = "shield1",
                    Type = StatusEffectType.Shielded,
                    Category = StatusEffectCategory.Buff,
                    Name = "Shield",
                    RemainingDuration = 2
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetEnemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ActiveEffectTypes.Should().HaveCount(2);
        result.ActiveEffectTypes.Should().Contain(StatusEffectType.Poisoned);
        result.ActiveEffectTypes.Should().Contain(StatusEffectType.Shielded);
    }

    [Fact]
    public async Task Should_Apply_Multiple_Stacks_Of_Same_Effect()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Dragon",
            Health = 200,
            MaxHealth = 200,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "bleed1",
                    Type = StatusEffectType.Bleeding,
                    Category = StatusEffectCategory.DamageOverTime,
                    Name = "Deep Wound",
                    RemainingDuration = 4,
                    TickDamage = 10,
                    StackCount = 5,
                    DamageType = "physical"
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetEnemy = enemy };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalDamageTaken.Should().Be(50); // 10 * 5 stacks
        enemy.Health.Should().Be(150);
    }

    [Fact]
    public async Task Should_Apply_Stacked_Stat_Modifiers()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "haste1",
                    Type = StatusEffectType.Hasted,
                    Category = StatusEffectCategory.Buff,
                    Name = "Haste",
                    RemainingDuration = 3,
                    StackCount = 2,
                    StatModifiers = new Dictionary<string, int> { { "speed", 15 } }
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalStatModifiers.Should().ContainKey("speed");
        result.TotalStatModifiers["speed"].Should().Be(30); // 15 * 2 stacks
    }

    [Fact]
    public async Task Should_Expire_Multiple_Effects_In_Same_Tick()
    {
        // Arrange
        var character = new Character
        {
            Name = "Hero",
            Health = 100,
            MaxHealth = 100,
            ActiveStatusEffects = new List<StatusEffect>
            {
                new StatusEffect
                {
                    Id = "stun1",
                    Type = StatusEffectType.Stunned,
                    Category = StatusEffectCategory.CrowdControl,
                    Name = "Stunned",
                    RemainingDuration = 1
                },
                new StatusEffect
                {
                    Id = "silence1",
                    Type = StatusEffectType.Silenced,
                    Category = StatusEffectCategory.CrowdControl,
                    Name = "Silenced",
                    RemainingDuration = 1
                },
                new StatusEffect
                {
                    Id = "buff1",
                    Type = StatusEffectType.Blessed,
                    Category = StatusEffectCategory.Buff,
                    Name = "Blessing",
                    RemainingDuration = 3
                }
            }
        };

        var command = new ProcessStatusEffectsCommand { TargetCharacter = character };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.EffectsExpired.Should().Be(2);
        result.ExpiredEffectTypes.Should().Contain(StatusEffectType.Stunned);
        result.ExpiredEffectTypes.Should().Contain(StatusEffectType.Silenced);
        character.ActiveStatusEffects.Should().HaveCount(1);
        character.ActiveStatusEffects[0].Type.Should().Be(StatusEffectType.Blessed);
    }
}
