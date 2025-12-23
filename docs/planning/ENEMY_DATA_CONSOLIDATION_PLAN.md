# Enemy Data Consolidation Plan

**Date**: December 17, 2025  
**Objective**: Consolidate prefixes.json and suffixes.json into names.json for all enemy categories

---

## Current Structure Analysis

### Enemy Categories with Prefix/Suffix Files
All enemy subdirectories have the same structure:
- `names.json` - Base name components (size, descriptive, origin, title)
- `prefixes.json` - Stat modifiers (Wild, Feral, Enraged, etc.)
- `suffixes.json` - Title modifiers (the Fierce, the Ancient, etc.)
- `types.json` - Enemy type catalog with stats
- `traits.json` - Trait definitions

**Directories to consolidate**:
1. enemies/beasts
2. enemies/demons
3. enemies/dragons
4. enemies/elementals
5. enemies/goblinoids
6. enemies/humanoids
7. enemies/insects
8. enemies/orcs
9. enemies/plants
10. enemies/reptilians
11. enemies/trolls
12. enemies/undead
13. enemies/vampires

---

## Consolidation Strategy (Following Items Pattern)

### 1. Update names.json Metadata
- Change version to `4.0`
- Add `"supportsTraits": true`
- Add `"prefix"` and `"suffix"` to `componentKeys` and `patternTokens`
- Update notes to indicate consolidation

### 2. Add Prefix Components
Extract from `prefixes.json`:
- `items` array → `components.prefix` array
- Convert structure:
  ```json
  // FROM (prefixes.json)
  {
    "name": "Wild",
    "rarityWeight": 50,
    "traits": { "attackSpeed": { "value": 5, "type": "number" } }
  }
  
  // TO (names.json components.prefix)
  {
    "value": "Wild",
    "rarityWeight": 50,
    "traits": { "attackSpeed": { "value": 5, "type": "number" } }
  }
  ```

### 3. Add Suffix Components
Extract from `suffixes.json`:
- `items` array → `components.suffix` array
- Same transformation: `name` → `value`
- Preserve all traits and rarityWeight

### 4. Update Patterns
Add patterns that use prefix/suffix:
```json
{
  "pattern": "{prefix} {base}",
  "weight": 15,
  "example": "Wild Wolf"
},
{
  "pattern": "{base} {suffix}",
  "weight": 15,
  "example": "Wolf the Fierce"
},
{
  "pattern": "{prefix} {base} {suffix}",
  "weight": 5,
  "example": "Wild Wolf the Fierce"
}
```

### 5. Update types.json Metadata
Change `type` from `"enemy_catalog"` to `"item_catalog"` for consistency

### 6. Delete Deprecated Files
After consolidation:
- Delete `prefixes.json`
- Delete `suffixes.json`

---

## NPCs Assessment

**Directory**: `npcs/names/`
- `first_names.json` - First names
- `last_names.json` - Last names

**Recommendation**: Keep separate
- Different purpose than modifiers
- No stat bonuses/traits
- Pairing logic is different (first + last)
- No consolidation needed

---

## Quests Assessment

**Directory**: `quests/templates/`
- Multiple template files (delivery.json, escort.json, etc.)

**Recommendation**: Keep separate
- Each file is a different quest type
- Not modifiers but complete templates
- No consolidation needed

---

## General Assessment

**Directory**: `general/`
- Various config files (adjectives.json, colors.json, etc.)

**Recommendation**: Keep separate
- Reference data, not generation patterns
- No prefix/suffix structure
- No consolidation needed

---

## Implementation Checklist

### Per Enemy Category (13 total):
- [ ] Read current names.json
- [ ] Read prefixes.json
- [ ] Read suffixes.json
- [ ] Transform prefix items: name → value
- [ ] Transform suffix items: name → value
- [ ] Add prefix/suffix to metadata componentKeys
- [ ] Add prefix/suffix to metadata patternTokens
- [ ] Add patterns using prefix/suffix
- [ ] Update version to 4.0
- [ ] Add supportsTraits: true
- [ ] Update notes array
- [ ] Write updated names.json
- [ ] Update types.json metadata (enemy_catalog → item_catalog)
- [ ] Delete prefixes.json
- [ ] Delete suffixes.json
- [ ] Update .cbconfig.json (remove prefix/suffix icons)

---

## Expected Outcomes

### Before (3 files per enemy category):
```
enemies/beasts/
├── names.json (base components only)
├── prefixes.json (9 prefix modifiers)
├── suffixes.json (16 suffix modifiers)
└── types.json (4 beast types with 15 items)
```

### After (1 file per enemy category):
```
enemies/beasts/
├── names.json (base + prefix + suffix components, v4.0)
└── types.json (updated metadata)
```

### Files Removed: 26 total
- 13 prefixes.json files
- 13 suffixes.json files

### Benefits:
1. **Consistency**: All categories use same v4.0 pattern
2. **Simpler**: One naming file instead of three
3. **Trait Support**: Built-in trait merging
4. **Editor Support**: Works with NamesEditor
5. **Maintainability**: Easier to manage one file

---

## Validation

After consolidation, verify:
1. All prefix modifiers preserved
2. All suffix modifiers preserved
3. All traits correctly formatted
4. Patterns include prefix/suffix combinations
5. Metadata accurately reflects changes
6. FileTypeDetector correctly identifies files
7. NamesEditor can load and edit files

---

## Risk Mitigation

1. **Backup**: Files already in `backups/` directory
2. **Version Control**: Git tracks all changes
3. **Validation**: Can compare old vs new structure
4. **Rollback**: Easy to restore from git if needed

---

## Next Steps

1. Execute consolidation for all 13 enemy categories
2. Update .cbconfig.json files to remove prefix/suffix icons
3. Test with ContentBuilder NamesEditor
4. Test enemy generation in game
5. Update documentation

**Status**: Ready to Execute ✅
