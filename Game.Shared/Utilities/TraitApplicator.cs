using Game.Shared.Models;

namespace Game.Shared.Utilities;

/// <summary>
/// Utility class for applying and querying traits on entities.
/// </summary>
public static class TraitApplicator
{
    /// <summary>
    /// Apply a dictionary of traits to an entity.
    /// </summary>
    public static void ApplyTraits(ITraitable entity, Dictionary<string, TraitValue> traits)
    {
        foreach (var trait in traits)
        {
            entity.Traits[trait.Key] = trait.Value;
        }
    }

    /// <summary>
    /// Apply a single trait to an entity.
    /// </summary>
    public static void ApplyTrait(ITraitable entity, string traitName, object value, TraitType type)
    {
        entity.Traits[traitName] = new TraitValue(value, type);
    }

    /// <summary>
    /// Get a trait value with a default fallback.
    /// </summary>
    public static T GetTrait<T>(ITraitable entity, string traitName, T defaultValue)
    {
        if (!entity.Traits.ContainsKey(traitName))
            return defaultValue;

        var trait = entity.Traits[traitName];

        if (typeof(T) == typeof(int))
            return (T)(object)trait.AsInt();
        if (typeof(T) == typeof(double))
            return (T)(object)trait.AsDouble();
        if (typeof(T) == typeof(string))
            return (T)(object)trait.AsString();
        if (typeof(T) == typeof(bool))
            return (T)(object)trait.AsBool();
        if (typeof(T) == typeof(List<string>))
            return (T)(object)trait.AsStringList();
        if (typeof(T) == typeof(List<int>))
            return (T)(object)trait.AsIntList();

        return defaultValue;
    }

    /// <summary>
    /// Check if an entity has a specific trait.
    /// </summary>
    public static bool HasTrait(ITraitable entity, string traitName)
    {
        return entity.Traits.ContainsKey(traitName);
    }

    /// <summary>
    /// Remove a trait from an entity.
    /// </summary>
    public static void RemoveTrait(ITraitable entity, string traitName)
    {
        entity.Traits.Remove(traitName);
    }

    /// <summary>
    /// Get all trait names for an entity.
    /// </summary>
    public static List<string> GetTraitNames(ITraitable entity)
    {
        return entity.Traits.Keys.ToList();
    }

    /// <summary>
    /// Merge traits from source to target (target traits take precedence).
    /// </summary>
    public static void MergeTraits(ITraitable target, Dictionary<string, TraitValue> sourceTraits)
    {
        foreach (var trait in sourceTraits)
        {
            if (!target.Traits.ContainsKey(trait.Key))
            {
                target.Traits[trait.Key] = trait.Value;
            }
        }
    }

    /// <summary>
    /// Add a numeric bonus to an existing trait (or create it if it doesn't exist).
    /// </summary>
    public static void AddNumericBonus(ITraitable entity, string traitName, double bonus)
    {
        if (entity.Traits.ContainsKey(traitName))
        {
            var currentValue = entity.Traits[traitName].AsDouble();
            entity.Traits[traitName] = new TraitValue(currentValue + bonus, TraitType.Number);
        }
        else
        {
            entity.Traits[traitName] = new TraitValue(bonus, TraitType.Number);
        }
    }

    /// <summary>
    /// Calculate total bonus for a stat from all applicable traits.
    /// Example: Get total strength from strengthBonus, might, titan, etc.
    /// </summary>
    public static int GetTotalStatBonus(ITraitable entity, params string[] traitNames)
    {
        int total = 0;
        foreach (var traitName in traitNames)
        {
            total += GetTrait(entity, traitName, 0);
        }
        return total;
    }

    /// <summary>
    /// Get resistance percentage for a damage type.
    /// </summary>
    public static int GetResistance(ITraitable entity, string resistanceType)
    {
        return GetTrait(entity, resistanceType, 0);
    }

    /// <summary>
    /// Check if entity has any resistance.
    /// </summary>
    public static bool HasResistance(ITraitable entity, string resistanceType)
    {
        return GetResistance(entity, resistanceType) > 0;
    }

    /// <summary>
    /// Get all resistances for an entity.
    /// </summary>
    public static Dictionary<string, int> GetAllResistances(ITraitable entity)
    {
        var resistances = new Dictionary<string, int>();

        var resistanceTraits = new[]
        {
            StandardTraits.ResistFire,
            StandardTraits.ResistIce,
            StandardTraits.ResistLightning,
            StandardTraits.ResistPoison,
            StandardTraits.ResistPhysical,
            StandardTraits.ResistMagic
        };

        foreach (var trait in resistanceTraits)
        {
            var value = GetResistance(entity, trait);
            if (value > 0)
            {
                resistances[trait] = value;
            }
        }

        return resistances;
    }

    /// <summary>
    /// Pretty print all traits for debugging.
    /// </summary>
    public static string DebugTraits(ITraitable entity)
    {
        if (!entity.Traits.Any())
            return "No traits";

        var lines = new List<string>();
        foreach (var trait in entity.Traits)
        {
            var valueStr = trait.Value.Type switch
            {
                TraitType.Number => trait.Value.AsDouble().ToString("F2"),
                TraitType.String => $"\"{trait.Value.AsString()}\"",
                TraitType.Boolean => trait.Value.AsBool().ToString(),
                TraitType.StringArray => $"[{string.Join(", ", trait.Value.AsStringList())}]",
                TraitType.NumberArray => $"[{string.Join(", ", trait.Value.AsIntList())}]",
                _ => trait.Value.ToString() ?? "null"
            };
            lines.Add($"  {trait.Key}: {valueStr}");
        }

        return string.Join("\n", lines);
    }
}
