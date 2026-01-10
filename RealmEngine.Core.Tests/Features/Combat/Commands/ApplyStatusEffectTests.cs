using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RealmEngine.Core.Features.Combat.Commands;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Core.Tests.Features.Combat.Commands;

[Trait("Category", "Feature")]
public class ApplyStatusEffectTests
{
    private readonly Mock<ILogger<ApplyStatusEffectHandler>> _mockLogger;
    private readonly ApplyStatusEffectHandler _handler;

    public ApplyStatusEffectTests()
    {
        _mockLogger = new Mock<ILogger<ApplyStatusEffectHandler>>();
        _handler = new ApplyStatusEffectHandler(_mockLogger.Object);
    }

    [Fact]
    public async Task Should_Apply_Status_Effect_To_Character()
    {
        // Arrange
        var character = new Character { Name = "Hero", Wisdom = 10 };
        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison",
            OriginalDuration = 3,
            TickDamage = 5,
            DamageType = "poison"
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        character.ActiveStatusEffects.Should().HaveCount(1);
        character.ActiveStatusEffects[0].Type.Should().Be(StatusEffectType.Poisoned);
        character.ActiveStatusEffects[0].RemainingDuration.Should().Be(3);
    }

    [Fact]
    public async Task Should_Apply_Status_Effect_To_Enemy()
    {
        // Arrange
        var enemy = new Enemy { Name = "Goblin" };
        var effect = new StatusEffect
        {
            Id = "burn1",
            Type = StatusEffectType.Burning,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Burning",
            OriginalDuration = 2,
            TickDamage = 8,
            DamageType = "fire"
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetEnemy = enemy,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        enemy.ActiveStatusEffects.Should().HaveCount(1);
        enemy.ActiveStatusEffects[0].Type.Should().Be(StatusEffectType.Burning);
    }

    [Fact]
    public async Task Should_Fail_When_No_Target_Specified()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "stun1",
            Type = StatusEffectType.Stunned,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Stunned",
            OriginalDuration = 1
        };

        var command = new ApplyStatusEffectCommand { Effect = effect };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No valid target");
    }

    [Fact]
    public async Task Should_Stack_Effect_When_Allowed()
    {
        // Arrange
        var character = new Character { Name = "Hero" };
        var effect = new StatusEffect
        {
            Id = "bleed1",
            Type = StatusEffectType.Bleeding,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Bleeding",
            OriginalDuration = 3,
            TickDamage = 3,
            DamageType = "physical",
            CanStack = true,
            MaxStacks = 5,
            StackCount = 1
        };

        // Apply first time
        character.ActiveStatusEffects.Add(effect);

        // Apply second time
        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = new StatusEffect
            {
                Id = "bleed2",
                Type = StatusEffectType.Bleeding,
                Category = StatusEffectCategory.DamageOverTime,
                Name = "Bleeding",
                OriginalDuration = 3,
                TickDamage = 3,
                DamageType = "physical",
                CanStack = true,
                MaxStacks = 5
            },
            AllowStacking = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Stacked.Should().BeTrue();
        result.CurrentStacks.Should().Be(2);
        character.ActiveStatusEffects.Should().HaveCount(1);
        character.ActiveStatusEffects[0].StackCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_Not_Stack_Beyond_Max_Stacks()
    {
        // Arrange
        var character = new Character { Name = "Hero" };
        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison",
            OriginalDuration = 3,
            TickDamage = 5,
            DamageType = "poison",
            CanStack = true,
            MaxStacks = 3,
            StackCount = 3
        };

        character.ActiveStatusEffects.Add(effect);

        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = new StatusEffect
            {
                Id = "poison2",
                Type = StatusEffectType.Poisoned,
                Category = StatusEffectCategory.DamageOverTime,
                Name = "Poison",
                OriginalDuration = 3,
                TickDamage = 5,
                DamageType = "poison",
                CanStack = true,
                MaxStacks = 3
            },
            AllowStacking = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        character.ActiveStatusEffects[0].StackCount.Should().Be(3);
    }

    [Fact]
    public async Task Should_Refresh_Duration_When_Enabled()
    {
        // Arrange
        var character = new Character { Name = "Hero" };
        var effect = new StatusEffect
        {
            Id = "stun1",
            Type = StatusEffectType.Stunned,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Stunned",
            OriginalDuration = 2,
            RemainingDuration = 1
        };

        character.ActiveStatusEffects.Add(effect);

        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = new StatusEffect
            {
                Id = "stun2",
                Type = StatusEffectType.Stunned,
                Category = StatusEffectCategory.CrowdControl,
                Name = "Stunned",
                OriginalDuration = 2
            },
            RefreshDuration = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.DurationRefreshed.Should().BeTrue();
        character.ActiveStatusEffects[0].RemainingDuration.Should().Be(2);
    }

    [Fact]
    public async Task Should_Resist_Effect_With_100_Percent_Resistance()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Fire Elemental",
            Traits = new Dictionary<string, TraitValue>
            {
                { "resistFire", new TraitValue(100, TraitType.Number) }
            }
        };

        var effect = new StatusEffect
        {
            Id = "burn1",
            Type = StatusEffectType.Burning,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Burning",
            OriginalDuration = 3,
            TickDamage = 10,
            DamageType = "fire"
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetEnemy = enemy,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Resisted.Should().BeTrue();
        result.ResistancePercentage.Should().Be(100);
        enemy.ActiveStatusEffects.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Be_Immune_To_Poison()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Undead",
            Traits = new Dictionary<string, TraitValue>
            {
                { "immuneToPoison", new TraitValue(true, TraitType.Boolean) }
            }
        };

        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison",
            OriginalDuration = 3,
            TickDamage = 5,
            DamageType = "poison"
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetEnemy = enemy,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Resisted.Should().BeTrue();
        result.ResistancePercentage.Should().Be(100);
        result.Message.Should().Contain("immune");
        enemy.ActiveStatusEffects.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Calculate_Character_Resistance_From_Wisdom()
    {
        // Arrange
        var character = new Character
        {
            Name = "Wizard",
            Wisdom = 50 // Should give 5% resistance
        };

        var effect = new StatusEffect
        {
            Id = "stun1",
            Type = StatusEffectType.Stunned,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Stunned",
            OriginalDuration = 1
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ResistancePercentage.Should().Be(5);
    }

    [Fact]
    public async Task Should_Apply_Magic_Resistance_To_Status_Effects()
    {
        // Arrange
        var enemy = new Enemy
        {
            Name = "Magic Resistant Creature",
            Traits = new Dictionary<string, TraitValue>
            {
                { StandardTraits.ResistMagic, new TraitValue(40, TraitType.Number) }
            }
        };

        var effect = new StatusEffect
        {
            Id = "freeze1",
            Type = StatusEffectType.Frozen,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Frozen",
            OriginalDuration = 2,
            DamageType = "ice"
        };

        var command = new ApplyStatusEffectCommand
        {
            TargetEnemy = enemy,
            Effect = effect
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ResistancePercentage.Should().Be(20); // 40 / 2 = 20
    }

    [Fact]
    public async Task Should_Not_Apply_Same_Effect_Twice_Without_Stacking()
    {
        // Arrange
        var character = new Character { Name = "Hero" };
        var effect = new StatusEffect
        {
            Id = "fear1",
            Type = StatusEffectType.Feared,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Fear",
            OriginalDuration = 2,
            CanStack = false
        };

        character.ActiveStatusEffects.Add(effect);

        var command = new ApplyStatusEffectCommand
        {
            TargetCharacter = character,
            Effect = new StatusEffect
            {
                Id = "fear2",
                Type = StatusEffectType.Feared,
                Category = StatusEffectCategory.CrowdControl,
                Name = "Fear",
                OriginalDuration = 2,
                CanStack = false
            },
            AllowStacking = false,
            RefreshDuration = false
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already affected");
        character.ActiveStatusEffects.Should().HaveCount(1);
    }
}
