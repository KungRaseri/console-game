# JSON Structure Standards

**Date:** December 17, 2025  
**Status:** ENFORCED across all item JSON files  
**Purpose:** Consistent metadata and documentation structure

---

## Standard Structure

### All JSON Files MUST Have:

```json
{
  "metadata": {
    "description": "Brief description of the file's purpose",
    "version": "X.Y",
    "last_updated": "YYYY-MM-DD",
    "type": "file_type (e.g., pattern_generation, item_catalog, component_catalog)",
    // ... other metadata fields ...
    "usage": "How to use this file at runtime (optional but recommended)",
    "notes": ["Array of", "architectural notes", "about the system"] // Optional
  },
  // ... rest of the file structure ...
}
```

### Metadata Field Guidelines

| Field | Type | Required | Purpose |
|-------|------|----------|---------|
| `description` | string | ✅ Yes | Brief summary of file purpose |
| `version` | string | ✅ Yes | Semantic version (e.g., "4.0", "3.0") |
| `last_updated` | string | ✅ Yes | ISO date format (YYYY-MM-DD) |
| `type` | string | ✅ Yes | File category (see types below) |
| `usage` | string/object | ⭐ Recommended | Runtime usage instructions |
| `notes` | array | ❌ Optional | Architectural/system notes |
| Custom fields | varies | ❌ Optional | File-specific metadata |

---

## File Types

### Pattern Generation Files (`names.json`)

**Type:** `"pattern_generation"`

**Required Metadata:**
- `component_keys`: Array of component group names
- `pattern_tokens`: Array of tokens used in patterns
- `total_patterns`: Number of patterns defined
- `rarity_system`: Rarity calculation method (e.g., "weight-based")
- `supports_traits`: Boolean (v4.0+ only)

**Structure:**
```json
{
  "metadata": { /* metadata here */ },
  "components": {
    "component_group_name": [
      {
        "value": "Component Name",
        "rarityWeight": 10,
        "traits": { /* v4.0+ only */ }
      }
    ]
  },
  "patterns": [
    {
      "pattern": "token1 + token2",
      "weight": 50,
      "example": "Example Output"
    }
  ]
}
```

**Examples:**
- `items/weapons/names.json`
- `items/armor/names.json`
- `items/enchantments/names.json`
- `items/consumables/names.json`

---

### Item Catalog Files (`types.json`)

**Type:** `"item_catalog"`

**Required Metadata:**
- `total_*_types`: Count of category types
- `total_*`: Count of total items (if applicable)

**Structure:**
```json
{
  "metadata": { /* metadata here */ },
  "category_types": {
    "subcategory_name": {
      "traits": { /* shared traits */ },
      "items": [
        {
          "name": "Item Name",
          "stat1": value,
          "rarityWeight": 10
        }
      ]
    }
  }
}
```

**Examples:**
- `items/weapons/types.json`
- `items/armor/types.json`
- `items/consumables/types.json`

---

### Component Catalog Files

**Type:** `"component_catalog"` or `"material_catalog"`

**Structure:**
```json
{
  "metadata": { /* metadata here */ },
  "components": {
    "group_name": [
      { "value": "Name", "rarityWeight": 10 }
    ]
  },
  "patterns": [ /* optional */ ]
}
```

**Examples:**
- `items/materials/names.json`
- `items/materials/types.json`

---

## Consistency Rules

### ✅ DO:
1. **Place ALL documentation in `metadata`** (no root-level `notes` section)
2. **Use `metadata.usage`** for runtime instructions
3. **Use `metadata.notes`** (array) for architectural notes
4. **Keep metadata at the TOP** of the file
5. **Use consistent date format** (YYYY-MM-DD)
6. **Update `last_updated`** when modifying files
7. **Include examples** in `metadata.usage` or `metadata.examples`

### ❌ DON'T:
1. **Don't use root-level `notes` section** (deprecated)
2. **Don't scatter metadata** across the file
3. **Don't use inconsistent date formats**
4. **Don't omit version numbers**
5. **Don't duplicate documentation** in multiple places

---

## Migration from Old Format

### Before (Inconsistent):
```json
{
  "data": { /* content */ },
  "metadata": { /* some metadata */ },
  "notes": {
    "usage": "How to use this"
  }
}
```

### After (Consistent):
```json
{
  "metadata": {
    "description": "...",
    "version": "4.0",
    "last_updated": "2025-12-17",
    "type": "pattern_generation",
    "usage": "How to use this",
    "notes": ["Architectural notes here"]
  },
  "data": { /* content */ }
}
```

---

## v4.0 Trait System

Files with `"supports_traits": true` include trait definitions:

```json
{
  "value": "Component Name",
  "rarityWeight": 10,
  "traits": {
    "traitName": { "value": 123, "type": "number" },
    "anotherTrait": { "value": "fire", "type": "string" },
    "booleanTrait": { "value": true, "type": "boolean" }
  }
}
```

**Trait Merging Rules:**
- **Numbers**: Take highest value
- **Strings**: Take last defined value
- **Booleans**: Use OR logic (any true = true)

---

## Validation Checklist

Before committing JSON changes:

- [ ] Metadata at top of file
- [ ] All required fields present (description, version, last_updated, type)
- [ ] No root-level `notes` section
- [ ] Usage instructions in `metadata.usage`
- [ ] Architectural notes in `metadata.notes` array (if needed)
- [ ] Date in YYYY-MM-DD format
- [ ] Version number updated if structure changed
- [ ] Build successful (`dotnet build`)

---

## Enforcement

**Date Applied:** December 17, 2025  
**Files Updated:** 8 item JSON files

| File | Old Structure | New Structure | Status |
|------|---------------|---------------|--------|
| `items/weapons/names.json` | Metadata only ✅ | No change needed | ✅ |
| `items/weapons/types.json` | Root notes ❌ | Moved to metadata | ✅ |
| `items/armor/names.json` | Metadata only ✅ | No change needed | ✅ |
| `items/armor/types.json` | Root notes ❌ | Moved to metadata | ✅ |
| `items/enchantments/names.json` | Metadata only ✅ | No change needed | ✅ |
| `items/consumables/names.json` | Root notes ❌ | Moved to metadata | ✅ |
| `items/consumables/types.json` | Root notes ❌ | Moved to metadata | ✅ |
| `items/materials/names.json` | Metadata at end ❌ | Moved to top | ✅ |
| `items/materials/types.json` | Root notes ❌ | Moved to metadata | ✅ |

**Build Status:** ✅ All projects compile successfully

---

## Future Additions

When creating NEW JSON files:
1. Start with metadata section
2. Follow structure for file type
3. Include usage instructions
4. Add examples if complex
5. Use semantic versioning (start at "1.0")

---

## Contact

For questions about JSON structure standards, see:
- `docs/standards/JSON_STANDARDS_COMPLETION.md`
- `docs/planning/NAMING_SYSTEM_V4_MIGRATION_COMPLETE.md`
- `docs/planning/CONSUMABLES_V4_MIGRATION_COMPLETE.md`
