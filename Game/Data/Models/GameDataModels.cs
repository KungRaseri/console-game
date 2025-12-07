namespace Game.Data.Models;

/// <summary>
/// Data models for deserializing JSON game data files.
/// </summary>

// Item-related data models
public class WeaponPrefixData
{
    public List<string> Common { get; set; } = new();
    public List<string> Uncommon { get; set; } = new();
    public List<string> Rare { get; set; } = new();
    public List<string> Epic { get; set; } = new();
    public List<string> Legendary { get; set; } = new();
}

public class WeaponNameData
{
    public List<string> Swords { get; set; } = new();
    public List<string> Axes { get; set; } = new();
    public List<string> Bows { get; set; } = new();
    public List<string> Daggers { get; set; } = new();
    public List<string> Spears { get; set; } = new();
    public List<string> Maces { get; set; } = new();
    public List<string> Staves { get; set; } = new();
}

public class ArmorMaterialData
{
    public List<string> Common { get; set; } = new();
    public List<string> Uncommon { get; set; } = new();
    public List<string> Rare { get; set; } = new();
    public List<string> Epic { get; set; } = new();
    public List<string> Legendary { get; set; } = new();
}

public class EnchantmentSuffixData
{
    public List<string> Power { get; set; } = new();
    public List<string> Protection { get; set; } = new();
    public List<string> Wisdom { get; set; } = new();
    public List<string> Agility { get; set; } = new();
    public List<string> Magic { get; set; } = new();
    public List<string> Fire { get; set; } = new();
    public List<string> Ice { get; set; } = new();
    public List<string> Lightning { get; set; } = new();
    public List<string> Life { get; set; } = new();
    public List<string> Death { get; set; } = new();
}

// Enemy-related data models
public class EnemyNameData
{
    public List<string> Prefixes { get; set; } = new();
    public List<string> Creatures { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

public class DragonNameData
{
    public List<string> Prefixes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

public class HumanoidNameData
{
    public List<string> Professions { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public List<string> Factions { get; set; } = new();
    public Dictionary<string, List<string>> Variants { get; set; } = new();
}

// NPC-related data models
public class FantasyNameData
{
    public List<string> Male { get; set; } = new();
    public List<string> Female { get; set; } = new();
    public List<string> Surnames { get; set; } = new();
}

public class OccupationData
{
    public List<string> Merchants { get; set; } = new();
    public List<string> Craftsmen { get; set; } = new();
    public List<string> Professionals { get; set; } = new();
    public List<string> Service { get; set; } = new();
    public List<string> Nobility { get; set; } = new();
    public List<string> Religious { get; set; } = new();
    public List<string> Adventurers { get; set; } = new();
    public List<string> Magical { get; set; } = new();
    public List<string> Criminal { get; set; } = new();
    public List<string> Common { get; set; } = new();
}

public class DialogueTemplateData
{
    public List<string> Greetings { get; set; } = new();
    public List<string> Merchants { get; set; } = new();
    public List<string> Quests { get; set; } = new();
    public List<string> Rumors { get; set; } = new();
    public List<string> Farewells { get; set; } = new();
    public List<string> Hostile { get; set; } = new();
    public List<string> Friendly { get; set; } = new();
}

// General data models
public class AdjectiveData
{
    public List<string> Positive { get; set; } = new();
    public List<string> Negative { get; set; } = new();
    public List<string> Size { get; set; } = new();
    public List<string> Appearance { get; set; } = new();
    public List<string> Condition { get; set; } = new();
}

public class MaterialData
{
    public List<string> Metals { get; set; } = new();
    public List<string> Precious { get; set; } = new();
    public List<string> Natural { get; set; } = new();
    public List<string> Magical { get; set; } = new();
}
