#!/usr/bin/env python3
"""
JSON v4.x to v5.1 Migration Script

Converts JSON catalogs to v5.1 structure with attributes/stats/properties/traits separation.

Usage:
    python migrate-json-to-v5.py <path-to-catalog.json>
    python migrate-json-to-v5.py --all  # Migrate all catalogs
    python migrate-json-to-v5.py --enemies  # Migrate only enemies
    python migrate-json-to-v5.py --items  # Migrate only items
"""

import json
import sys
import os
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Any, Optional
import re

# Root-level meta fields
ROOT_LEVEL_FIELDS = {
    'slug',
    'name',
    'rarity',
    'rarityWeight',
    'selectionWeight',
    'level',  # For enemies
    'xp'      # For enemies
}

# Standard 6 attributes
ATTRIBUTES = {
    'strength', 'dexterity', 'constitution',
    'intelligence', 'wisdom', 'charisma'
}

# Stats (calculated values)
STAT_FIELDS = {
    'health', 'attack', 'defense', 'speed',
    'damage', 'attackSpeed', 'weight', 'value',
    'healing', 'duration', 'manaCost', 'cooldown',
    'dodgeBonus', 'rangeMin', 'rangeMax'
}

# Type-level properties (descriptive characteristics)
TYPE_PROPERTIES = {
    'category', 'size', 'behavior', 'habitat',
    'damageType', 'range', 'slot', 'skillReference',
    'consumableType', 'effectType', 'targetType'
}

# Combat-specific (enemies only)
COMBAT_FIELDS = {
    'abilities', 'abilityUnlocks'
}

# Special traits/modifiers (everything else)
TRAIT_FIELDS = {
    'packLeader', 'legendary', 'questBoss',
    'resistances', 'vulnerabilities', 'immunities',
    'sizeOverride', 'damageTypeOverride',
    'twoHanded', 'socketCount', 'throwable', 'finesse',
    'ammoType', 'stealthPenalty', 'stackSize', 'enchantable'
}


def extract_properties(type_obj: Dict[str, Any]) -> Dict[str, Any]:
    """Extract type-level properties from traits or root."""
    properties = {}
    
    # Check if traits exist (v4.x structure)
    if 'traits' in type_obj and isinstance(type_obj['traits'], dict):
        for key, value in type_obj['traits'].items():
            if key in TYPE_PROPERTIES:
                properties[key] = value
    
    return properties


def migrate_type_structure(type_obj: Dict[str, Any]) -> Dict[str, Any]:
    """Migrate type-level object to v5.1 structure."""
    result = {}
    
    # Extract properties
    result['properties'] = extract_properties(type_obj)
    
    # Migrate items
    if 'items' in type_obj:
        result['items'] = [migrate_item(item) for item in type_obj['items']]
    else:
        result['items'] = []
    
    return result


def create_damage_object(damage_value: Any) -> Dict[str, Any]:
    """Convert damage string/number to structured object."""
    if isinstance(damage_value, str):
        # Try to parse "1d8" format
        match = re.match(r'(\d+)d(\d+)', damage_value)
        if match:
            dice_count = int(match.group(1))
            dice_sides = int(match.group(2))
            return {
                "min": dice_count,
                "max": dice_count * dice_sides,
                "modifier": "wielder.strength_mod"  # Default assumption
            }
    
    # Default fallback
    return {
        "min": 1,
        "max": 8,
        "modifier": "wielder.strength_mod"
    }


def generate_stat_formula(stat_name: str, stat_value: Any, attributes: Dict[str, int]) -> str:
    """Generate placeholder formula for a stat (to be manually tuned)."""
    # These are placeholder formulas - should be manually reviewed
    formulas = {
        'health': f"constitution_mod * 2 + level * 5 + 10",
        'attack': f"strength_mod + level",
        'defense': f"10 + dexterity_mod + constitution_mod",
        'speed': f"30 + dexterity_mod * 5"
    }
    
    return formulas.get(stat_name, str(stat_value))


def migrate_item(item: Dict[str, Any]) -> Dict[str, Any]:
    """Migrate item from v4.x to v5.1."""
    result = {}
    attributes = {}
    stats = {}
    combat = {}
    traits = {}
    
    # Separate fields by category
    for key, value in item.items():
        if key in ROOT_LEVEL_FIELDS:
            result[key] = value
        elif key in ATTRIBUTES:
            attributes[key] = value
        elif key in STAT_FIELDS:
            if key == 'damage' and isinstance(value, str):
                stats[key] = create_damage_object(value)
            else:
                stats[key] = value
        elif key in COMBAT_FIELDS:
            combat[key] = value
        elif key in TRAIT_FIELDS or key not in ROOT_LEVEL_FIELDS:
            # Check if it's an attribute bonus (should be in traits)
            if key in ATTRIBUTES:
                traits[key] = value
            else:
                traits[key] = value
    
    # Add sections
    result['attributes'] = attributes if attributes else {}
    result['stats'] = stats if stats else {}
    
    if combat:
        result['combat'] = combat
    
    result['traits'] = traits if traits else {}
    
    return result


def calculate_rarity_tier(rarity_weight: int) -> int:
    """Convert rarityWeight to numerical rarity (1-100)."""
    if rarity_weight <= 10:
        return 15  # Common (1-20)
    elif rarity_weight <= 30:
        return 30  # Uncommon (21-40)
    elif rarity_weight <= 60:
        return 50  # Rare (41-60)
    elif rarity_weight <= 85:
        return 70  # Elite (61-80)
    else:
        return 95  # Legendary (81-100)


def add_rarity_field(item: Dict[str, Any]) -> None:
    """Add numerical rarity field based on rarityWeight if missing."""
    if 'rarity' not in item and 'rarityWeight' in item:
        item['rarity'] = calculate_rarity_tier(item['rarityWeight'])


def migrate_catalog(data: Dict[str, Any]) -> Dict[str, Any]:
    """Migrate entire catalog from v4.x to v5.1."""
    result = {}
    
    # Update metadata
    if 'metadata' in data:
        metadata = data['metadata'].copy()
        metadata['version'] = '5.1'
        metadata['lastUpdated'] = datetime.now().strftime('%Y-%m-%d')
        
        # Ensure proper catalog type naming
        if 'type' in metadata:
            if not metadata['type'].endswith('_catalog'):
                metadata['type'] = metadata['type'] + '_catalog'
        
        result['metadata'] = metadata
    
    # Find and migrate type collections  
    for key, value in data.items():
        if key == 'metadata':
            continue
        
        # Check if this is a type collection (ends with "_types")
        if isinstance(value, dict) and key.endswith('_types'):
            result[key] = {}
            
            for type_name, type_obj in value.items():
                result[key][type_name] = migrate_type_structure(type_obj)
                
                # Add rarity to items if missing
                for item in result[key][type_name]['items']:
                    add_rarity_field(item)
    
    return result


def migrate_file(input_path: Path, output_path: Path = None, in_place: bool = False):
    """Migrate a single JSON file."""
    print(f"Migrating: {input_path}")
    
    # Read input
    with open(input_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    # Check version
    version = data.get('metadata', {}).get('version', 'unknown')
    if version == '5.1':
        print(f"  ⚠️ Already v5.1, skipping")
        return
    
    # Migrate
    migrated = migrate_catalog(data)
    
    # Write output
    if in_place:
        output_path = input_path
    elif output_path is None:
        output_path = input_path.parent / f"{input_path.stem}_v5.1{input_path.suffix}"
    
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(migrated, f, indent=2, ensure_ascii=False)
    
    print(f"  ✅ Migrated to: {output_path}")
    print(f"  ⚠️ NOTE: Review stats formulas - placeholders were generated
    print(f"  ✅ Migrated to: {output_path}")

in_directory(data_path: Path, subdirectory: str, in_place: bool = False):
    """Migrate all catalog files in a subdirectory."""
    target_path = data_path / 'RealmEngine.Data' / 'Data' / 'Json' / subdirectory
    
    if not target_path.exists():
        print(f"❌ Directory not found: {target_path}")
        return
    
    catalog_files = list(target_path.glob('**/catalog.json'))
    print(f"Found {len(catalog_files)} {subdirectory} catalogs to migrate\n")
    
    success_count = 0
    for catalog_file in catalog_files:
        try:
            migrate_file(catalog_file, in_place=in_place)
            success_count += 1
        except Exception as e:
            print(f"  ❌ Error: {e}")
    
    print(f"\n✅ Successfully migrated {success_count}/{len(catalog_files)} files")


def migrate_all_enemies(data_path: Path, in_place: bool = False):
    """Migrate all enemy catalog files."""
    migrate_all_in_directory(data_path, 'enemies', in_place)
script_dir = Path(__file__).parent
    workspace_root = script_dir.parent
    in_place = '--in-place' in sys.argv
    
    if arg == '--all':
        print("Migrating all catalog files to v5.1...\n")
        migrate_all_enemies(workspace_root, in_place)
        migrate_all_items(workspace_root, in_place)
        print("\n✅ Migration complete!")
        print("⚠️ IMPORTANT: Review all stats formulas - placeholders were generated")
    elif arg == '--enemies':
        migrate_all_enemies(workspace_root, in_place)
    elif arg == '--items':
        migrate_all_items(workspace_root, in_place)
    else:
        input_path = Path(arg)
        if not input_path.exists():
            print(f"❌ File not found: {input_path}")
            sys.exit(1)
        
    
    if arg == '--all':
        # Find workspace root
        script_dir = Path(__file__).parent
        workspace_root = script_dir.parent
        
        in_place = '--in-place' in sys.argv
        migrate_all_enemies(workspace_root, in_place)
    else:
        input_path = Path(arg)
        if not input_path.exists():
            print(f"❌ File not found: {input_path}")
            sys.exit(1)
        
        in_place = '--in-place' in sys.argv
        migrate_file(input_path, in_place=in_place)


if __name__ == '__main__':
    main()
