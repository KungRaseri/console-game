# .cbconfig.json Standard

**Version:** 1.0  
**Date:** December 27, 2025  
**Purpose:** ContentBuilder UI configuration for folder display and navigation

---

## Overview

The `.cbconfig.json` file configures how a folder appears in the ContentBuilder WPF application. It controls:
- **Folder Icon** - MaterialDesign icon for the folder
- **Display Name** - Human-readable name (overrides folder name)
- **Description** - Tooltip/summary text
- **File Icons** - Individual icons for specific JSON files
- **Sort Order** - Position in tree view
- **File Listing** - Which files to show prominently

---

## File Location

**Placement:** Root of each data category folder

```
Game.Data/Data/Json/
├── items/
│   ├── weapons/
│   │   └── .cbconfig.json     ← Configures weapons folder
│   ├── armor/
│   │   └── .cbconfig.json     ← Configures armor folder
├── enemies/
│   ├── trolls/
│   │   └── .cbconfig.json     ← Configures trolls folder
```

---

## Standard Structure

### Minimal Configuration

```json
{
  "icon": "Sword",
  "sortOrder": 1
}
```

### Full Configuration

```json
{
  "icon": "Sword",
  "displayName": "Weapons",
  "description": "Weapon definitions with names.json (pattern generation) and catalog.json (base weapons)",
  "sortOrder": 1,
  "defaultFileIcon": "FileDocument",
  "showFileCount": true,
  "dataFiles": [
    "catalog.json",
    "names.json"
  ],
  "childDirectories": [
    "materials",
    "enchantments"
  ],
  "fileIcons": {
    "names": "FormatListBulleted",
    "catalog": "ShapeOutline",
    "traits": "EmoticonHappy"
  }
}
```

---

## Field Reference

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `icon` | string | ✅ Yes | MaterialDesign icon name for folder |
| `displayName` | string | ❌ Optional | Override folder name in UI |
| `description` | string | ❌ Optional | Tooltip text shown on hover |
| `sortOrder` | number | ✅ Yes | Position in tree (1 = top) |
| `defaultFileIcon` | string | ❌ Optional | Default icon for unspecified files |
| `showFileCount` | boolean | ❌ Optional | Display file count badge |
| `dataFiles` | array | ❌ Optional | Explicit list of files to show |
| `childDirectories` | array | ❌ Optional | Subfolders to highlight |
| `fileIcons` | object | ❌ Optional | Icon mapping for specific files |

---

## Field Details

### `icon` (Required)

**Purpose:** Visual identifier for the folder in tree view

**Format:** MaterialDesign icon name (without "mdi-" prefix)

**Common Icons:**
```json
{
  "icon": "Sword",           // Weapons
  "icon": "Shield",          // Armor
  "icon": "Flask",           // Consumables
  "icon": "Skull",           // Enemies

  "icon": "Sparkles",        // Abilities/Magic
  "icon": "Cube"            // Materials
}
```

**Icon Resources:**
- Browse: https://materialdesignicons.com/
- Search for thematic matches
- Use PascalCase naming (not kebab-case)

---

### `displayName` (Optional)

**Purpose:** Override folder name with user-friendly text

**Example:**
```json
{
  "displayName": "Weapons",    // Folder: "weapons"

  "displayName": "Abilities"  // Folder: "active"
}
```

**When to Use:**
- Technical names need clarification
- Capitalization/formatting improvements

---

### `description` (Optional)

**Purpose:** Brief explanation shown in tooltips

**Example:**
```json
{
  "description": "Weapon definitions with names.json (v4.0 pattern generation) and catalog.json (base weapons)"
}
```

**Best Practices:**
- Keep under 120 characters
- Mention key files (names.json, catalog.json)
- Note version numbers if relevant
- Describe the folder's purpose

---

### `sortOrder` (Required)

**Purpose:** Control folder position in tree view

**Format:** Integer (lower = higher in list)

**Standard Ordering:**
```json
1-10   : Core game data (items, enemies)
11-20  : Enemy subtypes (trolls, goblins, etc.)
21-30  : Abilities and magic
31-40  : Configuration and utilities
51+    : Utilities and configuration
```

**Example:**
```json
{ "sortOrder": 1 }   // items/weapons (top)
{ "sortOrder": 2 }   // items/armor
{ "sortOrder": 11 }  // enemies/trolls
```

---

### `fileIcons` (Optional)

**Purpose:** Assign specific icons to individual files

**Format:** Object mapping filename → icon name

**Standard Mappings:**
```json
{
  "fileIcons": {
    "names": "FormatListBulleted",      // Pattern generation files
    "catalog": "ShapeOutline",          // Item catalog files
    "traits": "EmoticonHappy",          // Trait/personality files
    "abilities": "FlashOutline",        // Ability files
    "dialogue": "MessageText"           // Dialogue files
  }
}
```

**Common File Icons:**
- `names.json` → `FormatListBulleted`
- `catalog.json` → `ShapeOutline` or `ViewList`
- `traits.json` → `EmoticonHappy`
- `abilities*.json` → `FlashOutline`
- `dialogue*.json` → `MessageText`

---

### `defaultFileIcon` (Optional)

**Purpose:** Icon for files not in `fileIcons` mapping

**Example:**
```json
{
  "defaultFileIcon": "FileDocument"
}
```

**Common Defaults:**
- `FileDocument` - Generic file icon
- `FileOutline` - Outlined file icon
- `CodeJson` - JSON-specific icon

---

### `showFileCount` (Optional)

**Purpose:** Display badge with number of files in folder

**Example:**
```json
{
  "showFileCount": true    // Shows "weapons (3)" in tree
}
```

**When to Use:**
- Folders with many files
- To show completion status
- For data-heavy categories

---

### `dataFiles` (Optional)

**Purpose:** Explicitly list which files to show/prioritize

**Example:**
```json
{
  "dataFiles": [
    "catalog.json",
    "traits.json",
    "names.json"
  ]
}
```

**Use Cases:**
- Control file display order
- Hide auxiliary files
- Feature important files first

---

### `childDirectories` (Optional)

**Purpose:** Highlight specific subfolders

**Example:**
```json
{
  "childDirectories": [
    "dialogue",
    "occupations"
  ]
}
```

**Use Cases:**
- Navigate to important subfolders
- Quick access to nested data
- Documentation/navigation hints

---

## Examples by Category

### Items (Weapons, Armor, Consumables)

```json
{
  "icon": "Sword",
  "displayName": "Weapons",
  "description": "Weapon definitions with names.json (v4.0 pattern generation) and catalog.json (base weapons)",
  "sortOrder": 1,
  "defaultFileIcon": "FileDocument",
  "showFileCount": true,
  "fileIcons": {
    "names": "FormatListBulleted",
    "catalog": "ShapeOutline"
  }
}
```

### Enemies (Trolls, Goblins, etc.)

```json
{
  "icon": "Hammer",
  "sortOrder": 11,
  "fileIcons": {
    "names": "FormatListBulleted",
    "abilities": "FlashOutline",
    "catalog": "ShapeOutline"
  }
}
```

### Minimal (Enemy Subfolder)

```json
{
  "icon": "SkullOutline",
  "sortOrder": 15,
  "fileIcons": {
    "names": "FormatListBulleted",
    "catalog": "ViewList"
  }
}
```

---

## Best Practices

### ✅ DO:
1. **Always include `icon` and `sortOrder`** - Required for display
2. **Use semantic icons** - Match icon to content (Sword for weapons)
3. **Add descriptions** for complex folders - Help users understand purpose
4. **Map standard files** - names → FormatListBulleted, catalog → ShapeOutline
5. **Use consistent sort ranges** - Items 1-10, Enemies 11-20, etc.
6. **Test icon names** - Verify MaterialDesign icon exists

### ❌ DON'T:
1. **Don't use `mdi-` prefix** - Icon names are PascalCase without prefix
2. **Don't skip sortOrder** - Results in unpredictable ordering
3. **Don't duplicate sort values** - Each folder should have unique sortOrder
4. **Don't overuse showFileCount** - Only for multi-file folders
5. **Don't list all files** in dataFiles - Let ContentBuilder discover them

---

## Validation Checklist

Before committing `.cbconfig.json` changes:

- [ ] `icon` field present and valid MaterialDesign icon
- [ ] `sortOrder` field present and unique within parent
- [ ] Icon names use PascalCase (not kebab-case)
- [ ] File icon mappings use base filename without extension
- [ ] Description under 120 characters (if present)
- [ ] Sort order fits within category range
- [ ] Test in ContentBuilder UI

---

## Migration Notes

**From:** No `.cbconfig.json` (default behavior)  
**To:** Explicit configuration

**Steps:**
1. Create `.cbconfig.json` in folder root
2. Add minimum fields (`icon`, `sortOrder`)
3. Test folder appears in ContentBuilder
4. Add optional fields as needed

---

## ContentBuilder Integration

**How ContentBuilder Uses This File:**

1. **Folder Tree Display** - Reads `.cbconfig.json` on folder scan
2. **Icon Rendering** - Loads MaterialDesign icon from `icon` field
3. **Sorting** - Orders folders by `sortOrder` value
4. **File Discovery** - Uses `dataFiles` if present, else scans directory
5. **File Icons** - Maps individual file icons via `fileIcons` object

**Fallback Behavior:**
- Missing `.cbconfig.json` → Default folder icon
- Missing `icon` → Generic folder icon
- Missing `sortOrder` → Alphabetical order
- Missing `fileIcons` → Default file icon for all

---

## Related Standards

- **names.json Standard** - Pattern generation file structure
- **catalog.json Standard** - Item/enemy catalog file structure
- **JSON Structure Standards** - General JSON metadata requirements

---

## Change Log

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-27 | Initial standard documentation |
