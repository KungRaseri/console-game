# Naming System Migration Guide (v3.0 → v4.0)

**Date**: December 17, 2025  
**Migration Type**: Structural Refactor (Consolidation)  
**Impact**: High - Replaces 3 files with 1 unified file

---

## Overview

This guide helps migrate from the **legacy separate file structure** (v3.0) to the **unified naming system** (v4.0).

### What's Changing?

**BEFORE (v3.0 - Legacy)**:
- `names.json` - Base name components and patterns
- `prefixes.json` - Prefix modifiers with traits
- `suffixes.json` - Suffix modifiers (often NO traits!)
- **3 separate files** = 3 separate editing sessions

**AFTER (v4.0 - Unified)**:
- `names.json` - ALL components (prefix, material, quality, suffix) with traits and patterns
- **1 unified file** = see everything together

---

## Migration Steps

### Step 1: Backup Original Files

```powershell
# Navigate to category folder (e.g., items/weapons)
cd RealmEngine.Shared/Data/Json/items/weapons

# Create backup folder
mkdir backup_v3
Copy-Item names.json backup_v3/
Copy-Item prefixes.json backup_v3/
Copy-Item suffixes.json backup_v3/
```

### Step 2: Extract Prefix Data

Open `prefixes.json` and convert items to component format:

**From** (prefixes.json):
```json
{
  "items": [
    {
      "name": "Rusty",
      "displayName": "Rusty",
      "rarityWeight": 50,
      "traits": {
        "durability": { "value": 50, "type": "number" },
        "damageMultiplier": { "value": 0.8, "type": "number" }
      }
    }
  ]
}
```

**To** (names.json v4.0):
```json
{
  "components": {
    "prefix": [
      {
        "value": "Rusty",
        "rarityWeight": 50,
        "traits": {
          "durability": { "value": 50, "type": "number" },
          "damageMultiplier": { "value": 0.8, "type": "number" }
        }
      }
    ]
  }
}
```

**Conversion Rules**:
- `name` or `displayName` → `value`
- Keep `rarityWeight` as-is
- Keep `traits` object as-is (if present)
- **If traits are missing**, add empty object: `"traits": {}`

### Step 3: Extract Suffix Data

Open `suffixes.json` and convert items:

**From** (suffixes.json):
```json
{
  "items": [
    {
      "name": "of Slaying",
      "displayName": "of Slaying",
      "rarityWeight": 50,
      "description": "Deals extra damage to all foes"
    }
  ]
}
```

**To** (names.json v4.0):
```json
{
  "components": {
    "suffix": [
      {
        "value": "of Slaying",
        "rarityWeight": 50,
        "traits": {
          "damageBonus": { "value": 5, "type": "number" }
        }
      }
    ]
  }
}
```

**Conversion Rules**:
- `name` or `displayName` → `value`
- Keep `rarityWeight` as-is
- **ADD traits based on description** (many suffixes have descriptions but NO traits!)
- Use semantic trait names: `damageBonus`, `fireDamage`, `lifeSteal`, etc.

**Common Suffix → Trait Mappings**:
| Suffix | Suggested Traits |
|--------|------------------|
| of Slaying | `damageBonus: 5` |
| of Sharpness | `criticalChance: 8, damageBonus: 3` |
| of Power | `damageMultiplier: 1.15, strengthBonus: 1` |
| of Speed | `attackSpeed: 1.2, dexterityBonus: 2` |
| of Fire | `fireDamage: 8, element: "fire"` |
| of Ice | `iceDamage: 8, element: "ice", slowEffect: true` |
| of Lightning | `lightningDamage: 9, element: "lightning"` |
| of the Warrior | `strengthBonus: 2, damageBonus: 4` |
| of the Hero | `allStatsBonus: 1, damageBonus: 6, healthBonus: 10` |
| of Life Stealing | `lifeSteal: 8, damageBonus: 6` |

### Step 4: Update Existing Material Components

If existing `names.json` material components don't have traits, add them:

**From** (names.json v3.0):
```json
{
  "components": {
    "material": [
      {
        "value": "Iron",
        "rarityWeight": 10
      }
    ]
  }
}
```

**To** (names.json v4.0):
```json
{
  "components": {
    "material": [
      {
        "value": "Iron",
        "rarityWeight": 10,
        "traits": {
          "weightMultiplier": { "value": 1.0, "type": "number" },
          "damageBonus": { "value": 2, "type": "number" },
          "durability": { "value": 100, "type": "number" }
        }
      }
    ]
  }
}
```

**Common Material → Trait Mappings**:
| Material | Suggested Traits |
|----------|------------------|
| Iron | `weightMultiplier: 1.0, damageBonus: 2, durability: 100` |
| Steel | `damageBonus: 3, criticalChance: 5, durability: 120` |
| Silver | `damageVsUndead: 10, damageVsDemonic: 10, durability: 90` |
| Mithril | `weightMultiplier: 0.7, damageBonus: 5, durability: 150, glowEffect: true` |
| Adamantine | `damageBonus: 8, armorPiercing: 20, durability: 250` |
| Dragonbone | `damageBonus: 12, resistFire: 30, damageMultiplier: 1.2, durability: 300` |

### Step 5: Add Patterns Using Prefix/Suffix

Update patterns to include new component keys:

**Add these new patterns**:
```json
{
  "patterns": [
    { "pattern": "prefix + base", "weight": 30, "example": "Rusty Longsword" },
    { "pattern": "base + suffix", "weight": 30, "example": "Longsword of Slaying" },
    { "pattern": "prefix + material + base", "weight": 10, "example": "Blessed Mithril Longsword" },
    { "pattern": "material + base + suffix", "weight": 20, "example": "Mithril Longsword of Fire" },
    { "pattern": "prefix + base + suffix", "weight": 8, "example": "Ancient Longsword of the Dragon" },
    { "pattern": "prefix + material + base + suffix", "weight": 3, "example": "Blessed Dragonbone Longsword of the Phoenix" }
  ]
}
```

### Step 6: Update Metadata

Update metadata to reflect v4.0 schema:

```json
{
  "metadata": {
    "description": "Unified weapon naming system with prefix/suffix support and trait assignment (v4.0)",
    "version": "4.0",
    "lastUpdated": "2025-12-17",
    "type": "pattern_generation",
    "supportsTraits": true,
    "componentKeys": ["prefix", "material", "quality", "descriptive", "suffix"],
    "patternTokens": ["base", "prefix", "material", "quality", "descriptive", "suffix"],
    "totalPatterns": 18,
    "notes": [
      "Base token resolves from items/weapons/types.json",
      "Prefixes and suffixes merged from separate files (deprecated in v4.0)",
      "Traits are applied when components are selected in patterns",
      "Trait merging: numbers take highest, strings take last, booleans use OR",
      "Emergent rarity calculated from combined component weights"
    ]
  }
}
```

### Step 7: Deprecate Old Files

Add deprecation notice to old files:

**prefixes.json** → **prefixes.deprecated.json**:
```json
{
  "_DEPRECATED": {
    "reason": "Prefixes merged into names.json v4.0",
    "migration_date": "2025-12-17",
    "new_location": "names.json → components.prefix[]",
    "see_also": "docs/planning/NAMING_SYSTEM_MIGRATION_GUIDE.md"
  },
  "items": []
}
```

**suffixes.json** → **suffixes.deprecated.json**:
```json
{
  "_DEPRECATED": {
    "reason": "Suffixes merged into names.json v4.0",
    "migration_date": "2025-12-17",
    "new_location": "names.json → components.suffix[]",
    "see_also": "docs/planning/NAMING_SYSTEM_MIGRATION_GUIDE.md"
  },
  "items": []
}
```

### Step 8: Test in ContentBuilder

1. Open ContentBuilder
2. Navigate to migrated `names.json`
3. Verify:
   - ✅ Components tab shows all groups (prefix, material, quality, suffix)
   - ✅ Components display as "Value (weight: N)"
   - ✅ Patterns validate correctly
   - ✅ Live examples generate with all components
   - ✅ Trait editing UI appears (if implemented)

### Step 9: Test in Game Engine

```csharp
// Pattern generation should work with new structure
var generator = new PatternGenerator();
var result = generator.Generate("prefix + material + base + suffix", componentsData);

// Expected: "Blessed Mithril Longsword of Fire"
// With merged traits from all 4 components
```

---

## Automated Migration Script

Use this PowerShell script to help automate the conversion:

```powershell
# migrate-names-to-v4.ps1

param(
    [Parameter(Mandatory=$true)]
    [string]$CategoryPath  # e.g., "items/weapons"
)

$namesFile = Join-Path $CategoryPath "names.json"
$prefixesFile = Join-Path $CategoryPath "prefixes.json"
$suffixesFile = Join-Path $CategoryPath "suffixes.json"

# Load existing files
$names = Get-Content $namesFile | ConvertFrom-Json
$prefixes = Get-Content $prefixesFile | ConvertFrom-Json
$suffixes = Get-Content $suffixesFile | ConvertFrom-Json

# Convert prefixes
$prefixComponents = @()
foreach ($item in $prefixes.items) {
    $prefixComponents += @{
        value = if ($item.displayName) { $item.displayName } else { $item.name }
        rarityWeight = $item.rarityWeight
        traits = if ($item.traits) { $item.traits } else { @{} }
    }
}

# Convert suffixes
$suffixComponents = @()
foreach ($item in $suffixes.items) {
    $suffixComponents += @{
        value = if ($item.displayName) { $item.displayName } else { $item.name }
        rarityWeight = $item.rarityWeight
        traits = if ($item.traits) { $item.traits } else { @{} }
    }
}

# Update names.json
$names.components.prefix = $prefixComponents
$names.components.suffix = $suffixComponents
$names.metadata.version = "4.0"
$names.metadata.supportsTraits = $true
$names.metadata.componentKeys += @("prefix", "suffix")
$names.metadata.patternTokens += @("prefix", "suffix")

# Save updated names.json
$names | ConvertTo-Json -Depth 10 | Set-Content "$CategoryPath/names_v4.json"

Write-Host "✅ Migration complete! Review names_v4.json before replacing names.json"
```

Usage:
```powershell
.\scripts\migrate-names-to-v4.ps1 -CategoryPath "RealmEngine.Shared/Data/Json/items/weapons"
```

---

## Trait Type Reference

All trait values should use this structure:

```json
{
  "traitName": {
    "value": <actual_value>,
    "type": "<value_type>"
  }
}
```

### Supported Types

| Type | Description | Examples |
|------|-------------|----------|
| `number` | Numeric stats | `"value": 5`, `"value": 1.5`, `"value": -10` |
| `string` | Text values | `"value": "fire"`, `"value": "crimson"` |
| `boolean` | True/false flags | `"value": true`, `"value": false` |
| `array` | Lists of values | `"value": ["fire", "ice"]` |

### Common Trait Names

**Damage**:
- `damageBonus` - Flat damage increase
- `damageMultiplier` - Multiplicative damage (e.g., 1.5 = +50%)
- `fireDamage`, `iceDamage`, `lightningDamage`, `poisonDamage`, `holyDamage`, `darkDamage`
- `damageVsUndead`, `damageVsDemonic`, `damageVsDragons`, `damageVsArmored`

**Defense**:
- `durability` - Item health
- `armorPiercing` - Ignore armor %
- `armorBreak` - Reduce enemy armor
- `resistFire`, `resistIce`, `resistLightning`, `resistMagic`

**Stats**:
- `strengthBonus`, `dexterityBonus`, `constitutionBonus`, `wisdomBonus`, `intelligenceBonus`
- `allStatsBonus` - Bonus to all attributes
- `healthBonus`, `manaBonus`, `staminaBonus`
- `healthRegen`, `manaRegen`

**Special**:
- `lifeSteal` - % damage converted to health
- `criticalChance` - % chance for critical hit
- `criticalMultiplier` - Critical damage multiplier
- `attackSpeed` - Attack speed multiplier
- `weightMultiplier` - Item weight multiplier (0.5 = half weight)
- `valueMultiplier` - Gold value multiplier
- `glowEffect` - Visual glow (boolean)
- `visualColor` - Color tint (string)

---

## Rollback Procedure

If migration fails:

```powershell
# Restore from backup
Copy-Item backup_v3/names.json ./
Copy-Item backup_v3/prefixes.json ./
Copy-Item backup_v3/suffixes.json ./
```

---

## Categories to Migrate

### Priority Order

1. ✅ **items/weapons** - COMPLETE (proof-of-concept)
2. ⏳ **items/armor** - TODO
3. ⏳ **enemies/*** - TODO (13 categories)
4. ⏳ **npcs/names** - TODO (if applicable)

### Estimated Time

- **Per category**: 30-60 minutes (manual)
- **With script**: 5-10 minutes + review time
- **Total (all categories)**: 4-8 hours

---

## Validation Checklist

After migration, verify:

- [ ] All prefix items converted to `components.prefix[]`
- [ ] All suffix items converted to `components.suffix[]`
- [ ] Every component has `value`, `rarityWeight`, and `traits` fields
- [ ] Trait values use `{ value, type }` structure
- [ ] Metadata updated to v4.0 with `supportsTraits: true`
- [ ] Component keys include `prefix` and `suffix`
- [ ] Patterns added that use prefix/suffix tokens
- [ ] Old files renamed with `.deprecated.json` extension
- [ ] ContentBuilder can open and display the file
- [ ] Pattern validation works
- [ ] Live examples generate correctly
- [ ] Game engine can resolve patterns with new structure

---

## Related Documents

- `docs/planning/NAMING_SYSTEM_REFACTOR_PROPOSAL.md` - Original proposal
- `docs/standards/PATTERN_COMPONENT_STANDARDS.md` - Updated schema documentation
- `docs/implementation/CONTENT_BUILDER_TRAIT_SUPPORT.md` - ContentBuilder trait editing UI

