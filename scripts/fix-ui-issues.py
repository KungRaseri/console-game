#!/usr/bin/env python3
"""
Fix ContentBuilder UI issues:
1. Metadata input labels (use floating hints)
2. Component trait types
3. Pattern search visibility
4. Catalog types expansion
5. Materials data organization
"""

import json
import re
from pathlib import Path

def fix_materials_data():
    """Fix materials names.json and reorganize data"""
    print("\n=== Fixing Materials Data ===")
    
    names_file = Path("Game.Data/Data/Json/items/materials/names.json")
    
    with open(names_file, 'r', encoding='utf-8-sig') as f:
        data = json.load(f)
    
    # Fix pattern tokens - should only have 'base'
    if 'patternTokens' in data['metadata']:
        old_tokens = data['metadata']['patternTokens']
        # Materials use component keys, not 'material' or 'quality' tokens
        data['metadata']['patternTokens'] = ['base']
        print(f"  Updated patternTokens: {old_tokens} → {data['metadata']['patternTokens']}")
    
    # Update patterns to use valid tokens
    if 'patterns' in data:
        for pattern in data['patterns']:
            if 'template' in pattern:
                old = pattern['template']
                # Replace invalid tokens with component keys
                pattern['template'] = old.replace('{material}', '{base}').replace('{quality}', '{base}')
                if old != pattern['template']:
                    print(f"  Updated pattern: {old} → {pattern['template']}")
    
    with open(names_file, 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    
    print("  ✓ Materials names.json fixed")

def fix_catalog_expansion():
    """Set catalog types to be expanded by default"""
    print("\n=== Fixing Catalog Expansion ===")
    
    vm_file = Path("Game.ContentBuilder/ViewModels/CatalogEditorViewModel.cs")
    content = vm_file.read_text(encoding='utf-8')
    
    # Find TypeCatalogNode class and add IsExpanded = true
    pattern = r'(public\s+partial\s+class\s+TypeCatalogNode[^{]*\{)'
    if re.search(pattern, content):
        # Add IsExpanded property initialization in constructor or as default
        if 'IsExpanded' not in content:
            # Add property
            insert_pattern = r'(\[ObservableProperty\]\s+private\s+string\s+_name\s*=\s*string\.Empty;)'
            replacement = r'\1\n\n    [ObservableProperty]\n    private bool _isExpanded = true;'
            content = re.sub(insert_pattern, replacement, content, count=1)
            print("  ✓ Added IsExpanded property to TypeCatalogNode")
        else:
            # Update existing to default true
            content = re.sub(
                r'(\[ObservableProperty\]\s+private\s+bool\s+_isExpanded)\s*=\s*false;',
                r'\1 = true;',
                content
            )
            print("  ✓ Updated IsExpanded default to true")
        
        vm_file.write_text(content, encoding='utf-8')
    
    # Update XAML TreeView to bind IsExpanded
    xaml_file = Path("Game.ContentBuilder/Views/CatalogEditorView.xaml")
    xaml_content = xaml_file.read_text(encoding='utf-8')
    
    if 'IsExpanded=' not in xaml_content or 'IsExpanded="False"' in xaml_content:
        xaml_content = re.sub(
            r'(<TreeViewItem[^>]*Header="{Binding Name}"[^>]*)',
            r'\1 IsExpanded="{Binding IsExpanded, Mode=TwoWay}"',
            xaml_content
        )
        xaml_file.write_text(xaml_content, encoding='utf-8')
        print("  ✓ Updated CatalogEditorView TreeView binding")

def fix_component_trait_types():
    """Ensure component trait Type property is properly bound"""
    print("\n=== Fixing Component Trait Types ===")
    
    xaml_file = Path("Game.ContentBuilder/Views/NameListEditorView.xaml")
    content = xaml_file.read_text(encoding='utf-8')
    
    # Find ComboBox for trait type and ensure it has proper binding
    pattern = r'<ComboBox[^>]*SelectedItem="{Binding Type[^"]*}"[^>]*>'
    if re.search(pattern, content):
        # Ensure UpdateSourceTrigger is set
        content = re.sub(
            r'(SelectedItem="{Binding Type)(, Mode=TwoWay)?(")',
            r'\1, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged\3',
            content
        )
        print("  ✓ Updated trait type ComboBox binding")
        xaml_file.write_text(content, encoding='utf-8')

def main():
    print("ContentBuilder UI Fixes")
    print("=" * 50)
    
    try:
        fix_materials_data()
        fix_catalog_expansion()
        fix_component_trait_types()
        
        print("\n" + "=" * 50)
        print("✓ All fixes applied successfully!")
        print("\nRemaining manual fixes needed:")
        print("  - Metadata labels: Convert to floating hints in NameListEditorView.xaml")
        print("  - Pattern search: Check padding/margin around line 956")
        
    except Exception as e:
        print(f"\n✗ Error: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()
