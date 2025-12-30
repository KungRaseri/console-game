using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Game.ContentBuilder.ViewModels;

/// <summary>
/// ViewModel for editing .cbconfig.json files (folder configuration)
/// </summary>
public partial class ConfigEditorViewModel : ObservableObject
{
    private readonly JsonEditorService _jsonEditorService;
    private readonly string _storedFileName;
    private JObject? _jsonData;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private int _sortOrder;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _defaultFileIcon = string.Empty;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _rawJsonPreview = string.Empty;

    public ConfigEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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
                Log.Warning("Failed to load config file: {FileName}", _storedFileName);
                return;
            }

            // Load all properties
            DisplayName = _jsonData["displayName"]?.ToString() ?? "";
            Icon = _jsonData["icon"]?.ToString() ?? "";
            SortOrder = _jsonData["sortOrder"]?.Value<int>() ?? 0;
            Description = _jsonData["description"]?.ToString() ?? "";
            DefaultFileIcon = _jsonData["defaultFileIcon"]?.ToString() ?? "";

            UpdateRawJsonPreview();

            StatusMessage = "Config loaded";
            IsDirty = false;
            Log.Information("Config loaded: {FileName}", _storedFileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Log.Error(ex, "Failed to load config: {FileName}", _storedFileName);
        }
    }

    private void UpdateRawJsonPreview()
    {
        if (_jsonData != null)
        {
            RawJsonPreview = _jsonData.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }

    partial void OnDisplayNameChanged(string value)
    {
        IsDirty = true;
    }

    partial void OnIconChanged(string value)
    {
        IsDirty = true;
    }

    partial void OnSortOrderChanged(int value)
    {
        IsDirty = true;
    }

    partial void OnDescriptionChanged(string value)
    {
        IsDirty = true;
    }

    partial void OnDefaultFileIconChanged(string value)
    {
        IsDirty = true;
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            var config = new JObject
            {
                ["displayName"] = DisplayName,
                ["icon"] = Icon,
                ["sortOrder"] = SortOrder
            };

            if (!string.IsNullOrWhiteSpace(Description))
            {
                config["description"] = Description;
            }

            if (!string.IsNullOrWhiteSpace(DefaultFileIcon))
            {
                config["defaultFileIcon"] = DefaultFileIcon;
            }

            _jsonEditorService.SaveJObject(_storedFileName, config);
            _jsonData = config;

            IsDirty = false;
            StatusMessage = "Saved successfully";
            UpdateRawJsonPreview();
            Log.Information("Config saved: {FileName}", _storedFileName);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
            Log.Error(ex, "Failed to save config: {FileName}", _storedFileName);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
        StatusMessage = "Refreshed from disk";
    }
}
