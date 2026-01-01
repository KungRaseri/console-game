# Trait and Ability Systems Design Document

**Date**: December 28, 2025  
**Status**: PROPOSAL - Awaiting User Approval  
**Related Standards**: JSON v4.0, JSON Reference System v4.1

---

## Overview

This document proposes standardized designs for four interconnected game systems:
1. **Enemy Abilities** - Converting hardcoded stats to ability references
2. **Trait System** - Standardizing trait structure and behavior
3. **Item-Granted Powers** - How equipment provides traits and abilities
4. **Consumable Effects** - How potions/scrolls grant temporary powers

---

## 1. Enemy Ability System

### Current State Analysis

**Enemies currently lack ability references:**
```json
{
  "name": "Red Dragon",
  "health": 300,
  "attack": 35,
  "defense": 25,
  "breathType": "fire"
  // NO abilities array!
}
```

**Classes use ability references:**
```json
{
  "name": "Fighter",
  "startingAbilities": [
    "@abilities/active/offensive:basic-attack",
    "@abilities/active/defensive:defend"
  ]
}
```

### Proposed Design: Option A - Direct Ability References

**Add abilities array to enemy definitions:**
```json
{
  "name": "Red Dragon",
  "health": 300,
  "attack": 35,
  "defense": 25,
  "speed": 20,
  "level": 20,
  "xp": 2000,
  "breathType": "fire",
  "rarityWeight": 80,
  "abilities": [
    "@abilities/active/offensive:dragon-breath",
    "@abilities/active/offensive:claw-attack",
    "@abilities/active/defensive:frightful-presence",
    "@abilities/ultimate:ancient-power"
  ]
}
```

**Benefits:**
- ✅ Consistent with class system
- ✅ Enables ability reuse across enemies
- ✅ Clear separation of stats vs abilities
- ✅ Easy to add/remove abilities
- ✅ Supports ability upgrades and variants

**Migration Strategy:**
1. Add `abilities: []` field to all enemy items
2. Convert existing special attacks to ability references
3. Create new ability references for enemy-specific powers
4. Update enemy generator to use ability references

### Proposed Design: Option B - Level-Based Ability Unlocks

**Add progression-based abilities:**
```json
{
  "name": "Dragon Wyrmling",
  "level": 5,
  "abilities": ["@abilities/active/offensive:bite"],
  "abilityUnlocks": {
    "10": ["@abilities/active/offensive:dragon-breath"],
    "15": ["@abilities/active/defensive:frightful-presence"],
    "20": ["@abilities/ultimate:ancient-power"]
  }
}
```

**Benefits:**
- ✅ Enemies scale naturally with level
- ✅ Boss variants have more abilities
- ✅ Clear power progression

**Trade-offs:**
- ⚠️ More complex than flat ability lists
- ⚠️ May not be needed for simple enemies

### ✅ APPROVED: **Option B (Level-Based Ability Unlocks)**
- Enemies scale naturally with level
- Boss variants automatically have more abilities
- Clear power progression for enemy scaling

---

## 2. Trait System Standardization

### Current State Analysis

**Traits are used inconsistently across domains:**

**Classes use typed traits (v4.0 compliant):**
```json
"traits": {
  "weaponProficiency": {"value": "all", "type": "string"},
  "combatBonus": {"value": 10, "type": "number"}
}
```

**Enemies use untyped traits:**
```json
"traits": {
  "category": "dragon",
  "size": "huge",
  "breathWeapon": true
}
```

**Items use untyped traits:**
```json
"traits": {
  "damageType": "slashing",
  "slot": "mainhand",
  "category": "melee_one_handed"
}
```

### Proposed Unified Trait Structure

**All traits MUST use typed values:**
```json
"traits": {
  "traitName": {
    "value": <any>,
    "type": "string" | "number" | "boolean" | "array" | "object",
    "source": "base" | "item" | "ability" | "consumable" | "buff",
    "duration": <number | null>,
    "stackable": <boolean>
  }
}
```

**Field Definitions:**
- `value` - The trait's actual value (required)
- `type` - Data type for validation (required)
- `source` - Where the trait originates (optional, for runtime tracking)
- `duration` - Time in seconds before trait expires (null = permanent)
- `stackable` - Can multiple instances stack (default: false)

### Trait Categories

**1. Static Traits** (permanent, from base definition)
```json
"traits": {
  "size": {"value": "huge", "type": "string"},
  "category": {"value": "dragon", "type": "string"},
  "breathWeapon": {"value": true, "type": "boolean"}
}
```

**2. Dynamic Traits** (temporary, from buffs/debuffs)
```json
"traits": {
  "strengthBonus": {
    "value": 10,
    "type": "number",
    "source": "consumable",
    "duration": 300,
    "stackable": false
  }
}
```

**3. Equipment Traits** (from worn/wielded items)
```json
"traits": {
  "damageBonus": {
    "value": 5,
    "type": "number",
    "source": "item",
    "duration": null,
    "stackable": true
  }
}
```

**4. Ability Traits** (inherent to abilities)
```json
"traits": {
  "damage": {"value": "2d6", "type": "string"},
  "cooldown": {"value": 10, "type": "number"},
  "manaCost": {"value": 20, "type": "number"}
}
```

### Trait Calculation System

**Base + Modifiers Pattern:**
```csharp
// Entity has baseStrength from definition
int baseStrength = 16;

// Calculate total strength from all trait sources
int itemStrength = GetTraitSum("strengthBonus", source: "item");
int buffStrength = GetTraitSum("strengthBonus", source: "consumable");
int abilityStrength = GetTraitSum("strengthBonus", source: "ability");

int totalStrength = baseStrength + itemStrength + buffStrength + abilityStrength;
```

**Stacking Rules:**
- Non-stackable traits: Only highest value applies
- Stackable traits: Sum all values
- Duration expiration: Remove trait when duration reaches 0

### Migration Plan

**Phase 1: Update All Catalogs to Typed Traits**
- Enemies: Add type to all trait values
- Items: Add type to all trait values
- Keep existing trait names for compatibility

**Phase 2: Add Optional Metadata**
- Add `source`, `duration`, `stackable` to runtime trait system
- Catalog files only need `value` and `type`

**Phase 3: Implement Trait Calculation Service**
- Aggregate traits from all sources
- Handle stacking and duration logic
- Provide query methods (GetTrait, GetTraitSum, HasTrait)

---

## 3. Items Providing Traits and Abilities

### Design Philosophy

**Items can grant two types of powers:**
1. **Traits** - Stat bonuses, resistances, passive effects
2. **Abilities** - Active powers usable by the player

### Item Structure with Granted Powers

```json
{
  "name": "Flaming Longsword",
  "damage": "1d8",
  "weight": 3.0,
  "value": 250,
  "rarity": "rare",
  "rarityWeight": 50,
  "traits": {
    "damageType": {"value": "slashing", "type": "string"},
    "slot": {"value": "mainhand", "type": "string"},
    "category": {"value": "melee_one_handed", "type": "string"},
    "skillType": {"value": "blade", "type": "string"}
  },
  "grantedTraits": {
    "damageBonus": {"value": 5, "type": "number"},
    "fireResist": {"value": 25, "type": "number"}
  },
  "grantedAbilities": [
    "@abilities/active/offensive:flame-strike"
  ]
}
```

**Field Breakdown:**
- `traits` - Describes the item itself (slot, category, type)
- `grantedTraits` - Stat bonuses applied when equipped
- `grantedAbilities` - Abilities usable while equipped

### Example Use Cases

**1. Weapon with Bonus Damage**
```json
{
  "name": "Sword of Striking",
  "grantedTraits": {
    "attackBonus": {"value": 10, "type": "number"},
    "critChance": {"value": 5, "type": "number"}
  }
}
```

**2. Armor with Resistances**
```json
{
  "name": "Dragon Scale Mail",
  "grantedTraits": {
    "armorClass": {"value": 18, "type": "number"},
    "fireResist": {"value": 50, "type": "number"},
    "physicalResist": {"value": 25, "type": "number"}
  }
}
```

**3. Magical Item with Ability**
```json
{
  "name": "Staff of Fireballs",
  "grantedAbilities": [
    "@abilities/active/offensive:fireball"
  ],
  "grantedTraits": {
    "spellPower": {"value": 20, "type": "number"}
  }
}
```

**4. Ring with Multiple Bonuses**
```json
{
  "name": "Ring of Protection",
  "grantedTraits": {
    "armorClass": {"value": 2, "type": "number"},
    "saveBonus": {"value": 1, "type": "number"},
    "magicResist": {"value": 10, "type": "number"}
  }
}
```

### Equipment Management Rules

**When Item is Equipped:**
1. Add all `grantedTraits` to character with `source: "item"`
2. Add all `grantedAbilities` to character's ability list
3. Mark abilities as "granted" (removed when unequipped)

**When Item is Unequipped:**
1. Remove all traits with matching `source: "item"` from this item
2. Remove all granted abilities from character

**Multiple Items with Same Trait:**
- If `stackable: true` → Sum values
- If `stackable: false` → Use highest value

---

## 4. Consumable Effect System

### Design Philosophy

**Consumables provide temporary effects:**
- **Immediate**: Instant health/mana restoration
- **Duration-Based**: Temporary stat buffs
- **Ability Grants**: Temporary access to abilities

### Consumable Structure

```json
{
  "name": "Potion of Strength",
  "effect": "buff",
  "power": 10,
  "duration": 300,
  "weight": 0.5,
  "value": 25,
  "rarityWeight": 10,
  "traits": {
    "slot": {"value": "consumable", "type": "string"},
    "category": {"value": "potion", "type": "string"},
    "stackable": {"value": true, "type": "boolean"},
    "oneUse": {"value": true, "type": "boolean"}
  },
  "grantedTraits": {
    "strengthBonus": {
      "value": 10,
      "type": "number",
      "duration": 300,
      "stackable": false
    }
  }
}
```

### Consumable Categories

**1. Healing Consumables** (immediate effect)
```json
{
  "name": "Greater Health Potion",
  "effect": "heal",
  "power": 75,
  "duration": 0,
  "immediateEffect": {
    "healthRestore": 75
  }
}
```

**2. Buff Consumables** (temporary traits)
```json
{
  "name": "Elixir of Might",
  "effect": "buff",
  "duration": 600,
  "grantedTraits": {
    "strengthBonus": {"value": 20, "type": "number", "duration": 600},
    "damageBonus": {"value": 15, "type": "number", "duration": 600}
  }
}
```

**3. Ability Scrolls** (temporary abilities)
```json
{
  "name": "Scroll of Fireball",
  "effect": "ability_grant",
  "duration": 0,
  "oneUse": true,
  "grantedAbilities": [
    "@abilities/active/offensive:fireball"
  ],
  "usageLimit": 1
}
```

**4. Over-Time Effects** (HoT/DoT)
```json
{
  "name": "Regeneration Salve",
  "effect": "heal_overtime",
  "power": 5,
  "duration": 180,
  "tickRate": 3,
  "grantedTraits": {
    "healthRegen": {"value": 5, "type": "number", "duration": 180}
  }
}
```

**5. Multi-Effect Consumables**
```json
{
  "name": "Panacea",
  "effect": "mixed",
  "immediateEffect": {
    "healthRestore": 100,
    "manaRestore": 50,
    "removeCurse": true,
    "curePoison": true
  },
  "grantedTraits": {
    "healthRegen": {"value": 10, "type": "number", "duration": 60},
    "diseaseImmune": {"value": true, "type": "boolean", "duration": 300}
  }
}
```

### Consumable Effect Types

| Effect Type | Description | Example |
|-------------|-------------|---------|
| `heal` | Instant health restoration | Health Potion |
| `restore` | Instant mana restoration | Mana Potion |
| `buff` | Temporary stat boost | Strength Elixir |
| `debuff` | Negative effect (poison, etc.) | Poison Vial |
| `heal_overtime` | Gradual health recovery | Regeneration Salve |
| `cure` | Remove negative effects | Antidote |
| `ability_grant` | Temporary ability access | Spell Scroll |
| `mixed` | Multiple effects | Panacea |

### Stacking and Replacement Rules

**Same Consumable Used Twice:**
```json
// First use: Strength +10 for 300s
// Second use (before expiration):
{
  "stackBehavior": "refresh" // Options: refresh, stack, ignore, replace
}
```

**Stack Behaviors:**
- `refresh` - Reset duration to full, keep same value
- `stack` - Add values, keep longest duration
- `ignore` - No effect if already active
- `replace` - Remove old, apply new

**Example:**
```json
{
  "name": "Potion of Strength",
  "stackBehavior": "refresh", // Default for most buffs
  "grantedTraits": {
    "strengthBonus": {
      "value": 10,
      "type": "number",
      "duration": 300,
      "stackable": false
    }
  }
}
```

---

## Implementation Roadmap

### Phase 1: Trait System Standardization ⚡ HIGH PRIORITY
**Tasks:**
1. Update all enemy catalog.json files to use typed traits
2. Update all item catalog.json files to use typed traits
3. Update JSON v4.0 standards to mandate typed traits
4. Create TraitCalculationService in RealmEngine.Core

**Validation:**
- All catalogs compile
- No runtime errors from trait access

### Phase 2: Enemy Ability Integration ⚡ HIGH PRIORITY
**Tasks:**
1. Add `abilities: []` field to all enemy items
2. Map existing enemy special attacks to ability references
3. Create missing enemy-specific abilities if needed
4. Update enemy generator to handle ability references

**Validation:**
- All enemy abilities resolve correctly
- Enemies can use abilities in combat

### Phase 3: Item Power System 🔥 MEDIUM PRIORITY
**Tasks:**
1. Add `grantedTraits` field to item schemas
2. Add `grantedAbilities` field to item schemas
3. Create EquipmentManager service
4. Implement equip/unequip trait application

**Validation:**
- Equipping items applies traits
- Unequipping items removes traits
- Abilities appear/disappear correctly

### Phase 4: Consumable Effect System 🔥 MEDIUM PRIORITY
**Tasks:**
1. Redesign consumable structure with new fields
2. Implement ConsumableEffectService
3. Add duration tracking and expiration
4. Implement stack behaviors

**Validation:**
- Consumables apply effects correctly
- Duration expires properly
- Stacking rules work as intended

---

## Discussion Questions

### 1. Enemy Abilities
**Q:** Should enemies use:
- **A)** Direct ability arrays (simple, consistent)
- **B)** Level-based unlocks (progression-focused)
- **C)** Both (enemies can have base + unlocks)

### 2. Trait Stacking
**Q:** Default stacking behavior for traits:
- **A)** All traits stackable by default
- **B)** No traits stackable by default
- **C)** Per-trait configuration in catalog

### 3. Item Ability Limits
**Q:** How many abilities can one item grant?
- **A)** Unlimited (powerful artifacts can grant many)
- **B)** 1-3 abilities max (balance concern)
- **C)** Based on item rarity (common=1, legendary=3+)

### 4. Consumable Stacking
**Q:** Default stack behavior for same consumable:
- **A)** Always refresh duration
- **B)** Always stack values
- **C)** Per-consumable configuration

### 5. Scroll Behavior
**Q:** Should scrolls:
- **A)** Grant ability for one use only
- **B)** Grant ability for duration (unlimited uses)
- **C)** ✅ **APPROVED** Grant charges (varies by item - some single, others multiple)

---

## Recommendations Summary

| System | Recommendation | Rationale |
|--------|----------------|-----------|
| **Enemy Abilities** | ✅ Option B - Level-Based Unlocks | Enemy progression, boss scaling |
| **Trait Types** | Mandatory typed traits everywhere | Data validation, consistency |
| **Trait Stacking** | Per-trait `stackable` field | Maximum flexibility |
| **Item Powers** | `grantedTraits` + `grantedAbilities` | Clear separation of concerns |
| **Consumables** | `immediateEffect` + `grantedTraits` | Supports all use cases |
| **Scroll Behavior** | ✅ Charge-Based System | Mix of single-use and multi-charge |

---

## Next Steps

**Please review and provide feedback on:**
1. ✅ Enemy ability system design (Option A vs B)
2. ✅ Trait structure and calculation rules
3. ✅ Item granted powers approach
4. ✅ Consumable effect categories
5. ✅ Stacking and duration behaviors

**Once approved, implementation order:**
1. Trait system standardization (all catalogs)
2. Enemy ability integration (add references)
3. Item power system (equip/unequip logic)
4. Consumable effect system (duration tracking)

---

**Awaiting user approval to proceed with implementation.**
