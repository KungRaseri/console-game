using Newtonsoft.Json.Linq;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace Game.Data.Services;

/// <summary>
/// Centralized cache for all game JSON data loaded at startup.
/// Provides fast in-memory access without repeated file I/O.
/// Uses IMemoryCache for automatic memory management and supports hot-reload via FileSystemWatcher.
/// </summary>
public class GameDataCache : IDisposable
{
    private readonly string _dataRootPath;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, List<string>> _pathsByType;
    private readonly Dictionary<string, List<string>> _pathsByDomain;
    private FileSystemWatcher? _fileWatcher;
    private bool _hotReloadEnabled;
    private bool _disposed;
    
    // Performance tracking
    private long _cacheHits;
    private long _cacheMisses;
    private long _totalLoadTime;
    private readonly object _statsLock = new object();

    public GameDataCache(string dataRootPath, IMemoryCache? memoryCache = null)
    {
        _dataRootPath = dataRootPath ?? throw new ArgumentNullException(nameof(dataRootPath));
        _cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
        _pathsByType = new Dictionary<string, List<string>>();
        _pathsByDomain = new Dictionary<string, List<string>>();
        
        Log.Information("GameDataCache initialized with path: {Path}", _dataRootPath);
    }

    /// <summary>
    /// Gets the total number of loaded files
    /// </summary>
    public int TotalFilesLoaded => _pathsByType.Values.Sum(list => list.Count);

    /// <summary>
    /// Enables hot-reload: automatically refreshes cache when files change on disk
    /// </summary>
    public void EnableHotReload()
    {
        if (_hotReloadEnabled)
        {
            Log.Warning("Hot-reload already enabled");
            return;
        }

        _fileWatcher = new FileSystemWatcher(_dataRootPath)
        {
            Filter = "*.json",
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
        };

        _fileWatcher.Changed += OnFileChanged;
        _fileWatcher.Created += OnFileChanged;
        _fileWatcher.Deleted += OnFileDeleted;
        _fileWatcher.Renamed += OnFileRenamed;

        _fileWatcher.EnableRaisingEvents = true;
        _hotReloadEnabled = true;

        Log.Information("🔥 Hot-reload enabled - watching {Path} for changes", _dataRootPath);
    }

    /// <summary>
    /// Disables hot-reload file watching
    /// </summary>
    public void DisableHotReload()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            _fileWatcher = null;
            _hotReloadEnabled = false;
            Log.Information("Hot-reload disabled");
        }
    }

    /// <summary>
    /// Gets statistics about loaded data
    /// </summary>
    public DataCacheStats GetStats()
    {
        lock (_statsLock)
        {
            var stats = new DataCacheStats
            {
                TotalFiles = TotalFilesLoaded,
                CatalogFiles = _pathsByType.ContainsKey(JsonFileType.GenericCatalog.ToString()) 
                    ? _pathsByType[JsonFileType.GenericCatalog.ToString()].Count : 0,
                NamesFiles = _pathsByType.ContainsKey(JsonFileType.NamesFile.ToString()) 
                    ? _pathsByType[JsonFileType.NamesFile.ToString()].Count : 0,
                ConfigFiles = _pathsByType.ContainsKey(JsonFileType.ConfigFile.ToString()) 
                    ? _pathsByType[JsonFileType.ConfigFile.ToString()].Count : 0,
                ComponentFiles = _pathsByType.ContainsKey(JsonFileType.ComponentData.ToString()) 
                    ? _pathsByType[JsonFileType.ComponentData.ToString()].Count : 0,
                Domains = _pathsByDomain.Keys.ToList(),
                CacheHits = _cacheHits,
                CacheMisses = _cacheMisses,
                TotalLoadTimeMs = _totalLoadTime
            };
            return stats;
        }
    }

    /// <summary>
    /// Loads all JSON files from the data directory into cache
    /// </summary>
    public void LoadAllData()
    {
        var startTime = DateTime.Now;
        Log.Information("Starting data load from: {Path}", _dataRootPath);

        if (!Directory.Exists(_dataRootPath))
        {
            Log.Warning("Data directory not found: {Path}", _dataRootPath);
            return;
        }

        // Clear existing indexes (cache entries remain until accessed)
        _pathsByType.Clear();
        _pathsByDomain.Clear();

        var allJsonFiles = Directory.GetFiles(_dataRootPath, "*.json", SearchOption.AllDirectories);
        Log.Information("Found {Count} JSON files to load", allJsonFiles.Length);

        int loaded = 0;
        int failed = 0;

        foreach (var filePath in allJsonFiles)
        {
            try
            {
                var relativePath = Path.GetRelativePath(_dataRootPath, filePath);
                LoadFileIntoCache(relativePath);
                loaded++;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load file: {FilePath}", filePath);
                failed++;
            }
        }

        var elapsed = DateTime.Now - startTime;
        Log.Information("Data load complete. Loaded {Loaded} files, {Failed} failed in {Duration}ms", 
            loaded, failed, elapsed.TotalMilliseconds);
        
        LogStats();
    }

    /// <summary>
    /// Loads a file into cache and updates indexes
    /// </summary>
    private void LoadFileIntoCache(string relativePath)
    {
        // Normalize path to use forward slashes for consistent cache keys
        var normalizedPath = NormalizePath(relativePath);
        var absolutePath = Path.Combine(_dataRootPath, relativePath);
        
        if (!File.Exists(absolutePath))
        {
            Log.Warning("File not found: {Path}", normalizedPath);
            return;
        }

        try
        {
            var jsonText = File.ReadAllText(absolutePath);
            var jsonData = JObject.Parse(jsonText);
            var fileType = DetectFileType(normalizedPath);
            var domain = ExtractDomain(normalizedPath);

            var cachedFile = new CachedJsonFile
            {
                AbsolutePath = absolutePath,
                RelativePath = normalizedPath,
                FileType = fileType,
                Domain = domain,
                JsonData = jsonData,
                LastModified = File.GetLastWriteTime(absolutePath)
            };

            // Store in IMemoryCache with normalized path
            var cacheEntry = _cache.CreateEntry($"json:{normalizedPath}");
            cacheEntry.Value = cachedFile;
            cacheEntry.Priority = CacheItemPriority.NeverRemove; // Keep indefinitely
            cacheEntry.Dispose(); // Commit to cache

            // Update indexes for fast querying
            AddToTypeIndex(fileType.ToString(), normalizedPath);
            AddToDomainIndex(domain, normalizedPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse JSON file: {Path}", relativePath);
            throw;
        }
    }

    /// <summary>
    /// Gets a cached file by its relative path
    /// </summary>
    /// <param name="relativePath">Path relative to data root (e.g., "abilities/offensive/catalog.json")</param>
    /// <returns>Cached file or null if not found</returns>
    public CachedJsonFile? GetFile(string relativePath)
    {
        // Normalize path to match cache key format
        var normalizedPath = NormalizePath(relativePath);
        var startTime = DateTime.Now;
        
        if (_cache.TryGetValue($"json:{normalizedPath}", out CachedJsonFile? cachedFile))
        {
            // Cache hit!
            lock (_statsLock)
            {
                _cacheHits++;
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _totalLoadTime += (long)elapsed;
            }
            Log.Debug("⚡ CACHE HIT: {Path} ({Time:F3}ms)", normalizedPath, (DateTime.Now - startTime).TotalMilliseconds);
            return cachedFile;
        }

        // Cache miss - try to load it
        lock (_statsLock)
        {
            _cacheMisses++;
        }
        
        try
        {
            Log.Warning("💾 CACHE MISS: Loading from disk: {Path}", normalizedPath);
            LoadFileIntoCache(relativePath);
            _cache.TryGetValue($"json:{normalizedPath}", out cachedFile);
            
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            Log.Information("💾 Disk load completed: {Path} ({Time:F1}ms)", normalizedPath, elapsed);
            
            lock (_statsLock)
            {
                _totalLoadTime += (long)elapsed;
            }
            
            return cachedFile;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load file on-demand: {Path}", relativePath);
            return null;
        }
    }

    /// <summary>
    /// Gets all files of a specific type
    /// </summary>
    public IEnumerable<CachedJsonFile> GetFilesByType(JsonFileType fileType)
    {
        var typeKey = fileType.ToString();
        if (!_pathsByType.TryGetValue(typeKey, out var paths))
            return Enumerable.Empty<CachedJsonFile>();

        return paths.Select(GetFile).Where(f => f != null)!;
    }

    /// <summary>
    /// Gets all files in a specific domain
    /// </summary>
    public IEnumerable<CachedJsonFile> GetFilesByDomain(string domain)
    {
        if (!_pathsByDomain.TryGetValue(domain, out var paths))
            return Enumerable.Empty<CachedJsonFile>();

        return paths.Select(GetFile).Where(f => f != null)!;
    }

    /// <summary>
    /// Gets all catalog files
    /// </summary>
    public IEnumerable<CachedJsonFile> GetAllCatalogs() => GetFilesByType(JsonFileType.GenericCatalog);

    /// <summary>
    /// Gets all names files
    /// </summary>
    public IEnumerable<CachedJsonFile> GetAllNames() => GetFilesByType(JsonFileType.NamesFile);

    /// <summary>
    /// Gets all config files
    /// </summary>
    public IEnumerable<CachedJsonFile> GetAllConfigs() => GetFilesByType(JsonFileType.ConfigFile);

    /// <summary>
    /// Gets all component data files
    /// </summary>
    public IEnumerable<CachedJsonFile> GetAllComponentData() => GetFilesByType(JsonFileType.ComponentData);

    /// <summary>
    /// Reloads a specific file from disk
    /// </summary>
    public void ReloadFile(string relativePath)
    {
        var normalizedPath = NormalizePath(relativePath);
        Log.Information("Reloading file: {Path}", normalizedPath);
        
        // Remove from cache
        _cache.Remove($"json:{normalizedPath}");
        
        // Remove from indexes
        RemoveFromIndexes(relativePath);
        
        // Reload
        try
        {
            LoadFileIntoCache(relativePath);
            Log.Information("✅ File reloaded: {Path}", relativePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to reload file: {Path}", relativePath);
        }
    }

    #region File System Watcher Handlers

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Debounce: ignore rapid successive changes
        System.Threading.Thread.Sleep(100);
        
        var relativePath = Path.GetRelativePath(_dataRootPath, e.FullPath);
        Log.Information("🔥 File changed: {Path}", relativePath);
        ReloadFile(relativePath);
    }

    private void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        var relativePath = Path.GetRelativePath(_dataRootPath, e.FullPath);
        Log.Information("🔥 File deleted: {Path}", relativePath);
        
        _cache.Remove($"json:{relativePath}");
        RemoveFromIndexes(relativePath);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        var oldRelativePath = Path.GetRelativePath(_dataRootPath, e.OldFullPath);
        var newRelativePath = Path.GetRelativePath(_dataRootPath, e.FullPath);
        
        Log.Information("🔥 File renamed: {OldPath} → {NewPath}", oldRelativePath, newRelativePath);
        
        // Remove old
        _cache.Remove($"json:{oldRelativePath}");
        RemoveFromIndexes(oldRelativePath);
        
        // Add new
        LoadFileIntoCache(newRelativePath);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Normalizes a path to use forward slashes for consistent cache keys
    /// </summary>
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private void AddToTypeIndex(string typeKey, string relativePath)
    {
        if (!_pathsByType.ContainsKey(typeKey))
            _pathsByType[typeKey] = new List<string>();
        
        if (!_pathsByType[typeKey].Contains(relativePath))
            _pathsByType[typeKey].Add(relativePath);
    }

    private void AddToDomainIndex(string domain, string relativePath)
    {
        if (string.IsNullOrEmpty(domain))
            return;

        if (!_pathsByDomain.ContainsKey(domain))
            _pathsByDomain[domain] = new List<string>();
        
        if (!_pathsByDomain[domain].Contains(relativePath))
            _pathsByDomain[domain].Add(relativePath);
    }

    private void RemoveFromIndexes(string relativePath)
    {
        foreach (var list in _pathsByType.Values)
            list.Remove(relativePath);

        foreach (var list in _pathsByDomain.Values)
            list.Remove(relativePath);
    }

    private JsonFileType DetectFileType(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath).ToLowerInvariant();

        if (fileName == ".cbconfig.json")
            return JsonFileType.ConfigFile;
        
        if (fileName == "catalog.json")
            return JsonFileType.GenericCatalog;
        
        if (fileName == "names.json")
            return JsonFileType.NamesFile;
        
        if (fileName.EndsWith(".json") && 
            (fileName.Contains("colors") || fileName.Contains("traits") || 
             fileName.Contains("objectives") || fileName.Contains("materials") ||
             fileName.Contains("rarity_config") || fileName.Contains("sizes") ||
             fileName.Contains("types")))
        {
            return JsonFileType.ComponentData;
        }

        return JsonFileType.Unknown;
    }

    private string ExtractDomain(string relativePath)
    {
        var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    private void LogStats()
    {
        var stats = GetStats();
        Log.Information("Cache Statistics:");
        Log.Information("  Total Files: {Total}", stats.TotalFiles);
        Log.Information("  Catalogs: {Catalogs}", stats.CatalogFiles);
        Log.Information("  Names: {Names}", stats.NamesFiles);
        Log.Information("  Configs: {Configs}", stats.ConfigFiles);
        Log.Information("  Component Data: {Components}", stats.ComponentFiles);
        Log.Information("  Domains: {Domains}", string.Join(", ", stats.Domains));
    }
    
    /// <summary>
    /// Logs performance statistics (cache efficiency)
    /// </summary>
    public void LogPerformanceStats()
    {
        var stats = GetStats();
        Log.Information("=== Cache Performance Statistics ===");
        Log.Information("  Total Requests: {Total}", stats.TotalRequests);
        Log.Information("  Cache Hits: {Hits} ⚡", stats.CacheHits);
        Log.Information("  Cache Misses: {Misses} 💾", stats.CacheMisses);
        Log.Information("  Hit Rate: {Rate:F2}%", stats.CacheHitRate);
        Log.Information("  Avg Load Time: {AvgTime}ms", stats.AverageLoadTimeMs);
        Log.Information("  Total Load Time: {TotalTime}ms", stats.TotalLoadTimeMs);
        Log.Information("====================================");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            DisableHotReload();
            
            if (_cache is IDisposable disposableCache)
                disposableCache.Dispose();
        }

        _disposed = true;
    }

    #endregion
}

/// <summary>
/// Cached JSON file with metadata
/// </summary>
public class CachedJsonFile
{
    public required string AbsolutePath { get; init; }
    public required string RelativePath { get; init; }
    public required JsonFileType FileType { get; init; }
    public required string Domain { get; init; }
    public required JObject JsonData { get; init; }
    public required DateTime LastModified { get; init; }
}

/// <summary>
/// Statistics about cached data
/// </summary>
public class DataCacheStats
{
    public int TotalFiles { get; set; }
    public int CatalogFiles { get; set; }
    public int NamesFiles { get; set; }
    public int ConfigFiles { get; set; }
    public int ComponentFiles { get; set; }
    public List<string> Domains { get; set; } = new();
    
    // Performance metrics
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public long TotalRequests => CacheHits + CacheMisses;
    public double CacheHitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests * 100 : 0;
    public long AverageLoadTimeMs => TotalRequests > 0 ? TotalLoadTimeMs / TotalRequests : 0;
    public long TotalLoadTimeMs { get; set; }
}

/// <summary>
/// Types of JSON files in the game data
/// </summary>
public enum JsonFileType
{
    Unknown,
    NamesFile,
    GenericCatalog,
    ComponentData,
    ConfigFile
}
