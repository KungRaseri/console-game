#!/usr/bin/env python3
"""
JSON v4.x to v5.1 Migration Script

Converts catalog JSON files from v4.0/v4.1/v4.2 structure to v5.1:
- Separates attributes (STR, DEX, CON, INT, WIS, CHA) into attributes object
- Converts stat values to formula strings with _mod references
- Extracts type-level properties from items
- Converts traits array to traits object
- Converts dice notation to structured damage objects
- Adds combat section for enemies
"""

import json
import sys
import os
import re
from pathlib import Path
from typing import Dict, Any, List, Optional

# Field categorization for v5.1
ROOT_LEVEL_FIELDS = {'slug', 'name', 'description', 'level', 'xp', 'rarity', 'rarityWeight', 
                     'weight', 'value', 'budgetCost', 'selectionWeight'}

ATTRIBUTES = {'strength', 'dexterity', 'constitution', 'intelligence', 'wisdom', 'charisma'}

STAT_FIELDS = {'health', 'mana', 'stamina', 'attack', 'defense', 'speed', 'magicPower', 
               'critChance', 'critDamage', 'evasion', 'accuracy'}

ENEMY_FIELDS = {'abilities', 'abilityUnlocks', 'resistances', 'vulnerabilities', 'immunities'}

TYPE_PROPERTIES = {'size', 'behavior', 'habitat', 'damageType', 'slot', 'category', 
                   'armorType', 'skillReference', 'finesse', 'reach', 'requiresAmmo', 
                   'magicFocus', 'twoHanded'}

def calculate_modifier(attribute_value: int) -> int:
    """Calculate D&D-style modifier: (attribute - 10) / 2, rounded down"""
    return (attribute_value - 10) // 2

def create_formula_for_stat(stat_name: str, base_value: Any) -> str:
    """Convert a stat value to a formula string"""
    if isinstance(base_value, str):
        # Already a formula or reference
        return base_value
    
    # Convert numeric values to formula strings
    value = int(base_value) if isinstance(base_value, (int, float)) else 0
    
    # Common formula patterns
    if stat_name == 'health':
        return f"constitution_mod * 2 + level * 5 + {value}"
    elif stat_name == 'mana':
        return f"intelligence_mod * 3 + level * 3 + {value}"
    elif stat_name == 'stamina':
        return f"constitution_mod * 2 + level * 2 + {value}"
    elif stat_name == 'attack':
        return f"strength_mod + level + {value}"
    elif stat_name == 'defense':
        return f"10 + dexterity_mod + constitution_mod + {value}"
    elif stat_name == 'speed':
        return f"30 + dexterity_mod * 5"
    elif stat_name == 'magicPower':
        return f"intelligence_mod + wisdom_mod + level"
    else:
        return str(value)

def create_damage_object(damage_str: str) -> Dict[str, Any]:
    """Convert dice notation like '1d8' or '2d6' to structured damage object"""
    if not isinstance(damage_str, str):
        return {"min": 1, "max": 4}
    
    # Parse dice notation
    match = re.match(r'(\d+)d(\d+)', damage_str)
    if match:
        dice_count = int(match.group(1))
        dice_sides = int(match.group(2))
        return {
            "min": dice_count,
            "max": dice_count * dice_sides,
            "modifier": "wielder.strength_mod"  # Default for weapons
        }
    
    # Parse range notation
    match = re.match(r'(\d+)-(\d+)', damage_str)
    if match:
        return {
            "min": int(match.group(1)),
            "max": int(match.group(2)),
            "modifier": "wielder.strength_mod"
        }
    
    # Default fallback
    return {"min": 1, "max": 4}

def migrate_item(item: Dict[str, Any], is_enemy: bool = False) -> Dict[str, Any]:
    """Migrate a single item to v5.1 structure"""
    migrated = {}
    
    # 1. Keep root-level fields
    for field in ROOT_LEVEL_FIELDS:
        if field in item:
            migrated[field] = item[field]
    
    # Add rarity field if missing (calculate from rarityWeight)
    if 'rarity' not in migrated and 'rarityWeight' in migrated:
        weight = migrated['rarityWeight']
        # Map rarityWeight to rarity value (1-100 scale)
        if weight <= 5:
            migrated['rarity'] = 15  # Common
        elif weight <= 10:
            migrated['rarity'] = 30  # Uncommon
        elif weight <= 15:
            migrated['rarity'] = 50  # Rare
        elif weight <= 20:
            migrated['rarity'] = 70  # Epic
        else:
            migrated['rarity'] = 90  # Legendary
    
    # 2. Extract attributes into attributes object
    attributes = {}
    for attr in ATTRIBUTES:
        if attr in item:
            attributes[attr] = item[attr]
        else:
            # Provide default attributes if missing
            if attr == 'strength':
                attributes[attr] = 10
            elif attr == 'dexterity':
                attributes[attr] = 10
            elif attr == 'constitution':
                attributes[attr] = 10
            elif attr == 'intelligence':
                attributes[attr] = 8
            elif attr == 'wisdom':
                attributes[attr] = 8
            elif attr == 'charisma':
                attributes[attr] = 8
    
    if attributes:
        migrated['attributes'] = attributes
    
    # 3. Extract stats and convert to formulas
    stats = {}
    for stat in STAT_FIELDS:
        if stat in item:
            stats[stat] = create_formula_for_stat(stat, item[stat])
    
    if stats:
        migrated['stats'] = stats
    
    # 4. Handle combat section for enemies
    if is_enemy:
        combat = {}
        for field in ENEMY_FIELDS:
            if field in item:
                combat[field] = item[field]
        
        # Ensure abilities array exists
        if 'abilities' not in combat:
            combat['abilities'] = []
        
        if combat:
            migrated['combat'] = combat
    
    # 5. Convert damage to structured object (for weapons)
    if 'damage' in item:
        if isinstance(item['damage'], str):
            migrated['damage'] = create_damage_object(item['damage'])
        else:
            migrated['damage'] = item['damage']
    
    # 6. Handle traits - convert array to object or keep object
    if 'traits' in item:
        if isinstance(item['traits'], list):
            # Convert trait array to object
            traits_obj = {}
            for trait in item['traits']:
                if isinstance(trait, dict) and 'key' in trait:
                    traits_obj[trait['key']] = trait.get('value', True)
            migrated['traits'] = traits_obj if traits_obj else {}
        else:
            migrated['traits'] = item.get('traits', {})
    else:
        migrated['traits'] = {}
    
    # 7. Copy any remaining fields not categorized
    for key, value in item.items():
        if key not in ROOT_LEVEL_FIELDS and key not in ATTRIBUTES and \
           key not in STAT_FIELDS and key not in ENEMY_FIELDS and \
           key not in TYPE_PROPERTIES and key not in ['damage', 'traits']:
            migrated[key] = value
    
    return migrated

def migrate_type_structure(type_data: Dict[str, Any], type_key: str, is_enemy: bool = False) -> Dict[str, Any]:
    """Migrate a type structure (e.g., wolves, heavy-blades)"""
    migrated_type = {}
    
    # 1. Extract type-level properties
    properties = {}
    for prop in TYPE_PROPERTIES:
        if prop in type_data:
            properties[prop] = type_data[prop]
        # Also check in traits if it exists
        elif 'traits' in type_data and isinstance(type_data['traits'], dict):
            if prop in type_data['traits']:
                properties[prop] = type_data['traits'][prop]
    
    if properties:
        migrated_type['properties'] = properties
    
    # 2. Migrate items array
    if 'items' in type_data and isinstance(type_data['items'], list):
        migrated_type['items'] = [migrate_item(item, is_enemy) for item in type_data['items']]
    
    return migrated_type

def migrate_catalog(data: Dict[str, Any], is_enemy: bool = False) -> Dict[str, Any]:
    """Migrate entire catalog to v5.1"""
    migrated = {}
    
    # 1. Update metadata
    if 'metadata' in data:
        metadata = data['metadata'].copy()
        metadata['version'] = '5.1'
        metadata['lastUpdated'] = '2026-01-08'
        migrated['metadata'] = metadata
    
    # 2. Find and migrate *_types structure
    for key in data.keys():
        if key.endswith('_types'):
            types_obj = data[key]
            migrated_types = {}
            
            for type_key, type_data in types_obj.items():
                migrated_types[type_key] = migrate_type_structure(type_data, type_key, is_enemy)
            
            migrated[key] = migrated_types
            break
    
    return migrated

def process_file(input_path: Path, output_path: Optional[Path] = None, is_enemy: bool = False):
    """Process a single JSON catalog file"""
    print(f"Processing: {input_path}")
    
    try:
        with open(input_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        migrated = migrate_catalog(data, is_enemy)
        
        output = output_path or input_path
        with open(output, 'w', encoding='utf-8') as f:
            json.dump(migrated, f, indent=2, ensure_ascii=False)
        
        print(f" Migrated to: {output}")
        return True
    except Exception as e:
        print(f" Error processing {input_path}: {e}")
        return False

def main():
    """Main entry point"""
    import argparse
    
    parser = argparse.ArgumentParser(description='Migrate JSON catalogs from v4.x to v5.1')
    parser.add_argument('input', nargs='?', help='Input JSON file or directory')
    parser.add_argument('-o', '--output', help='Output file (default: overwrite input)')
    parser.add_argument('--enemy', action='store_true', help='Treat as enemy catalog')
    parser.add_argument('--all-enemies', action='store_true', help='Migrate all enemy catalogs')
    parser.add_argument('--all-items', action='store_true', help='Migrate all item catalogs')
    parser.add_argument('--dry-run', action='store_true', help='Show what would be migrated')
    
    args = parser.parse_args()
    
    base_dir = Path('RealmEngine.Data/Data/Json')
    
    if args.all_enemies:
        enemy_dirs = ['beasts', 'demons', 'dragons', 'elementals', 'goblinoids', 
                      'humanoids', 'insects', 'orcs', 'plants', 'reptilians', 
                      'trolls', 'undead', 'vampires']
        
        for enemy_dir in enemy_dirs:
            catalog_path = base_dir / 'enemies' / enemy_dir / 'catalog.json'
            if catalog_path.exists():
                if not args.dry_run:
                    process_file(catalog_path, is_enemy=True)
                else:
                    print(f"Would migrate: {catalog_path}")
        
    elif args.all_items:
        item_paths = [
            'items/armor/catalog.json',
            'items/weapons/catalog.json',
            'items/consumables/catalog.json'
        ]
        
        for item_path in item_paths:
            full_path = base_dir / item_path
            if full_path.exists():
                if not args.dry_run:
                    process_file(full_path, is_enemy=False)
                else:
                    print(f"Would migrate: {full_path}")
    
    elif args.input:
        input_path = Path(args.input)
        output_path = Path(args.output) if args.output else None
        
        if not args.dry_run:
            process_file(input_path, output_path, is_enemy=args.enemy)
        else:
            print(f"Would migrate: {input_path} -> {output_path or input_path}")
    
    else:
        parser.print_help()
        return 1
    
    return 0

if __name__ == '__main__':
    sys.exit(main())

