using Game.Models;

namespace Game.Services;

/// <summary>
/// Service for calculating skill effects and bonuses.
/// </summary>
public static class SkillEffectService
{
    /// <summary>
    /// Get total physical damage bonus from skills.
    /// </summary>
    public static double GetPhysicalDamageMultiplier(Character character)
    {
        double multiplier = 1.0;
        
        var powerAttack = character.LearnedSkills.FirstOrDefault(s => s.Name == "Power Attack");
        if (powerAttack != null)
        {
            multiplier += (powerAttack.CurrentRank * 0.10); // +10% per rank
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Get total magic damage bonus from skills.
    /// </summary>
    public static double GetMagicDamageMultiplier(Character character)
    {
        double multiplier = 1.0;
        
        var arcaneKnowledge = character.LearnedSkills.FirstOrDefault(s => s.Name == "Arcane Knowledge");
        if (arcaneKnowledge != null)
        {
            multiplier += (arcaneKnowledge.CurrentRank * 0.10); // +10% per rank
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Get total critical hit chance bonus from skills.
    /// </summary>
    public static double GetCriticalChanceBonus(Character character)
    {
        double bonus = 0.0;
        
        var criticalStrike = character.LearnedSkills.FirstOrDefault(s => s.Name == "Critical Strike");
        if (criticalStrike != null)
        {
            bonus += (criticalStrike.CurrentRank * 2.0); // +2% per rank
        }
        
        return bonus;
    }
    
    /// <summary>
    /// Get total physical defense bonus from skills.
    /// </summary>
    public static double GetPhysicalDefenseMultiplier(Character character)
    {
        double multiplier = 1.0;
        
        var ironSkin = character.LearnedSkills.FirstOrDefault(s => s.Name == "Iron Skin");
        if (ironSkin != null)
        {
            multiplier += (ironSkin.CurrentRank * 0.05); // +5% per rank
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Get total dodge chance bonus from skills.
    /// </summary>
    public static double GetDodgeChanceBonus(Character character)
    {
        double bonus = 0.0;
        
        var quickReflexes = character.LearnedSkills.FirstOrDefault(s => s.Name == "Quick Reflexes");
        if (quickReflexes != null)
        {
            bonus += (quickReflexes.CurrentRank * 3.0); // +3% per rank
        }
        
        return bonus;
    }
    
    /// <summary>
    /// Get total rare item find bonus from skills.
    /// </summary>
    public static double GetRareItemFindBonus(Character character)
    {
        double bonus = 0.0;
        
        var treasureHunter = character.LearnedSkills.FirstOrDefault(s => s.Name == "Treasure Hunter");
        if (treasureHunter != null)
        {
            bonus += (treasureHunter.CurrentRank * 10.0); // +10% per rank
        }
        
        return bonus;
    }
    
    /// <summary>
    /// Get total max mana bonus from skills.
    /// </summary>
    public static double GetMaxManaMultiplier(Character character)
    {
        double multiplier = 1.0;
        
        var manaEfficiency = character.LearnedSkills.FirstOrDefault(s => s.Name == "Mana Efficiency");
        if (manaEfficiency != null)
        {
            multiplier += (manaEfficiency.CurrentRank * 0.10); // +10% per rank
        }
        
        return multiplier;
    }
    
    /// <summary>
    /// Get health regeneration per turn from skills.
    /// </summary>
    public static int GetHealthRegeneration(Character character)
    {
        int regen = 0;
        
        var regeneration = character.LearnedSkills.FirstOrDefault(s => s.Name == "Regeneration");
        if (regeneration != null)
        {
            regen += (regeneration.CurrentRank * 2); // +2 HP per rank
        }
        
        return regen;
    }
    
    /// <summary>
    /// Apply regeneration effect to character.
    /// </summary>
    public static int ApplyRegeneration(Character character)
    {
        var regenAmount = GetHealthRegeneration(character);
        
        if (regenAmount > 0 && character.Health < character.MaxHealth)
        {
            var actualRegen = Math.Min(regenAmount, character.MaxHealth - character.Health);
            character.Health += actualRegen;
            return actualRegen;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Get a summary of all active skill bonuses.
    /// </summary>
    public static string GetSkillBonusSummary(Character character)
    {
        var summary = new List<string>();
        
        var physDmg = GetPhysicalDamageMultiplier(character);
        if (physDmg > 1.0)
            summary.Add($"[green]+{(physDmg - 1.0) * 100:F0}% Physical Damage[/]");
        
        var magicDmg = GetMagicDamageMultiplier(character);
        if (magicDmg > 1.0)
            summary.Add($"[cyan]+{(magicDmg - 1.0) * 100:F0}% Magic Damage[/]");
        
        var critBonus = GetCriticalChanceBonus(character);
        if (critBonus > 0)
            summary.Add($"[yellow]+{critBonus:F0}% Critical Chance[/]");
        
        var defMulti = GetPhysicalDefenseMultiplier(character);
        if (defMulti > 1.0)
            summary.Add($"[blue]+{(defMulti - 1.0) * 100:F0}% Physical Defense[/]");
        
        var dodgeBonus = GetDodgeChanceBonus(character);
        if (dodgeBonus > 0)
            summary.Add($"[magenta]+{dodgeBonus:F0}% Dodge Chance[/]");
        
        var lootBonus = GetRareItemFindBonus(character);
        if (lootBonus > 0)
            summary.Add($"[gold1]+{lootBonus:F0}% Rare Item Find[/]");
        
        var manaMulti = GetMaxManaMultiplier(character);
        if (manaMulti > 1.0)
            summary.Add($"[blue]+{(manaMulti - 1.0) * 100:F0}% Max Mana[/]");
        
        var regen = GetHealthRegeneration(character);
        if (regen > 0)
            summary.Add($"[green]+{regen} HP Regeneration per turn[/]");
        
        return summary.Any() ? string.Join("\n", summary) : "[dim]No active skill bonuses[/]";
    }
}
