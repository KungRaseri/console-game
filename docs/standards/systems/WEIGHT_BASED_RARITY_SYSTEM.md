# Weight-Based Rarity System Implementation

**Date:** December 16, 2025  
**Version:** 1.0  
**Status:** ✅ Approved - Ready for Implementation

## Executive Summary

The game now uses a **weight-based rarity system** where item rarity emerges naturally from the combination of components rather than being predetermined by tier. This creates a more flexible, realistic, and interesting loot system.

### Key Principles

1. **Rarity is Composite** - Final rarity = sum of component weights
2. **Materials Matter Most** - Rare materials create rare items
3. **Modifiers Add Power** - Prefixes/suffixes contribute to rarity
4. **Base Items Are Equal** - Any crafter can make a "Longsword"
5. **Weights Are Adjustable** - Balance tuning via weight configuration

---

## Rarity Calculation Formula

```
Total Weight = (material.weight × 1.0) 
             + (prefix.weight × 1.0)
             + (suffix.weight × 0.8)
             + (base.weight × 0.5)
             + (quality.weight × 1.2)
             + (descriptive.weight × 1.0)

Final Rarity = Map(Total Weight, Rarity Thresholds)
```

### Rarity Thresholds

| Tier | Weight Range | Color | Drop Rate | Examples |
|------|--------------|-------|-----------|----------|
| **Common** | 0-20 | Gray (#808080) | 60% | Rusty Iron Shortsword |
| **Uncommon** | 21-50 | Green (#00FF00) | 25% | Fine Steel Longsword |
| **Rare** | 51-100 | Blue (#0000FF) | 10% | Superior Mythril Greatsword of Sharpness |
| **Epic** | 101-200 | Purple (#A020F0) | 4% | Masterwork Dragonbone Katana of Fire |
| **Legendary** | 201+ | Orange (#FFA500) | 1% | Eternal Void Crystal Dragonblade of Divine Power |

---

## Example Calculations

### Example 1: Common Item
```
"Rusty Iron Shortsword"

Components:
  prefix:   "Rusty" (weight 2) × 1.0     = 2
  material: "Iron" (weight 5) × 1.0      = 5
  base:     "Shortsword" (weight 5) × 0.5 = 2.5
                                          ───
Total Weight: 9.5 → Common tier (0-20)
```

### Example 2: Rare Item
```
"Superior Mythril Longsword of Fire"

Components:
  quality:  "Superior" (weight 25) × 1.2  = 30
  material: "Mythril" (weight 50) × 1.0   = 50
  base:     "Longsword" (weight 10) × 0.5 = 5
  suffix:   "of Fire" (weight 40) × 0.8   = 32
                                           ───
Total Weight: 117 → Epic tier (101-200)
```

### Example 3: Legendary Item
```
"Eternal Void Crystal Dragonblade of Divine Power"

Components:
  quality:    "Eternal" (weight 250) × 1.2     = 300
  material:   "Void Crystal" (weight 250) × 1.0 = 250
  base:       "Dragonblade" (weight 80) × 0.5  = 40
  enchantment: "of Divine Power" (weight 200) × 0.8 = 160
                                                     ───
Total Weight: 750 → Legendary tier (201+)
```

---

## JSON Structure Changes

### types.json - Add Weights to Base Items

**Before:**
```json
{
  "weapon_types": {
    "swords": {
      "items": [
        { "name": "Shortsword", "damage": "1d6", "weight": 2.0 },
        { "name": "Longsword", "damage": "1d8", "weight": 3.0 }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand"
      }
    }
  }
}
```

**After (with rarity weights):**
```json
{
  "weapon_types": {
    "swords": {
      "items": [
        {
          "name": "Shortsword",
          "damage": "1d6",
          "weight": 2.0,
          "rarityWeight": 5
        },
        {
          "name": "Longsword",
          "damage": "1d8",
          "weight": 3.0,
          "rarityWeight": 10
        },
        {
          "name": "Katana",
          "damage": "1d10",
          "weight": 2.5,
          "rarityWeight": 35
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand"
      }
    }
  }
}
```

**Note:** Added `rarityWeight` field to track rarity contribution (separate from physical weight)

---

### names.json - Components Get Weights (NO items array)

**Before:**
```json
{
  "components": {
    "material": ["Iron", "Steel", "Mythril"]
  }
}
```

**After:**
```json
{
  "components": {
    "material": [
      { "name": "Iron", "weight": 5 },
      { "name": "Steel", "weight": 10 },
      { "name": "Mythril", "weight": 50 },
      { "name": "Adamantine", "weight": 75 },
      { "name": "Void Crystal", "weight": 250 }
    ],
    "quality": [
      { "name": "Ruined", "weight": 2 },
      { "name": "Fine", "weight": 10 },
      { "name": "Superior", "weight": 25 },
      { "name": "Masterwork", "weight": 40 },
      { "name": "Legendary", "weight": 90 }
    ]
  }
}
```

---

### Before: prefixes.json (Rarity Tiers)

```json
{
  "common": {
    "Rusty": {
      "displayName": "Rusty",
      "traits": { "damageMultiplier": { "value": 0.8, "type": "number" } }
    }
  },
  "rare": {
    "Mythril": {
      "displayName": "Mythril",
      "traits": { "damageBonus": { "value": 5, "type": "number" } }
    }
  }
}
```

### After: prefixes.json (Flat with Weights)

```json
{
  "prefixes": {
    "Rusty": {
      "displayName": "Rusty",
      "weight": 2,
      "traits": {
        "damageMultiplier": { "value": 0.8, "type": "number" },
        "durability": { "value": 50, "type": "number" }
      }
    },
    "Mythril": {
      "displayName": "Mythril",
      "weight": 50,
      "traits": {
        "damageBonus": { "value": 5, "type": "number" },
        "durability": { "value": 150, "type": "number" },
        "glowEffect": { "value": true, "type": "boolean" }
      }
    },
    "Dragonbone": {
      "displayName": "Dragonbone",
      "weight": 80,
      "traits": {
        "damageBonus": { "value": 10, "type": "number" },
        "fireResist": { "value": 25, "type": "number" }
      }
    }
  }
}
```

**Key Changes:**
- ❌ Removed rarity tier organization (common/rare/epic sections)
- ✅ Added `weight` field to every prefix/suffix
- ✅ Flattened structure (all items at root level)
- ✅ Kept trait system with `{ value, type }` wrappers

---

## New Configuration File

### general/rarity_config.json

```json
{
  "thresholds": {
    "common": {
      "min": 0,
      "max": 20,
      "color": "#808080",
      "displayName": "Common",
      "dropRate": 0.60,
      "glowEffect": false
    },
    "uncommon": {
      "min": 21,
      "max": 50,
      "color": "#00FF00",
      "displayName": "Uncommon",
      "dropRate": 0.25,
      "glowEffect": false
    },
    "rare": {
      "min": 51,
      "max": 100,
      "color": "#0000FF",
      "displayName": "Rare",
      "dropRate": 0.10,
      "glowEffect": true
    },
    "epic": {
      "min": 101,
      "max": 200,
      "color": "#A020F0",
      "displayName": "Epic",
      "dropRate": 0.04,
      "glowEffect": true
    },
    "legendary": {
      "min": 201,
      "max": 999,
      "color": "#FFA500",
      "displayName": "Legendary",
      "dropRate": 0.01,
      "glowEffect": true
    }
  },
  "weight_multipliers": {
    "material": 1.0,
    "prefix": 1.0,
    "suffix": 0.8,
    "base": 0.5,
    "quality": 1.2,
    "descriptive": 1.0,
    "enchantment": 0.8,
    "title": 0.6
  },
  "metadata": {
    "description": "Rarity system configuration with weight thresholds and multipliers",
    "version": "1.0",
    "lastUpdated": "2025-12-16"
  }
}
```

---

## Recommended Weight Ranges

### By Component Type

| Component | Common | Uncommon | Rare | Epic | Legendary |
|-----------|--------|----------|------|------|-----------|
| **Materials** | 2-10 | 11-30 | 31-70 | 71-150 | 151-300 |
| **Prefixes** | 2-10 | 11-30 | 31-70 | 71-150 | 151-300 |
| **Suffixes** | 5-15 | 16-35 | 36-65 | 66-120 | 121-250 |
| **Quality** | 2-12 | 13-28 | 29-50 | 51-90 | 91-200 |
| **Base Items** | 5-10 | 11-20 | 21-40 | 41-80 | 81-150 |
| **Enchantments** | 10-18 | 19-40 | 41-75 | 76-140 | 141-280 |

### Example Component Weights

**Materials:**
- Common: Iron (5), Copper (3), Wood (4), Leather (6), Bronze (8)
- Uncommon: Steel (12), Silver (18), Hardwood (15), Studded Leather (22)
- Rare: Mythril (50), Adamantine (65), Crystal (55), Dragonhide (60)
- Epic: Dragonbone (100), Celestial Steel (120), Void Iron (110)
- Legendary: Void Crystal (250), Divine Essence (300), Dragon Heart (275)

**Quality Modifiers:**
- Common: Ruined (2), Old (4), Simple (6), Plain (8)
- Uncommon: Fine (12), Well-Made (16), Quality (20), Solid (18)
- Rare: Superior (28), Exceptional (32), Impressive (30)
- Epic: Masterwork (55), Exquisite (60), Flawless (65)
- Legendary: Legendary (95), Mythical (120), Divine (150)

**Prefixes:**
- Common: Rusty (2), Chipped (3), Dull (4), Worn (5)
- Uncommon: Sharpened (14), Reinforced (18), Tempered (16)
- Rare: Blessed (45), Enchanted (50), Flaming (55), Frozen (52)
- Epic: Dragonforged (85), Ancient (90), Celestial (100), Infernal (95)
- Legendary: Godforged (220), Eternal (250), Void-Blessed (280)

**Suffixes:**
- Common: of Sharpness (12), of Durability (10), of Balance (14)
- Uncommon: of Power (25), of Speed (28), of Protection (22)
- Rare: of Fire (48), of Ice (50), of Lightning (52), of Slaying (55)
- Epic: of the Dragon (90), of the Phoenix (100), of the Titan (85)
- Legendary: of Divine Power (200), of Eternal Flames (220), of the Void (250)

---

## Loot Generation

### Loot Table Configuration

```json
{
  "loot_tables": {
    "trash_mob": {
      "targetWeight": { "min": 5, "max": 25 },
      "count": { "min": 0, "max": 1 },
      "dropChance": 0.30
    },
    "common_chest": {
      "targetWeight": { "min": 15, "max": 35 },
      "count": { "min": 1, "max": 3 }
    },
    "elite_enemy": {
      "targetWeight": { "min": 50, "max": 90 },
      "count": { "min": 1, "max": 2 },
      "dropChance": 0.80
    },
    "rare_chest": {
      "targetWeight": { "min": 60, "max": 120 },
      "count": { "min": 1, "max": 2 }
    },
    "boss_drop": {
      "targetWeight": { "min": 120, "max": 180 },
      "count": 1,
      "dropChance": 1.0
    },
    "legendary_quest_reward": {
      "targetWeight": { "min": 220, "max": 400 },
      "count": 1
    }
  }
}
```

### Loot Generation Algorithm

```pseudocode
function generateLoot(lootTableName):
    table = lootTables[lootTableName]
    
    // Roll drop chance
    if random() > table.dropChance:
        return null
    
    // Determine target weight
    targetWeight = random(table.targetWeight.min, table.targetWeight.max)
    
    // Build item to match target weight
    item = buildItemForWeight(targetWeight)
    
    return item

function buildItemForWeight(targetWeight):
    remainingWeight = targetWeight
    components = {}
    
    // Allocate weight budget to different component types
    budgets = {
        material: targetWeight * 0.35,  // 35% to material
        quality: targetWeight * 0.25,   // 25% to quality
        enchantment: targetWeight * 0.30, // 30% to enchantment
        base: targetWeight * 0.10       // 10% to base
    }
    
    // Select components close to budget
    components.material = selectClosestWeight(allMaterials, budgets.material)
    components.quality = selectClosestWeight(allQualities, budgets.quality)
    
    if budgets.enchantment > 15:  // Only add enchantment if weight budget allows
        components.enchantment = selectClosestWeight(allEnchantments, budgets.enchantment)
    
    components.base = selectRandom(allBases)  // Base has minimal impact
    
    // Generate item via pattern
    pattern = selectAppropriatePattern(components)
    item = executePattern(pattern, components)
    
    return item
```

---

## Migration Plan

### Phase 1: Update Structure (Files)

**For Each types.json File:**
1. Add `rarityWeight` field to each item
2. Assign weights based on item rarity tier
3. Keep existing stats intact (damage, physical weight, etc.)
4. Test `base` token resolution

**For Each names.json File:**
1. Convert component arrays to objects with weights: `["Iron"]` → `[{ "name": "Iron", "weight": 5 }]`
2. Assign weights based on rarity guidelines
3. **NO items array** - items live in types.json
4. Test pattern execution

**For Each prefixes.json / suffixes.json File:**
1. Flatten rarity tier structure
2. Add `weight` field to each modifier
3. Move all modifiers to root `"prefixes"` or `"suffixes"` object
4. Verify trait structures remain intact

**Create New Files:**
1. `general/rarity_config.json` with thresholds and multipliers
2. Loot table configurations (if not exists)

### Phase 2: Update Code

**RealmEngine.Shared:**
1. Create `WeightedComponent` class with `name` and `weight` properties
2. Update pattern executor to calculate total weight
3. Add `GetRarityFromWeight()` method
4. Load `rarity_config.json` at startup

**ContentBuilder:**
1. Add weight input field to component editors
2. Show live rarity calculation in preview
3. Add weight validation (warn if outside expected range)
4. Create rarity simulator tool

**Game.Console:**
1. Update loot generation to use weight targeting
2. Display item rarity with color
3. Add glow effect for rare+ items
4. Update item tooltips with weight/rarity info

### Phase 3: Content Migration

**Estimated Files to Update:**
- ✅ `general/*.json` - 9 files (already done, add weights)
- 📋 `items/weapons/*.json` - 4 files (names, prefixes, suffixes, types)
- 📋 `items/armor/*.json` - 5 files
- 📋 `items/consumables/*.json` - 3 files
- 📋 `items/enchantments/*.json` - 2 files
- 📋 `enemies/*/*.json` - ~52 files (13 categories × 4 files each)
- 📋 `npcs/*/*.json` - ~20 files (estimated)

**Total:** ~95 files to update

### Phase 4: Testing & Balancing

1. Generate 1000 random items across all weight ranges
2. Verify rarity distribution matches expected drop rates
3. Test loot tables for each encounter type
4. Adjust weights if certain items feel too common/rare
5. Player testing for "feel" of progression

---

## Benefits of This System

### Flexibility
- ✅ Same components can create different rarities based on combination
- ✅ No hard-coded "this is always rare" limitations
- ✅ Easy to add new components without disrupting balance

### Realism
- ✅ Rare materials naturally create rare items
- ✅ Crafting makes sense (Mythril sword > Iron sword)
- ✅ Modifiers stack meaningfully

### Balance Control
- ✅ Single config file controls all thresholds
- ✅ Weight multipliers allow fine-tuning
- ✅ Loot tables target specific power levels

### Emergent Gameplay
- ✅ Players hunt for specific materials
- ✅ Crafting becomes strategic (combine weights wisely)
- ✅ Trade economy based on material rarity

---

## Potential Issues & Solutions

### Issue 1: Weight Inflation
**Problem:** Items become too powerful too quickly  
**Solution:** Adjust multipliers down (base × 0.3 instead of 0.5)

### Issue 2: Too Many Legendary Items
**Problem:** Legendary threshold too easy to reach  
**Solution:** Increase legendary min to 250 or 300

### Issue 3: Components Too Narrow
**Problem:** Only a few materials reach each tier  
**Solution:** Add more component variety at each weight range

### Issue 4: Loot Feels Random
**Problem:** Weight targeting doesn't feel intentional  
**Solution:** Tighten targetWeight min/max ranges in loot tables

---

## Next Steps

1. ✅ **Approve this system** (DONE)
2. 📋 **Update PATTERN_COMPONENT_STANDARDS.md** (DONE)
3. 📋 **Create `rarity_config.json`**
4. 📋 **Update one sample file** (items/weapons/names.json) as proof-of-concept
5. 📋 **Test with pattern executor**
6. 📋 **Migrate remaining files**
7. 📋 **Update ContentBuilder UI**
8. 📋 **Implement loot generation**
9. 📋 **Playtest and balance**

---

## Conclusion

The weight-based rarity system provides a sophisticated, flexible approach to item rarity that:
- Respects player crafting choices
- Makes material hunting meaningful
- Creates natural item progression
- Allows for easy balance tuning
- Generates interesting loot combinations

This system is **approved and ready for implementation**.
