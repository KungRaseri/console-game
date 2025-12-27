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
/// Types of editors available in the content builder (v4.0 Clean Architecture)
/// </summary>
public enum EditorType
{
    None,

    // Active Editors (v4.0)
    NameListEditor,        // names.json: metadata + components + patterns + name generation
    CatalogEditor,         // catalog.json: item types (weapons, armor, etc.) with categories

    // Future Editors (Planned for v4.0)
    QuestEditor,           // quests/catalog.json: quest templates, objectives, rewards
    NpcEditor              // npcs/: occupations, dialogues, shops, traits
}

