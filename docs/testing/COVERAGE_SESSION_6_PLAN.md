# Coverage Session 6: High-Impact Test Plan

**Date**: December 13, 2025  
**Current Overall Coverage**: ~53%  
**Session 5 Achievement**: Character model 68.3% → 91.2% (+22.9%)

## Top 10 High-Impact Files to Test Next

Based on coverage analysis, these files offer the best return on investment for testing effort:

### Tier 1: Critical Services (0% Coverage - High Priority)
These are core business logic services with 0% coverage that are now testable after ISaveGameService refactoring:

1. **Features/Quest/Services/QuestService.cs** - 0%
   - **Impact**: HIGH - Core quest management logic
   - **Testability**: ✅ Now mockable with ISaveGameService
   - **Lines**: ~200+ (estimated)
   - **Priority**: 1 - Start here
   - **Tests to write**: Quest creation, completion, progress tracking, rewards

2. **Features/Victory/Services/VictoryService.cs** - 0%
   - **Impact**: HIGH - Victory condition checking, endgame logic
   - **Testability**: ✅ Now mockable with ISaveGameService
   - **Lines**: ~150+ (estimated)
   - **Priority**: 2
   - **Tests to write**: Victory conditions, final boss detection, rewards

3. **Features/Victory/Services/NewGamePlusService.cs** - 0%
   - **Impact**: MEDIUM-HIGH - New Game+ features
   - **Testability**: ✅ Now mockable with ISaveGameService
   - **Lines**: ~100+ (estimated)
   - **Priority**: 3
   - **Tests to write**: NG+ initialization, stat retention, difficulty scaling

4. **Features/Quest/Services/QuestProgressService.cs** - 0%
   - **Impact**: MEDIUM - Quest progress tracking
   - **Testability**: ✅ Likely testable
   - **Lines**: ~100+ (estimated)
   - **Priority**: 4
   - **Tests to write**: Progress updates, milestone tracking

5. **Features/Quest/Services/MainQuestService.cs** - 0%
   - **Impact**: MEDIUM - Main storyline quest logic
   - **Testability**: ✅ Likely testable
   - **Lines**: ~100+ (estimated)
   - **Priority**: 5
   - **Tests to write**: Main quest progression, chapter unlocking

### Tier 2: Partially Covered Services (Quick Wins)

6. **Services/LevelUpService.cs** - 2.8%
   - **Impact**: HIGH - Almost no coverage on critical leveling logic
   - **Testability**: ✅ Should be straightforward
   - **Lines**: ~100+ (estimated, 97% uncovered)
   - **Priority**: 6
   - **Tests to write**: Stat allocation, skill point distribution, level validation

7. **Shared/Services/CharacterViewService.cs** - 36.5%
   - **Impact**: MEDIUM - Character stat display logic
   - **Testability**: ✅ UI service, testable with Spectre.Console.Testing
   - **Lines**: ~80+ uncovered (estimated)
   - **Priority**: 7
   - **Tests to write**: Stat formatting, equipment display, UI rendering

8. **Features/SaveLoad/SaveGameService.cs** - 37.7%
   - **Impact**: HIGH - Core persistence, but many methods untested
   - **Testability**: ✅ Already has some tests, extend coverage
   - **Lines**: ~150+ uncovered (estimated)
   - **Priority**: 8
   - **Tests to write**: Edge cases, error handling, data validation

### Tier 3: Well-Covered Services (Diminishing Returns)

9. **Shared/Services/GameStateService.cs** - 77.3%
   - **Impact**: MEDIUM - Game state management
   - **Testability**: ✅ Most logic already covered
   - **Lines**: ~50 uncovered (estimated)
   - **Priority**: 9
   - **Tests to write**: Edge cases, state transitions

10. **Features/Combat/CombatService.cs** - 81.8%
    - **Impact**: MEDIUM - Combat mechanics
    - **Testability**: ✅ Most logic already covered
    - **Lines**: ~40 uncovered (estimated)
    - **Priority**: 10
    - **Tests to write**: Complex combat scenarios, edge cases

## Bonus Targets (Models - Goal: 100% Model Coverage)

After Tier 1-2 services, complete model coverage:

11. **Models/Character.cs** - 91.2% → 100%
    - **Uncovered**: ~8.8% remaining (property setters, edge cases)
    - **Tests needed**: Property validation, edge case scenarios

12. **Models/Quest.cs** - 97.7% → 100%
    - **Uncovered**: ~2.3% remaining
    - **Tests needed**: Remaining edge cases

## Utility Classes (User Requested - Later Phase)

Per user: "later we'll focus on utilities"

13. **Utilities/SkillEffectCalculator.cs** - 96.3%
    - **Uncovered**: 3.7%
    - **Tests needed**: Edge cases in skill calculations

14. **Utilities/TraitApplicator.cs** - 96.3%
    - **Uncovered**: 3.7%
    - **Tests needed**: Trait application edge cases

## Implementation Strategy

### Phase 1: Quest System (Files 1, 4, 5)
- Start with QuestService (priority 1)
- Then QuestProgressService (priority 4)
- Then MainQuestService (priority 5)
- **Expected Impact**: ~15-20% overall coverage increase

### Phase 2: Victory & NG+ (Files 2, 3)
- VictoryService (priority 2)
- NewGamePlusService (priority 3)
- **Expected Impact**: ~5-10% overall coverage increase

### Phase 3: Core Services (Files 6, 7, 8)
- LevelUpService (priority 6)
- CharacterViewService (priority 7)
- SaveGameService extensions (priority 8)
- **Expected Impact**: ~8-12% overall coverage increase

### Phase 4: Polish & Edge Cases (Files 9, 10, 11, 12)
- Complete remaining service coverage
- Achieve 100% model coverage
- **Expected Impact**: ~5-8% overall coverage increase

## Estimated Total Impact

- **Current Coverage**: ~53%
- **After All 4 Phases**: ~85-90% coverage
- **Test Count**: +150-200 tests (currently 1532)
- **Time Estimate**: 4-6 hours of focused work

## Success Criteria

✅ All Quest services > 90% coverage  
✅ All Victory services > 90% coverage  
✅ LevelUpService > 90% coverage  
✅ All Models at 100% coverage  
✅ Overall project coverage > 70%  

## Next Steps

1. ✅ Start with QuestService - highest priority
2. Create QuestServiceTests.cs
3. Mock ISaveGameService dependency
4. Write comprehensive test suite
5. Run coverage and verify improvement
6. Move to next file in priority order

---

**Notes**:
- ISaveGameService refactoring in Session 5 unlocked testing for Quest/Victory services
- Focus on high-value files first (0% coverage → 90%+ coverage = huge impact)
- Models are already mostly covered, finish them after services
- Utilities last per user preference
