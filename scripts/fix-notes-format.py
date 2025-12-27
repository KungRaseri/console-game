import json
import os
from pathlib import Path

def fix_notes_recursive(obj, path="root"):
    """Recursively fix notes fields: convert objects to arrays"""
    if isinstance(obj, dict):
        # Check if this dict has a 'notes' key with an object value
        if 'notes' in obj and isinstance(obj['notes'], dict):
            # Convert object to array of its values
            note_values = list(obj['notes'].values())
            obj['notes'] = note_values
            return True
        
        # Recurse into nested dicts
        modified = False
        for key, value in obj.items():
            if fix_notes_recursive(value, f"{path}.{key}"):
                modified = True
        return modified
    
    elif isinstance(obj, list):
        # Recurse into list items
        modified = False
        for i, item in enumerate(obj):
            if fix_notes_recursive(item, f"{path}[{i}]"):
                modified = True
        return modified
    
    return False

def process_json_file(filepath):
    """Process a single JSON file"""
    try:
        # Try utf-8-sig first to handle BOM, fallback to utf-8
        try:
            with open(filepath, 'r', encoding='utf-8-sig') as f:
                data = json.load(f)
        except:
            with open(filepath, 'r', encoding='utf-8') as f:
                data = json.load(f)
        
        modified = False
        
        # Step 1: Remove root-level notes if it's a string or object
        if 'notes' in data:
            root_notes = data['notes']
            if isinstance(root_notes, str):
                del data['notes']
                modified = True
            elif isinstance(root_notes, dict):
                del data['notes']
                modified = True
        
        # Step 2: Convert nested notes objects to arrays
        if fix_notes_recursive(data):
            modified = True
        
        # Save if modified
        if modified:
            with open(filepath, 'w', encoding='utf-8', newline='\n') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            return True
        
        return False
    
    except json.JSONDecodeError as e:
        return False
    except Exception as e:
        return False

def main():
    json_dir = Path("Game.Data/Data/Json")
    
    if not json_dir.exists():
        print(f"ERROR: Directory not found: {json_dir}")
        return
    
    files_processed = 0
    files_modified = 0
    
    # Process all JSON files recursively
    for json_file in json_dir.rglob("*.json"):
        files_processed += 1
        if process_json_file(json_file):
            files_modified += 1
    
    print("=" * 50)
    print(f"Files processed: {files_processed}")
    print(f"Files modified: {files_modified}")
    print("=" * 50)

if __name__ == "__main__":
    main()
