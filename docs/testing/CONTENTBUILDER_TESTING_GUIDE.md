# ContentBuilder Testing Guide

## Overview

This guide provides comprehensive testing options for the Game ContentBuilder WPF application, covering manual testing, automated testing, and integration testing approaches.

---

## 1. 🚀 Manual Testing

### Quick Start Testing

**Time Required**: 10-15 minutes  
**Best For**: Initial validation, UX feedback

#### Step 1: Launch the Application
```powershell
# From the solution root
dotnet run --project RealmForge
```

**Expected Result**: Window opens with tree view on left, empty editor area on right, status bar at bottom showing "Ready"

#### Step 2: Test Navigation
- [ ] Expand "General" category → 9 files should appear
- [ ] Expand "Items" → 4 subcategories (Weapons, Armor, Consumables, Enchantments, Materials)
- [ ] Expand "Enemies" → 13 creature types
- [ ] Expand "NPCs" → 4 subcategories (Names, Personalities, Dialogue, Occupations)
- [ ] Expand "Quests" → 4 subcategories (Objectives, Rewards, Locations, Templates)

**Expected Result**: All 93 files visible in tree view with appropriate icons

#### Step 3: Test HybridArray Editor
Select `general/colors.json` (should use HybridArray editor)

**Verify UI Elements**:
- [ ] Header shows "colors" as title
- [ ] Three statistics chips display counts (Items, Components, Patterns)
- [ ] Three tabs visible: Items, Components, Patterns
- [ ] Save button in footer
- [ ] Status message shows "Loaded colors: X items, Y components, Z patterns"

**Test Items Tab**:
1. Switch to Items tab
2. Type "test-color" in input box
3. Click "Add" button
4. [ ] Item appears in list
5. [ ] Count updates in header chip
6. [ ] Click delete button on item → item removed
7. [ ] Count decreases

**Test Components Tab**:
1. Switch to Components tab
2. Left panel: Type "test_group" in input
3. Click "Add" button
4. [ ] New group appears in left list
5. Select the new group
6. Right panel: Type "test component" in input
7. Click "Add" button
8. [ ] Component appears in right list
9. [ ] Components count updates

**Test Patterns Tab**:
1. Switch to Patterns tab
2. Type "test + pattern" in input
3. Click "Add" button
4. [ ] Pattern appears in list
5. [ ] Patterns count updates

**Test Save Functionality**:
1. Make some edits across tabs
2. [ ] Save button is enabled
3. Click "Save Changes"
4. [ ] Status message shows "Saved colors successfully"
5. Close and reopen the file
6. [ ] Edits persist

#### Step 4: Test FlatItem Editor
Select `items/materials/metals.json` (should use FlatItem editor)

**Verify**:
- [ ] Different editor loads (not tabbed interface)
- [ ] Shows stat block structure
- [ ] Can view existing metals

#### Step 5: Test Error Handling
- [ ] Try saving with no changes → button disabled
- [ ] Try adding empty item → button disabled
- [ ] Select different files rapidly → no crashes
- [ ] Resize window → UI adapts properly

---

## 2. 🧪 Automated Unit Testing

### Test Project Setup

I've created a test project structure for you:
```
RealmForge.Tests/
├── RealmForge.Tests.csproj
└── ViewModels/
    └── HybridArrayEditorViewModelTests.cs (9 tests)
```

### Running the Tests

```powershell
# Run all tests
dotnet test RealmForge.Tests

# Run with verbose output
dotnet test RealmForge.Tests --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~Should_Load_HybridArray_Structure_Correctly"

# Run with coverage
dotnet test RealmForge.Tests --collect:"XPlat Code Coverage"
```

### Current Test Coverage

#### HybridArrayEditorViewModelTests (9 tests)
1. ✅ `Should_Load_HybridArray_Structure_Correctly` - Validates JSON parsing
2. ✅ `AddItem_Should_Add_Item_And_Update_Count` - Tests item addition
3. ✅ `DeleteItem_Should_Remove_Item_And_Update_Count` - Tests item deletion
4. ✅ `AddComponentGroup_Should_Create_New_Group` - Tests group creation
5. ✅ `AddComponent_Should_Add_To_Selected_Group` - Tests component addition
6. ✅ `Save_Should_Preserve_Metadata` - Tests metadata preservation
7. ✅ `CanSave_Should_Return_True_When_Content_Exists` - Tests save validation
8. ✅ `Should_Handle_Empty_File_Gracefully` - Tests empty file handling

### Expanding Test Coverage

**Recommended Additional Tests**:

```csharp
// Add to HybridArrayEditorViewModelTests.cs

[Fact]
public void Should_Handle_Complex_Object_Items()
{
    // Test items that are JSON objects, not just strings
}

[Fact]
public void Should_Update_Counts_When_Deleting_Component_Group()
{
    // Test total count recalculation
}

[Theory]
[InlineData("")]
[InlineData("   ")]
[InlineData(null)]
public void AddItem_Should_Not_Add_Empty_Items(string input)
{
    // Test input validation
}

[Fact]
public void Should_Reload_File_After_External_Changes()
{
    // Test file watching/refresh
}
```

---

## 3. 🎯 Integration Testing

### Test Real JSON Files

**Option A: Test Against Actual Game Files**

```powershell
# Create a backup first!
cp -Recurse RealmEngine.Shared/Data/Json RealmEngine.Shared/Data/Json.backup

# Run ContentBuilder and test with real files
dotnet run --project RealmForge
```

**Test Scenarios**:
1. Open `general/colors.json` → Edit → Save → Verify structure intact
2. Open `enemies/beasts/traits.json` → Add trait → Save → Check rarity balance
3. Open `quests/objectives/primary.json` → Add objective → Save → Verify metadata
4. Build the Game project → Ensure no JSON errors

```powershell
# Verify game still builds with edited files
dotnet build Game/Game.csproj
```

**Option B: Create Test Fixtures**

Create a separate test data directory:

```powershell
# Create test data
mkdir RealmForge.Tests/TestData
# Copy sample files for testing
cp RealmEngine.Shared/Data/Json/general/colors.json RealmForge.Tests/TestData/
```

---

## 4. 📊 Performance Testing

### Load Testing

**Test Large Files**:
```csharp
[Fact]
public void Should_Handle_Large_Item_Collections()
{
    // Create file with 1000+ items
    // Measure load time (should be < 1 second)
}

[Fact]
public void Should_Handle_Many_Component_Groups()
{
    // Create file with 50+ component groups
    // Verify UI remains responsive
}
```

### Memory Testing

**Monitor Memory Usage**:
```powershell
# Run with diagnostic logging
dotnet run --project RealmForge --configuration Release

# Open Task Manager → Monitor memory usage
# Expected: < 200 MB for typical usage
```

---

## 5. 🔍 End-to-End Testing

### Complete Workflow Test

**Scenario**: Create new enemy trait from scratch

1. Launch ContentBuilder
2. Navigate to `enemies/beasts/traits.json`
3. Add new trait:
   - Item: "Venomous"
   - Add to component group "damage_types"
   - Add pattern: "{damage_type} creature"
4. Save file
5. Close ContentBuilder
6. Rebuild Game project
7. Run game
8. Verify new trait appears in enemy generation

**Expected Result**: Full workflow from editing to game integration works

---

## 6. 🐛 Regression Testing

### After Code Changes

**Test Suite to Run**:
```powershell
# 1. Unit tests
dotnet test RealmForge.Tests

# 2. Build verification
dotnet build RealmForge

# 3. Manual smoke test (3 minutes)
#    - Launch app
#    - Open 3 different file types (HybridArray, FlatItem, NameList)
#    - Make small edit in each
#    - Save all
#    - Verify no errors

# 4. Integration test
dotnet build Game
dotnet test Game.Tests
```

---

## 7. 🎨 UI/UX Testing

### Visual Testing Checklist

**Material Design Compliance**:
- [ ] Color scheme matches Material Design guidelines
- [ ] Icons are clear and appropriate (from MaterialDesignInXamlToolkit)
- [ ] Buttons have hover states
- [ ] Tabs animate smoothly
- [ ] Cards have proper elevation/shadows
- [ ] Text is readable (proper contrast)

**Responsiveness**:
- [ ] Resize window to 600x400 (minimum) → UI still usable
- [ ] Resize window to 1920x1080 → UI scales properly
- [ ] Split panels resize smoothly
- [ ] Lists scroll properly with many items

**Accessibility**:
- [ ] Tab navigation works (Tab key moves through UI)
- [ ] Enter key triggers default actions
- [ ] Escape key cancels operations
- [ ] Status messages are clear and helpful

---

## 8. 🔐 Data Integrity Testing

### Validate JSON Output

**Test JSON Structure Preservation**:

```csharp
[Fact]
public void Save_Should_Maintain_Valid_JSON_Structure()
{
    // Load file, make edits, save
    // Parse saved JSON
    // Verify structure matches expected schema
}

[Fact]
public void Save_Should_Not_Corrupt_Special_Characters()
{
    // Add item with quotes, newlines, unicode
    // Save and reload
    // Verify special characters preserved
}
```

### Test with Real Game Systems

```powershell
# After editing files in ContentBuilder
dotnet run --project Game

# Verify in game:
# - Items generate correctly
# - Enemies use new traits
# - NPCs use new dialogue
# - Quests display properly
```

---

## 9. 📝 Test Documentation

### Test Report Template

```markdown
# ContentBuilder Test Report

**Date**: YYYY-MM-DD
**Tester**: Your Name
**Version**: ContentBuilder vX.X.X

## Test Results

### Unit Tests
- Tests Run: X
- Passed: X
- Failed: X
- Code Coverage: X%

### Manual Tests
- [x] Application Launch
- [x] Navigation
- [x] HybridArray Editor
- [x] FlatItem Editor
- [x] Save/Load
- [x] Error Handling

### Issues Found
1. **Issue Title**: Description
   - Severity: High/Medium/Low
   - Steps to Reproduce:
   - Expected vs Actual:

### Performance
- App Launch Time: X seconds
- File Load Time: X ms
- Save Time: X ms
- Memory Usage: X MB

### Recommendations
- List of improvements or issues to address
```

---

## 10. 🚀 Quick Test Commands

### One-Line Test Commands

```powershell
# Quick smoke test (build + run)
dotnet build RealmForge && dotnet run --project RealmForge

# Full test suite
dotnet test RealmForge.Tests && dotnet build RealmForge

# Test with coverage report
dotnet test RealmForge.Tests --collect:"XPlat Code Coverage" && reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report

# Verify no breaking changes to Game
dotnet test Game.Tests --filter "FullyQualifiedName~Json"
```

---

## 11. 📋 Test Priorities

### Priority 1: Critical (Must Test)
1. ✅ Application launches without errors
2. ✅ Files load correctly (all 93 files)
3. ✅ Save/load cycle preserves data
4. ✅ No data corruption
5. ✅ Game still builds after edits

### Priority 2: Important (Should Test)
1. ✅ All UI elements functional
2. ✅ Commands execute properly
3. ✅ Validation prevents bad input
4. ✅ Error messages are helpful
5. ✅ Performance is acceptable

### Priority 3: Nice to Have (Could Test)
1. ✅ UI looks polished
2. ✅ Animations are smooth
3. ✅ Accessibility features work
4. ✅ Advanced features (search, bulk edit)
5. ✅ Edge cases handled gracefully

---

## 12. 🎯 Recommended Testing Workflow

### First Time Testing
1. **Run unit tests** (5 min)
   ```powershell
   dotnet test RealmForge.Tests
   ```

2. **Launch and explore** (10 min)
   - Open app, navigate tree, test each editor type

3. **Edit real file** (5 min)
   - Make small change to `general/colors.json`
   - Save, verify structure

4. **Integration test** (5 min)
   - Build Game project
   - Run a few game tests

**Total Time**: ~25 minutes for comprehensive first test

### Regular Testing (After Changes)
1. **Unit tests** → 2 minutes
2. **Quick smoke test** → 3 minutes  
3. **Build verification** → 2 minutes

**Total Time**: ~7 minutes

---

## Summary

### Available Test Options

| Test Type | Time | Coverage | Automation | Recommended For |
|-----------|------|----------|------------|-----------------|
| **Manual Smoke Test** | 3 min | Basic | None | Quick validation |
| **Manual Full Test** | 15 min | High | None | UX feedback |
| **Unit Tests** | 2 min | High | Full | CI/CD, Regression |
| **Integration Test** | 10 min | Medium | Partial | Release testing |
| **E2E Test** | 20 min | Complete | None | Major releases |

### Next Steps

1. **Immediate**: Run manual smoke test to verify current state
2. **Short-term**: Run unit tests to validate ViewModel logic
3. **Medium-term**: Add more test cases for edge scenarios
4. **Long-term**: Set up automated testing in CI/CD pipeline

---

**Ready to start testing!** Choose the option that best fits your needs and timeline. I recommend starting with the manual smoke test (Option 1) followed by running the unit tests (Option 2).
