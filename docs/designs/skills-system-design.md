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

**Current State**: Skills system with JSON v4.2 catalog containing 54 skills organized into 5 categories (Attribute, Weapon, Armor, Magic, Profession). Skills rank 0-100 with XP-based progression.

**Implementation Status**: 
- ✅ JSON catalog structure complete (skills/catalog.json)
- ✅ 54 skills defined with traits, effects, xpActions
- ✅ v4.2 data standards applied
- ⏳ Code implementation pending
- ⏳ Service layer for skill progression
- ⏳ UI integration

**Scope**: Skills progression system implementation
- Character model integration for skill storage
- Skill XP award system triggered by gameplay actions
- Rank-up calculation and notification system
- Integration with combat, magic, crafting, exploration
- Character sheet display with progress tracking
- Data loading from JSON catalog

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

**File**: `RealmEngine.Data/Data/Json/skills/catalog.json` ✅ IMPLEMENTED

**Structure** (JSON v4.2):
```json
{
  "metadata": {
    "version": "4.2",
    "lastUpdated": "2026-01-06",
    "description": "Master catalog of all skills (54 total)",
    "type": "skills_catalog",
    "totalSkills": 54
  },
  "attribute_strength": {
    "description": "Skills governed by Strength attribute",
    "skills": [{
      "slug": "athletics",
      "name": "Athletics",
      "displayName": "Athletics",
      "description": "Running, jumping, general physical activity",
      "traits": {
        "baseXPCost": {"value": 80, "type": "number"},
        "costMultiplier": {"value": 0.4, "type": "number"},
        "maxRank": {"value": 100, "type": "number"},
        "governingAttribute": {"value": "strength", "type": "string"},
        "effects": {
          "value": [
            {"effectType": "movement_speed", "effectValue": 0.002, "appliesTo": "movement"},
            {"effectType": "jump_height", "effectValue": 0.01, "appliesTo": "jump"}
          ],
          "type": "array"
        },
        "xpActions": {
          "value": [
            {"action": "sprint", "xpAmount": 2},
            {"action": "jump_distance", "xpAmount": 5},
            {"action": "complete_physical_challenge", "xpAmount": 15}
          ],
          "type": "array"
        }
      }
    }]
  },
  "weapon": {
    "description": "Weapon proficiency skills",
    "skills": [{
      "slug": "light-blades",
      "name": "Light Blades",
      "displayName": "Light Blades",
      "description": "Daggers, short swords, rapiers - fast DEX-focused weapons",
      "traits": {
        "baseXPCost": {"value": 100, "type": "number"},
        "costMultiplier": {"value": 0.5, "type": "number"},
        "maxRank": {"value": 100, "type": "number"},
        "governingAttribute": {"value": "dexterity", "type": "string"},
        "effects": {
          "value": [
            {"effectType": "damage_multiplier", "effectValue": 0.005, "appliesTo": "light_blade_damage"},
            {"effectType": "critical_chance", "effectValue": 0.002, "appliesTo": "light_blade_critical"}
          ],
          "type": "array"
        },
        "xpActions": {
          "value": [
            {"action": "light_blade_hit", "xpAmount": 5},
            {"action": "light_blade_kill", "xpAmount": 20},
            {"action": "light_blade_critical", "xpAmount": 15}
          ],
          "type": "array"
        }
      }
    }]
  },
  "magic_arcane": {
    "description": "Arcane tradition magic skills",
    "skills": [{
      "slug": "arcane",
      "name": "Arcane",
      "displayName": "Arcane",
      "description": "Core Arcane tradition - unlocks all Arcane spells",
      "traits": {
        "baseXPCost": {"value": 120, "type": "number"},
        "costMultiplier": {"value": 0.6, "type": "number"},
        "maxRank": {"value": 100, "type": "number"},
        "governingAttribute": {"value": "intelligence", "type": "string"},
        "magicTradition": {"value": "arcane", "type": "string"},
        "effects": {
          "value": [
            {"effectType": "spell_damage", "effectValue": 0.004, "appliesTo": "arcane_spells"},
            {"effectType": "spell_critical_chance", "effectValue": 0.003, "appliesTo": "arcane_spells"}
          ],
          "type": "array"
        },
        "xpActions": {
          "value": [
            {"action": "cast_arcane_spell", "xpAmount": 8},
            {"action": "arcane_spell_hit", "xpAmount": 10},
            {"action": "arcane_spell_kill", "xpAmount": 25}
          ],
          "type": "array"
        }
      }
    }]
  }
}
```

**Implemented Categories**:
- `attribute_strength`, `attribute_dexterity`, `attribute_constitution`, `attribute_intelligence`, `attribute_wisdom`, `attribute_charisma` (24 skills)
- `weapon` (10 skills: light-blades, heavy-blades, axes, blunt, polearms, bows, crossbows, throwing, unarmed, shield)
- `armor` (4 skills: light-armor, medium-armor, heavy-armor, unarmored-defense)
- `magic_arcane`, `magic_divine`, `magic_occult`, `magic_primal` (16 skills: 4 core + 12 specialists)
- `profession` (12 skills: blacksmithing, leatherworking, tailoring, etc.)

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
