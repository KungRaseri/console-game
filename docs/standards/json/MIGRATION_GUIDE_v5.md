# JSON v5.0 Migration Guide

**Date**: January 8, 2026  
**From**: v4.0 (object-based traits)  
**To**: v5.0 (trait arrays with universal rarity)

---

## Overview

This guide walks through migrating existing JSON catalogs to v5.0 format.

### Key Changes in v5.0

1. **Trait Arrays**: `{ "level": 3 }` → `[{ "key": "level", "value": 3 }]`
2. **Type Inheritance**: Type-level traits applied to all items
3. **Universal Rarity**: Numerical rarity (1-100) for all entities
4. **Consistent Structure**: Same hierarchy for all domains

---

## Automated Migration

### Using the Python Script

```powershell
# Migrate single file (creates new file with _v5 suffix)
python scripts/migrate-json-to-v5.py RealmEngine.Data/Data/Json/enemies/humanoids/catalog.json

# Migrate all enemy catalogs
python scripts/migrate-json-to-v5.py --all

# Migrate in-place (overwrites original files)
python scripts/migrate-json-to-v5.py --all --in-place
```

### What the Script Does

1. **Reads v4.0 catalog** - Checks metadata version
2. **Converts traits** - Objects → arrays of key-value pairs
3. **Separates fields** - Root-level meta vs traits gameplay data
4. **Adds rarity** - Calculates numerical rarity from rarityWeight
5. **Updates metadata** - Version to 5.0, updates lastUpdated
6. **Writes output** - New file or overwrites original

---

## Manual Migration Steps

### Step 1: Update Metadata

**Before (v4.0):**
```json
{
  "metadata": {
    "description": "Humanoid enemies",
    "version": "4.0",
    "lastUpdated": "2025-12-28",
    "type": "item_catalog"
  }
}
```

**After (v5.0):**
```json
{
  "metadata": {
    "description": "Humanoid enemies",
    "version": "5.0",
    "lastUpdated": "2026-01-08",
    "type": "enemy_catalog",
    "total_types": 1,
    "total_items": 5
  }
}
```

### Step 2: Convert Type-Level Traits

**Before (v4.0):**
```json
"bandits": {
  "traits": {
    "category": "humanoid",
    "size": "medium",
    "strength": 12,
    "dexterity": 14
  },
  "items": [ ... ]
}
```

**After (v5.0):**
```json
"bandits": {
  "traits": [
    { "key": "category", "value": "humanoid" },
    { "key": "size", "value": "medium" },
    { "key": "strength", "value": 12 },
    { "key": "dexterity", "value": 14 }
  ],
  "items": [ ... ]
}
```

### Step 3: Convert Item-Level Traits

**Before (v4.0):**
```json
{
  "slug": "bandit",
  "name": "Bandit",
  "health": 35,
  "attack": 8,
  "defense": 5,
  "level": 3,
  "rarityWeight": 5,
  "abilities": ["@abilities/active/offensive:*"]
}
```

**After (v5.0):**
```json
{
  "slug": "bandit",
  "name": "Bandit",
  "rarity": 15,
  "rarityWeight": 5,
  "traits": [
    { "key": "level", "value": 3 },
    { "key": "health", "value": 35 },
    { "key": "attack", "value": 8 },
    { "key": "defense", "value": 5 },
    { "key": "abilities", "value": ["@abilities/active/offensive:*"] }
  ]
}
```

### Step 4: Add Rarity Field

Map rarityWeight to numerical rarity (1-100):

| rarityWeight | Rarity Value | Tier |
|-------------|--------------|------|
| 1-10 | 15 | Common |
| 11-30 | 30 | Uncommon |
| 31-60 | 50 | Rare |
| 61-85 | 70 | Elite |
| 86-100 | 95 | Legendary |

Example:
- `rarityWeight: 5` → `rarity: 15` (Common)
- `rarityWeight: 40` → `rarity: 50` (Rare)
- `rarityWeight: 95` → `rarity: 95` (Legendary)

---

## Field Classification

### Root-Level Fields (DON'T move to traits)
- `slug` - Unique identifier
- `name` - Display name
- `rarity` - Numerical value (1-100)
- `rarityWeight` - Spawn/selection weight
- `selectionWeight` - Pattern selection weight (names.json)

### Type-Level Traits (Inherited by items)
- `category` - beast, humanoid, undead, etc.
- `size` - tiny, small, medium, large, huge
- `behavior` - pack, solitary, swarm, etc.
- `damageType` - physical, fire, cold, etc.
- `habitat` - forest, cave, dungeon, etc.
- `weaponType` - sword, axe, bow, etc.
- `armorType` - light, medium, heavy

### Item-Level Traits (Gameplay data)
**Stats:**
- `level`, `xp`, `health`, `attack`, `defense`, `speed`

**Attributes:**
- `strength`, `dexterity`, `constitution`, `intelligence`, `wisdom`, `charisma`

**Combat:**
- `abilities`, `abilityUnlocks`, `resistances`, `vulnerabilities`, `immunities`

**Special:**
- `packLeader`, `legendary`, `questBoss`
- `damageTypeOverride`, `sizeOverride`

**Items:**
- `damage`, `defense`, `durability`, `weight`, `value`
- `twoHanded`, `range`, `equipSlot`, `socketCount`

**Consumables:**
- `uses`, `duration`, `stackSize`, `effectType`

---

## Adding Missing Rarity Tiers

Each enemy type should have 5 variants representing all rarity tiers:

```json
"wolves": {
  "traits": [ /* type traits */ ],
  "items": [
    { "slug": "wolf", "rarity": 15 },           // Common
    { "slug": "timber-wolf", "rarity": 30 },    // Uncommon
    { "slug": "frost-wolf", "rarity": 50 },     // Rare
    { "slug": "alpha-dire-wolf", "rarity": 70 }, // Elite
    { "slug": "fenrir", "rarity": 95 }          // Legendary/Boss
  ]
}
```

### Stat Scaling by Rarity

Use stat multipliers from rarity_config.json:

| Tier | Multiplier | Example (Base: 30 HP) |
|------|-----------|----------------------|
| Common | 1.0x | 30 HP |
| Uncommon | 1.3x | 39 HP |
| Rare | 1.7x | 51 HP |
| Elite | 2.5x | 75 HP |
| Legendary | 4.0x | 120 HP |

---

## Validation Checklist

After migration, verify:

### File Structure
- [ ] `metadata.version` = "5.0"
- [ ] `metadata.lastUpdated` = current date
- [ ] `metadata.type` ends with "_catalog"
- [ ] At least one `*_types` object exists

### Type Level
- [ ] `traits` is an array (not object)
- [ ] `items` is an array with at least 1 item
- [ ] Type traits are descriptive (category, size, behavior)

### Item Level
- [ ] Has `slug`, `name`, `rarity`, `rarityWeight` at root
- [ ] Has `traits` array
- [ ] All gameplay data in `traits` array
- [ ] `rarity` is number (1-100), not string
- [ ] References use v4.1 format (`@domain/path:item`)

### Rarity Coverage
- [ ] Common tier (1-20) represented
- [ ] Uncommon tier (21-40) represented
- [ ] Rare tier (41-60) represented
- [ ] Elite tier (61-80) represented
- [ ] Legendary tier (81-100) represented
- [ ] Boss variants for quest objectives

---

## Testing After Migration

```powershell
# Run compliance tests
dotnet test RealmEngine.Data.Tests

# Run specific catalog validation
dotnet test --filter "FullyQualifiedName~CatalogJsonComplianceTests"

# Check reference validation
dotnet test --filter "FullyQualifiedName~ReferenceValidationTests"
```

---

## Common Issues

### Issue 1: Nested Objects in Traits

**Problem:**
```json
"traits": {
  "resistances": { "fire": 50, "cold": 25 }
}
```

**Solution:**
```json
"traits": [
  { "key": "resistances", "value": { "fire": 50, "cold": 25 } }
]
```

Value can be object/array, but trait itself must be key-value pair.

### Issue 2: Abilities at Root Level

**Problem:** Should abilities be in traits or at root?

**Solution:** Abilities stay in traits array:
```json
"traits": [
  { "key": "abilities", "value": ["@abilities/active/offensive:bite"] }
]
```

### Issue 3: Missing Rarity Field

**Problem:** Old items only have rarityWeight, no rarity.

**Solution:** Calculate from rarityWeight using mapping table or script.

### Issue 4: Type Traits Duplicated in Items

**Problem:** Items repeat type-level traits (category, size, etc.)

**Solution:** Remove from items, they inherit from type automatically.

---

## Rollback Plan

If migration causes issues:

1. **Restore from backup**: `git checkout HEAD -- RealmEngine.Data/Data/Json/enemies`
2. **Revert metadata only**: Change version back to 4.0
3. **Hybrid approach**: Keep v4.0 structure, add v5.0 alongside
4. **Fix forward**: Update migration script and re-run

---

## Domain-Specific Notes

### Enemies
- Add boss variants for quest objectives
- Use `questBoss` trait with quest slug
- Ensure 5 rarity tiers per type

### Items (Weapons/Armor)
- Type traits: weaponType, damageType, twoHanded
- Item traits: damage, attack, defense, durability
- Calculate rarity from component weights

### Abilities
- Type traits: category (active/passive), targetType
- Item traits: manaCost, cooldown, damage, effects

### Quests
- May not need traits if simple structure
- Use traits for dynamic rewards/conditions if needed

---

## Next Steps

1. **Run automated migration** on all enemy catalogs
2. **Add missing rarity tiers** to each enemy type
3. **Test with ContentBuilder** to verify UI compatibility
4. **Update item catalogs** (weapons, armor, consumables)
5. **Audit all JSON** for v5.0 compliance
6. **Update code generators** to support trait arrays
7. **Add rarity-based stat scaling** to enemy generation

---

## References

- **v5.0 Standard**: `docs/standards/json/ENEMY_JSON_STANDARD_v5.md`
- **Rarity Config**: `RealmEngine.Data/Data/Json/rarity_config.json`
- **Example Catalog**: `docs/standards/json/EXAMPLE_ENEMY_CATALOG_v5.json`
- **Migration Script**: `scripts/migrate-json-to-v5.py`
