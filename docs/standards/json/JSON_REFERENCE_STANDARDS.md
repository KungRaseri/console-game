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

## Reference Quick Lookup

| Domain | Path Example | Use Case |
|--------|-------------|----------|
| `@items` | `@items/weapons/swords:longsword` | Equipment, loot, crafting |
| `@abilities` | `@abilities/active/offensive:fireball` | Class abilities, enemy skills |
| `@classes` | `@classes/warriors:fighter` | Character classes, NPCs |
| `@enemies` | `@enemies/beasts/wolves:dire-wolf` | Combat encounters |
| `@npcs` | `@npcs/backgrounds:noble` | NPC generation |
| `@quests` | `@quests/templates/fetch:easy-fetch` | Quest system |

---

## Related Documentation

- [JSON_STRUCTURE_TYPES.md](./JSON_STRUCTURE_TYPES.md) - Structure type definitions
- [CATALOG_JSON_STANDARD.md](./CATALOG_JSON_STANDARD.md) - Catalog file format
- [NAMES_JSON_STANDARD.md](./NAMES_JSON_STANDARD.md) - Pattern generation with references
- [README.md](./README.md) - JSON standards overview

---

## Version History

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
