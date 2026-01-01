# catalog.json Standard (Item/Enemy Catalog)

**Version:** 1.0  
**Date:** December 27, 2025  
**Purpose:** Base item/enemy definitions with stats and type-level traits

---

## Overview

The `catalog.json` file is an **item/enemy catalog** containing base definitions that patterns reference via the `{base}` token. It organizes items into types with:

**Key Concepts:**
- **Type Categories** - Groups of related items (swords, axes, trolls, goblins)
- **Type-Level Traits** - Properties shared by ALL items in a type
- **Item-Level Stats** - Individual properties unique to each item
- **rarityWeight** - Selection probability for each item
- **Hierarchical Structure** - *_types → traits + items array

---

## File Location

**Naming Convention:** Always `catalog.json`

```
RealmEngine.Data/Data/Json/
├── items/
│   ├── weapons/
│   │   └── catalog.json        ← Weapon type catalog
│   ├── armor/
│   │   └── catalog.json        ← Armor type catalog
│   ├── consumables/
│   │   └── catalog.json        ← Consumable type catalog
├── enemies/
│   ├── trolls/
│   │   └── catalog.json        ← Troll enemy catalog
│   │   └── abilities_catalog.json ← Troll ability catalog (prefixed)
```

**Naming Pattern:**
- Standard: `catalog.json`
- Prefixed for sub-systems: `abilities_catalog.json`

---

## Standard Structure

```json
{
  "metadata": {
    "description": "Brief description of catalog purpose",
    "version": "1.0",
    "lastUpdated": "YYYY-MM-DD",
    "type": "item_catalog",
    "totalTypes": 6,
    "total_*": 50,
    "usage": "Usage instructions",
    "notes": ["Implementation", "notes"]
  },
  "[category]_types": {
    "type_name": {
      "traits": {
        "sharedProperty1": "value",
        "sharedProperty2": "value"
      },
      "items": [
        {
          "name": "ItemName",
          "stat1": "value",
          "stat2": 123,
          "rarityWeight": 10
        }
      ]
    }
  }
}
```

---

## Metadata Section

### Required Fields

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `description` | string | Purpose of this catalog | "Weapon type catalog with stats" |
| `version` | string | Schema version | "1.0" |
| `lastUpdated` | string | ISO date (YYYY-MM-DD) | "2025-12-27" |
| `type` | string | File type identifier | "item_catalog" |
| `total*` | number | Count fields for types/items | `totalWeaponTypes: 6` |

### Optional Fields

| Field | Type | Description |
|-------|------|-------------|
| `usage` | string | Runtime usage instructions |
| `notes` | array | Implementation notes |
| `categories` | array | List of type categories |

### Example Metadata

```json
{
  "metadata": {
    "description": "Weapon type catalog with base stats and traits",
    "version": "1.0",
    "lastUpdated": "2025-12-27",
    "type": "item_catalog",
    "totalWeaponTypes": 6,
    "totalWeapons": 69,
    "usage": "Base catalog of weapons with inherent stats and traits. Each item has individual properties. Type-level traits apply to all items in category.",
    "notes": [
      "Base items used in pattern generation with names.json",
      "Each weapon has damage, weight, value, and rarityWeight",
      "Type-level traits (damageType, slot, category) apply to all items in that weapon type"
    ]
  }
}
```

---

## Structure Overview

### Hierarchical Organization

```
catalog.json
├── metadata
└── [category]_types
    ├── type_name_1
    │   ├── traits (shared by all items in type)
    │   └── items[] (individual item definitions)
    ├── type_name_2
    │   ├── traits
    │   └── items[]
```

### Category Naming Convention

Root key is plural, descriptive of domain:

```json
{
  "weapon_types": {...},      // Weapons catalog
  "armor_types": {...},       // Armor catalog
  "troll_types": {...}        // Troll enemies catalog
}
```

---

## Type Structure

### Type Object

```json
{
  "type_name": {
    "traits": {
      "sharedProperty1": "value",
      "sharedProperty2": "value"
    },
    "items": [
      {
        "name": "ItemName",
        "stat1": "value",
        "stat2": 123,
        "rarityWeight": 10
      }
    ]
  }
}
```

### Type-Level Traits

**Purpose:** Properties shared by ALL items in this type

**Common Traits:**

**Weapons:**
```json
{
  "traits": {
    "damageType": "slashing",
    "slot": "mainhand",
    "category": "melee_one_handed",
    "skillType": "blade"
  }
}
```

**Armor:**
```json
{
  "traits": {
    "armorType": "heavy",
    "slot": "chest",
    "category": "plate_armor"
  }
}
```

**Enemies:**
```json
{
  "traits": {
    "category": "troll",
    "size": "large",
    "intelligence": "low",
    "behavior": "aggressive",
    "damageType": "physical",
    "regeneration": true,
    "vulnerability": "fire"
  }
}
```

---

## Items Array

### Item Object

Every item MUST have:
- `name` - Display name (string)
- `rarityWeight` - Selection probability (number)
- Individual stats specific to item type

### Item Fields by Category

**Weapons:**
```json
{
  "name": "Longsword",
  "damage": "1d8",
  "weight": 3.0,
  "value": 15,
  "rarityWeight": 5
}
```

**Armor:**
```json
{
  "name": "Chainmail",
  "armor": 16,
  "weight": 55.0,
  "value": 75,
  "rarityWeight": 10
}
```

**Enemies:**
```json
{
  "name": "Troll",
  "health": 120,
  "attack": 30,
  "defense": 10,
  "speed": 5,
  "level": 8,
  "xp": 140,
  "rarityWeight": 12
}
```

**Consumables:**
```json
{
  "name": "Health Potion",
  "effect": "heal",
  "potency": 50,
  "duration": "instant",
  "value": 25,
  "rarityWeight": 5
}
```

---

## Complete Examples

### Weapon Catalog

```json
{
  "metadata": {
    "description": "Weapon type catalog with base stats and traits",
    "version": "1.0",
    "lastUpdated": "2025-12-27",
    "type": "item_catalog",
    "totalWeaponTypes": 6,
    "totalWeapons": 69
  },
  "weapon_types": {
    "swords": {
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "melee_one_handed",
        "skillType": "blade"
      },
      "items": [
        {
          "name": "Longsword",
          "damage": "1d8",
          "weight": 3.0,
          "value": 15,
          "rarityWeight": 5
        },
        {
          "name": "Shortsword",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarityWeight": 5
        },
        {
          "name": "Greatsword",
          "damage": "2d6",
          "weight": 6.0,
          "value": 50,
          "rarityWeight": 10,
          "twoHanded": true
        }
      ]
    },
    "axes": {
      "traits": {
        "damageType": "slashing",
        "slot": "mainhand",
        "category": "melee_one_handed",
        "skillType": "axe"
      },
      "items": [
        {
          "name": "Handaxe",
          "damage": "1d6",
          "weight": 2.0,
          "value": 10,
          "rarityWeight": 5,
          "throwable": true
        },
        {
          "name": "Battleaxe",
          "damage": "1d8",
          "weight": 4.0,
          "value": 20,
          "rarityWeight": 8
        }
      ]
    }
  }
}
```

### Enemy Catalog

```json
{
  "metadata": {
    "description": "Troll enemy type catalog with base stats",
    "version": "1.0",
    "lastUpdated": "2025-12-27",
    "type": "item_catalog",
    "total_types": 3,
    "categories": ["common_trolls", "elemental_trolls", "special_trolls"]
  },
  "troll_types": {
    "common_trolls": {
      "traits": {
        "category": "troll",
        "size": "large",
        "intelligence": "low",
        "social": "solitary",
        "behavior": "aggressive",
        "damageType": "physical",
        "regeneration": true,
        "vulnerability": "fire"
      },
      "items": [
        {
          "name": "Troll",
          "health": 120,
          "attack": 30,
          "defense": 10,
          "speed": 5,
          "level": 8,
          "xp": 140,
          "rarityWeight": 12
        },
        {
          "name": "Cave Troll",
          "health": 140,
          "attack": 34,
          "defense": 12,
          "speed": 4,
          "level": 9,
          "xp": 160,
          "rarityWeight": 18
        }
      ]
    },
    "elemental_trolls": {
      "traits": {
        "category": "troll",
        "size": "large",
        "behavior": "elemental",
        "regeneration": true,
        "magical": true
      },
      "items": [
        {
          "name": "Ice Troll",
          "health": 150,
          "attack": 36,
          "defense": 13,
          "speed": 5,
          "level": 10,
          "xp": 180,
          "rarityWeight": 28,
          "damageType": "cold",
          "vulnerability": "fire"
        },
        {
          "name": "Fire Troll",
          "health": 140,
          "attack": 40,
          "defense": 11,
          "speed": 6,
          "level": 10,
          "xp": 185,
          "rarityWeight": 30,
          "damageType": "fire",
          "vulnerability": "cold",
          "immunity": "fire"
        }
      ]
    }
  }
}
```

---

## Type-Level vs Item-Level Properties

### What Goes in Traits?

**Type-Level Traits** = Properties ALL items in type share

**Examples:**
- All swords have damageType: "slashing"
- All trolls have regeneration: true
- All plate armor has armorType: "heavy"

### What Goes in Item?

**Item-Level Stats** = Properties that DIFFER between items

**Examples:**
- Longsword damage: "1d8" vs Shortsword damage: "1d6"
- Troll health: 120 vs Ice Troll health: 150
- Health Potion potency: 50 vs Greater Health Potion potency: 100

---

## Weight-Based Selection

### rarityWeight Field

**Purpose:** Determines selection probability when generating from catalog

**Formula:**
```
Probability = 100 / rarityWeight
```

### Example Weights

| rarityWeight | Probability | Use Case |
|--------------|-------------|----------|
| 5 | 20% | Very common items (starting gear) |
| 10 | 10% | Common items (standard loot) |
| 20 | 5% | Uncommon items |
| 50 | 2% | Rare items |
| 100 | 1% | Very rare items |
| 250 | 0.4% | Legendary items |

### Balanced Distribution

For a balanced loot table:
```json
{
  "items": [
    {"name": "Common Item", "rarityWeight": 5},   // 50% of drops
    {"name": "Common Item 2", "rarityWeight": 5}, // 50% of drops
    {"name": "Uncommon Item", "rarityWeight": 20}, // 12.5% of drops
    {"name": "Rare Item", "rarityWeight": 50}      // 5% of drops
  ]
}
```

---

## Item Overrides

### When Item-Level Overrides Type-Level

If an item defines a property that exists in type traits, **item-level wins**:

```json
{
  "elemental_trolls": {
    "traits": {
      "damageType": "elemental"  // Type default
    },
    "items": [
      {
        "name": "Ice Troll",
        "damageType": "cold"     // Item override
      }
    ]
  }
}
```

**Result:** Ice Troll has damageType "cold" (not "elemental")

---

---

## Validation Rules

### ✅ Catalog Validation

1. **Metadata must include** description, version, lastUpdated, type
2. **Root key** must match pattern `*_types` or domain-specific name
3. **Each type must have** traits object and items array
4. **Each item must have** name and rarityWeight
5. **rarityWeight** must be positive number
6. **Type traits** should be shared across all items

---

## Common Mistakes

### ❌ Don't Put Item-Specific Stats in Traits

**WRONG:**
```json
{
  "swords": {
    "traits": {
      "damage": "1d8"  // This varies per sword!
    }
  }
}
```

**RIGHT:**
```json
{
  "swords": {
    "traits": {
      "damageType": "slashing"  // ALL swords slash
    },
    "items": [
      {"name": "Longsword", "damage": "1d8"},  // Individual damage
      {"name": "Shortsword", "damage": "1d6"}
    ]
  }
}
```

### ❌ Don't Forget rarityWeight

**WRONG:**
```json
{
  "items": [
    {"name": "Longsword", "damage": "1d8"}  // Missing rarityWeight!
  ]
}
```

**RIGHT:**
```json
{
  "items": [
    {"name": "Longsword", "damage": "1d8", "rarityWeight": 5}
  ]
}
```

### ❌ Don't Mix Different Item Types in One Category

**WRONG:**
```json
{
  "weapons": {
    "items": [
      {"name": "Longsword", "damage": "1d8"},
      {"name": "Chainmail", "armor": 16}  // This is armor!
    ]
  }
}
```

**RIGHT:**
- Keep weapons in weapon_types
- Keep armor in armor_types
- Separate by semantic category

---

## Best Practices

### ✅ DO:

1. **Use semantic type names** - "swords", "axes" (not "type1", "type2")
2. **Share common properties** in traits
3. **Balance rarityWeight** across items
4. **Include all required fields** in metadata
5. **Group related items** in same type
6. **Update lastUpdated** when modifying
7. **Provide clear descriptions** in metadata

### ❌ DON'T:

1. **Don't duplicate type traits** in every item
2. **Don't mix unrelated items** in same type
3. **Don't skip rarityWeight** field
4. **Don't use arbitrary stat names** - be consistent
5. **Don't forget metadata** section

---

## Integration with names.json

### How Catalog Works with Pattern Generation

**In names.json:**
```json
{
  "patterns": [
    {"pattern": "{quality} {base}", "rarityWeight": 40}
  ]
}
```

**{base} resolves from catalog.json:**
```json
{
  "weapon_types": {
    "swords": {
      "items": [
        {"name": "Longsword", "rarityWeight": 5}
      ]
    }
  }
}
```

**Generated:** "Fine Longsword"

**Final Item Properties:**
```json
{
  "name": "Fine Longsword",
  "damage": "1d8",           // From catalog
  "damageType": "slashing",  // From traits
  "durability": 130,         // From quality component trait
  "valueMultiplier": 1.3     // From quality component trait
}
```

---

## v4.1 Reference System

### Referencing Catalog Items

Catalog items can be referenced from other JSON files using the v4.1 reference syntax:

**Syntax:** `@domain/path/category:item-name[filters]?.property.nested`

### Reference Examples

**Specific Item Reference:**
```json
{
  "parentClass": "@classes/warriors:fighter",
  "weapon": "@items/weapons/swords:longsword",
  "ability": "@abilities/active/offensive:fireball"
}
```

**Random Selection (Wildcard):**
```json
{
  "randomWeapon": "@items/weapons/swords:*",
  "randomEnemy": "@enemies/goblinoids:*",
  "randomAbility": "@abilities/passive/defensive:*"
}
```
*Wildcard `:*` selects random item using rarityWeight-based probability*

**Property Access:**
```json
{
  "weaponDamage": "@items/weapons/swords:longsword.damage",
  "className": "@classes/warriors:fighter.name",
  "abilityPower": "@abilities/active/offensive:fireball.power"
}
```

**Optional References (Nullable):**
```json
{
  "optionalParent": "@classes/warriors:paladin?",
  "optionalWeapon": "@items/weapons/exotic:whip?"
}
```
*The `?` suffix returns null instead of error if item not found*

### Reference Filters

**Filter by Property:**
```json
{
  "heavyWeapon": "@items/weapons/swords:*[weight>5]",
  "rareItems": "@items/weapons:*[rarityWeight>=50]",
  "fireAbilities": "@abilities/active/offensive:*[element=fire]"
}
```

**Supported Operators:**
- `=`, `!=` - Equality
- `<`, `<=`, `>`, `>=` - Comparison
- `EXISTS`, `NOT EXISTS` - Property existence
- `MATCHES` - Regex pattern matching

### Benefits of References

✅ **Eliminates Duplication** - Define once, reference many times  
✅ **Type Safety** - ContentBuilder validates references at design time  
✅ **Auto-Updates** - Changes to source catalog propagate automatically  
✅ **Maintainability** - Single source of truth for all game data  

### Reference Documentation

For complete reference system documentation, see:  
**[docs/standards/json/JSON_REFERENCE_STANDARDS.md](JSON_REFERENCE_STANDARDS.md)**

---

## Quest System Catalogs (v4.2+)

### Quest Objectives Catalog

**Purpose:** Define reusable objective templates organized by quest type

**File Location:** `quests/objectives/catalog.json`

**Type Value:** `quest_objectives_catalog`

**Structure:**
```json
{
  "metadata": {
    "description": "Quest objective templates organized by quest type",
    "version": "4.0",
    "lastUpdated": "2025-12-31",
    "type": "quest_objectives_catalog"
  },
  "[questType]_objectives": {
    "[subType]": {
      "items": [
        {
          "name": "objective_name",
          "displayName": "Defeat {quantity} {enemyType}",
          "rarityWeight": 15,
          "description": "Description of objective",
          "enemyReference": "@enemies:*",
          "minQuantity": 5,
          "maxQuantity": 15,
          "rewardMultiplier": 1.0
        }
      ]
    }
  }
}
```

**Quest Type Categories:**
- `kill_objectives` - Boss fights, group combat, bounty hunts
- `fetch_objectives` - Item collection, resource gathering, artifact retrieval
- `escort_objectives` - NPC protection, caravan escort
- `investigate_objectives` - Mystery solving, exploration, clue finding
- `delivery_objectives` - Package delivery, courier missions

**Key Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Unique objective identifier |
| `displayName` | string | Template with placeholders ({quantity}, {enemyType}) |
| `rarityWeight` | number | Selection probability |
| `description` | string | Objective description |
| `*Reference` | string | v4.1 references to entities (@enemies:*, @items:*, @npcs:*) |
| `minQuantity` / `maxQuantity` | number | Quantity ranges |
| `rewardMultiplier` | number | Multiplier for quest rewards (0.5-5.0) |

**Example:**
```json
{
  "kill_objectives": {
    "boss_fight": {
      "items": [
        {
          "name": "defeat_boss",
          "displayName": "Defeat {bossName}",
          "rarityWeight": 40,
          "description": "Slay a powerful boss creature",
          "enemyReference": "@enemies:*[level>=10]",
          "quantity": 1,
          "rewardMultiplier": 3.0
        }
      ]
    },
    "group_combat": {
      "items": [
        {
          "name": "extermination",
          "displayName": "Eliminate {quantity} {enemyType}",
          "rarityWeight": 15,
          "description": "Clear out a group of enemies",
          "enemyReference": "@enemies:*",
          "minQuantity": 5,
          "maxQuantity": 15,
          "rewardMultiplier": 1.0
        }
      ]
    }
  }
}
```

---

### Quest Rewards Catalog

**Purpose:** Define reusable reward templates organized by reward type

**File Location:** `quests/rewards/catalog.json`

**Type Value:** `quest_rewards_catalog`

**Structure:**
```json
{
  "metadata": {
    "description": "Quest reward templates organized by reward type",
    "version": "4.0",
    "lastUpdated": "2025-12-31",
    "type": "quest_rewards_catalog"
  },
  "[rewardType]_rewards": {
    "[subType]": {
      "items": [
        {
          "name": "reward_name",
          "rarityWeight": 20,
          "goldMin": 10,
          "goldMax": 50,
          "description": "Description of reward"
        }
      ]
    }
  }
}
```

**Reward Type Categories:**
- `gold_rewards` - Currency rewards (copper, silver, gold, fortune)
- `item_rewards` - Equipment rewards (weapons, armor, consumables)
- `reputation_rewards` - Faction standing increases
- `ability_rewards` - New skills and techniques
- `experience_rewards` - XP amounts

**Key Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Unique reward identifier |
| `rarityWeight` | number | Selection probability |
| `description` | string | Reward description |
| `goldMin` / `goldMax` | number | Gold amount range (for gold_rewards) |
| `itemReference` | string | v4.1 reference to items (for item_rewards) |
| `quantity` | string/number | Item quantity or range ("3-5", 1) |
| `factionReference` | string | v4.1 reference to factions (for reputation_rewards) |
| `reputationGain` | string | Reputation amount or range ("10-25") |
| `abilityReference` | string | v4.1 reference to abilities (for ability_rewards) |
| `xpMin` / `xpMax` | number | Experience point range |

**Example:**
```json
{
  "gold_rewards": {
    "small_purse": {
      "items": [
        {
          "name": "copper_reward",
          "rarityWeight": 10,
          "goldMin": 5,
          "goldMax": 25,
          "description": "A small amount of coins"
        },
        {
          "name": "gold_reward",
          "rarityWeight": 40,
          "goldMin": 100,
          "goldMax": 500,
          "description": "A hefty bag of gold"
        }
      ]
    }
  },
  "item_rewards": {
    "weapons": {
      "items": [
        {
          "name": "rare_weapon",
          "rarityWeight": 50,
          "itemReference": "@items/weapons:*[rarityWeight>=100]",
          "quantity": 1,
          "description": "A fine weapon"
        }
      ]
    }
  },
  "reputation_rewards": {
    "faction_standing": {
      "items": [
        {
          "name": "major_favor",
          "rarityWeight": 40,
          "factionReference": "@organizations/factions:*",
          "reputationGain": "50-100",
          "description": "Significant recognition from the faction"
        }
      ]
    }
  }
}
```

**Usage in QuestGenerator:**
1. Load objectives catalog based on quest type
2. Select objective by rarityWeight
3. Resolve entity references (@enemies:*, @items:*, etc.)
4. Load rewards catalog
5. Select reward(s) by rarityWeight
6. Apply rewardMultiplier from objective
7. Generate complete quest

---

## Related Standards

- **names.json Standard** - Pattern generation that references this catalog
- **CBCONFIG Standard** - Folder configuration for ContentBuilder
- **Weight-Based Rarity System** - Rarity calculation formulas

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| 4.2 | 2025-12-31 | Added quest objectives and rewards catalog structures |
| 1.0 | 2025-12-27 | Initial catalog standard documentation |
