# Class Abilities Mapping - Existing to New Structure

## Summary

**Found**: 19/48 class abilities exist in current catalog
**Missing**: 29/48 need to be created from design document
**Status**: Ready to create class catalogs and migrate

## Warrior (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Second Wind | ✅ FOUND | active/support | `second-wind` |
| Battle Cry | ✅ FOUND | active/support | `battle-cry` |
| Iron Will (passive) | ❌ MISSING | - | Create from design |
| Charge | ❌ MISSING | - | Create from design |
| Whirlwind | ✅ FOUND | active/offensive | `whirlwind` |
| Execute | ✅ FOUND | active/offensive | `execute` |
| Shield Bash | ✅ FOUND | active/offensive | `shield-bash` |
| Last Stand (ultimate) | ✅ FOUND | reactive/defensive OR active/defensive | `last-stand` (2 versions!) |

**Warrior Status**: 6/8 found (75%)

## Rogue (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Backstab | ✅ FOUND | active/offensive | `backstab` |
| Evasion | ✅ FOUND | active/defensive | `evasion` |
| Shadow Affinity (passive) | ❌ MISSING | - | Create from design |
| Poison Strike | ❌ MISSING | - | Create from design |
| Vanish | ✅ FOUND | active/utility | `vanish` |
| Shadow Step | ✅ FOUND | active/mobility | `shadow-step` |
| Assassination (ultimate) | ❌ MISSING | - | Create from design |

**Rogue Status**: 4/8 found (50%)

## Mage (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Arcane Missiles | ❌ MISSING | - | Create from design |
| Mana Shield | ❌ MISSING | - | Create from design (already in spells) |
| Arcane Affinity (passive) | ❌ MISSING | - | Create from design |
| Frost Nova | ❌ MISSING | - | Create from design (possibly a spell) |
| Blink | ✅ FOUND | reactive/utility | `blink` |
| Spell Steal | ❌ MISSING | - | Create from design |
| Meteor (ultimate) | ❌ MISSING | - | Create from design (already in spells) |

**Mage Status**: 1/8 found (12.5%)

## Cleric (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Smite | ✅ FOUND | active/offensive | `smite` |
| Heal | ✅ FOUND | active/support | `heal` |
| Divine Grace (passive) | ❌ MISSING | - | Create from design |
| Divine Shield | ✅ FOUND | active/defensive | `divine-shield` |
| Cleanse | ✅ FOUND | reactive/utility | `cleanse` |
| Blessing | ❌ MISSING | - | Create from design |
| Divine Intervention (ultimate) | ❌ MISSING | - | Create from design |

**Cleric Status**: 4/8 found (50%)

## Ranger (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Power Shot | ❌ MISSING | - | Create from design |
| Trap | ✅ FOUND | active/utility | `trap` |
| Keen Senses (passive) | ❌ MISSING | - | Create from design |
| Hunter's Mark | ✅ FOUND | active/support | `hunters-mark` |
| Camouflage | ✅ FOUND | passive/defensive OR passive/mobility | `camouflage` (2 versions!) |
| Pet Summon | ❌ MISSING | - | Create from design |
| Arrow Storm (ultimate) | ❌ MISSING | - | Create from design |

**Ranger Status**: 3/8 found (37.5%)

## Paladin (8 abilities)

| Ability | Status | Current Location | Notes |
|---------|--------|------------------|-------|
| Holy Strike | ❌ MISSING | - | Create from design |
| Protective Aura | ❌ MISSING | - | Create from design |
| Righteous Vigor (passive) | ❌ MISSING | - | Create from design |
| Divine Smite | ❌ MISSING | - | Create from design |
| Lay on Hands | ✅ FOUND | active/support | `lay-on-hands` |
| Consecration | ❌ MISSING | - | Create from design |
| Judgment (ultimate) | ❌ MISSING | - | Create from design |

**Paladin Status**: 1/8 found (12.5%)

## Overall Status

```
Total Class Abilities: 48
Found in Existing Catalog: 19 (39.6%)
Need to Create: 29 (60.4%)

By Class:
- Warrior: 6/8 (75%) ✅ Best coverage
- Rogue: 4/8 (50%)
- Cleric: 4/8 (50%)
- Ranger: 3/8 (37.5%)
- Mage: 1/8 (12.5%) ❌ Needs most work
- Paladin: 1/8 (12.5%) ❌ Needs most work
```

## Issues Found

### Duplicate Abilities
1. **last-stand**: Exists in both `reactive/defensive` AND `active/defensive`
   - Need to review which version matches design
2. **camouflage**: Exists in both `passive/defensive` AND `passive/mobility`
   - Need to review which version matches design

### Abilities vs Spells Conflict
The following abilities might overlap with spells:
- **Mana Shield**: Should it be a mage ability or a spell? (Currently in spells/alteration)
- **Meteor**: Should it be a mage ultimate or a spell? (Currently in spells/destruction)
- **Heal**: Should it be a cleric ability or a spell? (Currently in both!)
- **Frost Nova**: Not found, but conceptually similar to destruction spells

**Decision**: Keep as class abilities (auto-granted) AND as spells (learnable by anyone)
- Class abilities are more powerful/specialized versions
- Spells are universal access versions requiring skill

## Missing Passive Abilities (6 total)

All class passive abilities need creation:
1. Warrior: **Iron Will** (+10% max HP)
2. Rogue: **Shadow Affinity** (+15% crit damage)
3. Mage: **Arcane Affinity** (+10% max mana, +10% spell damage)
4. Cleric: **Divine Grace** (+5% heal effectiveness, +5% max health)
5. Ranger: **Keen Senses** (+20% detection range, +10% ranged crit chance)
6. Paladin: **Righteous Vigor** (+5% max health, +5% max mana)

## Missing Ultimate Abilities (5 total)

1. Rogue: **Assassination** (massive single-target burst)
2. Mage: **Meteor** (exists in spells, need ability version)
3. Cleric: **Divine Intervention** (prevent ally death)
4. Ranger: **Arrow Storm** (massive AoE ranged attack)
5. Paladin: **Judgment** (divine damage + debuff)

Note: Warrior's **Last Stand** ultimate exists in catalog

## Migration Strategy

### Phase 1: Create Class Ability Catalogs
For each class, create `abilities/classes/{class}/catalog.json` with:
- Found abilities (copy from existing locations)
- Missing abilities (create from design document)
- Ensure all 8 abilities per class

### Phase 2: Handle Duplicates
- Review duplicate abilities (last-stand, camouflage)
- Choose best version or merge attributes
- Delete duplicates from old structure

### Phase 3: Reorganize Enemy Abilities
- Move existing type-based abilities to `abilities/enemy/`
- Remove abilities that were migrated to classes
- Keep enemy-specific abilities organized by type

### Phase 4: Update References
- Search for `@abilities/active/offensive:backstab` patterns
- Update to `@abilities/classes/rogue:backstab`
- Update enemy references if needed

## Next Steps

1. ✅ Read existing ability definitions (whirlwind, execute, backstab, etc.)
2. Create warrior/catalog.json with 6 existing + 2 new abilities
3. Create rogue/catalog.json with 4 existing + 4 new abilities
4. Continue for remaining classes
5. Test with existing references

---

**Status**: Mapping complete, ready to create class catalogs
**Created**: Current Date
**Last Updated**: Current Date
