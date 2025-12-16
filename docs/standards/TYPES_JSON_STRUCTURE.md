# types.json Structure Standard

**Date:** December 16, 2025  
**Status:** ✅ Finalized  
**Applies To:** ALL categories (weapons, armor, enemies, NPCs, items)

## Overview

The `types.json` file serves as the **item/enemy catalog** containing:
- Base items/enemies organized by category
- Individual stats per item (damage, weight, value, etc.)
- Shared traits per category (damageType, slot, etc.)

## Standard Structure

```json
{
  "[category]_types": {
    "[type_name]": {
      "items": [
        {
          "name": "ItemName1",
          "stat1": "value1",
          "stat2": value2,
          "stat3": true
        },
        {
          "name": "ItemName2",
          "stat1": "value1",
          "stat2": value2
        }
      ],
      "traits": {
        "sharedTrait1": "value",
        "sharedTrait2": "value"
      }
    }
  }
}
```

## Key Principles

### 1. Item-Level Stats (Unique to Each Item)

**What:** Properties that differ between items of the same type

**Examples:**
- `damage` - Damage range (Shortsword: "1d6" vs Longsword: "1d8")
- `weight` - Weight in pounds
- `value` - Base gold value
- `rarity` - Drop rarity (common, uncommon, rare, etc.)
- `twoHanded` - Specific to item (Greatsword: true, Shortsword: false)

**Format:** Object with `name` property + individual stats

### 2. Type-Level Traits (Shared by All Items)

**What:** Properties that ALL items of this type share

**Examples:**
- `damageType` - Type of damage (all swords: "slashing")
- `slot` - Equipment slot (all weapons: "mainhand")
- `category` - Item category for filtering/organization

**Format:** Object with shared properties

### 3. Fixed Stats with Range Support

**Current:** Stats are fixed values (damage: "1d8")  
**Future:** Allow range notation (damage: "1d6-1d10") for randomization

**Implementation:**
- Start with fixed values
- Runtime can later parse range notation
- Example: `"1d6-1d10"` → randomly pick between 1d6, 1d8, or 1d10

---

## Complete Example: Weapons

```json
{
  "weapon_types": {
    "swords": {
      "items": [
        {
          "name": "Dagger",
          "damage": "1d4",
          "weight": 1.0,
          "value": 5,
          "rarity": "common",
          "finesse": true
        },
        {
          "name": "Shortsword",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarity": "common",
          "finesse": true
        },
        {
          "name": "Longsword",
          "damage": "1d8",
          "weight": 3.0,
          "value": 15,
          "rarity": "common"
        },
        {
          "name": "Greatsword",
          "damage": "2d6",
          "weight": 6.0,
          "value": 50,
          "rarity": "uncommon",
          "twoHanded": true
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "sword"
      }
    },
    "axes": {
      "items": [
        {
          "name": "Handaxe",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarity": "common",
          "thrown": true
        },
        {
          "name": "Battleaxe",
          "damage": "1d8",
          "weight": 4.0,
          "value": 20,
          "rarity": "common"
        },
        {
          "name": "Greataxe",
          "damage": "1d12",
          "weight": 7.0,
          "value": 60,
          "rarity": "uncommon",
          "twoHanded": true
        }
      ],
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "axe"
      }
    },
    "bows": {
      "items": [
        {
          "name": "Shortbow",
          "damage": "1d6",
          "weight": 2.0,
          "value": 25,
          "rarity": "common",
          "range": 80,
          "ammunition": "arrow"
        },
        {
          "name": "Longbow",
          "damage": "1d8",
          "weight": 2.5,
          "value": 50,
          "rarity": "uncommon",
          "range": 150,
          "ammunition": "arrow",
          "twoHanded": true
        }
      ],
      "traits": {
        "damageType": "piercing",
        "slot": "mainhand",
        "category": "bow",
        "ranged": true
      }
    }
  }
}
```

---

## Complete Example: Armor

```json
{
  "armor_types": {
    "helmets": {
      "items": [
        {
          "name": "Leather Cap",
          "armor": 1,
          "weight": 0.5,
          "value": 5,
          "rarity": "common"
        },
        {
          "name": "Chainmail Coif",
          "armor": 3,
          "weight": 2.0,
          "value": 30,
          "rarity": "uncommon"
        },
        {
          "name": "Plate Helmet",
          "armor": 5,
          "weight": 5.0,
          "value": 75,
          "rarity": "rare"
        }
      ],
      "traits": {
        "slot": "head",
        "category": "helmet"
      }
    },
    "chest": {
      "items": [
        {
          "name": "Leather Armor",
          "armor": 2,
          "weight": 10.0,
          "value": 10,
          "rarity": "common"
        },
        {
          "name": "Chainmail",
          "armor": 5,
          "weight": 20.0,
          "value": 75,
          "rarity": "uncommon"
        },
        {
          "name": "Plate Armor",
          "armor": 8,
          "weight": 45.0,
          "value": 400,
          "rarity": "rare",
          "stealthDisadvantage": true
        }
      ],
      "traits": {
        "slot": "chest",
        "category": "body_armor"
      }
    }
  }
}
```

---

## Complete Example: Enemies

```json
{
  "beast_types": {
    "wolves": {
      "items": [
        {
          "name": "Wolf",
          "health": 15,
          "damage": "1d6",
          "armor": 1,
          "speed": 40,
          "level": 1,
          "xp": 50
        },
        {
          "name": "Dire Wolf",
          "health": 30,
          "damage": "2d6",
          "armor": 2,
          "speed": 50,
          "level": 3,
          "xp": 200
        },
        {
          "name": "Winter Wolf",
          "health": 50,
          "damage": "2d8",
          "armor": 3,
          "speed": 50,
          "level": 5,
          "xp": 450,
          "abilities": ["frost_breath"]
        }
      ],
      "traits": {
        "type": "beast",
        "category": "wolf",
        "packHunter": true
      }
    },
    "bears": {
      "items": [
        {
          "name": "Black Bear",
          "health": 25,
          "damage": "1d8",
          "armor": 2,
          "speed": 30,
          "level": 2,
          "xp": 100
        },
        {
          "name": "Brown Bear",
          "health": 40,
          "damage": "2d6",
          "armor": 3,
          "speed": 40,
          "level": 3,
          "xp": 200
        },
        {
          "name": "Cave Bear",
          "health": 60,
          "damage": "2d10",
          "armor": 4,
          "speed": 30,
          "level": 5,
          "xp": 500,
          "abilities": ["maul"]
        }
      ],
      "traits": {
        "type": "beast",
        "category": "bear",
        "strength": "high"
      }
    }
  }
}
```

---

## Integration with Other Files

### Pattern Execution (names.json)

When pattern contains `base` token:

1. Runtime picks item type from types.json (e.g., "swords")
2. Runtime picks random item object from items array
3. Runtime uses `item.name` to resolve `base` token
4. Runtime inherits all item stats + type traits

**Example:**
```
types.json → Pick "swords" → Pick { name: "Longsword", damage: "1d8", weight: 3.0 }
Pattern: "quality + material + base"
Components: quality="Fine", material="Steel"
Result: "Fine Steel Longsword" with damage="1d8", weight=3.0, damageType="slashing"
```

### Stat Modifiers (prefixes.json / suffixes.json)

Runtime applies modifiers to item stats:

1. Pick base item from types.json → get item stats + type traits
2. Pick prefix (if applicable) → add prefix modifiers
3. Pick suffix (if applicable) → add suffix modifiers
4. Calculate final stats: item + type + prefix + suffix

**Example:**
```
Item: { name: "Longsword", damage: "1d8", value: 15 }
Type: { damageType: "slashing", slot: "mainhand" }
Prefix: { name: "Flaming", bonusDamage: +5, damageType: "fire", value: +50 }
Suffix: { name: "of Slaying", bonusVsUndead: +10, value: +100 }

Final Stats:
  damage: "1d8+5" (fire)
  bonusVsUndead: +10
  value: 165
  slot: "mainhand"
  
Final Name: "Flaming Longsword of Slaying"
```

---

## Benefits of This Structure

### ✅ Flexibility
- Each item can have unique stats
- Easy to add new items without changing type structure
- Optional properties per item (finesse, thrown, twoHanded, etc.)

### ✅ Consistency
- Shared traits ensure category-wide properties
- Type-level organization keeps data clean
- Clear separation of unique vs shared properties

### ✅ Maintainability
- Easy to balance items (tweak individual stats)
- Easy to balance categories (adjust type traits)
- Single source of truth for all item data

### ✅ Extensibility
- Add new stat properties as needed
- Support range notation in the future
- Works with pattern system AND stat modifiers

---

## Migration Checklist

When converting existing files to this structure:

- [ ] Identify category types (weapon_types, armor_types, etc.)
- [ ] Convert items array from strings to objects with `name` property
- [ ] Identify unique stats per item (damage, weight, value, etc.)
- [ ] Identify shared traits for type (damageType, slot, category)
- [ ] Add item-level stats to each item object
- [ ] Add type-level traits to traits object
- [ ] Validate JSON structure
- [ ] Test pattern execution with new structure
- [ ] Update ContentBuilder to handle item objects

---

## Questions & Answers

**Q: What if two items have the same stats?**  
A: Keep them as separate items. They may diverge in the future, and it's clearer to show each item explicitly.

**Q: Should ALL items have ALL the same properties?**  
A: No. Use optional properties. Example: Only Greatsword has `twoHanded: true`, others omit it (treated as false).

**Q: What about items with multiple types (e.g., versatile weapons)?**  
A: Add property to item: `versatile: true` with additional stats like `twoHandedDamage: "1d10"`

**Q: How do I know what goes in item stats vs type traits?**  
A: Ask: "Do ALL items of this type share this property?"
- Yes → type traits (all swords are slashing)
- No → item stats (Shortsword damage ≠ Greatsword damage)

**Q: Can I nest item types further?**  
A: Yes, but keep it shallow. Example:
```json
"swords": {
  "longswords": { "items": [...], "traits": {...} },
  "shortswords": { "items": [...], "traits": {...} }
}
```
But prefer flat structure when possible for simplicity.

---

## Status

- ✅ Structure finalized
- ✅ Examples created
- ✅ Integration documented
- ⏳ Ready for implementation
- ⏳ Awaiting file migration
