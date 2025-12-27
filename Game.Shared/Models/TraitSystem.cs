namespace Game.Shared.Models;

/// <summary>
/// Represents a trait value that can be of various types.
/// </summary>
public class TraitValue
{
    public object? Value { get; set; }
    public TraitType Type { get; set; }

    public TraitValue() { }

    public TraitValue(object? value, TraitType type)
    {
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Get the value as an integer.
    /// </summary>
    public int AsInt()
    {
        if (Value == null) return 0;
        return Convert.ToInt32(Value);
    }

    /// <summary>
    /// Get the value as a double.
    /// </summary>
    public double AsDouble()
    {
        if (Value == null) return 0.0;
        return Convert.ToDouble(Value);
    }

    /// <summary>
    /// Get the value as a string.
    /// </summary>
    public string AsString()
    {
        return Value?.ToString() ?? "";
    }

    /// <summary>
    /// Get the value as a boolean.
    /// </summary>
    public bool AsBool()
    {
        if (Value == null) return false;
        return Convert.ToBoolean(Value);
    }

    /// <summary>
    /// Get the value as a list of strings.
    /// </summary>
    public List<string> AsStringList()
    {
        if (Value is List<string> stringList)
            return stringList;

        if (Value is string[] stringArray)
            return stringArray.ToList();

        return new List<string>();
    }

    /// <summary>
    /// Get the value as a list of integers.
    /// </summary>
    public List<int> AsIntList()
    {
        if (Value is List<int> intList)
            return intList;

        if (Value is int[] intArray)
            return intArray.ToList();

        return new List<int>();
    }
}

/// <summary>
/// Defines the type of a trait value.
/// </summary>
public enum TraitType
{
    Number,        // int or double
    String,        // string
    Boolean,       // bool
    StringArray,   // List<string>
    NumberArray    // List<int> or List<double>
}

/// <summary>
/// Interface for entities that can have traits applied to them.
/// </summary>
public interface ITraitable
{
    Dictionary<string, TraitValue> Traits { get; }
}

/// <summary>
/// Standard trait names used across the game for consistency.
/// </summary>
public static class StandardTraits
{
    // Combat - Damage
    public const string DamageBonus = "damageBonus";
    public const string DamageMultiplier = "damageMultiplier";
    public const string PhysicalDamage = "physicalDamage";
    public const string MagicDamage = "magicDamage";
    public const string CriticalChance = "criticalChance";
    public const string CriticalMultiplier = "criticalMultiplier";

    // Combat - Defense
    public const string DefenseBonus = "defenseBonus";
    public const string ArmorRating = "armorRating";
    public const string BlockChance = "blockChance";
    public const string DodgeChance = "dodgeChance";

    // Health & Resources
    public const string HealthBonus = "healthBonus";
    public const string HealthMultiplier = "healthMultiplier";
    public const string ManaBonus = "manaBonus";
    public const string StaminaBonus = "staminaBonus";
    public const string HealthRegen = "healthRegen";
    public const string ManaRegen = "manaRegen";

    // Resistances
    public const string ResistFire = "resistFire";
    public const string ResistIce = "resistIce";
    public const string ResistLightning = "resistLightning";
    public const string ResistPoison = "resistPoison";
    public const string ResistPhysical = "resistPhysical";
    public const string ResistMagic = "resistMagic";

    // Stat Bonuses
    public const string StrengthBonus = "strengthBonus";
    public const string DexterityBonus = "dexterityBonus";
    public const string ConstitutionBonus = "constitutionBonus";
    public const string IntelligenceBonus = "intelligenceBonus";
    public const string WisdomBonus = "wisdomBonus";
    public const string CharismaBonus = "charismaBonus";

    // Item Properties
    public const string Durability = "durability";
    public const string MaxDurability = "maxDurability";
    public const string Weight = "weight";
    public const string WeightMultiplier = "weightMultiplier";
    public const string ValueMultiplier = "valueMultiplier";
    public const string RarityMultiplier = "rarityMultiplier";

    // Visual/Effects
    public const string GlowEffect = "glowEffect";
    public const string ParticleEffect = "particleEffect";
    public const string SoundEffect = "soundEffect";
    public const string VisualColor = "visualColor";

    // Special Abilities
    public const string LifeSteal = "lifeSteal";
    public const string ManaSteal = "manaSteal";
    public const string Thorns = "thorns";
    public const string Vampiric = "vampiric";
    public const string ChainLightning = "chainLightning";

    // Weapon Specific
    public const string AttackSpeed = "attackSpeed";
    public const string Range = "range";
    public const string IsTwoHanded = "isTwoHanded";
    public const string CanParry = "canParry";

    // Armor Specific
    public const string MovementSpeed = "movementSpeed";
    public const string NoiseReduction = "noiseReduction";
    public const string Visibility = "visibility";

    // Economic
    public const string GoldMin = "goldMin";
    public const string GoldMax = "goldMax";
    public const string PriceModifier = "priceModifier";
    public const string SellValue = "sellValue";

    // Behavior/AI
    public const string Aggressive = "aggressive";
    public const string Friendly = "friendly";
    public const string Fearful = "fearful";
    public const string Hostile = "hostile";

    // Services
    public const string CanRepair = "canRepair";
    public const string CanCraft = "canCraft";
    public const string CanEnchant = "canEnchant";
    public const string SellsItems = "sellsItems";
    public const string BuysItems = "buysItems";

    // Quality/Rarity
    public const string Quality = "quality";
    public const string LootQuality = "lootQuality";
    public const string DropChance = "dropChance";

    // Ability-Specific Traits
    public const string DamageType = "damageType";               // string: "fire", "ice", "poison", "physical", "magic", etc.
    public const string BaseDamage = "baseDamage";               // string: dice notation "2d6", "1d10", etc.
    public const string Cooldown = "cooldown";                   // number: seconds between uses
    public const string CooldownReduction = "cooldownReduction"; // number: percentage or flat reduction
    public const string Duration = "duration";                   // number: seconds the effect lasts
    public const string DurationBonus = "durationBonus";         // number: additional duration seconds
    public const string CastTime = "castTime";                   // number: seconds to activate
    public const string RangeBonus = "rangeBonus";               // number: additional range units
    public const string AreaOfEffect = "areaOfEffect";           // number: radius of effect
    public const string TargetCount = "targetCount";             // number: maximum targets affected
    public const string StatusEffect = "statusEffect";           // string: "burning", "frozen", "paralyzed", "poisoned", "stunned", "bleeding", "feared"
    public const string StatusChance = "statusChance";           // number: percentage chance to apply status
    public const string HealAmount = "healAmount";               // number: HP restored
    public const string RegenInterval = "regenInterval";         // number: seconds between regen ticks
    public const string ArmorBonus = "armorBonus";               // number: flat armor increase
    public const string ResistBonus = "resistBonus";             // number: resistance percentage
    public const string DamageReduction = "damageReduction";     // number: flat or percentage damage reduction
    public const string SummonType = "summonType";               // string: type of creature summoned
    public const string SummonCount = "summonCount";             // number: number of summons
    public const string Cleave = "cleave";                       // boolean: hits multiple targets
    public const string Piercing = "piercing";                   // boolean: ignores armor
    public const string AbilityClass = "abilityClass";           // string: "active", "passive", "toggle", "channeled", "ultimate"
    public const string TargetType = "targetType";               // string: "self", "enemy", "ally", "area", "all"
    public const string Category = "category";                   // string: "offensive", "defensive", "control", "utility", "legendary"
}
