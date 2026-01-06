# Skills System Architecture Design

**Status**: Design Document  
**Purpose**: Technical specification for rebuilding the practice-based skills progression system

**Related Documents**:
- [skills-system.md](../features/skills-system.md) - Feature overview
- [progression-system.md](../features/progression-system.md) - XP and leveling
- [abilities-system.md](../features/abilities-system.md) - Active powers
- [ROADMAP.md](../ROADMAP.md) - Development timeline

---

## Executive Summary

**Current State**: 10 hardcoded passive bonuses stored in `List<Skill>` on Character model. MaxRank of 5, no real progression system.

**Target State**: Practice-based skill system where actions grant skill XP, skills rank up (0-100), and provide cumulative passive bonuses. Skills stored per-character, persisted in save files, with JSON catalog defining skill properties.

**Scope**: Complete rebuild of skills system infrastructure
- New data models supporting 0-100 ranks with XP progression
- JSON catalog structure for skill definitions
- Skill XP award system triggered by gameplay actions
- Rank-up calculation and notification system
- Integration with combat, magic, crafting, and exploration
- Character sheet display with progress tracking

**Out of Scope** (Future Enhancements):
- Skill trainers and training mechanics
- Skill books granting XP
- Perk trees at milestone ranks
- Skill synergy bonuses
- Specialization bonuses

---

## 1. Data Model Architecture

### 1.1 Skill Model (RealmEngine.Shared.Models.Skill.cs)

**Current Implementation** (WRONG):
```csharp
public class Skill
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public int MaxRank { get; set; } = 5;  // Too low!
    public int CurrentRank { get; set; }
    public SkillType Type { get; set; }
    public string Effect { get; set; } = string.Empty;
}
```

**New Implementation**:
```csharp
namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a character's proficiency in a specific skill.
/// Skills rank from 0 (untrained) to 100 (master) through practice.
/// </summary>
public class CharacterSkill
{
    /// <summary>
    /// Unique identifier matching JSON catalog (e.g., "one-handed", "destruction").
    /// </summary>
    public required string SkillId { get; set; }
    
    /// <summary>
    /// Display name for UI (loaded from JSON).
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Current rank (0-100). Starts at 0 for all skills.
    /// </summary>
    public int CurrentRank { get; set; } = 0;
    
    /// <summary>
    /// Current XP progress toward next rank.
    /// </summary>
    public int CurrentXP { get; set; } = 0;
    
    /// <summary>
    /// XP required to reach next rank (calculated from formula).
    /// </summary>
    public int XPToNextRank { get; set; } = 100;
    
    /// <summary>
    /// Total XP earned in this skill (lifetime stat).
    /// </summary>
    public int TotalXP { get; set; } = 0;
    
    /// <summary>
    /// Category for organization (Combat, Magic, Profession, Survival, Attribute).
    /// </summary>
    public SkillCategory Category { get; set; }
    
    /// <summary>
    /// Optional: Track which actions contributed XP (for analytics).
    /// </summary>
    public Dictionary<string, int> XPSources { get; set; } = new();
}

/// <summary>
/// Skill categories for organization and filtering.
/// </summary>
public enum SkillCategory
{
    Attribute,      // STR, DEX, CON, INT, WIS, CHA
    Combat,         // One-Handed, Two-Handed, Archery, Block, Heavy/Light Armor
    Magic,          // Destruction, Restoration, Alteration, Conjuration, Illusion, Mysticism
    Profession,     // Blacksmithing, Alchemy, Enchanting
    Survival        // Lockpicking, Sneaking, Pickpocketing, Speech
}
```

### 1.2 Character Model Integration

**Add to Character.cs**:
```csharp
/// <summary>
/// Dictionary of skills keyed by SkillId for O(1) lookup.
/// All characters start with all skills at Rank 0.
/// </summary>
public Dictionary<string, CharacterSkill> Skills { get; set; } = new();

/// <summary>
/// Tracks last skill rank-up for notifications.
/// Key: SkillId, Value: Previous rank (for "Rank 45 → 46" messages).
/// </summary>
[BsonIgnore]
public Dictionary<string, int> RecentSkillRankUps { get; set; } = new();
```

### 1.3 Skill Definition Model (JSON Catalog Structure)

**File**: `RealmEngine.Data/Data/Json/skills/catalog.json`

```json
{
  "metadata": {
    "version": "1.0",
    "lastUpdated": "Current Date",
    "description": "Master catalog of all skills in the game",
    "type": "skills_catalog",
    "totalSkills": 27
  },
  "skills": {
    "combat": [
      {
        "id": "one-handed",
        "name": "One-Handed",
        "description": "Skill with single-hand weapons like swords, axes, and maces",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.5,
        "effectType": "damage_multiplier",
        "effectValue": 0.005,
        "effectDescription": "+0.5% damage per rank",
        "xpActions": [
          {
            "action": "melee_hit",
            "xpAmount": 5,
            "description": "Hit enemy with one-handed weapon"
          },
          {
            "action": "melee_kill",
            "xpAmount": 20,
            "description": "Kill enemy with one-handed weapon"
          },
          {
            "action": "critical_hit",
            "xpAmount": 15,
            "description": "Land critical hit with one-handed weapon"
          }
        ]
      },
      {
        "id": "two-handed",
        "name": "Two-Handed",
        "description": "Skill with large weapons requiring both hands",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.5,
        "effectType": "damage_multiplier",
        "effectValue": 0.005,
        "effectDescription": "+0.5% damage per rank",
        "xpActions": [
          {
            "action": "melee_hit",
            "xpAmount": 5
          },
          {
            "action": "melee_kill",
            "xpAmount": 20
          }
        ]
      },
      {
        "id": "archery",
        "name": "Archery",
        "description": "Skill with bows, crossbows, and thrown weapons",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.5,
        "effectType": "damage_multiplier",
        "effectValue": 0.005,
        "effectDescription": "+0.5% damage per rank",
        "xpActions": [
          {
            "action": "ranged_hit",
            "xpAmount": 5
          },
          {
            "action": "ranged_kill",
            "xpAmount": 20
          }
        ]
      },
      {
        "id": "block",
        "name": "Block",
        "description": "Skill in shield blocking and parrying attacks",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.4,
        "effectType": "block_chance",
        "effectValue": 0.003,
        "effectDescription": "+0.3% block chance per rank",
        "xpActions": [
          {
            "action": "block_success",
            "xpAmount": 10
          },
          {
            "action": "block_critical",
            "xpAmount": 25
          }
        ]
      },
      {
        "id": "heavy-armor",
        "name": "Heavy Armor",
        "description": "Proficiency in plate, chainmail, and heavy protective gear",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.4,
        "effectType": "armor_rating",
        "effectValue": 0.003,
        "effectDescription": "+0.3% armor rating per rank",
        "xpActions": [
          {
            "action": "take_damage_heavy",
            "xpAmount": 3
          }
        ]
      },
      {
        "id": "light-armor",
        "name": "Light Armor",
        "description": "Proficiency in leather, cloth, and agile protective gear",
        "category": "Combat",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.4,
        "effectType": "evasion",
        "effectValue": 0.002,
        "effectDescription": "+0.2% dodge chance per rank",
        "xpActions": [
          {
            "action": "take_damage_light",
            "xpAmount": 3
          },
          {
            "action": "dodge_success",
            "xpAmount": 8
          }
        ]
      }
    ],
    "magic": [
      {
        "id": "destruction",
        "name": "Destruction",
        "description": "Offensive magic dealing fire, ice, and lightning damage",
        "category": "Magic",
        "maxRank": 100,
        "baseXPCost": 120,
        "costMultiplier": 0.6,
        "effectType": "spell_power",
        "effectValue": 0.004,
        "effectDescription": "+0.4% spell damage per rank",
        "xpActions": [
          {
            "action": "cast_destruction_spell",
            "xpAmount": 8
          },
          {
            "action": "kill_with_spell",
            "xpAmount": 25
          }
        ]
      },
      {
        "id": "restoration",
        "name": "Restoration",
        "description": "Healing magic and restorative spells",
        "category": "Magic",
        "maxRank": 100,
        "baseXPCost": 120,
        "costMultiplier": 0.6,
        "effectType": "healing_power",
        "effectValue": 0.004,
        "effectDescription": "+0.4% healing per rank",
        "xpActions": [
          {
            "action": "cast_healing_spell",
            "xpAmount": 8
          },
          {
            "action": "restore_full_health",
            "xpAmount": 20
          }
        ]
      }
    ],
    "profession": [
      {
        "id": "alchemy",
        "name": "Alchemy",
        "description": "Crafting potions and elixirs from ingredients",
        "category": "Profession",
        "maxRank": 100,
        "baseXPCost": 150,
        "costMultiplier": 0.8,
        "effectType": "potion_effectiveness",
        "effectValue": 0.01,
        "effectDescription": "+1% potion power per rank",
        "xpActions": [
          {
            "action": "craft_potion",
            "xpAmount": 15
          },
          {
            "action": "discover_recipe",
            "xpAmount": 50
          }
        ]
      },
      {
        "id": "blacksmithing",
        "name": "Blacksmithing",
        "description": "Forging weapons and armor at the smithy",
        "category": "Profession",
        "maxRank": 100,
        "baseXPCost": 150,
        "costMultiplier": 0.8,
        "effectType": "crafted_quality",
        "effectValue": 0.005,
        "effectDescription": "+0.5% item quality per rank",
        "xpActions": [
          {
            "action": "forge_weapon",
            "xpAmount": 20
          },
          {
            "action": "forge_armor",
            "xpAmount": 25
          }
        ]
      }
    ],
    "survival": [
      {
        "id": "lockpicking",
        "name": "Lockpicking",
        "description": "Opening locked chests and doors",
        "category": "Survival",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.5,
        "effectType": "success_chance",
        "effectValue": 0.003,
        "effectDescription": "+0.3% lockpick success per rank",
        "xpActions": [
          {
            "action": "pick_lock",
            "xpAmount": 10
          },
          {
            "action": "pick_difficult_lock",
            "xpAmount": 30
          }
        ]
      },
      {
        "id": "sneaking",
        "name": "Sneaking",
        "description": "Moving stealthily to avoid detection",
        "category": "Survival",
        "maxRank": 100,
        "baseXPCost": 100,
        "costMultiplier": 0.5,
        "effectType": "detection_avoidance",
        "effectValue": 0.003,
        "effectDescription": "+0.3% stealth per rank",
        "xpActions": [
          {
            "action": "successful_sneak",
            "xpAmount": 5
          },
          {
            "action": "sneak_attack",
            "xpAmount": 20
          }
        ]
      }
    ]
  }
}
```

---

## 2. XP and Rank-Up Formulas

### 2.1 XP Cost Formula

**Formula**: `XPToNextRank = baseXPCost + (currentRank * baseXPCost * costMultiplier)`

**Examples**:

**One-Handed** (baseXPCost: 100, costMultiplier: 0.5):
- Rank 0→1: 100 + (0 × 100 × 0.5) = **100 XP**
- Rank 1→2: 100 + (1 × 100 × 0.5) = **150 XP**
- Rank 10→11: 100 + (10 × 100 × 0.5) = **600 XP**
- Rank 50→51: 100 + (50 × 100 × 0.5) = **2,600 XP**
- Rank 99→100: 100 + (99 × 100 × 0.5) = **5,050 XP**

**Destruction** (baseXPCost: 120, costMultiplier: 0.6):
- Rank 0→1: 120 XP
- Rank 50→51: 3,720 XP
- Rank 99→100: 7,248 XP

**Alchemy** (baseXPCost: 150, costMultiplier: 0.8):
- Rank 0→1: 150 XP
- Rank 50→51: 6,150 XP
- Rank 99→100: 12,030 XP

### 2.2 XP Award Guidelines

**Combat Skills** (Fast progression via frequent hits):
- Melee hit: 5 XP
- Melee kill: 20 XP
- Critical hit: +15 XP bonus
- Block success: 10 XP

**Magic Skills** (Moderate progression via spell casting):
- Cast spell: 8 XP
- Kill with spell: 25 XP
- Heal full health: 20 XP

**Profession Skills** (Slow progression via crafting):
- Craft basic potion: 15 XP
- Forge weapon: 20 XP
- Discover recipe: 50 XP (one-time)

**Survival Skills** (Variable progression):
- Pick lock: 10 XP (easy), 30 XP (difficult)
- Successful sneak: 5 XP per minute
- Sneak attack: 20 XP

### 2.3 Rank-Up Calculation Logic

```csharp
public class SkillProgressionService
{
    /// <summary>
    /// Award skill XP and check for rank-ups.
    /// </summary>
    public SkillRankUpResult AwardSkillXP(Character character, string skillId, int xpAmount)
    {
        if (!character.Skills.TryGetValue(skillId, out var skill))
        {
            // Skill not found - initialize it
            skill = InitializeSkill(skillId);
            character.Skills[skillId] = skill;
        }
        
        skill.CurrentXP += xpAmount;
        skill.TotalXP += xpAmount;
        
        var ranksGained = 0;
        var notifications = new List<string>();
        
        // Check for rank-ups (can rank up multiple times with large XP awards)
        while (skill.CurrentXP >= skill.XPToNextRank && skill.CurrentRank < 100)
        {
            skill.CurrentXP -= skill.XPToNextRank;
            skill.CurrentRank++;
            ranksGained++;
            
            // Recalculate XP needed for next rank
            skill.XPToNextRank = CalculateXPToNextRank(skillId, skill.CurrentRank);
            
            notifications.Add($"{skill.Name} increased to rank {skill.CurrentRank}!");
        }
        
        return new SkillRankUpResult
        {
            SkillId = skillId,
            NewRank = skill.CurrentRank,
            RanksGained = ranksGained,
            Notifications = notifications
        };
    }
    
    private int CalculateXPToNextRank(string skillId, int currentRank)
    {
        // Load skill definition from JSON catalog
        var skillDef = _skillCatalog.GetSkillDefinition(skillId);
        
        return skillDef.BaseXPCost + 
               (currentRank * skillDef.BaseXPCost * skillDef.CostMultiplier);
    }
}
```

---

## 3. Effect Calculation System

### 3.1 Per-Rank Effect Multipliers

Each skill provides cumulative bonuses based on current rank:

**Combat Damage Skills** (One-Handed, Two-Handed, Archery):
- Effect: +0.5% damage per rank
- Formula: `finalDamage = baseDamage * (1.0 + skillRank * 0.005)`
- Example: Rank 50 One-Handed = 25% damage boost

**Defense Skills** (Block, Heavy Armor, Light Armor):
- Effect: +0.3% defense per rank
- Formula: `blockChance = baseChance + (skillRank * 0.003)`
- Example: Rank 50 Block = +15% block chance

**Magic Skills** (Destruction, Restoration):
- Effect: +0.4% spell power per rank
- Formula: `spellDamage = baseSpellDamage * (1.0 + skillRank * 0.004)`
- Example: Rank 50 Destruction = 20% spell damage boost

**Profession Skills** (Alchemy, Blacksmithing):
- Effect: +1% effectiveness per rank
- Formula: `potionHealing = baseHealing * (1.0 + skillRank * 0.01)`
- Example: Rank 50 Alchemy = 50% better potions

### 3.2 SkillEffectCalculator Integration

**Update existing**: `RealmEngine.Shared.Utilities.SkillEffectCalculator`

```csharp
public static class SkillEffectCalculator
{
    /// <summary>
    /// Calculate damage bonus from weapon skill.
    /// </summary>
    public static double GetWeaponSkillMultiplier(Character character, string weaponType)
    {
        string skillId = weaponType switch
        {
            "OneHanded" => "one-handed",
            "TwoHanded" => "two-handed",
            "Ranged" => "archery",
            _ => null
        };
        
        if (skillId == null || !character.Skills.TryGetValue(skillId, out var skill))
            return 1.0; // No bonus
        
        // +0.5% per rank
        return 1.0 + (skill.CurrentRank * 0.005);
    }
    
    /// <summary>
    /// Calculate spell power bonus from magic school.
    /// </summary>
    public static double GetSpellPowerMultiplier(Character character, string magicSchool)
    {
        string skillId = magicSchool.ToLower(); // "destruction", "restoration", etc.
        
        if (!character.Skills.TryGetValue(skillId, out var skill))
            return 1.0;
        
        // +0.4% per rank
        return 1.0 + (skill.CurrentRank * 0.004);
    }
    
    /// <summary>
    /// Calculate block chance bonus.
    /// </summary>
    public static double GetBlockChanceBonus(Character character)
    {
        if (!character.Skills.TryGetValue("block", out var skill))
            return 0.0;
        
        // +0.3% per rank
        return skill.CurrentRank * 0.003;
    }
    
    /// <summary>
    /// Calculate potion effectiveness bonus.
    /// </summary>
    public static double GetPotionEffectivenessMultiplier(Character character)
    {
        if (!character.Skills.TryGetValue("alchemy", out var skill))
            return 1.0;
        
        // +1% per rank
        return 1.0 + (skill.CurrentRank * 0.01);
    }
}
```

---

## 4. Integration Points

### 4.1 Combat System Integration

**File**: `RealmEngine.Core.Features.Combat.CombatService`

**XP Award Points**:
```csharp
// In ExecutePlayerAction() after successful hit:
if (result.PlayerDamage > 0)
{
    // Award weapon skill XP
    var weaponType = DetermineWeaponType(player);
    await _skillProgressionService.AwardSkillXP(player, weaponType, 5);
    
    if (result.IsCritical)
    {
        await _skillProgressionService.AwardSkillXP(player, weaponType, 15); // Bonus
    }
}

// In ExecutePlayerAction() when defending:
if (action == CombatAction.Defend && result.IsBlocked)
{
    await _skillProgressionService.AwardSkillXP(player, "block", 10);
}

// In ResolveTurn() when enemy dies:
if (enemy.Health <= 0)
{
    var weaponType = DetermineWeaponType(player);
    await _skillProgressionService.AwardSkillXP(player, weaponType, 20);
}
```

**Damage Calculation**:
```csharp
// In CalculateDamage():
var baseDamage = weapon.AttackPower + player.Strength / 2;

// Apply weapon skill multiplier
var weaponType = DetermineWeaponType(player);
var skillMultiplier = SkillEffectCalculator.GetWeaponSkillMultiplier(player, weaponType);

var finalDamage = (int)(baseDamage * skillMultiplier);
```

### 4.2 Magic System Integration (Future)

When magic spells are implemented:
```csharp
// Award Destruction XP when casting offensive spell:
await _skillProgressionService.AwardSkillXP(player, "destruction", 8);

// Apply spell power bonus:
var baseDamage = spell.BaseDamage + player.Intelligence;
var spellPowerMultiplier = SkillEffectCalculator.GetSpellPowerMultiplier(player, "destruction");
var finalDamage = (int)(baseDamage * spellPowerMultiplier);
```

### 4.3 Crafting System Integration (Future)

When crafting is implemented:
```csharp
// Award Alchemy XP when crafting potion:
await _skillProgressionService.AwardSkillXP(player, "alchemy", 15);

// Apply potion effectiveness bonus:
var baseHealing = potion.BaseHealing;
var effectivenessMultiplier = SkillEffectCalculator.GetPotionEffectivenessMultiplier(player);
var actualHealing = (int)(baseHealing * effectivenessMultiplier);
```

### 4.4 Character Initialization

**File**: `RealmEngine.Core.Features.NewGame.NewGameService`

```csharp
// In CreateNewCharacter():
private void InitializeSkills(Character character)
{
    // Load all skills from catalog
    var skillCatalog = _dataCache.GetFile("skills/catalog.json");
    
    // Initialize all skills at Rank 0
    foreach (var skillCategory in skillCatalog.JsonData["skills"])
    {
        foreach (var skillDef in skillCategory.Value)
        {
            var skill = new CharacterSkill
            {
                SkillId = skillDef["id"].ToString(),
                Name = skillDef["name"].ToString(),
                CurrentRank = 0,
                CurrentXP = 0,
                XPToNextRank = skillDef["baseXPCost"].Value<int>(),
                Category = Enum.Parse<SkillCategory>(skillDef["category"].ToString())
            };
            
            character.Skills[skill.SkillId] = skill;
        }
    }
}
```

---

## 5. Service Architecture

### 5.1 SkillProgressionService

**File**: `RealmEngine.Core.Services.SkillProgressionService`

```csharp
namespace RealmEngine.Core.Services;

/// <summary>
/// Manages skill XP awards, rank-ups, and effect calculations.
/// </summary>
public class SkillProgressionService
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<SkillProgressionService> _logger;
    
    public SkillProgressionService(GameDataCache dataCache, ILogger<SkillProgressionService> logger)
    {
        _dataCache = dataCache;
        _logger = logger;
    }
    
    /// <summary>
    /// Award XP to a skill and handle rank-ups.
    /// </summary>
    public async Task<SkillRankUpResult> AwardSkillXP(Character character, string skillId, int xpAmount)
    {
        // Implementation from section 2.3
    }
    
    /// <summary>
    /// Initialize all skills for a new character.
    /// </summary>
    public void InitializeAllSkills(Character character)
    {
        // Load from JSON catalog
    }
    
    /// <summary>
    /// Get skill definition from catalog.
    /// </summary>
    public SkillDefinition GetSkillDefinition(string skillId)
    {
        // Parse from JSON
    }
    
    /// <summary>
    /// Calculate XP needed for next rank.
    /// </summary>
    public int CalculateXPToNextRank(string skillId, int currentRank)
    {
        // Formula from section 2.1
    }
}

/// <summary>
/// Result of skill XP award operation.
/// </summary>
public class SkillRankUpResult
{
    public string SkillId { get; set; }
    public int NewRank { get; set; }
    public int RanksGained { get; set; }
    public List<string> Notifications { get; set; } = new();
    public bool DidRankUp => RanksGained > 0;
}
```

### 5.2 Skill Catalog Service

**File**: `RealmEngine.Data.Services.SkillCatalogService`

```csharp
namespace RealmEngine.Data.Services;

/// <summary>
/// Loads and caches skill definitions from JSON catalog.
/// </summary>
public class SkillCatalogService
{
    private readonly GameDataCache _dataCache;
    private Dictionary<string, SkillDefinition>? _cachedSkills;
    
    public SkillCatalogService(GameDataCache dataCache)
    {
        _dataCache = dataCache;
    }
    
    /// <summary>
    /// Get all skill definitions from catalog.
    /// </summary>
    public List<SkillDefinition> GetAllSkills()
    {
        if (_cachedSkills != null)
            return _cachedSkills.Values.ToList();
        
        LoadSkillCatalog();
        return _cachedSkills!.Values.ToList();
    }
    
    /// <summary>
    /// Get specific skill definition by ID.
    /// </summary>
    public SkillDefinition GetSkillDefinition(string skillId)
    {
        if (_cachedSkills == null)
            LoadSkillCatalog();
        
        return _cachedSkills![skillId];
    }
    
    private void LoadSkillCatalog()
    {
        var catalogFile = _dataCache.GetFile("skills/catalog.json");
        _cachedSkills = new Dictionary<string, SkillDefinition>();
        
        // Parse JSON and populate cache
        // (Implementation details)
    }
}

/// <summary>
/// Skill definition from JSON catalog.
/// </summary>
public class SkillDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SkillCategory Category { get; set; }
    public int MaxRank { get; set; }
    public int BaseXPCost { get; set; }
    public double CostMultiplier { get; set; }
    public string EffectType { get; set; }
    public double EffectValue { get; set; }
    public string EffectDescription { get; set; }
    public List<XPAction> XPActions { get; set; }
}

public class XPAction
{
    public string Action { get; set; }
    public int XPAmount { get; set; }
    public string Description { get; set; }
}
```

---

## 6. Testing Strategy

### 6.1 Unit Tests

**File**: `RealmEngine.Core.Tests.Services.SkillProgressionServiceTests`

```csharp
[Fact]
public void Should_Award_XP_And_Rank_Up()
{
    // Arrange
    var character = TestHelpers.CreateTestCharacter();
    character.Skills["one-handed"] = new CharacterSkill 
    { 
        SkillId = "one-handed",
        CurrentRank = 0, 
        CurrentXP = 80,
        XPToNextRank = 100 
    };
    
    // Act
    var result = _service.AwardSkillXP(character, "one-handed", 25);
    
    // Assert
    result.DidRankUp.Should().BeTrue();
    result.NewRank.Should().Be(1);
    character.Skills["one-handed"].CurrentXP.Should().Be(5); // Overflow
}

[Fact]
public void Should_Calculate_XP_Cost_Correctly()
{
    // Test formula: 100 + (rank * 100 * 0.5)
    _service.CalculateXPToNextRank("one-handed", 0).Should().Be(100);
    _service.CalculateXPToNextRank("one-handed", 10).Should().Be(600);
    _service.CalculateXPToNextRank("one-handed", 50).Should().Be(2600);
}

[Theory]
[InlineData(0, 1.0)]
[InlineData(20, 1.1)]
[InlineData(50, 1.25)]
[InlineData(100, 1.5)]
public void Should_Calculate_Damage_Multiplier(int rank, double expectedMultiplier)
{
    var character = TestHelpers.CreateTestCharacter();
    character.Skills["one-handed"] = new CharacterSkill { CurrentRank = rank };
    
    var multiplier = SkillEffectCalculator.GetWeaponSkillMultiplier(character, "OneHanded");
    
    multiplier.Should().BeApproximately(expectedMultiplier, 0.001);
}
```

### 6.2 Integration Tests

**Test Combat XP Awards**:
```csharp
[Fact]
public async Task Combat_Should_Award_Weapon_Skill_XP()
{
    // Arrange
    var combat = await StartTestCombat();
    var initialXP = combat.Player.Skills["one-handed"].CurrentXP;
    
    // Act
    await combat.ExecutePlayerAction(CombatAction.Attack);
    
    // Assert
    combat.Player.Skills["one-handed"].CurrentXP.Should().BeGreaterThan(initialXP);
}
```

---

## 7. UI/Display Requirements

### 7.1 Character Sheet Display

**API Response Model**:
```csharp
public class CharacterSheetResponse
{
    public List<SkillDisplay> Skills { get; set; }
}

public class SkillDisplay
{
    public string SkillId { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public int CurrentRank { get; set; }
    public int CurrentXP { get; set; }
    public int XPToNextRank { get; set; }
    public double ProgressPercent { get; set; } // CurrentXP / XPToNextRank * 100
    public string CurrentEffect { get; set; } // "+12.5% damage"
    public string NextRankEffect { get; set; } // "+13.0% damage"
}
```

### 7.2 Rank-Up Notifications

**MediatR Notification**:
```csharp
public record SkillRankedUpNotification(
    string SkillId,
    string SkillName,
    int OldRank,
    int NewRank,
    string NewBonusDescription
) : INotification;
```

**Notification Handler** (for Godot UI):
```csharp
public class SkillRankUpNotificationHandler : INotificationHandler<SkillRankedUpNotification>
{
    private readonly IGameUI _gameUI;
    
    public async Task Handle(SkillRankedUpNotification notification, CancellationToken ct)
    {
        _gameUI.ShowSuccess($"🎉 {notification.SkillName} increased to Rank {notification.NewRank}!");
        _gameUI.ShowInfo($"New bonus: {notification.NewBonusDescription}");
    }
}
```

---

## 8. Implementation Steps

### Step 1: Data Models & JSON
- [ ] Create `CharacterSkill` model
- [ ] Update `Character` model with `Skills` dictionary
- [ ] Create `skills/catalog.json` with all 27 skills
- [ ] Create `SkillCatalogService` to load JSON
- [ ] Unit tests for data models

### Step 2: Progression Service
- [ ] Implement `SkillProgressionService`
- [ ] XP award logic with rank-up detection
- [ ] XP cost calculation formula
- [ ] Character initialization with skills
- [ ] Unit tests for progression logic

### Step 3: Effect Calculation
- [ ] Update `SkillEffectCalculator` utility
- [ ] Damage multiplier methods
- [ ] Defense bonus methods
- [ ] Magic power methods
- [ ] Unit tests for effect calculations

### Step 4: Combat Integration
- [ ] Award XP on melee hits
- [ ] Award XP on kills
- [ ] Award XP on blocks
- [ ] Apply skill damage bonuses
- [ ] Integration tests

### Step 5: Notifications & UI APIs
- [ ] `SkillRankedUpNotification` event
- [ ] Character sheet query/response models
- [ ] Skill display formatting
- [ ] Progress tracking

### Step 6: Save/Load Integration
- [ ] Persist skills in save files
- [ ] Load skills on game load
- [ ] Backwards compatibility for old saves
- [ ] Migration tests

---

## 9. Performance Considerations

### 9.1 Skill Lookup Optimization
- Use `Dictionary<string, CharacterSkill>` for O(1) lookup by SkillId
- Cache skill definitions in memory (load once at startup)
- Avoid repeated JSON parsing

### 9.2 XP Award Batching
- Award XP synchronously during combat (already in turn-based loop)
- No need for async batching since actions are sequential
- Consider batching notifications for multiple rank-ups

### 9.3 Memory Usage
- 27 skills per character × ~100 bytes per skill = ~2.7 KB per character
- Acceptable memory footprint for save files

---

## 10. Backwards Compatibility

### 10.1 Save File Migration
Old saves have `List<Skill>` with MaxRank=5. Migration strategy:

```csharp
public class SaveFileMigrationService
{
    public void MigrateSkillsToNewSystem(Character character)
    {
        // If old LearnedSkills list exists, migrate it
        if (character.LearnedSkills.Any() && !character.Skills.Any())
        {
            // Convert old skills to new system
            foreach (var oldSkill in character.LearnedSkills)
            {
                var newSkill = new CharacterSkill
                {
                    SkillId = ConvertOldNameToId(oldSkill.Name),
                    Name = oldSkill.Name,
                    CurrentRank = oldSkill.CurrentRank * 20, // Scale 0-5 to 0-100
                    CurrentXP = 0,
                    XPToNextRank = CalculateXPForRank(oldSkill.CurrentRank * 20)
                };
                
                character.Skills[newSkill.SkillId] = newSkill;
            }
            
            // Clear old list
            character.LearnedSkills.Clear();
        }
        
        // Initialize missing skills at Rank 0
        InitializeMissingSkills(character);
    }
}
```

---

## 11. Future Enhancements

### Advanced Features:
- **Skill Trainers**: NPCs that boost skill XP gain
- **Skill Books**: Consumable items granting instant XP
- **Milestone Perks**: Special abilities at ranks 25/50/75/100
- **Skill Synergies**: Bonuses for matching skill pairs
- **Specialization Trees**: Branch skills into sub-specializations

### Extended Features:
- **Legendary Skills**: Continue past 100 with diminishing returns
- **Skill Challenges**: Timed events testing specific skills
- **Skill Competitions**: Leaderboards for highest skill ranks
- **Prestige System**: Reset skills for permanent bonuses

---

## 12. Success Metrics

### Technical Metrics:
- All 27 skills load from JSON catalog
- XP awards trigger correctly in combat
- Rank-ups calculate accurately
- Damage bonuses apply correctly
- No performance degradation with 27 active skills

### Gameplay Metrics:
- Players reach Rank 20-30 in primary skills by level 20
- Rank 50-60 by level 40
- Rank 80-100 feels achievable but challenging
- Skill bonuses feel impactful (20%+ damage boost noticeable)

---

## 13. Documentation Deliverables

- [ ] This design document
- [ ] API documentation for `SkillProgressionService`
- [ ] JSON catalog schema documentation
- [ ] Integration guide for future features
- [ ] Player-facing skill guide (game manual)

---

## Implementation Readiness

**Design Status**: ✅ Ready for Implementation

**Next Steps**:
1. Review this design document
2. Begin implementation with Data Models & JSON
3. Create feature branch: `feature/skills-system-rebuild`
4. Implement iteratively with TDD approach
