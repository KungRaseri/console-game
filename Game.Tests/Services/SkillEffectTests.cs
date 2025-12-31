using FluentAssertions;
using Game.Shared.Models;

namespace Game.Tests.Services;

public class SkillEffectTests
{
    [Fact]
    public void PowerAttack_Should_Increase_Physical_Damage()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Power Attack",
            CurrentRank = 3
        });

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(character);

        // Assert
        multiplier.Should().Be(1.30); // 1.0 + (3 * 0.10)
    }

    [Fact]
    public void ArcaneKnowledge_Should_Increase_Magic_Damage()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Arcane Knowledge",
            CurrentRank = 5
        });

        // Act
        var multiplier = SkillEffectCalculator.GetMagicDamageMultiplier(character);

        // Assert
        multiplier.Should().Be(1.50); // 1.0 + (5 * 0.10)
    }

    [Fact]
    public void CriticalStrike_Should_Increase_Crit_Chance()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Critical Strike",
            CurrentRank = 4
        });

        // Act
        var bonus = SkillEffectCalculator.GetCriticalChanceBonus(character);

        // Assert
        bonus.Should().Be(8.0); // 4 * 2.0%
    }

    [Fact]
    public void IronSkin_Should_Increase_Physical_Defense()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Iron Skin",
            CurrentRank = 2
        });

        // Act
        var multiplier = SkillEffectCalculator.GetPhysicalDefenseMultiplier(character);

        // Assert
        multiplier.Should().Be(1.10); // 1.0 + (2 * 0.05)
    }

    [Fact]
    public void QuickReflexes_Should_Increase_Dodge_Chance()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Quick Reflexes",
            CurrentRank = 5
        });

        // Act
        var bonus = SkillEffectCalculator.GetDodgeChanceBonus(character);

        // Assert
        bonus.Should().Be(15.0); // 5 * 3.0%
    }

    [Fact]
    public void TreasureHunter_Should_Increase_Rare_Item_Find()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Treasure Hunter",
            CurrentRank = 3
        });

        // Act
        var bonus = SkillEffectCalculator.GetRareItemFindBonus(character);

        // Assert
        bonus.Should().Be(30.0); // 3 * 10.0%
    }

    [Fact]
    public void ManaEfficiency_Should_Increase_Max_Mana()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Mana Efficiency",
            CurrentRank = 2
        });

        // Act
        var multiplier = SkillEffectCalculator.GetMaxManaMultiplier(character);

        // Assert
        multiplier.Should().Be(1.20); // 1.0 + (2 * 0.10)
    }

    [Fact]
    public void Regeneration_Should_Increase_HP_Regen()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill
        {
            Name = "Regeneration",
            CurrentRank = 3
        });

        // Act
        var regen = SkillEffectCalculator.GetHealthRegeneration(character);

        // Assert
        regen.Should().Be(6); // 3 * 2 HP
    }

    [Fact]
    public void ApplyRegeneration_Should_Heal_Character()
    {
        // Arrange
        var character = new Character
        {
            Health = 50,
            MaxHealth = 100
        };
        character.LearnedSkills.Add(new Skill
        {
            Name = "Regeneration",
            CurrentRank = 2
        });

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(4); // 2 * 2 HP
        character.Health.Should().Be(54);
    }

    [Fact]
    public void ApplyRegeneration_Should_Not_Overheal()
    {
        // Arrange
        var character = new Character
        {
            Health = 98,
            MaxHealth = 100
        };
        character.LearnedSkills.Add(new Skill
        {
            Name = "Regeneration",
            CurrentRank = 2
        });

        // Act
        var healed = SkillEffectCalculator.ApplyRegeneration(character);

        // Assert
        healed.Should().Be(2); // Only heal to max
        character.Health.Should().Be(100);
    }

    [Fact]
    public void GetSkillBonusSummary_Should_List_All_Active_Bonuses()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill { Name = "Power Attack", CurrentRank = 2 });
        character.LearnedSkills.Add(new Skill { Name = "Critical Strike", CurrentRank = 1 });
        character.LearnedSkills.Add(new Skill { Name = "Regeneration", CurrentRank = 1 });

        // Act
        var summary = SkillEffectCalculator.GetSkillBonusSummary(character);

        // Assert
        summary.Should().Contain("Physical Damage");
        summary.Should().Contain("Critical Chance");
        summary.Should().Contain("Regeneration");
    }

    [Fact]
    public void GetSkillBonusSummary_Should_Show_No_Bonuses_When_No_Skills()
    {
        // Arrange
        var character = new Character();

        // Act
        var summary = SkillEffectCalculator.GetSkillBonusSummary(character);

        // Assert
        summary.Should().Contain("No active skill bonuses");
    }

    [Fact]
    public void Character_GetRareItemChance_Should_Include_Skill_Bonus()
    {
        // Arrange
        var character = new Character { Charisma = 10 };
        character.LearnedSkills.Add(new Skill { Name = "Treasure Hunter", CurrentRank = 2 });

        // Act
        var chance = character.GetRareItemChance();

        // Assert
        chance.Should().Be(25.0); // (10 * 0.5) + (2 * 10.0)
    }

    [Fact]
    public void Character_GetMaxMana_Should_Include_Skill_Bonus()
    {
        // Arrange
        var character = new Character
        {
            Wisdom = 10,
            Level = 5
        };
        character.LearnedSkills.Add(new Skill { Name = "Mana Efficiency", CurrentRank = 3 });

        // Act
        var maxMana = character.GetMaxMana();

        // Assert
        // Base: (10 * 5) + (5 * 3) = 65
        // With skill: 65 * 1.30 = 84
        maxMana.Should().Be(84);
    }

    [Fact]
    public void Multiple_Skills_Should_Stack_Bonuses()
    {
        // Arrange
        var character = new Character();
        character.LearnedSkills.Add(new Skill { Name = "Power Attack", CurrentRank = 5 });
        character.LearnedSkills.Add(new Skill { Name = "Critical Strike", CurrentRank = 5 });
        character.LearnedSkills.Add(new Skill { Name = "Iron Skin", CurrentRank = 5 });

        // Act
        var physDmg = SkillEffectCalculator.GetPhysicalDamageMultiplier(character);
        var critChance = SkillEffectCalculator.GetCriticalChanceBonus(character);
        var defense = SkillEffectCalculator.GetPhysicalDefenseMultiplier(character);

        // Assert
        physDmg.Should().Be(1.50); // +50% damage
        critChance.Should().Be(10.0); // +10% crit
        defense.Should().Be(1.25); // +25% defense
    }
}
