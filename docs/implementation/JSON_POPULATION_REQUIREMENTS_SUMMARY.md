# JSON Population - Requirements Summary ✅

**Date**: December 14, 2024  
**Status**: Requirements finalized, ready for implementation  
**Full Plan**: See `JSON_POPULATION_PLAN.md`

---

## 🎯 Quick Reference: Key Decisions

### 1. Data Structure: Hybrid Pattern (ALL Files)

```json
{
  "items": [/* 100-150 pre-generated entries */],
  "components": {/* Building blocks for procedural */},
  "patterns": [/* Generation rules */]
}
```

**Usage**: Game uses `items` only (Phase 1-5), procedural generation in Phase 6

---

### 2. Tier System: 7 Levels (Applied Everywhere)

**Tiers**: Common → Uncommon → Rare → Epic → Legendary → Mythic → Ancient

**Applied to**:
- ✅ All items (weapons, armor, consumables, enchantments)
- ✅ Enemy traits and suffixes
- ✅ Quest objectives and rewards
- ✅ All loot generation

**Stat Scaling**: Conservative base + rarity multipliers + level scaling + difficulty modifiers

---

### 3. Enemy System: Separate Components

- **Prefixes**: Name modifiers ("Ancient", "Corrupted")
- **Suffixes**: Titles + stats ("the Savage" +2 damage)
- **Traits**: Abilities ("Pack Hunter", "Regeneration")
- **Independence**: All can combine freely (no restrictions Phase 1-5)

**Trait Distribution** (per enemy type):
- 6 common, 5 uncommon, 4 rare, 3 epic, 2 legendary, 1 mythic, 1 ancient = 22 total

---

### 4. Enchantment Power: Scaling + Tiers

**Hybrid System**:
- Each tier has base damage + scaling coefficient
- Higher tiers = better base AND better scaling
- Example: "Flaming" enchantment scales from 3-7.5 (Lesser) to 40-60 (Greater)

**Tier Names**: Mix of Classic/Fancy/Context-specific as appropriate

---

### 5. NPC System: Personality-Dialogue Linking

**Components**:
- **Traits**: Core characteristics (brave, greedy)
- **Quirks**: Unique behaviors (twitches nose)
- **Backgrounds**: Histories + optional skill bonuses (hybrid)

**Dialogue**:
- Metadata-rich (tone, formality, personality_match tags)
- 50-80 greetings/farewells (distributed across formality levels)
- Rumors = quest hooks with metadata (location, danger, type, tier)

---

### 6. Quest System: Full Metadata + Multi-Scaling

**Locations**: Full metadata
```json
{
  "name": "Darkwood Forest",
  "tier": "uncommon",
  "level_range": [5, 15],
  "environment": "forest",
  "danger": "medium",
  "enemy_types": ["beasts", "plants"],
  "tags": ["wilderness", "hunt"]
}
```

**Rewards**: Triple-factor scaling
- `reward = base * tier_mult * level_mult * difficulty_mult`

**Hidden Objectives**: Tiered (common = bonus, rare = hidden, legendary = easter eggs)

**Quest-Location**: Pre-assigned + tags for flexibility

---

## 📊 Target Counts (Higher End)

| Category | Target | Notes |
|----------|--------|-------|
| Colors, Sounds, Textures | 40-50 | High variety |
| Verbs | 50-60 | Combat + general |
| Last Names | 150-200 | 40% fantasy, 30% Nordic/Celtic, 20% Eastern, 10% other |
| Item Prefixes/Suffixes | 40-60 | Distributed across 7 tiers |
| Enemy Traits | 20-25 per type | Weighted (6 common → 1 ancient) |
| Dialogue | 50-80 | Account for formality/tone |
| Quest Locations | 30-60 per type | Varied environments |

---

## ⏱️ Timeline (Updated)

| Phase | Files | Time | Deliverable |
|-------|-------|------|-------------|
| Phase 1: Foundation | 15 | 2-3h | General descriptors, names, item basics |
| Phase 2: Items | 12 | 3-4h | Armor, weapons, enchantments, consumables |
| Phase 3A: Core Enemies | 14 | 3-4h | Traits + suffixes for 7 enemy types |
| Phase 3B: New Enemies | 8 | 2-3h | Goblinoids + Orcs complete |
| Phase 4: NPCs | 8 | 3-4h | Personalities + dialogue |
| Phase 5: Quests | 9 | 3-4h | Objectives + locations + rewards |
| **TOTAL** | **66** | **18-25h** | **Feature Complete** |

**Why longer than original?**
- Hybrid structure for all files
- 7-tier system with metadata
- Higher target counts for quality
- Full metadata (locations, objectives, rewards, dialogue)
- Converting existing files to new structure

**Minimum Viable**: Phases 1-3A (44 files, 8-11h, 63%)  
**Feature Complete**: Phases 1-5 (66 files, 18-25h, 94%)

---

## 🔄 Implementation Strategy

**Workflow**:
1. **Populate** all JSON files with hybrid structure (Phases 1-5)
2. **Convert** existing files to new structure (during Phase 1-2)
3. **Test** build + validation after Phase 1
4. **Update code** (GameDataService) to use `items` array after Phase 1
5. **Continue** through remaining phases
6. **Implement procedural** generation using components/patterns (Phase 6)

**Testing Cadence**:
- After Phase 1: Validate structure, update code
- After Phase 2+3A: Test items + enemies
- After Phase 3B+4: Test NPCs
- After Phase 5: Full integration test
- Phase 6: Procedural + polish

---

## 🚀 Next Steps

**Immediate**: Begin Phase 1 - Foundation Data

**Starting with**:
1. General category (7 files): colors, smells, sounds, textures, time_of_day, weather, verbs
2. NPC names (1 file): last_names with cultural diversity
3. Item basics (2 files): consumables/names, armor/names

**Phase 1 Deliverables**:
- 15 files populated with hybrid structure
- Existing files converted to new structure
- GameDataService updated to use `items` array
- All builds passing
- Ready for Phase 2

---

## 📚 Reference Documents

- **Full Plan**: `JSON_POPULATION_PLAN.md` (detailed breakdown, examples, guidelines)
- **Reorganization Complete**: `JSON_REORGANIZATION_COMPLETE.md` (how we got here)
- **ContentBuilder Update**: `CONTENTBUILDER_UPDATE_COMPLETE.md` (UI integration)

---

**Status**: ✅ Ready to begin Phase 1  
**Updated**: December 14, 2024  
**Next**: Phase 1 implementation (2-3 hours)
