# GDD Maintenance Guide

**Purpose**: Quick reference for keeping GDD-Main.md up to date  
**Last Updated**: December 9, 2025

---

## 📋 When to Update the GDD

### ✅ Always Update For:

1. **New Features Implemented**
   - Add to "Implemented Features ✅" section
   - Move from "Future Features 🔮" if applicable
   - Update feature counts in Executive Summary
   - Document mechanics in relevant Core Systems section

2. **Major System Changes**
   - Update relevant Core Systems section
   - Add/modify tables (stats, scaling, progression)
   - Update UI mockups if screens changed

3. **Architecture Changes**
   - Update Technical Architecture section
   - Modify project structure diagram
   - Update technology stack table

4. **Test Count Changes**
   - Update Executive Summary stats
   - Update Technical Architecture test count

5. **New Phases/Roadmap Items**
   - Add to Future Roadmap section
   - Estimate timeline
   - List planned features

### ⚠️ Update If Needed:

- Balance changes (damage, XP, gold values in tables)
- New character classes or attributes
- New item types or rarity tiers
- Configuration changes (new settings)

### ❌ Don't Update For:

- Bug fixes (unless they change documented behavior)
- Code refactoring (unless architecture changes)
- Minor text/UI tweaks
- Internal implementation details (use implementation docs)

---

## 🎯 What Goes Where

### GDD-Main.md (Game Design Document)
**Purpose**: High-level game overview and design

**Include**:
- ✅ What features exist
- ✅ How systems work (player perspective)
- ✅ Game mechanics and rules
- ✅ Content tables (stats, progression, loot)
- ✅ UI mockups and screen flows
- ✅ Future roadmap

**Exclude**:
- ❌ Code examples (unless small snippets)
- ❌ Implementation details (how code works)
- ❌ Technical architecture internals
- ❌ Completed work history

### Implementation Docs (docs/implementation/)
**Purpose**: How systems are built internally

**Include**:
- ✅ Code architecture
- ✅ Class diagrams
- ✅ Code examples
- ✅ Technical decisions
- ✅ File structure
- ✅ API references

### User Guides (docs/guides/)
**Purpose**: How to use systems as a developer/player

**Include**:
- ✅ Step-by-step tutorials
- ✅ Code examples (for devs)
- ✅ UI walkthroughs
- ✅ Common tasks
- ✅ Tips and tricks

---

## 📝 Update Checklist

When implementing a new feature:

### Step 1: During Development
- [ ] Create implementation doc (if complex): `docs/implementation/[FEATURE]_IMPLEMENTATION.md`
- [ ] Create user guide (if user-facing): `docs/guides/[FEATURE]_GUIDE.md`
- [ ] Write unit tests
- [ ] Document in code comments

### Step 2: After Completion
- [ ] **Update GDD-Main.md**:
  - [ ] Add to "Implemented Features ✅" section
  - [ ] Document in relevant "Core Systems" section
  - [ ] Update Executive Summary stats (features, tests, handlers)
  - [ ] Add UI mockup if new screen
  - [ ] Update content tables if applicable
- [ ] **Update docs/README.md** (if new files added)
- [ ] **Update project README.md** (if major feature)
- [ ] **Update TEST_COVERAGE_REPORT.md**
- [ ] **Update this checklist if process changes**

### Step 3: Communication
- [ ] Commit with clear message: "feat: Add [feature] + update GDD"
- [ ] Update version number in GDD if major release
- [ ] Update "Last Updated" date in GDD

---

## 🔧 Common GDD Updates

### Adding a New Feature

**Location**: Multiple sections

1. **Executive Summary** (line ~20):
   ```markdown
   - **X Features** → Update count
   - **Y Tests** → Update count
   ```

2. **Game Features > Implemented Features ✅** (line ~410):
   ```markdown
   #### [Category]
   - ✅ [Feature name] - Description
   ```

3. **Core Systems** (line ~50+):
   - Add new section if major system
   - Add to existing section if extending system
   - Include mechanics, formulas, tables

4. **User Interface** (line ~700+):
   - Add screen mockup
   - Document menu flow

### Moving Feature from Future to Implemented

**Before**:
```markdown
### Future Features 🔮
#### Content Expansion
- 🔮 Side quests (10-20 optional quests)
```

**After**:
```markdown
### Implemented Features ✅
#### Progression
- ✅ Side quests (12 optional quests)

### Future Features 🔮
#### Content Expansion
- 🔮 [Other features...]
```

### Adding a New Phase to Roadmap

**Location**: Future Roadmap section (line ~900+)

```markdown
### Phase X: [Name] (Planned)

**Timeline**: QX YYYY
**Focus**: [Main focus]

#### Features

1. **[Feature 1]**
   - [Detail]
   - [Detail]

2. **[Feature 2]**
   - [Detail]
```

### Updating Statistics

**Locations to update**:

1. Executive Summary (line ~20):
   ```markdown
   - **375 Unit Tests** → Update
   - **10 Features** → Update
   - **27 CQRS Handlers** → Update
   ```

2. Technical Architecture > Project Structure (line ~500):
   ```markdown
   375 tests (98.9% pass rate) → Update
   ```

3. Footer (last line):
   ```markdown
   **Last Updated**: December 9, 2025 → Update date
   ```

---

## 📊 Section-by-Section Guide

### Executive Summary
- **Update frequency**: After major features
- **What to update**: Feature counts, test counts, key highlights
- **Keep concise**: 1-2 paragraphs max per subsection

### Game Overview
- **Update frequency**: Rarely (only if core concept changes)
- **What to update**: Genre, theme, victory conditions
- **Keep high-level**: Player-facing description

### Core Systems
- **Update frequency**: Every new system or major system change
- **What to update**: Mechanics, formulas, tables, stats
- **Keep detailed**: This is the meat of the GDD

### Game Features
- **Update frequency**: Every feature implementation
- **What to update**: Move items from Future to Implemented
- **Keep organized**: Group by category (Core Gameplay, Content, etc.)

### Technical Architecture
- **Update frequency**: Architecture changes, new libraries
- **What to update**: Project structure, tech stack, patterns
- **Keep current**: Reflects actual codebase

### Content & Progression
- **Update frequency**: Balance changes, new content
- **What to update**: Tables (XP, scaling, loot), progression paths
- **Keep accurate**: Players use these for planning

### User Interface
- **Update frequency**: New screens or major UI changes
- **What to update**: Screen mockups, menu flows, commands
- **Keep visual**: ASCII mockups help understanding

### Future Roadmap
- **Update frequency**: After each phase completion
- **What to update**: Move completed to Implemented, add new phases
- **Keep realistic**: Estimate timelines conservatively

---

## 🚨 Common Mistakes to Avoid

### ❌ Don't:

1. **Add implementation details to GDD**
   - ❌ "The CombatService.cs uses dependency injection..."
   - ✅ "Combat uses turn-based mechanics with 4 actions..."

2. **Duplicate information**
   - ❌ Copy-pasting between GDD and implementation docs
   - ✅ GDD has overview, implementation doc has details

3. **Leave outdated info**
   - ❌ "Future: Quest system" when it's implemented
   - ✅ Move to Implemented section immediately

4. **Forget to update stats**
   - ❌ "38 tests" when you have 375
   - ✅ Update Executive Summary after test additions

5. **Write for developers only**
   - ❌ Technical jargon without explanation
   - ✅ Explain concepts for designers/players too

### ✅ Do:

1. **Update immediately after feature completion**
2. **Use consistent formatting** (follow existing style)
3. **Keep it player/designer focused** (how, not implementation)
4. **Include tables and formulas** (make it useful)
5. **Update the date** (Last Updated in footer)

---

## 🔗 Quick Reference Links

- **GDD Location**: `docs/GDD-Main.md`
- **Implementation Docs**: `docs/implementation/`
- **User Guides**: `docs/guides/`
- **Test Coverage**: `docs/testing/TEST_COVERAGE_REPORT.md`
- **Documentation Index**: `docs/README.md`

---

## 📞 Questions?

If unsure where to document something:

1. **Is it user/player-facing?** → GDD-Main.md
2. **Is it technical implementation?** → docs/implementation/
3. **Is it a how-to guide?** → docs/guides/
4. **Is it test-related?** → docs/testing/

**When in doubt**: Put high-level overview in GDD, details in implementation docs.

---

**Last Updated**: December 9, 2025  
**Document Version**: 1.0
