using Game.Shared.Models;
using System.Text.Json.Serialization;

namespace Game.Shared.Data.Models;

/// <summary>
/// Helper class for deserializing trait data from JSON.
/// </summary>
public class JsonTraitValue
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    public TraitValue ToTraitValue()
    {
        var traitType = Type.ToLower() switch
        {
            "number" => TraitType.Number,
            "string" => TraitType.String,
            "boolean" => TraitType.Boolean,
            "stringarray" => TraitType.StringArray,
            "numberarray" => TraitType.NumberArray,
            _ => TraitType.String
        };

        return new TraitValue(Value, traitType);
    }
}
