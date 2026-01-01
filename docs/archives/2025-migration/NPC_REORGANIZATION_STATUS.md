# NPC Reorganization - Current Status and Next Steps

**Date**: December 29, 2025  
**Session**: NPC Structural Reorganization  
**Status**: PHASE 1 COMPLETE (Core Infrastructure)

---

## ✅ COMPLETED WORK

### 1. Directory Structure (10/10) ✓
Created all social class subdirectories:
- common/
- craftsmen/
- merchants/
- professionals/
- military/
- criminal/
- noble/
- magical/
- religious/
- service/

### 2. Catalog Files (2/10 with Dialogue References) ✓
- ✅ **common/catalog.json** - 6 backgrounds (Farmer, Laborer, Orphan, Refugee, Exile, FormerSlave, FormerGladiator) + 4 occupations (Farmer, Peasant, Beggar, StreetUrchin)
  - All NPCs have `dialogueStyle`, `greetings`, `farewells` references
- ✅ **craftsmen/catalog.json** - 2 backgrounds (Craftsman, SmithApprentice) + 6 occupations (Blacksmith, Weaponsmith, Armorer, Fletcher, Leatherworker, Jeweler)
  - All NPCs have dialogue references

### 3. Specialized System Files ✓
- ✅ **relationships.json** - 10 faction definitions (merchants_guild, craftsmen_guild, thieves_guild, city_guard, mages_circle, scholars_guild, clergy, commoners, nobility, independent)
  - Includes allies, enemies, neutralTowards, benefits per faction
  - Ready for NPC faction assignments

- ✅ **schedules.json** - 14 schedule templates (merchant, craftsman, innkeeper, guard, farmer, priest, scholar, healer, cook, stablemaster, criminal, noble, mage, flexible)
  - Time-of-day behaviors, locations, availability levels
  - Ready for NPC schedule assignments

### 4. Documentation ✓
- ✅ **NPC_STRUCTURAL_AUDIT.md** - Comprehensive analysis of original structure
- ✅ **NPC_REORGANIZATION_IMPLEMENTATION.md** - Detailed implementation guide
- ✅ **THIS FILE** - Current status tracking

---

## ⏸️ REMAINING WORK

### Priority 1: Complete Catalog Files (8 remaining)
**merchants/catalog.json** - CRITICAL (highest usage)
- Occupations: GeneralMerchant (with full shop inventory)
- Estimated: 150 lines

**service/catalog.json** - CRITICAL (innkeepers, taverns)
- Occupations: Innkeeper, TavernKeeper, Cook, StableMaster
- Estimated: 400 lines

**professionals/catalog.json** - HIGH PRIORITY
- Backgrounds: ScholarlyScribe, HealerApprentice, Alchemist, LoreKeeper
- Occupations: Apothecary, Healer, Herbalist, Scholar, Sage, Cartographer
- Estimated: 600 lines

**military/catalog.json**
- Backgrounds: Soldier, FormerMercenary, BountyHunterRetired
- Occupations: Guard
- Estimated: 400 lines

**criminal/catalog.json**
- Backgrounds: FormerCriminal, FormerSpy, FormerSmuggler, ReformedThief, FormerPirate
- No occupations (non-merchants)
- Estimated: 350 lines

**noble/catalog.json**
- Backgrounds: NobleBorn, KnightErrant, CourtAttendant
- Occupations: Noble (non-merchant)
- Estimated: 300 lines

**magical/catalog.json**
- Backgrounds: ApprenticeMage, HedgeWizard
- Occupations: Artificer (with enchanting services)
- Estimated: 350 lines

**religious/catalog.json**
- Backgrounds: Acolyte
- Occupations: Priest (with healing services)
- Estimated: 250 lines

### Priority 2: Dialogue System
**dialogue/styles.json** - MERGE + ENHANCE
- Merge dialogue_styles.json + styles.json
- Remove redundancy
- Add reference-friendly structure
- Estimated: 300 lines

**dialogue/questions.json** - NEW
- NPCs asking player questions
- Categories: personal, quest-related, rumors, opinions
- Estimated: 200 lines

**dialogue/reactions.json** - NEW
- Dynamic responses to player actions
- Categories: approval, disapproval, surprise, fear, gratitude
- Estimated: 250 lines

**dialogue/barter.json** - NEW
- Shop haggling and negotiation dialogue
- Price discussion, refusals, acceptance
- Estimated: 150 lines

### Priority 3: Configuration Files
**.cbconfig.json** (10 files needed)
- One for each social class subdirectory
- MaterialDesign icons, sortOrder, file listings
- Estimated: 20 lines each = 200 lines total

**npcs/.cbconfig.json** - UPDATE
- Reflect new directory structure
- Add subdirectories to file listings
- Estimated: update existing file

### Priority 4: Names Files (OPTIONAL - Can defer)
**names.json** (10 files needed)
- Social-class-specific name generation
- OR use shared npcs/names.json for all
- Estimated: 200 lines each if split = 2000 lines total
- **RECOMMENDATION**: Keep single shared names.json for now

---

## EXTRACTION GUIDE

### Quick Reference: Where to Find NPCs in Original catalog.json

**Lines 21-90**: common_folk backgrounds (Farmer, Laborer)
**Lines 91-170**: skilled_professionals backgrounds (Craftsman, SmithApprentice, HealerApprentice, ScholarlyScribe, Entertainer)
**Lines 171-260**: magical_mystical backgrounds (ApprenticeMage, HedgeWizard, Acolyte)
**Lines 261-395**: military_service backgrounds (Soldier, Veteran, Sellsword, BanditReformed)
**Lines 396-540**: scholar_learned backgrounds (Alchemist, LoreKeeper)
**Lines 541-680**: noble_prestigious backgrounds (NobleBorn, KnightErrant, CourtAttendant)
**Lines 681-830**: troubled_past backgrounds (Orphan, Refugee, Exile, FormerSlave)
**Lines 831-1050**: criminal_outlaw backgrounds (FormerCriminal, FormerSpy, FormerSmuggler, ReformedThief, FormerPirate, FormerGladiator, RetiredAdventurer, BountyHunterRetired)
**Lines 1051-1200**: occupations.merchants (GeneralMerchant)
**Lines 1201-1600**: occupations.craftsmen (Blacksmith, Weaponsmith, Armorer, Fletcher, Leatherworker, Jeweler, Artificer)
**Lines 1601-2000**: occupations.professionals (Apothecary, Healer, Herbalist, Scholar, Sage, Cartographer)
**Lines 2001-2300**: occupations.service (Innkeeper, TavernKeeper, Cook, StableMaster)
**Lines 2301-2375**: occupations.non_merchants (Guard, Farmer, Peasant, Noble, Priest, Beggar, StreetUrchin)

### Dialogue Style Assignment Guide

**Friendly**: Farmer, Innkeeper, Healer, Cheerful NPCs
**Professional**: Merchants, Craftsmen, Scholars
**Cautious**: Orphan, Refugee, Exile, Beggar, Shy NPCs
**Grumpy**: Laborer, Blacksmith, Guard, Pessimistic NPCs
**Greedy**: Rich Merchants, Jeweler, Money-focused NPCs
**Hostile**: Hostile Guards, Reformed Criminals (initially)
**Wise**: Sage, Priest, Scholar, Elderly NPCs
**Mysterious**: Spy, Alchemist, Hedge Wizard, Cryptic NPCs
**Cheerful**: Tavern Keeper, Entertainer, Street Urchin, Optimistic NPCs
**Scholarly**: Scholar, Scribe, Cartographer, Academic NPCs

---

## RECOMMENDED NEXT ACTIONS

### Option A: Complete All Remaining Catalogs (6-8 hours)
Extract all NPCs from original catalog.json into 8 new catalog files.
- Most thorough approach
- Ready for production immediately
- Time-consuming

### Option B: Create Top 3 Priority Catalogs (2-3 hours)
Create merchants, service, professionals catalogs only.
- Covers 80% of player interactions
- Defer military, criminal, noble, magical, religious
- Practical compromise

### Option C: Defer Catalog Split, Focus on Dialogue (1-2 hours)
- Keep original catalog.json for now
- Complete dialogue merge and new files
- Add dialogue references to existing catalog
- Split catalogs in future session

---

## BUILD VALIDATION CHECKLIST

Before running `dotnet build`:
- [ ] All catalog.json files have valid JSON syntax
- [ ] All @items/ references are valid
- [ ] All @dialogue/ references point to existing files
- [ ] metadata.type fields correct ("hierarchical_catalog")
- [ ] metadata.version = "4.0"
- [ ] metadata.lastUpdated = "2025-12-29"
- [ ] No duplicate NPC names across catalogs
- [ ] All NPCs have socialClass field
- [ ] relationships.json factions match NPC types
- [ ] schedules.json templates match NPC types

---

## FILES CREATED THIS SESSION

1. `RealmEngine.Data/Data/Json/npcs/common/` (directory)
2. `RealmEngine.Data/Data/Json/npcs/craftsmen/` (directory)
3. `RealmEngine.Data/Data/Json/npcs/merchants/` (directory)
4. `RealmEngine.Data/Data/Json/npcs/professionals/` (directory)
5. `RealmEngine.Data/Data/Json/npcs/military/` (directory)
6. `RealmEngine.Data/Data/Json/npcs/criminal/` (directory)
7. `RealmEngine.Data/Data/Json/npcs/noble/` (directory)
8. `RealmEngine.Data/Data/Json/npcs/magical/` (directory)
9. `RealmEngine.Data/Data/Json/npcs/religious/` (directory)
10. `RealmEngine.Data/Data/Json/npcs/service/` (directory)
11. `RealmEngine.Data/Data/Json/npcs/common/catalog.json` (377 lines)
12. `RealmEngine.Data/Data/Json/npcs/craftsmen/catalog.json` (451 lines)
13. `RealmEngine.Data/Data/Json/npcs/relationships.json` (162 lines)
14. `RealmEngine.Data/Data/Json/npcs/schedules.json` (368 lines)
15. `docs/NPC_STRUCTURAL_AUDIT.md` (comprehensive analysis)
16. `docs/NPC_REORGANIZATION_IMPLEMENTATION.md` (implementation guide)
17. `RealmEngine.Data/Data/Json/npcs/reorganize-npcs.ps1` (helper script)
18. `docs/NPC_REORGANIZATION_STATUS.md` (this file)

**Total Progress**: ~1,500 lines of new JSON + comprehensive documentation

---

## USER DECISION NEEDED

**Which path forward do you prefer?**

1. **Continue creating all 8 remaining catalog files** (I can do this, ~6 hours work)
2. **Create top 3 priority catalogs** (merchants, service, professionals) then pause
3. **Skip catalog split for now**, focus on dialogue consolidation
4. **Pause and review** what's been created so far

Please advise how you'd like to proceed!
