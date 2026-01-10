using FluentAssertions;
using RealmEngine.Shared.Models;
using Xunit;

namespace RealmEngine.Shared.Tests.Models;

[Trait("Category", "Model")]
public class StatusEffectTests
{
    [Fact]
    public void StatusEffect_Should_Initialize_With_Required_Properties()
    {
        // Arrange & Act
        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison"
        };

        // Assert
        effect.Id.Should().Be("poison1");
        effect.Type.Should().Be(StatusEffectType.Poisoned);
        effect.Category.Should().Be(StatusEffectCategory.DamageOverTime);
        effect.Name.Should().Be("Poison");
        effect.StackCount.Should().Be(1);
        effect.MaxStacks.Should().Be(1);
        effect.CanDispel.Should().BeTrue();
        effect.CanStack.Should().BeFalse();
    }

    [Theory]
    [InlineData(StatusEffectType.Burning, "fire")]
    [InlineData(StatusEffectType.Poisoned, "poison")]
    [InlineData(StatusEffectType.Bleeding, "physical")]
    [InlineData(StatusEffectType.Frozen, "ice")]
    public void GetDamageType_Should_Return_Correct_Damage_Type(StatusEffectType type, string expectedDamageType)
    {
        // Act
        var damageType = type.GetDamageType();

        // Assert
        damageType.Should().Be(expectedDamageType);
    }

    [Theory]
    [InlineData(StatusEffectType.Burning, StatusEffectCategory.DamageOverTime)]
    [InlineData(StatusEffectType.Poisoned, StatusEffectCategory.DamageOverTime)]
    [InlineData(StatusEffectType.Bleeding, StatusEffectCategory.DamageOverTime)]
    [InlineData(StatusEffectType.Regenerating, StatusEffectCategory.HealOverTime)]
    [InlineData(StatusEffectType.Frozen, StatusEffectCategory.CrowdControl)]
    [InlineData(StatusEffectType.Stunned, StatusEffectCategory.CrowdControl)]
    [InlineData(StatusEffectType.Strengthened, StatusEffectCategory.Buff)]
    [InlineData(StatusEffectType.Weakened, StatusEffectCategory.Debuff)]
    public void GetCategory_Should_Return_Correct_Category(StatusEffectType type, StatusEffectCategory expectedCategory)
    {
        // Act
        var category = type.GetCategory();

        // Assert
        category.Should().Be(expectedCategory);
    }

    [Theory]
    [InlineData(StatusEffectType.Burning, "fire")]
    [InlineData(StatusEffectType.Poisoned, "poison")]
    [InlineData(StatusEffectType.Frozen, "snowflake")]
    [InlineData(StatusEffectType.Stunned, "dizzy")]
    [InlineData(StatusEffectType.Shielded, "shield")]
    [InlineData(StatusEffectType.Regenerating, "heart-plus")]
    public void GetDefaultIcon_Should_Return_Appropriate_Icon(StatusEffectType type, string expectedIcon)
    {
        // Act
        var icon = type.GetDefaultIcon();

        // Assert
        icon.Should().Be(expectedIcon);
    }

    [Fact]
    public void StatusEffect_Should_Support_Stat_Modifiers()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "buff1",
            Type = StatusEffectType.Strengthened,
            Category = StatusEffectCategory.Buff,
            Name = "Strength Buff",
            StatModifiers = new Dictionary<string, int>
            {
                { "attack", 10 },
                { "defense", 5 }
            }
        };

        // Assert
        effect.StatModifiers.Should().HaveCount(2);
        effect.StatModifiers["attack"].Should().Be(10);
        effect.StatModifiers["defense"].Should().Be(5);
    }

    [Fact]
    public void StatusEffect_Should_Support_Stacking()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison",
            CanStack = true,
            MaxStacks = 5,
            StackCount = 3
        };

        // Assert
        effect.CanStack.Should().BeTrue();
        effect.MaxStacks.Should().Be(5);
        effect.StackCount.Should().Be(3);
    }

    [Fact]
    public void StatusEffect_Should_Track_Source()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "burn1",
            Type = StatusEffectType.Burning,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Burning",
            Source = "Fireball Spell"
        };

        // Assert
        effect.Source.Should().Be("Fireball Spell");
    }

    [Fact]
    public void StatusEffect_Should_Support_Duration_Tracking()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "stun1",
            Type = StatusEffectType.Stunned,
            Category = StatusEffectCategory.CrowdControl,
            Name = "Stunned",
            OriginalDuration = 3,
            RemainingDuration = 2
        };

        // Assert
        effect.OriginalDuration.Should().Be(3);
        effect.RemainingDuration.Should().Be(2);
    }

    [Fact]
    public void StatusEffect_Should_Support_DoT_And_HoT()
    {
        // Arrange
        var dotEffect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Poison",
            TickDamage = 5
        };

        var hotEffect = new StatusEffect
        {
            Id = "regen1",
            Type = StatusEffectType.Regenerating,
            Category = StatusEffectCategory.HealOverTime,
            Name = "Regeneration",
            TickHealing = 8
        };

        // Assert
        dotEffect.TickDamage.Should().Be(5);
        hotEffect.TickHealing.Should().Be(8);
    }

    [Fact]
    public void StatusEffect_Should_Support_Dispel_Flag()
    {
        // Arrange
        var dispellable = new StatusEffect
        {
            Id = "curse1",
            Type = StatusEffectType.Cursed,
            Category = StatusEffectCategory.Debuff,
            Name = "Curse",
            CanDispel = true
        };

        var permanent = new StatusEffect
        {
            Id = "blessing1",
            Type = StatusEffectType.Blessed,
            Category = StatusEffectCategory.Buff,
            Name = "Divine Blessing",
            CanDispel = false
        };

        // Assert
        dispellable.CanDispel.Should().BeTrue();
        permanent.CanDispel.Should().BeFalse();
    }

    [Fact]
    public void StatusEffect_Should_Store_Icon_Name()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "shield1",
            Type = StatusEffectType.Shielded,
            Category = StatusEffectCategory.Buff,
            Name = "Magic Shield",
            IconName = "shield-magic"
        };

        // Assert
        effect.IconName.Should().Be("shield-magic");
    }

    [Fact]
    public void StatusEffect_Should_Store_Description()
    {
        // Arrange
        var effect = new StatusEffect
        {
            Id = "poison1",
            Type = StatusEffectType.Poisoned,
            Category = StatusEffectCategory.DamageOverTime,
            Name = "Deadly Poison",
            Description = "A lethal toxin that deals 10 damage per turn."
        };

        // Assert
        effect.Description.Should().Be("A lethal toxin that deals 10 damage per turn.");
    }

    [Fact]
    public void StatusEffectType_Should_Have_All_Expected_Types()
    {
        // Assert
        Enum.GetValues<StatusEffectType>().Should().Contain(new[]
        {
            StatusEffectType.Burning,
            StatusEffectType.Poisoned,
            StatusEffectType.Bleeding,
            StatusEffectType.Frozen,
            StatusEffectType.Stunned,
            StatusEffectType.Paralyzed,
            StatusEffectType.Feared,
            StatusEffectType.Confused,
            StatusEffectType.Silenced,
            StatusEffectType.Weakened,
            StatusEffectType.Cursed,
            StatusEffectType.Regenerating,
            StatusEffectType.Shielded,
            StatusEffectType.Strengthened,
            StatusEffectType.Hasted,
            StatusEffectType.Protected,
            StatusEffectType.Blessed,
            StatusEffectType.Enraged,
            StatusEffectType.Invisible,
            StatusEffectType.Taunted
        });
    }

    [Fact]
    public void StatusEffectCategory_Should_Have_All_Expected_Categories()
    {
        // Assert
        Enum.GetValues<StatusEffectCategory>().Should().Contain(new[]
        {
            StatusEffectCategory.Debuff,
            StatusEffectCategory.Buff,
            StatusEffectCategory.DamageOverTime,
            StatusEffectCategory.HealOverTime,
            StatusEffectCategory.CrowdControl
        });
    }
}
