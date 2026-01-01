using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RealmForge.Models;

/// <summary>
/// Custom JSON converter for NamePatternBase to handle field name variations and type discrimination
/// - "template" or "pattern" -> PatternTemplate
/// - "rarityWeight" or "weight" -> Weight
/// - "description" or "example" -> Description
/// - Has "socialClass", "requiresTitle" -> NpcNamePattern
/// - Otherwise -> ItemNamePattern
/// </summary>
public class NamePatternConverter : JsonConverter<NamePatternBase>
{
    public override NamePatternBase ReadJson(JsonReader reader, Type objectType, NamePatternBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var jObject = JObject.Load(reader);

            // Determine type based on properties
            bool isNpcPattern = jObject.ContainsKey("socialClass") ||
                                jObject.ContainsKey("requiresTitle") ||
                                jObject.ContainsKey("excludeTitles") ||
                                jObject.ContainsKey("template");

            NamePatternBase pattern = isNpcPattern
                ? new NpcNamePattern()
                : new ItemNamePattern();

            // Handle template/pattern field
            var templateValue = jObject["template"]?.ToString() ?? jObject["pattern"]?.ToString() ?? string.Empty;
            pattern.PatternTemplate = templateValue;

            // Handle rarityWeight/weight field
            var weightValue = jObject["rarityWeight"]?.ToObject<int>() ?? jObject["weight"]?.ToObject<int>() ?? 0;
            pattern.Weight = weightValue;

            // Handle description/example field
            var descValue = jObject["description"]?.ToString() ?? jObject["example"]?.ToString();
            pattern.Description = descValue;

            // Deserialize type-specific properties manually to avoid recursion
            if (pattern is NpcNamePattern npcPattern)
            {
                if (jObject["socialClass"] is JArray socialClassArr)
                    npcPattern.SocialClass = socialClassArr.ToObject<ObservableCollection<string>>();
                npcPattern.ExcludeTitles = jObject["excludeTitles"]?.ToObject<bool>();
                npcPattern.RequiresTitle = jObject["requiresTitle"]?.ToObject<bool>();
            }

            // Store all other properties in AdditionalProperties
            pattern.AdditionalProperties = new Dictionary<string, JToken>();
            foreach (var prop in jObject.Properties())
            {
                if (prop.Name != "template" && prop.Name != "pattern" &&
                    prop.Name != "rarityWeight" && prop.Name != "weight" &&
                    prop.Name != "description" && prop.Name != "example" &&
                    prop.Name != "socialClass" && prop.Name != "excludeTitles" && prop.Name != "requiresTitle")
                {
                    pattern.AdditionalProperties[prop.Name] = prop.Value;
                }
            }

            return pattern;
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Critical error in NamePatternConverter.ReadJson");
            throw;
        }
    }

    public override void WriteJson(JsonWriter writer, NamePatternBase? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var jObject = new JObject();

        // Try to determine which field names to use based on type
        bool isNpcPattern = value is NpcNamePattern;

        // Write template/pattern
        if (isNpcPattern)
            jObject["template"] = value.PatternTemplate;
        else
            jObject["pattern"] = value.PatternTemplate;

        // Write rarityWeight/weight
        if (isNpcPattern)
            jObject["rarityWeight"] = value.Weight;
        else
            jObject["weight"] = value.Weight;

        // Write description/example
        if (!string.IsNullOrEmpty(value.Description))
        {
            if (isNpcPattern)
                jObject["description"] = value.Description;
            else
                jObject["example"] = value.Description;
        }

        // Write type-specific properties
        if (value is NpcNamePattern npcPattern)
        {
            if (npcPattern.SocialClass != null && npcPattern.SocialClass.Count > 0)
                jObject["socialClass"] = JArray.FromObject(npcPattern.SocialClass);
            if (npcPattern.ExcludeTitles.HasValue)
                jObject["excludeTitles"] = npcPattern.ExcludeTitles.Value;
            if (npcPattern.RequiresTitle.HasValue)
                jObject["requiresTitle"] = npcPattern.RequiresTitle.Value;
        }

        // Write additional properties
        if (value.AdditionalProperties != null)
        {
            foreach (var kvp in value.AdditionalProperties)
            {
                jObject[kvp.Key] = kvp.Value;
            }
        }

        jObject.WriteTo(writer);
    }
}
