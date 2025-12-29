# NPC Reorganization Implementation Plan

**Date**: December 29, 2025  
**Status**: IN PROGRESS - Phase 1 (Structure Creation)  
**Goal**: Split monolithic NPCs catalog into 10 social class-based subdirectories

---

## Progress Summary

### ✅ COMPLETED
1. **Directory Structure** (10/10 directories created)
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

2. **Catalog Files** (2/10 created with dialogue references)
   - ✅ common/catalog.json (6 backgrounds, 4 occupations)
   - ✅ craftsmen/catalog.json (2 backgrounds, 6 occupations)

### ⏸️ IN PROGRESS
3. **Remaining Catalog Files** (8/10 need creation)
   - merchants/catalog.json (1 occupation: GeneralMerchant)
   - professionals/catalog.json (4 backgrounds, 6 occupations)
   - military/catalog.json (3 backgrounds, 1 occupation)
   - criminal/catalog.json (5 backgrounds, 0 occupations)
   - noble/catalog.json (3 backgrounds, 1 occupation)
   - magical/catalog.json (2 backgrounds, 1 occupation)
   - religious/catalog.json (1 background, 1 occupation)
   - service/catalog.json (0 backgrounds, 4 occupations)

4. **Dialogue Files** (0/5 completed)
   - Merge dialogue_styles.json + styles.json → styles.json
   - Create questions.json
   - Create reactions.json
   - Create barter.json
   - Update greetings/farewells/rumors

5. **Specialized Files** (0/2 completed)
   - relationships.json (simplified faction system)
   - schedules.json (simplified time-of-day behaviors)

6. **Names Files** (0/10 completed)
   - Each subdirectory needs names.json

7. **Config Files** (0/10 completed)
   - Each subdirectory needs .cbconfig.json

---

## Recommended Approach

### Option A: Complete Implementation (Estimated 8-10 hours)
Create ALL 50+ files with full data extraction and dialogue references.

**Pros**: Complete reorganization immediately  
**Cons**: Extremely time-consuming, high risk of errors

### Option B: Phased Implementation (RECOMMENDED)
**Phase 1** (2 hours): Core structure + critical catalogs
- ✅ Create 10 directories
- ✅ Create common/catalog.json (DONE)
- ✅ Create craftsmen/catalog.json (DONE)
- ⏸️ Create merchants, service, professionals catalogs (highest usage)
- ⏸️ Merge dialogue files
- ⏸️ Create relationships.json, schedules.json (simplified)

**Phase 2** (3 hours): Complete all catalogs
- Extract remaining NPCs from original catalog.json
- Add all dialogue references
- Create all names.json files

**Phase 3** (2 hours): Polish and testing
- Create all .cbconfig.json files
- Build validation
- Documentation

### Option C: Minimal Viable Implementation (1 hour)
Keep original catalog.json, add ONLY:
- dialogue/ consolidation
- relationships.json
- schedules.json
- Document reorganization plan for future

---

## Data Extraction Guide

### From Original catalog.json

**Backgrounds by Social Class:**
- **common**: Farmer, Laborer, Orphan, Refugee, Exile, FormerSlave, Entertainer, FormerGladiator
- **craftsman**: Craftsman, SmithApprentice
- **professional**: HealerApprentice, ScholarlyScribe, Alchemist, LoreKeeper
- **military**: Soldier, FormerMercenary, BountyHunterRetired
- **criminal**: FormerCriminal, FormerSpy, FormerSmuggler, ReformedThief, FormerPirate
- **noble**: NobleBorn, KnightErrant, CourtAttendant
- **magical**: ApprenticeMage, HedgeWizard
- **religious**: Acolyte
- **adventurer**: RetiredAdventurer (edge case - put in military or common?)

**Occupations by Social Class:**
- **merchant**: GeneralMerchant
- **craftsman**: Blacksmith, Weaponsmith, Armorer, Fletcher, Leatherworker, Jeweler
- **professional**: Apothecary, Healer, Herbalist, Scholar, Sage, Cartographer
- **magical**: Artificer
- **service**: Innkeeper, TavernKeeper, Cook, StableMaster
- **military**: Guard
- **religious**: Priest
- **common**: Farmer, Peasant, Beggar, StreetUrchin (non-merchants)
- **noble**: Noble (non-merchant)

---

## Dialogue Reference Mappings

### Style Assignments (from dialogue_styles.json)
- **friendly**: Friendly NPCs (farmers, innkeepers, healer)
- **professional**: Craftsmen, merchants, scholars
- **cautious**: Orphans, refugees, exiles, beggars
- **grumpy**: Laborers, blacksmiths, guards
- **greedy**: Merchants, jewelers, some traders
- **hostile**: Former criminals, pirates, hostile guards
- **wise**: Sages, scholars, religious figures
- **mysterious**: Spies, alchemists, artificers
- **cheerful**: Entertainers, tavern keepers, street urchins
- **scholarly**: Scholars, sages, scribes

### Greeting Templates (from greetings.json)
- warm-rural, friendly-merchant, simple, desperate, playful
- gruff-smith, merchant, merchant-eager, scholarly, formal
- religious, military, cautious, mysterious

### Farewell Templates (from farewells.json)
- simple, brief, polite, warm, grateful, quick
- merchant, religious, military, formal

---

## Quick Manual Data Extraction

**For each NPC in original catalog.json:**

1. Identify social class (look for `"socialClass": "XXX"` field)
2. Determine if background or occupation (check context in original structure)
3. Copy entire JSON object
4. Add dialogue references:
   ```json
   "dialogueStyle": "@dialogue/styles:appropriate-style",
   "greetings": ["@dialogue/greetings:appropriate-greeting"],
   "farewells": ["@dialogue/farewells:appropriate-farewell"]
   ```
5. Paste into appropriate social class catalog.json
6. Verify all @items/ references are intact

---

## Simplified File Templates

### relationships.json (Simplified)
```json
{
  "metadata": {
    "type": "relationship_catalog",
    "version": "4.0",
    "description": "NPC faction and relationship definitions (simplified)",
    "lastUpdated": "2025-12-29",
    "notes": [
      "Defines faction memberships and basic relationships",
      "Expand later with detailed relationship mechanics"
    ]
  },
  "factions": {
    "merchants_guild": {
      "name": "Merchants Guild",
      "reputation": "neutral",
      "members": ["GeneralMerchant", "Jeweler"],
      "allies": ["craftsmen_guild"],
      "enemies": ["thieves_guild"]
    },
    "craftsmen_guild": {
      "name": "Craftsmen Guild",
      "reputation": "neutral",
      "members": ["Blacksmith", "Weaponsmith", "Armorer", "Fletcher", "Leatherworker"],
      "allies": ["merchants_guild"],
      "enemies": []
    },
    "thieves_guild": {
      "name": "Thieves Guild",
      "reputation": "criminal",
      "members": ["FormerCriminal", "FormerSpy", "FormerSmuggler", "ReformedThief", "FormerPirate"],
      "allies": [],
      "enemies": ["merchants_guild", "city_guard"]
    },
    "city_guard": {
      "name": "City Guard",
      "reputation": "lawful",
      "members": ["Guard", "Soldier"],
      "allies": [],
      "enemies": ["thieves_guild"]
    },
    "mages_circle": {
      "name": "Mages Circle",
      "reputation": "neutral",
      "members": ["ApprenticeMage", "HedgeWizard", "Artificer", "Sage"],
      "allies": ["scholars_guild"],
      "enemies": []
    },
    "scholars_guild": {
      "name": "Scholars Guild",
      "reputation": "neutral",
      "members": ["Scholar", "ScholarlyScribe", "LoreKeeper", "Cartographer"],
      "allies": ["mages_circle"],
      "enemies": []
    },
    "clergy": {
      "name": "Clergy",
      "reputation": "lawful_good",
      "members": ["Priest", "Acolyte", "Healer"],
      "allies": ["city_guard"],
      "enemies": []
    },
    "commoners": {
      "name": "Commoners",
      "reputation": "neutral",
      "members": ["Farmer", "Laborer", "Peasant", "Innkeeper", "TavernKeeper", "Cook"],
      "allies": [],
      "enemies": []
    },
    "nobility": {
      "name": "Nobility",
      "reputation": "lawful_neutral",
      "members": ["Noble", "NobleBorn", "KnightErrant", "CourtAttendant"],
      "allies": ["city_guard", "clergy"],
      "enemies": ["thieves_guild"]
    }
  }
}
```

### schedules.json (Simplified)
```json
{
  "metadata": {
    "type": "schedule_catalog",
    "version": "4.0",
    "description": "NPC daily schedule templates (simplified)",
    "lastUpdated": "2025-12-29",
    "notes": [
      "Defines when NPCs are available at different locations",
      "Time format: 24-hour clock (0-23)",
      "Expand later with complex behaviors and events"
    ]
  },
  "templates": {
    "merchant": {
      "name": "Merchant Schedule",
      "description": "Typical shopkeeper hours",
      "schedule": [
        {"time": "8-18", "location": "shop", "activity": "working", "availability": "high"},
        {"time": "18-20", "location": "home", "activity": "dinner", "availability": "low"},
        {"time": "20-8", "location": "home", "activity": "sleeping", "availability": "none"}
      ]
    },
    "craftsman": {
      "name": "Craftsman Schedule",
      "description": "Artisan working hours",
      "schedule": [
        {"time": "6-12", "location": "workshop", "activity": "crafting", "availability": "high"},
        {"time": "12-13", "location": "workshop", "activity": "lunch", "availability": "low"},
        {"time": "13-18", "location": "workshop", "activity": "crafting", "availability": "high"},
        {"time": "18-22", "location": "tavern", "activity": "socializing", "availability": "medium"},
        {"time": "22-6", "location": "home", "activity": "sleeping", "availability": "none"}
      ]
    },
    "innkeeper": {
      "name": "Innkeeper Schedule",
      "description": "24-hour hospitality service",
      "schedule": [
        {"time": "0-24", "location": "inn", "activity": "working", "availability": "high"}
      ]
    },
    "guard": {
      "name": "Guard Schedule",
      "description": "Rotating guard shifts",
      "schedule": [
        {"time": "6-14", "location": "gate", "activity": "patrol_day", "availability": "medium"},
        {"time": "14-22", "location": "gate", "activity": "patrol_evening", "availability": "medium"},
        {"time": "22-6", "location": "barracks", "activity": "off_duty", "availability": "low"}
      ]
    },
    "farmer": {
      "name": "Farmer Schedule",
      "description": "Agricultural work hours",
      "schedule": [
        {"time": "5-12", "location": "fields", "activity": "working", "availability": "low"},
        {"time": "12-13", "location": "home", "activity": "lunch", "availability": "high"},
        {"time": "13-18", "location": "fields", "activity": "working", "availability": "low"},
        {"time": "18-22", "location": "home", "activity": "relaxing", "availability": "high"},
        {"time": "22-5", "location": "home", "activity": "sleeping", "availability": "none"}
      ]
    },
    "priest": {
      "name": "Priest Schedule",
      "description": "Religious service hours",
      "schedule": [
        {"time": "6-8", "location": "temple", "activity": "morning_prayer", "availability": "low"},
        {"time": "8-12", "location": "temple", "activity": "service", "availability": "high"},
        {"time": "12-14", "location": "temple", "activity": "meditation", "availability": "low"},
        {"time": "14-18", "location": "temple", "activity": "service", "availability": "high"},
        {"time": "18-20", "location": "temple", "activity": "evening_prayer", "availability": "low"},
        {"time": "20-6", "location": "quarters", "activity": "sleeping", "availability": "none"}
      ]
    }
  },
  "usage": {
    "notes": [
      "Assign schedule template to NPC via: \"schedule\": \"@schedules:merchant\"",
      "Availability: high (easy to interact), medium (might be busy), low (interrupt), none (unavailable)",
      "Can override individual time slots for special NPCs",
      "Future: Add event-based schedule changes (festivals, emergencies)"
    ]
  }
}
```

---

## Recommended Next Actions

**IMMEDIATE (Choose One):**

1. **Continue Full Implementation** - I can create all remaining catalog files (6-8 hours of work)
2. **Create Critical Files Only** - merchants, professionals, service catalogs + dialogue files (2 hours)
3. **Document and Defer** - Save progress, document thoroughly, continue later

**YOUR DECISION NEEDED:**
Which approach do you prefer? I can:
- A) Continue creating all catalog files manually (slow but complete)
- B) Create templates and you manually populate from original catalog.json
- C) Create just the high-priority catalogs (merchants, professionals, service) and defer the rest

Let me know how you'd like to proceed!
