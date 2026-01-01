# Enemy Ability Integration - Implementation Guide

**Date**: December 28, 2025  
**Status**: IN PROGRESS (2 of 13 enemy catalogs completed)  
**Standard**: JSON Reference System v4.1

---

## Overview

Integrating level-based ability unlocks into all enemy catalogs, following the pattern established in player classes. Enemies now scale naturally with progression, unlocking more powerful abilities as they level up.

---

## Implementation Pattern

### Structure
```json
{
  "name": "Enemy Name",
  "health": 100,
  "attack": 15,
  "defense": 10,
  "speed": 12,
  "level": 8,
  "xp": 150,
  "rarityWeight": 10,
  "abilities": [
    "@abilities/active/offensive:basic-attack",
    "@abilities/active/offensive:special-attack"
  ],
  "abilityUnlocks": {
    "10": ["@abilities/active/defensive:shield"],
    "12": ["@abilities/active/offensive:power-attack"],
    "15": ["@abilities/passive:mastery"],
    "20": ["@abilities/ultimate:final-form"]
  }
}
```

### Field Definitions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `abilities` | array | Yes | Starting abilities available at base level |
| `abilityUnlocks` | object | No | Map of level → ability array for progression |

### Ability Reference Format
All abilities use JSON Reference System v4.1:
- `@abilities/active/offensive:ability-name`
- `@abilities/active/defensive:ability-name`
- `@abilities/active/utility:ability-name`
- `@abilities/active/support:ability-name`
- `@abilities/active/mobility:ability-name`
- `@abilities/active/summon:ability-name`
- `@abilities/passive:ability-name`
- `@abilities/ultimate:ability-name`

---

## Completed Enemy Types

### ✅ 1. Humanoids (14 enemies)

**Bandits** (4 enemies, levels 3-6):
- Basic rogue abilities (backstab, sneak-attack)
- Utility abilities (steal, lockpicking)
- Mobility (shadow-step at higher levels)

**Soldiers** (4 enemies, levels 6-11):
- Warrior progression (basic-attack → power-strike → cleave → devastating-blow)
- Support abilities (battle-cry, second-wind, taunt)
- Defensive abilities (defend, indomitable)
- Champion unlocks ultimate: ragnarok (level 25)

**Specialists** (4 enemies, levels 7-9):
- **Assassin**: Rogue abilities → shadow-master passive → death-incarnate ultimate
- **Archer**: Bow abilities → hunters-mark → volley
- **Mage**: Magic progression → meteor → archmages-supremacy ultimate
- **Berserker**: Rage abilities → unstoppable passive → ragnarok ultimate

**Cultists** (2 enemies, levels 4-6):
- **Cultist**: Necromancy (drain-life, death-bolt, summon-skeleton)
- **Zealot**: Cleric abilities (smite, bless, divine-shield, holy-nova)

**Key Design Decisions:**
- Low-level enemies (3-6): 1-2 starting abilities, 1-2 unlocks
- Mid-level enemies (7-11): 2-4 starting abilities, 3-4 unlocks
- Boss enemies (11+): 4+ starting abilities, unlocks including ultimate

---

### ✅ 2. Dragons (13 enemies)

**Chromatic Dragons** (5 enemies, levels 16-20):
- All start with: claw-attack, bite, dragon-breath
- Unlock frightful-presence at +2 levels
- Unlock element-specific abilities (poison-cloud, acid-pool, ice-storm, tail-sweep, wing-buffet)
- Unlock ultimates at +8-10 levels (ancient-power, storm-fury, plague-bearer, corruption-wave, blizzard)

**breathType Trait Integration:**
- Dragons have `breathType` trait (fire, lightning, poison, acid, cold)
- `dragon-breath` ability behavior varies based on breathType
- Element-specific unlocks match breathType

**Power Scaling:**
- Red Dragon (level 20): Most powerful, unlocks ancient-power at 30
- White Dragon (level 16): Least powerful chromatic, unlocks blizzard at 25

---

## Remaining Enemy Types (11 catalogs)

### 3. Metallic Dragons (5 enemies, levels 17-22)
**Recommendation**: Similar to chromatic but with good-aligned abilities
- Gold Dragon: holy-aligned abilities, divine ultimates
- Silver/Bronze: Support abilities, protective powers
- Copper/Brass: Trickster abilities, charm effects

### 4. Drakes (3 enemies, levels 11-14)
**Recommendation**: Lesser dragon abilities, no ultimates
- Basic dragon attacks (bite, claw)
- Limited breath weapons
- No frightful-presence until higher levels

### 5. Undead (varies)
**Recommendation**: Necromancy and death-themed abilities
- Zombies/Skeletons: Basic attacks, grab, infectious bite
- Ghosts/Wraiths: Drain-life, phase, possession
- Liches/Death Knights: Full necromancer/dark knight progressions

### 6. Demons (varies)
**Recommendation**: Fire/chaos abilities, summoning
- Lesser demons: Fire attacks, corruption
- Greater demons: Summon minions, hellfire, demon-lord ultimate
- Demon lords: Multiple ultimates, reality-warping powers

### 7. Elementals (4 types)
**Recommendation**: Element-specific abilities
- Fire: Fireball, immolate, phoenix-rebirth
- Water: Ice-shard, tidal-wave, freeze
- Earth: Stone-skin, earthquake, mountain-stance
- Air: Lightning-bolt, whirlwind, storm-call

### 8. Undead (varies)
**Recommendation**: Life-drain and fear abilities
- Skeletons: Basic melee, bone-shield
- Zombies: Infectious-bite, horde-call
- Vampires: Drain-life, charm, bat-form, mist-form
- Liches: Full necromancer spell list

### 9. Beasts (varies)
**Recommendation**: Natural animal abilities
- Wolves: Pack-tactics, howl, bite, pounce
- Bears: Maul, roar, hibernate
- Big cats: Stealth, pounce, rake
- Dire variants: Enhanced versions + rage

### 10. Goblinoids (varies)
**Recommendation**: Swarm tactics and tricks
- Goblins: Backstab, flee, swarm-tactics
- Hobgoblins: Military tactics, formations
- Bugbears: Ambush, intimidate

### 11. Orcs (varies)
**Recommendation**: Brutal melee and rage
- Grunts: Basic attacks, berserk
- Warriors: Power-strike, cleave
- Chieftains: Battle-cry, war-stomp, inspire

### 12. Trolls (varies)
**Recommendation**: Regeneration and brute strength
- Basic: Regeneration, slam, rend
- Advanced: Fast-healing, throw-rock
- Boss: Unstoppable, titanic-strength

### 13. Insects (varies)
**Recommendation**: Swarm mechanics and poison
- Spiders: Web, poison-bite, egg-sac
- Scorpions: Sting, grab, poison
- Giant ants: Acid-spray, swarm, queen-call

### 14. Plants (varies)
**Recommendation**: Entangle and nature magic
- Vines: Entangle, constrict, root
- Carnivorous: Bite, digest, lure
- Treants: Stomp, root-burst, nature-call

### 15. Reptilians (varies)
**Recommendation**: Various based on type
- Lizardfolk: Tribal warrior abilities
- Kobolds: Traps, ambush, pack-tactics
- Yuan-ti: Snake abilities, hypnosis, venom

### 16. Vampires (varies)
**Recommendation**: Blood magic and transformation
- Spawn: Drain-life, regeneration
- Vampires: Charm, bat-form, mist-form
- Lords: Dominate, blood-storm, ancient-vampire ultimate

---

## Ability Creation Status

### Existing Abilities (from previous work)
✅ All player class abilities created (190 total)
✅ Core combat abilities (basic-attack, power-strike, cleave, etc.)
✅ Class-specific abilities (backstab, fireball, smite, etc.)
✅ Ultimate abilities (ragnarok, death-incarnate, archmages-supremacy, etc.)

### Missing Enemy-Specific Abilities

**Dragon Abilities** (need to create):
- `claw-attack` - Basic dragon melee
- `bite` - Dragon bite attack
- `dragon-breath` - Breath weapon (varies by type)
- `tail-sweep` - Area knockback
- `wing-buffet` - Knockback and stun
- `frightful-presence` - Fear aura
- `poison-cloud` - Poison AOE (Green Dragon)
- `acid-pool` - Acid DOT zone (Black Dragon)
- `ice-storm` - Cold AOE (White Dragon)
- `storm-fury` - Lightning ultimate (Blue Dragon)
- `plague-bearer` - Disease ultimate (Green Dragon)
- `corruption-wave` - Chaos ultimate (Black Dragon)
- `ancient-power` - Generic dragon ultimate (Red Dragon)
- `blizzard` - Ice ultimate (White Dragon)

**Undead Abilities** (need to create):
- `life-drain` - Already exists (necromancer)
- `infectious-bite` - Zombie attack
- `horde-call` - Summon zombies
- `phase` - Ghost intangibility
- `possession` - Mind control

**Beast Abilities** (need to create):
- `pounce` - Leap attack
- `maul` - Bear attack
- `rake` - Claw flurry
- `howl` - Wolf buff
- `hibernate` - Bear defensive

**Insect Abilities** (need to create):
- `web` - Entangle
- `poison-bite` - Venomous attack
- `egg-sac` - Summon spiderlings
- `acid-spray` - Acid ranged

---

## Migration Steps

### For Each Enemy Catalog:

**1. Update Metadata**
```json
"metadata": {
  "version": "4.0",
  "lastUpdated": "2025-12-28",
  "notes": [
    "Abilities use JSON Reference System v4.1",
    "Level-based ability unlocks provide enemy progression"
  ]
}
```

**2. Add Abilities to Each Enemy**
- Review enemy level and role
- Add 1-4 starting abilities based on level
- Add abilityUnlocks map for progression
- Ensure abilities match enemy theme/type

**3. Validate References**
- All ability references must exist in abilities catalogs
- Create missing abilities if needed
- Test with ability reference validator

**4. Build and Test**
```bash
dotnet build Game.sln
# Should succeed with no errors
```

---

## Ability Design Guidelines

### Starting Abilities (by level)
- **Levels 1-5**: 1-2 abilities (basic attacks, one special)
- **Levels 6-10**: 2-3 abilities (attacks + utility/defense)
- **Levels 11-15**: 3-4 abilities (full combat kit)
- **Levels 16-20**: 4-5 abilities (advanced powers)
- **Levels 20+**: 5+ abilities (boss tier)

### Unlock Progression
- First unlock: +2-4 levels from base
- Subsequent unlocks: Every 2-5 levels
- Passive abilities: Mid-to-late progression
- Ultimate abilities: 10-15 levels above base

### Thematic Consistency
- Melee enemies: Offensive/defensive abilities
- Ranged enemies: Offensive/utility abilities
- Magic enemies: Offensive/support abilities
- Boss enemies: Mix of everything + ultimate

---

## Next Steps

### Immediate (Priority 1)
1. ✅ Complete humanoids catalog
2. ✅ Complete dragons catalog (chromatic)
3. ⏳ Complete dragons catalog (metallic + drakes)
4. ⏳ Create missing dragon abilities (14 abilities)

### Short Term (Priority 2)
5. Complete undead catalog (high variety)
6. Complete demons catalog (summoning theme)
7. Complete elementals catalog (4 types)
8. Create missing undead/demon abilities

### Medium Term (Priority 3)
9. Complete beasts catalog (natural abilities)
10. Complete goblinoids catalog (swarm tactics)
11. Complete orcs catalog (brutal melee)
12. Complete trolls catalog (regeneration)

### Long Term (Priority 4)
13. Complete insects catalog (poison/swarm)
14. Complete plants catalog (entangle)
15. Complete reptilians catalog (varied)
16. Complete vampires catalog (blood magic)
17. Create all missing enemy-specific abilities

---

## Validation Checklist

For each completed enemy catalog:

- [ ] Metadata updated to v4.0
- [ ] lastUpdated set to current date
- [ ] All enemies have `abilities` array
- [ ] Ability unlocks make thematic sense
- [ ] Ability references follow v4.1 format
- [ ] Build succeeds with no errors
- [ ] All referenced abilities exist in abilities catalogs

---

## Implementation Statistics

| Status | Count | Catalogs |
|--------|-------|----------|
| ✅ Complete | 2 | humanoids, dragons (partial) |
| ⏳ In Progress | 1 | dragons (metallic/drakes) |
| ⏸️ Pending | 10 | undead, demons, elementals, beasts, goblinoids, orcs, trolls, insects, plants, reptilians, vampires |

**Total Progress**: 2/13 enemy catalogs (15%)

**Estimated Remaining Work**:
- 11 enemy catalogs to update
- ~50-100 missing enemy-specific abilities to create
- ~2-4 hours of work

---

**Status**: Ready for batch implementation  
**Next Action**: Complete remaining dragon types, then tackle undead
