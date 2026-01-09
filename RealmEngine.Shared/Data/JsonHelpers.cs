using RealmEngine.Shared.Models;
using System.Text.Json.Serialization;

namespace RealmEngine.Shared.Data.Models;

/// <summary>
/// Helper class for deserializing trait data from JSON.
/// </summary>
public class JsonTraitValue
{
    /// <summary>Gets or sets the value.</summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>Gets or sets the type.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "string";

    /// <summary>
    /// Converts to TraitValue.
    /// </summary>
    /// <returns>The converted TraitValue.</returns>
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
