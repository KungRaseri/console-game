# ContentBuilder Editor Type Reference

Quick visual guide for assigning the correct editor to JSON files.

## Visual Decision Tree

```
Is the file structure...

├─ Simple string array? → NameList
│  Example: { "names": ["item1", "item2"] }
│  Files: weapon_names.json, adjectives.json
│
├─ Has rarity levels (common/rare/epic)?
│  ├─ Is it a PREFIX? → ItemPrefix
│  │  Example: { "common": { "Sharp": { traits } }, "rare": { ... } }
│  │  Files: weapons/prefixes.json, beasts/prefixes.json
│  │
│  └─ Is it a SUFFIX? → ItemSuffix  
│     Example: { "power": { "of Power": { traits } }, "speed": { ... } }
│     Files: enchantments/suffixes.json
│
├─ Has "items" array + "components"/"patterns"? → HybridArray
│  Example: { "items": [...], "components": {...}, "patterns": [...] }
│  Files: colors.json, weapons/suffixes.json, armor/prefixes.json
│
└─ Just Item → Traits (no rarity levels)? → FlatItem
   Example: { "ItemName": { "displayName": "...", "traits": {...} } }
   Files: metals.json, woods.json, dragons/colors.json
```

## Complete Category Mapping

### General
- Colors → HybridArray
- Smells → HybridArray
- Sounds → HybridArray
- Textures → HybridArray
- Time of Day → HybridArray
- Weather → HybridArray
- Verbs → HybridArray
- Adjectives → NameList
- Materials → NameList

### Items

#### Weapons
- Names → NameList
- **Prefixes → ItemPrefix** ⭐
- Suffixes → HybridArray

#### Armor
- Names → HybridArray
- Prefixes → HybridArray
- Suffixes → HybridArray
- Materials → FlatItem

#### Consumables
- Names → HybridArray
- Effects → HybridArray
- Rarities → HybridArray

#### Enchantments
- Prefixes → HybridArray
- Effects → HybridArray
- **Suffixes → ItemSuffix** ⭐

#### Materials
- Metals → FlatItem
- Woods → FlatItem
- Leathers → FlatItem
- Gemstones → FlatItem

### Enemies

#### Pattern: All enemy types follow this structure
- Names → NameList
- **Prefixes → ItemPrefix** ⭐
- Traits → HybridArray
- Suffixes → HybridArray

#### Beasts
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Traits → HybridArray
- Suffixes → HybridArray

#### Demons
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Traits → HybridArray
- Suffixes → HybridArray

#### Dragons
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Colors → FlatItem
- Traits → HybridArray
- Suffixes → HybridArray

#### Elementals
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Traits → HybridArray
- Suffixes → HybridArray

#### Humanoids
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Traits → HybridArray
- Suffixes → HybridArray

#### Undead
- Names → NameList
- **Prefixes → ItemPrefix** ⭐ (was FlatItem)
- Traits → HybridArray
- Suffixes → HybridArray

#### Vampires
- Traits → HybridArray
- Suffixes → HybridArray

#### Goblinoids
- Traits → HybridArray
- Suffixes → HybridArray

⭐ = Changed during synchronization

## Editor Feature Comparison

| Editor | Supports Rarity | Supports Items | Supports Components | Supports Patterns | Supports Traits |
|--------|----------------|----------------|---------------------|-------------------|-----------------|
| NameList | ❌ | ❌ | ❌ | ❌ | ❌ |
| ItemPrefix | ✅ | ✅ | ❌ | ❌ | ✅ |
| ItemSuffix | ✅ | ✅ | ❌ | ❌ | ✅ |
| FlatItem | ❌ | ✅ | ❌ | ❌ | ✅ |
| HybridArray | ❌ | ✅ | ✅ | ✅ | ❌ |

## Quick Identification Guide

### Look for these keywords in the JSON:

**"common", "uncommon", "rare", "epic", "legendary"** → ItemPrefix or ItemSuffix  
**"items": [...]** → HybridArray  
**"components": {...}** → HybridArray  
**"patterns": [...]** → HybridArray  
**"names": [...]** → NameList  
**Direct item → traits mapping** → FlatItem

## When in Doubt

1. Open the JSON file
2. Look at the top-level structure
3. Match it to one of the examples above
4. Choose the corresponding editor type
5. Test by loading it in the ContentBuilder
