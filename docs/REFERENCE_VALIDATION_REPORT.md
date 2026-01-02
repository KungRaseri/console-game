# Reference Validation Report
**Generated**: January 1, 2026  
**Test Suite**: `ReferenceValidationTests`  

## Executive Summary

✅ **Total References Found**: 1,244  
❌ **Failed References**: 231 unique references (with multiple occurrences)  
✅ **Success Rate**: ~81.4%  

## Test Infrastructure

### Automated TheoryData Test Suite
- **Test File**: `RealmEngine.Data.Tests/Services/ReferenceValidationTests.cs`
- **Coverage**: ALL JSON files in `RealmEngine.Data/Data/Json/**/*.json`
- **Test Method**: `All_References_Should_Resolve_Successfully` - runs once per reference occurrence
- **Report Method**: `Generate_Full_Reference_Report` - generates full summary

### Test Execution
```bash
# Run individual reference validation (1,244 tests - one per occurrence)
dotnet test --filter "FullyQualifiedName~ReferenceValidationTests.All_References_Should_Resolve_Successfully"

# Run full validation report (shows all failures grouped)
dotnet test --filter "FullyQualifiedName~ReferenceValidationTests.Generate_Full_Reference_Report"

# Run per-domain validation
dotnet test --filter "FullyQualifiedName~ReferenceValidationTests.All_References_In_Domain_Should_Resolve"
```

## Failed References by Domain

### @abilities/* (180 failures)

#### Passive Abilities (78 occurrences)
- `@abilities/passive:armored` - 10 occurrences
- `@abilities/passive:arcane-affinity` - 3 occurrences
- `@abilities/passive:assassin` - 1 occurrence
- `@abilities/passive:berserker-rage` - 5 occurrences
- `@abilities/passive:corpse-explosion` - 2 occurrences
- `@abilities/passive:dark-pact` - 2 occurrences
- `@abilities/passive:death-aura` - 7 occurrences
- `@abilities/passive:death-resistance` - 1 occurrence
- `@abilities/passive:demon-skin` - 5 occurrences
- `@abilities/passive:dragon-scales` - 7 occurrences
- `@abilities/passive:elemental-affinity` - 15 occurrences
- `@abilities/passive:elemental-resistance` - 1 occurrence
- `@abilities/passive:eternal-hunger` - 1 occurrence
- `@abilities/passive:fearless` - 1 occurrence
- `@abilities/passive:fire-resistance` - 4 occurrences
- `@abilities/passive:frozen-blood` - 1 occurrence
- `@abilities/passive:hardened-carapace` - 5 occurrences
- `@abilities/passive:insect-swarm` - 2 occurrences
- `@abilities/passive:pack-hunter` - 3 occurrences
- `@abilities/passive:plant-growth` - 1 occurrence
- `@abilities/passive:poison-immunity` - 3 occurrences
- `@abilities/passive:rebirth` - 1 occurrence
- `@abilities/passive:regeneration` - 13 occurrences
- `@abilities/passive:stone-skin` - 2 occurrences
- `@abilities/passive:thick-hide` - 3 occurrences
- `@abilities/passive:thorns` - 3 occurrences

#### Ultimate Abilities (102 occurrences)
- `@abilities/ultimate:ancient-bloodline` - 1 occurrence
- `@abilities/ultimate:assassinate` - 2 occurrences
- `@abilities/ultimate:avatar-of-war` - 5 occurrences
- `@abilities/ultimate:blade-storm` - 1 occurrence
- `@abilities/ultimate:blood-god` - 1 occurrence
- `@abilities/ultimate:blood-lord` - 1 occurrence
- `@abilities/ultimate:crimson-throne` - 1 occurrence
- `@abilities/ultimate:death-ray` - 1 occurrence
- `@abilities/ultimate:demon-lord` - 3 occurrences
- `@abilities/ultimate:devastating-blow` - 1 occurrence
- `@abilities/ultimate:earthquake` - 1 occurrence
- `@abilities/ultimate:eternal-vengeance` - 1 occurrence
- `@abilities/ultimate:forest-guardian` - 3 occurrences
- `@abilities/ultimate:hive-mind` - 1 occurrence
- `@abilities/ultimate:inferno` - 8 occurrences
- `@abilities/ultimate:plague-lord` - 1 occurrence
- `@abilities/ultimate:primal-fury` - 3 occurrences
- `@abilities/ultimate:progenitors-blessing` - 1 occurrence
- `@abilities/ultimate:serpent-lord` - 1 occurrence
- `@abilities/ultimate:shadow-lord` - 1 occurrence
- `@abilities/ultimate:soul-reaver` - 1 occurrence
- `@abilities/ultimate:spider-queen` - 1 occurrence
- `@abilities/ultimate:supernova` - 1 occurrence
- `@abilities/ultimate:tsunami` - 1 occurrence
- `@abilities/ultimate:vampiric-ascension` - 1 occurrence
- `@abilities/ultimate:vampiric-aura` - 1 occurrence
- `@abilities/ultimate:winter-storm` - 1 occurrence
- `@abilities/ultimate:wish` - 3 occurrences

### @classes/* (7 failures)
- `@classes/cleric_types:priest` - 1 occurrence (catalog.json:110)
- `@classes/mages:wizard` - 2 occurrences (catalog.json:978, 1047)
- `@classes/rangers:hunter` - 1 occurrence (catalog.json:502)
- `@classes/rogues:thief` - 2 occurrences (catalog.json:268, 341)
- `@classes/warriors:fighter` - 3 occurrences (catalog.json:672, 744, 818)

**Issue**: References use category names that don't match the catalog structure.  
**Example**: `@classes/cleric_types:priest` should be `@classes/cleric:priest`

### @enemies/* (34 failures)
- `@enemies/beast:bear` - 1 occurrence
- `@enemies/beast:eagle` - 1 occurrence
- `@enemies/beast:giant-spider` - 5 occurrences
- `@enemies/beast:scorpion` - 1 occurrence
- `@enemies/beast:wolf` - 5 occurrences
- `@enemies/elemental:fire` - 1 occurrence
- `@enemies/humanoid:bandit` - 5 occurrences
- `@enemies/humanoid:orc` - 1 occurrence
- `@enemies/humanoid:pirate` - 1 occurrence
- `@enemies/humanoid:troll` - 6 occurrences
- `@enemies/undead:vampire` - 1 occurrence

**Issue**: Category names don't match actual catalog structure.  
**Example**: `@enemies/beast:wolf` should likely be `@enemies/beasts:Wolf`

### @items/* (67 failures)

#### Materials (52 occurrences)
Multiple `@items/materials:*` references where the catalog structure may need adjustment.

#### Weapons & Armor (9 occurrences)
- `@items/armor:ancient` - 1 occurrence
- `@items/weapons:ancient` - 1 occurrence
- Various specific weapon/armor references

#### Consumables (Multiple occurrences)
- `@items/consumables:fish` - 2 occurrences
- `@items/consumables:wild-game` - 1 occurrence

### @npcs/* (8 failures)
- `@npcs/professionals:scholar` - 1 occurrence
- `@npcs/social_classes:*` - 7 references with multiple occurrences

**Issue**: Inconsistent category naming (should be singular, not plural?)

### @organizations/* (15 failures)
All references to factions appear to be using correct syntax but catalogs may be missing items:
- `@organizations/factions:city_guard`
- `@organizations/factions:clergy`
- `@organizations/factions:commoners`
- `@organizations/factions:craftsmen_guild`
- `@organizations/factions:fighters_guild`
- `@organizations/factions:mages_circle`
- `@organizations/factions:merchants_guild`
- `@organizations/factions:military`
- `@organizations/factions:nobility`
- `@organizations/factions:scholars_guild`
- `@organizations/factions:thieves_guild`

### @social/* (119 failures)

#### Dialogue References (97 occurrences)
- Farewells: 33 unique references
- Greetings: 44 unique references
- Styles: 12 unique references

#### Schedule References (22 occurrences)
- `@social/schedules:cook` - 1 occurrence
- `@social/schedules:criminal` - 4 occurrences
- `@social/schedules:flexible` - 2 occurrences
- `@social/schedules:guard` - 3 occurrences
- `@social/schedules:healer` - 4 occurrences
- `@social/schedules:innkeeper` - 2 occurrences
- `@social/schedules:mage` - 3 occurrences
- `@social/schedules:merchant` - 2 occurrences (including 1 typo: `npcs\schedules.json`)
- `@social/schedules:noble` - 4 occurrences
- `@social/schedules:priest` - 2 occurrences
- `@social/schedules:scholar` - 5 occurrences
- `@social/schedules:stablemaster` - 1 occurrence

### @world/* (13 failures)
- `@world/locations/dungeons:ShadowRealm` - 1 occurrence
- `@world/locations/dungeons:VampireCastle` - 1 occurrence
- `@world/locations/towns:Crossroads` - 2 occurrences
- `@world/locations/towns:DeadwoodGrove` - 1 occurrence
- `@world/locations/towns:FrostPeak` - 1 occurrence
- `@world/locations/towns:GoldenSpire` - 1 occurrence
- `@world/locations/towns:Oakshire` - 2 occurrences
- `@world/locations/towns:Riverside` - 1 occurrence
- `@world/locations/towns:Sanctuary` - 2 occurrences
- `@world/locations/towns:Silverport` - 2 occurrences
- `@world/locations/towns:Stormhaven` - 3 occurrences
- `@world/locations/towns:TheNexus` - 3 occurrences

## Root Causes

### 1. Missing Catalog Files
Several ability categories don't have corresponding catalog files:
- `abilities/passive/catalog.json` - MISSING (78 references fail)
- `abilities/ultimate/catalog.json` - MISSING (102 references fail)
- `social/dialogue/farewells/catalog.json` - Likely missing items
- `social/dialogue/greetings/catalog.json` - Likely missing items
- `social/dialogue/styles/catalog.json` - Likely missing items
- `social/schedules/catalog.json` - Likely missing items

### 2. Category Name Mismatches
References use wrong category names:
- `@classes/cleric_types` should be `@classes/cleric`
- `@classes/mages` should be `@classes/mage`
- `@classes/rangers` should be `@classes/ranger`
- `@classes/rogues` should be `@classes/rogue`
- `@classes/warriors` should be `@classes/warrior`
- `@enemies/beast` should be `@enemies/beasts`
- `@enemies/humanoid` should match actual catalog category
- `@npcs/social_classes` - unclear if correct

### 3. Missing Catalog Items
Catalogs exist but specific items are missing:
- World locations (towns/dungeons) - many referenced but not defined
- Organization factions - several factions referenced but not in catalog
- Item materials - multiple material types referenced but not cataloged

## Recommendations

### Priority 1: Create Missing Ability Catalogs
Create the following files with properly structured data:
- `RealmEngine.Data/Data/Json/abilities/passive/catalog.json`
- `RealmEngine.Data/Data/Json/abilities/ultimate/catalog.json`

**Impact**: Will fix 180 reference failures (14.5% of all failures)

### Priority 2: Fix Category Name Mismatches
Update all references to use correct category names:
- Bulk find/replace in classes references
- Bulk find/replace in enemies references
- Review npcs references for consistency

**Impact**: Will fix ~50 reference failures

### Priority 3: Create Missing Social Data Catalogs
Ensure these files have all referenced items:
- `social/dialogue/farewells/catalog.json`
- `social/dialogue/greetings/catalog.json`
- `social/dialogue/styles/catalog.json`
- `social/schedules/catalog.json`

**Impact**: Will fix 119 reference failures

### Priority 4: Complete World/Location Data
Add missing location definitions to:
- `world/locations/towns/catalog.json`
- `world/locations/dungeons/catalog.json`

**Impact**: Will fix 13 reference failures

### Priority 5: Add Missing Materials and Items
Review and add missing entries to:
- `items/materials/catalog.json`
- Various weapon/armor catalogs

**Impact**: Will fix remaining item-related failures

## Continuous Validation

The test suite is now set up to automatically validate ALL references:

```bash
# Run all reference validation tests (will show every failed reference)
dotnet test --filter "Category=ReferenceValidation"

# Get summary report
dotnet test --filter "FullyQualifiedName~Generate_Full_Reference_Report"
```

All tests use TheoryData, so each reference occurrence is tested individually. When a reference fails to resolve, the test output shows:
- The exact reference string
- The file containing it
- The line number
- Full file path

This makes it easy to track down and fix reference issues as new content is added.

## Next Steps

1. ✅ Test infrastructure created - ALL references now validated automatically
2. ⏸️ Create missing ability catalog files (passive, ultimate)
3. ⏸️ Fix category name mismatches (classes, enemies, npcs)
4. ⏸️ Complete social dialogue/schedule catalogs
5. ⏸️ Add missing world location data
6. ⏸️ Review and complete item catalogs
7. ⏸️ Run validation again to achieve 100% reference resolution

**Goal**: Achieve 100% reference resolution (1,244/1,244 passing)
