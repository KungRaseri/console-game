# NPC Structural Organization Audit

**Date**: December 29, 2025  
**Scope**: Comprehensive audit of NPC file organization and structure  
**Status**: v4.0/v4.1 COMPLIANT - Evaluating Organizational Patterns

---

## Executive Summary

**Current State**: NPCs use a **FLAT + 1 FUNCTIONAL SUBDIR** structure with 10 files totaling ~4,700 lines across 2 directories.

**Key Findings**:
- ✅ **Standards Compliance**: 10/10 files comply with JSON v4.0/v4.1 standards
- ⚠️ **File Size**: Single `catalog.json` at 2,375 lines (largest single catalog in codebase)
- ⚠️ **Organization**: Flat structure differs from enemies (13 subdirs) and items (5 subdirs)
- ✅ **File Types**: Follows established patterns (catalog, names, traits, dialogue)
- ⚠️ **Scalability**: Limited room for growth without restructuring

**Recommendation**: **Evaluate organizational options** before adding more NPC types.

---

## 1. Current NPC File Structure

### Directory Tree
```
npcs/
├── .cbconfig.json               # ContentBuilder config (17 lines)
├── catalog.json                 # ⚠️ LARGEST: 2,375 lines (backgrounds + occupations)
├── names.json                   # 1,295 lines (pattern generation)
├── traits.json                  # 735 lines (personality traits + quirks)
└── dialogue/
    ├── .cbconfig.json           # ContentBuilder config
    ├── dialogue_styles.json     # 254 lines (10 personalities)
    ├── greetings.json           # 260 lines (25 greetings)
    ├── farewells.json           # 287 lines (25 farewells)
    ├── rumors.json              # 248 lines (30 rumors)
    └── styles.json              # 86 lines (quick templates)
```

### File Count Breakdown
- **Core Data Files**: 3 (catalog, names, traits)
- **Functional Subdir**: 1 (dialogue/)
- **Dialogue Files**: 5 (styles, greetings, farewells, rumors, templates)
- **Config Files**: 2 (.cbconfig.json)
- **TOTAL**: 10 files in 2 directories

### Line Count Analysis
- **catalog.json**: 2,375 lines (51% of all NPC data)
- **names.json**: 1,295 lines (28%)
- **traits.json**: 735 lines (16%)
- **dialogue/**: ~1,135 lines (5 files combined)
- **TOTAL DATA**: ~4,605 lines across all files

---

## 2. Comparison with Other Domains

### Domain Organization Patterns

| Domain | Structure Type | Subdirectories | Catalog Files | Largest Catalog | Total Lines | Notes |
|--------|---------------|----------------|---------------|-----------------|-------------|-------|
| **NPCs** | **FLAT + 1 FUNC** | 1 (dialogue) | 1 (2,375 lines) | 2,375 | ~4,700 | Single monolithic catalog |
| **Enemies** | **HIERARCHICAL** | 13 (by type) | 13 (avg ~800) | ~1,100 | ~13,000 | Split by creature family |
| **Items** | **HIERARCHICAL** | 5 (by category) | 5 (varies) | ~1,500 | ~8,000 | Split by item category |
| **Classes** | **FLAT** | 0 | 1 (757 lines) | 757 | ~2,100 | Flat structure (smaller dataset) |
| **Quests** | **MIXED** | 4 (by function) | 1 + 9 special | 1,082 | ~3,500 | Functional subdirs |
| **Abilities** | **HIERARCHICAL** | 7 (by type) | 15 (varies) | ~600 | ~7,000 | Deep hierarchy (active/passive/reactive) |

### Key Observations

#### 1. **File Size Concern**
- ⚠️ **NPC catalog.json (2,375 lines)** is the **LARGEST single catalog** in the codebase
- Enemies' largest catalog: ~1,100 lines (dragons)
- Items' largest catalog: ~1,500 lines (weapons)
- Classes catalog: 757 lines (comparable domain)
- **NPCs exceed comparable domains by 3x in single-file size**

#### 2. **Organizational Patterns**
Three distinct patterns emerge:

**A. HIERARCHICAL (Enemies, Items, Abilities)**
- **Structure**: Multiple subdirectories by TYPE or CATEGORY
- **Catalogs**: One catalog per subdirectory
- **Pros**: Scalable, modular, easy to navigate, small file sizes
- **Cons**: More complex directory structure
- **Examples**:
  - `enemies/dragons/catalog.json` (12 dragon types)
  - `enemies/undead/catalog.json` (15 undead types)
  - `items/weapons/catalog.json` (30+ weapons)

**B. FLAT (Classes, NPCs)**
- **Structure**: All data in one catalog.json
- **Catalogs**: Single monolithic file
- **Pros**: Simple structure, everything in one place
- **Cons**: File size grows quickly, harder to maintain
- **Examples**:
  - `classes/catalog.json` (757 lines, 5 archetypes)
  - `npcs/catalog.json` (2,375 lines, 56 NPC types)

**C. MIXED (Quests)**
- **Structure**: Root catalog + functional subdirectories
- **Catalogs**: Main catalog + specialized files
- **Pros**: Flexible, separates concerns by function
- **Cons**: Can be confusing (which data goes where?)
- **Examples**:
  - `quests/catalog.json` (main templates + locations)
  - `quests/objectives/primary.json` (functional separation)
  - `quests/rewards/items.json` (functional separation)

#### 3. **File Type Standardization**

| File Type | Enemies | Items | NPCs | Classes | Quests | Abilities |
|-----------|---------|-------|------|---------|--------|-----------|
| **catalog.json** | ✅ (13x) | ✅ (5x) | ✅ (1x) | ✅ (1x) | ✅ (1x) | ✅ (15x) |
| **names.json** | ✅ (13x) | ✅ (5x) | ✅ (1x) | ✅ (1x) | ❌ | ✅ (15x) |
| **traits.json** | ❌ | ❌ | ✅ (1x) | ❌ | ❌ | ❌ |
| **.cbconfig.json** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Functional Subdirs** | ❌ | ❌ | ✅ (dialogue) | ❌ | ✅ (4 dirs) | ❌ |

**Unique to NPCs**:
- `traits.json` (personality traits + quirks) - **NO OTHER DOMAIN HAS THIS**
- `dialogue/` subdirectory with 5 specialized files - **UNIQUE FUNCTIONAL SEPARATION**

**Missing from NPCs**:
- No type-based subdirectories (unlike enemies/items)
- No specialized catalogs (unlike quests with objectives.json, rewards.json)

---

## 3. Standards Compliance Assessment

### ✅ JSON v4.0 Standards - PASSING

| Standard | Status | Details |
|----------|--------|---------|
| **Metadata Fields** | ✅ PASS | All files have `type`, `version`, `lastUpdated`, `description` |
| **Type Values** | ✅ PASS | `hierarchical_catalog` (catalog.json), `pattern_generation` (names.json) |
| **rarityWeight** | ✅ PASS | All items use `rarityWeight` (NOT "weight") |
| **lastUpdated** | ✅ PASS | All files use `lastUpdated` (NOT "lastModified") |
| **Component Structure** | ✅ PASS | catalog.json uses proper hierarchical structure |
| **No "example" Fields** | ✅ PASS | No banned "example" fields found |

### ✅ JSON v4.1 Reference System - PASSING

| Reference Type | Status | Count | Details |
|---------------|--------|-------|---------|
| **@items/** | ✅ COMPLETE | 31 refs | All shop inventories use item references |
| **@abilities/** | ⏸️ N/A | 0 refs | NPCs don't have combat abilities (not needed) |
| **@traits/** | ⏸️ N/A | 0 refs | Traits are self-contained (no external refs needed) |
| **@classes/** | ⏸️ N/A | 0 refs | NPCs don't reference player classes (not needed) |
| **@quests/** | ❓ FUTURE | 0 refs | Could reference quests given by NPCs (not implemented) |
| **@dialogue/** | ❓ INTERNAL | 0 refs | Dialogue is internal to NPCs (could be formalized) |

**Reference Coverage**: 100% of applicable references implemented

### ✅ File Type Standards - PASSING

| File Type | Expected Pattern | NPCs Implementation | Compliance |
|-----------|------------------|---------------------|------------|
| **catalog.json** | `type: "hierarchical_catalog"` | ✅ Correct | PASS |
| **names.json** | `type: "pattern_generation"` | ✅ Correct | PASS |
| **traits.json** | Custom type (no standard) | ✅ Uses custom structure | PASS |
| **.cbconfig.json** | ContentBuilder format | ✅ Correct | PASS |
| **dialogue files** | Custom JSON arrays | ✅ Consistent structure | PASS |

---

## 4. Gap Analysis

### Critical Questions

#### **Q1: Does the current flat structure scale well?**
- **Current**: 56 NPC types in 2,375 lines (single file)
- **Comparison**: 
  - Enemies: 100+ types split into 13 catalogs (~800 lines each)
  - Items: 50+ items split into 5 catalogs (~1,500 lines each)
- **Concern**: NPCs already exceed ideal file size (~1,000-1,500 lines per catalog)
- **Risk**: Adding 20-30 more NPC types = 3,500-4,000 line file (hard to maintain)

#### **Q2: Should NPCs be subdivided like enemies?**
**Option A: Split by Social Class** (like enemy types)
```
npcs/
├── common/        # common_folk (8 backgrounds)
├── craftsmen/     # blacksmiths, jewelers (6 occupations)
├── merchants/     # shopkeepers (8 occupations)
├── noble/         # aristocrats (3 backgrounds)
├── military/      # soldiers, guards (4 backgrounds)
├── criminal/      # thieves, smugglers (3 backgrounds)
├── magical/       # wizards, sages (4 backgrounds + occupations)
└── religious/     # priests, clerics (3 occupations)
```
- **Pros**: Matches social class taxonomy, clear separation
- **Cons**: Splits backgrounds and occupations into different files

**Option B: Split by Function** (backgrounds vs occupations)
```
npcs/
├── backgrounds/
│   ├── catalog.json  # 31 backgrounds
│   └── names.json
└── occupations/
    ├── catalog.json  # 25 occupations
    └── names.json
```
- **Pros**: Logical separation (backgrounds = past, occupations = present)
- **Cons**: Still large files (~1,200 lines each), doesn't solve scalability

**Option C: Hybrid Split** (social class + function)
```
npcs/
├── backgrounds/
│   ├── common/catalog.json
│   ├── noble/catalog.json
│   ├── military/catalog.json
│   └── ...
└── occupations/
    ├── merchants/catalog.json
    ├── craftsmen/catalog.json
    └── ...
```
- **Pros**: Maximum modularity, small files (~200-400 lines)
- **Cons**: Complex structure, many directories

**Option D: Keep Flat Structure** (status quo)
```
npcs/
├── catalog.json   # Accept 2,375+ lines
├── names.json
└── traits.json
```
- **Pros**: Simple, everything in one place
- **Cons**: File size concerns, harder to maintain as it grows

#### **Q3: Are there missing file types?**

**Currently Missing**:
1. **No relationships.json** - NPC relationships/faction affiliations
   - Enemies have faction data in catalogs
   - Items have enchantment relationships
   - **NPCs could benefit from faction/guild/family relationships**

2. **No locations.json** - NPC spawn locations/territories
   - Quests have `locations/` subdirectory
   - Enemies have habitat data in catalogs
   - **NPCs have no spawn location definitions**

3. **No schedules.json** - NPC daily schedules/behaviors
   - **Could define when NPCs are at shop/tavern/home**
   - **Time-of-day behavior patterns**

4. **No inventory_templates.json** - Standardized shop inventory templates
   - Currently defined inline in catalog.json
   - **Could be extracted for reuse across multiple NPCs**

5. **No reputation.json** - NPC reputation/faction standing
   - **Could define how NPCs react to player reputation**
   - **Faction relationships and allegiances**

6. **No quests.json** - Quests given by specific NPCs
   - Quests domain has templates but no NPC linkage
   - **Could formalize which NPCs give which quests**

#### **Q4: Is `dialogue/` subdirectory correctly organized?**

**Current Structure**:
```
dialogue/
├── dialogue_styles.json  # 10 personality types
├── greetings.json        # 25 greeting templates
├── farewells.json        # 25 farewell templates
├── rumors.json           # 30 rumor templates
└── styles.json           # Quick dialogue templates
```

**Analysis**:
- ✅ **Good**: Functional separation from core NPC data
- ✅ **Good**: Small, manageable files (86-287 lines each)
- ⚠️ **Concern**: `dialogue_styles.json` vs `styles.json` naming confusion
- ⚠️ **Missing**: No `questions.json` (NPC asking player questions)
- ⚠️ **Missing**: No `reactions.json` (NPC reactions to player actions)
- ⚠️ **Missing**: No `barter.json` (shop haggling dialogue)

**Recommendation**: Keep `dialogue/` separate but consider consolidation/expansion:
- Merge `dialogue_styles.json` + `styles.json` → `styles.json`
- Add `questions.json` for interactive dialogue
- Add `reactions.json` for dynamic responses
- Add `barter.json` for shop interactions

#### **Q5: Should `traits.json` be expanded?**

**Current Scope**:
- 40 personality traits (traits[])
- 35 quirks (quirks[])
- Total: 735 lines

**Potential Expansions**:
1. **Add `fears.json`** - NPC fears/phobias (affects behavior)
2. **Add `goals.json`** - NPC motivations/objectives
3. **Add `secrets.json`** - Hidden NPC information (quest hooks)
4. **Split into subdirectory**:
   ```
   traits/
   ├── personalities.json  # Current traits[]
   ├── quirks.json         # Current quirks[]
   ├── fears.json          # NEW
   ├── goals.json          # NEW
   └── secrets.json        # NEW
   ```

**Question**: Is a single `traits.json` sufficient or should traits be expanded/subdivided?

#### **Q6: Are there consistency issues with other domains?**

**Inconsistencies Found**:

1. **Enemy catalogs use `"type": "item_catalog"`** (WRONG - should be `enemy_catalog`)
   - All 13 enemy catalogs incorrectly labeled
   - **Action**: File bug report or fix across enemies domain

2. **Quests use MIXED file organization** (catalog.json + specialized files)
   - Has both `quests/catalog.json` AND `quests/objectives.json`, `quests/rewards.json`
   - **NPCs could adopt similar pattern**: `npcs/catalog.json` + `npcs/shops.json` + `npcs/dialogue.json`

3. **Classes use FLAT structure** (similar to NPCs)
   - Classes: 757 lines for 5 archetypes
   - NPCs: 2,375 lines for 56 types
   - **Classes prove flat works for smaller datasets (~10-20 items)**
   - **NPCs prove flat struggles at scale (50+ items)**

---

## 5. Organizational Recommendations

### Immediate Actions (No Restructuring)

#### ✅ **Priority 1: Documentation**
- Document why NPCs use flat structure (design decision)
- Add comments to catalog.json explaining structure
- Update GDD with NPC organization rationale

#### ✅ **Priority 2: Dialogue Cleanup**
- **Merge `dialogue_styles.json` + `styles.json`** (naming confusion)
- Consider adding `questions.json`, `reactions.json`, `barter.json`
- Add `.cbconfig.json` metadata updates

#### ✅ **Priority 3: Monitor File Size**
- Set alert threshold: 3,000 lines = time to restructure
- Track NPC count growth in README.md
- Plan restructuring if hitting 80+ NPC types

### Medium-Term Options (Restructuring)

#### **Option A: Split by Function** (RECOMMENDED)
**Rationale**: Matches NPC design (backgrounds = past, occupations = current job)

**Structure**:
```
npcs/
├── backgrounds/
│   ├── catalog.json    # 31 backgrounds (~1,200 lines)
│   ├── names.json      # Background-specific names
│   └── .cbconfig.json
├── occupations/
│   ├── catalog.json    # 25 occupations (~1,100 lines)
│   ├── names.json      # Occupation-specific names
│   └── .cbconfig.json
├── traits.json         # Shared personality traits
└── dialogue/           # Shared dialogue templates
```

**Pros**:
- ✅ Matches NPC generation logic (pick background + occupation)
- ✅ Reduces file sizes to ~1,200 lines each (manageable)
- ✅ Simple 2-directory split (not complex)
- ✅ Clear separation of concerns

**Cons**:
- ⚠️ Requires refactoring generation code
- ⚠️ Shared traits/dialogue remain at root (not fully modular)

**Estimated Effort**: 4-6 hours (split files, update references, test generation)

#### **Option B: Split by Social Class** (ALTERNATE)
**Rationale**: Matches enemy organization pattern (by type)

**Structure**:
```
npcs/
├── common/
│   ├── catalog.json    # common_folk backgrounds + service occupations
│   └── names.json
├── craftsmen/
│   ├── catalog.json    # craftsman backgrounds + trade occupations
│   └── names.json
├── merchants/
│   ├── catalog.json    # merchant backgrounds + shop occupations
│   └── names.json
├── noble/
│   ├── catalog.json    # noble backgrounds + leadership occupations
│   └── names.json
└── ...
```

**Pros**:
- ✅ Consistent with enemies domain pattern
- ✅ Small files (~300-500 lines each)
- ✅ Scalable to 100+ NPC types

**Cons**:
- ⚠️ Mixes backgrounds + occupations (less clear separation)
- ⚠️ More directories (8+ subdirectories needed)
- ⚠️ Harder to generate NPCs (pick class first, then pick background/occupation)

**Estimated Effort**: 8-12 hours (complex split, multiple directories, logic changes)

#### **Option C: Keep Flat + Add Specialized Files** (MINIMAL)
**Rationale**: Don't fix what isn't broken yet

**Structure**:
```
npcs/
├── catalog.json        # Keep as-is (2,375 lines)
├── names.json
├── traits.json
├── relationships.json  # NEW: faction/guild affiliations
├── locations.json      # NEW: spawn locations
├── schedules.json      # NEW: daily behavior patterns
└── dialogue/
```

**Pros**:
- ✅ No restructuring needed (low risk)
- ✅ Adds missing functionality without breaking changes
- ✅ Simple to implement

**Cons**:
- ⚠️ Doesn't solve file size concern
- ⚠️ Catalog.json will continue to grow
- ⚠️ Future restructuring inevitable at 100+ NPCs

**Estimated Effort**: 2-4 hours per new file

### Long-Term Vision

**Goal**: NPCs should scale to 200+ types without performance issues

**Recommended Path**:
1. **Phase 1 (Now)**: Keep flat, add specialized files (relationships, locations, schedules)
2. **Phase 2 (50-80 NPCs)**: Split by function (backgrounds / occupations)
3. **Phase 3 (100+ NPCs)**: Split by social class (8+ subdirectories)

**Trigger Points**:
- 50 NPC types = Add specialized files
- 80 NPC types = Split by function
- 120 NPC types = Split by social class

---

## 6. Questions for Discussion

### Critical Decisions Needed

1. **File Size Tolerance**
   - Is 2,375 lines acceptable for a catalog.json?
   - What is the maximum file size before restructuring?
   - Should we set a hard limit (e.g., 3,000 lines)?

2. **Restructuring Timeline**
   - Should we restructure now (proactive) or wait until file size is critical?
   - How many more NPC types are planned?
   - What is the target NPC count (100? 200? 500?)?

3. **Organizational Philosophy**
   - Should NPCs match enemies pattern (by type/class)?
   - Or keep unique structure (flat is sufficient)?
   - Is consistency across domains more important than domain-specific optimization?

4. **Missing File Types**
   - Should we add relationships.json, locations.json, schedules.json?
   - Are these features needed now or deferred to future?
   - What is priority order for new features?

5. **Dialogue Organization**
   - Is `dialogue/` subdirectory correct or should it be at root level?
   - Should dialogue be split further (reactions, questions, barter)?
   - Should dialogue styles reference traits.json?

6. **Traits Expansion**
   - Should traits.json remain single file or split into traits/ subdirectory?
   - Add fears.json, goals.json, secrets.json?
   - How deep should NPC characterization go?

7. **Shop Economy**
   - Should shop inventory templates be extracted to shops/inventory_templates.json?
   - Should shop data be its own catalog (shops/catalog.json)?
   - How tightly coupled should NPCs + shops be?

### Feature Requests

1. **Add @quests/ References**
   - Link NPCs to quests they can give (questGiver field)
   - Example: `"questsGiven": ["@quests/main-story:rats-in-cellar"]`

2. **Add @locations/ References**
   - Define where NPCs spawn (spawnLocations field)
   - Example: `"spawnLocations": ["@locations/towns:village-square"]`

3. **Add @dialogue/ Internal References**
   - Formalize dialogue style references
   - Example: `"dialogueStyle": "@dialogue/styles:gruff"`

4. **Add Faction/Guild System**
   - relationships.json defining NPC affiliations
   - Affects dialogue, shop prices, quest availability

5. **Add Schedule System**
   - schedules.json defining time-of-day behaviors
   - Example: Blacksmith at shop 8am-6pm, tavern 6pm-10pm

---

## 7. Comparison Matrix

| Aspect | Enemies | Items | NPCs | Classes | Quests |
|--------|---------|-------|------|---------|--------|
| **File Count** | 39 | 15 | 10 | 3 | 16 |
| **Subdirectories** | 13 | 5 | 1 | 0 | 4 |
| **Largest Catalog** | 1,100 | 1,500 | **2,375** | 757 | 1,082 |
| **Organization** | Hierarchical | Hierarchical | Flat+1 | Flat | Mixed |
| **Total Lines** | 13,000 | 8,000 | 4,700 | 2,100 | 3,500 |
| **Item Count** | 100+ | 50+ | 56 | 5 | 27 |
| **Avg Items/Catalog** | 8 | 10 | **56** | 5 | 27 |
| **Scalability** | ✅ High | ✅ High | ⚠️ Medium | ✅ High | ✅ High |
| **Maintainability** | ✅ Easy | ✅ Easy | ⚠️ Medium | ✅ Easy | ✅ Easy |

**Key Insight**: NPCs have the **LARGEST single catalog** (2,375 lines) and **MOST items per catalog** (56 types), making them the **LEAST SCALABLE** domain despite being only the **4th largest by total lines**.

---

## 8. Action Items

### Immediate (This Session)
- [ ] **Discuss restructuring options** with team
- [ ] **Set file size policy** (max lines per catalog)
- [ ] **Prioritize missing file types** (relationships, locations, schedules)
- [ ] **Decide on dialogue consolidation** (merge styles files)

### Short-Term (Next Session)
- [ ] **Document organizational decision** in GDD
- [ ] **Add missing specialized files** (if approved)
- [ ] **Update ContentBuilder configs** for new files
- [ ] **Test build** after any changes

### Medium-Term (Next Sprint)
- [ ] **Implement restructuring** (if approved)
- [ ] **Refactor generation code** (if structure changes)
- [ ] **Update tests** for new file structure
- [ ] **Update documentation** comprehensively

### Long-Term (Future Releases)
- [ ] **Monitor NPC count growth** (track in README)
- [ ] **Plan Phase 2 restructuring** (at 80 NPCs)
- [ ] **Implement faction system** (relationships.json)
- [ ] **Add schedule system** (schedules.json)

---

## 9. Conclusion

**Status**: NPCs are **v4.0/v4.1 COMPLIANT** but **ORGANIZATIONALLY AT CAPACITY**.

**The Good**:
- ✅ All standards met (metadata, references, file types)
- ✅ Clean, consistent structure
- ✅ Dialogue properly separated
- ✅ Build passing

**The Concerns**:
- ⚠️ Largest single catalog (2,375 lines = 3x ideal size)
- ⚠️ Flat structure limits scalability
- ⚠️ Missing specialized files (relationships, locations, schedules)
- ⚠️ Inconsistent with enemies/items patterns

**The Decision**:
Choose ONE path forward:
1. **Proactive Restructuring** (split by function NOW) - Future-proof
2. **Reactive Monitoring** (wait until 80+ NPCs) - Pragmatic
3. **Status Quo Plus** (add specialized files, keep flat) - Minimal risk

**Recommendation**: **Option 2 (Reactive Monitoring)** + add specialized files
- Current size manageable (2,375 lines not critical yet)
- Add relationships.json, locations.json (expand functionality)
- Set trigger: 3,000 lines OR 80 NPC types = restructure
- Revisit in 2-3 months after growth assessed

---

**Next Steps**: Await user decision on organizational path forward.
