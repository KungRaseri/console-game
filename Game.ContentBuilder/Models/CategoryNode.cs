using System.Collections.ObjectModel;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Represents a node in the TreeView for navigating game data categories
/// </summary>
public class CategoryNode
{
    /// <summary>
    /// Display name of the category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Material Design icon kind for this category
    /// </summary>
    public string Icon { get; set; } = "Folder";

    /// <summary>
    /// Child categories/items
    /// </summary>
    public ObservableCollection<CategoryNode> Children { get; set; } = new();

    /// <summary>
    /// Number of direct file children (not including subdirectories)
    /// </summary>
    public int FileCount { get; set; } = 0;

    /// <summary>
    /// Total number of files including all subdirectories
    /// </summary>
    public int TotalFileCount { get; set; } = 0;

    /// <summary>
    /// Display name with file count
    /// </summary>
    public string DisplayNameWithCount => TotalFileCount > 0 ? $"{Name} ({TotalFileCount})" : Name;

    /// <summary>
    /// Optional data associated with this node (e.g., file path, data object)
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Type of editor to display when this node is selected
    /// </summary>
    public EditorType EditorType { get; set; } = EditorType.None;
}

/// <summary>
/// Types of editors available in the content builder
/// </summary>
public enum EditorType
{
    None,
    ItemPrefix,        // 3-level hierarchy: rarity → item → traits (weapon_prefixes, armor_materials, etc.)
    ItemSuffix,        // 3-level hierarchy: rarity → item → traits (enchantment_suffixes)
    FlatItem,          // 2-level flat: item → traits (metals, woods, leathers, gemstones, dragon_colors)
    NameList,          // Array structure: category → string[] (weapon_names, beast_names, fantasy_names, dialogue_templates)
    HybridArray,       // Hybrid structure: { items: [], components: {}, patterns: [], metadata: {} }
    
    // New v4.0 specialized editors
    NamesEditor,       // names.json editor: metadata + components + patterns + traits
    TypesEditor,       // types.json editor: metadata + *_types catalog
    ComponentEditor,   // Component catalog editor (e.g., materials/names.json)
    MaterialEditor,    // Material catalog editor (materials/types.json)
    TraitEditor        // Trait definition editor
}
