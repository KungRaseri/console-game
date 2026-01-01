using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing component/data JSON files (colors.json, traits.json, objectives.json, etc.)
/// Supports both simple arrays and complex hierarchical structures
/// </summary>
public partial class ComponentDataEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _jsonData;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _metadataVersion = string.Empty;

    [ObservableProperty]
    private string _metadataType = string.Empty;

    [ObservableProperty]
    private string _metadataDescription = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _metadataNotes = new();

    [ObservableProperty]
    private bool _hasMetadata;

    [ObservableProperty]
    private ObservableCollection<ComponentItem> _items = new();

    [ObservableProperty]
    private ComponentItem? _selectedItem;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _rawJsonPreview = string.Empty;

    public string MetadataNotesText
    {
        get => string.Join(Environment.NewLine, MetadataNotes);
        set
        {
            MetadataNotes.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    MetadataNotes.Add(line.Trim());
                }
            }
            OnPropertyChanged(nameof(MetadataNotesText));
            IsDirty = true;
        }
    }

    partial void OnMetadataVersionChanged(string value)
    {
        if (_jsonData != null) // Only mark dirty after initial load
            IsDirty = true;
    }

    partial void OnMetadataTypeChanged(string value)
    {
        if (_jsonData != null)
            IsDirty = true;
    }

    partial void OnMetadataDescriptionChanged(string value)
    {
        if (_jsonData != null)
            IsDirty = true;
    }

    public ComponentDataEditorViewModel(JsonEditorService jsonEditorService, string fileName)
    {
        _jsonEditorService = jsonEditorService;
        _storedFileName = fileName;
        FileName = Path.GetFileNameWithoutExtension(fileName);
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            _jsonData = _jsonEditorService.LoadJObject(_storedFileName);
            if (_jsonData == null)
            {
                StatusMessage = "Failed to load file";
                Log.Warning("Failed to load component data file: {FileName}", _storedFileName);
                return;
            }

            LoadMetadata();
            LoadItems();
            UpdateRawJsonPreview();

            StatusMessage = $"Loaded {Items.Count} items";
            IsDirty = false;
            Log.Information("Component data loaded: {FileName} - {Count} items", _storedFileName, Items.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Log.Error(ex, "Failed to load component data: {FileName}", _storedFileName);
        }
    }

    private void LoadMetadata()
    {
        if (_jsonData?["metadata"] is JObject metadata)
        {
            HasMetadata = true;
            MetadataVersion = metadata["version"]?.ToString() ?? "";
            MetadataType = metadata["type"]?.ToString() ?? "";
            MetadataDescription = metadata["description"]?.ToString() ?? "";

            MetadataNotes.Clear();
            if (metadata["notes"] is JArray notes)
            {
                foreach (var note in notes)
                {
                    MetadataNotes.Add(note.ToString());
                }
            }
        }
        else
        {
            HasMetadata = false;
        }
    }

    private void LoadItems()
    {
        Items.Clear();

        if (_jsonData == null) return;

        // Try to find the data section (components, settings, or root level properties)
        JToken? dataSection = null;
        
        if (_jsonData["components"] != null)
        {
            dataSection = _jsonData["components"];
        }
        else if (_jsonData["settings"] != null)
        {
            dataSection = _jsonData["settings"];
        }
        else
        {
            // Use root level properties (excluding metadata)
            dataSection = _jsonData;
        }

        if (dataSection is JObject obj)
        {
            foreach (var prop in obj.Properties().Where(p => p.Name != "metadata"))
            {
                var item = new ComponentItem
                {
                    Key = prop.Name,
                    Value = prop.Value.ToString(Newtonsoft.Json.Formatting.Indented),
                    JsonToken = prop.Value
                };
                Items.Add(item);
            }
        }
        else if (dataSection is JArray arr)
        {
            int index = 0;
            foreach (var element in arr)
            {
                var item = new ComponentItem
                {
                    Key = $"Item {index}",
                    Value = element.ToString(Newtonsoft.Json.Formatting.Indented),
                    JsonToken = element
                };
                Items.Add(item);
                index++;
            }
        }
    }

    private void UpdateRawJsonPreview()
    {
        // Rebuild preview from current data to reflect unsaved changes
        var preview = new JObject();
        
        if (HasMetadata)
        {
            var metadata = new JObject
            {
                ["version"] = MetadataVersion,
                ["type"] = MetadataType,
                ["description"] = MetadataDescription
            };
            
            if (MetadataNotes.Any())
            {
                metadata["notes"] = new JArray(MetadataNotes);
            }
            
            preview["metadata"] = metadata;
        }
        
        // Rebuild data section from Items
        var dataSection = new JObject();
        foreach (var item in Items)
        {
            try
            {
                var token = JToken.Parse(item.Value);
                dataSection[item.Key] = token;
            }
            catch
            {
                dataSection[item.Key] = item.Value;
            }
        }
        
        // Add data section to preview
        if (_jsonData?["components"] != null)
        {
            preview["components"] = dataSection;
        }
        else if (_jsonData?["settings"] != null)
        {
            preview["settings"] = dataSection;
        }
        else
        {
            // Merge into root
            foreach (var prop in dataSection.Properties())
            {
                preview[prop.Name] = prop.Value;
            }
        }
        
        RawJsonPreview = preview.ToString(Newtonsoft.Json.Formatting.Indented);
    }

    [RelayCommand]
    private void AddItem()
    {
        // Generate unique key
        int maxNum = 0;
        foreach (var item in Items)
        {
            if (item.Key.StartsWith("newKey"))
            {
                var numPart = item.Key.Substring(6); // After "newKey"
                if (int.TryParse(numPart, out int num))
                {
                    maxNum = Math.Max(maxNum, num);
                }
            }
            else if (item.Key == "newKey")
            {
                maxNum = Math.Max(maxNum, 0);
            }
        }
        
        string newKey = maxNum == 0 ? "newKey" : $"newKey{maxNum + 1}";
        
        var newItem = new ComponentItem
        {
            Key = newKey,
            Value = "{}",
            JsonToken = new JObject()
        };
        Items.Add(newItem);
        SelectedItem = newItem;
        IsDirty = true;
        UpdateRawJsonPreview();
        StatusMessage = "Added new item";
    }

    [RelayCommand]
    private void DeleteItem()
    {
        if (SelectedItem != null)
        {
            var key = SelectedItem.Key;
            Items.Remove(SelectedItem);
            IsDirty = true;
            UpdateRawJsonPreview();
            StatusMessage = $"Deleted item: {key}";
        }
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            if (_jsonData == null)
            {
                StatusMessage = "No data to save";
                return;
            }

            // Rebuild the data section from Items
            var dataSection = new JObject();
            foreach (var item in Items)
            {
                try
                {
                    var token = JToken.Parse(item.Value);
                    dataSection[item.Key] = token;
                }
                catch
                {
                    // If parsing fails, store as string
                    dataSection[item.Key] = item.Value;
                }
            }

            // Reconstruct the full JSON
            var newRoot = new JObject();
            
            if (HasMetadata)
            {
                // Build metadata from current property values
                var metadata = new JObject
                {
                    ["version"] = MetadataVersion,
                    ["type"] = MetadataType,
                    ["description"] = MetadataDescription
                };
                
                if (MetadataNotes.Any())
                {
                    metadata["notes"] = new JArray(MetadataNotes);
                }
                
                newRoot["metadata"] = metadata;
            }

            // Determine where to put the data
            if (_jsonData["components"] != null)
            {
                newRoot["components"] = dataSection;
            }
            else if (_jsonData["settings"] != null)
            {
                newRoot["settings"] = dataSection;
            }
            else
            {
                // Merge data into root
                foreach (var prop in dataSection.Properties())
                {
                    newRoot[prop.Name] = prop.Value;
                }
            }

            _jsonEditorService.SaveJObject(_storedFileName, newRoot);
            _jsonData = newRoot;

            IsDirty = false;
            StatusMessage = "Saved successfully";
            UpdateRawJsonPreview();
            Log.Information("Component data saved: {FileName}", _storedFileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
            Log.Error(ex, "Failed to save component data: {FileName}", _storedFileName);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
        StatusMessage = "Refreshed from disk";
    }
}

/// <summary>
/// Represents a single component item (key-value pair or array element)
/// </summary>
public class ComponentItem
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public JToken? JsonToken { get; set; }
}
