#!/usr/bin/env python3
"""
JSON v4.0 to v5.0 Migration Script

Converts JSON catalogs from v4.0 (object-based traits) to v5.0 (trait arrays).

Usage:
    python migrate-json-to-v5.py <path-to-catalog.json>
    python migrate-json-to-v5.py --all  # Migrate all enemy catalogs
"""

import json
import sys
import os
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Any

# Fields that stay at root level (not moved to traits)
ROOT_LEVEL_FIELDS = {
    'slug',
    'name',
    'rarity',
    'rarityWeight',
    'selectionWeight'
}

# Type-level descriptive fields (inherited by items)
TYPE_LEVEL_DESCRIPTIVE = {
    'category',
    'size',
    'behavior',
    'damageType',
    'habitat',
    'description',
    'weaponType',
    'armorType',
    'consumableType'
}

# Gameplay fields (move to traits array)
GAMEPLAY_FIELDS = {
    # Stats
    'level', 'xp', 'health', 'attack', 'defense', 'speed',
    # Attributes
    'strength', 'dexterity', 'constitution', 'intelligence', 'wisdom', 'charisma',
    # Combat
    'abilities', 'abilityUnlocks', 'resistances', 'vulnerabilities', 'immunities',
    # Special
    'packLeader', 'legendary', 'questBoss',
    'damageTypeOverride', 'sizeOverride',
    # Item-specific
    'damage', 'defense', 'durability', 'weight', 'value',
    'twoHanded', 'range', 'equipSlot', 'socketCount',
    # Consumable-specific
    'uses', 'duration', 'stackSize', 'effectType'
}


def convert_object_to_trait_array(obj: Dict[str, Any]) -> List[Dict[str, Any]]:
    """Convert object properties to trait array format."""
    traits = []
    for key, value in obj.items():
        if key not in ROOT_LEVEL_FIELDS:
            traits.append({"key": key, "value": value})
    return traits


def migrate_type_traits(type_obj: Dict[str, Any]) -> Dict[str, Any]:
    """Migrate type-level object to v5.0 structure."""
    result = {}
    
    # Convert traits object to array
    if 'traits' in type_obj and isinstance(type_obj['traits'], dict):
        result['traits'] = convert_object_to_trait_array(type_obj['traits'])
    else:
        result['traits'] = []
    
    # Migrate items
    if 'items' in type_obj:
        result['items'] = [migrate_item(item) for item in type_obj['items']]
    else:
        result['items'] = []
    
    return result


def migrate_item(item: Dict[str, Any]) -> Dict[str, Any]:
    """Migrate item from v4.0 to v5.0."""
    result = {}
    traits = []
    
    # Separate root-level from traits
    for key, value in item.items():
        if key in ROOT_LEVEL_FIELDS:
            result[key] = value
        elif key in GAMEPLAY_FIELDS or key not in ROOT_LEVEL_FIELDS:
            traits.append({"key": key, "value": value})
    
    result['traits'] = traits
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
    """Migrate entire catalog from v4.0 to v5.0."""
    result = {}
    
    # Update metadata
    if 'metadata' in data:
        metadata = data['metadata'].copy()
        metadata['version'] = '5.0'
        metadata['lastUpdated'] = datetime.now().strftime('%Y-%m-%d')
        
        # Update type field if needed
        if 'type' in metadata:
            metadata['type'] = metadata['type'].replace('item_catalog', 'enemy_catalog')
        
        result['metadata'] = metadata
    
    # Find and migrate type collections  
    for key, value in data.items():
        if key == 'metadata':
            continue
        
        # Check if this is a type collection (ends with "_types")
        if isinstance(value, dict) and key.endswith('_types'):
            result[key] = {}
            
            for type_name, type_obj in value.items():
                result[key][type_name] = migrate_type_traits(type_obj)
                
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
    if version == '5.0':
        print(f"  ⚠️ Already v5.0, skipping")
        return
    
    # Migrate
    migrated = migrate_catalog(data)
    
    # Write output
    if in_place:
        output_path = input_path
    elif output_path is None:
        output_path = input_path.parent / f"{input_path.stem}_v5{input_path.suffix}"
    
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(migrated, f, indent=2, ensure_ascii=False)
    
    print(f"  ✅ Migrated to: {output_path}")


def migrate_all_enemies(data_path: Path, in_place: bool = False):
    """Migrate all enemy catalog files."""
    enemies_path = data_path / 'RealmEngine.Data' / 'Data' / 'Json' / 'enemies'
    
    if not enemies_path.exists():
        print(f"❌ Enemies directory not found: {enemies_path}")
        return
    
    catalog_files = list(enemies_path.glob('**/catalog.json'))
    print(f"Found {len(catalog_files)} enemy catalogs to migrate\n")
    
    for catalog_file in catalog_files:
        try:
            migrate_file(catalog_file, in_place=in_place)
        except Exception as e:
            print(f"  ❌ Error: {e}")


def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    
    arg = sys.argv[1]
    
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
