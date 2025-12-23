# Phase 3: Pattern Validation & Live Examples - Testing Checklist

**Implementation Date:** December 17, 2025  
**Status:** ✅ Code Complete - Testing In Progress

## Overview

Phase 3 adds real-time pattern validation and live example preview to the ContentBuilder's HybridArrayEditor. This testing checklist verifies all features work correctly across different file types.

---

## ✅ Completed Implementation

### Core Features
- [x] PatternValidator service with 3-level validation (Valid/Warning/Error)
- [x] Real-time validation as user types patterns
- [x] Visual validation indicators (icons, colors, tooltips)
- [x] Live example preview (5 diverse examples)
- [x] Refresh button for regenerating examples
- [x] Support for weight-based component structure `{value, rarityWeight}`
- [x] Conditional UI based on file type (HasItemsArray flag)
- [x] Items tab auto-hides for pattern_generation files
- [x] Info panel explaining base token resolution
- [x] Enhanced file type detection using metadata.type

### Files Created
- [x] `Game.ContentBuilder/Services/PatternValidator.cs` (145 lines)
- [x] `Game.ContentBuilder/Converters/GreaterThanZeroConverter.cs` (23 lines)

### Files Modified
- [x] `Game.ContentBuilder/Models/PatternComponent.cs` (validation properties)
- [x] `Game.ContentBuilder/ViewModels/HybridArrayEditorViewModel.cs` (validation logic, examples)
- [x] `Game.ContentBuilder/Views/HybridArrayEditorView.xaml` (validation UI, examples panel)
- [x] `Game.ContentBuilder/Services/PatternExampleGenerator.cs` (random sampling)
- [x] `Game.ContentBuilder/Services/FileTreeService.cs` (metadata-based detection)
- [x] `Game.ContentBuilder/App.xaml` (converter registration)

---

## 🧪 Testing Checklist

### 1. Pattern Generation Files (names.json)

**File to Test:** `items/weapons/names.json` (or any pattern_generation type)

#### UI Layout
- [ ] Items tab is hidden (only Components and Patterns tabs visible)
- [ ] Info panel displays in right sidebar with blue background
- [ ] Info text: "This file generates names using patterns. The 'base' token resolves from the corresponding types.json file."

#### Component Display
- [ ] Components show weight format: "Iron (weight: 10)"
- [ ] Legacy string components still display correctly
- [ ] Component count displays correctly in header

#### Pattern Validation - Valid Patterns
**Test Pattern:** `material + base`
- [ ] Green checkmark icon appears next to input field
- [ ] Tooltip shows: "Valid pattern"
- [ ] No error messages displayed
- [ ] Pattern can be added to list

**Test Pattern:** `prefix + base + suffix`
- [ ] Green checkmark validation
- [ ] Multi-token pattern works correctly

#### Pattern Validation - Invalid Patterns
**Test Pattern:** `invalid_key + base`
- [ ] Red X icon appears
- [ ] Tooltip shows: "Invalid component key: invalid_key"
- [ ] Error message displayed
- [ ] Cannot add pattern (or shows warning)

**Test Pattern:** `material +`
- [ ] Orange warning icon appears
- [ ] Tooltip shows warning about incomplete pattern
- [ ] Warning message displayed

**Test Pattern:** `+ base`
- [ ] Orange warning icon
- [ ] Leading operator warning

#### Live Examples
**Action:** Type valid pattern `material + base`
- [ ] Live examples panel appears below pattern input
- [ ] Exactly 5 examples displayed
- [ ] Examples use actual component values from file
- [ ] Examples show variety (not all the same)
- [ ] "Example Results (5)" header displays count correctly

**Action:** Click refresh button (🔄)
- [ ] New set of 5 examples generated
- [ ] Examples are different from previous set
- [ ] No errors or delays

**Action:** Type invalid pattern
- [ ] Examples panel disappears (count = 0)
- [ ] No examples shown for invalid patterns

#### Pattern List
**Existing Patterns:**
- [ ] Each pattern shows validation icon
- [ ] Valid patterns: green checkmark
- [ ] Invalid patterns: red X with tooltip
- [ ] Clicking pattern shows tooltip with validation message

### 2. Item Catalog Files (types.json)

**File to Test:** `items/weapons/types.json` (or any item_catalog type)

#### UI Layout
- [ ] All 3 tabs visible: Components, Patterns, **Items**
- [ ] Items tab shows nested category structure
- [ ] Info panel does NOT display (HasItemsArray = true)

#### Items Display
- [ ] Nested categories load correctly (weapon_types, etc.)
- [ ] Items within categories display
- [ ] Can add/edit/delete items
- [ ] Items save correctly

#### Pattern Validation
**Test Pattern:** `material + base`
- [ ] Validation works same as pattern_generation files
- [ ] Can reference 'base' token from types within same file

### 3. Prefix/Suffix Modifier Files

**Files to Test:** `items/weapons/prefixes.json`, `items/weapons/suffixes.json`

#### UI Layout
- [ ] All 3 tabs visible (has items array)
- [ ] Items tab shows flat item list
- [ ] No info panel (modifier files have items)

#### Items Display
- [ ] Items show with stat bonuses
- [ ] rarityWeight displays correctly
- [ ] Can add/edit/delete prefix/suffix items

#### Component Display
- [ ] Components display correctly
- [ ] Weight-based components: "Mighty (weight: 5)"

### 4. Edge Cases & Error Handling

#### Empty Files
**Create New File:**
- [ ] New pattern_generation file hides Items tab
- [ ] New item_catalog file shows Items tab
- [ ] No crashes with empty component arrays

#### Missing Metadata
**Test File Without metadata.type:**
- [ ] Falls back to heuristic detection
- [ ] Still functions (Items tab shows by default)
- [ ] No crashes or errors

#### Large Component Arrays
**File with 100+ components:**
- [ ] UI remains responsive
- [ ] Live examples generate quickly (< 1 second)
- [ ] Scrolling works smoothly
- [ ] Weight-based display doesn't lag

#### Special Characters in Patterns
**Test Pattern:** `material + "base with spaces"`
- [ ] Pattern handles quoted strings
- [ ] Validation accounts for quotes

**Test Pattern:** `material+base` (no spaces)
- [ ] Validation works without spaces
- [ ] Examples generate correctly

#### Save Operations
**Pattern Generation File:**
- [ ] Save does NOT write empty items array
- [ ] Components and patterns save correctly
- [ ] File structure matches JSON standards

**Item Catalog File:**
- [ ] Items array saves correctly
- [ ] Nested structure preserved

### 5. Performance Testing

#### Response Time
- [ ] Pattern validation completes < 100ms
- [ ] Live examples generate < 500ms
- [ ] UI updates feel instant (no lag)

#### Memory Usage
- [ ] Open 5+ files simultaneously
- [ ] No memory leaks
- [ ] Examples generate without accumulating memory

#### Refresh Button Spam Test
- [ ] Click refresh 20 times rapidly
- [ ] No crashes or hangs
- [ ] Examples update correctly each time

### 6. Integration Testing

#### File Switching
- [ ] Switch from names.json to types.json
- [ ] Items tab appears/disappears correctly
- [ ] No lingering state from previous file

#### Simultaneous Editing
- [ ] Edit pattern in one file
- [ ] Switch to another file
- [ ] Return to first file
- [ ] Pattern validation state preserved

#### Cross-File References
**Pattern in names.json:** `material + base`
**Base Token Source:** types.json
- [ ] Validation checks that 'base' exists in types
- [ ] Examples pull actual item names from types.json
- [ ] Changing types.json updates examples (after refresh)

---

## 🐛 Known Issues / Limitations

### Current Limitations
- Live examples always show 5 results (not configurable)
- Examples use simple random selection (not weighted by rarityWeight yet)
- Validation doesn't check for circular pattern references
- Base token resolution requires types.json to be in same directory

### Future Enhancements (Phase 4+)
- Weight-based example generation (respect rarityWeight)
- Configurable example count (slider: 1-20)
- Pattern dependency graph visualization
- Cross-file validation (verify types.json exists)
- Batch pattern testing (test all patterns at once)
- Export examples to clipboard

---

## ✅ Sign-Off

**Tester:** _________________  
**Date:** _________________  
**Build Version:** Debug net9.0-windows  

**Test Results:**
- [ ] All critical tests passed
- [ ] All edge cases handled
- [ ] Performance acceptable
- [ ] Ready for Phase 4

**Notes:**
```
(Add testing observations here)
```

---

## 📋 Quick Reference

### Validation Levels
| Level | Icon | Color | Meaning |
|-------|------|-------|---------|
| Valid | ✓ (CheckCircle) | Green | Pattern syntax correct, all tokens exist |
| Warning | ⚠ (AlertCircle) | Orange | Potential issue but not critical |
| Error | ✗ (CloseCircle) | Red | Invalid syntax or missing component keys |

### File Types & Expected Behavior
| metadata.type | HasItemsArray | Items Tab | Info Panel |
|---------------|---------------|-----------|------------|
| pattern_generation | false | Hidden | Visible |
| item_catalog | true | Visible | Hidden |
| prefix_modifier | true | Visible | Hidden |
| suffix_modifier | true | Visible | Hidden |
| component_library | false | Hidden | Hidden |

### Test Data Locations
- Pattern Generation: `Game.Shared/Data/items/weapons/names.json`
- Item Catalog: `Game.Shared/Data/items/weapons/types.json`
- Prefixes: `Game.Shared/Data/items/weapons/prefixes.json`
- Suffixes: `Game.Shared/Data/items/weapons/suffixes.json`
