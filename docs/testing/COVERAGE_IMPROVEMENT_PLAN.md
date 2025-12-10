# Code Coverage Improvement Plan
**Generated:** December 9, 2025  
**Current Coverage:** 36.6% (2155/5875 lines)  
**Target:** 95%+ coverage  
**Tests Status:** 387 passing, 1 skipped

## Executive Summary

**Current State:**
- Total Lines: 5,875 coverable lines
- Covered: 2,155 lines (36.6%)
- Uncovered: 3,720 lines (63.4%)
- **Gap to 95%:** Need to cover ~3,430 additional lines

**Well-Tested Areas (>80% coverage):**
- ✅ Settings Validators: 100% (5 classes)
- ✅ CharacterCreationService: 97.3%
- ✅ CharacterCreationOrchestrator: 93.8%
- ✅ ExplorationService: 93.3% (just improved from 8 to 20 tests!)
- ✅ InventoryService: 86%
- ✅ GameplayService: 83.3%
- ✅ NpcGenerator: 82.5%
- ✅ CombatService: 81.4%

**Critical Gaps (0% coverage - 25 classes):**
- ❌ All MediatR Command/Query Handlers (20+ classes)
- ❌ GameEngine (main game loop)
- ❌ DeathService, VictoryService, QuestService
- ❌ All Orchestrators (Victory, Inventory)
- ❌ AudioService, LoggingService
- ❌ MenuService: 5.3%
- ❌ CharacterViewService: 2.3%
- ❌ LevelUpService: 2.8%

## Phase 1: Low-Hanging Fruit (Target: 50% coverage)
**Estimated Impact:** +800 lines (~13.6%)

### 1.1 MediatR Query Handlers (Simple DTOs)
**Files to test:**
- `GetCurrentLocationQueryHandler.cs` (returns simple location string)
- `GetKnownLocationsQueryHandler.cs` (returns list of locations)
- `GetCombatStateHandler.cs` (returns combat state)
- `GetEnemyInfoHandler.cs` (returns enemy details)
- `GetAllSavesHandler.cs` (returns save game list)
- `GetMostRecentSaveHandler.cs` (returns latest save)
- `GetActiveQuestsHandler.cs` (returns quest list)
- `GetInventoryItemsHandler.cs` (returns inventory)
- `GetEquippedItemsHandler.cs` (returns equipped items)

**Test Strategy:**
- Create test file: `Game.Tests/Features/Queries/QueryHandlerTests.cs`
- Mock dependencies, verify correct data returned
- Simple arrange-act-assert pattern
- **Estimated lines:** 200-300 lines covered

### 1.2 MediatR Command Validators
**Files to test:**
- `CreateCharacterValidator.cs`
- `AttackEnemyValidator.cs`
- `DefendActionValidator.cs`
- `FleeFromCombatValidator.cs`
- `UseCombatItemValidator.cs`
- `EquipItemValidator.cs`

**Test Strategy:**
- Use FluentValidation.TestHelper (already proven with CharacterValidatorTests)
- Test valid inputs, invalid inputs, edge cases
- **Estimated lines:** 150-200 lines covered

### 1.3 Simple Service Methods
**Services:**
- `MenuService.cs` (5.3% → 80%+)
- `CharacterViewService.cs` (2.3% → 80%+)
- `LevelUpService.cs` (2.8% → 80%+)

**Test Strategy:**
- Use existing TestConsoleHelper patterns
- Mock UI interactions
- **Estimated lines:** 300-400 lines covered

## Phase 2: Medium Complexity (Target: 70% coverage)
**Estimated Impact:** +1,200 lines (~20%)

### 2.1 MediatR Command Handlers (Business Logic)
**Files to test:**
- `AttackEnemyHandler.cs` (already 100%! ✅)
- `DefendActionHandler.cs`
- `FleeFromCombatHandler.cs`
- `UseCombatItemHandler.cs`
- `EquipItemHandler.cs`
- `UnequipItemHandler.cs`
- `UseItemHandler.cs`
- `DropItemHandler.cs`
- `SortInventoryHandler.cs`
- `SaveGameHandler.cs`
- `LoadGameHandler.cs`
- `DeleteSaveHandler.cs`

**Test Strategy:**
- Follow pattern from `AttackEnemyHandlerTests.cs` (100% coverage achieved)
- Create one test file per feature area
- **Estimated lines:** 600-800 lines covered

### 2.2 Orchestrators
**Files to test:**
- `InventoryOrchestrator.cs` (currently 0%)
- `CombatOrchestrator.cs` (currently 45.9% → 90%+)
- `VictoryOrchestrator.cs` (currently 0%)

**Test Strategy:**
- Test user interaction flows
- Mock console input/output with TestConsoleHelper
- **Estimated lines:** 400-500 lines covered

### 2.3 Additional Generators
**Files:**
- `QuestGenerator.cs` (0% → 80%+)
- Improve `ItemGenerator.cs` (67.2% → 90%+)
- Improve `EnemyGenerator.cs` (71.5% → 90%+)

**Test Strategy:**
- Follow patterns from existing generator tests
- Verify data variety and correctness
- **Estimated lines:** 200-300 lines covered

## Phase 3: Complex Business Logic (Target: 85% coverage)
**Estimated Impact:** +900 lines (~15%)

### 3.1 Core Services
**Files to test:**
- `DeathService.cs` (0% → 90%+)
- `VictoryService.cs` (0% → 90%+)
- `QuestService.cs` (0% → 90%+)
- `MainQuestService.cs` (0% → 90%+)
- `QuestProgressService.cs` (0% → 90%+)
- `NewGamePlusService.cs` (0% → 90%+)
- `HallOfFameService.cs` (0% → 90%+)
- `AchievementService.cs` (0% → 90%+)

**Test Strategy:**
- Create comprehensive test suites
- Test happy path and edge cases
- Mock dependencies (SaveGameService, GameStateService, etc.)
- **Estimated lines:** 700-900 lines covered

### 3.2 Model Coverage
**Files:**
- `Character.cs` (26% → 80%+)
- `Item.cs` (24.4% → 80%+)
- `Enemy.cs` (71.4% → 95%+)
- `Quest.cs` (0% → 80%+)
- `SaveGame.cs` (45% → 80%+)
- `DifficultySettings.cs` (15.6% → 90%+)

**Test Strategy:**
- Create comprehensive model tests
- Test all methods, properties, calculations
- **Estimated lines:** 400-500 lines covered

## Phase 4: Integration & GameEngine (Target: 95%+)
**Estimated Impact:** +530 lines (~9%)

### 4.1 GameEngine.cs
**Current:** 0%  
**Target:** 80%+

**Test Strategy:**
- Create integration tests for main game loop
- Mock all dependencies (console, services, etc.)
- Test menu navigation flows
- **Estimated lines:** 200-300 lines covered

### 4.2 Integration Tests
**Files:**
- `GameStateService.cs` (76.1% → 95%+)
- `SaveGameService.cs` (36.9% → 90%+)
- `ConsoleUI.cs` (19.1% → 80%+)
- `ApocalypseTimer.cs` (9.4% → 80%+)

**Test Strategy:**
- End-to-end workflow tests
- Multi-service interaction tests
- **Estimated lines:** 300-400 lines covered

### 4.3 Untested Features
**Files:**
- `AudioService.cs` (0% → 60%+) - partial coverage acceptable
- `LoggingService.cs` (0% → 60%+) - partial coverage acceptable
- `TraitApplicator.cs` (11% → 80%+)
- `ValidationBehavior.cs` (0% → 80%+)

**Estimated lines:** 200-300 lines covered

## Detailed Action Plan

### Immediate Next Steps (Today)
1. ✅ ExplorationService expanded (8 → 20 tests) - DONE!
2. ⏭️ Create QueryHandlerTests.cs (9 simple handlers)
3. ⏭️ Create CommandValidatorTests.cs (6 validators)
4. ⏭️ Expand MenuServiceTests.cs
5. ⏭️ Create CharacterViewServiceTests.cs
6. ⏭️ Create LevelUpServiceTests.cs

**Expected Coverage After Today:** ~50%

### This Week
1. Create all MediatR command handler tests (12 handlers)
2. Test orchestrators (Inventory, Victory)
3. Expand model tests (Character, Item, Quest)
4. Create DeathServiceTests, VictoryServiceTests, QuestServiceTests

**Expected Coverage After Week:** ~75%

### Next Week
1. GameEngine integration tests
2. Complete model coverage
3. Final service tests (Achievement, NewGamePlus, HallOfFame)
4. UI and infrastructure tests

**Expected Coverage After 2 Weeks:** ~90-95%

## Test File Organization

```
Game.Tests/
├── Features/
│   ├── Achievement/
│   │   └── AchievementServiceTests.cs (NEW)
│   ├── Combat/
│   │   ├── CombatOrchestratorTests.cs (EXPAND)
│   │   ├── DefendActionHandlerTests.cs (NEW)
│   │   ├── FleeFromCombatHandlerTests.cs (NEW)
│   │   └── UseCombatItemHandlerTests.cs (NEW)
│   ├── Death/
│   │   ├── DeathServiceTests.cs (NEW)
│   │   └── HallOfFameServiceTests.cs (NEW)
│   ├── Inventory/
│   │   ├── InventoryOrchestratorTests.cs (NEW)
│   │   ├── EquipItemHandlerTests.cs (NEW)
│   │   ├── UseItemHandlerTests.cs (NEW)
│   │   └── DropItemHandlerTests.cs (NEW)
│   ├── Quest/
│   │   ├── QuestServiceTests.cs (NEW)
│   │   ├── MainQuestServiceTests.cs (NEW)
│   │   └── QuestProgressServiceTests.cs (NEW)
│   ├── Victory/
│   │   ├── VictoryServiceTests.cs (NEW)
│   │   ├── VictoryOrchestratorTests.cs (NEW)
│   │   └── NewGamePlusServiceTests.cs (NEW)
│   └── Queries/
│       └── QueryHandlerTests.cs (NEW - all simple queries)
├── Models/
│   ├── CharacterTests.cs (EXPAND)
│   ├── ItemTests.cs (EXPAND)
│   ├── QuestTests.cs (NEW)
│   └── DifficultySettingsTests.cs (NEW)
├── Services/
│   ├── MenuServiceTests.cs (EXPAND)
│   ├── CharacterViewServiceTests.cs (NEW)
│   ├── LevelUpServiceTests.cs (NEW)
│   └── ApocalypseTimerTests.cs (NEW)
├── Validators/
│   └── CommandValidatorTests.cs (NEW - all FluentValidation validators)
└── Integration/
    └── GameEngineTests.cs (NEW)
```

## Success Metrics

### Daily Tracking
- Run coverage report: `dotnet test --collect:"XPlat Code Coverage" --settings:"coverage.runsettings"`
- Generate report: `reportgenerator -reports:<coverage.xml> -targetdir:./TestResults/CoverageReport`
- Check Summary.txt for line coverage percentage

### Milestones
- [ ] 50% coverage (Phase 1 complete)
- [ ] 70% coverage (Phase 2 complete)
- [ ] 85% coverage (Phase 3 complete)
- [ ] 95% coverage (Phase 4 complete)

### Quality Gates
- All new tests must pass
- No skipped tests (except known issues like equipped items persistence)
- Build must succeed with zero errors
- FluentAssertions for readable assertions
- TestConsoleHelper for UI testing consistency

## Key Insights from Coverage Report

**What's Working:**
- Services we've tested thoroughly have 80%+ coverage
- Validators are 100% covered (good pattern to follow)
- MediatR handlers we've tested (AttackEnemyHandler) are 100% covered
- Generators have decent coverage (67-82%)

**Biggest Opportunities:**
1. **MediatR Handlers** - 20+ classes at 0% (but simple to test)
2. **Orchestrators** - 3 classes mostly untested (but follow similar patterns)
3. **Core Services** - Death, Victory, Quest systems completely untested
4. **Models** - Character and Item classes have many untested methods
5. **GameEngine** - Main game loop has zero coverage (complex but critical)

## Notes

- **Excluded from coverage:** Test projects, generated code, obsolete code
- **Current test count:** 387 passing (excellent foundation!)
- **New tests added today:** ExplorationService +12 tests (8→20)
- **Known issues:** 1 skipped test (SaveGame_Should_Persist_Equipped_Items - LiteDB serialization)

## Recommended Priority Order

1. **Quick Wins:** Query handlers + validators (simple, high impact)
2. **Critical Paths:** Death, Combat, Victory services (core gameplay)
3. **User-Facing:** Orchestrators and UI services (user experience)
4. **Models:** Character, Item, Quest (foundation logic)
5. **Integration:** GameEngine and cross-service workflows

---

**Ready to proceed with Phase 1?** Let's start with creating QueryHandlerTests.cs to test 9 simple query handlers and boost coverage quickly!
