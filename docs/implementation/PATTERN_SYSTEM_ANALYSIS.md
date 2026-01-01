# Pattern System Analysis & Redesign

**Date:** December 16, 2025  
**Status:** 🔍 Analysis Complete - Needs Implementation

## Current State

### Problem Identified

The **patterns in the JSON files are NOT being used by the runtime code**! 

**Evidence:**
1. `RealmEngine.Core/Generators/ItemGenerator.cs` manually constructs weapon names
2. No code references or parses the "patterns" array from JSON
3. Pattern system only exists in ContentBuilder for UI examples
4. Components like `weapon_types`, `prefixes_material`, etc. are defined but patterns don't reference them correctly

### Current JSON Structure (weapons/names.json)

```json
{
  "items": [
    "Longsword", "Shortsword", "Greatsword", ...
  ],
  "components": {
    "weapon_types": {
      "swords": [...],
      "axes": [...],
      "bows": [...]
    },
    "prefixes_material": ["Iron", "Steel", ...],
    "prefixes_quality": ["Fine", "Superior", ...],
    "prefixes_descriptive": ["Ancient", "Enchanted", ...],
    "suffixes_enchantment": ["of Slaying", "of Power", ...]
  },
  "patterns": [
    "material + base",              // ❌ What is "material"? Not a component key!
    "quality + material + base",    // ❌ What is "quality"? Not a component key!
    "base + suffix",                // ❌ What is "suffix"? Not a component key!
    "descriptive + base",           // ❌ What is "descriptive"? Not a component key!
    "quality + descriptive + base + suffix"
  ]
}
```

### The Disconnect

**Component Keys:**
- `weapon_types`
- `prefixes_material`
- `prefixes_quality`
- `prefixes_descriptive`
- `suffixes_enchantment`

**Pattern Tokens (don't match!):**
- `material` (should be `prefixes_material`?)
- `quality` (should be `prefixes_quality`?)
- `descriptive` (should be `prefixes_descriptive`?)
- `suffix` (should be `suffixes_enchantment`?)
- `base` (should be `items`?)

### Current Runtime Code (ItemGenerator.cs)

```csharp
private static string GenerateWeaponName(Faker f, Item item)
{
    var weaponType = f.PickRandom("swords", "axes", "bows", ...);
    var weaponList = weaponType switch
    {
        "swords" => data.WeaponNames.Items.Swords,
        "axes" => data.WeaponNames.Items.Axes,
        // ...
    };
    
    var weaponName = GameDataService.GetRandom(weaponList);
    
    // Manually construct: material + prefix + weapon name
    string? materialPrefix = ApplyMetalMaterial(item, f);
    string? prefixName = f.Random.Bool(0.3f) ? GetPrefixByRarity(item.Rarity) : null;
    
    if (materialPrefix != null && prefixName != null)
        return $"{prefixName} {materialPrefix} {weaponName}";
    else if (materialPrefix != null)
        return $"{materialPrefix} {weaponName}";
    else if (prefixName != null)
        return $"{prefixName} {weaponName}";
    else
        return weaponName;
}
```

**Issues:**
1. Hardcoded logic - not using patterns from JSON
2. Inconsistent with what patterns claim to do
3. No way to add new patterns without code changes

## Proposed Solution

### Design Principles

1. **Component Keys = Pattern Tokens**
   - Pattern tokens should EXACTLY match component keys
   - No abbreviations or aliases
   - Clear, unambiguous mapping

2. **Base = Items Array**
   - `items` array is the base/core names
   - Special token: `base` or `item` resolves to items array

3. **Patterns Should Be Executable**
   - Runtime code should parse and execute patterns
   - No hardcoded name generation logic
   - Data-driven approach

### Proposed JSON Structure

```json
{
  "items": [
    "Longsword", "Shortsword", "Greatsword"
  ],
  "components": {
    "material": ["Iron", "Steel", "Mithril"],
    "quality": ["Fine", "Superior", "Exceptional"],
    "descriptive": ["Ancient", "Enchanted", "Cursed"],
    "suffix": ["of Slaying", "of Power", "of Speed"]
  },
  "patterns": [
    "material + base",
    "quality + material + base",
    "base + suffix",
    "descriptive + base",
    "quality + descriptive + base + suffix"
  ]
}
```

**OR** (more explicit):

```json
{
  "items": [
    "Longsword", "Shortsword", "Greatsword"
  ],
  "components": {
    "prefixes_material": ["Iron", "Steel", "Mithril"],
    "prefixes_quality": ["Fine", "Superior", "Exceptional"],
    "prefixes_descriptive": ["Ancient", "Enchanted", "Cursed"],
    "suffixes_enchantment": ["of Slaying", "of Power", "of Speed"]
  },
  "patterns": [
    "prefixes_material + base",
    "prefixes_quality + prefixes_material + base",
    "base + suffixes_enchantment",
    "prefixes_descriptive + base",
    "prefixes_quality + prefixes_descriptive + base + suffixes_enchantment"
  ]
}
```

### Recommended Approach: Simplified Keys

**I recommend the FIRST approach (simplified keys)** because:
- ✅ Patterns are more readable: `"quality + material + base"`
- ✅ Shorter, cleaner JSON
- ✅ Easier to understand intent
- ✅ Less typing in ContentBuilder
- ❌ Prefixes lose explicit categorization (but we can infer from position)

### Pattern Execution Engine

Create a new service: `PatternExecutor`

```csharp
public class PatternExecutor
{
    /// <summary>
    /// Execute a pattern like "quality + material + base" using component data
    /// </summary>
    public static string Execute(
        string pattern, 
        JArray items, 
        JObject components, 
        Faker faker)
    {
        var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToArray();
        
        var parts = new List<string>();
        
        foreach (var token in tokens)
        {
            string? value = null;
            
            if (token == "base" || token == "item")
            {
                // Get random from items array
                value = items.Count > 0 
                    ? items[faker.Random.Int(0, items.Count - 1)].ToString() 
                    : null;
            }
            else if (components[token] is JArray componentArray)
            {
                // Get random from component array
                value = componentArray.Count > 0 
                    ? componentArray[faker.Random.Int(0, componentArray.Count - 1)].ToString() 
                    : null;
            }
            
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add(value);
            }
        }
        
        return string.Join(" ", parts);
    }
}
```

### Updated ItemGenerator

```csharp
private static string GenerateWeaponName(Faker f, Item item)
{
    var data = GameDataService.Instance;
    
    // Get weapon data with patterns
    var weaponData = LoadWeaponDataWithPatterns(); // New method
    
    // Pick a random pattern
    var pattern = f.PickRandom(weaponData.Patterns);
    
    // Execute the pattern to generate name
    return PatternExecutor.Execute(
        pattern, 
        weaponData.Items, 
        weaponData.Components, 
        f
    );
}
```

## Migration Plan

### Phase 1: Standardize Existing Patterns ✅ (ContentBuilder Only)

**Goal:** Make patterns match component keys for UI examples

**Files to Update:**
1. ✅ `items/weapons/names.json`
2. ✅ `items/armor/names.json` (if exists)
3. ✅ All other files with HybridArray structure

**Changes:**
- Decide on naming convention (simple vs. explicit)
- Update all pattern tokens to match component keys
- Update PatternExampleGenerator to handle the new format
- Test in ContentBuilder UI

### Phase 2: Implement Pattern Execution Engine

**Goal:** Make runtime code use patterns from JSON

**Files to Create:**
1. `RealmEngine.Shared/Services/PatternExecutor.cs` - Core pattern parsing/execution
2. `RealmEngine.Shared/Data/Models/PatternData.cs` - Data models with patterns property

**Files to Update:**
1. `RealmEngine.Shared/Data/Models/GameDataModels.cs` - Add Patterns property
2. `RealmEngine.Core/Generators/ItemGenerator.cs` - Use PatternExecutor instead of manual logic
3. `RealmEngine.Core/Generators/EnemyGenerator.cs` - Use PatternExecutor for enemy names
4. `RealmEngine.Core/Generators/NpcGenerator.cs` - Use PatternExecutor for NPC names

### Phase 3: ContentBuilder Pattern Validation

**Goal:** Validate patterns in ContentBuilder before saving

**Features:**
1. Real-time pattern validation (show errors if token doesn't match component)
2. Pattern tester: show 5-10 examples when typing pattern
3. Component key dropdown/autocomplete when typing patterns
4. Warning if pattern references non-existent component

### Phase 4: Advanced Pattern Features

**Goal:** Support more complex patterns

**Features:**
1. **Optional tokens:** `"material? + base"` (30% chance to include material)
2. **Rarity-based selection:** `"material[rarity] + base"` (pick material based on item rarity)
3. **Category selection:** `"material[metal] + base"` (only metal materials)
4. **Weighted patterns:** Some patterns more common than others
5. **Nested patterns:** `"(quality + material) + base"` (group tokens)

## Component Key Naming Convention

### Option A: Simple/Short Keys (RECOMMENDED)

```json
"components": {
  "material": [...],
  "quality": [...],
  "descriptive": [...],
  "enchantment": [...],
  "size": [...],
  "color": [...],
  "condition": [...],
  "origin": [...]
}
```

**Patterns:**
```
"quality + material + base"
"base + enchantment"
"color + base + enchantment"
```

**Pros:**
- ✅ Clean, readable patterns
- ✅ Easy to type and remember
- ✅ Matches common language (quality, material, etc.)
- ✅ Less verbose

**Cons:**
- ❌ Less explicit about prefix/suffix position
- ❌ May conflict if same word used for prefix and suffix

### Option B: Explicit Prefix/Suffix Keys

```json
"components": {
  "prefix_material": [...],
  "prefix_quality": [...],
  "prefix_descriptive": [...],
  "suffix_enchantment": [...],
  "suffix_condition": [...],
  "suffix_origin": [...]
}
```

**Patterns:**
```
"prefix_quality + prefix_material + base"
"base + suffix_enchantment"
"prefix_material + base + suffix_origin"
```

**Pros:**
- ✅ Very explicit about position
- ✅ No naming conflicts
- ✅ Clear intent

**Cons:**
- ❌ Verbose patterns
- ❌ More typing in ContentBuilder
- ❌ Less intuitive for non-technical users

### Option C: Position Notation

```json
"components": {
  "1_quality": [...],     // Position 1 (leftmost prefix)
  "2_material": [...],    // Position 2
  "3_descriptive": [...], // Position 3
  "suffix_1": [...],      // Suffix position 1
  "suffix_2": [...]       // Suffix position 2
}
```

**Patterns:**
```
"1_quality + 2_material + base"
"base + suffix_1"
"1_quality + base + suffix_1 + suffix_2"
```

**Pros:**
- ✅ Enforces ordering
- ✅ Very explicit

**Cons:**
- ❌ Not intuitive to read
- ❌ Numbers don't convey meaning
- ❌ Harder to remember

## Recommendation

### Use Option A: Simple Keys with Positional Context

**Naming Rules:**
1. **Prefixes** = Words that describe (material, quality, size, color, origin, descriptive)
2. **Suffixes** = Phrases starting with "of" (enchantment, power, origin)
3. **Base** = Special token for items array
4. **Categories** = Nested objects for organization (weapon_types, armor_types)

**Example: weapons/names.json**

```json
{
  "items": [
    "Longsword", "Greatsword", "Battleaxe"
  ],
  "components": {
    "material": ["Iron", "Steel", "Mithril", "Adamantine"],
    "quality": ["Fine", "Superior", "Exceptional", "Masterwork"],
    "descriptive": ["Ancient", "Enchanted", "Cursed", "Holy"],
    "enchantment": ["of Slaying", "of Power", "of Speed", "of Fire"],
    "origin": ["of the Dragon", "of the Phoenix", "of the Hero"],
    "weapon_types": {
      "swords": ["Longsword", "Greatsword", "Scimitar"],
      "axes": ["Battleaxe", "Handaxe", "Waraxe"],
      "bows": ["Longbow", "Crossbow"]
    }
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base",
    "descriptive + base",
    "base + enchantment",
    "material + base + enchantment",
    "quality + descriptive + base + origin"
  ]
}
```

**Generated Names:**
- `"base"` → "Longsword"
- `"material + base"` → "Steel Greatsword"
- `"quality + material + base"` → "Superior Mithril Battleaxe"
- `"descriptive + base"` → "Ancient Longsword"
- `"base + enchantment"` → "Greatsword of Slaying"
- `"material + base + enchantment"` → "Iron Battleaxe of Fire"
- `"quality + descriptive + base + origin"` → "Masterwork Enchanted Longsword of the Dragon"

## Implementation Checklist

### Immediate (ContentBuilder Only)
- [ ] Decide on naming convention (recommend Option A)
- [ ] Update weapons/names.json with clean component keys
- [ ] Update all pattern strings to match component keys
- [ ] Test PatternExampleGenerator with new format
- [ ] Document the convention in GDD

### Short-term (Runtime Integration)
- [ ] Create PatternExecutor service
- [ ] Update GameDataModels to include Patterns property
- [ ] Update ItemGenerator to use PatternExecutor
- [ ] Add unit tests for pattern execution
- [ ] Test generated names in game

### Long-term (Advanced Features)
- [ ] Add pattern validation in ContentBuilder
- [ ] Add real-time pattern testing UI
- [ ] Implement optional tokens (`?` suffix)
- [ ] Implement rarity-based selection (`[rarity]`)
- [ ] Implement category selection (`[category]`)
- [ ] Add pattern weights/probabilities

## Questions to Answer

1. **Should patterns be validated at save time?** 
   - Yes, ContentBuilder should warn if pattern references non-existent component

2. **Should we support wildcard patterns?**
   - e.g., `"prefix_* + base"` matches any prefix_X component
   - Probably overkill for now

3. **Should base items be in separate files per weapon type?**
   - Current: One file with weapon_types nested object
   - Alternative: swords.json, axes.json, etc. each with own patterns
   - Keep current structure for simplicity

4. **Should patterns support conditional logic?**
   - e.g., `"material + base [if not legendary]"`
   - Not needed yet, can add later

## Conclusion

**Current state:** Patterns exist in JSON but are NOT used by runtime code.

**Proposed solution:** 
1. Standardize pattern tokens to match component keys (simple names)
2. Implement PatternExecutor to parse and execute patterns
3. Update generators to use PatternExecutor instead of hardcoded logic
4. Add validation and testing tools in ContentBuilder

**Benefit:** Fully data-driven name generation with no code changes needed to add new patterns or components.
