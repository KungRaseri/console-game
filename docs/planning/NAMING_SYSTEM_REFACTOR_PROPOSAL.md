# Naming System Refactor Proposal

**Date**: December 17, 2025  
**Status**: PROPOSED  
**Priority**: HIGH - Architectural Simplification

---

## Current Problems

### 1. **Structural Inconsistency**
**Prefixes/Suffixes** use different structures across files:
- `weapons/prefixes.json` - Has items array with `name`, `traits`, `rarityWeight`
- `weapons/suffixes.json` - Has items array with `name`, `description`, `rarityWeight` (NO traits!)
- `armor/prefixes.json` - Has items array with `name`, `description`, `rarity` (string, not weight!)

**Names** use modern pattern-based system:
- `weapons/names.json` - Has `components` (weight-based) + `patterns`, NO items array
- Uses `metadata.type = "pattern_generation"`

### 2. **Duplication & Overlap**
Many prefix/suffix values overlap with names components:
- **Prefixes**: "Iron", "Steel", "Elven", "Dwarven", "Ancient", "Blessed"
- **Names Components**: "Iron", "Steel", "Elven", "Dwarven" (in material)
- **Names Components**: "Ancient", "Blessed" (in quality/descriptive)

### 3. **Trait Assignment Confusion**
- Prefixes have `traits` objects (weapon damage, durability)
- Suffixes have NO traits, just descriptions
- Names have NO trait assignment at all
- **Where should traits actually live?**

### 4. **Separate Files = Separate UI**
Current structure forces separate editing workflows:
- Edit names.json for base name generation
- Edit prefixes.json for prefix modifiers
- Edit suffixes.json for suffix modifiers
- **Users can't see the full picture in one place**

---

## Proposed Solution: Unified Naming System

### Core Concept
**Merge prefixes, suffixes, and names into a single `names.json` file with components and patterns**

### New Structure

```json
{
  "metadata": {
    "description": "Unified weapon naming with traits",
    "type": "pattern_generation",
    "version": "4.0",
    "componentKeys": ["prefix", "material", "base", "quality", "suffix"],
    "patternTokens": ["prefix", "material", "base", "quality", "suffix"],
    "supportsTraits": true,
    "notes": [
      "Base token resolves from types.json",
      "Prefix/suffix components can have trait modifiers",
      "Traits are applied when component is selected in pattern"
    ]
  },
  "components": {
    "prefix": [
      {
        "value": "Rusty",
        "rarityWeight": 50,
        "traits": {
          "durability": { "value": 50, "type": "number" },
          "damageMultiplier": { "value": 0.8, "type": "number" }
        }
      },
      {
        "value": "Blessed",
        "rarityWeight": 15,
        "traits": {
          "holyDamage": { "value": 5, "type": "number" },
          "damageVsUndead": { "value": 10, "type": "number" }
        }
      },
      {
        "value": "Ancient",
        "rarityWeight": 8,
        "traits": {
          "durability": { "value": 200, "type": "number" },
          "damageBonus": { "value": 10, "type": "number" }
        }
      }
    ],
    "material": [
      {
        "value": "Iron",
        "rarityWeight": 10,
        "traits": {
          "weightMultiplier": { "value": 1.0, "type": "number" },
          "durability": { "value": 100, "type": "number" }
        }
      },
      {
        "value": "Mithril",
        "rarityWeight": 50,
        "traits": {
          "weightMultiplier": { "value": 0.5, "type": "number" },
          "durability": { "value": 300, "type": "number" },
          "damageBonus": { "value": 5, "type": "number" }
        }
      }
    ],
    "quality": [
      {
        "value": "Fine",
        "rarityWeight": 15,
        "traits": {
          "durability": { "value": 120, "type": "number" }
        }
      },
      {
        "value": "Masterwork",
        "rarityWeight": 5,
        "traits": {
          "durability": { "value": 200, "type": "number" },
          "damageBonus": { "value": 3, "type": "number" }
        }
      }
    ],
    "suffix": [
      {
        "value": "of Slaying",
        "rarityWeight": 50,
        "traits": {
          "damageBonus": { "value": 5, "type": "number" }
        }
      },
      {
        "value": "of Fire",
        "rarityWeight": 30,
        "traits": {
          "fireDamage": { "value": 8, "type": "number" },
          "element": { "value": "fire", "type": "string" }
        }
      },
      {
        "value": "of the Phoenix",
        "rarityWeight": 2,
        "traits": {
          "fireDamage": { "value": 20, "type": "number" },
          "lifeSteal": { "value": 5, "type": "number" },
          "durability": { "value": 500, "type": "number" }
        }
      }
    ]
  },
  "patterns": [
    {
      "pattern": "base",
      "weight": 100,
      "example": "Longsword"
    },
    {
      "pattern": "material + base",
      "weight": 50,
      "example": "Iron Longsword"
    },
    {
      "pattern": "prefix + base",
      "weight": 30,
      "example": "Rusty Longsword"
    },
    {
      "pattern": "base + suffix",
      "weight": 30,
      "example": "Longsword of Slaying"
    },
    {
      "pattern": "material + base + suffix",
      "weight": 20,
      "example": "Mithril Longsword of Fire"
    },
    {
      "pattern": "prefix + material + base",
      "weight": 15,
      "example": "Ancient Iron Longsword"
    },
    {
      "pattern": "quality + material + base + suffix",
      "weight": 5,
      "example": "Fine Mithril Longsword of the Phoenix"
    },
    {
      "pattern": "prefix + quality + material + base + suffix",
      "weight": 1,
      "example": "Blessed Masterwork Dragonbone Longsword of the Phoenix"
    }
  ]
}
```

---

## Benefits

### ✅ **1. Architectural Simplification**
- **One file** instead of three (names, prefixes, suffixes)
- **Consistent structure** across all name components
- **Single source of truth** for weapon naming

### ✅ **2. Better UX in ContentBuilder**
- **See everything** in one view (prefix, material, quality, suffix)
- **Edit traits** alongside the component values
- **Test patterns** with all components visible
- **Live examples** show full name generation (prefix + base + suffix)

### ✅ **3. Trait System Integration**
- **Traits live with components** - clear ownership
- **Pattern selects components** → traits are automatically applied
- **Emergent rarity** from combined component weights
- **Emergent stats** from combined component traits

### ✅ **4. Flexibility**
- Same component can be prefix OR material ("Iron" prefix vs "Iron" material)
- Same component can be suffix OR quality ("Masterwork" quality vs "of the Master" suffix)
- **Patterns determine usage**, not separate files

### ✅ **5. Consistency**
- All components use `{ value, rarityWeight, traits }` structure
- No confusion about weight vs rarity strings
- No missing traits on some components

---

## Migration Strategy

### Phase 1: Update Schema
1. Update `PATTERN_COMPONENT_STANDARDS.md` with new unified structure
2. Update `names.json` to include `prefix` and `suffix` components
3. Add `supportsTraits: true` to metadata

### Phase 2: Migrate Data
1. Extract prefix items from `prefixes.json` → add to `components.prefix[]`
2. Extract suffix items from `suffixes.json` → add to `components.suffix[]`
3. Ensure all components have `traits` objects (even if empty: `"traits": {}`)
4. Update patterns to include prefix/suffix usage

### Phase 3: Update Code
1. **Pattern Generator** - Already supports any token name
2. **Trait Application** - Add trait merging logic when resolving patterns
3. **ContentBuilder** - Already handles component groups dynamically
4. **Validators** - Update to allow `prefix`/`suffix` component keys

### Phase 4: Deprecate Old Files
1. Mark `prefixes.json` and `suffixes.json` as deprecated
2. Add migration warning in ContentBuilder if detected
3. Eventually remove old files

---

## Example Usage

### Pattern: `"prefix + material + base + suffix"`
Generated Name: **"Blessed Mithril Longsword of Fire"**

**Applied Traits** (merged from all components):
```json
{
  "holyDamage": 5,           // from "Blessed" prefix
  "damageVsUndead": 10,      // from "Blessed" prefix
  "weightMultiplier": 0.5,   // from "Mithril" material
  "durability": 300,         // from "Mithril" material (overrides lower values)
  "damageBonus": 5,          // from "Mithril" material
  "fireDamage": 8,           // from "of Fire" suffix
  "element": "fire"          // from "of Fire" suffix
}
```

**Emergent Rarity**: 
- Blessed (weight: 15) × Mithril (weight: 50) × of Fire (weight: 30) = **Epic/Legendary tier**

---

## Questions to Answer

### 1. **Should we keep separate component groups?**
**YES** - `prefix`, `material`, `quality`, `suffix` are semantically different
- Helps pattern authoring ("I want prefix + base" vs "I want material + base")
- Allows filtering in ContentBuilder UI
- Makes patterns more readable

### 2. **Can the same value appear in multiple component groups?**
**YES** - e.g., "Ancient" could be both prefix and quality
- Different context, different traits
- "Ancient Sword" (prefix) vs "Ancient-quality Mithril" (quality modifier)

### 3. **How do we handle trait conflicts?**
**Merge Strategy**:
- Numbers: Take **highest value** (e.g., durability: max(100, 200, 300) = 300)
- Strings: **Last wins** (right-to-left in pattern)
- Booleans: **OR** (true if any component has true)
- Arrays: **Concat** unique values

### 4. **Do all components need traits?**
**NO** - Empty traits object is fine
- Some components are purely cosmetic ("of the Warrior" = flavor text)
- ContentBuilder should make traits optional
- Pattern still valid with or without traits

---

## Recommendation

**✅ YES - Proceed with unified naming system**

**Reasoning**:
1. **Current system is broken** - inconsistent structures, duplicate data
2. **New system is simpler** - one file, one structure, one truth
3. **Backward compatible** - can keep old files during migration
4. **Better UX** - ContentBuilder already supports this (component groups + patterns)
5. **More powerful** - traits + patterns = emergent item generation

**Next Steps**:
1. ✅ Get approval on this proposal
2. Update `PATTERN_COMPONENT_STANDARDS.md` documentation
3. Migrate one category (e.g., weapons) as proof-of-concept
4. Test in ContentBuilder + game engine
5. Migrate remaining categories (armor, enemies)

---

## Related Documents
- `docs/standards/PATTERN_COMPONENT_STANDARDS.md` - Pattern syntax rules
- `docs/standards/JSON_FILE_STANDARDS.md` - JSON structure standards
- `docs/implementation/CONTENT_BUILDER_MVP.md` - ContentBuilder feature set
- `RealmEngine.Core/Services/PatternGenerator.cs` - Pattern resolution logic

