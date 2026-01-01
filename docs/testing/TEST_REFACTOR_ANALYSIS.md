# Test Suite Refactor Analysis

**Date**: December 18, 2024
**Purpose**: Identify obsolete tests due to data refactor and determine which tests to keep/refactor

---

## Current Data Architecture

### Actual JSON Structure (Post-Refactor)

Our data now follows a standardized hierarchical structure:

```
RealmEngine.Shared/Data/Json/
├── general/
│   ├── adjectives.json (NameList - components structure)
│   ├── colors.json (HybridArray - components + patterns)
│   ├── verbs.json, smells.json, etc.
├── npcs/
│   ├── names/
│   │   ├── first_names.json (NameCatalog - categories: male_*, female_*)
│   │   └── last_names.json
│   ├── occupations/
│   │   └── occupations.json (GenericCatalog - catalog structure)
│   ├── dialogue/, personalities/
├── items/
│   ├── armor/, weapons/, consumables/
│   │   └── catalog.json, names.json
├── enemies/
│   ├── beasts/, demons/, dragons/, etc.
│   │   └── catalog.json, names.json, abilities.json
├── quests/
    └── locations/, objectives/, rewards/, templates/
```

### Data Type Mappings

| Editor Type | Data Structure | Examples | Key Features |
|------------|----------------|----------|--------------|
| **NameCatalog** | `categories: { category_name: [items] }` | first_names.json, last_names.json | Category-based name lists |
| **NameList** | `components: { category: [items] }` | adjectives.json, verbs.json | Simple component lists |
| **HybridArray** | `components: {...}, patterns: [...]` | colors.json, textures.json | Components + pattern generation |
| **GenericCatalog** | `catalog: { id: {...properties} }` | occupations.json, catalog.json files | Dynamic property-based catalogs |
| **FlatItem** | Simple arrays | time_of_day.json, weather.json | Basic string arrays |

---

## Test File Analysis

### ✅ KEEP & REFACTOR

#### 1. **NameCatalogEditorUITests.cs** (7 tests)
**Status**: Outdated but USEFUL
**Why Keep**: Tests critical NameCatalog editor functionality
**Current Issue**: Creates fake test data that never loads
**Refactor Plan**:
- Remove custom test data creation
- Navigate to actual `npcs/names/first_names.json`
- Update assertions to match real data structure
- Categories: `male_common`, `male_noble`, `female_common`, etc.
**Effort**: 1-2 hours

#### 2. **NameListEditorUITests.cs** (10 tests)
**Status**: Outdated but USEFUL
**Why Keep**: Tests NameList editor (adjectives, verbs, etc.)
**Current Issue**: Navigates to non-existent data
**Refactor Plan**:
- Navigate to `general/adjectives.json`
- Update to expect `components` structure
- Categories: `positive`, `negative`, `size`, `appearance`, `condition`
**Effort**: 1-2 hours

#### 3. **HybridArrayEditorUITests.cs** (13 tests)
**Status**: Outdated but USEFUL
**Why Keep**: Tests HybridArray editor (patterns)
**Current Issue**: Tests colors.json which exists but may have different structure
**Refactor Plan**:
- Navigate to `general/colors.json`
- Test Items/Components/Patterns tabs
- Verify components: `base_color`, `modifier`, `material`
- Verify patterns: `base_color`, `modifier + base_color`, `material`
**Effort**: 2-3 hours

#### 4. **GenericCatalogEditorUITests.cs** (7 tests)
**Status**: Outdated but USEFUL
**Why Keep**: Tests most complex editor type
**Current Issue**: Tests occupations.json navigation
**Refactor Plan**:
- Navigate to `npcs/occupations/occupations.json`
- Test dynamic catalog properties
- Verify search functionality
- Test add/edit/delete operations
**Effort**: 2-3 hours

### ❌ DELETE OR DEPRECATE

#### 5. **FlatItemEditorUITests.cs** (8 tests)
**Status**: OBSOLETE
**Why Delete**: 
- FlatItem editor is simplest (just arrays)
- Low value tests (basic CRUD)
- Better covered by integration tests
- Takes significant effort to refactor
**Decision**: **DELETE** - Not worth refactoring effort

#### 6. **AbilitiesEditorUITests.cs** (if exists)
**Status**: Check if this exists and is used
**Decision**: Review and determine usefulness

---

## Test Strategy Matrix

| Test Class | Tests | Pass Rate | Refactor Effort | Value | Decision |
|-----------|-------|-----------|----------------|-------|----------|
| NameCatalogEditorUITests | 7 | 0% | Medium (2h) | HIGH | **REFACTOR** |
| NameListEditorUITests | 10 | 0% | Medium (2h) | HIGH | **REFACTOR** |
| HybridArrayEditorUITests | 13 | 0% | High (3h) | MEDIUM | **REFACTOR** |
| GenericCatalogEditorUITests | 7 | 0% | High (3h) | HIGH | **REFACTOR** |
| FlatItemEditorUITests | 8 | 0% | Medium (2h) | LOW | **DELETE** |
| TreeNavigationUITests | 15 | 73% | Low (1h) | HIGH | **FIX** |
| ContentBuilderUITests | 8 | 62% | Low (1h) | HIGH | **FIX** |
| AllEditorsUITests | 15 | 27% | Medium (2h) | MEDIUM | **REVIEW** |

---

## Refactor Plan

### Phase 1: DELETE Obsolete Tests (15 minutes)
- [x] Analyze FlatItemEditorUITests.cs
- [ ] Delete FlatItemEditorUITests.cs (8 tests removed)
- [ ] Update test count documentation

### Phase 2: REFACTOR High-Value Editor Tests (6-8 hours)

#### Step 1: NameCatalogEditorUITests (2 hours)
1. Remove `CreateTestDataFiles()` method
2. Remove `_testDataPath` and environment variable
3. Update `NavigateToFirstNamesEditor()`:
   - Navigate to NPCs → Names → first_names.json
4. Update category expectations:
   - Old: `male_common`, `female_common`
   - New: `male_common`, `male_noble`, `male_mystical`, `female_common`, etc.
5. Update test data expectations (real names vs fake "John", "Mary")
6. Run tests and verify

#### Step 2: NameListEditorUITests (2 hours)
1. Update `NavigateToAdjectivesEditor()`:
   - Navigate to General → adjectives.json
2. Update category expectations:
   - Categories: `positive`, `negative`, `size`, `appearance`, `condition`
3. Update data structure expectations (components, not categories)
4. Run tests and verify

#### Step 3: HybridArrayEditorUITests (3 hours)
1. Update `NavigateToColorsEditor()`:
   - Navigate to General → colors.json
2. Verify tabs: Items, Components, Patterns
3. Update component expectations:
   - Components: `base_color`, `modifier`, `material`
4. Update pattern expectations:
   - Patterns: `base_color`, `modifier + base_color`, `material`
5. Test pattern generation preview
6. Run tests and verify

#### Step 4: GenericCatalogEditorUITests (3 hours)
1. Navigate to NPCs → Occupations → occupations.json
2. Verify catalog structure with dynamic properties
3. Update search/filter tests
4. Test add/edit/delete with real catalog items
5. Run tests and verify

### Phase 3: FIX Navigation & Integration Tests (2 hours)
1. TreeNavigationUITests - Fix 4 failing tests
2. ContentBuilderUITests - Fix 3 failing tests
3. AllEditorsUITests - Review and fix critical tests

---

## Expected Outcomes

### Before Refactor
- Total UI Tests: 103
- Passing: 40 (38.8%)
- Failing: 63 (61.2%)

### After Phase 1 (Delete)
- Total UI Tests: 95 (removed 8)
- Passing: 40 (42.1%)
- Failing: 55 (57.9%)

### After Phase 2 (Refactor Editors)
- Total UI Tests: 95
- Passing: 72-80 (76-84%)
- Failing: 15-23 (16-24%)

### After Phase 3 (Fix Navigation)
- Total UI Tests: 95
- Passing: 80-88 (84-93%)
- Failing: 7-15 (7-16%)

---

## Refactoring Patterns

### Pattern 1: Remove Custom Test Data
```csharp
// BEFORE
private readonly string _testDataPath;
public NameCatalogEditorUITests() : base()
{
    _testDataPath = Path.Combine(Path.GetTempPath(), "ContentBuilderUITests", ...);
    CreateTestDataFiles(); // Creates fake JSON
    Environment.SetEnvironmentVariable("CONTENTBUILDER_DATA_PATH", _testDataPath);
    LaunchApplication();
}

// AFTER
public NameCatalogEditorUITests() : base()
{
    LaunchApplication();
    Thread.Sleep(1500);
    NavigateToFirstNamesEditor(); // Uses real data
}
```

### Pattern 2: Navigate to Real Data
```csharp
// BEFORE
private void NavigateToFirstNamesEditor()
{
    var namesNode = treeView.Items.FirstOrDefault(i => i.Name.Contains("Names"));
    var firstNamesNode = namesNode.Items.FirstOrDefault(i => i.Name.Contains("first_names"));
    // Assumes "Names" is top-level - WRONG!
}

// AFTER
private void NavigateToFirstNamesEditor()
{
    // Navigate: NPCs → Names → first_names.json
    var npcsNode = treeView.Items.FirstOrDefault(i => i.Name.Contains("NPCs"));
    npcsNode.Expand();
    Thread.Sleep(300);
    
    var namesFolder = npcsNode.Items.FirstOrDefault(i => i.Name.Contains("Names"));
    namesFolder.Expand();
    Thread.Sleep(300);
    
    var firstNamesFile = namesFolder.Items.FirstOrDefault(i => i.Name.Contains("first_names"));
    firstNamesFile.Click();
    Thread.Sleep(1000);
}
```

### Pattern 3: Update Data Expectations
```csharp
// BEFORE
var categoryItems = categoryList.Items;
categoryItems.Should().Contain(c => c.Name == "male_common");
categoryItems.Should().Contain(c => c.Name == "female_common");
// Expected 2 categories (fake data)

// AFTER
var categoryItems = categoryList.Items.Select(i => i.Name).ToList();
categoryItems.Should().Contain("male_common");
categoryItems.Should().Contain("male_noble");
categoryItems.Should().Contain("male_mystical");
categoryItems.Should().Contain("female_common");
categoryItems.Should().Contain("female_noble");
// Expected 10+ categories (real data)
```

---

## Next Steps

1. **DELETE FlatItemEditorUITests.cs** ✅
2. **REFACTOR NameCatalogEditorUITests** (Start here - highest value, medium effort)
3. **REFACTOR NameListEditorUITests** (Second - high value, medium effort)
4. **REFACTOR HybridArrayEditorUITests** (Third - medium value, high effort)
5. **REFACTOR GenericCatalogEditorUITests** (Fourth - high value, high effort)
6. **FIX TreeNavigationUITests** (Quick wins)
7. **FIX ContentBuilderUITests** (Quick wins)
8. **REVIEW AllEditorsUITests** (Determine if useful)

---

## Success Criteria

- ✅ All obsolete tests deleted
- ✅ Editor tests use real data paths
- ✅ Tests match current data structure (components, categories, patterns)
- ✅ 80%+ pass rate on UI tests
- ✅ No custom test data or environment variables
- ✅ Tests validate actual user workflows
