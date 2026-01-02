namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a world location or environment.
/// </summary>
public class Location
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; } // "town", "dungeon", "wilderness", "environment", "region"
    public string? ParentRegion { get; set; }
    public int Level { get; set; }
    public int DangerRating { get; set; }
    public List<string> Features { get; set; } = new();
    public List<string> Npcs { get; set; } = new();
    public List<string> Enemies { get; set; } = new();
    public List<string> Loot { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Fully resolved NPC objects present at this location.
    /// Populated by LocationGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in Npcs instead).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<NPC> NpcObjects { get; set; } = new();

    /// <summary>
    /// Fully resolved Enemy objects that can spawn at this location.
    /// Populated by LocationGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in Enemies instead).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Enemy> EnemyObjects { get; set; } = new();

    /// <summary>
    /// Fully resolved Item objects available as loot at this location.
    /// Populated by LocationGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in Loot instead).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Item> LootObjects { get; set; } = new();
}

/// <summary>
/// Represents an organization (guild, faction, shop, business).
/// </summary>
public class Organization
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; } // "guild", "faction", "shop", "business"
    public string? Leader { get; set; }
    public List<string> Members { get; set; } = new();
    public int Reputation { get; set; }
    public int Wealth { get; set; }
    public List<string> Services { get; set; } = new();
    public List<string> Inventory { get; set; } = new();
    public Dictionary<string, int> Prices { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Fully resolved NPC objects who are members of this organization.
    /// Populated by OrganizationGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in Members instead).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<NPC> MemberObjects { get; set; } = new();

    /// <summary>
    /// Fully resolved Item objects available in this organization's inventory.
    /// Populated by OrganizationGenerator.GenerateAsync() when hydrating templates.
    /// Not serialized to JSON (template IDs stored in Inventory instead).
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Item> InventoryObjects { get; set; } = new();
}

/// <summary>
/// Represents a dialogue line or response.
/// </summary>
public class DialogueLine
{
    public required string Id { get; set; }
    public required string Text { get; set; }
    public required string Type { get; set; } // "greeting", "farewell", "response"
    public required string Style { get; set; } // "formal", "casual", "aggressive", etc.
    public string? Context { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
