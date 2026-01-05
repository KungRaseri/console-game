# Game Design Document - Console RPG

**Project**: Console RPG  
**Version**: 1.5  
**Last Updated**: December 24, 2025  
**Engine**: .NET 9.0 Console Application  
**Architecture**: Vertical Slice + CQRS Pattern  

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Game Overview](#game-overview)
3. [Core Systems](#core-systems)
4. [Game Features](#game-features)
5. [Technical Architecture](#technical-architecture)
6. [Content & Progression](#content--progression)
7. [User Interface](#user-interface)
8. [ContentBuilder - Data Editor Tool](#contentbuilder---data-editor-tool)
9. [Future Roadmap](#future-roadmap)

---

## Executive Summary

**Console RPG** is a feature-rich, turn-based role-playing game built as a .NET Core console application. It combines classic RPG mechanics (character creation, combat, inventory, quests) with modern software architecture patterns (CQRS, Vertical Slice, MediatR event-driven design).

### Key Highlights

- **D20 System**: Six-attribute system (STR, DEX, CON, INT, WIS, CHA) with derived stats
- **6 Character Classes**: Warrior, Rogue, Mage, Cleric, Ranger, Paladin
- **Turn-Based Combat**: Tactical combat with dodge, critical hits, blocking, and item usage
- **Procedural Content**: Random enemies, loot, and NPCs using Bogus library
- **Rich UI**: Spectre.Console for colorful tables, progress bars, and interactive menus
- **Persistent State**: LiteDB for save/load functionality with auto-save
- **Quest System**: Main quest chain with 6 major quests and achievement tracking
- **Difficulty Modes**: 5 difficulty levels including permadeath and Apocalypse mode
- **New Game+**: Replay with bonus stats and retained achievements

### Development

This game features a modern software architecture with vertical slice organization, CQRS pattern, and event-driven design using MediatR. A companion **ContentBuilder** WPF tool provides a visual editor for game data.

**For current implementation status, test counts, and feature completion percentages, see IMPLEMENTATION_STATUS.md.**

---

## Game Overview

### Genre & Theme

**Genre**: Turn-based RPG, Dungeon Crawler  
**Theme**: High Fantasy Adventure  
**Tone**: Epic quest with lighthearted moments  
**Target Audience**: RPG enthusiasts, retro gaming fans, developers learning software architecture

### Game Concept

Players create a character, explore locations, battle enemies, complete quests, collect loot, and build their character through leveling and equipment. The game features a main quest chain leading to a climactic final battle, with optional achievements and New Game+ for replayability.

### Victory Conditions

1. **Complete Main Quest Chain**: Finish all 6 main quests culminating in "The Final Confrontation"
2. **Defeat the Final Boss**: Win the climactic battle
3. **Victory Celebration**: Dramatic victory sequence with statistics and rewards
4. **Optional**: Unlock all achievements, reach max level (50), or complete Apocalypse mode

---

## Core Systems

### 1. Character System

#### Attributes (D20 System)

Six primary attributes determine character capabilities:

| Attribute | Abbr | Affects |
|-----------|------|---------|
| **Strength** | STR | Melee damage, carrying capacity |
| **Dexterity** | DEX | Dodge chance, initiative, ranged damage |
| **Constitution** | CON | Max HP, damage resistance |
| **Intelligence** | INT | Magic damage, mana pool |
| **Wisdom** | WIS | Mana regeneration, magic defense |
| **Charisma** | CHA | Shop prices, NPC interactions |

**Derived Stats:**
- **Health Points (HP)**: Base 100 + (CON × 5)
- **Mana Points (MP)**: Base 50 + (INT × 3)
- **Attack Bonus**: STR or DEX modifier
- **Defense**: Base 10 + DEX modifier
- **Dodge Chance**: DEX × 2%
- **Critical Hit Chance**: Base 5%

#### Character Classes

Six unique classes with distinct playstyles and bonuses:

| Class | Primary Stat | Bonuses | Starting Equipment |
|-------|-------------|---------|-------------------|
| **Warrior** | STR | +2 STR, +1 CON | Iron Sword, Leather Armor, Small Shield |
| **Rogue** | DEX | +2 DEX, +1 WIS | Iron Dagger, Leather Armor, Lockpicks |
| **Mage** | INT | +2 INT, +1 WIS | Wooden Staff, Cloth Robes, Mana Potion |
| **Cleric** | WIS | +2 WIS, +1 CHA | Wooden Mace, Chain Mail, Holy Symbol |
| **Ranger** | DEX | +1 DEX, +1 WIS, +1 STR | Shortbow, Leather Armor, Arrows (50) |
| **Paladin** | STR/CHA | +1 STR, +1 CON, +1 CHA | Longsword, Chain Mail, Kite Shield |

#### Character Creation

1. **Name Entry**: Player chooses character name (validated)
2. **Class Selection**: Interactive menu of 6 classes with descriptions
3. **Attribute Allocation**: 27 points auto-distributed with class bonuses applied
4. **Starting Equipment**: Class-specific gear and items
5. **Starting Gold**: 100 gold pieces

### 2. Combat System

#### Turn-Based Combat

Combat is turn-based with 4 player actions per turn:

| Action | Description | Mechanics |
|--------|-------------|-----------|
| **Attack** | Strike the enemy | Base damage + STR/DEX modifier, chance to crit (2x damage) |
| **Defend** | Reduce incoming damage | +50% defense for 1 turn, chance to block (negate damage) |
| **Use Item** | Consume an item | Health/Mana potions restore HP/MP in combat |
| **Flee** | Attempt to escape | Success based on DEX, fails if enemy too fast |

#### Combat Mechanics

**Damage Formula:**
```
Base Damage = Weapon Damage + Attribute Modifier
Final Damage = Base Damage - (Enemy Defense × 0.5)
Critical Hit = Base Damage × 2
```

**Dodge Mechanics:**
- Dodge Chance = DEX × 2%
- Dodged attacks deal 0 damage
- Cannot dodge while defending

**Critical Hits:**
- Base 5% crit chance
- Deals double damage
- Visual feedback with "[red]CRITICAL HIT![/]"

**Blocking (Defend Action):**
- 40% chance to block when defending
- Blocked attacks deal 0 damage
- Defense bonus (+50%) applies to non-blocked attacks

#### Enemy System

Enemies are procedurally generated with:
- **Level-based scaling**: Enemy level = Player level ± 1
- **Traits**: Random traits affecting behavior and stats
- **Loot tables**: Gold and item drops based on difficulty
- **Boss modifier**: 3x HP, 1.5x damage for boss encounters

**Enemy Types** (via Trait System):
- Aggressive, Defensive, Cowardly, Berserker, Tank, Glass Cannon
- Elemental (Fire, Ice, Lightning, Poison)
- Special traits (Regeneration, Thorns, Life Steal, Evasive)

### 3. Inventory System

#### Item Management

- **Capacity**: 20 item slots
- **Equipment Slots**: Weapon, Armor, Shield, Accessory
- **Consumables**: Health/Mana potions usable in and out of combat
- **Sorting**: By name, type, or rarity

#### Item Categories

| Category | Types | Examples |
|----------|-------|----------|
| **Weapons** | Sword, Axe, Dagger, Staff, Bow, Mace | Iron Sword, Flaming Axe, Enchanted Staff |
| **Armor** | Light, Medium, Heavy, Robes | Leather Armor, Chain Mail, Plate Armor |
| **Shields** | Small, Medium, Large, Tower | Wooden Shield, Kite Shield, Tower Shield |
| **Consumables** | Potions, Scrolls, Food | Health Potion, Mana Potion, Scroll of Fireball |
| **Accessories** | Rings, Amulets, Charms | Ring of Protection, Amulet of Wisdom |

#### Item Rarity System

Five rarity tiers with increasing power and value:

| Rarity | Color | Drop Rate | Value Multiplier |
|--------|-------|-----------|------------------|
| **Common** | Gray | 50% | 1× |
| **Uncommon** | Green | 30% | 2× |
| **Rare** | Blue | 15% | 4× |
| **Epic** | Purple | 4% | 8× |
| **Legendary** | Gold | 1% | 16× |

#### Trait System

Items have **traits** that provide gameplay bonuses:

**Weapon Traits:**
- Damage bonuses (+5 Fire Damage, +10% Attack)
- Special effects (Life Steal, Mana Burn, Poison)
- Elemental affinity (Fire, Ice, Lightning)

**Armor Traits:**
- Defense bonuses (+5 Defense, +10% Damage Resistance)
- Resistances (Fire Resistance 20%, Poison Immunity)
- Stat bonuses (+2 STR, +5 HP Regeneration)

**Enchantments:**
- Prefix modifiers (Flaming, Frozen, Masterwork)
- Suffix modifiers (of Fire, of the Bear, of Swiftness)
- Combined for powerful items (Flaming Sword of the Dragon)

#### Procedural Name Generation System (v4.0)

**Pattern-Based Architecture:**
- Names generated from **patterns** (templates with tokens)
- Component tokens: `{base}`, `{prefix}`, `{suffix}`, `{quality}`, `{descriptive}`
- External reference tokens: `[@materialRef/weapon]`, `[@materialRef/armor]`
- Weighted random selection for variety and rarity
- **100% data-driven** - no hardcoded item names

**Pattern Syntax (v4.0 Standard):**
- Component tokens use curly braces: `{base}`, `{prefix}`
- External references use square brackets: `[@materialRef/weapon]`
- All patterns use `rarityWeight` for selection probability
- NO "example" fields allowed in JSON files

**Pattern Examples:**
- Simple: `{base}` → "Longsword"
- Material: `[@materialRef/weapon] {base}` → "Steel Longsword"
- Complex: `{prefix} [@materialRef/weapon] {base} {suffix}` → "Ancient Mithril Longsword of Fire"

**Services:**
- **PatternExecutor**: Parses patterns, resolves tokens, applies weighted selection
- **DataReferenceResolver**: Resolves cross-file references (materials, items, enemies)
- **ComponentValue**: Record type for weighted component selection

**Features:**
- Context-aware material filtering (weapon vs armor materials)
- Dynamic component resolution from JSON files
- Trait merging from resolved references
- Automatic default `{base}` pattern for all catalogs
- Supports trait inheritance from references

**JSON Standards Compliance (v4.0):**
- All 30 names.json files follow NAMES_JSON_STANDARD.md
- All 28 catalog.json files follow CATALOG_JSON_STANDARD.md
- All 33 .cbconfig.json files follow CBCONFIG_STANDARD.md
- Standards documented in `docs/standards/json/`
- 100% compliance achieved December 27, 2025

**Testing:**
- 18 comprehensive unit tests (PatternExecutor + DataReferenceResolver)
- Tests cover token parsing, reference resolution, weighted selection, error handling
- All pattern generation logic fully tested

### 4. Progression System

#### Experience & Leveling

- **Level Cap**: 50
- **XP Curve**: Exponential (100 XP for level 2, scaling up)
- **XP Sources**: Combat victories, quest completion, exploration
- **Level-Up Rewards**: +5 HP, +3 MP, +3 attribute points, skill selection

#### Attribute Allocation

On level-up, players receive **3 attribute points** to distribute freely among the 6 attributes. This allows for:
- Class specialization (pure Warrior focuses STR/CON)
- Hybrid builds (Battle Mage with STR + INT)
- Balanced characters (jack-of-all-trades)

#### Skills System (Passive Progression)

**Design Intent**: Players learn passive skills that provide permanent stat bonuses, creating diverse build options.

**Planned Skills:**

| Skill | Level Req | Effect |
|-------|-----------|--------|
| **Power Strike** | 3 | +20% melee damage |
| **Precision Shot** | 3 | +15% ranged damage, +5% crit chance |
| **Toughness** | 5 | +25% max HP |
| **Arcane Focus** | 5 | +30% spell damage |
| **Evasion** | 7 | +10% dodge chance |
| **Fortitude** | 7 | +15% damage resistance |
| **Lucky Strike** | 10 | +10% critical hit chance |
| **Mana Mastery** | 10 | +50% max MP, +20% mana regen |

**Mechanics:**
- Players select 1 skill per level-up
- Skills have ranks (1-5) that can be upgraded
- Skill bonuses stack with attribute bonuses
- Skills provide passive, always-active benefits

#### Abilities System (Active Powers)

**Design Intent**: Characters learn active abilities that can be used in combat or exploration.

**Ability Types:**
- **Active/Offensive**: Attack abilities (Fireball, Power Attack, Backstab)
- **Active/Defensive**: Defensive abilities (Shield Bash, Dodge Roll, Parry)
- **Active/Support**: Buffs and utility (Heal, Bless, Haste)
- **Passive**: Always-on effects (Regeneration, Thorns, Aura)
- **Ultimate**: Powerful special moves with cooldowns

**Mechanics:**
- Abilities cost mana to activate
- Abilities have cooldowns preventing spam
- Abilities may require specific items (staves for spells)
- Classes start with signature abilities
- New abilities unlock at specific levels

**Examples:**
- Warrior: Power Strike, Shield Bash, Whirlwind Attack
- Mage: Fireball, Ice Lance, Chain Lightning
- Rogue: Backstab, Shadow Step, Poison Dagger
- Cleric: Heal, Holy Smite, Divine Shield

### 5. Quest System

The quest system provides structured objectives for players to pursue, ranging from a core main quest chain to procedurally generated side quests. Completing quests grants rewards (XP, gold, items) and drives narrative progression.

#### Quest Categories

**Main Quest Chain:**
- Linear story-driven quests forming the core narrative
- Sequential prerequisites (must complete Quest A to unlock Quest B)
- Culminates in a final confrontation that grants victory
- Provides time bonuses in Apocalypse mode

**Side Quests:**
- Optional quests for additional rewards and world-building
- Procedurally generated from quest templates
- Can be accepted and completed in any order
- Provides variety and replay value

**Daily/Repeatable Quests:** *(Future)*
- Refresh daily for consistent engagement
- Lower rewards but can be repeated

#### Quest Types

The system supports diverse objective types to create varied gameplay:

**Investigation Quests:**
- Gather clues (3-8 clues depending on difficulty)
- Track targets across multiple locations
- Optional combat encounters
- May have multiple endings based on player choices

**Fetch Quests:**
- Retrieve specific items (herbs, artifacts, lost belongings)
- Quantity varies by difficulty (5-20 items for easy, fewer for rare items)
- Item types: herbs, relics, documents, supplies
- May require exploring specific location types

**Kill Quests:**
- Eliminate specific enemy types (goblins, undead, bandits, dragons)
- Target count scales with difficulty (3-15 enemies)
- Location hints guide players to enemy spawns
- Boss quests require defeating unique enemies

**Escort Quests:**
- Protect NPCs traveling between locations
- Difficulty based on distance and danger level
- NPCs may provide dialogue or lore during journey
- Failure if NPC dies

**Delivery Quests:**
- Transport items/messages between locations
- Time limits for urgent deliveries
- Stealth/speed challenges for high-difficulty versions

#### Quest Structure

**Prerequisites:**
- Minimum level requirements
- Completed previous quests (for chains)
- Reputation thresholds *(Future)*
- Owned items *(Future)*

**Objective Tracking:**
- Primary objectives (required for completion)
- Secondary objectives (optional bonus rewards)
- Hidden objectives (discovered during gameplay, unlock secrets)
- Real-time progress updates (kill counters, collection tracking)

**Quest States:**
- Available (meets prerequisites, not yet accepted)
- Active (accepted and in progress)
- Completed (all objectives finished)
- Failed (time limit expired or critical failure) *(Future)*

**Difficulty Tiers:**
- Easy (levels 1-10, low rewards)
- Medium (levels 10-20, uncommon/rare rewards)
- Hard (levels 20-30, rare/epic rewards)
- Very Hard (levels 30-40, epic/legendary rewards)
- Epic/Legendary (levels 40+, legendary/mythic rewards)

#### Reward System

**Gold Rewards:**
- 9 tiers (trivial to ancient: 10g - 10,000g base)
- Scales with player level: `final_amount = base_amount × (1 + player_level × 0.05)`
- Matched to quest difficulty

**Experience Rewards:**
- 9 tiers (trivial to ancient: 50 XP - 50,000 XP base)
- Scales with level and difficulty: `final_xp = base_xp × difficulty_multiplier × (1 + player_level × 0.1)`
- Maintains progression balance across level ranges

**Item Rewards:**
- Equipment (weapons, armor, accessories)
- Consumables (potions, scrolls, materials)
- Unique quest items (progression unlocks)
- Rarity matches quest difficulty (easy = common/uncommon, hard = epic/legendary)
- Item level scales near player level (±2 levels)

**Special Rewards:**
- Apocalypse time bonuses (5-60 minutes)
- Achievement unlocks
- Reputation gains *(Future)*
- Skill points *(Future)*

#### Quest Integration

**Quest Givers:**
- NPCs in towns/locations offer quests
- Quest giver types: common folk, merchants, military, nobles, professionals
- Quest type matches NPC background (merchants give fetch, military gives kill)

**Location Integration:**
- Quest objectives reference location types (towns, dungeons, wilderness)
- Location danger/difficulty matched to quest tier
- Location hints guide players ("forests", "graveyards", "dragon-lair")

**Combat Integration:**
- Kill objectives track specific enemy types
- Combat during investigation/escort quests
- Boss encounters for story quests

**Inventory Integration:**
- Fetch quests require inventory space
- Delivery quests place items in inventory
- Item rewards automatically granted on completion

#### Quest UI

**Quest Journal:** *(Future)*
- View active quests with progress
- Read quest descriptions and lore
- Track primary/secondary/hidden objectives
- Mark quests for tracking

**Quest Markers:** *(Future)*
- Show relevant locations on map
- Update as objectives progress
- Distinguish between main/side quests

**Completion Notifications:**
- Display rewards earned
- Show objective completion in real-time
- Celebrate quest chain completion

### 6. Exploration System

The exploration system provides diverse locations for players to discover and traverse. Locations vary in purpose (towns provide services, dungeons provide combat/loot, wilderness provides random encounters) and difficulty (scaled to player level or fixed challenge rating).

#### Location Types

**Towns:**
- Safe zones where players can access services
- No random enemy encounters
- NPCs provide quests, shops, lore, and social interactions
- Resting/healing available at inns
- Procedurally generated towns with unique names
- Hand-crafted story-critical towns (Hub Town, capital cities)

**Dungeons:**
- Multi-room exploration with increasing difficulty
- Combat encounters in most rooms
- Treasure rooms with rare loot
- Boss encounters in final rooms
- Procedurally generated layouts from templates
- Hand-crafted dungeons for story quests
- May have environmental hazards (traps, darkness, poison gas)

**Wilderness:**
- Open areas connecting towns and dungeons
- Random encounter rate (varies by location danger level)
- Resource gathering opportunities *(Future)*
- Dynamic weather affecting gameplay *(Future)*
- Multiple biomes (forests, mountains, deserts, swamps)

**Points of Interest:** *(Future)*
- Unique, discoverable locations with lore
- One-time special encounters
- Hidden treasure caches
- Lore books and environmental storytelling

#### Exploration Mechanics

**Location Discovery:**
- Locations start hidden until discovered
- Discovery through exploration, quests, or NPC hints
- First visit grants XP bonus
- Discovered locations added to map

**Fast Travel:**
- Return instantly to previously visited locations
- Only accessible from safe zones (towns, camp sites)
- Cannot fast travel during combat or quests
- May cost gold *(Future)* or have cooldown

**Location Levels:**
- Locations have fixed or dynamic difficulty levels
- Enemy spawns match location level (±2 levels)
- Loot quality matches location level
- Low-level players warned when entering high-level areas

**Location States:**
- Undiscovered (hidden, not yet found)
- Discovered (found, can visit)
- Cleared (dungeon boss defeated, reduced encounter rate)
- Active Quest (location relevant to active quest)
- Locked (requires key, quest completion, or level) *(Future)*

#### Location Features

**Enemy Spawns:**
- Location defines enemy types that can appear
- Spawn probability per enemy type
- Boss spawns in specific locations
- Combat frequency varies by location type (towns 0%, dungeons 80%, wilderness 40-60%)

**Loot Distribution:**
- Location defines loot tables
- Rare items in dangerous locations
- Common supplies in towns/safe areas
- Unique items in story locations

**Environmental Properties:**
- Weather conditions (clear, rain, fog, snow) *(Future)*
- Lighting levels (bright, dim, dark) affecting stealth *(Future)*
- Temperature (hot, cold, moderate) affecting stamina *(Future)*
- Danger rating (safe, low, moderate, high, extreme)

**NPC Presence:**
- Towns populated with merchants, quest givers, trainers
- Dungeons may have hostile NPCs (bandits, cultists)
- Wilderness may have travelers, hermits, or wandering merchants
- NPC schedules *(Future)* - appear at different times

#### Town Services

**Shops:**
- Buy and sell items
- Shop inventory based on town tier and merchant type
- Blacksmith (weapons/armor), Alchemist (potions), General Store (supplies)
- Prices affected by player Charisma attribute

**Inns:**
- Rest to fully restore HP/MP
- Save game at inn
- Rumors and lore from patrons
- May offer temporary buffs (well-rested bonus) *(Future)*

**Quest Hubs:**
- NPCs offer quests appropriate to player level
- Quest boards for procedural quests *(Future)*
- Completed quests turn in to quest givers

**Trainers:** *(Future)*
- Teach new abilities
- Respec attribute points (fee required)
- Provide skill training

**Crafting Stations:** *(Future)*
- Blacksmith forges (weapon/armor crafting)
- Alchemy labs (potion brewing)
- Enchanting altars (item enhancement)

#### Dungeon Systems

**Room Types:**
- Combat rooms (enemy encounters)
- Treasure rooms (loot chests, no enemies)
- Rest rooms (safe zone mid-dungeon, limited use)
- Puzzle rooms (challenges requiring wit, not combat) *(Future)*
- Boss rooms (final challenge, unique rewards)

**Multi-Room Progression:**
- Navigate through 5-15 rooms depending on dungeon size
- Map reveals explored rooms *(Future)*
- Cannot backtrack once room cleared (forward-only progression)
- Escape scrolls allow immediate exit *(Future)*

**Dungeon Difficulty:**
- Easy dungeons: 5-7 rooms, common/uncommon loot, level-appropriate enemies
- Medium dungeons: 8-10 rooms, rare loot, +2 level enemies
- Hard dungeons: 11-15 rooms, epic loot, +5 level enemies, boss required
- Legendary dungeons: 15+ rooms, legendary loot, +10 level enemies, multiple bosses

**Dungeon Generation:**
- Procedural generation from room templates
- Ensures path to boss room exists
- Treasure room placement varies (1-3 per dungeon)
- Room layout uses branching paths or linear progression

#### Wilderness Encounters

**Random Events:**
- Combat encounters (60% of wilderness exploration)
- Peaceful encounters (NPCs, travelers, merchants)
- Resource nodes (gather herbs, mine ore) *(Future)*
- Hidden caches (treasure chests off the beaten path)
- Ambushes (higher-level enemies surprise attack) *(Future)*

**Biome-Specific Features:**
- Forests: Abundant herbs, animal enemies, thick undergrowth
- Mountains: Ore nodes, flying enemies, cliff hazards
- Deserts: Heat exhaustion risk, scorpion enemies, oases
- Swamps: Poison hazards, undead enemies, disease risk
- Tundra: Cold damage, ice enemies, limited visibility

**Encounter Scaling:**
- Wilderness difficulty based on distance from safe zones
- Near towns: Low-level, common enemies
- Remote areas: High-level, elite enemies
- Unknown regions: Unpredictable, mixed difficulty

#### Location-Quest Integration

**Quest Objectives:**
- Quests direct players to specific locations
- "Explore [Location]" objectives mark location as quest target
- "Kill enemies in [Location]" require visiting and combat
- "Deliver item to [Location]" require travel and NPC interaction

**Location Hints:**
- Quest descriptions provide location type hints (forests, graveyards, dragon-lair)
- NPCs give directions to quest locations
- Location discovery may complete exploration objectives

**Dynamic Events:** *(Future)*
- Locations change state based on quest progress
- Clearing dungeons reduces enemy respawns
- Completing town quests improves services
- Failed quests may alter location availability

### 7. Achievement System

**6 Achievements** tracking player milestones:

| Achievement | Requirement | Unlocks |
|-------------|-------------|---------|
| **First Blood** | Defeat your first enemy | Badge, 50 bonus XP |
| **Survivor** | Reach level 10 | Title: "The Survivor" |
| **Dragon Slayer** | Defeat a boss enemy | Badge, bonus loot modifier |
| **Treasure Hunter** | Collect 1000 gold | Access to secret shop |
| **Master Explorer** | Visit all locations | Map completion bonus |
| **Champion** | Complete main quest | Victory screen, New Game+ |

Achievements persist across saves and New Game+ playthroughs.

### 8. Difficulty System

**5 Difficulty Modes** catering to different player preferences:

| Difficulty | Enemy Stats | Death Penalty | Special Rules |
|------------|-------------|---------------|---------------|
| **Casual** | 70% HP/Damage | Respawn with full HP | Extra save slots |
| **Normal** | 100% HP/Damage | Lose 50% gold | Standard experience |
| **Hard** | 130% HP/Damage | Lose 75% gold, 1 random item | Higher XP rewards |
| **Nightmare** | 150% HP/Damage | Permadeath (New Game) | 2× XP, better loot |
| **Apocalypse** | 200% HP/Damage | Permadeath + Time Limit | 3× XP, legendary drops, 60-minute timer |

**Apocalypse Mode** adds a countdown timer:
- Starts at 60 minutes
- Bonus time from quests (+5 to +60 minutes)
- Game over when timer hits 0
- UI displays remaining time in red/yellow/green

### 9. Death System

#### Standard Modes (Casual, Normal, Hard)

- **Death Screen**: Displays stats and "You Died" message
- **Respawn**: Character revives with full HP/MP
- **Penalties**: 
  - Casual: No penalty
  - Normal: Lose 50% gold
  - Hard: Lose 75% gold + 1 random equipped item

#### Permadeath Modes (Nightmare, Apocalypse)

- **Permanent Death**: Character deleted
- **Hall of Fame**: Character added to leaderboard
- **Dropped Items**: Items left on ground for new characters
- **New Game Required**: Must create new character

#### Hall of Fame

Tracks top 10 characters with:
- Character name, class, level
- Total gold earned, enemies defeated
- Quests completed, achievements unlocked
- Cause of death, final location
- Difficulty mode, play time

### 10. Save/Load System

#### Save Game Features

- **Auto-Save**: After combat victories, quest completion, major actions
- **Manual Save**: Player-initiated saves
- **Multiple Characters**: Unlimited save slots
- **Save Data**: Character, inventory, quests, achievements, statistics
- **Persistence**: LiteDB database (savegames.db)

#### Save Game Contents

```csharp
SaveGame {
    CharacterName, Class, Level
    Attributes (STR, DEX, CON, INT, WIS, CHA)
    HP, MaxHP, Mana, MaxMana
    Experience, Gold
    InventoryItems, EquippedItems
    Skills, LearnedSkills
    ActiveQuests, CompletedQuests
    Achievements, UnlockedAchievements
    Statistics (kills, deaths, gold earned, etc.)
    CurrentLocation, KnownLocations
    Difficulty, IsNewGamePlus
    Timestamp
}
```

### 11. New Game+ System

After completing the main quest, players can start **New Game+**:

**Bonuses:**
- +5 to all attributes
- +50 HP, +30 MP
- Starting gold: 500 (vs. 100)
- Achievements carry over
- Hall of Fame entry retained

**Challenges:**
- Enemies scale to player level +2
- Quest requirements increased
- New difficulty tiers unlocked

**Rewards:**
- Access to legendary items earlier
- Bonus XP multiplier (1.5×)
- Unique "NG+ Veteran" title

---

## Game Features

## Game Features

This section describes the **intended feature set** for the complete game. For the current implementation status of each feature, see [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md).

### Core Gameplay

- **Character Creation**: 6 unique classes with distinct playstyles and starting equipment
- **Turn-Based Combat**: Tactical combat with attack, defend, use item, and flee options
- **Leveling System**: XP-based progression with attribute allocation on level-up
- **Skills & Abilities**: Passive skills (stat bonuses) and active abilities (combat powers)
- **Save/Load**: Persistent game state with auto-save and multiple save slots

### Content Systems

- **Procedural Enemies**: Level-scaled enemies with traits and special behaviors
- **Item Generation**: 5 rarity tiers (Common to Legendary) with procedural generation
- **Trait System**: Custom properties for items and enemies affecting gameplay
- **Equipment System**: 4 equipment slots (Weapon, Armor, Shield, Accessory)
- **Inventory Management**: 20-slot inventory with sorting and consumable items
- **Consumables**: Health and mana potions usable in and out of combat

### Progression

- **Main Quest Chain**: 6 major story quests forming the core narrative
- **Achievement System**: 6 achievements tracking player milestones
- **Difficulty Modes**: 5 difficulty levels (Casual to Apocalypse)
- **Permadeath**: Optional permanent death in Nightmare/Apocalypse modes
- **Hall of Fame**: Leaderboard tracking top characters
- **New Game+**: Replay with bonus stats and retained achievements

### UI/UX

- **Rich Console UI**: Colorful interface using Spectre.Console library
- **Interactive Menus**: Keyboard navigation for all choices
- **Combat Log**: Detailed combat feedback with colored text
- **Progress Indicators**: Health bars, XP bars, and timers
- **Data Tables**: Inventory, stats, and leaderboard displays
- **Victory Sequence**: Dramatic celebration with statistics and rewards

### Technical Architecture

- **CQRS Pattern**: Command/Query separation for clean business logic
- **Vertical Slice**: Feature-focused organization reducing coupling
- **Event-Driven**: MediatR for decoupled event handling
- **Input Validation**: FluentValidation for robust user input handling
- **Structured Logging**: Serilog for debugging and diagnostics
- **Resilience**: Polly for retry logic and error handling
- **Comprehensive Testing**: Unit tests covering core gameplay systems

### Development Tools

#### ContentBuilder - Game Data Editor

**ContentBuilder** is a WPF desktop application providing visual editing of game data files:

**Core Features:**
- **Pattern Composer**: Visual token-based pattern creation with dynamic UI
- **Reference Browser**: Navigate cross-file dependencies using v4.1 reference system
- **Dynamic Component UI**: Adapts interface based on file type and structure
- **Real-Time Preview**: See generated examples as you edit patterns
- **Material Catalog Editor**: Manage materials with trait properties
- **Quest Editor**: Design quests with objectives and rewards (v4.2 standard)
- **NPC Name Editor**: Create NPCs with social classes and traits
- **Data Validation**: FluentValidation ensures data integrity
- **Readonly Patterns**: Protects default patterns ({base}) from accidental edits

**Technology Stack:**
- MVVM architecture (CommunityToolkit.Mvvm)
- Material Design UI (MaterialDesignThemes)
- JSON v4.0 + v4.1 reference system compliance
- Real-time syntax highlighting
- Integrated file validation

### Future Roadmap

These are potential features being considered for future development. Priority and scope may change based on player feedback and development resources.

#### Content Expansion
- Side quests (10-20 optional quests for deeper world-building)
- Boss encounters (unique, challenging enemies with special mechanics)
- Dungeons (multi-room procedural exploration with puzzles)
- Towns (shops, NPCs, rest areas, and services)
- Crafting system (combine materials to create items)
- Enchanting system (modify existing items with magical properties)

#### Gameplay Features
- Party system (recruit NPCs to join the player in combat)
- Magic/spell system (expanded spellcasting beyond basic abilities)
- Status effects (poison, burning, frozen, stunned, etc.)
- Weather system (rain, snow, fog affecting visibility and combat)
- Day/night cycle (NPC schedules, enemy spawn patterns)
- Reputation system (faction relationships affecting interactions)

#### Audio
- Background music (NAudio integration for atmospheric soundtracks)
- Sound effects (combat impacts, UI feedback, environmental sounds)
- Dynamic music (changes based on location and combat state)
- Audio settings (volume control, mute options)

#### UI Enhancements
- Quest journal interface (track active/completed quests)
- World map visualization (navigate discovered locations)
- Combat animations (ASCII art effects for abilities)
- FigletText title screens (large ASCII art branding)
- Minimap for dungeons (show explored rooms and current position)
- Tooltip system (hover over items for detailed information)

#### Online Features
- Global leaderboards (compare scores with other players)
- Daily challenges (randomized objectives with special rewards)
- Save file sharing (export/import characters)
- Community events (seasonal content updates)

---

## Technical Architecture

### Architecture Pattern: Vertical Slice + CQRS

The game uses **Vertical Slice Architecture** where code is organized by **business capability** (feature) rather than technical layer. Each feature is self-contained with all necessary components.

#### Feature Structure

```
Game/Features/[FeatureName]/
├── Commands/           ← Write operations (change state)
│   ├── XxxCommand.cs       ← Command definition (record)
│   └── XxxHandler.cs       ← Command handler (IRequestHandler)
├── Queries/            ← Read operations (no side effects)
│   ├── XxxQuery.cs         ← Query definition (record)
│   └── XxxHandler.cs       ← Query handler (IRequestHandler)
├── XxxService.cs       ← Business logic
└── XxxOrchestrator.cs  ← UI workflow (optional)
```

#### Benefits

✅ **Feature Cohesion**: All code for a feature lives together  
✅ **Clear Separation**: Commands (write) vs Queries (read)  
✅ **Easy Navigation**: Find all code for "Combat" in `Features/Combat/`  
✅ **Testability**: Mock dependencies at feature boundary  
✅ **Scalability**: Add features without modifying existing code  

### Implemented Features (Vertical Slices)

1. **CharacterCreation** - Character creation workflow
2. **Combat** - Turn-based combat mechanics
3. **Inventory** - Item management and equipment
4. **Exploration** - Location exploration and travel
5. **SaveLoad** - Save/load game state
6. **Quest** - Quest tracking and completion
7. **Achievement** - Achievement unlocking
8. **Victory** - Victory screen and New Game+
9. **Death** - Death handling and Hall of Fame

### Technology Stack

#### Core Framework
- **.NET 9.0** - Latest .NET runtime
- **C# 13** - Modern C# language features

#### Libraries & Dependencies

| Library | Version | Purpose |
|---------|---------|---------|
| **Spectre.Console** | 0.54.0 | Rich console UI (colors, tables, menus) |
| **Spectre.Console.Cli** | 0.53.1 | Command-line argument parsing |
| **MediatR** | 14.0.0 | CQRS/event-driven architecture |
| **FluentValidation** | 12.1.1 | Input validation with rules |
| **Serilog** | 4.3.0 | Structured logging framework |
| **Polly** | 8.6.5 | Resilience patterns (retry logic) |
| **LiteDB** | 5.0.21 | Embedded NoSQL database |
| **Bogus** | 35.6.5 | Procedural content generation |
| **Humanizer** | 3.0.1 | Natural language formatting |
| **Newtonsoft.Json** | 13.0.4 | JSON serialization |
| **NAudio** | 2.2.1 | Audio playback (future) |
| **xUnit** | 2.9.3 | Unit testing framework |
| **FluentAssertions** | 8.8.0 | Test assertions |
| **MaterialDesignThemes** | 5.1.0 | WPF Material Design UI (ContentBuilder) |
| **CommunityToolkit.Mvvm** | 8.4.0 | MVVM framework (ContentBuilder) |

#### Architectural Patterns

- **CQRS**: Commands (write) separated from Queries (read)
- **Mediator Pattern**: MediatR for request/response and events
- **Repository Pattern**: Data access abstraction (LiteDB repositories)
- **Service Layer**: Business logic in feature services
- **Orchestrator Pattern**: Complex UI workflows
- **State Machine**: GameEngine game loop
- **Event Sourcing**: MediatR notifications for game events

#### Pipeline Behaviors (MediatR)

1. **ValidationBehavior**: Automatic FluentValidation before commands
2. **LoggingBehavior**: Structured logging for all requests
3. **ErrorHandlingBehavior**: Centralized exception handling

### Testing Strategy

**Total Tests**: 397 (393 passing, 4 skipped - 99.0% pass rate)

#### Test Coverage by Category

| Category | Tests | Pass | Skip | Coverage |
|----------|-------|------|------|----------|
| Settings & Validators | 83 | 83 | 0 | 100% |
| Models & Domain Logic | 77 | 77 | 0 | 100% |
| Services | 194 | 190 | 4 | 97.9% |
| Generators | 23 | 23 | 0 | 100% |
| Equipment & Traits | 25 | 25 | 0 | 100% |
| Integration Tests | 5 | 5 | 0 | 100% |

**New Test Coverage:**
- ✅ PatternExecutorTests (12 tests) - Pattern parsing and token resolution
- ✅ DataReferenceResolverTests (6 tests) - Cross-file reference resolution

#### Testing Approach

1. **Unit Tests**: All business logic, models, and services
2. **CQRS Handlers**: Commands and queries fully tested
3. **Integration Tests**: Multi-service workflows
4. **Validation Tests**: FluentValidation rules exhaustively tested

#### Skipped Tests (4 total)

The following tests are intentionally skipped because they test UI orchestration methods requiring interactive terminal input:

1. **LoadGameServiceTests** (3 tests):
   - `LoadGameAsync_Should_Return_Unsuccessful_When_No_Saves_Exist`
   - `LoadGameAsync_Should_Display_Available_Saves_When_Saves_Exist`
   - `DeleteSaveAsync_Should_Delete_Save_With_Confirmation`
   - **Reason**: Calls `ConsoleUI.ShowMenu()`, `ShowTable()`, `Confirm()` which require user interaction

2. **ExplorationServiceTests** (1 test):
   - `TravelToLocation_Should_Update_Current_Location`
   - **Reason**: Calls `ConsoleUI.ShowMenu()` for location selection

**Note**: The underlying business logic for these features IS tested:
- CQRS handlers (LoadGameCommand, DeleteSaveCommand, etc.) are tested
- Non-UI service methods are fully covered
- UI orchestration is tested manually/integration style
- Tests serve as documentation of UI-dependent methods

### Project Structure

```
RealmEngine/
├── RealmEngine.Core/                     ← Core game logic (extracted)
├── RealmEngine.Shared/                   ← Shared services (cross-project)
│   ├── Services/
│   │   ├── PatternExecutor.cs     ← Pattern-based name generation
│   │   └── DataReferenceResolver.cs ← Cross-file reference resolution
│   ├── Models/
│   │   └── GameDataModels.cs      ← JSON data structures (v4)
│   └── Events/
├── RealmEngine.Data/                     ← Data repositories and JSON files
│   └── Data/Json/                 ← Game data (items, enemies, materials)
│       ├── items/
│       │   ├── weapons/
│       │   │   ├── names.json     ← v4 pattern-based names
│       │   │   └── catalog.json   ← Weapon base stats
│       │   ├── armor/
│       │   └── materials/
│       │       └── catalog.json   ← Material properties & traits
│       ├── enemies/
│       ├── npcs/
│       └── quests/
├── RealmForge/           ← WPF data editor tool
│   ├── Views/
│   │   ├── NameListEditorView.xaml   ← Pattern/component editor
│   │   ├── ReferenceSelectorDialog.xaml ← Reference browser
│   │   └── QuestCatalogEditorView.xaml  ← Quest editor
│   ├── ViewModels/
│   ├── Models/
│   ├── Services/
│   ├── Validators/
│   └── Converters/
├── RealmForge.Tests/     ← ContentBuilder unit tests
├── Game/                          ← Legacy folder (being migrated)
│   ├── Features/                  ← Vertical slices (9 features)
│   │   ├── Achievement/
│   │   ├── CharacterCreation/
│   │   ├── Combat/
│   │   ├── Death/
│   │   ├── Exploration/
│   │   ├── Inventory/
│   │   ├── Quest/
│   │   ├── SaveLoad/
│   │   └── Victory/
│   ├── Models/                    ← Domain models (19 models)
│   │   ├── Character.cs
│   │   ├── Item.cs
│   │   ├── Quest.cs
│   │   ├── Achievement.cs
│   │   └── ...
│   ├── Settings/                  ← Configuration classes
│   │   ├── GameSettings.cs
│   │   ├── GameplaySettings.cs
│   │   ├── DifficultySettings.cs
│   │   └── ...
│   ├── Shared/                    ← Cross-cutting concerns
│   │   ├── Behaviors/             ← MediatR pipeline behaviors
│   │   ├── Services/              ← Shared services
│   │   ├── UI/                    ← UI components
│   │   └── Events/                ← Domain events
│   ├── Services/                  ← Utility/static services
│   │   ├── GameStateService.cs    ← Global game state
│   │   ├── LevelUpService.cs      ← Level-up calculations
│   │   └── ...
│   ├── Generators/                ← Procedural generation (Bogus)
│   │   ├── EnemyGenerator.cs
│   │   ├── ItemGenerator.cs
│   │   └── NpcGenerator.cs
│   ├── Validators/                ← FluentValidation validators
│   ├── Utilities/                 ← Helper classes
│   ├── Data/                      ← LiteDB repositories
│   ├── Audio/                     ← Audio management (NAudio)
│   ├── GameEngine.cs              ← Main game loop
│   ├── Program.cs                 ← Entry point
│   └── appsettings.json           ← Configuration file
│
├── Game.Tests/                    ← Test project (379 tests)
│   ├── Features/                  ← Feature tests
│   ├── Models/                    ← Model tests
│   ├── Services/                  ← Service tests
│   ├── Validators/                ← Validator tests
│   ├── Generators/                ← Generator tests
│   └── Integration/               ← Integration tests
│
├── docs/                          ← Documentation
│   ├── GDD-Main.md               ← This document
│   ├── guides/                    ← User guides
│   ├── implementation/            ← Implementation notes
│   └── testing/                   ← Test reports
│
├── logs/                          ← Serilog output
├── Game.sln                       ← Solution file
└── README.md                      ← Project overview
```

### Configuration System

**appsettings.json** structure:

```json
{
  "GameSettings": {
    "AutoSaveEnabled": true,
    "AutoSaveFrequency": 5,
    "MaxSaveSlots": 10
  },
  "GameplaySettings": {
    "StartingGold": 100,
    "BaseHealth": 100,
    "BaseMana": 50,
    "AttributePointsPerLevel": 3,
    "SkillsLearnedPerLevel": 1,
    "MaxLevel": 50
  },
  "DifficultySettings": {
    "Casual": { "EnemyHealthMultiplier": 0.7, /* ... */ },
    "Normal": { /* ... */ },
    "Hard": { /* ... */ },
    "Nightmare": { /* ... */ },
    "Apocalypse": { /* ... */ }
  },
  "UISettings": {
    "UseColors": true,
    "AnimationSpeed": 50,
    "ShowTutorials": true
  },
  "LoggingSettings": {
    "LogLevel": "Information",
    "LogToFile": true,
    "LogToConsole": true
  }
}
```

All settings validated with FluentValidation on startup.

---

## Content & Progression

### Level Progression Table

| Level | XP Required | Cumulative XP | HP Gain | MP Gain | Attr Points | Skills |
|-------|-------------|---------------|---------|---------|-------------|--------|
| 1 | 0 | 0 | - | - | - | - |
| 2 | 100 | 100 | +5 | +3 | +3 | - |
| 3 | 150 | 250 | +5 | +3 | +3 | +1 (Power Strike or Precision Shot) |
| 5 | 300 | 850 | +5 | +3 | +3 | +1 (Toughness or Arcane Focus) |
| 7 | 500 | 2350 | +5 | +3 | +3 | +1 (Evasion or Fortitude) |
| 10 | 800 | 5650 | +5 | +3 | +3 | +1 (Lucky Strike or Mana Mastery) |
| 25 | 3000 | ~50,000 | +5 | +3 | +3 | All skills unlocked |
| 50 | 8000 | ~500,000 | +5 | +3 | +3 | Max level |

### Enemy Scaling

| Player Level | Enemy Level | Avg HP | Avg Damage | XP Reward | Gold Reward |
|--------------|-------------|--------|------------|-----------|-------------|
| 1 | 1-2 | 30-50 | 5-10 | 50-75 | 10-25 |
| 5 | 4-6 | 100-150 | 15-25 | 150-200 | 30-60 |
| 10 | 9-11 | 250-350 | 30-50 | 300-400 | 75-150 |
| 25 | 24-26 | 800-1200 | 80-120 | 800-1200 | 200-400 |
| 50 | 49-51 | 2500-3500 | 200-300 | 2000-3000 | 500-1000 |

Bosses have 3× HP and 1.5× damage.

### Item Progression

**Early Game (Level 1-10):**
- Common/Uncommon items
- Iron/Bronze equipment
- Basic potions (Heal 50 HP)
- 10-50 gold value

**Mid Game (Level 10-25):**
- Rare/Epic items
- Steel/Mithril equipment
- Greater potions (Heal 100 HP)
- 100-500 gold value

**Late Game (Level 25-50):**
- Epic/Legendary items
- Adamantine/Dragonscale equipment
- Superior potions (Heal 200 HP)
- 500-5000 gold value

**New Game+:**
- Legendary items more common
- Artifact-tier equipment
- Unique NG+ exclusive items

### Location Progression

**Planned Locations (Future):**

1. **Starting Village** (Level 1-3)
   - Safe zone, shops, NPCs
   - Tutorial quests
   - Basic enemies (rats, bandits)

2. **Forest Clearing** (Level 3-7)
   - Medium difficulty
   - Wildlife enemies (wolves, bears)
   - Herb gathering

3. **Ancient Ruins** (Level 10-15)
   - Quest location
   - Undead enemies
   - Rare loot

4. **Dark Caverns** (Level 15-20)
   - Dungeon
   - Boss encounter
   - Epic equipment

5. **Shadow Realm** (Level 20-30)
   - Late-game area
   - Demonic enemies
   - Main quest climax

6. **Final Throne** (Level 25+)
   - Final boss arena
   - Victory location
   - Legendary rewards

---

## JSON Data Standards (v4.0)

All game data files follow strict standards for consistency, maintainability, and ContentBuilder compatibility.

### Standards Documentation

Comprehensive standards documented in `docs/standards/json/`:

1. **NAMES_JSON_STANDARD.md** - Pattern generation file standard
2. **CATALOG_JSON_STANDARD.md** - Item/enemy catalog file standard  
3. **CBCONFIG_STANDARD.md** - ContentBuilder UI configuration standard
4. **README.md** - Overview and navigation

### names.json Standard (Pattern Generation)

**Purpose**: Pattern-based procedural name generation with weighted components

**Required Fields:**
- `version`: "4.0" (current standard)
- `type`: "pattern_generation"
- `supportsTraits`: true or false
- `lastUpdated`: ISO date string (YYYY-MM-DD)
- `description`: File purpose
- `patterns[]`: Array of pattern templates
- `components{}`: Component arrays

**Key Rules:**
- Use `rarityWeight` for selection probability (NOT "weight")
- NO "example" fields allowed
- Component tokens: `{base}`, `{prefix}`, `{suffix}` (curly braces)
- External references: `[@materialRef/weapon]`, `[@materialRef/armor]` (square brackets)

**Pattern Syntax:**
```json
{
  "patterns": [
    { "rarityWeight": 40, "pattern": "{base}" },
    { "rarityWeight": 35, "pattern": "[@materialRef/weapon] {base}" },
    { "rarityWeight": 20, "pattern": "{prefix} {base} {suffix}" }
  ],
  "components": {
    "prefix": [
      { "rarityWeight": 10, "value": "Ancient", "traits": {...} }
    ]
  }
}
```

### catalog.json Standard (Item/Enemy Definitions)

**Purpose**: Base definitions for items, enemies, abilities, etc.

**Required Metadata:**
- `description`: File purpose
- `version`: Version number
- `lastUpdated`: ISO date string
- `type`: Must end with "_catalog" (item_catalog, ability_catalog, etc.)

**Structure:**
- Hierarchical: `{category}_types → type_name → traits + items[]`
- Flat: `items[]` at root (abilities only)

**Key Rules:**
- All items MUST have `name` and `rarityWeight`
- Physical "weight" is allowed (item weight in pounds)
- Type-level traits apply to all items of that type
- Item-level stats override type traits

**Example:**
```json
{
  "metadata": {
    "description": "Weapon catalog",
    "version": "1.0",
    "lastUpdated": "2025-12-27",
    "type": "item_catalog"
  },
  "weapon_types": {
    "swords": {
      "traits": { "category": { "value": "melee", "type": "string" } },
      "items": [
        { "name": "Longsword", "rarityWeight": 5, "weight": 3.0 }
      ]
    }
  }
}
```

### .cbconfig.json Standard (ContentBuilder UI)

**Purpose**: Configure folder display in ContentBuilder WPF application

**Required Fields:**
- `icon`: MaterialDesign icon name (e.g., "SwordCross", "Shield")
- `sortOrder`: Integer for tree position (1 = top)

**Key Rules:**
- Use MaterialDesign icon names, NOT emojis
- Lower sortOrder = higher in tree
- Standard ranges: 1-10 (core), 11-20 (enemy types), 21-30 (abilities)

**Optional Fields:**
- `displayName`: Override folder name
- `description`: Tooltip text
- `fileIcons`: Icon mapping for specific files
- `showFileCount`: Display file count badge

**Example:**
```json
{
  "icon": "SwordCross",
  "displayName": "Weapons",
  "description": "Weapon definitions with v4.0 pattern generation",
  "sortOrder": 1,
  "fileIcons": {
    "names": "FormatListBulleted",
    "catalog": "ShapeOutline"
  }
}
```

### Compliance Status

**100% Compliance Achieved - December 27, 2025**

| File Type | Total Files | Compliant | Status |
|-----------|-------------|-----------|--------|
| names.json | 30 | 30 | ✅ 100% |
| catalog.json | 30 | 28 | ✅ 100% (2 excluded) |
| .cbconfig.json | 33 | 33 | ✅ 100% |

**Notes:**
- npcs/catalog.json and quests/catalog.json use specialized structures (excluded from standard)
- All other files follow standards 100%
- Standards enforced via ContentBuilder validation

### Validation & Enforcement

**ContentBuilder Tool:**
- Real-time validation against standards
- Visual warnings for violations
- Auto-complete for required fields
- Icon picker for .cbconfig.json (MaterialDesign icons only)

**Manual Validation:**
- Check version: "4.0" for names.json
- Verify no "example" fields
- Ensure `rarityWeight` (not "weight") in patterns
- Confirm all items have `name` and `rarityWeight`
- Validate icon names in .cbconfig.json

**When Creating New JSON Files:**
1. Follow the appropriate standard based on file type
2. Use `version: "4.0"` for all pattern files
3. Always include `supportsTraits` field in names.json
4. Use `rarityWeight` for selection probability
5. Never use "example" fields
6. Use MaterialDesign icon names in .cbconfig.json
7. Validate against standards before committing

---

## User Interface

### Main Menu

```
╔══════════════════════════════════════╗
║       CONSOLE RPG - MAIN MENU        ║
╚══════════════════════════════════════╝

> New Game
  Load Game
  Settings
  Hall of Fame
  Quit

Use ↑↓ arrows to navigate, Enter to select
```

### Character Creation

**Step 1: Name Entry**
```
Enter your character's name:
> [_________________]
```

**Step 2: Class Selection**
```
Choose your class:

┌─────────────────────────────────────────┐
│ ⚔️  Warrior - Master of melee combat    │
│     +2 STR, +1 CON                       │
│     High HP, devastating attacks         │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ 🗡️  Rogue - Swift and deadly            │
│     +2 DEX, +1 WIS                       │
│     High dodge, critical strikes         │
└─────────────────────────────────────────┘

[... other classes ...]

> Select: Warrior
```

### In-Game Menu

```
╔══════════════════════════════════════╗
║         IN-GAME MENU                 ║
╚══════════════════════════════════════╝

> Explore
  View Character
  Inventory
  Quests (Coming Soon)
  Rest
  Save Game
  Main Menu

[Apocalypse Timer: 45:23 remaining]
```

### Combat Screen

```
╔══════════════════════════════════════╗
║            COMBAT!                   ║
╚══════════════════════════════════════╝

Enemy: Shadow Demon (Level 15)
HP: [████████████░░░░░░░░] 200/300

Your HP: [██████████████████] 250/250
Your MP: [████████████░░░░░░] 80/120

What will you do?

> Attack
  Defend  
  Use Item
  Flee

Shadow Demon attacks you for 35 damage!
You attack Shadow Demon for 50 damage!
[red]CRITICAL HIT![/] You deal 100 damage!
```

### Character Screen

```
╔══════════════════════════════════════╗
║       CHARACTER SHEET                ║
╚══════════════════════════════════════╝

Name: Aragorn                Level: 15
Class: Warrior               XP: 5650/6000
Gold: 450                    
HP: 250/250                  MP: 80/120

┌─ Attributes ────────────────────────┐
│ STR: 18 (+4)    DEX: 14 (+2)       │
│ CON: 16 (+3)    INT: 10 (+0)       │
│ WIS: 12 (+1)    CHA: 11 (+0)       │
└─────────────────────────────────────┘

┌─ Skills ────────────────────────────┐
│ ✓ Power Strike (+20% melee damage) │
│ ✓ Toughness (+25% max HP)          │
│ ✓ Evasion (+10% dodge)             │
└─────────────────────────────────────┘

┌─ Equipment ─────────────────────────┐
│ Weapon:    Flaming Longsword (+35)  │
│ Armor:     Plate Mail of the Bear   │
│ Shield:    Kite Shield (+15 DEF)    │
│ Accessory: Ring of Strength (+2 STR)│
└─────────────────────────────────────┘

[Press any key to continue]
```

### Inventory Screen

```
╔══════════════════════════════════════╗
║          INVENTORY                   ║
╚══════════════════════════════════════╝

Slots: 14/20

┌─────┬────────────────────────┬──────┬───────┐
│ #   │ Item                   │ Type │ Value │
├─────┼────────────────────────┼──────┼───────┤
│ [E] │ Flaming Longsword      │ WPN  │ 500g  │
│ [E] │ Plate Mail of the Bear │ ARM  │ 800g  │
│ [E] │ Kite Shield            │ SHD  │ 250g  │
│ 1   │ Health Potion x5       │ CON  │ 50g   │
│ 2   │ Mana Potion x3         │ CON  │ 75g   │
│ 3   │ Iron Dagger            │ WPN  │ 25g   │
└─────┴────────────────────────┴──────┴───────┘

Commands:
[E]quip  [U]se  [D]rop  [S]ort  [B]ack

> _
```

### Victory Screen

```
╔══════════════════════════════════════════╗
║                                          ║
║     🎉 VICTORY! 🎉                      ║
║                                          ║
║   You have defeated the Shadow Lord     ║
║   and saved the realm!                   ║
║                                          ║
╚══════════════════════════════════════════╝

╔══════════════════════════════════════════╗
║         Final Statistics                 ║
╠══════════════════════════════════════════╣
║ Final Level:           25                ║
║ Enemies Defeated:      142               ║
║ Quests Completed:      6/6               ║
║ Achievements:          5/6               ║
║ Total Gold Earned:     12,450            ║
║ Play Time:             5h 23m            ║
║ Difficulty:            Hard              ║
╚══════════════════════════════════════════╝

🏆 Achievements Unlocked:
   ✓ First Blood
   ✓ Survivor  
   ✓ Dragon Slayer
   ✓ Treasure Hunter
   ✓ Champion

🎁 Rewards:
   ✓ 5000 XP
   ✓ 2000 Gold
   ✓ Legendary Weapon: Sword of Destiny
   ✓ New Game+ Unlocked!

[Press any key to continue]
```

### UI Components (Spectre.Console)

The game uses the `ConsoleUI` wrapper class for all UI operations:

**Available Methods:**
- `WriteColoredText()` - Markup-enabled text
- `WriteText()` - Safe plain text (auto-escapes)
- `ShowBanner()` - Styled title banners
- `ShowSuccess/Error/Warning/Info()` - Status messages
- `AskForInput()` - Text input
- `AskForNumber()` - Numeric input with validation
- `ShowMenu()` - Single-selection menu
- `ShowMultiSelectMenu()` - Multi-selection menu
- `Confirm()` - Yes/No confirmation
- `ShowTable()` - Tables with headers/rows
- `ShowPanel()` - Bordered panels
- `ShowProgress()` - Progress bars
- `PressAnyKey()` - Wait for input

**Security:** All user input is automatically escaped to prevent markup injection attacks.

---

## ContentBuilder - Data Editor Tool

**ContentBuilder** is a WPF desktop application for editing game data files (JSON) with a visual interface.

### Features

#### Pattern Editor (Name Lists)
- **Dynamic Component Buttons**: Auto-generates buttons for each component in the file
  - Weapons: `{base}`, `{prefix}`, `{suffix}`, `{quality}`, `{descriptive}`
  - NPCs: `{firstName}`, `{lastName}`, `{title}`, `{nickname}`
  - Adapts to any JSON structure
- **Reference Token Buttons**: Quick-insert for cross-file references
  - `@materialRef/weapon` - Weapon-compatible materials
  - `@materialRef/armor` - Armor-compatible materials
  - `@itemRef`, `@enemyRef` - Item/enemy references
- **Browse Dialog**: Visual catalog browser for selecting specific references
- **Default Patterns**: Readonly `{base}` pattern auto-created for all files
- **Real-time Preview**: Generate example names as you edit patterns
- **Weight-based Selection**: Configure rarity/frequency of components

#### Component Editor
- Add/remove component groups (prefix, suffix, quality, etc.)
- Edit component values and rarity weights
- Trait assignment (future)
- Visual organization by category

#### Quest Catalog Editor (v4.2)
- Quest metadata (ID, name, description, level)
- Prerequisite management
- Objective definition with progress tracking
- Reward configuration (XP, gold, items)
- Social class filtering (for NPCs)

#### Material Catalog Editor
- Material properties (hardness, value, weight)
- Context-specific traits (weapon vs armor)
- Rarity weights for procedural generation
- Visual trait editor

### Architecture

**Technology Stack:**
- **WPF** (.NET 9.0) - Desktop UI framework
- **MaterialDesignThemes** 5.1.0 - Material Design UI components
- **CommunityToolkit.Mvvm** 8.4.0 - MVVM framework
- **Newtonsoft.Json** - JSON parsing/serialization
- **FluentValidation** - Input validation
- **Serilog** - Structured logging

**MVVM Pattern:**
- ViewModels: Observable properties, RelayCommands
- Models: Data structures (NameListCategory, ComponentBase, PatternBase)
- Services: JsonEditorService, ValidationService
- Converters: UI data transformation

**Key Classes:**
- `NameListEditorViewModel` - Pattern/component editing logic
- `ReferenceSelectorViewModel` - Catalog browser logic
- `QuestCatalogEditorViewModel` - Quest editing logic
- `NamePatternBase` - Abstract pattern model (Item/NPC specializations)
- `NameComponentBase` - Abstract component model (Item/NPC specializations)

### Usage Workflow

1. **Open File**: Select a JSON data file (names.json, catalog.json, quests.json)
2. **Edit Components**: Add/modify component groups and values
3. **Create Patterns**: Click token buttons to compose patterns without typing `{}` or `@`
4. **Browse References**: Use dialog to select materials, items, enemies
5. **Preview**: See generated examples in real-time
6. **Save**: Write changes back to JSON with validation

### Data Validation

- **Pattern syntax**: Validates token format (`{component}`, `@reference/context`)
- **Component keys**: Ensures referenced components exist
- **Reference paths**: Validates cross-file reference format
- **Rarity weights**: Enforces positive integers
- **Required fields**: Name, weight, etc. must be present

### Error Prevention

- **No Manual Typing**: Token buttons eliminate syntax errors
- **Readonly Defaults**: `{base}` pattern can't be deleted/broken
- **Visual Feedback**: Gray background for readonly fields
- **Disabled Actions**: Delete button disabled for protected patterns
- **Auto-spacing**: Proper spacing when appending tokens

---

## ContentBuilder - Data Editor Tool

**ContentBuilder** is a WPF desktop application for editing game data files (JSON) with a visual interface. It eliminates manual JSON editing and prevents syntax errors through UI-driven data composition.

### Purpose & Benefits

**Why ContentBuilder?**
- **Error Prevention**: Token buttons prevent manual typing errors (`{base}` vs `{base}`)
- **Visual Workflow**: See patterns, components, and generated examples in real-time
- **No JSON Knowledge Required**: Non-technical users can edit game data
- **Data Integrity**: Validation ensures all references exist and syntax is correct
- **Productivity**: Faster than manual JSON editing with Find/Replace

### Features

#### 1. Pattern Editor (Name Lists)

**Dynamic Component Buttons:**
- Auto-generates buttons for each component in the loaded file
- **Weapons**: `{base}`, `{prefix}`, `{suffix}`, `{quality}`, `{descriptive}`
- **NPCs**: `{firstName}`, `{lastName}`, `{title}`, `{nickname}`
- **Quests**: `{action}`, `{target}`, `{location}`, `{reward}`
- Adapts to ANY JSON structure - buttons reflect actual components

**Reference Token Buttons:**
- `@materialRef/weapon` - Insert weapon-compatible material reference
- `@materialRef/armor` - Insert armor-compatible material reference
- `@itemRef` - Reference another item (future)
- `@enemyRef` - Reference enemy type (future)
- **Browse Dialog**: Visual catalog browser for selecting specific references (e.g., Iron, Steel, Mithril)

**Default Patterns:**
- Every file gets a readonly `{base}` pattern automatically
- Cannot be edited or deleted (data integrity protection)
- Visual indicator: Gray background and text
- Delete button disabled for readonly patterns
- Not saved to JSON (regenerated on load)

**Real-time Preview:**
- Generate example names as you edit patterns
- See how components and references combine
- Test pattern weights and rarity distribution
- Validate syntax before saving

**Weight-based Selection:**
- Configure rarity/frequency of each component/pattern
- Higher weights = more common in generated names
- Used by Bogus library for procedural generation

#### 2. Component Editor

- Add/remove component groups (prefix, suffix, quality, base, etc.)
- Edit component values with weights
- Trait assignment to components (future)
- Visual organization by category
- Validation: Ensures no duplicate keys

#### 3. Quest Catalog Editor (v4.2)

**Quest Metadata:**
- Quest ID, Name, Description, Level requirement
- Quest giver (NPC name)
- Social class requirements (Noble, Merchant, Peasant, etc.)

**Prerequisite Management:**
- Select prerequisite quests from dropdown
- Chain quests together (Quest B requires Quest A)
- Visual dependency tree (future)

**Objective Definition:**
- Objective type (Kill, Collect, Explore, Interact)
- Target (enemy type, item, location, NPC)
- Required count (defeat 5 enemies, collect 3 items)
- Progress tracking configuration

**Reward Configuration:**
- XP reward
- Gold reward
- Item rewards (select from catalog)
- Apocalypse time bonuses
- Skill unlocks (future)

#### 4. Material Catalog Editor

**Material Properties:**
- Name, Description, Rarity (Common to Legendary)
- Base Value (gold cost)
- Hardness (affects durability)
- Weight (affects item stats)
- Elemental affinity (Fire, Ice, Lightning, etc.)

**Context-Specific Traits:**
- **Weapon Materials**: Damage bonus, critical chance, attack speed
- **Armor Materials**: Defense bonus, resistances, weight reduction
- Same material can have different traits for weapons vs armor

**Rarity Weights:**
- Configure how often materials appear in generation
- Iron: Common (high weight)
- Mithril: Rare (medium weight)
- Adamantine: Legendary (low weight)

**Visual Trait Editor:**
- Add/remove traits with key-value pairs
- Trait templates (damage bonuses, resistances, stat boosts)
- Validation: Numeric values, valid trait keys

### Architecture

**Technology Stack:**
- **WPF** (.NET 9.0) - Desktop UI framework
- **MaterialDesignThemes** 5.1.0 - Material Design UI components
- **CommunityToolkit.Mvvm** 8.4.0 - MVVM framework (RelayCommand, ObservableProperty)
- **Newtonsoft.Json** 13.0.4 - JSON parsing/serialization
- **FluentValidation** 12.1.1 - Input validation
- **Serilog** 4.3.0 - Structured logging

**MVVM Pattern:**
- **ViewModels**: Observable properties, RelayCommands, business logic
- **Models**: Data structures (NameListCategory, ComponentBase, PatternBase)
- **Services**: JsonEditorService (file I/O), ValidationService
- **Converters**: InverseBooleanConverter (for UI bindings)
- **Views**: XAML with Material Design styling

**Key Classes:**

*ViewModels:*
- `NameListEditorViewModel` - Pattern/component editing logic
- `ReferenceSelectorViewModel` - Catalog browser logic
- `QuestCatalogEditorViewModel` - Quest editing logic
- `MaterialCatalogEditorViewModel` - Material editing logic

*Models:*
- `NamePatternBase` - Abstract pattern model (Item/NPC specializations)
- `NameComponentBase` - Abstract component model (Item/NPC specializations)
- `QuestTemplate` - Quest data model
- `MaterialDefinition` - Material data model

*Services:*
- `PatternExecutor` - Parses and executes patterns (in RealmEngine.Shared)
- `DataReferenceResolver` - Resolves cross-file references (in RealmEngine.Shared)

### Usage Workflow

**Typical Editing Session:**

1. **Launch ContentBuilder** - Open the WPF application
2. **Open File** - Browse to JSON data file (e.g., `weapons/names.json`)
3. **Edit Components** - Add/modify component groups:
   - Add new `{quality}` component with values: Masterwork, Fine, Standard
   - Set weights: Masterwork (10), Fine (30), Standard (60)
4. **Create Patterns** - Compose new patterns WITHOUT typing:
   - Click `{quality}` button → inserts `{quality}`
   - Click `@materialRef/weapon` button → inserts `@materialRef/weapon`
   - Click `{base}` button → inserts `{base}`
   - Result: `{quality} @materialRef/weapon {base}`
5. **Browse References** - Click "Browse" button:
   - Opens dialog showing materials catalog
   - Select "Mithril" from list
   - Inserts `@materialRef/weapon/Mithril` into pattern
6. **Preview** - Click "Generate Examples" button:
   - See generated names: "Masterwork Mithril Longsword", "Fine Steel Dagger"
   - Verify pattern works as expected
7. **Save** - Click "Save" button:
   - Validates all patterns and components
   - Writes changes back to JSON file
   - Shows success/error message

### Data Validation

**Pattern Syntax Validation:**
- Token format: `{componentKey}` must match existing component
- Reference format: `@domain/category` or `@domain/category/identifier`
- No orphaned tokens (all `{keys}` must have matching components)
- No duplicate pattern templates

**Component Validation:**
- Component keys must be unique
- Weights must be positive integers
- At least one value per component
- No empty component names

**Reference Validation:**
- Cross-file references must point to existing files
- Reference paths must follow format: `domain/category[/identifier]`
- Context must match (weapon materials for weapon patterns)

**Required Fields:**
- Patterns: `patternTemplate`, `weight`
- Components: `key`, `values[]`, `weights[]`
- Quests: `id`, `name`, `level`, `objectives[]`, `rewards`
- Materials: `name`, `value`, `itemTypes[]`, `traits{}`

### Error Prevention Features

**1. No Manual Typing:**
- Token buttons eliminate syntax errors (`{base}` vs `{Base}` vs `base`)
- Reference buttons ensure proper format (`@materialRef/weapon` vs `materialRef`)
- Auto-spacing prevents double spaces or missing spaces

**2. Readonly Defaults:**
- `{base}` pattern always exists, can't be deleted
- Prevents broken catalogs with no patterns
- Visual indicator (gray background) shows read-only state

**3. Visual Feedback:**
- Disabled buttons when no pattern selected
- Gray background for readonly fields
- Red border for validation errors
- Success/error messages after save

**4. Smart UI:**
- Delete button disabled for protected patterns
- Reference browser shows only valid options
- Component buttons only show existing components
- Tooltips explain each button's purpose

### Testing

**ContentBuilder Tests (11 tests):**
- ViewModel tests (8 tests) - UI logic and data binding
- UI tests (3 tests) - Control initialization and visibility
- Integration tests (future) - End-to-end workflows

**Pattern System Tests (18 tests):**
- PatternExecutor tests (12 tests) - Token parsing, resolution, error handling
- DataReferenceResolver tests (6 tests) - Cross-file references, singleton pattern

**All tests use xUnit + FluentAssertions for readable assertions.**

### File Format Support

**Supported JSON Structures:**

*Name Lists (v4 format):*
```json
{
  "components": {
    "base": [
      { "value": "Longsword", "weight": 50 },
      { "value": "Dagger", "weight": 30 }
    ],
    "prefix": [ /* ... */ ]
  },
  "patterns": [
    { "patternTemplate": "{base}", "weight": 100 },
    { "patternTemplate": "@materialRef/weapon {base}", "weight": 80 }
  ]
}
```

*Material Catalog:*
```json
{
  "materials": [
    {
      "name": "Iron",
      "value": 10,
      "itemTypes": ["weapon", "armor"],
      "traits": { "damage": 5, "defense": 3 }
    }
  ]
}
```

*Quest Catalog:*
```json
{
  "quests": [
    {
      "id": "quest_001",
      "name": "The Awakening",
      "level": 1,
      "objectives": [ /* ... */ ],
      "rewards": { "xp": 100, "gold": 50 }
    }
  ]
}
```

### Future Enhancements

**Planned Features:**
- **Trait Assignment UI**: Assign traits to components/patterns visually
- **Pattern Testing**: Run generation 100 times, show distribution
- **Dependency Visualization**: Graph showing quest prerequisites
- **Bulk Operations**: Edit multiple files at once
- **Import/Export**: Copy patterns between files
- **Version Control**: Track changes, undo/redo
- **Collaborative Editing**: Multi-user support (long-term)

---

## Future Roadmap

### Phase 5: Content Expansion (Planned)

**Timeline**: Q1 2026  
**Focus**: More content, replayability, polish

#### Features

1. **Side Quests** (10-20 quests)
   - Optional quests for XP/gold/items
   - Character-specific quests
   - Repeatable daily quests

2. **Boss Encounters** (5-10 unique bosses)
   - Unique mechanics per boss
   - Legendary loot drops
   - Achievement unlocks

3. **Dungeons** (3-5 multi-room dungeons)
   - Procedurally generated layouts
   - Multiple floors/rooms
   - Boss at the end
   - Loot chests

4. **Towns & NPCs** (2-3 towns)
   - Shops (buy/sell items)
   - Blacksmith (repair/upgrade)
   - Inn (rest for HP/MP)
   - Quest givers

### Phase 6: Advanced Systems (Planned)

**Timeline**: Q2 2026  
**Focus**: Deeper mechanics, customization

#### Features

1. **Crafting System**
   - Combine items to create new ones
   - Recipes (found or learned)
   - Materials (gathered from exploration)
   - Quality tiers (Crude to Masterwork)

2. **Enchanting System**
   - Add enchantments to items
   - Enchanting materials (gems, runes)
   - Risk of failure
   - Overenchanting for legendary effects

3. **Magic/Spell System**
   - Castable spells (Fireball, Heal, etc.)
   - Mana cost and cooldowns
   - Spell learning (scrolls, trainers)
   - Spell schools (Evocation, Restoration, etc.)

4. **Status Effects**
   - Poison (damage over time)
   - Burning (fire damage)
   - Frozen (reduced speed)
   - Stunned (skip turn)
   - Blessed (bonus stats)

### Phase 7: Multiplayer & Online (Future)

**Timeline**: TBD  
**Focus**: Community features

#### Features

1. **Global Leaderboards**
   - Hall of Fame (online)
   - Daily/weekly/all-time rankings
   - Filter by difficulty/class

2. **Daily Challenges**
   - Pre-generated challenges
   - Unique rewards
   - Leaderboard tracking

3. **Save Sharing**
   - Export/import save files
   - Share builds with community
   - Challenge friends

4. **Community Events**
   - Special limited-time quests
   - Boss rush mode
   - Hardcore leagues

### Phase 8: Audio & Polish (Future)

**Timeline**: TBD  
**Focus**: Immersion, feel, quality of life

#### Features

1. **Background Music** (NAudio)
   - Music per location
   - Combat music
   - Victory fanfare
   - Boss themes

2. **Sound Effects**
   - Attack sounds (sword swing, magic)
   - UI sounds (menu select, error)
   - Ambient sounds (birds, wind)

3. **Visual Polish**
   - ASCII art for locations
   - FigletText title screens
   - Combat animations (attack effects)
   - Loading screens

4. **Quality of Life**
   - Undo last action
   - Keybind customization
   - Quick-save hotkey
   - Tutorial system
   - Hint system

---

## Appendix

### Development Guidelines

1. **Adding a New Feature**
   - Create feature folder in `Game/Features/`
   - Add Commands and/or Queries
   - Implement handlers with MediatR
   - Add validators (FluentValidation)
   - Write unit tests (xUnit)
   - Update this GDD

2. **Testing Requirements**
   - All commands must have validators
   - All handlers must have unit tests
   - Integration tests for workflows
   - Minimum 90% code coverage

3. **Code Style**
   - Use record types for DTOs/commands/queries
   - Async/await for all I/O operations
   - Structured logging with Serilog
   - XML documentation for public APIs

4. **Documentation**
   - Update GDD for new features
   - Add guides for complex systems
   - Keep README.md current
   - Document breaking changes

### Glossary

- **CQRS**: Command Query Responsibility Segregation (separate read/write operations)
- **Vertical Slice**: Organizing code by feature instead of technical layer
- **MediatR**: Library for mediator pattern and event-driven architecture
- **Spectre.Console**: Library for rich console UI
- **LiteDB**: Embedded NoSQL database
- **Bogus**: Library for fake data generation
- **FluentValidation**: Library for building validation rules
- **Polly**: Library for resilience and transient-fault-handling

### External Resources

- **Project Repository**: [GitHub Link]
- **Spectre.Console Docs**: https://spectreconsole.net/
- **MediatR Docs**: https://github.com/jbogard/MediatR
- **LiteDB Docs**: https://www.litedb.org/
- **FluentValidation Docs**: https://docs.fluentvalidation.net/

### Credits

**Development Team**: Solo developer  
**Architecture Inspiration**: Jimmy Bogard (CQRS), Microsoft (Vertical Slice)  
**Libraries Used**: See Technology Stack section  
**Special Thanks**: .NET community, GitHub Copilot

---

**Document Version**: 1.5  
**Last Updated**: December 24, 2025  
**Status**: Complete - Phase 1-4 features + ContentBuilder development tool  
**Next Update**: Phase 5 planning (Q1 2026)  
**Latest Changes**:
- Added v4 pattern-based name generation system (PatternExecutor, DataReferenceResolver)
- Added ContentBuilder WPF desktop editor with visual pattern composer
- Added 18 new unit tests for pattern system (397 total tests, 99.0% pass rate)
- Updated architecture with RealmEngine.Shared project for cross-project services
- Added comprehensive ContentBuilder documentation section
