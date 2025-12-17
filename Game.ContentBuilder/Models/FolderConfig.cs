namespace Game.ContentBuilder.Models;

/// <summary>
/// Configuration for folder/file icons and metadata
/// Loaded from .cbconfig.json files in each directory
/// </summary>
public class FolderConfig
{
    /// <summary>
    /// Material Design icon for this folder
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Display name override for this folder
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Description of this folder's purpose
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon mappings for files in this directory
    /// Key = filename (without extension), Value = Material Design icon name
    /// </summary>
    public Dictionary<string, string> FileIcons { get; set; } = new();

    /// <summary>
    /// Default icon for files in this directory (if not specified in FileIcons)
    /// </summary>
    public string? DefaultFileIcon { get; set; }

    /// <summary>
    /// Whether to show file count in folder display
    /// </summary>
    public bool ShowFileCount { get; set; } = true;

    /// <summary>
    /// Sort order priority (lower numbers appear first)
    /// </summary>
    public int SortOrder { get; set; } = 100;
}
