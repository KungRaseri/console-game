using Godot;
using RealmEngine.Shared.Services;
using RealmEngine.Data.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Example Godot script showing how to integrate RealmEngine logging with GD.Print().
/// Copy this into your Godot project and modify as needed.
/// </summary>
public partial class RealmEngineManager : Node
{
    private ILoggerFactory? _loggerFactory;
    private GameDataCache? _dataCache;
    private ReferenceResolverService? _referenceResolver;

    public override void _Ready()
    {
        GD.Print("🎮 Initializing RealmEngine with Godot Logging Integration...");

        try
        {
            // Initialize RealmEngine logging to forward to Godot
            _loggerFactory = GodotLogger.Initialize(
                minimumLevel: Serilog.Events.LogEventLevel.Information,
                includeConsole: false // Don't duplicate in console, only send to Godot
            );

            // Subscribe to log events and forward to GD.Print
            GodotLogger.Subscribe(OnRealmEngineLogReceived);

            // Initialize RealmEngine services
            InitializeRealmEngine();

            GD.Print("✅ RealmEngine initialization complete!");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"❌ Failed to initialize RealmEngine: {ex.Message}");
        }
    }

    public override void _ExitTree()
    {
        GD.Print("🔌 Shutting down RealmEngine logging...");
        
        // Clean up logging resources
        GodotLogger.Shutdown();
        _loggerFactory?.Dispose();
    }

    /// <summary>
    /// Callback that receives RealmEngine log messages and forwards them to Godot.
    /// </summary>
    private void OnRealmEngineLogReceived(LogLevel level, string message)
    {
        switch (level)
        {
            case LogLevel.Error:
            case LogLevel.Fatal:
                GD.PrintErr($"[RealmEngine] {message}");
                break;
            
            case LogLevel.Warning:
                GD.Print($"[RealmEngine] ⚠️ {message}");
                break;
            
            case LogLevel.Info:
                GD.Print($"[RealmEngine] ℹ️ {message}");
                break;
            
            case LogLevel.Debug:
                GD.Print($"[RealmEngine] 🐛 {message}");
                break;
            
            case LogLevel.Trace:
                GD.Print($"[RealmEngine] 🔍 {message}");
                break;
            
            default:
                GD.Print($"[RealmEngine] {message}");
                break;
        }
    }

    private void InitializeRealmEngine()
    {
        if (_loggerFactory == null)
            throw new InvalidOperationException("Logger factory not initialized");

        // Initialize RealmEngine services with proper logging
        var dataPath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Data", "Json");
        _dataCache = new GameDataCache(dataPath);
        
        var resolverLogger = _loggerFactory.CreateLogger<ReferenceResolverService>();
        _referenceResolver = new ReferenceResolverService(_dataCache, resolverLogger);

        GD.Print($"📁 Loaded {_dataCache.LoadedFiles.Count} JSON data files");
    }

    /// <summary>
    /// Public method to get the configured logger factory for other Godot scripts.
    /// </summary>
    public ILoggerFactory GetLoggerFactory()
    {
        return _loggerFactory ?? throw new InvalidOperationException("RealmEngine not initialized");
    }

    /// <summary>
    /// Public method to get RealmEngine services for other Godot scripts.
    /// </summary>
    public (GameDataCache DataCache, ReferenceResolverService ReferenceResolver) GetRealmEngineServices()
    {
        if (_dataCache == null || _referenceResolver == null)
            throw new InvalidOperationException("RealmEngine services not initialized");

        return (_dataCache, _referenceResolver);
    }
}

/// <summary>
/// Extension methods to make RealmEngine integration easier from other Godot scripts.
/// </summary>
public static class RealmEngineExtensions
{
    /// <summary>
    /// Gets the RealmEngineManager from the scene tree.
    /// </summary>
    public static RealmEngineManager GetRealmEngine(this Node node)
    {
        var manager = node.GetTree().GetFirstNodeInGroup("RealmEngineManager") as RealmEngineManager;
        if (manager == null)
        {
            throw new InvalidOperationException("RealmEngineManager not found in scene tree. Add it to a group called 'RealmEngineManager'.");
        }
        return manager;
    }

    /// <summary>
    /// Quick access to RealmEngine services from any Node.
    /// </summary>
    public static (GameDataCache DataCache, ReferenceResolverService ReferenceResolver) GetRealmEngineServices(this Node node)
    {
        return node.GetRealmEngine().GetRealmEngineServices();
    }

    /// <summary>
    /// Creates a logger for the calling class from any Node.
    /// </summary>
    public static Microsoft.Extensions.Logging.ILogger<T> CreateLogger<T>(this Node node)
    {
        return node.GetRealmEngine().GetLoggerFactory().CreateLogger<T>();
    }
}