using System.IO;
using Newtonsoft.Json;
using Serilog;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for loading and saving JSON data files with automatic backup
/// </summary>
public class JsonEditorService
{
    private readonly string _dataDirectory;
    private readonly string _backupDirectory;

    public JsonEditorService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        _backupDirectory = Path.Combine(dataDirectory, "backups");
        
        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    /// <summary>
    /// Loads JSON data from a file
    /// </summary>
    /// <typeparam name="T">Type to deserialize to</typeparam>
    /// <param name="fileName">Name of the JSON file (e.g., "weapon_prefixes.json")</param>
    /// <returns>Deserialized data or default if file doesn't exist</returns>
    public T? Load<T>(string fileName) where T : class
    {
        try
        {
            var filePath = Path.Combine(_dataDirectory, fileName);
            
            if (!File.Exists(filePath))
            {
                Log.Warning("JSON file not found: {FilePath}", filePath);
                return default;
            }

            var json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<T>(json);
            
            Log.Information("Loaded JSON file: {FileName}", fileName);
            return data;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load JSON file: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to load {fileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves data to a JSON file with automatic backup of the existing file
    /// </summary>
    /// <typeparam name="T">Type to serialize</typeparam>
    /// <param name="fileName">Name of the JSON file (e.g., "weapon_prefixes.json")</param>
    /// <param name="data">Data to save</param>
    public void Save<T>(string fileName, T data) where T : class
    {
        try
        {
            var filePath = Path.Combine(_dataDirectory, fileName);
            
            // Create backup if file exists
            if (File.Exists(filePath))
            {
                CreateBackup(fileName);
            }

            // Serialize with formatting for readability
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
            
            Log.Information("Saved JSON file: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save JSON file: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to save {fileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a timestamped backup of the specified file
    /// </summary>
    /// <param name="fileName">Name of the file to backup</param>
    public void CreateBackup(string fileName)
    {
        try
        {
            var sourcePath = Path.Combine(_dataDirectory, fileName);
            
            if (!File.Exists(sourcePath))
            {
                Log.Warning("Cannot create backup - source file not found: {FilePath}", sourcePath);
                return;
            }

            // Create backup filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var backupFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            File.Copy(sourcePath, backupPath, overwrite: false);
            
            Log.Information("Created backup: {BackupFileName}", backupFileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create backup for: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to create backup for {fileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the full path to a data file
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Full path to the file</returns>
    public string GetFilePath(string fileName)
    {
        return Path.Combine(_dataDirectory, fileName);
    }

    /// <summary>
    /// Checks if a data file exists
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>True if file exists, false otherwise</returns>
    public bool FileExists(string fileName)
    {
        var filePath = Path.Combine(_dataDirectory, fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Gets all backup files for a specific data file
    /// </summary>
    /// <param name="fileName">Name of the original file</param>
    /// <returns>Array of backup file paths, sorted by date descending</returns>
    public string[] GetBackups(string fileName)
    {
        try
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var pattern = $"{fileNameWithoutExt}_*{extension}";
            
            var backups = Directory.GetFiles(_backupDirectory, pattern);
            Array.Sort(backups);
            Array.Reverse(backups); // Most recent first
            
            return backups;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get backups for: {FileName}", fileName);
            return Array.Empty<string>();
        }
    }
}
