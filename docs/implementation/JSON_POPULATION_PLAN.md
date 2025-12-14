# JSON Data Population Plan 📊

**Date**: December 14, 2024  
**Purpose**: Systematic plan to populate 70+ placeholder JSON files  
**Current State**: 93 JSON files total, ~70 need content  
**Strategy**: Prioritized approach - quick wins first, then expansive content

---

## 📋 File Status Overview

### ✅ Already Populated (23 files)

**Enemies** (6 categories, 10 files):
- ✅ Beasts: names, prefixes (2 files)
- ✅ Demons: names, prefixes (2 files)
- ✅ Dragons: names, prefixes, colors (3 files)
- ✅ Elementals: names, prefixes (2 files)
- ✅ Humanoids: names, prefixes (2 files)
- ✅ Undead: names, prefixes (2 files)

**Items** (8 files):
- ✅ Materials: metals, woods, leathers, gemstones (4 files)
- ✅ Armor: materials (1 file)
- ✅ Weapons: names, prefixes (2 files)
- ✅ Enchantments: suffixes (1 file)

**NPCs** (4 files):
- ✅ Names: first_names (1 file)
- ✅ Occupations: common, criminal, magical, noble (4 files)
- ✅ Dialogue: templates, traits (2 files)

**Quests** (5 files):
- ✅ Templates: kill, fetch, investigate, escort, delivery (5 files)

**General** (2 files):
- ✅ adjectives, materials (2 files)

---

## ⏳ Need Population (70 files)

### Priority 1: Foundation Data (15 files) ⭐⭐⭐
**Impact**: HIGH | **Effort**: LOW | **Timeline**: 1-2 hours

These are simple list-based files that provide foundational data for descriptive text generation.

#### General Category (7 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `general/colors.json` | Color descriptions | 30-50 | "crimson", "azure", "emerald" |
| `general/smells.json` | Smell descriptions | 20-30 | "musty", "fragrant", "acrid" |
| `general/sounds.json` | Sound descriptions | 20-30 | "echoing", "whisper", "roar" |
| `general/textures.json` | Texture descriptions | 20-30 | "rough", "smooth", "slimy" |
| `general/time_of_day.json` | Time descriptions | 8-12 | "dawn", "dusk", "midnight" |
| `general/weather.json` | Weather conditions | 15-25 | "foggy", "stormy", "clear" |
| `general/verbs.json` | Action verbs | 40-60 | "attacks", "defends", "casts" |

#### NPC Names (1 file)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `npcs/names/last_names.json` | NPC surnames | 100-200 | "Ironforge", "Nightshade", "Stormwind" |

#### Items - Consumables (1 file)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/consumables/names.json` | Potion names | 30-50 | "Elixir", "Draught", "Tonic" |

#### Items - Armor (1 file)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/armor/names.json` | Armor base names | 20-40 | "Breastplate", "Gauntlets", "Helm" |

---

### Priority 2: Item Expansion (12 files) ⭐⭐
**Impact**: MEDIUM-HIGH | **Effort**: MEDIUM | **Timeline**: 2-3 hours

Expand item generation with prefixes, suffixes, and effects for richer loot.

#### Armor (2 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/armor/prefixes.json` | Armor modifiers | 40-60 | { "name": "Reinforced", "protection": 2 } |
| `items/armor/suffixes.json` | Armor quality | 30-50 | { "name": "of the Bear", "constitution": 1 } |

#### Weapons (1 file)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/weapons/suffixes.json` | Weapon quality | 30-50 | { "name": "of Precision", "dexterity": 2 } |

#### Enchantments (2 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/enchantments/prefixes.json` | Enchantment modifiers | 40-60 | { "name": "Flaming", "fire_damage": 5 } |
| `items/enchantments/effects.json` | Magical effects | 30-50 | "burns enemies", "freezes targets" |

#### Consumables (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `items/consumables/effects.json` | Potion effects | 25-40 | { "effect": "heal", "power": 50 } |
| `items/consumables/rarities.json` | Rarity tiers | 5-8 | { "name": "Rare", "multiplier": 2.0 } |

---

### Priority 3: Enemy Expansion (24 files) ⭐⭐
**Impact**: MEDIUM | **Effort**: MEDIUM-HIGH | **Timeline**: 3-4 hours

Add traits and suffixes to all enemy types for varied combat encounters.

#### Existing Enemy Types - Traits & Suffixes (16 files)
Each enemy type needs:
- **Traits** (special abilities, behaviors)
- **Suffixes** (titles, modifiers)

| Enemy Type | Files Needed | Target Per File | Example Trait | Example Suffix |
|------------|--------------|-----------------|---------------|----------------|
| Beasts | traits, suffixes | 15-25 | "Pack Hunter", "Feral Rage" | "the Savage", "Alpha" |
| Demons | traits, suffixes | 15-25 | "Soul Drain", "Hellfire" | "Infernal", "Corrupted" |
| Dragons | traits, suffixes | 15-25 | "Breath Weapon", "Ancient" | "the Destroyer", "Elder" |
| Elementals | traits, suffixes | 15-25 | "Immunity", "Volatile" | "Primordial", "Raging" |
| Humanoids | traits, suffixes | 15-25 | "Tactical", "Shield Wall" | "Captain", "Veteran" |
| Undead | traits, suffixes | 15-25 | "Life Drain", "Undying" | "Risen", "Cursed" |
| Vampires | traits, suffixes | 15-25 | "Regeneration", "Charm" | "Lord", "Ancient" |

#### New Enemy Types - Full Sets (8 files)
Add names, prefixes, traits, and suffixes for new enemy categories:

| Enemy Type | Files Needed | Priority | Example Names |
|------------|--------------|----------|---------------|
| **Goblinoids** | (4) names, prefixes, traits, suffixes | 🔥 HIGH | "Gruk", "Snarl", "Bitterfang" |
| **Orcs** | (4) names, prefixes, traits, suffixes | 🔥 HIGH | "Grommash", "Thrall", "Durotan" |
| **Trolls** | (4) names, prefixes, traits, suffixes | ⚠️ MEDIUM | "Zul'jin", "Vol'jin", "Sen'jin" |
| **Insects** | (4) names, prefixes, traits, suffixes | ⚠️ MEDIUM | "Hive Queen", "Swarm Lord" |
| **Plants** | (4) names, prefixes, traits, suffixes | ⚠️ LOW | "Thornbeast", "Vinegrasp" |
| **Reptilians** | (4) names, prefixes, traits, suffixes | ⚠️ LOW | "Scale Lord", "Fang Priest" |

**Note**: Only Goblinoids and Orcs are marked as high priority. Others can be deferred.

---

### Priority 4: NPC Personality & Dialogue (8 files) ⭐
**Impact**: MEDIUM | **Effort**: MEDIUM | **Timeline**: 2-3 hours

Enhance NPC depth with personality traits, quirks, backgrounds, and dialogue variations.

#### Personalities (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `npcs/personalities/traits.json` | Character traits | 40-60 | "brave", "cowardly", "greedy" |
| `npcs/personalities/quirks.json` | Unique behaviors | 30-50 | "twitches nose", "quotes poetry" |
| `npcs/personalities/backgrounds.json` | NPC histories | 25-40 | "former soldier", "orphaned merchant" |

#### Dialogue (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `npcs/dialogue/greetings.json` | Opening dialogue | 25-40 | "Well met, traveler!", "What brings you here?" |
| `npcs/dialogue/farewells.json` | Closing dialogue | 25-40 | "Safe travels!", "May the gods watch over you" |
| `npcs/dialogue/rumors.json` | NPC gossip/hints | 40-80 | "I heard strange noises from the old mill..." |

---

### Priority 5: Quest System (9 files) ⭐
**Impact**: MEDIUM | **Effort**: MEDIUM | **Timeline**: 2-3 hours

Expand quest variety with objectives, locations, and rewards.

#### Objectives (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `quests/objectives/primary.json` | Main quest goals | 30-50 | { "type": "kill", "target": "boss", "count": 1 } |
| `quests/objectives/secondary.json` | Optional goals | 25-40 | { "type": "collect", "item": "herbs", "count": 10 } |
| `quests/objectives/hidden.json` | Secret objectives | 15-25 | { "type": "discover", "location": "hidden_cave" } |

#### Locations (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `quests/locations/towns.json` | Town/city names | 30-50 | "Ironforge", "Stormwind", "Goldshire" |
| `quests/locations/wilderness.json` | Outdoor locations | 40-60 | "Darkwood Forest", "Frozen Peaks", "Misty Swamp" |
| `quests/locations/dungeons.json` | Dungeon names | 30-50 | "Tomb of Eternal Darkness", "Crystal Caverns" |

#### Rewards (3 files)
| File | Purpose | Target Count | Example Content |
|------|---------|--------------|-----------------|
| `quests/rewards/gold.json` | Gold ranges | 10-15 | { "min": 50, "max": 100, "level": 5 } |
| `quests/rewards/experience.json` | XP rewards | 10-15 | { "base": 100, "level_multiplier": 1.5 } |
| `quests/rewards/items.json` | Item rewards | 30-50 | { "type": "weapon", "rarity": "rare", "level": 10 } |

---

## 📅 Implementation Roadmap

### Phase 1: Foundation (Priority 1) - Week 1
**Goal**: Establish baseline descriptive vocabulary  
**Files**: 15 files  
**Estimated Time**: 1-2 hours  
**Deliverable**: All general descriptors, NPC last names, basic consumable/armor names

**Order**:
1. General: colors, smells, sounds, textures (30 min)
2. General: time_of_day, weather, verbs (30 min)
3. NPCs: last_names (20 min)
4. Items: consumables/names, armor/names (20 min)

---

### Phase 2: Item System (Priority 2) - Week 1-2
**Goal**: Rich, varied loot generation  
**Files**: 12 files  
**Estimated Time**: 2-3 hours  
**Deliverable**: Complete item generation pipeline (armor, weapons, enchantments, consumables)

**Order**:
1. Armor: prefixes, suffixes (45 min)
2. Weapons: suffixes (30 min)
3. Enchantments: prefixes, effects (45 min)
4. Consumables: effects, rarities (45 min)

---

### Phase 3A: Core Enemies (Priority 3 - Part 1) - Week 2
**Goal**: Enhanced combat variety for existing enemy types  
**Files**: 14 files (7 enemy types × 2 files each)  
**Estimated Time**: 2-3 hours  
**Deliverable**: Traits and suffixes for all 7 existing enemy categories

**Order**:
1. Common enemies: Beasts, Humanoids, Undead (1 hour)
2. Powerful enemies: Demons, Dragons, Elementals (1 hour)
3. Specialty enemy: Vampires (30 min)

---

### Phase 3B: New Enemy Types (Priority 3 - Part 2) - Week 3
**Goal**: Expand enemy roster with new categories (OPTIONAL)  
**Files**: 8 files (Goblinoids, Orcs priority)  
**Estimated Time**: 2-3 hours  
**Deliverable**: Goblinoids and Orcs fully implemented

**Order**:
1. Goblinoids: names, prefixes, traits, suffixes (1 hour)
2. Orcs: names, prefixes, traits, suffixes (1 hour)
3. (Optional) Trolls, Insects, Plants, Reptilians (defer to Phase 6)

---

### Phase 4: NPC Depth (Priority 4) - Week 2-3
**Goal**: Memorable, varied NPCs  
**Files**: 8 files  
**Estimated Time**: 2-3 hours  
**Deliverable**: Full NPC personality and dialogue system

**Order**:
1. Personalities: traits, quirks, backgrounds (1 hour)
2. Dialogue: greetings, farewells (45 min)
3. Dialogue: rumors (1 hour)

---

### Phase 5: Quest Variety (Priority 5) - Week 3
**Goal**: Diverse, engaging quests  
**Files**: 9 files  
**Estimated Time**: 2-3 hours  
**Deliverable**: Complete quest generation system

**Order**:
1. Objectives: primary, secondary, hidden (1 hour)
2. Locations: towns, wilderness, dungeons (1 hour)
3. Rewards: gold, experience, items (1 hour)

---

### Phase 6: Polish & Expansion (Optional) - Week 4+
**Goal**: Complete remaining enemy types  
**Files**: 12 files (Trolls, Insects, Plants, Reptilians)  
**Estimated Time**: 3-4 hours  
**Deliverable**: All enemy types fully populated

**Order**: Based on game needs and priority

---

## 🎯 Success Metrics

### Completion Tracking
- **Phase 1**: 15/70 files (21%) - ✅ Foundation complete
- **Phase 2**: 27/70 files (39%) - ✅ Items complete
- **Phase 3A**: 41/70 files (59%) - ✅ Core enemies complete
- **Phase 3B**: 49/70 files (70%) - ✅ New enemies added (Goblinoids/Orcs)
- **Phase 4**: 57/70 files (81%) - ✅ NPCs complete
- **Phase 5**: 66/70 files (94%) - ✅ Quests complete
- **Phase 6**: 70/70 files (100%) - 🎉 ALL COMPLETE

### Quality Targets
- **Minimum**: 15-20 items per simple list file
- **Standard**: 25-40 items per standard file
- **Rich**: 50+ items for key files (colors, verbs, rumors)
- **Complex**: Full data structures with properties for prefixes/suffixes/traits

---

## 📝 Content Guidelines

### Data Structure Patterns

#### Simple String Arrays
```json
[
  "item1",
  "item2",
  "item3"
]
```
**Used for**: Names, colors, sounds, verbs, etc.

#### Object Arrays with Properties
```json
[
  {
    "name": "Flaming",
    "damage_bonus": 5,
    "element": "fire",
    "description": "Burns enemies on contact"
  }
]
```
**Used for**: Prefixes, suffixes, traits, effects

#### Categorized Data
```json
{
  "common": ["item1", "item2"],
  "rare": ["item3", "item4"],
  "legendary": ["item5"]
}
```
**Used for**: Rarities, tiers, categories

### Naming Conventions
- **Fantasy-appropriate**: Medieval/fantasy theme
- **Evocative**: Descriptive, atmospheric
- **Varied**: Mix of common and unique
- **Balanced**: Avoid overpowered or useless items

### Content Sources
- **Existing game lore**: D&D, Warcraft, Elder Scrolls, etc.
- **Fantasy literature**: Tolkien, R.R. Martin, Brandon Sanderson
- **Procedural generation**: Combine base words creatively
- **Player expectations**: Common RPG tropes and standards

---

## 🔄 Validation & Testing

### After Each Phase
1. **Build test**: `dotnet build` - ensure no JSON errors
2. **Unit tests**: `dotnet test` - verify data loading
3. **Game test**: Launch game, create character, check generation
4. **ContentBuilder test**: Open relevant files, verify display

### Quality Checks
- ✅ Valid JSON syntax (no trailing commas, proper quotes)
- ✅ Consistent structure within each file
- ✅ No duplicate entries
- ✅ Appropriate quantity (meeting target counts)
- ✅ Thematic coherence (fits game world)
- ✅ Balanced properties (no extreme values)

---

## 🚀 Quick Start Commands

### Start Phase 1 (Foundation)
```bash
# Create working branch
git checkout -b feature/json-population-phase1

# Edit files in order:
# 1. general/colors.json
# 2. general/smells.json
# 3. general/sounds.json
# 4. general/textures.json
# 5. general/time_of_day.json
# 6. general/weather.json
# 7. general/verbs.json
# 8. npcs/names/last_names.json
# 9. items/consumables/names.json
# 10. items/armor/names.json

# Test after each file
dotnet build
dotnet test

# Commit when phase complete
git add Game.Shared/Data/Json/general/*.json
git add Game.Shared/Data/Json/npcs/names/last_names.json
git add Game.Shared/Data/Json/items/consumables/names.json
git add Game.Shared/Data/Json/items/armor/names.json
git commit -m "Phase 1: Populate foundation data (15 files)"
```

---

## 📊 Estimated Total Time

| Phase | Files | Time | Priority |
|-------|-------|------|----------|
| Phase 1: Foundation | 15 | 1-2h | ⭐⭐⭐ |
| Phase 2: Items | 12 | 2-3h | ⭐⭐ |
| Phase 3A: Core Enemies | 14 | 2-3h | ⭐⭐ |
| Phase 3B: New Enemies | 8 | 2-3h | ⭐ |
| Phase 4: NPCs | 8 | 2-3h | ⭐ |
| Phase 5: Quests | 9 | 2-3h | ⭐ |
| Phase 6: Polish | 12 | 3-4h | (Optional) |
| **TOTAL** | **70+** | **14-20h** | |

**Realistic Timeline**: 
- **Minimum Viable** (Phases 1-3A): 5-8 hours (44 files, 63%)
- **Feature Complete** (Phases 1-5): 10-14 hours (66 files, 94%)
- **Full Polish** (All Phases): 14-20 hours (70+ files, 100%)

---

## ✅ Next Steps

### Immediate Actions
1. **Review this plan** - Confirm approach and priorities
2. **Choose starting phase** - Recommend Phase 1 (Foundation)
3. **Set timeline** - Decide on incremental vs. comprehensive approach
4. **Begin population** - Start with general/colors.json

### Decision Points
- **Incremental vs. Batch**: Populate and commit per file, or complete phases before committing?
- **Manual vs. AI-Assisted**: Write content manually, or use AI to generate initial data?
- **Testing Frequency**: Test after each file, or after each phase?

### Recommended Approach
**🎯 Incremental + AI-Assisted + Phase Testing**
1. Use AI to generate initial content for each file
2. Review and refine AI-generated content
3. Commit after completing each phase
4. Test thoroughly before moving to next phase
5. Focus on Phases 1-3A first (minimum viable)

---

**Ready to begin?** Let's start with Phase 1 - Foundation Data! 🚀

---

**End of Plan** | JSON Population Roadmap Complete ✅
