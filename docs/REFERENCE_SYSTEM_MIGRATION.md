# Reference System Migration Plan

**Date**: January 6, 2026  
**Version**: 1.1  
**Status**: In Progress - Phase 1

## Decision Log

**January 6, 2026 - User Decisions**:
1. ✅ **Wildcard References**: Implemented - `@skills/weapon:*` for random selection with rarityWeight
2. ✅ **Item-Granted Abilities**: Implement now (not later)
3. ❌ **Skill Requirements**: Items NOT locked behind skill levels (bonuses only, no walls)
4. ✅ **Hard Cutover**: Remove all legacy/obsolete code immediately
5. ✅ **Validation Tests**: Always required
6. ⏳ **Multiple Skill References**: Defer for now
7. ⏳ **Ability Scaling**: Defer for now

## Executive Summary

Migrate from hardcoded string matching and legacy trait systems to a unified JSON v4.1 reference system across all game data. This will eliminate fragile code dependencies and enable true data-driven gameplay.

---

## 1. Current State Analysis

### What's Working ✅
- JSON v4.1 reference syntax defined: `@domain/path/category:item-name`
- ReferenceResolverService exists and passes tests
- Character progression uses new `Skills` dictionary (v4.2)
- Combat system refactored to support trait-based skill resolution

### What's Broken ❌
- Weapons have no skill references (still using legacy `skillType` string)
- Armor has no skill references
- Abilities/Spells not referenced from items
- Consumables don't reference effects or skills
- Character classes still use hardcoded ability lists
- Enemies don't reference abilities they can use

---

## 2. Reference Architecture Design

### 2.1 Skill References

**Purpose**: Link items to the skills that govern their use

**Domains**:
- `@skills/weapon:light-blades` - Weapon proficiency skills
- `@skills/armor:light-armor` - Armor proficiency skills
- `@skills/magic:arcane` - Magic tradition skills
- `@skills/attribute:athletics` - Attribute-based skills
- `@skills/social:persuasion` - Social skills
- `@skills/profession:blacksmithing` - Crafting/profession skills

**Items That Should Reference Skills**:

| Item Type | Skill Reference Example | Use Case |
|-----------|-------------------------|----------|
| Weapons | `@skills/weapon:heavy-blades` | Damage multiplier, XP gain on hit/kill |
| Armor | `@skills/armor:heavy-armor` | Defense bonus, movement penalty reduction |
| Shields | `@skills/defense:block` | Block chance, stamina cost reduction |
| Consumables (Potions) | `@skills/profession:alchemy` | Crafting requirement, effect potency |
| Lockpicks | `@skills/utility:lockpicking` | Success chance modifier |
| Musical Instruments | `@skills/social:performance` | Quality of buff granted |
| Spell Scrolls | `@skills/magic:arcane` (etc.) | Cast success chance, mana cost |

**JSON Structure**:
```json
{
  "slug": "iron-longsword",
  "name": "Iron Longsword",
  "traits": {
    "skillReference": {
      "value": "@skills/weapon:heavy-blades",
      "type": "string"
    },
    "damage": { "value": "1d8", "type": "string" }
  }
}
```

### 2.2 Ability References

**Purpose**: Link items, classes, and enemies to abilities they can use

**Domains**:
- `@abilities/active:...` - Active abilities requiring activation
- `@abilities/passive:...` - Always-on passive bonuses
- `@abilities/reactive:...` - Triggered by conditions
- `@abilities/ultimate:...` - Powerful cooldown abilities

**Items That Should Reference Abilities**:

| Item Type | Ability Reference Example | Use Case |
|-----------|---------------------------|----------|
| Weapons (Enchanted) | `@abilities/active:flame-strike` | Activate to deal fire damage |
| Armor Sets | `@abilities/passive:iron-skin` | Grants passive defense when full set worn |
| Rings/Amulets | `@abilities/reactive:shield-barrier` | Auto-cast when health drops below 30% |
| Quest Items | `@abilities/active:detect-magic` | Special detection ability |
| Class Definitions | `[@abilities/active:charge, @abilities/passive:armor-mastery]` | Starting abilities for class |

**JSON Structure (Weapon with Ability)**:
```json
{
  "slug": "flaming-sword",
  "name": "Flaming Sword",
  "traits": {
    "skillReference": {
      "value": "@skills/weapon:heavy-blades",
      "type": "string"
    },
    "grantedAbilities": {
      "value": ["@abilities/active:flame-strike"],
      "type": "array"
    }
  }
}
```

**JSON Structure (Class with Abilities)**:
```json
{
  "slug": "warrior",
  "name": "Warrior",
  "traits": {
    "startingAbilities": {
      "value": [
        "@abilities/active:power-attack",
        "@abilities/passive:weapon-mastery",
        "@abilities/reactive:defensive-stance"
      ],
      "type": "array"
    }
  }
}
```

### 2.3 Spell References

**Purpose**: Link items and classes to spells they can cast

**Domains**:
- `@spells/arcane:...` - Arcane tradition spells
- `@spells/divine:...` - Divine tradition spells
- `@spells/occult:...` - Occult tradition spells
- `@spells/primal:...` - Primal tradition spells

**Items That Should Reference Spells**:

| Item Type | Spell Reference Example | Use Case |
|-----------|-------------------------|----------|
| Spell Scrolls | `@spells/arcane:fireball` | Single-use spell cast |
| Wands/Staves | `@spells/arcane:magic-missile` | Reusable spell channel |
| Spell Books | `[@spells/arcane:shield, @spells/arcane:detect-magic]` | Learn spells when used |
| Class Spellbooks | `@spells/divine:*` (wildcard) | Access to all divine spells |

**JSON Structure (Spell Scroll)**:
```json
{
  "slug": "scroll-of-fireball",
  "name": "Scroll of Fireball",
  "traits": {
    "spellReference": {
      "value": "@spells/arcane:fireball",
      "type": "string"
    },
    "requiredSkill": {
      "value": "@skills/magic:arcane",
      "type": "string"
    },
    "minimumSkillRank": {
      "value": 25,
      "type": "number"
    },
    "consumable": {
      "value": true,
      "type": "boolean"
    }
  }
}
```

### 2.4 Material References

**Purpose**: Link crafting recipes and enhancements to materials

**Domains**:
- `@materials/metals:...` - Metal materials for weapons/armor
- `@materials/leather:...` - Leather for light armor
- `@materials/cloth:...` - Cloth for robes
- `@materials/wood:...` - Wood for bows/staves
- `@materials/gems:...` - Gems for socketing
- `@materials/reagents:...` - Spell/potion components

### 2.5 Effect References

**Purpose**: Link consumables and abilities to their effects

**Domains**:
- `@effects/buff:...` - Temporary stat increases
- `@effects/debuff:...` - Temporary stat decreases
- `@effects/heal:...` - Healing effects
- `@effects/damage:...` - Direct damage
- `@effects/dot:...` - Damage over time
- `@effects/hot:...` - Healing over time

---

## 3. Migration Plan by Domain

### Phase 1: Weapons (High Priority) 🔴

**Files to Update**: ~70 weapon catalog files

**Changes Required**:
1. Add `skillReference` trait to all weapon type categories
2. Remove legacy `skillType` string values
3. Map existing skill types to proper references:
   - `"blade"` → `@skills/weapon:heavy-blades` or `@skills/weapon:light-blades`
   - `"axe"` → `@skills/weapon:axes`
   - `"bow"` → `@skills/weapon:bows`
   - etc.

**Example Migration**:
```json
// BEFORE (current state)
"weapon_types": {
  "swords": {
    "traits": {
      "damageType": "slashing",
      "skillType": "blade"  // ❌ Legacy string
    }
  }
}

// AFTER (with references)
"weapon_types": {
  "swords": {
    "traits": {
      "damageType": "slashing",
      "skillReference": {
        "value": "@skills/weapon:heavy-blades",
        "type": "string"
      }
    }
  }
}
```

**Code Changes**:
- ✅ `SkillEffectCalculator.GetSkillSlugFromItem()` already supports this
- ✅ `CombatServiceExtensions.GetEquippedWeaponSkillSlug()` already uses it
- ⚠️ Need to verify all weapon generation code uses traits correctly

**Validation**:
- All weapons must have `skillReference` trait
- References must resolve to valid skills in `skills/catalog.json`
- Combat XP awards must work with new references

### Phase 2: Armor (High Priority) 🔴

**Files to Update**: ~40 armor catalog files

**Changes Required**:
1. Add `skillReference` trait to armor types
2. Add `armorClass` reference (light/medium/heavy)
3. Link to relevant skills:
   - Light Armor → `@skills/armor:light-armor`
   - Medium Armor → `@skills/armor:medium-armor`
   - Heavy Armor → `@skills/armor:heavy-armor`
   - Shields → `@skills/defense:block`

**Example**:
```json
"armor_types": {
  "leather": {
    "traits": {
      "armorClass": "light",
      "skillReference": {
        "value": "@skills/armor:light-armor",
        "type": "string"
      },
      "defense": { "value": 3, "type": "number" }
    }
  }
}
```

**Code Changes**:
- Create `GetArmorSkillSlug()` extension method (similar to weapons)
- Update `GetPhysicalDefenseMultiplier()` to accept armor skill slug
- Add armor skill XP awards when taking damage
- Update equipment system to check armor skill requirements

### Phase 3: Consumables (Medium Priority) 🟡

**Files to Update**: ~30 consumable catalog files

**Changes Required**:
1. Add `effectReference` for what the consumable does
2. Add `skillReference` for crafting requirement (alchemy, cooking, etc.)
3. Link potions to healing/buff effects

**Example**:
```json
{
  "slug": "health-potion-minor",
  "name": "Minor Health Potion",
  "traits": {
    "effectReference": {
      "value": "@effects/heal:health-restoration",
      "type": "string"
    },
    "healAmount": {
      "value": 25,
      "type": "number"
    },
    "requiredSkill": {
      "value": "@skills/profession:alchemy",
      "type": "string"
    },
    "craftingDifficulty": {
      "value": 10,
      "type": "number"
    }
  }
}
```

### Phase 4: Classes (Medium Priority) 🟡

**Files to Update**: `classes/catalog.json`

**Changes Required**:
1. Convert ability arrays to reference arrays
2. Add starting skill bonuses as references
3. Link class restrictions to skill/item references

**Example**:
```json
{
  "slug": "warrior",
  "name": "Warrior",
  "traits": {
    "startingAbilities": {
      "value": [
        "@abilities/active:power-attack",
        "@abilities/passive:armor-mastery",
        "@abilities/reactive:second-wind"
      ],
      "type": "array"
    },
    "skillBonuses": {
      "value": {
        "@skills/weapon:heavy-blades": 10,
        "@skills/armor:heavy-armor": 10,
        "@skills/defense:block": 5
      },
      "type": "object"
    },
    "allowedWeaponSkills": {
      "value": [
        "@skills/weapon:heavy-blades",
        "@skills/weapon:axes",
        "@skills/weapon:polearms"
      ],
      "type": "array"
    }
  }
}
```

**Code Changes**:
- Update class loading to resolve ability references
- Create `InitializeClassAbilities()` command
- Update character creation to apply skill bonuses from references
- Add equipment restriction validation based on allowed skill references

### Phase 5: Enemies (Medium Priority) 🟡

**Files to Update**: ~50 enemy catalog files

**Changes Required**:
1. Replace hardcoded ability names with references
2. Add skill proficiencies for enemy AI
3. Link loot tables to item references

**Example**:
```json
{
  "slug": "goblin-warrior",
  "name": "Goblin Warrior",
  "traits": {
    "abilities": {
      "value": [
        "@abilities/active:basic-attack",
        "@abilities/reactive:dodge-roll"
      ],
      "type": "array"
    },
    "weaponSkills": {
      "value": {
        "@skills/weapon:light-blades": 30,
        "@skills/weapon:bows": 25
      },
      "type": "object"
    },
    "lootTable": {
      "value": [
        "@items/weapons:dagger",
        "@items/consumables:health-potion-minor"
      ],
      "type": "array"
    }
  }
}
```

### Phase 6: Spell Scrolls & Magic Items (Low Priority) 🟢

**Files to Update**: ~20 magic item catalog files

**Changes Required**:
1. Add spell references to scrolls/wands
2. Link to magic tradition skills
3. Add minimum skill requirements

### Phase 7: Crafting Recipes (Low Priority) 🟢

**Files to Update**: Future crafting system

**Changes Required**:
1. Material references for ingredients
2. Skill references for crafting requirements
3. Result item references

---

## 4. Code Infrastructure Changes

### 4.1 Item Loading System

**Create**: `ItemFactory.cs` with reference resolution
```csharp
public class ItemFactory
{
    private readonly ReferenceResolverService _referenceResolver;
    
    public async Task<Item> CreateFromCatalog(JObject json)
    {
        var item = ParseBaseItem(json);
        
        // Resolve skill reference if present
        if (json["traits"]?["skillReference"] != null)
        {
            var skillRef = json["traits"]["skillReference"]["value"].ToString();
            var skill = await _referenceResolver.ResolveReference<Skill>(skillRef);
            // Validate skill exists and cache for quick access
        }
        
        // Resolve ability references if present
        if (json["traits"]?["grantedAbilities"] != null)
        {
            var abilityRefs = json["traits"]["grantedAbilities"]["value"].ToObject<List<string>>();
            foreach (var abilityRef in abilityRefs)
            {
                var ability = await _referenceResolver.ResolveReference<Ability>(abilityRef);
                item.GrantedAbilities.Add(ability);
            }
        }
        
        return item;
    }
}
```

### 4.2 Equipment System Updates

**Update**: `Character` model
```csharp
public class Character
{
    // Add collection for active granted abilities from equipment
    public List<Ability> EquipmentAbilities { get; set; } = new();
    
    // Add method to recalculate when equipment changes
    public void RecalculateEquipmentBonuses()
    {
        EquipmentAbilities.Clear();
        
        foreach (var item in GetAllEquippedItems())
        {
            if (item.GrantedAbilities?.Any() == true)
            {
                EquipmentAbilities.AddRange(item.GrantedAbilities);
            }
        }
    }
}
```

**Create**: `EquipmentValidator.cs`
```csharp
public class EquipmentValidator
{
    public bool CanEquip(Character character, Item item)
    {
        // Check skill requirement from reference
        var skillSlug = SkillEffectCalculator.GetSkillSlugFromItem(item);
        if (skillSlug != null)
        {
            if (!character.Skills.TryGetValue(skillSlug, out var skill))
                return false; // Don't have the skill at all
                
            if (item.Traits.TryGetValue("minimumSkillRank", out var minRank))
            {
                if (skill.CurrentRank < minRank.AsInt())
                    return false; // Skill rank too low
            }
        }
        
        return true;
    }
}
```

### 4.3 Combat System Updates

**Update**: `CombatService.cs`
- ✅ Already awards weapon skill XP
- ⏳ Add armor skill XP on damage taken
- ⏳ Add ability usage integration
- ⏳ Check skill requirements before allowing actions

### 4.4 Ability System Integration

**Create**: `AbilityActivationService.cs`
```csharp
public class AbilityActivationService
{
    public async Task<AbilityResult> ActivateAbility(Character character, string abilityId)
    {
        // Check if ability is from class, equipment, or learned
        var ability = character.GetAbility(abilityId);
        
        // Check cooldown
        if (character.AbilityCooldowns.TryGetValue(abilityId, out var cooldown) && cooldown > 0)
            return AbilityResult.OnCooldown;
            
        // Execute ability
        var result = await ExecuteAbility(character, ability);
        
        // Set cooldown
        character.AbilityCooldowns[abilityId] = ability.CooldownTurns;
        
        // Award XP to related skill if ability is skill-based
        if (ability.RelatedSkill != null)
        {
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = character,
                SkillId = ability.RelatedSkill,
                XPAmount = 10,
                ActionSource = $"used_ability_{abilityId}"
            });
        }
        
        return result;
    }
}
```

---

## 5. Data Validation & Testing

### 5.1 Reference Integrity Tests

**Create**: `ReferenceIntegrityTests.cs`
```csharp
[Fact]
public async Task All_Weapon_Skill_References_Should_Resolve()
{
    var weapons = await LoadAllWeapons();
    var skills = await LoadAllSkills();
    
    foreach (var weapon in weapons)
    {
        var skillSlug = SkillEffectCalculator.GetSkillSlugFromItem(weapon);
        Assert.NotNull(skillSlug);
        Assert.Contains(skills, s => s.Slug == skillSlug);
    }
}

[Fact]
public async Task All_Class_Ability_References_Should_Resolve()
{
    var classes = await LoadAllClasses();
    var abilities = await LoadAllAbilities();
    
    foreach (var characterClass in classes)
    {
        var abilityRefs = characterClass.GetStartingAbilities();
        foreach (var abilityRef in abilityRefs)
        {
            var slug = ExtractSlugFromReference(abilityRef);
            Assert.Contains(abilities, a => a.Slug == slug);
        }
    }
}
```

### 5.2 Migration Validation Script

**Create**: `scripts/validate-references.ps1`
```powershell
# Scan all JSON files for references
# Verify all references resolve
# Report any broken references
# Suggest fixes for common issues
```

---

## 6. Migration Execution Plan

### Week 1: Weapons
- [ ] Day 1-2: Update weapon JSON files with skillReference
- [ ] Day 3: Test weapon skill XP in combat
- [ ] Day 4: Fix any broken references
- [ ] Day 5: Update weapon generation code

### Week 2: Armor
- [ ] Day 1-2: Update armor JSON files
- [ ] Day 3: Implement armor skill XP system
- [ ] Day 4-5: Test equipment restrictions

### Week 3: Classes & Abilities
- [ ] Day 1-2: Update class definitions
- [ ] Day 3-4: Implement ability granting system
- [ ] Day 5: Test class progression

### Week 4: Consumables & Polish
- [ ] Day 1-2: Update consumable JSON
- [ ] Day 3: Update enemy definitions
- [ ] Day 4-5: Comprehensive testing and bug fixes

---

## 7. Benefits & Impact

### Developer Benefits ✨
- No more hardcoded weapon/skill mappings
- Easy to add new skills without code changes
- Item abilities just work™ when equipped
- Class design is purely data-driven

### Player Benefits 🎮
- Equipment restrictions based on actual skill levels
- Items that grant abilities are obvious and clear
- Skill progression feels meaningful (unlock better gear)
- Hybrid builds possible (equipment + class abilities)

### Maintenance Benefits 🛠️
- Single source of truth for all game data
- JSON validation catches errors early
- Changes don't require recompilation
- Modding support becomes trivial

---

## 8. Risk Analysis & Mitigation

### Risk 1: Data Migration Errors
**Probability**: High  
**Impact**: High  
**Mitigation**: 
- Automated validation scripts
- Reference integrity tests
- Manual QA pass on each domain

### Risk 2: Performance Impact
**Probability**: Medium  
**Impact**: Low  
**Mitigation**:
- Cache resolved references
- Lazy-load referenced entities
- Profile critical paths

### Risk 3: Breaking Existing Saves
**Probability**: Medium  
**Impact**: High  
**Mitigation**:
- Save file migration utility
- Backwards compatibility for legacy traits
- Version stamping on all data

---

## 9. Next Steps

### Immediate Actions
1. ✅ Create this migration plan document
2. ⏳ Review and approve plan with team
3. ⏳ Create validation tests for Phase 1 (weapons)
4. ⏳ Start weapon JSON migration
5. ⏳ Implement `ItemFactory` with reference resolution

### Discussion Points
- Should we support multiple skill references per item? (e.g., bastard sword = one-handed OR two-handed)
- How to handle skill requirements for consumables? (alchemy skill for potions)
- Should abilities have skill requirements to use? (martial ability needs combat training)
- Wildcard reference support? (class gets all spells in tradition: `@spells/arcane:*`)

---

## 10. Open Questions

1. **Multiple Skill References**: Should items support multiple valid skills?
   - Example: Bastard sword can use either one-handed or two-handed skill
   - Solution: Array of references with OR logic?

2. **Skill Requirement Levels**: Should items have minimum AND maximum skill levels?
   - Example: Training sword only usable at skill ranks 0-20
   - Use case: Force progression to better weapons

3. **Dynamic Ability Scaling**: Should granted abilities scale with skill level?
   - Example: Flaming sword's fire damage increases with weapon skill
   - Implementation: Formula in ability definition?

4. **Reference Wildcards**: Support for `@skills/weapon:*` meaning "any weapon skill"?
   - Use case: Jack-of-all-trades class bonus
   - Complexity: Wildcard resolution in ReferenceResolverService

5. **Circular References**: How to prevent? Example: 
   - Ability references skill
   - Skill references ability for unlock
   - Detection needed?

---

## Appendix A: Reference Syntax Quick Reference

```
General Format:
@domain/category/subcategory:item-slug[filter]?.property.nested

Examples:
@skills/weapon:heavy-blades              # Simple reference
@abilities/active:power-attack           # Ability reference
@spells/arcane:fireball                  # Spell reference
@items/weapons:longsword                 # Item reference
@materials/metals:steel                  # Material reference
@effects/buff:strength-boost             # Effect reference
@skills/weapon:*                         # Wildcard (all weapon skills)
@skills/weapon:heavy-blades?.bonuses     # Optional property access
```

---

## Appendix B: File Impact Matrix

| Domain | Files Affected | Lines Changed | Test Files | Priority |
|--------|----------------|---------------|------------|----------|
| Weapons | 70 | ~2100 | 15 | 🔴 High |
| Armor | 40 | ~1200 | 10 | 🔴 High |
| Classes | 1 | ~300 | 5 | 🟡 Medium |
| Enemies | 50 | ~1500 | 12 | 🟡 Medium |
| Consumables | 30 | ~900 | 8 | 🟡 Medium |
| Abilities | 4 | ~400 | 20 | 🟢 Low |
| Spells | 1 | ~150 | 10 | 🟢 Low |
| **TOTAL** | **196** | **~6550** | **80** | |

---

**Document Owner**: Development Team  
**Last Updated**: January 6, 2026  
**Next Review**: After Phase 1 Completion
