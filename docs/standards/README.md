# RealmEngine Standards Documentation

This directory contains all technical standards and specifications for the RealmEngine game system.

## 📁 Directory Structure

### `/json/` - JSON Data Standards
Core standards for all JSON data files used throughout the game.

- **CATALOG_JSON_STANDARD.md** - Standards for catalog files (items, enemies, NPCs, etc.)
- **NAMES_JSON_STANDARD.md** - Pattern-based name generation standards
- **CBCONFIG_STANDARD.md** - ContentBuilder configuration file standards
- **JSON_REFERENCE_STANDARDS.md** - Reference system (v4.1) for linking game data
- **JSON_STRUCTURE_TYPES.md** - Common JSON structures and type definitions
- **TRAIT_STANDARDS.md** - Trait system specifications
- **README.md** - Overview of JSON standards
- `/templates/` - Template files for common JSON structures
  - **ATTRIBUTE_TEMPLATES.md** - Attribute system templates

### `/patterns/` - Design Patterns
Reusable design patterns and component standards.

- **PATTERN_COMPONENT_STANDARDS.md** - Component-based design patterns

### `/systems/` - Game Systems
Core game system specifications.

- **WEIGHT_BASED_RARITY_SYSTEM.md** - Rarity weight system specification
- **ITEM_ENHANCEMENT_SYSTEM.md** - Item enhancement and upgrade system

### `/proposals/` - Standards Proposals
Draft proposals for new or evolving standards.

- **TRAIT_STANDARDIZATION_PROPOSAL.md** - Proposed trait system improvements

## 📄 Root Standards

- **JSON_STRUCTURE_STANDARDS.md** - General JSON structure guidelines
- **TYPES_JSON_STRUCTURE.md** - Type system definitions

## 🎯 Quick Reference

### JSON Data Files
All game data files follow the standards in `/json/`. Key version:
- **JSON Standards**: v4.0
- **Reference System**: v4.1

### Reference Syntax
```
@domain/path/category:item-name[filters]?.property.nested
```

**Examples:**
- `@abilities/active/offensive:basic-attack`
- `@items/weapons/swords:iron-longsword`
- `@enemies/humanoid:goblin-warrior`

### File Types
- **catalog.json** - Item/enemy/NPC definitions
- **names.json** - Pattern-based name generation
- **.cbconfig.json** - ContentBuilder UI configuration

## 📚 Related Documentation

- **GDD-Main.md** (in parent folder) - Main game design document
- **Archives** (in parent folder) - Historical documentation and completed migrations

## 🔍 Finding Standards

| What are you looking for? | Where to go |
|---------------------------|-------------|
| JSON data file format | `/json/CATALOG_JSON_STANDARD.md` or `/json/NAMES_JSON_STANDARD.md` |
| Reference system | `/json/JSON_REFERENCE_STANDARDS.md` |
| Rarity weights | `/systems/WEIGHT_BASED_RARITY_SYSTEM.md` |
| Name generation | `/json/NAMES_JSON_STANDARD.md` |
| Item enhancements | `/systems/ITEM_ENHANCEMENT_SYSTEM.md` |
| Trait system | `/json/TRAIT_STANDARDS.md` |
| General JSON structure | `JSON_STRUCTURE_STANDARDS.md` |

## 📝 Version History

- **v4.1** (December 2025) - JSON Reference System introduced
- **v4.0** (December 2025) - Comprehensive JSON standards established
- **v3.x** (Archived) - Previous standards (see archives)

---

*Last Updated: January 2026*
