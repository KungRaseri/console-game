#!/usr/bin/env python3
"""
Fix JSON v5.1 compliance issues:
1. Rename 'selectionWeight' to 'rarityWeight' in items
2. Convert string attribute values like "high" to appropriate numeric values
"""

import json
import os
import sys
from pathlib import Path

# Attribute string mappings (from previous migration scripts)
ATTRIBUTE_MAPPINGS = {
    "very low": 6,
    "low": 8,
    "moderate": 10,
    "normal": 10,
    "average": 10,
    "high": 14,
    "very high": 16,
    "exceptional": 18,
    "legendary": 20
}

# Rarity string to numeric mappings
RARITY_MAPPINGS = {
    "common": 75,
    "uncommon": 50,
    "rare": 25,
    "epic": 10,
    "legendary": 3,
    "mythic": 1
}

def fix_attributes(obj):
    """Convert string attribute values to numeric"""
    if not isinstance(obj, dict):
        return obj
        
    if "attributes" in obj and isinstance(obj["attributes"], dict):
        attrs = obj["attributes"]
        for attr in ["strength", "dexterity", "constitution", "intelligence", "wisdom", "charisma"]:
            if attr in attrs and isinstance(attrs[attr], str):
                value_str = attrs[attr].lower()
                if value_str in ATTRIBUTE_MAPPINGS:
                    print(f"    Converting {attr}: '{attrs[attr]}' -> {ATTRIBUTE_MAPPINGS[value_str]}")
                    attrs[attr] = ATTRIBUTE_MAPPINGS[value_str]
                else:
                    print(f"    WARNING: Unknown attribute value '{attrs[attr]}' for {attr}")
    
    return obj

def rename_selection_weight(obj):
    """Rename selectionWeight to rarityWeight and convert string rarity to numeric"""
    if not isinstance(obj, dict):
        return obj
    
    # Rename selectionWeight to rarityWeight
    if "selectionWeight" in obj:
        obj["rarityWeight"] = obj.pop("selectionWeight")
    
    # Convert string rarity values to numeric
    if "rarity" in obj and isinstance(obj["rarity"], str):
        rarity_str = obj["rarity"].lower()
        if rarity_str in RARITY_MAPPINGS:
            old_value = obj["rarity"]
            obj["rarity"] = RARITY_MAPPINGS[rarity_str]
            print(f"    Converting rarity: '{old_value}' -> {obj['rarity']}")
        else:
            print(f"    WARNING: Unknown rarity value '{obj['rarity']}'")
    
    # If missing rarity but has rarityWeight, calculate from it
    if "rarity" not in obj and "rarityWeight" in obj:
        obj["rarity"] = obj["rarityWeight"]
        print(f"    Added rarity: {obj['rarity']} (from rarityWeight)")
    
    return obj

def process_items(items):
    """Process items list recursively"""
    if not isinstance(items, list):
        return items
    
    for item in items:
        fix_attributes(item)
        rename_selection_weight(item)
    
    return items

def process_catalog(catalog_path):
    """Process a single catalog file"""
    print(f"\nProcessing: {catalog_path}")
    
    with open(catalog_path, 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    changes_made = False
    
    # Process all type categories
    for key in data.keys():
        if key == "metadata":
            continue
        
        if isinstance(data[key], dict):
            for type_key in data[key].keys():
                type_data = data[key][type_key]
                if isinstance(type_data, dict) and "items" in type_data:
                    print(f"  Processing {type_key}...")
                    process_items(type_data["items"])
                    changes_made = True
    
    if changes_made:
        # Write back with formatting
        with open(catalog_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        print(f"  * Fixed: {catalog_path}")
    else:
        print(f"  - No changes needed")
    
    return changes_made

def main():
    """Main entry point"""
    # Get RealmEngine.Data path
    script_dir = Path(__file__).parent
    solution_root = script_dir.parent
    data_path = solution_root / "RealmEngine.Data" / "Data" / "Json"
    
    if not data_path.exists():
        print(f"ERROR: Data path not found: {data_path}")
        return 1
    
    print("="*60)
    print("JSON v5.1 Compliance Fixer")
    print("="*60)
    print(f"Data path: {data_path}")
    
    # Find all catalog.json files
    catalog_files = list(data_path.glob("**/catalog.json"))
    
    if not catalog_files:
        print("No catalog files found!")
        return 1
    
    print(f"Found {len(catalog_files)} catalog files")
    
    total_fixed = 0
    for catalog_file in sorted(catalog_files):
        relative_path = catalog_file.relative_to(data_path)
        if process_catalog(catalog_file):
            total_fixed += 1
    
    print("\n" + "="*60)
    print(f"Completed: Fixed {total_fixed} / {len(catalog_files)} catalogs")
    print("="*60)
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
