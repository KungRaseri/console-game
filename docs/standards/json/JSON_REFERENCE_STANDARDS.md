# JSON Reference Standards v4.1

**Version**: 4.1  
**Last Updated**: December 28, 2025  
**Status**: Standard

## Overview

This document defines the unified reference system for linking between JSON data files. References eliminate data duplication, ensure consistency, and enable dynamic content generation.

---

## Reference Syntax

### Basic Format
```
@domain/path/category:item-name
```

### Full Format with Options
```
@domain/path/category:item-or-wildcard[filters]?.property.nested
```

**Components:**
- `@` - Reference marker (required)
- `domain/path/category` - File location matching folder structure
- `:` - Separator between path and item
- `item-name` or `*` - Specific item or wildcard selection
- `[filters]` - Optional filtering conditions (max 3)
- `?` - Optional nullable suffix (returns null instead of error)
- `.property` - Optional property access (supports nesting)

---

## Reference Types

### 1. Direct Reference
Returns the entire object.

```json
{
  "weapon": "@items/weapons/swords:longsword"
}
```

**Resolves to:**
```json
{
  "weapon": {
    "name": "Longsword",
    "baseDamage": 10,
    "weight": 3.5,
    "rarityWeight": 15
  }
}
```

### 2. Property Access
Returns a specific property value.

```json
{
  "damage": "@items/weapons/swords:longsword.baseDamage",
  "element": "@items/weapons/swords:flaming-sword.traits.elementalDamage.value"
}
```

**Resolves to:**
```json
{
  "damage": 10,
  "element": "fire"
}
```

### 3. Wildcard Selection
Returns a random item from the specified category, respecting `rarityWeight`.

```json
{
  "randomSword": "@items/weapons/swords:*",
  "randomWeapon": "@items/weapons/*:*",
  "anyAbility": "@abilities/*:*"
}
```

**Selection Rules:**
- Respects `rarityWeight` - lower weight = higher chance
- Selection probability: `selectionChance = 100 / rarityWeight`
- Items with weight 5 are 4x more likely than weight 20

### 4. Optional Reference
Returns `null` if reference or property doesn't exist (logs warning).

```json
{
  "legendaryWeapon": "@items/weapons/legendary:excalibur?",
  "optionalStat": "@classes/warriors:fighter.customProp?"
}
```

**Behavior:**
- Missing reference with `?` → `null` + warning logged
- Missing reference without `?` → **Error** (fail at load time)
- Missing property → `null` + warning logged (always lenient)

---

## Filtering Syntax

### Basic Filter
```json
"@domain/path/category:*[property operator value]"
```

### Operators

| Operator | Meaning | Example |
|----------|---------|---------|
| `=` | Equals | `[difficulty=easy]` |
| `!=` | Not equals | `[element!=fire]` |
| `<` | Less than | `[manaCost<50]` |
| `<=` | Less than or equal | `[rarityWeight<=20]` |
| `>` | Greater than | `[baseDamage>15]` |
| `>=` | Greater than or equal | `[rarityWeight>=100]` |
| `EXISTS` | Property exists | `[traits.enchantment EXISTS]` |
| `NOT EXISTS` | Property doesn't exist | `[traits.curse NOT EXISTS]` |
| `MATCHES` | Regex pattern | `[name MATCHES "^Ancient.*"]` |

### Multiple Conditions
Use `AND` / `OR` to combine up to 3 conditions.

```json
"commonFireSword": "@items/weapons/swords:*[rarityWeight<=30 AND traits.element=fire]"
"fastOrCheap": "@abilities/active/*:*[manaCost<30 OR cooldown<5]"
"balanced": "@items/weapons/*:*[rarityWeight<=50 AND baseDamage>=10 AND weight<5]"
```

**Limit:** Maximum 3 conditions per filter.

### Nested Property Access in Filters
```json
"@items/weapons/*:*[traits.elementalDamage.value=fire]"
"@abilities/active/*:*[progression.manaCostPerLevel<5]"
```

---

## Examples by Domain

### Items
```json
{
  "equipment": {
    "weapon": "@items/weapons/swords:longsword",
    "material": "@items/materials/metals:steel",
    "randomCommon": "@items/consumables:*[rarityWeight<=15]",
    "magicalOnly": "@items/weapons/*:*[traits.enchantment EXISTS]"
  }
}
```

### Abilities
```json
{
  "startingAbilities": [
    "@abilities/active/offensive:power-strike",
    "@abilities/passive/defensive:iron-will"
  ],
  "randomFire": "@abilities/active/*:*[traits.element=fire]",
  "cheapAbilities": "@abilities/active/*:*[manaCost<50]"
}
```

### Classes
```json
{
  "class": "@classes/warriors:fighter",
  "parentClass": "@classes/warriors:fighter",
  "baseHealth": "@classes/warriors:fighter.startingStats.health",
  "statGrowth": "@classes/warriors:fighter.progression.statGrowth"
}
```

### Enemies
```json
{
  "enemy": "@enemies/beasts/wolves:dire-wolf",
  "bossAbility": "@enemies/dragons/ancient:*[rarityWeight>=200]"
}
```

### NPCs
```json
{
  "background": "@npcs/backgrounds:noble",
  "occupation": "@npcs/occupations:blacksmith",
  "trait": "@npcs/traits/personality:friendly"
}
```

### Quests
```json
{
  "template": "@quests/templates/fetch:medium-fetch",
  "location": "@quests/locations/wilderness:dark-forest",
  "easyQuest": "@quests/templates/*:*[difficulty=easy AND levelRequirement<=10]"
}
```

---

## Use Cases

### 1. Class Definitions with Embedded Progression
```json
{
  "name": "Fighter",
  "displayName": "Fighter",
  "rarityWeight": 5,
  "isSubclass": false,
  "startingStats": {
    "health": 120,
    "mana": 30,
    "strength": 16
  },
  "startingAbilities": [
    "@abilities/active/offensive:basic-attack",
    "@abilities/passive/defensive:second-wind"
  ],
  "progression": {
    "statGrowth": {
      "healthPerLevel": 10,
      "strengthPerLevel": 2
    },
    "abilityUnlocks": {
      "5": ["@abilities/active/offensive:power-strike"],
      "10": ["@abilities/active/offensive:whirlwind"]
    }
  }
}
```

### 2. Subclass Inheritance
```json
{
  "name": "Berserker",
  "displayName": "Berserker",
  "rarityWeight": 50,
  "isSubclass": true,
  "parentClass": "@classes/warriors:fighter",
  "startingAbilities": [
    "@abilities/reactive/offensive:berserker-rage",
    "@abilities/active/offensive:reckless-attack"
  ]
}
```

### 3. Enemy Loot Tables
```json
{
  "name": "Goblin",
  "loot": {
    "common": "@items/consumables:*[rarityWeight<=15]",
    "weapon": "@items/weapons/*:*[rarityWeight<=30 AND baseDamage<20]",
    "rare": "@items/weapons/daggers:*[rarityWeight>=50]?"
  }
}
```

### 4. Dynamic Quest Generation
```json
{
  "questTemplate": "@quests/templates/fetch:*[difficulty=easy]",
  "location": "@quests/locations/wilderness:*[danger=low]",
  "reward": "@items/weapons/*:*[rarityWeight<=40]"
}
```

### 5. Procedural Name Generation
```json
{
  "patterns": [
    {
      "value": "{@items/materials/metals} {base}",
      "rarityWeight": 40
    }
  ]
}
```

In pattern generation, references in `{}` are resolved during name assembly.

---

## Resolution Algorithm

### Step 1: Parse Reference
```
@items/weapons/swords:longsword.baseDamage
↓
domain: items
path: weapons/swords
item: longsword
property: baseDamage
```

### Step 2: Load Catalog
```
File: Game.Data/Data/Json/items/weapons/catalog.json
Category: swords
```

### Step 3: Find Item
```csharp
var category = catalog["swords"];
var item = category.items.FirstOrDefault(i => 
    i.name.Equals("Longsword", StringComparison.OrdinalIgnoreCase));
```

### Step 4: Apply Filters (if wildcard)
```csharp
if (itemName == "*" && hasFilters)
{
    items = ApplyFilters(items, filters);
    item = SelectByRarityWeight(items);
}
```

### Step 5: Access Property (if specified)
```csharp
if (hasProperty)
{
    return GetNestedProperty(item, propertyPath);
}
return item;
```

### Step 6: Handle Missing References
```csharp
if (item == null)
{
    if (isOptional)
    {
        Log.Warning($"Reference '{reference}' not found, returning null");
        return null;
    }
    else
    {
        throw new ReferenceNotFoundException($"Reference '{reference}' not found");
    }
}
```

---

## Type Safety

### JSON Types
References resolve to native JSON types:

```json
{
  "number": "@items/weapons/swords:longsword.baseDamage",      // → 10
  "string": "@items/weapons/swords:longsword.name",            // → "Longsword"
  "boolean": "@items/weapons/swords:flaming-sword.isMagical", // → true
  "object": "@items/weapons/swords:longsword",                // → {...}
  "array": "@classes/warriors:fighter.startingAbilities"      // → [...]
}
```

**Never return strings for numeric values** - resolution preserves types.

---

## Validation

### Load-Time Validation
Validate all non-optional references when loading JSON files.

```
✓ All referenced items exist
✓ All referenced properties exist (or gracefully null)
✓ Syntax is valid
✗ Fail fast on missing required references
```

### Runtime Validation
```
⚠ Log warning for missing properties
⚠ Log warning for optional references not found
⚠ Log warning for filter returning empty results
```

### Validation Tools
```csharp
// Pseudo-code
public interface IReferenceValidator
{
    ValidationResult ValidateReference(string reference);
    bool ReferenceExists(string reference);
    Type GetReferenceType(string reference);
}
```

---

## Filter Performance

### Performance Characteristics

| Filter Complexity | Performance | Example |
|-------------------|-------------|---------|
| No filter | **Fast** | `@items/weapons/swords:*` |
| Single condition | **Fast** | `@items/weapons/swords:*[rarityWeight<=20]` |
| 2-3 conditions | **Moderate** | `@items/weapons/*:*[rarityWeight<=30 AND element=fire]` |
| Regex matching | **Moderate** | `@items/weapons/*:*[name MATCHES "^Ancient.*"]` |

### Optimization Tips
1. **Be specific** - `@items/weapons/swords:*` beats `@items/weapons/*:*`
2. **Order conditions** - Put cheap checks first (numeric before regex)
3. **Cache results** - Resolve references once at load time when possible

---

## Migration from Old Patterns

### Before (Hardcoded Names)
```json
{
  "startingAbilities": ["Basic Attack", "Defend", "Second Wind"]
}
```

### After (References)
```json
{
  "startingAbilities": [
    "@abilities/active/offensive:basic-attack",
    "@abilities/passive/defensive:defend",
    "@abilities/reactive/defensive:second-wind"
  ]
}
```

### Before (Pattern Generation with Old Syntax)
```json
{
  "value": "[@materialRef/weapon] {base}"
}
```

### After (References in Patterns)
```json
{
  "value": "{@items/materials/metals} {base}"
}
```

---

## Common Pitfalls

### ❌ Wrong Case
```json
"weapon": "@items/weapons/swords:LongSword"  // Wrong - should be lowercase kebab-case
```
✅ **Correct:**
```json
"weapon": "@items/weapons/swords:longsword"
```

### ❌ Missing Category
```json
"weapon": "@items/weapons:longsword"  // Missing category (swords)
```
✅ **Correct:**
```json
"weapon": "@items/weapons/swords:longsword"
```

### ❌ Too Many Filters
```json
"item": "@items/*:*[rarityWeight<=20 AND element=fire AND damage>10 AND weight<5]"  // 4 conditions!
```
✅ **Correct (max 3):**
```json
"item": "@items/*:*[rarityWeight<=20 AND element=fire AND damage>10]"
```

### ❌ String Instead of Reference
```json
"ability": "power-strike"  // Wrong - just a string
```
✅ **Correct:**
```json
"ability": "@abilities/active/offensive:power-strike"
```

### ❌ Hardcoded Values from References
```json
"baseDamage": 10  // Wrong if this comes from weapon
```
✅ **Correct:**
```json
"baseDamage": "@items/weapons/swords:longsword.baseDamage"
```

---

## Enhancement System References (v4.2+)

### Material References

**Purpose:** Select materials for item generation with contextual filtering

**Location:** Used in `materialRef` field of item patterns

**Common Filters:**
```json
// Metals with weapon traits
"@items/materials/metals:*[itemTypeTraits.weapon EXISTS]"

// Leathers with armor traits
"@items/materials/leathers:*[itemTypeTraits.armor EXISTS]"

// Rare materials (high rarityWeight)
"@items/materials/*:*[rarityWeight>=50]"

// Lightweight materials
"@items/materials/*:*[traits.weight<1.0]"

// Highly enchantable materials
"@items/materials/*:*[traits.enchantability>=50]"

// Specific material
"@items/materials/metals:steel"
```

**Material Context Filters:**

| Filter | Description | Example |
|--------|-------------|---------|
| `itemTypeTraits.weapon EXISTS` | Material has weapon traits | Applied to swords, axes, etc. |
| `itemTypeTraits.armor EXISTS` | Material has armor traits | Applied to plate, mail, etc. |
| `itemTypeTraits.jewelry EXISTS` | Material has jewelry traits | Applied to rings, amulets |
| `traits.weight < N` | Material weight limit | Lightweight weapons/armor |
| `traits.durability >= N` | Minimum durability | Durable equipment |
| `traits.enchantability >= N` | Enchantment receptiveness | Magical items |

**Usage in Item Generation:**
1. Pattern with `materialRef` selected
2. Reference resolved with filters
3. Material selected by `rarityWeight` from filtered pool
4. Material `traits` + `itemTypeTraits[itemType]` applied to item
5. Material `name` used in `{material}` token

**Example Flow:**
```json
// Pattern in weapons/names.json
{
  "pattern": "{material} {base}",
  "rarityWeight": 50,
  "materialRef": "@items/materials/metals:*[itemTypeTraits.weapon EXISTS]"
}

// Resolves materials/catalog.json → filters to: Iron, Steel, Mithril, etc.
// Selects "Steel" (rarityWeight: 15)
// Applies: traits { durability: 120 } + itemTypeTraits.weapon { damage: 4, critChance: 2 }
// Result: "Steel Longsword" with enhanced stats
```

---

### Enchantment References

**Purpose:** Select enchantments for item generation with position filtering

**Location:** Used in `enchantmentSlots[].ref` field of item patterns

**Common Filters:**
```json
// Prefix enchantments
"@items/enchantments:*[position=prefix]"

// Suffix enchantments
"@items/enchantments:*[position=suffix]"

// Limited rarity enchantments
"@items/enchantments:*[position=suffix][rarityWeight<=100]"

// Element-only enchantments
"@items/enchantments:*[element_suffix EXISTS]"

// Combat enchantments
"@items/enchantments:*[combat_suffix EXISTS]"

// Quality-enhanced enchantments
"@items/enchantments:*[quality_prefix EXISTS]"
```

**Enchantment Position Filters:**

| Filter | Description | Example |
|--------|-------------|---------|
| `position=prefix` | Adjective form enchantments | "Flaming", "Frozen", "Ancient" |
| `position=suffix` | Noun phrase enchantments | "of Fire", "of Strength", "of the Bear" |
| `element_prefix EXISTS` | Has elemental prefix component | Fire, Ice, Lightning |
| `element_suffix EXISTS` | Has elemental suffix component | Fire, Ice, Lightning |
| `combat_suffix EXISTS` | Has combat suffix component | Strength, Defense, Might |
| `magic_suffix EXISTS` | Has magic suffix component | Intelligence, Wisdom, Power |
| `quality_prefix EXISTS` | Has quality prefix component | Minor, Greater, Superior |

**Usage in Item Generation:**
1. Pattern with `enchantmentSlots[]` selected
2. For each slot, resolve `ref` with filters
3. EnchantmentGenerator creates enchantment from enchantments/names.json
4. Enchantment pattern selected by `rarityWeight`
5. Components selected and traits applied to item
6. Enchantment name used in `{enchantment_prefix}` or `{enchantment_suffix}` token

**Example Flow:**
```json
// Pattern in weapons/names.json
{
  "pattern": "{material} {base} {enchantment_suffix}",
  "rarityWeight": 25,
  "materialRef": "@items/materials/metals:*",
  "enchantmentSlots": [
    {
      "type": "suffix",
      "ref": "@items/enchantments:*[position=suffix][rarityWeight<=150]",
      "rarityWeightMax": 150
    }
  ]
}

// Resolves to suffix enchantments ≤ 150 rarityWeight
// Generates: "of Fire Strength" (Fire: 25 + Strength: 30 = 55)
// Applies traits: { fireDamage: 10, bonusStrength: 2 }
// Result: "Steel Longsword of Fire Strength"
```

---

### Gem References (Future)

**Purpose:** Reference gem catalog for socketing system

**Location:** Used in gem socket resolution

**Common Filters:**
```json
// Combat gems (red sockets)
"@items/gems/combat:*[color=red]"

// Magic gems (blue sockets)
"@items/gems/magic:*[color=blue]"

// Agility gems (green sockets)
"@items/gems/agility:*[color=green]"

// Level-restricted gems
"@items/gems/*:*[level<=3]"

// Universal gems
"@items/gems/*:*[color=white]"
```

**Note:** Gem system is planned for future implementation. References documented here for completeness.

---

### Quest Objective References

**Purpose:** Select quest objectives dynamically based on quest type

**Location:** Used by QuestGenerator when creating quests

**Common Filters:**
```json
// Kill objectives
"@quests/objectives/kill_objectives/*:*"

// Boss fights
"@quests/objectives/kill_objectives/boss_fight:*"

// High reward objectives
"@quests/objectives/*:*[rewardMultiplier>=2.0]"

// Enemy-related objectives with level filter
"@quests/objectives/*:*[enemyReference EXISTS][rewardMultiplier>=1.5]"

// Fetch quests with rare items
"@quests/objectives/fetch_objectives/*:*[itemReference MATCHES @items.*rarityWeight>=100]"
```

**Objective Filter Context:**

| Filter | Description | Example |
|--------|-------------|---------|
| `rewardMultiplier >= N` | Minimum reward scaling | High-value quests |
| `enemyReference EXISTS` | Has enemy target | Combat objectives |
| `itemReference EXISTS` | Has item requirement | Collection objectives |
| `npcReference EXISTS` | Has NPC interaction | Escort/dialogue objectives |
| `timeLimit EXISTS` | Has time constraint | Urgent missions |

**Usage in QuestGenerator:**
1. Select quest type (kill, fetch, escort, etc.)
2. Load objectives: `@quests/objectives/{type}_objectives/*:*`
3. Apply filters based on quest context
4. Select objective by `rarityWeight`
5. Resolve embedded references (`enemyReference`, `itemReference`)
6. Generate complete objective description

---

### Quest Reward References

**Purpose:** Select quest rewards dynamically based on quest difficulty

**Location:** Used by QuestGenerator after objective selection

**Common Filters:**
```json
// Gold rewards
"@quests/rewards/gold_rewards/*:*"

// Item rewards
"@quests/rewards/item_rewards/weapons:*"

// High-value rewards
"@quests/rewards/*:*[rarityWeight>=50]"

// Rare item rewards
"@quests/rewards/item_rewards/*:*[itemReference MATCHES .*rarityWeight>=100]"

// Reputation rewards for specific faction
"@quests/rewards/reputation_rewards/*:*[factionReference=@organizations/factions:merchants_guild]"
```

**Reward Filter Context:**

| Filter | Description | Example |
|--------|-------------|---------|
| `goldMin >= N` | Minimum gold amount | High-paying quests |
| `itemReference EXISTS` | Rewards include items | Equipment/loot rewards |
| `factionReference EXISTS` | Grants reputation | Faction quests |
| `abilityReference EXISTS` | Teaches abilities | Training quests |
| `xpMin >= N` | Minimum XP reward | Leveling quests |

**Usage in QuestGenerator:**
1. Get `rewardMultiplier` from objective
2. Select reward type(s) based on quest tier
3. Load rewards: `@quests/rewards/{type}_rewards/*:*`
4. Apply multiplier to gold/xp amounts
5. Resolve item/ability/faction references
6. Return complete reward package

**Example Flow:**
```json
// Objective: Boss fight (rewardMultiplier: 3.0)
// Reward selection:
{
  "goldReward": "@quests/rewards/gold_rewards/*:*[rarityWeight>=40]",
  "itemReward": "@quests/rewards/item_rewards/weapons:*[rarityWeight>=100]"
}
// Resolves to:
// - Gold: 300-1500 (100-500 × 3.0 multiplier)
// - Item: "Rare Legendary Sword" from filtered pool
```

---

## Reference Quick Lookup

| Domain | Path Example | Use Case |
|--------|-------------|----------|
| `@items` | `@items/weapons/swords:longsword` | Equipment, loot, crafting |
| `@items/materials` | `@items/materials/metals:steel` | Material selection for item generation |
| `@items/enchantments` | `@items/enchantments:*[position=suffix]` | Enchantment selection for items |
| `@items/gems` | `@items/gems/combat:*[color=red]` | Gem socketing (future) |
| `@abilities` | `@abilities/active/offensive:fireball` | Class abilities, enemy skills |
| `@classes` | `@classes/warriors:fighter` | Character classes, NPCs |
| `@enemies` | `@enemies/beasts/wolves:dire-wolf` | Combat encounters |
| `@npcs` | `@npcs/backgrounds:noble` | NPC generation |
| `@quests` | `@quests/templates/fetch:easy-fetch` | Quest system |
| `@quests/objectives` | `@quests/objectives/kill_objectives/*:*` | Dynamic quest objectives |
| `@quests/rewards` | `@quests/rewards/item_rewards/*:*` | Dynamic quest rewards |
| `@organizations/factions` | `@organizations/factions:merchants_guild` | Faction reputation |
| `@world/locations` | `@world/locations/towns:stormhaven` | Quest locations |

---

## Related Documentation

- [JSON_STRUCTURE_TYPES.md](./JSON_STRUCTURE_TYPES.md) - Structure type definitions
- [CATALOG_JSON_STANDARD.md](./CATALOG_JSON_STANDARD.md) - Catalog file format
- [NAMES_JSON_STANDARD.md](./NAMES_JSON_STANDARD.md) - Pattern generation with references
- [README.md](./README.md) - JSON standards overview

---

## Version History

### v4.2 (2025-12-31)
- Added material references with `itemTypeTraits` context filtering
- Added enchantment references with `position` filtering
- Added gem references (placeholder for future implementation)
- Added quest objective references with reward multipliers
- Added quest reward references with embedded reference filtering
- Enhanced filter examples for generation context

### v4.1 (2025-12-28)
- Initial reference system specification
- Wildcard selection with filters
- Optional references with `?` suffix
- Property access with dot notation
- Regex matching support
- Max 3 filter conditions

---

## Implementation Checklist

- [ ] Create `IReferenceResolver` interface
- [ ] Implement reference parser (regex or parser combinator)
- [ ] Add load-time validation
- [ ] Support wildcard selection with `rarityWeight`
- [ ] Implement filter engine (comparison operators + AND/OR)
- [ ] Add regex matching support
- [ ] Property access with nested navigation
- [ ] Optional reference handling (? suffix)
- [ ] Error messages with file location context
- [ ] Performance optimization (caching, lazy loading)
- [ ] Unit tests for all reference patterns
- [ ] Integration tests with actual JSON files
