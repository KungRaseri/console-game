using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.ContentBuilder.Models;
using Game.ContentBuilder.Services;

namespace Game.ContentBuilder.ViewModels
{
    /// <summary>
    /// Editor for generic catalog files (occupations, personality traits, quirks, backgrounds, etc.)
    /// Handles component-based catalogs with dynamic property editing
    /// </summary>
    public partial class GenericCatalogEditorViewModel : ObservableObject
    {
        private readonly JsonEditorService _jsonEditorService;
        private readonly string _storedFileName;
        private JObject? _rootJson;
        
        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _catalogType = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<CategoryNodeViewModel> _categories = new();

        [ObservableProperty]
        private CategoryNodeViewModel? _selectedCategory;

        [ObservableProperty]
        private ObservableCollection<CatalogItemViewModel> _items = new();

        [ObservableProperty]
        private CatalogItemViewModel? _selectedItem;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editDisplayName = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private int _editRarityWeight = 10;

        [ObservableProperty]
        private ObservableCollection<PropertyViewModel> _dynamicProperties = new();

        [ObservableProperty]
        private bool _hasUnsavedChanges;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        // Confirmation dialog state
        [ObservableProperty]
        private bool _showDeleteConfirmation;

        [ObservableProperty]
        private string _confirmationMessage = string.Empty;

        [ObservableProperty]
        private CatalogItemViewModel? _pendingDeleteItem;

        public GenericCatalogEditorViewModel(JsonEditorService jsonEditorService, string fileName)
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
                var filePath = _jsonEditorService.GetFilePath(_storedFileName);
                var content = File.ReadAllText(filePath);
                _rootJson = JObject.Parse(content);

                // Extract metadata
                var metadata = _rootJson["metadata"];
                CatalogType = metadata?["type"]?.ToString() ?? "unknown";

                // Load categories and items
                LoadCategories();

                HasUnsavedChanges = false;
                Log.Information("Loaded generic catalog: {FilePath}, Type: {CatalogType}", filePath, CatalogType);
            }
            catch (Exception ex)
            {
                var filePath = _jsonEditorService.GetFilePath(_storedFileName);
                Log.Error(ex, "Failed to load generic catalog: {FilePath}", filePath);
                StatusMessage = $"Error loading file: {ex.Message}";
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();

            if (_rootJson?["components"] is not JObject components)
            {
                Log.Warning("No components found in catalog");
                return;
            }

            foreach (var category in components.Properties())
            {
                var itemCount = category.Value is JArray arr ? arr.Count : 0;
                Categories.Add(new CategoryNodeViewModel
                {
                    Name = category.Name,
                    DisplayName = FormatCategoryName(category.Name),
                    ItemCount = itemCount
                });
            }

            // Select first category by default
            if (Categories.Count > 0)
            {
                SelectedCategory = Categories[0];
            }
        }

        private string FormatCategoryName(string name)
        {
            // Convert snake_case or camelCase to Title Case
            var words = System.Text.RegularExpressions.Regex.Split(name, @"(?<!^)(?=[A-Z])|_");
            return string.Join(" ", words.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
        }

        partial void OnSelectedCategoryChanged(CategoryNodeViewModel? value)
        {
            if (value != null)
            {
                LoadItemsForCategory(value.Name);
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private void LoadItemsForCategory(string categoryName)
        {
            Items.Clear();

            if (_rootJson?["components"]?[categoryName] is not JArray items)
            {
                return;
            }

            foreach (var item in items.Cast<JObject>())
            {
                var vm = new CatalogItemViewModel
                {
                    Name = item["name"]?.ToString() ?? "Unnamed",
                    DisplayName = item["displayName"]?.ToString() ?? item["name"]?.ToString() ?? "Unnamed",
                    Description = item["description"]?.ToString() ?? string.Empty,
                    RarityWeight = item["rarityWeight"]?.ToObject<int>() ?? 10,
                    SourceJson = item
                };

                Items.Add(vm);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show all items
                foreach (var item in Items)
                {
                    item.IsVisible = true;
                }
            }
            else
            {
                var search = SearchText.ToLowerInvariant();
                foreach (var item in Items)
                {
                    item.IsVisible = item.Name.ToLowerInvariant().Contains(search) ||
                                    item.DisplayName.ToLowerInvariant().Contains(search) ||
                                    item.Description.ToLowerInvariant().Contains(search);
                }
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            if (SelectedCategory == null)
            {
                StatusMessage = "Please select a category first";
                return;
            }

            EditName = $"New{SelectedCategory.Name}";
            EditDisplayName = $"New {SelectedCategory.DisplayName}";
            EditDescription = string.Empty;
            EditRarityWeight = 10;
            DynamicProperties.Clear();
            SelectedItem = null;
            IsEditMode = true;
            StatusMessage = "Adding new item...";
        }

        [RelayCommand]
        private void EditItem(CatalogItemViewModel? item)
        {
            if (item == null) return;

            SelectedItem = item;
            EditName = item.Name;
            EditDisplayName = item.DisplayName;
            EditDescription = item.Description;
            EditRarityWeight = item.RarityWeight;

            // Load dynamic properties
            LoadDynamicProperties(item.SourceJson);

            IsEditMode = true;
        }

        private void LoadDynamicProperties(JObject json)
        {
            DynamicProperties.Clear();

            // Standard properties we handle separately
            var standardProps = new HashSet<string> { "name", "displayName", "description", "rarityWeight" };

            foreach (var prop in json.Properties())
            {
                if (standardProps.Contains(prop.Name)) continue;

                var propVm = new PropertyViewModel
                {
                    Name = prop.Name,
                    DisplayName = FormatCategoryName(prop.Name)
                };

                // Determine type and value
                switch (prop.Value.Type)
                {
                    case JTokenType.String:
                        propVm.Type = PropertyType.String;
                        propVm.StringValue = prop.Value.ToString();
                        break;
                    case JTokenType.Integer:
                        propVm.Type = PropertyType.Integer;
                        propVm.IntValue = prop.Value.ToObject<int>();
                        break;
                    case JTokenType.Float:
                        propVm.Type = PropertyType.Double;
                        propVm.DoubleValue = prop.Value.ToObject<double>();
                        break;
                    case JTokenType.Boolean:
                        propVm.Type = PropertyType.Boolean;
                        propVm.BoolValue = prop.Value.ToObject<bool>();
                        break;
                    case JTokenType.Object:
                        propVm.Type = PropertyType.Object;
                        propVm.StringValue = prop.Value.ToString(Formatting.Indented);
                        break;
                    default:
                        propVm.Type = PropertyType.String;
                        propVm.StringValue = prop.Value.ToString();
                        break;
                }

                DynamicProperties.Add(propVm);
            }
        }

        [RelayCommand]
        private void SaveItem()
        {
            if (SelectedCategory == null) return;

            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(EditName))
                {
                    StatusMessage = "Name is required";
                    return;
                }

                // Create new item JSON
                var newItem = new JObject
                {
                    ["name"] = EditName,
                    ["displayName"] = EditDisplayName,
                };

                if (!string.IsNullOrWhiteSpace(EditDescription))
                {
                    newItem["description"] = EditDescription;
                }

                newItem["rarityWeight"] = EditRarityWeight;

                // Add dynamic properties
                foreach (var prop in DynamicProperties)
                {
                    switch (prop.Type)
                    {
                        case PropertyType.String:
                            newItem[prop.Name] = prop.StringValue;
                            break;
                        case PropertyType.Integer:
                            newItem[prop.Name] = prop.IntValue;
                            break;
                        case PropertyType.Double:
                            newItem[prop.Name] = prop.DoubleValue;
                            break;
                        case PropertyType.Boolean:
                            newItem[prop.Name] = prop.BoolValue;
                            break;
                        case PropertyType.Object:
                            try
                            {
                                newItem[prop.Name] = JToken.Parse(prop.StringValue ?? "{}");
                            }
                            catch
                            {
                                newItem[prop.Name] = new JObject();
                            }
                            break;
                    }
                }

                // Update or add
                var categoryArray = _rootJson!["components"]![SelectedCategory.Name] as JArray;
                
                if (SelectedItem != null)
                {
                    // Update existing
                    var index = Items.IndexOf(SelectedItem);
                    categoryArray![index] = newItem;
                    
                    SelectedItem.Name = EditName;
                    SelectedItem.DisplayName = EditDisplayName;
                    SelectedItem.Description = EditDescription;
                    SelectedItem.RarityWeight = EditRarityWeight;
                    SelectedItem.SourceJson = newItem;
                }
                else
                {
                    // Add new
                    categoryArray!.Add(newItem);
                    
                    var vm = new CatalogItemViewModel
                    {
                        Name = EditName,
                        DisplayName = EditDisplayName,
                        Description = EditDescription,
                        RarityWeight = EditRarityWeight,
                        SourceJson = newItem,
                        IsVisible = true
                    };
                    Items.Add(vm);
                }

                // Update category count
                SelectedCategory.ItemCount = categoryArray!.Count;

                HasUnsavedChanges = true;
                IsEditMode = false;
                StatusMessage = $"Saved item: {EditName}";

                Log.Information("Saved item: {ItemName} in category: {Category}", EditName, SelectedCategory.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save item");
                StatusMessage = $"Error saving item: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedItem = null;
        }

        [RelayCommand]
        private void DeleteItem(CatalogItemViewModel? item)
        {
            if (item == null || SelectedCategory == null) return;

            PendingDeleteItem = item;
            ConfirmationMessage = $"Are you sure you want to delete '{item.DisplayName}'?";
            ShowDeleteConfirmation = true;
        }

        [RelayCommand]
        private void ConfirmDelete()
        {
            if (PendingDeleteItem == null || SelectedCategory == null)
            {
                ShowDeleteConfirmation = false;
                return;
            }

            try
            {
                var categoryArray = _rootJson!["components"]![SelectedCategory.Name] as JArray;
                var index = Items.IndexOf(PendingDeleteItem);
                categoryArray!.RemoveAt(index);
                Items.RemoveAt(index);

                SelectedCategory.ItemCount = categoryArray.Count;
                HasUnsavedChanges = true;
                StatusMessage = $"Deleted item: {PendingDeleteItem.DisplayName}";

                Log.Information("Deleted item: {ItemName}", PendingDeleteItem.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete item");
                StatusMessage = $"Error deleting item: {ex.Message}";
            }
            finally
            {
                PendingDeleteItem = null;
                ShowDeleteConfirmation = false;
            }
        }

        [RelayCommand]
        private void CancelDelete()
        {
            PendingDeleteItem = null;
            ShowDeleteConfirmation = false;
            StatusMessage = "Delete cancelled";
        }

        [RelayCommand]
        private void AddProperty()
        {
            var propName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter property name:",
                "Add Property",
                "newProperty");

            if (string.IsNullOrWhiteSpace(propName)) return;

            DynamicProperties.Add(new PropertyViewModel
            {
                Name = propName,
                DisplayName = FormatCategoryName(propName),
                Type = PropertyType.String,
                StringValue = string.Empty
            });
        }

        [RelayCommand]
        private void RemoveProperty(PropertyViewModel? prop)
        {
            if (prop != null)
            {
                DynamicProperties.Remove(prop);
            }
        }

        [RelayCommand]
        private async Task SaveFile()
        {
            if (_rootJson == null) return;

            try
            {
                var filePath = _jsonEditorService.GetFilePath(_storedFileName);
                var json = _rootJson.ToString(Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                HasUnsavedChanges = false;
                StatusMessage = $"File saved successfully: {FileName}";
                
                Log.Information("Saved generic catalog: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                var filePath = _jsonEditorService.GetFilePath(_storedFileName);
                Log.Error(ex, "Failed to save file: {FilePath}", filePath);
                StatusMessage = $"Error saving file: {ex.Message}";
            }
        }
    }

    // Supporting classes
    public partial class CategoryNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private int _itemCount;
    }

    public partial class CatalogItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _rarityWeight;

        [ObservableProperty]
        private bool _isVisible = true;

        public JObject SourceJson { get; set; } = new();
    }

    public partial class PropertyViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private PropertyType _type;

        [ObservableProperty]
        private string? _stringValue;

        [ObservableProperty]
        private int _intValue;

        [ObservableProperty]
        private double _doubleValue;

        [ObservableProperty]
        private bool _boolValue;
    }

    public enum PropertyType
    {
        String,
        Integer,
        Double,
        Boolean,
        Object
    }
}
