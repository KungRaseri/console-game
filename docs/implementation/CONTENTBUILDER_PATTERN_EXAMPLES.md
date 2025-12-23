# Pattern Example Generation Feature

**Date:** 2025-12-16  
**Status:** Complete  
**Component:** ContentBuilder HybridArray Editor

## Overview

Added automatic example generation for patterns in the ContentBuilder's HybridArray editor. Each pattern now displays a generated example based on actual data from the JSON file.

## Changes Made

### New Files

1. **Game.ContentBuilder/Models/PatternComponent.cs**
   - Model class representing a pattern with its auto-generated example
   - Properties: `Pattern` (string), `Example` (string)

2. **Game.ContentBuilder/Services/PatternExampleGenerator.cs**
   - Service class that generates examples for patterns
   - Parses pattern strings (e.g., `"material + base"`)
   - Resolves tokens by searching components and items
   - Handles various naming conventions (prefixes_, _types, etc.)

### Modified Files

1. **Game.ContentBuilder/ViewModels/HybridArrayEditorViewModel.cs**
   - Changed `Patterns` from `ObservableCollection<string>` to `ObservableCollection<PatternComponent>`
   - Added `_itemsData` and `_componentsData` fields to store raw JSON
   - Updated `LoadData()` to generate examples when loading patterns
   - Updated `AddPattern()` to generate example for new patterns
   - Updated `Save()` to extract only pattern strings (not examples)

2. **Game.ContentBuilder/Views/HybridArrayEditorView.xaml**
   - Updated Patterns tab ListBox to display both pattern and example
   - Pattern shown in bold
   - Example shown below in smaller, italic, gray text
   - Color-coded warnings for invalid patterns

## How It Works

### Pattern Parsing

```csharp
"material + base" → ["material", "base"]
```

### Token Resolution

For each token, the generator searches:
1. **Exact match** in components (e.g., `components.material`)
2. **Common prefixes** (e.g., `components.prefixes_material`)
3. **Common suffixes** (e.g., `components.material_types`)
4. **Fuzzy match** (contains token name)
5. **Nested objects** (e.g., `components.weapon_types.swords`)
6. **Items array** (for tokens like "base", "item", "piece")
7. **Placeholder** if not found (`[token]`)

### Example Generation

For `weapons/names.json` with pattern `"material + base"`:
```
Pattern: material + base
Example: Iron Longsword
```

Breakdown:
- `material` → Found in `components.prefixes_material` → First value: "Iron"
- `base` → Token suggests base item → Found in `items` array → First value: "Longsword"
- Result: "Iron Longsword"

## UI Display

### Pattern List Item
```
┌─────────────────────────────────────┬────┐
│ material + base                     │ 🗑️  │
│ Iron Longsword                      │    │
└─────────────────────────────────────┴────┘
```

- **Top line:** Pattern (bold)
- **Bottom line:** Auto-generated example (italic, gray)
- **Right:** Delete button

### Special Cases

- **(no data available)** - Shown in warning color when components/items are empty
- **(invalid pattern)** - Shown in error color when pattern can't be parsed
- **[token]** - Placeholder when specific token can't be resolved

## Benefits

1. **Visual Feedback:** Users immediately see what each pattern produces
2. **Data Validation:** Helps identify missing components or incorrect pattern syntax
3. **Documentation:** Examples serve as built-in documentation
4. **Quality Assurance:** Easy to spot when patterns don't match available data

## Testing

To test, open ContentBuilder and navigate to any HybridArray file:
- Items → Weapons → Names
- Items → Armor → Names  
- NPCs → Names → First Names
- General → Adjectives

Check the **Patterns** tab to see auto-generated examples!

## Technical Notes

- Examples are generated on-the-fly when loading/adding patterns
- Examples are **not** saved to JSON files (only pattern strings are saved)
- Generator is smart enough to handle various naming conventions
- Falls back gracefully when data is missing

## Example Output

### Items/Weapons/Names.json
```
Pattern: material + base
Example: Iron Longsword

Pattern: quality + material + base
Example: Fine Iron Longsword

Pattern: descriptive + base
Example: Flaming Longsword
```

### NPCs/Names/First_Names.json
```
Pattern: prefix + suffix
Example: Al Aldric

Pattern: nature-inspired
Example: Willow
```

### General/Verbs.json
```
Pattern: verb
Example: attacks

Pattern: adverb + verb
Example: quickly attacks
```
