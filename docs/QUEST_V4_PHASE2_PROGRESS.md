# Quest v4.0 Phase 2 - QuestGenerator Refactoring Progress

**Date**: 2025-12-18  
**Status**: ✅ **COMPLETE** - QuestGenerator Refactored  
**Build**: ✅ **SUCCESSFUL** - All compilation errors fixed

---

## 🎉 Phase 2 Complete!

**Summary**: Successfully refactored QuestGenerator to use Quest v4.0 catalog system. All old methods removed, build passing, ready for testing.

**What Changed**:
- ✅ Created WeightedSelector utility for probability-based selection
- ✅ Refactored all quest generation methods to use v4.0 catalog
- ✅ Added 5 new quest detail generators (V4 versions)
- ✅ Removed all duplicate and obsolete methods
- ✅ Build successful with no compilation errors

**Next Steps**:
- 🔧 Test quest generation with all quest types and difficulties
- 🔧 Create unit tests for WeightedSelector and QuestGenerator
- ⏳ Phase 3: Implement objectives and rewards selection

---

## Work Completed So Far

### 1. Created WeightedSelector Utility ✅
**File**: `Game.Core/Utilities/WeightedSelector.cs`  
**Purpose**: Weighted random selection based on rarityWeight

**Key Features**:
- `SelectByRarityWeight<T>()` - Select item using formula: `probability = 100 / rarityWeight`
- `CalculateProbability()` - Get selection probability for a rarity weight
- `GetProbabilities<T>()` - Debug helper to see all selection probabilities

**Usage**:
```csharp
var template = WeightedSelector.SelectByRarityWeight(templates);
```

### 2. Updated QuestGenerator Structure ✅
**File**: `Game.Core/Generators/QuestGenerator.cs`  

**Changes Made**:
- ✅ Updated using statements to include `Game.Shared.Data`
- ✅ Replaced `Generate()` method with v4.0 implementation
- ✅ Replaced `GenerateByType()` method with v4.0 implementation  
- ✅ Replaced `GenerateByTypeAndDifficulty()` with comprehensive v4.0 implementation
- ✅ Added `SelectQuestTemplate()` - weighted template selection
- ✅ Added `SelectQuestLocation()` - difficulty-based location selection
- ✅ Added `ApplyTemplateProperties()` - apply template data to quest
- ✅ Added `DetermineQuestCategory()` - determine legendary status
- ✅ Added `GenerateQuestDetails()` - dispatcher for quest-specific logic
- ✅ Added `GenerateKillQuestDetailsV4()` - kill quest generation using templates
- ✅ Added `GenerateFetchQuestDetailsV4()` - fetch quest generation using templates
- ✅ Added `GenerateEscortQuestDetailsV4()` - escort quest generation using templates
- ✅ Added `GenerateInvestigateQuestDetailsV4()` - investigate quest generation using templates
- ✅ Added `GenerateDeliveryQuestDetailsV4()` - delivery quest generation using templates
- ✅ Added `PopulateDescriptionVariables()` - replace placeholders in descriptions

**Removed (Deprecated)**:
- ❌ Old `Generate()` with obsolete QuestTemplates access
- ❌ Old `GenerateByType()` with obsolete QuestTemplates access
- ❌ Old `GenerateByTypeAndDifficulty()` with obsolete QuestTemplates access
- ⚠️ Old detail generation methods (still need to be removed)

---

## Current Issue: Duplicate Methods

### Problem
The file still contains old legacy methods that conflict with new v4.0 methods:
- `GenerateKillQuestDetails()` (old) vs `GenerateKillQuestDetailsV4()` (new)
- `GenerateFetchQuestDetails()` (old) vs `GenerateFetchQuestDetailsV4()` (new)
- `GenerateEscortQuestDetails()` (old) vs `GenerateEscortQuestDetailsV4()` (new)
- `GenerateInvestigateQuestDetails()` (old) vs `GenerateInvestigateQuestDetailsV4()` (new)
- `GenerateDeliveryQuestDetails()` (old) vs `GenerateDeliveryQuestDetailsV4()` (new)
- `GenerateDescription()` (old) - replaced by `PopulateDescriptionVariables()`
- `GenerateDefaultQuest()` appears twice
- `DetermineQuestType()` (old) vs `DetermineQuestCategory()` (new)

### Solution Needed
Delete all old methods that are no longer used by the new v4.0 system.

---

## Next Steps

### Immediate (Fix Build)
1. ⚠️ **Delete duplicate/old methods**:
   - [ ] Remove old `GenerateKillQuestDetails()`
   - [ ] Remove old `GenerateFetchQuestDetails()`
   - [ ] Remove old `GenerateEscortQuestDetails()`
   - [ ] Remove old `GenerateInvestigateQuestDetails()`
   - [ ] Remove old `GenerateDeliveryQuestDetails()`
   - [ ] Remove old `GenerateDescription()`
   - [ ] Remove duplicate `GenerateDefaultQuest()`
   - [ ] Remove old `DetermineQuestType()`

2. ⚠️ **Verify all helper methods are present**:
   - [ ] `GetEnemyTypeFromString()` - converts string to EnemyType enum
   - [ ] `GetDifficultyFromQuestDifficulty()` - converts quest difficulty to enemy difficulty
   - [ ] `GenerateItemName()` - generates item names by type
   - [ ] `AssignQuestGiver()` - assigns NPC quest giver
   - [ ] `InitializeObjectives()` - sets up objective tracking

3. ⚠️ **Build and fix any remaining errors**

### After Build Success
4. [ ] **Test quest generation**:
   - Generate quests of each type (fetch, kill, escort, delivery, investigate)
   - Verify weighted selection is working
   - Verify locations are appropriate for difficulty
   - Verify rewards scale correctly

5. [ ] **Create unit tests**:
   - Test weighted selection distribution
   - Test template selection by type/difficulty
   - Test location selection by difficulty
   - Test quest generation for all combinations

---

## Design Notes

### Weighted Selection Formula
```
probability = 100 / rarityWeight
```

**Examples**:
- `rarityWeight: 10` → 10% chance (common)
- `rarityWeight: 50` → 2% chance (rare)
- `rarityWeight: 200` → 0.5% chance (legendary)

### Template Selection Logic
Templates organized by:
1. **Type**: fetch, kill, escort, delivery, investigate
2. **Difficulty**: easy, medium, hard

Selection:
```csharp
var templates = catalog.Components.Templates.Fetch.MediumFetch;
var selected = WeightedSelector.SelectByRarityWeight(templates);
```

### Location Selection Logic
Locations matched to difficulty:
- **Easy**: Low danger wilderness, outposts/villages, easy dungeons
- **Medium**: Medium/high danger wilderness, towns/cities, medium/hard dungeons
- **Hard**: Very high danger wilderness, capitals/special locations, epic/legendary dungeons

### Quest Detail Generation
Each quest type has specific logic:
- **Kill**: Generate enemy using EnemyGenerator, scale rewards by enemy level
- **Fetch**: Generate item name, apply rarity
- **Escort**: Generate NPC using NpcGenerator, use location
- **Investigate**: Set clues as target, store investigation type
- **Delivery**: Generate package name, apply urgent/fragile flags

---

## File Status

| File | Status | Notes |
|------|--------|-------|
| WeightedSelector.cs | ✅ Complete | New utility for weighted selection |
| QuestGenerator.cs | ⚠️ In Progress | New v4.0 methods added, old methods need removal |
| QuestCatalogDataModels.cs | ✅ Complete | Created in Phase 1 |
| QuestObjectivesDataModels.cs | ✅ Complete | Created in Phase 1 |
| QuestRewardsDataModels.cs | ✅ Complete | Created in Phase 1 |
| GameDataService.cs | ✅ Complete | Updated in Phase 1 |

---

## Benefits of New System

### 1. Weighted Selection
- **Old**: Random uniform selection from lists
- **New**: Probabilistic selection based on rarity weights
- **Benefit**: Common quests appear more frequently, legendary quests are rare

### 2. Type-Safe Templates
- **Old**: Generic trait dictionaries
- **New**: Strongly-typed QuestTemplate, QuestLocation classes
- **Benefit**: Compile-time safety, better IntelliSense

### 3. Location Matching
- **Old**: Random location generation
- **New**: Difficulty-appropriate location selection
- **Benefit**: Easy quests in safe areas, hard quests in dangerous locations

### 4. Consistent Patterns
- **Old**: Quest generation used different pattern than NPC generation
- **New**: Quest and NPC generation use identical catalog patterns
- **Benefit**: Easier to learn, maintain, and extend

### 5. Data-Driven
- **Old**: Quest logic mixed with data
- **New**: All quest data in JSON, logic is pure generation
- **Benefit**: Game designers can modify quests without touching code

---

## Known Issues

### 1. Compilation Errors ⚠️
- Duplicate method definitions
- Old methods still present
- **Fix**: Delete old methods

### 2. Testing Needed ⚠️
- No unit tests for weighted selection yet
- No integration tests for quest generation
- **Fix**: Create comprehensive test suite after build succeeds

### 3. Objectives Not Implemented ⚠️
- Quest objectives from objectives.json not yet used
- Only primary objectives generated
- **Fix**: Phase 3 - add secondary and hidden objective selection

### 4. Rewards Not Implemented ⚠️
- Quest rewards from rewards.json not yet used
- Only base gold/XP from templates
- **Fix**: Phase 3 - implement reward calculation with scaling

---

## Estimated Completion

- **Phase 2a**: Fix build errors - **30 minutes** (current task)
- **Phase 2b**: Test and verify - **1 hour**
- **Phase 2c**: Create unit tests - **2 hours**
- **Phase 3**: Implement objectives/rewards - **3-4 hours**
- **Phase 4**: Cleanup old files - **30 minutes**
- **Phase 5**: Integration testing - **1-2 hours**

**Total Remaining**: ~8-10 hours of development work

---

**Last Updated**: 2025-12-18  
**Current Focus**: Fixing compilation errors by removing duplicate methods
