namespace Game.Models;

/// <summary>
/// Information about a pending level-up that needs player choices.
/// </summary>
public class LevelUpInfo
{
    public int NewLevel { get; set; }
    public int AttributePointsGained { get; set; }
    public int SkillPointsGained { get; set; }
    public bool IsProcessed { get; set; }
}

/// <summary>
/// Tracks attribute point allocations during level-up.
/// </summary>
public class AttributePointAllocation
{
    public int StrengthPoints { get; set; }
    public int DexterityPoints { get; set; }
    public int ConstitutionPoints { get; set; }
    public int IntelligencePoints { get; set; }
    public int WisdomPoints { get; set; }
    public int CharismaPoints { get; set; }
    
    public int TotalPointsAllocated => 
        StrengthPoints + DexterityPoints + ConstitutionPoints + 
        IntelligencePoints + WisdomPoints + CharismaPoints;
    
    public void Reset()
    {
        StrengthPoints = 0;
        DexterityPoints = 0;
        ConstitutionPoints = 0;
        IntelligencePoints = 0;
        WisdomPoints = 0;
        CharismaPoints = 0;
    }
}

/// <summary>
/// Represents a skill that can be learned or upgraded.
/// </summary>
public class Skill
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public int MaxRank { get; set; } = 5;
    public int CurrentRank { get; set; }
    public SkillType Type { get; set; }
    public string Effect { get; set; } = string.Empty;
}

/// <summary>
/// Categories of skills available.
/// </summary>
public enum SkillType
{
    Combat,
    Defense,
    Magic,
    Utility,
    Passive
}
