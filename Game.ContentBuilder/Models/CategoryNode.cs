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
    ItemPrefix,
    ItemSuffix,
    FlatItem,      // For flat JSON structure (no rarity levels): metals, woods, leathers, gemstones
    NameList,      // For array-based JSON structure: weapon_names
    EnemyNames,
    NpcNames,
    Quest
}
