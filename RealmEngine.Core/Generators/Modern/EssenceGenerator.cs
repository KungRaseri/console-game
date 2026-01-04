using RealmEngine.Data.Services;
using RealmEngine.Shared.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealmEngine.Core.Generators.Modern;

/// <summary>
/// Generator for creating essences (socketable items providing elemental/damage type bonuses)
/// Loads from essences catalog and creates Essence instances
/// </summary>
public class EssenceGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ILogger<EssenceGenerator> _logger;
    private readonly Random _random = new();

    public EssenceGenerator(GameDataCache dataCache, ILogger<EssenceGenerator> logger)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Essence? Generate(string? category = null)
    {
        try
        {
            var essences = LoadAllEssences();
            if (!essences.Any())
            {
                _logger.LogWarning("No essence catalog items found");
                return null;
            }

            if (!string.IsNullOrEmpty(category))
            {
                var filtered = essences.Where(e => e.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
                if (filtered.Any()) essences = filtered;
            }

            var selected = SelectWeighted(essences);
            return new Essence
            {
                Id = Guid.NewGuid().ToString(),
                Name = selected.Name,
                Description = selected.Description,
                SocketType = SocketType.Essence,
                Category = selected.Category,
                Rarity = selected.Rarity,
                Price = selected.Price,
                Traits = new Dictionary<string, TraitValue>(selected.Traits),
                RarityWeight = selected.RarityWeight
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating essence");
            return null;
        }
    }

    private List<Essence> LoadAllEssences()
    {
        var all = new List<Essence>();
        var categories = new[] { "fire", "shadow", "arcane", "nature", "holy" };

        foreach (var category in categories)
        {
            var catalogFile = _dataCache.GetFile($"essences/{category}/catalog.json");
            if (catalogFile?.JsonData == null) continue;

            var items = catalogFile.JsonData["items"] as JArray;
            if (items == null) continue;

            foreach (var item in items)
            {
                all.Add(new Essence
                {
                    Name = item["name"]?.ToString() ?? "Unknown Essence",
                    Category = item["category"]?.ToString() ?? category,
                    Description = item["description"]?.ToString() ?? string.Empty,
                    RarityWeight = item["rarityWeight"]?.Value<int>() ?? 50,
                    Traits = ParseTraits(item["traits"])
                });
            }
        }
        return all;
    }

    private Dictionary<string, TraitValue> ParseTraits(JToken? traitsToken)
    {
        var traits = new Dictionary<string, TraitValue>();
        if (traitsToken == null) return traits;

        foreach (var prop in traitsToken.Children<JProperty>())
        {
            var traitObj = prop.Value as JObject;
            if (traitObj == null) continue;

            var value = traitObj["value"];
            var typeStr = traitObj["type"]?.ToString() ?? "number";
            var traitType = typeStr.ToLower() switch
            {
                "number" => TraitType.Number,
                "string" => TraitType.String,
                "boolean" => TraitType.Boolean,
                "stringarray" => TraitType.StringArray,
                "numberarray" => TraitType.NumberArray,
                _ => TraitType.Number
            };

            if (value != null)
            {
                traits[prop.Name] = new TraitValue { Value = value.ToString(), Type = traitType };
            }
        }
        return traits;
    }

    private Essence SelectWeighted(List<Essence> items)
    {
        var totalWeight = items.Sum(i => i.RarityWeight);
        var roll = _random.Next(1, totalWeight + 1);
        var runningTotal = 0;

        foreach (var item in items)
        {
            runningTotal += item.RarityWeight;
            if (roll <= runningTotal) return item;
        }
        return items.Last();
    }
}
