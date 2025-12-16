# Pattern System Standardization Plan

**Date:** December 16, 2025  
**Status:** 🚀 In Progress

## Overview

Standardize the pattern system across all JSON data files, update ContentBuilder to enforce the standard, then implement runtime pattern execution in the game.

## Phase 1: Define the Standard ✅

### Component Key Naming Convention

**Simple, intuitive keys that match pattern tokens exactly.**

#### Key Categories

1. **Prefixes** (describe the base item)
   - `material` - Physical material (Iron, Steel, Wood, Leather)
   - `quality` - Craftsmanship level (Fine, Superior, Masterwork)
   - `descriptive` - Special attributes (Ancient, Enchanted, Cursed)
   - `size` - Size modifiers (Small, Large, Massive)
   - `color` - Color descriptors (Red, Blue, Golden)
   - `origin` - Origin/creator (Elven, Dwarven, Dragon)
   - `condition` - State/age (Worn, Pristine, Ruined)

2. **Suffixes** (add-ons, typically start with "of")
   - `enchantment` - Magical properties (of Slaying, of Power)
   - `title` - Named items (of the Dragon, of the Hero)
   - `purpose` - Intended use (of Defense, of Battle)

3. **Special Tokens**
   - `base` - Resolves to items array
   - `item` - Alias for base

4. **Categories** (organizational, nested objects)
   - `weapon_types` - {swords: [], axes: [], ...}
   - `armor_types` - {helmets: [], chest: [], ...}
   - `enemy_types` - {beasts: [], undead: [], ...}

### Pattern Syntax

```
pattern ::= token | token " + " pattern
token   ::= "base" | component_key
```

**Examples:**
- `"base"` → "Longsword"
- `"material + base"` → "Iron Longsword"
- `"quality + material + base"` → "Fine Steel Longsword"
- `"base + enchantment"` → "Longsword of Slaying"
- `"descriptive + base + title"` → "Ancient Longsword of the Dragon"

### Pattern Execution Rules

1. Tokens are evaluated left to right
2. Each token picks a random value from its component array
3. Special token `base` picks from items array
4. Results are joined with spaces
5. If a token's component is empty, skip it (graceful degradation)

## Phase 2: Standardize JSON Files 🔄

### Files to Update (HybridArray Structure)

#### Items
- [x] `items/weapons/names.json` - Already has structure, needs cleanup
- [ ] `items/armor/names.json` - Needs standardization
- [ ] `items/accessories/names.json` - Needs standardization
- [ ] `items/consumables/names.json` - Needs standardization

#### Enemies
- [ ] `enemies/beasts/names.json`
- [ ] `enemies/undead/names.json`
- [ ] `enemies/demons/names.json`
- [ ] `enemies/elementals/names.json`
- [ ] `enemies/dragons/names.json`
- [ ] `enemies/humanoids/names.json`

#### NPCs
- [ ] `npcs/names/first_names.json`
- [ ] `npcs/titles/titles.json`

### Standard Template

```json
{
  "items": [
    "Base1", "Base2", "Base3"
  ],
  "components": {
    "material": ["Iron", "Steel"],
    "quality": ["Fine", "Superior"],
    "descriptive": ["Ancient", "Enchanted"],
    "enchantment": ["of Slaying", "of Power"],
    "category_types": {
      "type1": ["Item1", "Item2"],
      "type2": ["Item3", "Item4"]
    }
  },
  "patterns": [
    "base",
    "material + base",
    "quality + material + base",
    "descriptive + base",
    "base + enchantment",
    "material + base + enchantment"
  ],
  "metadata": {
    "description": "Description of this file",
    "version": "1.0",
    "last_updated": "2025-12-16"
  }
}
```

### weapons/names.json Cleanup

**Current issues:**
- Components use `prefixes_*` and `suffixes_*` naming
- Patterns use abbreviated tokens that don't match
- Categories OK (weapon_types)

**Changes needed:**
1. Rename `prefixes_material` → `material`
2. Rename `prefixes_quality` → `quality`
3. Rename `prefixes_descriptive` → `descriptive`
4. Rename `suffixes_enchantment` → `enchantment`
5. Update patterns to use new names
6. Keep `weapon_types` as-is (category)

## Phase 3: Update ContentBuilder 🔧

### PatternExampleGenerator Updates

**File:** `Game.ContentBuilder/Services/PatternExampleGenerator.cs`

**Changes:**
1. Simplify token resolution logic
2. Remove fuzzy matching and prefixes_/suffixes_ attempts
3. Direct mapping: token → components[token]
4. Special case: `base` or `item` → items array
5. Better error messages for invalid tokens

### HybridArrayEditorViewModel Updates

**File:** `Game.ContentBuilder/ViewModels/HybridArrayEditorViewModel.cs`

**Features to add:**
1. Pattern validation on add/edit
2. Real-time pattern testing (show 5 examples)
3. Token autocomplete/suggestion
4. Warning if token doesn't match any component

### UI Enhancements

**File:** `Game.ContentBuilder/Views/HybridArrayEditorView.xaml`

**Features:**
1. Pattern validation indicator (✓ valid, ⚠ warning, ✗ invalid)
2. Live example preview (regenerate on keystroke)
3. Component key reference panel
4. Pattern syntax helper tooltip

## Phase 4: Runtime Implementation 🎮

### Create PatternExecutor Service

**File:** `Game.Shared/Services/PatternExecutor.cs`

```csharp
public class PatternExecutor
{
    /// <summary>
    /// Execute a pattern like "quality + material + base" to generate a name
    /// </summary>
    public static string Execute(
        string pattern, 
        List<string> items, 
        Dictionary<string, List<string>> components, 
        Random random)
    {
        var tokens = pattern.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToArray();
        
        var parts = new List<string>();
        
        foreach (var token in tokens)
        {
            string? value = ResolveToken(token, items, components, random);
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add(value);
            }
        }
        
        return parts.Count > 0 
            ? string.Join(" ", parts) 
            : items[random.Next(items.Count)]; // Fallback to random item
    }
    
    private static string? ResolveToken(
        string token, 
        List<string> items, 
        Dictionary<string, List<string>> components, 
        Random random)
    {
        // Special tokens
        if (token == "base" || token == "item")
        {
            return items.Count > 0 ? items[random.Next(items.Count)] : null;
        }
        
        // Component lookup
        if (components.TryGetValue(token, out var componentList) && componentList.Count > 0)
        {
            return componentList[random.Next(componentList.Count)];
        }
        
        // Token not found - log warning and skip
        Log.Warning("Pattern token '{Token}' not found in components", token);
        return null;
    }
}
```

### Update Data Models

**File:** `Game.Shared/Data/Models/GameDataModels.cs`

Add `Patterns` property to data classes:

```csharp
public class WeaponNameData
{
    public List<string> Items { get; set; } = new(); // Changed from WeaponNameItems
    public Dictionary<string, object> Components { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

### Update Generators

**File:** `Game.Core/Generators/ItemGenerator.cs`

Replace manual name construction with pattern execution:

```csharp
private static string GenerateWeaponName(Faker f, Item item)
{
    var data = GameDataService.Instance.WeaponNames;
    
    // Pick a random pattern
    if (data.Patterns.Count > 0)
    {
        var pattern = f.PickRandom(data.Patterns);
        return PatternExecutor.Execute(
            pattern, 
            data.Items, 
            data.Components, 
            f.Random
        );
    }
    
    // Fallback to random item if no patterns
    return f.PickRandom(data.Items);
}
```

**Similar updates for:**
- `EnemyGenerator.cs` - Use patterns for enemy names
- `NpcGenerator.cs` - Use patterns for NPC names/titles

## Phase 5: Testing & Validation ✅

### Unit Tests

**File:** `Game.Tests/Services/PatternExecutorTests.cs`

```csharp
[Fact]
public void Execute_SimplePattern_GeneratesName()
{
    var items = new List<string> { "Sword", "Axe" };
    var components = new Dictionary<string, List<string>>
    {
        ["material"] = new List<string> { "Iron", "Steel" }
    };
    
    var result = PatternExecutor.Execute("material + base", items, components, new Random(42));
    
    result.Should().MatchRegex(@"^(Iron|Steel) (Sword|Axe)$");
}

[Fact]
public void Execute_InvalidToken_SkipsToken()
{
    var items = new List<string> { "Sword" };
    var components = new Dictionary<string, List<string>>();
    
    var result = PatternExecutor.Execute("invalid + base", items, components, new Random());
    
    result.Should().Be("Sword"); // Invalid token skipped
}
```

### Integration Tests

**File:** `Game.Tests/Generators/ItemGeneratorIntegrationTests.cs`

```csharp
[Fact]
public void GenerateWeaponName_UsesPatterns_FromJsonData()
{
    // Ensure JSON data is loaded
    GameDataService.Initialize("../../../Data/Json");
    
    var item = new Item { Rarity = ItemRarity.Rare };
    var faker = new Faker();
    
    // Generate 100 names, ensure they follow pattern structure
    for (int i = 0; i < 100; i++)
    {
        var name = ItemGenerator.GenerateWeaponName(faker, item);
        name.Should().NotBeNullOrEmpty();
        // Could add more specific validation
    }
}
```

### ContentBuilder Tests

**File:** `Game.ContentBuilder.Tests/Services/PatternExampleGeneratorTests.cs`

```csharp
[Fact]
public void GenerateExample_StandardPattern_CreatesValidExample()
{
    var items = new JArray { "Sword", "Axe" };
    var components = new JObject
    {
        ["material"] = new JArray { "Iron", "Steel" },
        ["quality"] = new JArray { "Fine", "Superior" }
    };
    
    var result = PatternExampleGenerator.GenerateExample(
        "quality + material + base", 
        items, 
        components
    );
    
    result.Should().MatchRegex(@"^(Fine|Superior) (Iron|Steel) (Sword|Axe)$");
}
```

## Phase 6: Documentation 📚

### Files to Create/Update

1. **GDD Update** - Add pattern system documentation
2. **Component Key Reference** - List of standard component keys
3. **Pattern Syntax Guide** - How to write patterns
4. **Migration Guide** - For updating existing JSON files
5. **ContentBuilder User Guide** - How to use pattern features

### Example Documentation

**Pattern System Guide** (`docs/guides/PATTERN_SYSTEM.md`):

```markdown
# Pattern System Guide

## Overview
The pattern system generates dynamic names by combining components using patterns.

## Basic Syntax
`token + token + ...`

## Tokens
- `base` - Random item from items array
- `material` - Random material component
- `quality` - Random quality component
- `enchantment` - Random enchantment suffix

## Examples
- `"base"` → "Longsword"
- `"material + base"` → "Iron Longsword"
- `"quality + material + base"` → "Fine Steel Longsword"

## Adding New Patterns
1. Open ContentBuilder
2. Navigate to file with patterns
3. Go to Patterns tab
4. Type pattern in text box (autocomplete available)
5. See live examples as you type
6. Click Add
7. Save file
```

## Execution Timeline

### Day 1: Standardization ✅
- [x] Define standard (this document)
- [ ] Update weapons/names.json
- [ ] Update armor/names.json
- [ ] Update 2-3 enemy name files
- [ ] Test in ContentBuilder

### Day 2: ContentBuilder Updates
- [ ] Update PatternExampleGenerator
- [ ] Add pattern validation
- [ ] Add live example preview
- [ ] Add token autocomplete
- [ ] Test all updated files

### Day 3: Runtime Implementation
- [ ] Create PatternExecutor service
- [ ] Update data models
- [ ] Update ItemGenerator
- [ ] Update EnemyGenerator
- [ ] Write unit tests
- [ ] Integration testing

### Day 4: Polish & Documentation
- [ ] Write pattern system guide
- [ ] Update GDD
- [ ] Create migration guide
- [ ] Final testing
- [ ] Code review

## Success Criteria

✅ **Data Standardization**
- All JSON files use consistent component keys
- All patterns use tokens that match component keys
- No hardcoded abbreviations or aliases

✅ **ContentBuilder**
- Pattern validation works
- Live examples generate correctly
- Invalid patterns show warnings
- Save/load preserves structure

✅ **Runtime**
- PatternExecutor generates valid names
- Generators use patterns from JSON
- No crashes on invalid patterns
- Graceful degradation

✅ **Quality**
- Unit test coverage >80%
- Integration tests pass
- No performance regressions
- Documentation complete

## Rollback Plan

If issues arise:
1. Keep old generator code in comments
2. Feature flag: `USE_PATTERN_EXECUTION` (default: false)
3. Gradual rollout: Enable for one category at a time
4. Monitor logs for pattern resolution failures
5. Can revert to manual generation if needed

## Notes

- Keep weapon_types, armor_types as nested objects (organizational)
- Don't validate category objects as patterns (they're data groupings)
- Pattern execution should be fast (<1ms per name)
- Cache compiled patterns if performance becomes issue
- Consider pattern weights in future (some patterns more common)
