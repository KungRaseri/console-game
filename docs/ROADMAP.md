# Development Roadmap

**Purpose**: High-level development vision and work organization for RealmEngine console game.

**Scope**: Console game backend, APIs, and game logic. UI implementation (Godot) is out of scope but we design service APIs for future integration.

**Status Source**: See [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) for detailed feature status and test coverage.

---

## 🎯 Quick Wins (Integration-Only Work)
**Goal**: Connect existing production-ready code that's been orphaned

### LocationGenerator Integration
- **Status**: `[✅ Finished]` (January 5, 2026)
- **Effort**: Low (2-3 days) → **Actual: 1 day**
- **Impact**: HIGH - Unlocks location-specific content
- **Work**: ~~Wire 300+ lines of LocationGenerator into ExplorationService~~ **DONE**
- **File**: `RealmEngine.Core.Generators.Modern.LocationGenerator`
- **Dependencies**: None
- **API Impact**: `GetKnownLocationsAsync()` returns `List<Location>` with dynamic generation (2 towns, 3 dungeons, 3 wilderness)
- **Details**: 
  - Made `GenerateLocationsAsync()` virtual for testing
  - Changed from `List<string>` to `List<Location>` with Id/Name/Description/Type
  - Added `InitializeLocationsAsync()` for lazy loading
  - All 7,823 tests passing

---

### Quest Service Connections
- **Status**: `[Not Started]`
- **Effort**: Low (2-3 days)
- **Impact**: HIGH - Makes quests playable
- **Work**: 
  - Hook QuestService/QuestProgressService into gameplay loop
  - Connect kill counters to UpdateProgressAsync()
  - Trigger completion events
- **Files**: `QuestService`, `QuestProgressService`, `MainQuestService`
- **Dependencies**: Quest Service API (see Core Loop)
- **API Impact**: Quest progress updates automatically during gameplay

---

### Shop Service API
- **Status**: `[Not Started]`
- **Effort**: Medium (1 week)
- **Impact**: MEDIUM - Enables merchant interactions
- **Work**:
  - Expose ShopEconomyService methods as callable APIs
  - Design request/response models for buy/sell operations
  - Implement inventory generation TODOs
  - Load core items from catalog
- **Files**: `ShopEconomyService` (326 lines complete, needs API layer)
- **Dependencies**: None
- **API Impact**: 
  - `BrowseShopInventory(merchantId)` → returns items for sale
  - `BuyItem(merchantId, itemId)` → purchase transaction
  - `SellItem(merchantId, itemId)` → sell transaction

---

## 🎮 Core Loop (Make It Playable)
**Goal**: Essential systems for meaningful progression and objectives

### Progression System Overhaul `[SYSTEM GROUP]`
**This is the biggest work item - three interconnected systems that must be designed together**

**⚠️ ARCHITECTURE REQUIRED**: These systems need design documents before implementation

---

#### 1. Skills System
- **Status**: `[Rebuild Required]`
- **Effort**: Very High (3-4 weeks)
- **Current State**: 10 hardcoded passive bonuses (wrong approach)
- **Vision**: Practice-based skill progression system

**Design Needed**: `docs/designs/skills-system-design.md`

**Key Questions to Answer**:
- Domain structure: `/skills/attributes/`, `/skills/combat/`, `/skills/magic/`, `/skills/professions/`?
- Progression mechanics: XP per action? Milestone-based? Trainer-based?
- Skill effects: Damage multipliers? Unlock abilities? Success rates?
- Caps and limits: Hard cap at 100? Soft cap? Different per skill?

**Proposed Skill Categories**:
- **Attribute Skills**: Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma
- **Combat Skills**: One-Handed, Two-Handed, Archery, Block, Heavy Armor, Light Armor
- **Magic Skills**: Destruction, Restoration, Alteration, Conjuration, Illusion, Mysticism
- **Profession Skills**: Blacksmithing, Alchemy, Enchanting
- **Survival Skills**: Lockpicking, Sneaking, Pickpocketing, Speech

**Dependencies**: None (foundational)

**API Impact**:
- `GetSkillProgress(skillId)` → returns current level, XP, next level threshold
- `GetAllSkills()` → returns character's skill sheet
- Skill increases happen automatically during gameplay based on usage

---

#### 2. Abilities System
- **Status**: `[Needs Audit + Design]`
- **Effort**: High (2-3 weeks)
- **Current State**: 100+ JSON files in `/abilities/` (purpose unclear, needs audit)
- **Vision**: Class and species-specific active powers

**Design Needed**: `docs/designs/abilities-system-design.md`

**Work Required**:
1. **Audit existing abilities** - Are current JSON files class-specific? Spell-like? Usable?
2. **Define ability domains** - Organize by class, by type, by acquisition method?
3. **Class ability trees** - Warrior abilities (Charge, Cleave), Rogue abilities (Backstab, Vanish), etc.
4. **Species abilities** - If applicable (e.g., Dwarf resistance, Elf magic affinity)
5. **Acquisition system** - Level-based? Skill-based? Trainer-based?
6. **Integrate into CombatService** - Mana costs, cooldowns, effects

**Proposed Structure**:
```
/abilities/
  /warrior/     - Melee-focused powers
  /rogue/       - Stealth and critical powers
  /mage/        - Arcane combat powers (distinct from learned spells)
  /cleric/      - Divine powers
  /ranger/      - Nature and archery powers
  /paladin/     - Holy warrior powers
  /species/     - Racial abilities (if applicable)
  /shared/      - Universal abilities any class can learn
```

**Dependencies**: Skills System (for unlocking, balance)

**API Impact**:
- `GetClassAbilities(className)` → returns available abilities for class
- `GetLearnedAbilities()` → returns character's known abilities
- `UseAbility(abilityId, targetId)` → execute ability in combat
- `GetAbilityCooldowns()` → returns cooldown timers

---

#### 3. Magic & Spell System
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Current State**: Nothing (0%)
- **Vision**: Learnable spell system with schools of magic (domains)

**Design Needed**: `docs/designs/magic-system-design.md`

**Key Concepts**:
- **Spell Domains/Schools**: Destruction (fire, ice, lightning), Restoration (healing), Alteration (shields, buffs), Conjuration (summons), Illusion (charm, fear), Mysticism (detect, teleport)
- **Acquisition**: Spellbooks, scrolls, NPC teachers, quest rewards
- **Casting**: Mana costs, casting time, spell fizzle (based on magic skill)
- **Spell Levels**: Novice, Apprentice, Adept, Expert, Master
- **Distinction from Abilities**: Anyone can learn spells (with skill), but abilities are class-specific

**Proposed Structure**:
```
/spells/
  /destruction/     - Offensive magic
  /restoration/     - Healing and curing
  /alteration/      - Buffs, shields, utility
  /conjuration/     - Summoning and binding
  /illusion/        - Charm, fear, invisibility
  /mysticism/       - Teleportation, detection
```

**Dependencies**: 
- Skills System (magic skills affect success/power)
- Abilities System (clear distinction needed)

**API Impact**:
- `GetLearnedSpells()` → returns character's spellbook
- `LearnSpell(spellId)` → add spell to spellbook (from item/teacher)
- `CastSpell(spellId, targetId)` → cast spell in or out of combat
- `GetSpellSchools()` → returns available magic domains

---

### Quest & Objective System

#### Quest Service API
- **Status**: `[Not Started]`
- **Effort**: Medium (1-2 weeks)
- **Work**:
  - Expose QuestService methods as callable APIs
  - Design quest browsing/tracking interface
  - Implement objective tracking display
- **Dependencies**: Quick Win (Quest Service Connections)
- **API Impact**:
  - `GetActiveQuests()` → returns current quests with progress
  - `GetAvailableQuests()` → returns quests player can start
  - `StartQuest(questId)` → begin quest
  - `GetQuestDetails(questId)` → full quest info with objectives

---

#### Boss Encounters
- **Status**: `[Not Started]`
- **Effort**: High (2-3 weeks)
- **Work**:
  - Design unique boss enemies for quest climaxes
  - Boss-specific mechanics and abilities
  - Boss loot tables and rewards
  - Required for main quests #4 (Dark Prophecy) and #6 (Final Boss)
- **Dependencies**: Abilities System (bosses need unique abilities)
- **API Impact**: Boss encounters integrated into combat system

---

## 🌍 Content & Depth (Make It Engaging)
**Goal**: World feels alive, choices matter, content has variety

### Location System

#### Location-Specific Content
- **Status**: `[Not Started]`
- **Effort**: Medium (2-3 weeks)
- **Work**:
  - Location-specific enemy spawn tables
  - Location-specific loot tables
  - Environmental effects (heat, cold, darkness)
  - Location difficulty ratings
- **Dependencies**: LocationGenerator Integration (Quick Win)
- **API Impact**: Exploration yields different results per location

---

#### Town Mechanics
- **Status**: `[Not Started]`
- **Effort**: Medium (2-3 weeks)
- **Work**:
  - Rest/Inn system (heal, save, time passage)
  - Town-specific shops
  - NPC interactions and quest givers
  - Safe zones (no combat)
- **Dependencies**: Shop Service API (Quick Win)
- **API Impact**:
  - `GetTownServices(locationId)` → available services
  - `Rest(hours)` → heal and advance time
  - `GetTownNPCs(locationId)` → available NPCs

---

#### Dungeon Multi-Room System
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Work**:
  - Room-by-room progression (entrance → rooms → boss)
  - Room types (combat, puzzle, treasure, boss)
  - Persistent dungeon state (cleared rooms stay cleared)
  - Exit/retreat mechanics
- **Dependencies**: Location-Specific Content, Boss Encounters
- **API Impact**:
  - `EnterDungeon(locationId)` → start dungeon run
  - `ExploreRoom()` → progress to next room
  - `GetDungeonProgress(dungeonId)` → room completion status

---

### Combat Depth

#### Status Effects System
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Work**:
  - DoT effects (poison, burning, bleeding)
  - Buffs (strength, defense, speed)
  - Debuffs (weakness, slow, blind)
  - Crowd control (stun, freeze, fear)
  - Duration tracking and tick system
  - Cleansing and curing
  - Stacking rules (can you be poisoned AND burning?)
- **Dependencies**: None (but enables many other systems)
- **API Impact**:
  - `GetActiveEffects(entityId)` → current status effects
  - `ApplyEffect(entityId, effectId)` → add effect
  - `RemoveEffect(entityId, effectId)` → cleanse effect
  - Status effects auto-tick during combat turns

---

#### Trait Effects Integration
- **Status**: `[Partially Done - 20%]`
- **Effort**: Medium (2-3 weeks)
- **Current State**: Trait data exists in JSON, TraitValue class works, but traits don't affect gameplay
- **Work**:
  - Integrate weapon traits into combat (Fire damage, Lightning chain, etc.)
  - Elemental damage types and resistances
  - Enemy trait behaviors (regeneration, immunity, weakness)
  - Item trait bonuses applied to stats
- **Dependencies**: Status Effects System (traits trigger effects)
- **API Impact**: Traits automatically affect combat calculations

---

## 🚀 Feature Expansion (Long-term Vision)
**Goal**: Add depth, replayability, and polish for rich gameplay

### Social & World Systems

#### Reputation & Faction System
- **Status**: `[Not Started]`
- **Effort**: Medium (2-3 weeks)
- **Work**:
  - Faction definitions (guilds, cities, factions)
  - Reputation tracking per faction
  - Action consequences (killing faction members, quests)
  - Faction-locked content (shops, quests, areas)
- **API Impact**:
  - `GetFactionReputation(factionId)` → current standing
  - `GetFactionPerks(factionId)` → benefits at current standing

---

#### Party System
- **Status**: `[Not Started]`
- **Effort**: Very High (4-6 weeks)
- **Work**:
  - NPC recruitment system
  - Party management (add/remove members)
  - Party combat mechanics (AI-controlled allies)
  - Party progression (companion levels/equipment)
  - Companion quests and backstories
- **API Impact**:
  - `GetPartyMembers()` → current party
  - `RecruitNPC(npcId)` → add to party
  - `DismissNPC(npcId)` → remove from party
  - Party members act automatically in combat

---

#### Crafting System
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Work**:
  - Crafting stations (forge, alchemy table, enchanting altar)
  - Material system and gathering
  - Recipe discovery and learning
  - Item creation and enhancement
  - Quality levels and randomization
- **Dependencies**: Skills System (crafting skills)
- **API Impact**:
  - `GetRecipes(craftingType)` → known recipes
  - `CraftItem(recipeId, materials)` → create item
  - `EnchantItem(itemId, enchantmentId)` → add enchantment

---

### Polish & Presentation

#### Audio System
- **Status**: `[Not Started]`
- **Effort**: Low (3-5 days)
- **Work**:
  - Background music per location type
  - Combat music transitions
  - Sound effects for actions (attacks, spells, loot)
  - Volume controls and settings
- **Note**: NAudio library already installed
- **API Impact**: Audio plays automatically based on game state

---

#### Visual Enhancements
- **Status**: `[Not Started]`
- **Effort**: Medium (1-2 weeks)
- **Work**:
  - ASCII art for locations
  - Boss portraits
  - Combat visual effects (damage numbers, hit animations)
  - Screen transitions and fade effects
- **Note**: Console-only, Godot UI will replace eventually
- **API Impact**: Enhanced console output (backward compatible)

---

### Community & Longevity

#### Online & Community Features
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Work**:
  - Global leaderboards (Hall of Fame rankings)
  - Daily/weekly challenges
  - Save file sharing and showcase
  - Community events
- **Note**: Requires backend service (scope expansion)

---

#### Quality of Life Enhancements
- **Status**: `[Not Started]`
- **Effort**: Medium (2-3 weeks)
- **Work**:
  - Command history and undo
  - Keybind customization
  - Quick-save hotkey
  - Tutorial system for new players
  - Context-sensitive hints
- **API Impact**: Improved console commands and shortcuts

---

#### Modding Support
- **Status**: `[Not Started]`
- **Effort**: High (3-4 weeks)
- **Work**:
  - Mod loader system
  - Plugin architecture
  - Content creation documentation
  - Scripting API
  - Community mod repository
- **Note**: Requires careful architecture planning

---

## 🎨 UI Technology Evolution
**Status**: `[Out of Scope for Console]`

**Note**: Console game is backend only. Godot UI integration is a separate project that will consume our service APIs.

**When ready for Godot**:
- All game logic remains in C# console backend
- Godot frontend calls our exposed APIs
- Backend runs as service/process
- Communication via IPC or local networking

**This is not on the roadmap** - we design APIs to be UI-agnostic

---

## 📊 Legend

### Status Markers
- `[Finished]` - Complete and working
- `[In Progress]` - Actively being developed
- `[Partially Done - X%]` - Some work exists, needs completion  
- `[Not Started]` - Not yet begun
- `[Rebuild Required]` - Existing code wrong approach, needs redesign
- `[Needs Audit]` - Existing code unclear, needs investigation
- `[Needs Design]` - Requires design document before implementation
- `[SYSTEM GROUP]` - Multiple interconnected features that must be developed together
- `[Out of Scope]` - Not part of console game project

### Effort Estimates
- **Low**: 2-5 days
- **Medium**: 1-2 weeks
- **High**: 2-4 weeks
- **Very High**: 4+ weeks

### Impact Levels
- **HIGH**: Critical for core gameplay loop
- **MEDIUM**: Enhances gameplay significantly
- **LOW**: Polish and quality of life

---

## 🗺️ Recommended Work Order

1. **Quick Wins** (1-2 weeks total)
   - LocationGenerator Integration
   - Quest Service Connections
   - Shop Service API

2. **Progression System Group** (6-8 weeks total)
   - Design phase for all three systems
   - Skills System implementation
   - Abilities System implementation
   - Magic & Spell System implementation

3. **Quest Completion** (3-4 weeks total)
   - Quest Service API
   - Boss Encounters

4. **Location Depth** (5-7 weeks total)
   - Location-Specific Content
   - Town Mechanics
   - Dungeon Multi-Room

5. **Combat Depth** (5-7 weeks total)
   - Status Effects System
   - Trait Effects Integration

6. **Feature Expansion** (ongoing)
   - Social systems, crafting, polish, etc.

---

**See Also**:
- [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) - Current detailed status
- [GDD-Main.md](GDD-Main.md) - Game design documentation
- [REALMFORGE.md](REALMFORGE.md) - Content editing tool documentation
