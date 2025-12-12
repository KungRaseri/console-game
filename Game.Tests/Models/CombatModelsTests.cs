using FluentAssertions;
using Game.Models;
using Xunit;

namespace Game.Tests.Models;

/// <summary>
/// Comprehensive tests for CombatAction model.
/// Target: 0% -> 100% coverage.
/// </summary>
public class CombatActionTests
{
    [Fact]
    public void CombatAction_Should_Initialize_With_Default_Values()
    {
        // Act
        var action = new CombatAction();

        // Assert
        action.Type.Should().Be(CombatActionType.Attack);
        action.ActorName.Should().BeEmpty();
        action.TargetName.Should().BeNull();
        action.ItemUsed.Should().BeNull();
    }

    [Theory]
    [InlineData(CombatActionType.Attack)]
    [InlineData(CombatActionType.Defend)]
    [InlineData(CombatActionType.UseItem)]
    [InlineData(CombatActionType.Flee)]
    public void CombatAction_Should_Support_All_Action_Types(CombatActionType type)
    {
        // Arrange
        var action = new CombatAction();

        // Act
        action.Type = type;

        // Assert
        action.Type.Should().Be(type);
    }

    [Fact]
    public void CombatAction_Should_Create_Attack_Action()
    {
        // Act
        var action = new CombatAction
        {
            Type = CombatActionType.Attack,
            ActorName = "Hero",
            TargetName = "Goblin"
        };

        // Assert
        action.Type.Should().Be(CombatActionType.Attack);
        action.ActorName.Should().Be("Hero");
        action.TargetName.Should().Be("Goblin");
    }

    [Fact]
    public void CombatAction_Should_Create_Defend_Action()
    {
        // Act
        var action = new CombatAction
        {
            Type = CombatActionType.Defend,
            ActorName = "Warrior"
        };

        // Assert
        action.Type.Should().Be(CombatActionType.Defend);
        action.ActorName.Should().Be("Warrior");
    }

    [Fact]
    public void CombatAction_Should_Create_UseItem_Action()
    {
        // Arrange
        var potion = new Item { Name = "Health Potion" };

        // Act
        var action = new CombatAction
        {
            Type = CombatActionType.UseItem,
            ActorName = "Mage",
            ItemUsed = potion
        };

        // Assert
        action.Type.Should().Be(CombatActionType.UseItem);
        action.ActorName.Should().Be("Mage");
        action.ItemUsed.Should().Be(potion);
        action.ItemUsed.Name.Should().Be("Health Potion");
    }

    [Fact]
    public void CombatAction_Should_Create_Flee_Action()
    {
        // Act
        var action = new CombatAction
        {
            Type = CombatActionType.Flee,
            ActorName = "Rogue"
        };

        // Assert
        action.Type.Should().Be(CombatActionType.Flee);
        action.ActorName.Should().Be("Rogue");
    }

    [Fact]
    public void CombatActionType_Enum_Should_Have_Four_Values()
    {
        // Assert
        var types = Enum.GetValues<CombatActionType>();
        types.Should().HaveCount(4);
        types.Should().Contain(new[]
        {
            CombatActionType.Attack,
            CombatActionType.Defend,
            CombatActionType.UseItem,
            CombatActionType.Flee
        });
    }
}

/// <summary>
/// Comprehensive tests for CombatResult model.
/// Target: Already at 100% but adding comprehensive tests.
/// </summary>
public class CombatResultTests
{
    [Fact]
    public void CombatResult_Should_Initialize_With_Default_Values()
    {
        // Act
        var result = new CombatResult();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().BeEmpty();
        result.Damage.Should().Be(0);
        result.Healing.Should().Be(0);
        result.IsCritical.Should().BeFalse();
        result.IsDodged.Should().BeFalse();
        result.IsBlocked.Should().BeFalse();
        result.Effect.Should().Be(CombatEffect.None);
    }

    [Fact]
    public void CombatResult_Should_Create_Successful_Attack()
    {
        // Act
        var result = new CombatResult
        {
            Success = true,
            Message = "Hit for 25 damage!",
            Damage = 25
        };

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Hit for 25 damage!");
        result.Damage.Should().Be(25);
    }

    [Fact]
    public void CombatResult_Should_Create_Critical_Hit()
    {
        // Act
        var result = new CombatResult
        {
            Success = true,
            Message = "Critical hit!",
            Damage = 50,
            IsCritical = true
        };

        // Assert
        result.Success.Should().BeTrue();
        result.IsCritical.Should().BeTrue();
        result.Damage.Should().Be(50);
    }

    [Fact]
    public void CombatResult_Should_Create_Dodged_Attack()
    {
        // Act
        var result = new CombatResult
        {
            Success = false,
            Message = "Dodged!",
            IsDodged = true
        };

        // Assert
        result.Success.Should().BeFalse();
        result.IsDodged.Should().BeTrue();
        result.Damage.Should().Be(0);
    }

    [Fact]
    public void CombatResult_Should_Create_Blocked_Attack()
    {
        // Act
        var result = new CombatResult
        {
            Success = false,
            Message = "Blocked!",
            IsBlocked = true
        };

        // Assert
        result.Success.Should().BeFalse();
        result.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void CombatResult_Should_Create_Healing_Result()
    {
        // Act
        var result = new CombatResult
        {
            Success = true,
            Message = "Healed for 30 HP",
            Healing = 30
        };

        // Assert
        result.Success.Should().BeTrue();
        result.Healing.Should().Be(30);
        result.Damage.Should().Be(0);
    }

    [Theory]
    [InlineData(CombatEffect.None)]
    [InlineData(CombatEffect.Stunned)]
    [InlineData(CombatEffect.Poisoned)]
    [InlineData(CombatEffect.Burning)]
    [InlineData(CombatEffect.Frozen)]
    [InlineData(CombatEffect.Bleeding)]
    public void CombatResult_Should_Support_All_Effects(CombatEffect effect)
    {
        // Arrange
        var result = new CombatResult();

        // Act
        result.Effect = effect;

        // Assert
        result.Effect.Should().Be(effect);
    }

    [Fact]
    public void CombatEffect_Enum_Should_Have_Six_Values()
    {
        // Assert
        var effects = Enum.GetValues<CombatEffect>();
        effects.Should().HaveCount(6);
        effects.Should().Contain(new[]
        {
            CombatEffect.None,
            CombatEffect.Stunned,
            CombatEffect.Poisoned,
            CombatEffect.Burning,
            CombatEffect.Frozen,
            CombatEffect.Bleeding
        });
    }
}

/// <summary>
/// Comprehensive tests for CombatOutcome model.
/// Target: Already at 100% but adding comprehensive tests.
/// </summary>
public class CombatOutcomeTests
{
    [Fact]
    public void CombatOutcome_Should_Initialize_With_Default_Values()
    {
        // Act
        var outcome = new CombatOutcome();

        // Assert
        outcome.PlayerVictory.Should().BeFalse();
        outcome.XPGained.Should().Be(0);
        outcome.GoldGained.Should().Be(0);
        outcome.LootDropped.Should().NotBeNull();
        outcome.LootDropped.Should().BeEmpty();
        outcome.Summary.Should().BeEmpty();
    }

    [Fact]
    public void CombatOutcome_Should_Create_Victory_Outcome()
    {
        // Act
        var outcome = new CombatOutcome
        {
            PlayerVictory = true,
            XPGained = 100,
            GoldGained = 50,
            Summary = "You defeated the Goblin!"
        };

        // Assert
        outcome.PlayerVictory.Should().BeTrue();
        outcome.XPGained.Should().Be(100);
        outcome.GoldGained.Should().Be(50);
        outcome.Summary.Should().Be("You defeated the Goblin!");
    }

    [Fact]
    public void CombatOutcome_Should_Create_Defeat_Outcome()
    {
        // Act
        var outcome = new CombatOutcome
        {
            PlayerVictory = false,
            Summary = "You were defeated..."
        };

        // Assert
        outcome.PlayerVictory.Should().BeFalse();
        outcome.XPGained.Should().Be(0);
        outcome.GoldGained.Should().Be(0);
        outcome.Summary.Should().Be("You were defeated...");
    }

    [Fact]
    public void CombatOutcome_Should_Support_Loot_Drops()
    {
        // Arrange
        var sword = new Item { Name = "Iron Sword" };
        var potion = new Item { Name = "Health Potion" };

        // Act
        var outcome = new CombatOutcome
        {
            PlayerVictory = true,
            LootDropped = new List<Item> { sword, potion }
        };

        // Assert
        outcome.LootDropped.Should().HaveCount(2);
        outcome.LootDropped.Should().Contain(sword);
        outcome.LootDropped.Should().Contain(potion);
    }

    [Fact]
    public void CombatOutcome_Should_Support_Large_Rewards()
    {
        // Act
        var outcome = new CombatOutcome
        {
            PlayerVictory = true,
            XPGained = 10000,
            GoldGained = 5000
        };

        // Assert
        outcome.XPGained.Should().Be(10000);
        outcome.GoldGained.Should().Be(5000);
    }
}
