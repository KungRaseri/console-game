using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using RealmEngine.Data.Services;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for loading and saving JSON data files with automatic backup.
/// Uses GameDataCache for fast in-memory reads when available.
/// </summary>
public class JsonEditorService
{
    private readonly string _dataDirectory;
    private readonly string _backupDirectory;
    private readonly GameDataCache? _dataCache;

    public JsonEditorService(string dataDirectory, GameDataCache? dataCache = null)
    {
        _dataDirectory = dataDirectory;
        _backupDirectory = Path.Combine(dataDirectory, "backups");
        _dataCache = dataCache;

        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }

        if (_dataCache != null)
        {
            Log.Information("JsonEditorService initialized with cache - fast in-memory reads enabled");
        }
        else
        {
            Log.Warning("JsonEditorService initialized without cache - using file I/O for reads");
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
    /// Loads JSON data as a JObject for dynamic editing.
    /// Uses in-memory cache when available for instant loading.
    /// </summary>
    /// <param name="fileName">Name of the JSON file (e.g., "colors.json")</param>
    /// <returns>JObject or null if file doesn't exist</returns>
    public JObject? LoadJObject(string fileName)
    {
        try
        {
            var startTime = DateTime.Now;
            
            // Try cache first for fast in-memory access
            if (_dataCache != null)
            {
                var cachedFile = _dataCache.GetFile(fileName);
                if (cachedFile != null)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Debug("⚡ LoadJObject CACHE HIT: {FileName} ({Time:F3}ms)", fileName, elapsed);
                    // Return a deep clone to prevent accidental mutations of cached data
                    return (JObject)cachedFile.JsonData.DeepClone();
                }
                Log.Warning("💾 LoadJObject CACHE MISS: {FileName} - falling back to file I/O", fileName);
            }

            // Fallback to file I/O if cache unavailable or file not cached
            var filePath = Path.Combine(_dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                Log.Warning("JSON file not found: {FilePath}", filePath);
                return null;
            }

            var json = File.ReadAllText(filePath);
            var data = JObject.Parse(json);
            
            var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
            Log.Information("💾 LoadJObject from disk: {FileName} ({Time:F1}ms)", fileName, totalElapsed);
            return data;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load JSON file as JObject: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to load {fileName}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Loads JSON data as raw text string.
    /// Uses in-memory cache when available for instant loading.
    /// </summary>
    /// <param name="fileName">Name of the JSON file (e.g., "catalog.json")</param>
    /// <returns>Raw JSON string or null if file doesn't exist</returns>
    public string? LoadJsonText(string fileName)
    {
        try
        {
            var startTime = DateTime.Now;
            
            // Try cache first for fast in-memory access
            if (_dataCache != null)
            {
                var cachedFile = _dataCache.GetFile(fileName);
                if (cachedFile != null)
                {
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Log.Debug("⚡ LoadJsonText CACHE HIT: {FileName} ({Time:F3}ms)", fileName, elapsed);
                    return cachedFile.JsonData.ToString(Formatting.Indented);
                }
                Log.Warning("💾 LoadJsonText CACHE MISS: {FileName} - falling back to file I/O", fileName);
            }

            // Fallback to file I/O if cache unavailable or file not cached
            var filePath = Path.Combine(_dataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                Log.Warning("JSON file not found: {FilePath}", filePath);
                return null;
            }

            var json = File.ReadAllText(filePath);
            var totalElapsed = (DateTime.Now - startTime).TotalMilliseconds;
            Log.Information("💾 LoadJsonText from disk: {FileName} ({Time:F1}ms)", fileName, totalElapsed);
            return json;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load JSON text: {FileName}", fileName);
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
    /// Saves a JObject to a JSON file with automatic backup
    /// </summary>
    /// <param name="fileName">Name of the JSON file (e.g., "colors.json")</param>
    /// <param name="data">JObject to save</param>
    public void SaveJObject(string fileName, JObject data)
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
            var json = data.ToString(Formatting.Indented);
            File.WriteAllText(filePath, json);

            Log.Information("Saved JObject to file: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save JObject to file: {FileName}", fileName);
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
