# Trait Standards v1.2

**Version:** 1.2  
**Date:** December 29, 2025  
**Status:** ACTIVE  
**Purpose:** Standardized trait formats and value enums for all game entities

---

## Overview

**Traits** are inherent properties of game entities (enemies, items, NPCs) that describe their fundamental characteristics. This document defines:

- **Trait Formats** - How traits are structured (boolean, numeric, string enum)
- **Type-Level vs Individual** - Traits that apply to categories vs specific items
- **Value Standards** - Standardized enums and numeric scales
- **Attribute Traits** - All attribute-like traits use numeric values (1-20+)

---

## Fundamental Concepts

### Traits vs Abilities vs Skills

These three systems serve different purposes and should never be confused:

#### **Traits** - What You ARE
**Definition:** Inherent, relatively permanent properties that define an entity's fundamental nature

**Characteristics:**
- Describe BEING, not DOING
- Usually passive (don't require activation)
- Typically immutable during gameplay
- Examples: undead, size, intelligence, flying, incorporeal

**Questions to Ask:**
- Is this a fundamental property of what the entity IS?
- Would this trait still be true if the entity was unconscious?
- Is this a descriptor rather than an action?

**Examples:**
- ✅ `undead: true` - A zombie IS undead
- ✅ `intelligence: 14` - A cultist HAS intelligence 14
- ✅ `size: "large"` - A troll IS large
- ❌ `petrifying_gaze: true` - This is an ACTION (ability)
- ❌ `swordmastery: 15` - This is a PROFICIENCY (skill)

---

#### **Abilities** - What You CAN DO
**Definition:** Actions, powers, and special capabilities that entities can perform or use

**Characteristics:**
- Describe ACTIONS and POWERS
- Can be active (require activation) or passive (always on, but still a "doing")
- Can be used, triggered, or activated
- Examples: dragon-breath, petrifying-gaze, drain-life, regeneration (passive ability)

**Questions to Ask:**
- Is this something the entity DOES or CAN DO?
- Does this require activation or have an effect?
- Could this be used in combat or gameplay?

**Ability Types:**
- **Active/Offensive**: Attacks, damage-dealing powers
- **Active/Defensive**: Defensive maneuvers, blocks
- **Active/Utility**: Non-combat actions
- **Passive**: Always-on effects (regeneration, aura)
- **Ultimate**: Powerful special moves

**Examples:**
- ✅ `@abilities/active/offensive:dragon-breath` - Action the dragon takes
- ✅ `@abilities/passive:regeneration` - Ongoing effect (also trait: `regeneration: true`)
- ✅ `@abilities/active/offensive:petrifying-gaze` - Special power activation
- ❌ `undead: true` - This is inherent NATURE (trait)
- ❌ `blade: 15` - This is PROFICIENCY (skill)

**Note:** Some properties can be BOTH trait and ability:
- `regeneration: true` (trait) - The troll IS a regenerating creature
- `@abilities/passive:regeneration` (ability) - The game mechanic for regeneration

---

#### **Skills** - What You've LEARNED
**Definition:** Trained proficiencies and learned capabilities that represent expertise

**Characteristics:**
- Describe LEARNED PROFICIENCY, not inherent nature
- Numeric values represent mastery level
- Can improve through practice/training
- Examples: swordplay, lockpicking, persuasion, crafting

**Questions to Ask:**
- Is this something learned through training or practice?
- Can this improve over time?
- Does this represent expertise rather than nature?

**Skill Types (Future Implementation):**
- **Combat Skills**: blade, axe, bow, unarmed
- **Magic Skills**: fire magic, healing, necromancy
- **Social Skills**: persuasion, intimidation, deception
- **Utility Skills**: lockpicking, stealth, perception
- **Crafting Skills**: blacksmithing, alchemy, enchanting

**Examples:**
- ✅ `blade: 15` - Proficiency with bladed weapons
- ✅ `persuasion: 12` - Social skill
- ✅ `lockpicking: 8` - Utility skill
- ❌ `intelligence: 14` - This is INHERENT CAPABILITY (attribute/trait)
- ❌ `@abilities/active/offensive:power-attack` - This is an ACTION (ability)

**Current Status:** Skills are NOT yet implemented in the game. This is for future reference.

---

### Design Decision Tree

When adding new game content, use this tree to determine classification:

```
Is this a property of an entity?
├─ YES → Is it inherent to what the entity IS?
│  ├─ YES → TRAIT (undead, size, intelligence)
│  └─ NO → Is it something the entity CAN DO?
│     ├─ YES → ABILITY (dragon-breath, regeneration effect)
│     └─ NO → Is it something LEARNED?
│        ├─ YES → SKILL (blade proficiency, persuasion)
│        └─ NO → METADATA (name, description, rarityWeight)
└─ NO → Not a trait/ability/skill (probably metadata or game data)
```

---

### Examples Across All Three Systems

**Dragon:**
- **Traits**: `size: "huge"`, `intelligence: 16`, `flying: true`, `element: "fire"`
- **Abilities**: `@abilities/active/offensive:dragon-breath`, `@abilities/passive:frightful-presence`
- **Skills** (future): `intimidation: 18`, `perception: 14`

**Vampire:**
- **Traits**: `undead: true`, `intelligence: 18`, `shapeshifter: true`, `regeneration: true`
- **Abilities**: `@abilities/active/offensive:drain-life`, `@abilities/passive:regeneration`
- **Skills** (future): `persuasion: 16`, `stealth: 14`

**Player Character (future):**
- **Traits**: `size: "medium"`, `intelligence: 12`, `strength: 16`
- **Abilities**: `@abilities/active/offensive:power-attack`, `@abilities/ultimate:berserk-rage`
- **Skills**: `blade: 15`, `athletics: 12`, `persuasion: 8`

---

## Core Principles

1. **Typed Values** - Use appropriate data types (boolean, number, string enum)
2. **Numeric Attributes** - All attribute traits (intelligence, strength, etc.) are numeric 1-20+
3. **Type-Level Defaults** - Common traits at category level, overrides at item level
4. **Boolean for Inherent Properties** - Use booleans for true/false characteristics (undead, magical)
5. **Abilities over Traits** - Special powers are passive/active abilities, not individual boolean traits

---

## Trait Taxonomy

### Categories of Traits

Traits are organized into **7 main categories**, each with focused subcategories:

**Organizational Principle:** Main categories represent broad domains, subcategories organize related traits within those domains.

---

#### 1. Attributes (Core Stats)
**Definition:** D&D-style character/enemy statistics that measure fundamental capabilities

**Critical Design Rule:** ALL attributes are numeric 1-20+ scale, NEVER string enums

**No Subcategories** - Attributes are foundational and don't require subdivision

| Attribute | Purpose | Range | Current Status |
|-----------|---------|-------|----------------|
| `intelligence` | Mental acuity, reasoning, memory | 1-20+ | ✅ **ACTIVE** (all 163 enemies) |
| `wisdom` | Perception, intuition, insight | 1-20+ | ⏸️ Deferred |
| `charisma` | Force of personality, leadership | 1-20+ | ⏸️ Deferred |
| `strength` | Physical power, melee effectiveness | 1-20+ | ⏸️ Deferred |
| `dexterity` | Agility, reflexes, accuracy | 1-20+ | ⏸️ Deferred |
| `constitution` | Endurance, health, resilience | 1-20+ | ⏸️ Deferred |

**Attribute Scale Reference:**

| Range | Category | Examples |
|-------|----------|----------|
| 1-2 | Mindless | Zombies (1), Skeletons (2) |
| 3-5 | Animal | Beasts (3-5), Plants (2-4) |
| 6-9 | Below Average | Trolls (5-8), Goblins (8) |
| 10-12 | Average (Human) | Humanoids (10), Orcs (8-10) |
| 13-15 | Above Average | Hobgoblins (12), Cultists (14) |
| 16-17 | Brilliant | Yuan-ti (16), Dragons (16-18) |
| 18-19 | Genius | Vampires (18), Devils (18) |
| 20+ | Legendary | Ancient Dragons (20), Liches (20) |

**Scope:** Typically Type-Level, but can be overridden for exceptional individuals

**Examples:**
```json
{
  "traits": {
    "intelligence": 14,
    "wisdom": 12,
    "charisma": 16
  }
}
```

**Why Separate from Other Traits:**
- Attributes follow strict 1-20+ numeric scale
- Directly map to D&D/RPG stat system
- Used for skill checks, stat requirements
- Never boolean, never string enum, never arrays
- Form the foundational stat framework

---

#### 2. Physical Traits
**Definition:** Tangible, observable characteristics of an entity's physical form and capabilities

##### 2.1 Core Physical Properties

**Body and form characteristics:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `size` | string enum | tiny, small, medium, large, huge, gargantuan | Type-Level | ✅ Active |
| `weight` | number | Physical weight in lbs | Individual | ⏸️ Deferred |
| `appearance` | string | Description text | Individual | ⏸️ Deferred |
| `incorporeal` | boolean | Non-physical form | Type-Level | ✅ Active |
| `armored` | boolean | Natural armor plating | Type-Level | ✅ Active |
| `twoHanded` | boolean | Requires both hands (items) | Individual | ⏸️ Deferred |

**Examples:**
- Dragons: `size: "huge"`
- Ghosts: `incorporeal: true`
- Beetles: `armored: true`
- Greatswords: `twoHanded: true`

##### 2.2 Movement

**Locomotion and mobility capabilities:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `speed` | number | Movement rate (game units) | Type-Level | ⏸️ Deferred |
| `flying` | boolean | Can fly | Type-Level | ✅ Active |
| `swimming` | boolean | Can swim efficiently | Type-Level | ⏸️ Deferred |
| `climbing` | boolean | Can climb walls/surfaces | Type-Level | ⏸️ Deferred |
| `burrowing` | boolean | Can burrow underground | Type-Level | ⏸️ Deferred |
| `hover` | boolean | Can hover in place | Type-Level | ⏸️ Deferred |
| `teleportation` | boolean | Can teleport | Type-Level | ⏸️ Deferred |

**Examples:**
- Dragons: `flying: true`, `speed: 40`
- Fish Creatures: `swimming: true`, `speed: 30`
- Spiders: `climbing: true`
- Blink Dogs: `teleportation: true`

##### 2.3 Sensory

**Biological perception and sensory capabilities:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `darkvision` | number | Range in feet | Type-Level | ⏸️ Deferred |
| `blindsight` | number | Sense without sight (range) | Type-Level | ⏸️ Deferred |
| `tremorsense` | number | Sense through vibrations (range) | Type-Level | ⏸️ Deferred |
| `truesight` | number | See through illusions (range) | Type-Level | ⏸️ Deferred |
| `keenSenses` | boolean | Enhanced perception | Type-Level | ⏸️ Deferred |
| `echolocation` | boolean | Sound-based sensing | Type-Level | ⏸️ Deferred |
| `heatVision` | boolean | See heat signatures | Type-Level | ⏸️ Deferred |
| `blind` | boolean | Cannot see | Type-Level | ⏸️ Deferred |
| `deaf` | boolean | Cannot hear | Type-Level | ⏸️ Deferred |

**Note:** Perception checks may combine `wisdom` (attribute) + `keenSenses` (trait) + `perception` (skill - future)

**Examples:**
- Bats: `darkvision: 60`, `echolocation: true`
- Snakes: `blindsight: 30`, `heatVision: true`
- Underground Creatures: `darkvision: 120`, `tremorsense: 60`
- Angels: `truesight: 120`
- Oozes: `blindsight: 60`, `blind: true`

**Physical Trait Summary:**
All subcategories represent observable, physical/biological capabilities - not mental states or magical properties.

---

#### 3. Mental/Psychological Traits
**Definition:** Cognitive, behavioral, and social characteristics that define how entities think and interact

##### 3.1 Behavior

**Combat and general behavioral patterns:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `behavior` | string enum | passive, aggressive, tactical, cunning, legendary, etc. | Type-Level | ✅ Active |
| `nocturnal` | boolean | Active at night | Type-Level | ⏸️ Deferred |
| `sentient` | boolean | Self-aware, intelligent | Type-Level | ⏸️ Deferred |

**Behavior Values:**
- `passive` - Won't attack unless provoked
- `defensive` - Defends territory
- `opportunistic` - Attacks weak targets
- `aggressive` - Attacks on sight
- `tactical` - Uses strategy
- `cunning` - Sets traps, ambushes
- `territorial` - Guards areas
- `predatory` - Hunts prey
- `legendary` - Boss-level behavior
- `chaotic` - Unpredictable
- `calculating` - Methodical planning

**Examples:**
- Beasts: `behavior: "aggressive"`, `intelligence: 4`
- Vampires: `behavior: "cunning"`, `intelligence: 18`
- Hobgoblins: `behavior: "tactical"`, `intelligence: 12`

##### 3.2 Alignment

**Moral and ethical orientation:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `alignment` | string enum | good, evil, lawful_good, chaotic_evil, neutral, etc. | Type-Level | ✅ Active |

**Alignment Values:**
- `good` - Benevolent
- `evil` - Malevolent
- `lawful_good` - Honorable heroes
- `lawful_evil` - Tyrannical, follows rules
- `chaotic_good` - Freedom fighters
- `chaotic_evil` - Destructive chaos
- `neutral` - Balanced or indifferent

**Examples:**
- Dragons (Chromatic): `alignment: "evil"`
- Dragons (Metallic): `alignment: "good"`
- Yuan-ti: `alignment: "evil"`

##### 3.3 Social Structure

**How entities organize and relate to their kind:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `social` | string enum | solitary, pack, tribal, clan, hive_mind, etc. | Type-Level | ✅ Active |
| `aristocratic` | boolean | Noble hierarchy | Type-Level | ⏸️ Deferred |

**Social Values:**
- `solitary` - Lives alone
- `pack` - Small groups
- `tribal` - Tribe structure
- `clan` - Clan-based
- `military` - Organized ranks
- `hive_mind` - Shared consciousness
- `aristocratic` - Noble hierarchy
- `cult` - Religious group
- `leadership` - Has leaders

**Examples:**
- Wolves: `social: "pack"`
- Vampires: `social: "aristocratic"`
- Kobolds: `social: "tribal"`
- Insects: `social: "hive_mind"`

**Mental/Psychological Trait Summary:**
- **Behavior**: How entity ACTS (aggressive vs tactical)
- **Alignment**: Moral compass (good vs evil)
- **Social**: Group organization (pack vs solitary)
- **vs Attributes**: `intelligence` measures capability, behavior describes ACTION

---

#### 4. Combat Traits
**Definition:** Properties that directly affect combat mechanics, damage, and survivability

##### 4.1 Damage Types

**Offensive and defensive damage properties:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `damageType` | string enum | physical, fire, slashing, piercing, etc. | Type-Level or Individual | ✅ Active |
| `vulnerability` | string or array | Damage types causing extra damage | Type-Level | ✅ Active |
| `immunity` | array | Damage types that don't affect entity | Type-Level | ✅ Active |
| `resistance` | array | Damage types with reduced effect | Type-Level | ⏸️ Deferred |

**Damage Type Values:**
- **Physical**: physical, slashing, piercing, bludgeoning
- **Elemental**: fire, cold, lightning, acid
- **Magical**: magical, radiant, necrotic, psychic
- **Other**: poison

**Examples:**
- Fire Elementals: `damageType: "fire"`, `immunity: ["fire"]`, `vulnerability: "cold"`
- Trolls: `damageType: "physical"`, `vulnerability: "fire"`
- Undead: `immunity: ["poison", "necrotic"]`, `vulnerability: "radiant"`

##### 4.2 Special Combat Properties

**Unique combat-related abilities:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `regeneration` | boolean | Passive health regeneration | Type-Level | ✅ Active |

**Note:** `regeneration` exists as both:
- **Trait** (`regeneration: true`) - Inherent property (troll IS regenerating)
- **Ability** (`@abilities/passive:regeneration`) - Game mechanic implementation

**Examples:**
- Trolls: `regeneration: true`, `vulnerability: "fire"`
- Vampires: `regeneration: true`

**Combat Trait Summary:**
Focus on mechanics that affect damage calculation, resistances, and combat behavior.

---

#### 5. Supernatural Traits
**Definition:** Magical, otherworldly, and supernatural properties that defy natural laws

##### 5.1 Elemental

**Elemental affinity and powers:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `element` | string enum | fire, water, earth, air, lightning, ice | Type-Level | ✅ Active |
| `breathType` | string enum | fire, acid, lightning, poison, cold | Individual | ✅ Active |

**Element Values:**
- `fire` - Fire elementals/dragons
- `water` - Water elementals
- `earth` - Earth elementals
- `air` - Air elementals
- `lightning` - Storm/lightning
- `ice` - Ice elementals

**Examples:**
- Fire Elementals: `element: "fire"`
- Red Dragon: `element: "fire"`, `breathType: "fire"`
- Blue Dragon: `element: "lightning"`, `breathType: "lightning"`

##### 5.2 Magical Properties

**Supernatural and magical characteristics:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `undead` | boolean | Animated dead | Type-Level | ✅ Active |
| `magical` | boolean | Uses/is infused with magic | Type-Level | ✅ Active |
| `shapeshifter` | boolean | Can change form | Type-Level | ✅ Active |
| `unique` | boolean | Special/boss enemy | Type-Level or Individual | ⏸️ Deferred |

**Examples:**
- Undead: `undead: true`, `vulnerability: "radiant"`
- Vampires: `undead: true`, `shapeshifter: true`, `magical: true`
- Liches: `undead: true`, `magical: true`
- Demons: `magical: true`

**Supernatural Trait Summary:**
- **Elemental**: Natural element affinity
- **Magical**: Supernatural properties that break natural laws
- **vs Physical**: Incorporeal is physical (non-solid), magical is supernatural

---

#### 6. Environmental Traits
**Definition:** Habitat preferences and environmental adaptations

##### 6.1 Habitat

**Where entities naturally live:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `habitat` | string or array | forest, mountain, cave, water, underground, etc. | Type-Level | ✅ Active |

**Habitat Values:**
- `forest` - Woodland areas
- `mountain` - High altitude
- `cave` - Underground caves
- `water` - Aquatic environments
- `underground` - Deep underground
- `desert` - Arid regions
- `tundra` - Arctic/cold regions
- `swamp` - Wetlands

**Examples:**
- Wolves: `habitat: "forest"`
- Bears: `habitat: ["forest", "mountain"]`
- Fish Creatures: `habitat: "water"`

##### 6.2 Environmental Adaptations

**Special adaptations to environments:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `rooted` | boolean | Cannot move (plants) | Type-Level | ✅ Active |
| `aquatic` | boolean | Lives in water | Type-Level | ⏸️ Deferred |
| `nocturnal` | boolean | Active at night | Type-Level | ⏸️ Deferred |

**Examples:**
- Plants: `rooted: true`, `habitat: "forest"`
- Fish: `aquatic: true`, `habitat: "water"`
- Bats: `nocturnal: true`, `habitat: "cave"`

**Environmental Trait Summary:**
Where entities live and how they've adapted to those environments.

---

#### 7. Functional Traits (Game Systems)
**Definition:** Game mechanics, professions, and system-level properties

##### 7.1 Profession (NPCs)

**Occupation and professional characteristics:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `profession` | string enum | blacksmith, merchant, guard, healer, etc. | Individual | ⏸️ Deferred |
| `skillLevel` | string enum | apprentice, journeyman, master, legendary | Individual | ⏸️ Deferred |
| `reputation` | number | 1-100 standing in community | Individual | ⏸️ Deferred |
| `merchant` | boolean | Can buy/sell items | Individual | ⏸️ Deferred |
| `trainer` | boolean | Can teach skills | Individual | ⏸️ Deferred |
| `questGiver` | boolean | Provides quests | Individual | ⏸️ Deferred |

**Profession Values:**
- **Crafting**: blacksmith, weaponsmith, armorsmith, leatherworker, tailor, alchemist, enchanter
- **Trade**: merchant, trader, innkeeper, shopkeeper
- **Service**: healer, priest, mage, scholar, librarian
- **Labor**: miner, farmer, lumberjack, fisherman
- **Military**: guard, soldier, captain, trainer
- **Social**: noble, beggar, performer, spy

**Examples:**
- Blacksmith NPC: `profession: "blacksmith"`, `skillLevel: "master"`, `merchant: true`, `trainer: true`
- Guard NPC: `profession: "guard"`, `reputation: 75`

##### 7.2 Mechanical (Items)

**Item system properties:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `slot` | string enum | mainhand, offhand, head, chest, consumable | Type-Level | ⏸️ Deferred |
| `category` | string enum | weapon, armor, potion, scroll | Type-Level | ✅ Active |
| `stackable` | boolean | Can stack in inventory | Type-Level | ⏸️ Deferred |
| `oneUse` | boolean | Consumed on use | Type-Level | ⏸️ Deferred |
| `twoHanded` | boolean | Requires both hands | Individual | ⏸️ Deferred |
| `skillType` | string enum | blade, axe, bow, magic | Type-Level | ⏸️ Deferred |

**Examples:**
- Weapons: `slot: "mainhand"`, `category: "weapon"`, `skillType: "blade"`
- Potions: `slot: "consumable"`, `stackable: true`, `oneUse: true`
- Greatswords: `twoHanded: true`

**Functional Trait Summary:**
Game system traits for NPCs and items - not inherent properties but gameplay mechanics.

---

### Summary: 7 Main Categories

| # | Category | Subcategories | Purpose |
|---|----------|---------------|---------|
| 1 | **Attributes** | _(none)_ | Core D&D-style stats (1-20+) |
| 2 | **Physical** | Core, Movement, Sensory | Observable body/form characteristics |
| 3 | **Mental/Psychological** | Behavior, Alignment, Social | How entities think and interact |
| 4 | **Combat** | Damage Types, Special | Combat mechanics and damage |
| 5 | **Supernatural** | Elemental, Magical | Otherworldly and magical properties |
| 6 | **Environmental** | Habitat, Adaptations | Where entities live |
| 7 | **Functional** | Profession, Mechanical | Game systems (NPCs, items) |
**Definition:** D&D-style character/enemy statistics that measure fundamental capabilities

**Critical Design Rule:** ALL attributes are numeric 1-20+ scale, NEVER string enums

| Attribute | Purpose | Range | Current Status |
|-----------|---------|-------|----------------|
| `intelligence` | Mental acuity, reasoning, memory | 1-20+ | ✅ **ACTIVE** (all 163 enemies) |
| `wisdom` | Perception, intuition, insight | 1-20+ | ⏸️ Deferred |
| `charisma` | Force of personality, leadership | 1-20+ | ⏸️ Deferred |
| `strength` | Physical power, melee effectiveness | 1-20+ | ⏸️ Deferred |
| `dexterity` | Agility, reflexes, accuracy | 1-20+ | ⏸️ Deferred |
| `constitution` | Endurance, health, resilience | 1-20+ | ⏸️ Deferred |

**Attribute Scale Reference:**

| Range | Category | Examples |
|-------|----------|----------|
| 1-2 | Mindless | Zombies (1), Skeletons (2) |
| 3-5 | Animal | Beasts (3-5), Plants (2-4) |
| 6-9 | Below Average | Trolls (5-8), Goblins (8) |
| 10-12 | Average (Human) | Humanoids (10), Orcs (8-10) |
| 13-15 | Above Average | Hobgoblins (12), Cultists (14) |
| 16-17 | Brilliant | Yuan-ti (16), Dragons (16-18) |
| 18-19 | Genius | Vampires (18), Devils (18) |
| 20+ | Legendary | Ancient Dragons (20), Liches (20) |

**Scope:** Typically Type-Level, but can be overridden for exceptional individuals (e.g., "Ancient Troll" has higher intelligence)

**Examples:**
```json
{
  "traits": {
    "intelligence": 14,    // Cultist intelligence
    "wisdom": 12,          // (future) Perception/insight
    "charisma": 16         // (future) Leadership ability
  }
}
```

**Why Separate from Other Traits:**
- Attributes follow strict 1-20+ numeric scale
- Directly map to D&D/RPG stat system
- Used for skill checks, stat requirements
- Never boolean, never string enum, never arrays
- Form the foundational stat framework

---

#### 2. Physical Traits (Non-Attribute)
**Definition:** Tangible, observable characteristics of an entity's physical form (NOT stat-based)

##### Core Physical Properties

| Trait | Type | Values | Scope |
|-------|------|--------|-------|
| `size` | string enum | tiny, small, medium, large, huge, gargantuan | Type-Level |
| `weight` | number | Physical weight in lbs | Individual |
| `appearance` | string | Description text | Individual |
| `incorporeal` | boolean | Non-physical form | Type-Level |
| `armored` | boolean | Natural armor plating | Type-Level |
| `twoHanded` | boolean | Requires both hands (items) | Individual |

##### Movement Subcategory

**Movement-related physical traits:**

| Trait | Type | Values | Scope | Status |
|-------|------|--------|-------|--------|
| `speed` | number | Movement rate (game units) | Type-Level | ⏸️ Deferred |
| `flying` | boolean | Can fly | Type-Level | ✅ Active |
| `swimming` | boolean | Can swim efficiently | Type-Level | ⏸️ Deferred |
| `climbing` | boolean | Can climb walls/surfaces | Type-Level | ⏸️ Deferred |
| `burrowing` | boolean | Can burrow underground | Type-Level | ⏸️ Deferred |
| `hover` | boolean | Can hover in place (flying) | Type-Level | ⏸️ Deferred |
| `teleportation` | boolean | Can teleport | Type-Level | ⏸️ Deferred |

**Movement Examples:**
- Dragons: `flying: true`, `speed: 40`
- Fish Creatures: `swimming: true`, `speed: 30`
- Spiders: `climbing: true`, `speed: 30`
- Burrowing Insects: `burrowing: true`, `speed: 20`
- Blink Dogs: `teleportation: true`

**Physical vs Attribute:**
- **Physical Trait:** Observable property (size, appearance, flying)
- **Attribute:** Measurable capability (strength, dexterity)

**Examples:**
- Dragons: `size: "huge"`, `flying: true`, `speed: 40`
- Ghosts: `incorporeal: true`, `flying: true`
---

### Types of Traits by Scope

#### Type-Level Traits
**Applied to ALL items in a category**

**Characteristics:**
- Defined in `traits` object at category level
- Inherited by all items in category
- Can be overridden by individual items
- Define the "default" for the category

**Examples:**
```json
{
  "goblinoid_types": {
    "goblins": {
      "traits": {
        "category": "goblinoid",
        "size": "small",           // All goblins are small
        "intelligence": 8,         // Average goblin intelligence
        "social": "tribal"         // All goblins are tribal
      }
    }
  }
}
```

**Best Practices:**
- Use for inherent category properties
- Use for properties shared by 90%+ of items
- Override exceptions at individual level

---

#### Individual Traits
**Specific to one item, overrides type-level**

**Characteristics:**
- Defined directly on item object
- Overrides type-level trait of same name
- Used for exceptions and unique properties
- Higher priority than type-level

**Examples:**
```json
{
  "items": [
    {
      "name": "Naga",
      "size": "large",              // Overrides type-level "medium"
      "damageType": "magical",      // Item-specific trait
      "breathType": "lightning"     // Individual-only trait
    }
  ]
}
```

**Best Practices:**
- Use sparingly for exceptions
- Document why override is needed
- Avoid overriding core category traits

---

### Types of Traits by Data Type

#### Boolean Traits
**True/false properties**

**When to Use:**
- Inherent yes/no characteristics
- Type-level flags (undead, magical, flying)
- NOT for special powers (use abilities instead)

**Examples:**
- ✅ `undead: true` - Inherent property
- ✅ `regeneration: true` - Natural ability (also needs passive ability for mechanics)
- ✅ `flying: true` - Physical capability
- ❌ `petrifying_gaze: true` - This is a special power, use ability instead

---

#### Numeric Traits
**Quantifiable measurements**

**When to Use:**
- Attributes (intelligence, strength, dexterity) - Always 1-20+
- Resistances/percentages - 0-100
- Stats that match game mechanics - Variable scale
- Counts and quantities

**Scales:**
- **1-20+**: Attributes (intelligence, wisdom, charisma)
- **0-100**: Percentages (fireResistance: 50 = 50% reduction)
- **Game Stats**: speed, weight, etc. (match existing stat scale)

**Examples:**
```json
{
  "intelligence": 14,        // Attribute (1-20+)
  "fireResistance": 50,      // Percentage (0-100)
  "speed": 15,               // Game stat
  "weight": 3.5              // Physical measurement
}
```

---

#### String Enum Traits
**Categorical values from predefined set**

**When to Use:**
- Property has discrete categories
- Values are descriptive, not numeric
- Need to query by category (e.g., "all fire elementals")

**Benefits:**
- Self-documenting
- Easy to filter/query
- Prevents typos

**Examples:**
```json
{
  "size": "medium",          // From: tiny, small, medium, large, huge, gargantuan
  "behavior": "aggressive",  // From: passive, aggressive, tactical, etc.
  "element": "fire"          // From: fire, water, earth, air
}
```

**Validation:** All enums should be documented in this standard

---

#### Array Traits
**Multiple values of same type**

**When to Use:**
- Entity has multiple values for same property
- Immunities, vulnerabilities, habitats
- NOT for different types of properties

**Examples:**
```json
{
  "immunity": ["fire", "poison"],           // Multiple immunities
  "habitat": ["forest", "mountain"],        // Lives in multiple places
  "vulnerability": "radiant"                // Single value (string is fine)
}
```

**Array vs Single Value:**
- If entity typically has 0-1 values: use single value (string or null)
- If entity typically has 2+ values: use array
- Arrays are never empty (omit property instead)

---

### Types of Traits by Mutability

#### Inherent Traits (Immutable)
**Permanent properties that define entity's nature**

**Characteristics:**
- Cannot change during gameplay
- Define what the entity IS, not what it DOES
- Typically type-level traits

**Examples:**
- `undead: true` - Cannot become alive
- `size: "large"` - Size doesn't change (normally)
- `category: "dragon"` - Can't change type
- `incorporeal: true` - Ghost is always incorporeal

**Rule:** 99% of traits in current system are inherent

---

#### Dynamic Traits (Mutable) - FUTURE
**Properties that can change during gameplay**

**Not currently implemented, but future consideration:**
- Status effects (poisoned, paralyzed)
- Temporary buffs/debuffs
- Environmental effects

**Design Note:** These will likely use a separate `effects` or `conditions` system rather than traits

---

#### Conditional Traits - FUTURE
**Active only under certain conditions**

**Not currently implemented, examples:**
- `nightBonus: +2` - Only at night
- `underwaterBreathing: true` - When in water
- `rageBuff: {condition: "health < 50%", effect: "+damage"}`

**Design Note:** May be implemented as part of abilities system rather than traits

---

### Trait Inheritance and Override Rules

#### Priority Order (Highest to Lowest)

1. **Individual Item Trait** - Highest priority
2. **Type-Level Trait** - Default for category
3. **System Default** - Implicit defaults (e.g., `size: "medium"` if not specified)

#### Override Examples

```json
{
  "troll_types": {
    "common_trolls": {
      "traits": {
        "size": "large",           // Type-level default
        "intelligence": 5,
        "regeneration": true
      },
      "items": [
        {
          "name": "Troll",
          "rarityWeight": 12
          // Inherits: size=large, intelligence=5, regeneration=true
        },
        {
          "name": "Ancient Troll",
          "size": "huge",          // OVERRIDES type-level "large"
          "intelligence": 8,       // OVERRIDES type-level 5
          "rarityWeight": 50
          // Inherits: regeneration=true
          // Overrides: size, intelligence
        }
      ]
    }
  }
}
```

#### Merge vs Replace

**Replace (Current System):**
- Individual trait completely replaces type-level trait
- No merging of values
- Example: `size: "huge"` replaces `size: "large"`

**Merge (Not Implemented):**
- Arrays could theoretically merge
- Example: `immunity: ["fire"]` + `immunity: ["poison"]` → `["fire", "poison"]`
- Currently NOT supported - individual completely replaces type-level

---

### Trait Design Guidelines

#### When to Add a New Trait

**Add a trait when:**
- ✅ Property is inherent to entity (part of what it IS)
- ✅ Property affects game mechanics
- ✅ Property is needed for filtering/querying
- ✅ Property applies to multiple entities
- ✅ Property is relatively static

**DON'T add a trait when:**
- ❌ Property is an action (use ability instead)
- ❌ Property is temporary (use status effect)
- ❌ Property is unique to one item (maybe just a note/description)
- ❌ Property is calculated (derive it from other values)

#### Naming Conventions

**Boolean Traits:**
- Use adjectives: `magical`, `undead`, `incorporeal`
- NOT verbs: ❌ `canFly` → ✅ `flying`
- NOT questions: ❌ `isUndead` → ✅ `undead`

**Numeric Traits:**
- Use nouns: `intelligence`, `fireResistance`, `weight`
- CamelCase for compound names: `fireResistance`, `maxHealth`

**String Enum Traits:**
- Use nouns: `size`, `behavior`, `element`
- Lowercase values: `"medium"`, `"aggressive"`, `"fire"`
- Underscores for compounds: `"hive_mind"`, `"lawful_good"`

**Array Traits:**
- Use plural nouns: `immunity`, `vulnerability` (not `immunities`, `vulnerabilities`)
- OR use descriptive singular: `habitat`, `element`

---

## Trait Formats

### Format 1: Boolean (Type-Level)

**Use Case:** Inherent properties that apply to entire category

```json
"traits": {
  "undead": true,
  "magical": true,
  "regeneration": true,
  "incorporeal": true,
  "shapeshifter": true,
  "flying": true,
  "armored": true
}
```

**Examples:**
- `undead: true` - All items in "undead" category are undead
- `regeneration: true` - All trolls regenerate health
- `magical: true` - Category uses magic

---

### Format 2: Numeric (Attributes)

**Use Case:** Measurable properties, especially attributes (1-20+ scale)

```json
"traits": {
  "intelligence": 14,
  "strength": 18,
  "dexterity": 12,
  "fireResistance": 50,
  "speed": 15
}
```

**Scale Guidelines:**
- **1-20+**: D&D-style attributes (intelligence, wisdom, charisma)
- **1-100**: Percentages (resistances, chances)
- **Stat values**: Match game stat scale (speed, defense, etc.)

---

### Format 3: String Enum (Categorical)

**Use Case:** Categorical properties with predefined values

```json
"traits": {
  "category": "humanoid",
  "size": "medium",
  "behavior": "opportunistic",
  "damageType": "physical",
  "alignment": "evil",
  "element": "fire"
}
```

**Always use string enums** when property has discrete categories

---

### Format 4: Array (Multiple Values)

**Use Case:** Multiple values of same type (immunities, vulnerabilities)

```json
"traits": {
  "immunity": ["fire", "poison"],
  "vulnerability": "radiant",
  "habitat": ["forest", "mountain"]
}
```

---

## Standard Trait Values

### Size Values

| Value | Description | Examples |
|-------|-------------|----------|
| `tiny` | Very small creatures | Rats, fairies |
| `small` | Child-sized | Halflings, goblins |
| `medium` | Human-sized (default) | Humans, elves, orcs |
| `large` | Horse-sized | Ogres, bears, trolls |
| `huge` | House-sized | Giants, young dragons |
| `gargantuan` | Colossal | Ancient dragons, titans |

---

### Intelligence Values (Numeric 1-20+)

**All intelligence traits use numeric values on 1-20+ scale**

| Range | Category | Examples |
|-------|----------|----------|
| 1-2 | Mindless | Zombies (1), Skeletons (2) |
| 3-5 | Animal | Beasts (3-5), Plants (2-4) |
| 6-9 | Below Average | Trolls (5-8), Goblins (8) |
| 10-12 | Average | Humanoids (10), Orcs (8-10) |
| 13-15 | Above Average | Hobgoblins (12), Cultists (14) |
| 16-17 | Brilliant | Yuan-ti (16), Dragons (16-18) |
| 18-19 | Genius | Vampires (18), Devils (18) |
| 20+ | Legendary | Ancient Dragons (20), Liches (20) |

**Conversion from Legacy String Enums:**
- `"none"` → 1
- `"low"` → 3-8 (depends on creature type)
- `"medium"` → 10-12
- `"high"` → 14-16
- `"very_high"` → 18
- `"genius"` → 20+

---

### Behavior Values

| Value | Description | Combat Pattern |
|-------|-------------|----------------|
| `passive` | Won't attack unless provoked | Defensive only |
| `defensive` | Defends territory | Fights when approached |
| `opportunistic` | Attacks weak targets | Bandits, scavengers |
| `aggressive` | Attacks on sight | Most monsters |
| `tactical` | Uses strategy | Military units |
| `cunning` | Sets traps, ambushes | Devils, assassins |
| `territorial` | Guards specific areas | Bears, elementals |
| `predatory` | Hunts prey | Wolves, vampires |
| `legendary` | Boss-level behavior | Named enemies |
| `chaotic` | Unpredictable | Lesser demons |
| `calculating` | Methodical planning | Liches, masterminds |

---

### Damage Type Values

| Value | Description | Source |
|-------|-------------|--------|
| `physical` | Generic physical | Most melee |
| `slashing` | Bladed weapons | Swords, claws |
| `piercing` | Stabbing weapons | Arrows, fangs |
| `bludgeoning` | Blunt weapons | Clubs, fists |
| `fire` | Fire damage | Fire spells, dragons |
| `cold` | Ice/frost damage | Ice spells, elementals |
| `lightning` | Electric damage | Lightning spells |
| `poison` | Toxic damage | Venom, poison gas |
| `acid` | Corrosive damage | Acid breath |
| `magical` | Generic magic | Magic attacks |
| `radiant` | Holy/light damage | Divine spells |
| `necrotic` | Death/shadow damage | Undead, curses |
| `psychic` | Mind damage | Psionics |

---

### Alignment Values (Optional)

| Value | Description |
|-------|-------------|
| `good` | Benevolent |
| `evil` | Malevolent |
| `lawful_good` | Honorable heroes |
| `lawful_evil` | Tyrannical, follows rules |
| `chaotic_good` | Freedom fighters |
| `chaotic_evil` | Destructive chaos |
| `neutral` | Balanced or indifferent |

---

### Social Values

| Value | Description | Examples |
|-------|-------------|----------|
| `solitary` | Lives alone | Bears, hermits |
| `pack` | Small groups | Wolves |
| `tribal` | Tribe structure | Goblins, lizardfolk |
| `clan` | Clan-based | Orcs, dwarves |
| `military` | Organized ranks | Hobgoblins |
| `hive_mind` | Shared consciousness | Insects |
| `aristocratic` | Noble hierarchy | Vampires |
| `cult` | Religious group | Yuan-ti |
| `leadership` | Has leaders | Goblin King |

---

### Element Values (Elementals)

| Value | Description |
|-------|-------------|
| `fire` | Fire elementals |
| `water` | Water elementals |
| `earth` | Earth elementals |
| `air` | Air elementals |
| `lightning` | Lightning/storm |
| `ice` | Ice elementals |

---

## Type-Level vs Individual Traits

### Type-Level Traits

**Applied to ALL items in a category** (defined in `traits` object)

```json
{
  "vampire_types": {
    "lesser_vampires": {
      "traits": {
        "category": "vampire",
        "size": "medium",
        "intelligence": 14,
        "undead": true,
        "regeneration": true,
        "vulnerability": "radiant"
      }
    }
  }
}
```

**All lesser vampires inherit these traits:**
- Vampire category
- Medium size
- Intelligence 14
- Undead
- Regeneration
- Radiant vulnerability

---

### Individual Traits (Overrides)

**Item-specific traits override or extend type-level traits**

```json
{
  "items": [
    {
      "name": "Naga",
      "size": "large",              // Overrides type-level "medium"
      "damageType": "magical",      // Item-specific trait
      "rarityWeight": 45
    }
  ]
}
```

**Override Priority:** Individual > Type-Level

---

## Special Powers: Abilities vs Traits

### ❌ DON'T Use Individual Boolean Traits

**Bad Example:**
```json
{
  "name": "Basilisk",
  "petrifying_gaze": true,    // ❌ Special power as boolean trait
  "venomous": true            // ❌ Attack ability as trait
}
```

### ✅ DO Use Passive/Active Abilities

**Good Example:**
```json
{
  "name": "Basilisk",
  "abilities": [
    "@abilities/active/offensive:petrifying-gaze",
    "@abilities/active/offensive:poison-bite"
  ]
}
```

**Guideline:** If it's an action or special power, it's an ability. If it's an inherent property (undead, incorporeal), it's a trait.

---

## Common Trait Patterns

### Enemy Traits

**Typical enemy type-level traits:**

```json
"traits": {
  "category": "humanoid",      // Entity type
  "size": "medium",            // Physical size
  "intelligence": 10,          // 1-20+ numeric
  "behavior": "aggressive",    // Combat behavior
  "social": "clan",            // Social structure
  "damageType": "physical",    // Primary damage type
  "vulnerability": "fire",     // Weakness
  "regeneration": true         // Special property
}
```

---

### Item Traits

**Typical weapon/armor traits:**

```json
"traits": {
  "slot": "mainhand",          // Equipment slot
  "category": "weapon",        // Item type
  "damageType": "slashing",    // Damage type
  "skillType": "blade",        // Required skill
  "twoHanded": false           // Weapon property
}
```

---

### Consumable Traits

**Typical potion/scroll traits:**

```json
"traits": {
  "slot": "consumable",        // Inventory slot
  "category": "potion",        // Consumable type
  "stackable": true,           // Can stack in inventory
  "oneUse": true              // Consumed on use
}
```

---

## Trait Validation Rules

### Required Type-Level Traits (Enemies)

All enemy types **MUST** have:
- `category` (string)
- `size` (string enum)
- `intelligence` (number 1-20+)
- `behavior` (string enum)

### Optional Common Traits

- `social` - Social structure
- `damageType` - Primary damage type
- `vulnerability` - Weakness
- `immunity` - Damage immunities
- `alignment` - Moral alignment
- Boolean flags (undead, magical, etc.)

---

## Implementation Status

### ✅ Completed (December 28, 2025)

**All 13 enemy catalogs standardized:**
- ✅ Intelligence converted to numeric (1-20+)
- ✅ Individual boolean traits removed (use abilities)
- ✅ Type-level traits consistent
- ✅ All builds passing

**Catalogs Updated:**
1. enemies/humanoids/catalog.json (4 types, 14 enemies)
2. enemies/dragons/catalog.json (5 types, 13 dragons)
3. enemies/beasts/catalog.json (4 types, 15 beasts)
4. enemies/undead/catalog.json (4 types, 14 undead)
5. enemies/goblinoids/catalog.json (4 types, 12 goblinoids)
6. enemies/orcs/catalog.json (3 types, 11 orcs)
7. enemies/demons/catalog.json (4 types, 14 demons)
8. enemies/elementals/catalog.json (4 types, 12 elementals)
9. enemies/trolls/catalog.json (3 types, 10 trolls)
10. enemies/insects/catalog.json (4 types, 14 insects)
11. enemies/plants/catalog.json (4 types, 13 plants)
12. enemies/reptilians/catalog.json (4 types, 13 reptilians)
13. enemies/vampires/catalog.json (3 types, 11 vampires)

**Total Impact:**
- 163 enemies standardized
- 48 enemy types with numeric intelligence
- Zero breaking changes

---

### ⏸️ Deferred

**Item catalogs await item power system design:**
- items/weapons/catalog.json
- items/armor/catalog.json
- items/consumables/catalog.json
- items/materials/catalog.json

**Future features:**
- Enhanced trait objects (temporary buffs)
- Trait stacking system
- C# validation rules

---

## Examples

### Example 1: Dragon Type

```json
{
  "dragon_types": {
    "chromatic": {
      "traits": {
        "category": "dragon",
        "size": "huge",
        "intelligence": 16,
        "behavior": "territorial",
        "alignment": "evil"
      },
      "items": [
        {
          "name": "Red Dragon",
          "health": 300,
          "attack": 35,
          "breathType": "fire",
          "rarityWeight": 80,
          "abilities": [
            "@abilities/active/offensive:dragon-breath",
            "@abilities/active/offensive:claw-attack"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 16 (numeric)
- ✅ Type-level traits apply to all chromatic dragons
- ✅ breathType is item-specific (fire vs acid vs lightning)
- ✅ Dragon-breath is ability, not boolean trait

---

### Example 2: Vampire Type

```json
{
  "vampire_types": {
    "ancient_vampires": {
      "traits": {
        "category": "vampire",
        "size": "medium",
        "intelligence": 20,
        "behavior": "legendary",
        "undead": true,
        "regeneration": true,
        "magical": true,
        "shapeshifter": true,
        "vulnerability": "radiant"
      },
      "items": [
        {
          "name": "Vampire Progenitor",
          "health": 280,
          "attack": 60,
          "level": 18,
          "rarityWeight": 100,
          "abilities": [
            "@abilities/active/offensive:drain-life",
            "@abilities/active/offensive:blood-magic",
            "@abilities/ultimate:blood-god"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 20 (numeric, legendary level)
- ✅ Boolean traits are inherent (undead, regeneration)
- ✅ Special powers are abilities (drain-life, blood-magic)
- ✅ No individual boolean traits on items

---

### Example 3: Troll Type

```json
{
  "troll_types": {
    "common_trolls": {
      "traits": {
        "category": "troll",
        "size": "large",
        "intelligence": 5,
        "behavior": "aggressive",
        "damageType": "physical",
        "regeneration": true,
        "vulnerability": "fire"
      },
      "items": [
        {
          "name": "Troll",
          "health": 120,
          "attack": 30,
          "level": 8,
          "rarityWeight": 12,
          "abilities": [
            "@abilities/active/offensive:basic-attack",
            "@abilities/passive:regeneration"
          ]
        }
      ]
    }
  }
}
```

**Trait Analysis:**
- ✅ intelligence: 5 (numeric, low intelligence)
- ✅ regeneration: true at type level (all trolls regenerate)
- ✅ Regeneration also as passive ability (for game mechanics)
- ✅ Fire vulnerability defined at type level

---

## Migration Guide

### Converting String Intelligence to Numeric

**Before:**
```json
"traits": {
  "intelligence": "medium"
}
```

**After:**
```json
"traits": {
  "intelligence": 10
}
```

**Mapping:**
- `"none"` → 1
- `"low"` → 3-8 (context dependent)
- `"medium"` → 10-12
- `"high"` → 14-16
- `"very_high"` → 18
- `"genius"` → 20+

---

### Removing Individual Boolean Traits

**Before:**
```json
{
  "name": "Basilisk",
  "petrifying_gaze": true,
  "abilities": [...]
}
```

**After:**
```json
{
  "name": "Basilisk",
  "abilities": [
    "@abilities/active/offensive:petrifying-gaze",
    ...
  ]
}
```

**Process:**
1. Identify boolean traits that are special powers
2. Find or create corresponding ability
3. Add ability reference to abilities array
4. Remove boolean trait

---

## Related Standards

- **[CA1 (December 28, 2025)
- ✅ Added Fundamental Concepts section (Traits vs Abilities vs Skills)
- ✅ Added Decision Tree for classification
- ✅ Expanded Physical Traits with Movement subcategory
- ✅ Added Sensory Traits category (future-proofing)
- ✅ Added Profession Traits category (future-proofing)
- ✅ Updated to 11 total categories

### v1.TALOG_JSON_STANDARD.md](CATALOG_JSON_STANDARD.md)** - Catalog structure and rarityWeight
- **[JSON_REFERENCE_STANDARDS.md](JSON_REFERENCE_STANDARDS.md)** - Ability reference syntax
- **[JSON_STRUCTURE_TYPES.md](JSON_STRUCTURE_TYPES.md)** - Overall JSON structure patterns

---

## Changelog

### v1.0 (December 28, 2025)
- ✅ Initial standard created
- ✅ All enemy catalogs standardized
- ✅ Intelligence converted to numeric (1-20+)
- ✅ Individual boolean traits removed
- ✅ Type-level trait patterns established
- ✅ 163 enemies validated with builds passing
