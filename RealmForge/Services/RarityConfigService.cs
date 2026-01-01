using System.IO;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using Serilog;

namespace RealmForge.Services;

/// <summary>
/// Service that loads and provides access to rarity configuration from rarity_config.json
/// </summary>
public class RarityConfigService
{
  private static RarityConfigService? _instance;
  private static readonly object _lock = new object();

  private List<RarityThreshold> _thresholds = new();

  public static RarityConfigService Instance
  {
    get
    {
      if (_instance == null)
      {
        lock (_lock)
        {
          if (_instance == null)
          {
            _instance = new RarityConfigService();
          }
        }
      }
      return _instance;
    }
  }

  private RarityConfigService()
  {
    LoadConfiguration();
  }

  private void LoadConfiguration()
  {
    try
    {
      // Find rarity_config.json in the data directory
      var baseDir = AppDomain.CurrentDomain.BaseDirectory;
      var configPath = Path.Combine(baseDir, "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json", "general", "rarity_config.json");
      configPath = Path.GetFullPath(configPath);

      if (!File.Exists(configPath))
      {
        Log.Warning("Rarity config not found at {Path}, using defaults", configPath);
        LoadDefaults();
        return;
      }

      var json = File.ReadAllText(configPath);
      var config = JObject.Parse(json);
      var thresholds = config["thresholds"] as JObject;

      if (thresholds == null)
      {
        Log.Warning("No thresholds found in rarity config, using defaults");
        LoadDefaults();
        return;
      }

      _thresholds.Clear();

      foreach (var property in thresholds.Properties())
      {
        var threshold = property.Value as JObject;
        if (threshold == null) continue;

        var rarityThreshold = new RarityThreshold
        {
          Key = property.Name,
          Min = threshold["min"]?.ToObject<int>() ?? 0,
          Max = threshold["max"]?.ToObject<int>() ?? 999999,
          DisplayName = threshold["displayName"]?.ToString() ?? property.Name,
          HexColor = threshold["hexColor"]?.ToString() ?? "#808080"
        };

        _thresholds.Add(rarityThreshold);
      }

      // Sort by min value for easier lookup
      _thresholds = _thresholds.OrderBy(t => t.Min).ToList();

      Log.Information("Loaded {Count} rarity thresholds from config", _thresholds.Count);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Failed to load rarity config, using defaults");
      LoadDefaults();
    }
  }

  private void LoadDefaults()
  {
    _thresholds = new List<RarityThreshold>
        {
            new RarityThreshold { Key = "common", Min = 0, Max = 20, DisplayName = "Common", HexColor = "#808080" },
            new RarityThreshold { Key = "uncommon", Min = 21, Max = 50, DisplayName = "Uncommon", HexColor = "#81C784" },
            new RarityThreshold { Key = "rare", Min = 51, Max = 100, DisplayName = "Rare", HexColor = "#64B5F6" },
            new RarityThreshold { Key = "epic", Min = 101, Max = 200, DisplayName = "Epic", HexColor = "#BA68C8" },
            new RarityThreshold { Key = "legendary", Min = 201, Max = 999999, DisplayName = "Legendary", HexColor = "#FFB74D" }
        };
  }

  public string GetRarityName(int weight)
  {
    var threshold = _thresholds.FirstOrDefault(t => weight >= t.Min && weight <= t.Max);
    return threshold?.DisplayName ?? "Unknown";
  }

  public Color GetRarityColor(int weight)
  {
    var threshold = _thresholds.FirstOrDefault(t => weight >= t.Min && weight <= t.Max);
    if (threshold == null || string.IsNullOrEmpty(threshold.HexColor))
      return Color.FromRgb(128, 128, 128); // Default gray

    try
    {
      var hex = threshold.HexColor.TrimStart('#');
      if (hex.Length == 6)
      {
        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Color.FromRgb(r, g, b);
      }
    }
    catch
    {
      // Fall through to default
    }

    return Color.FromRgb(128, 128, 128);
  }

  public IEnumerable<RarityThreshold> GetAllThresholds() => _thresholds;
}

public class RarityThreshold
{
  public string Key { get; set; } = string.Empty;
  public int Min { get; set; }
  public int Max { get; set; }
  public string DisplayName { get; set; } = string.Empty;
  public string HexColor { get; set; } = string.Empty;
}
