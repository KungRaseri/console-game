using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.ContentBuilder.Models;

/// <summary>
/// Custom JSON converter that determines the correct NameComponent type based on properties
/// - Has "gender" or "weightMultiplier" -> NpcNameComponent
/// - Has "traits" -> ItemNameComponent
/// - Otherwise -> use base deserialization
/// </summary>
public class NameComponentConverter : JsonConverter<NameComponentBase>
{
    public override NameComponentBase ReadJson(JsonReader reader, Type objectType, NameComponentBase? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var jObject = JObject.Load(reader);

            // Create a new serializer without this converter to avoid infinite recursion
            var settings = new JsonSerializerSettings();
            
            if (jObject.ContainsKey("gender") || jObject.ContainsKey("weightMultiplier") || jObject.ContainsKey("preferredSocialClass"))
            {
                try
                {
                    return jObject.ToObject<NpcNameComponent>(JsonSerializer.Create(settings)) ?? new NpcNameComponent();
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Failed to deserialize NPC component: {Json}", jObject.ToString());
                    throw;
                }
            }
            else if (jObject.ContainsKey("traits"))
            {
                try
                {
                    return jObject.ToObject<ItemNameComponent>(JsonSerializer.Create(settings)) ?? new ItemNameComponent();
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Failed to deserialize Item component: {Json}", jObject.ToString());
                    throw;
                }
            }
            
            // Fallback: return NPC component as default (most flexible)
            try
            {
                return jObject.ToObject<NpcNameComponent>(JsonSerializer.Create(settings)) ?? new NpcNameComponent();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Failed to deserialize component (fallback): {Json}", jObject.ToString());
                throw;
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Fatal(ex, "Critical error in NameComponentConverter.ReadJson");
            throw;
        }
    }

    public override void WriteJson(JsonWriter writer, NameComponentBase? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        // Serialize the actual derived type
        serializer.Serialize(writer, value, value.GetType());
    }
}
