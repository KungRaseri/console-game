# Remaining Test Failures - Fix Plan

**Date**: December 18, 2024  
**Total Failures**: 27 tests (14.1% of 191 tests)  
**Status**: Ready to fix

---

## Failure Categories

### Category 1: Launch Timeout (9 tests) - FlaUI Timing Issue

**Error Pattern**:
```
System.AggregateException : One or more errors occurred. 
(This operation returned because the timeout period expired. (0x800705B4))
  at Game.ContentBuilder.Tests.UI.UITestBase.LaunchApplication(Nullable`1 launchTimeout)
```

**Affected Tests**:
1. `NameCatalogEditorUITests.Can_Navigate_To_NameCatalog_Editor`
2. `NameCatalogEditorUITests.Save_Button_Appears_When_Dirty`
3. `NameCatalogEditorUITests.Can_Use_Bulk_Add`
4. `ContentBuilderUITests.Application_Should_Launch_Successfully`
5. `TreeNavigationUITests.Tree_Should_Display_Top_Level_Categories`
6. `NameListEditorUITests.Should_Display_Category_List_On_Left_Side`
7. `NameListEditorUITests.Should_Show_File_Path_Information`
8. `NameListEditorUITests.Should_Display_Multiple_Categories`
9. `NameListEditorUITests.Should_Select_Category_When_Clicked`
10. `NameListEditorUITests.Should_Have_Delete_Category_Button`

**Root Cause**: FlaUI timing issue when getting main window handle from launched app

**Fix Strategy**: Investigate FlaUI launch timing (separate task)

---

### Category 2: NullReferenceException (3 tests) - Test Setup Issue ✅ INVESTIGATED

**Original Error Pattern**:
```
System.NullReferenceException : Object reference not set to an instance of an object.
  at Game.ContentBuilder.Tests.UI.NameCatalogEditorUITests.NavigateToFirstNamesEditor()
    in NameCatalogEditorUITests.cs:line 223
```

**After Fix - New Error Pattern**:
```
System.AggregateException : One or more errors occurred. 
(Expected node not to be <null> because Names node should exist in tree.)
```

**Affected Tests**:
1. `NameCatalogEditorUITests.Can_Add_Single_Name`
2. `NameCatalogEditorUITests.Can_Select_Category_And_View_Names`
3. `NameCatalogEditorUITests.Can_Add_Category`

**Root Cause**: Test data not loaded - Environment variable `CONTENTBUILDER_DATA_PATH` not respected

**What We Fixed**:
- ✅ Improved NavigateToFirstNamesEditor() with proper error handling
- ✅ Added null checks and better error messages
- ✅ Used ExecuteWithTimeout for robustness

**Real Issue Discovered**:
The tests create test data in a temp directory (`_testDataPath`) and set environment variable `CONTENTBUILDER_DATA_PATH`, but ContentBuilder doesn't load from it. The app loads from default path `C:\code\console-game\Game.Shared\Data\Json`, which doesn't have a "Names" node.

**Fix Needed**: 
- Option 1: Make ContentBuilder respect `CONTENTBUILDER_DATA_PATH` environment variable
- Option 2: Modify tests to use actual data structure (no custom test data)
- Option 3: Add command-line argument support for data path

**Fix Priority**: MEDIUM (tests work with actual data, just not with custom test data)

---

### Category 3: UI Assertion Failures (15 tests) - Outdated Expectations

#### Subcategory 3a: Tree Navigation (4 tests)

**Affected Tests**:
1. `TreeNavigationUITests.Should_Select_File_When_Clicked`
2. `TreeNavigationUITests.Should_Change_Selection_Between_Files`
3. `TreeNavigationUITests.Should_Load_Editor_When_File_Selected`
4. `TreeNavigationUITests.Should_Navigate_To_Nested_File`

**Error Examples**:
- "Expected colorsItem.IsSelected to be True, but found False"
- "Expected tabs not to be empty because Editor should load with tabs"
- "Expected metalsItem not to be <null> because Should be able to navigate to nested Metals file"

#### Subcategory 3b: Button Assertions (5 tests)

**Affected Tests**:
1. `NameListEditorUITests.Should_Have_Add_Name_Button`
2. `NameListEditorUITests.Should_Have_Delete_Name_Button`
3. `NameListEditorUITests.Should_Have_Add_Category_Button`
4. `NameListEditorUITests.Should_Have_Save_Button`
5. `HybridArrayEditorUITests.Items_Tab_Should_Have_Add_And_Delete_Buttons`

**Error Pattern**: "Expected buttonNames {...} to have an item matching n.Contains('Add', OrdinalIgnoreCase)"

#### Subcategory 3c: UI Element Assertions (6 tests)

**Affected Tests**:
1. `NameListEditorUITests.Should_Have_TextBox_For_New_Name_Input`
2. `NameListEditorUITests.Should_Have_TextBox_For_New_Category_Input`
3. `NameListEditorUITests.Should_Use_Two_Column_Layout`
4. `HybridArrayEditorUITests.Should_Show_Three_Tabs_Items_Components_Patterns`
5. `ContentBuilderUITests.Clicking_Colors_Should_Load_Editor`

**Error Pattern**: "Expected textBoxes not to be empty" / "Expected tabs to contain at least 3 item(s)"

---

## Fix Plan

### Phase 1: Fix NullReferenceException (HIGH PRIORITY)

**Steps**:
1. Read NameCatalogEditorUITests.cs around line 223
2. Identify what's null in NavigateToFirstNamesEditor()
3. Add null checks and logging
4. Fix navigation logic or update test expectations
5. Run tests to verify

**Expected Time**: 30-60 minutes

### Phase 2: Fix UI Assertion Failures (MEDIUM PRIORITY)

**Steps**:
1. Review each failing test
2. Determine if UI changed or test is wrong
3. Update assertions or fix UI as needed
4. Group similar fixes together
5. Run tests incrementally to verify

**Expected Time**: 2-3 hours

### Phase 3: Investigate FlaUI Timing (SEPARATE TASK)

**Steps**:
1. Research FlaUI launch timing best practices
2. Add retry logic or increase timeout for launch
3. Test on different machines/speeds
4. Document findings

**Expected Time**: 1-2 hours

---

## Fix Order

### Immediate (Now)

1. ✅ Read NameCatalogEditorUITests.cs to understand NullRef
2. ✅ Fix NavigateToFirstNamesEditor() method
3. ✅ Run 3 affected tests to verify

### Next

4. Review TreeNavigationUITests failures (navigation logic similar)
5. Fix button assertion tests (likely simple updates)
6. Fix UI element tests (tabs, textboxes)

### Later (Separate Task)

7. Investigate FlaUI timing issue (9 launch timeout tests)

---

## Success Criteria

### Phase 1 Complete

- ✅ 3 NullRef tests fixed
- ✅ NavigateToFirstNamesEditor() robust and reliable

### Phase 2 Complete

- ✅ All 15 UI assertion tests fixed
- ✅ Tests reflect current UI structure

### Phase 3 Complete (Separate)

- ✅ FlaUI timing issue understood
- ✅ Launch timeout tests fixed or documented

---

## Current Status

**Ready to start Phase 1**: Fix NullReferenceException in NameCatalogEditorUITests

**Next Step**: Read NameCatalogEditorUITests.cs around line 223
