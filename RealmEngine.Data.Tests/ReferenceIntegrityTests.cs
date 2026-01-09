using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RealmEngine.Data.Tests;

/// <summary>
/// Tests to verify all JSON references (skillReference, abilityReference, etc.) resolve correctly.
/// These tests ensure data integrity and will fail the build if any reference is broken.
/// </summary>
public class ReferenceIntegrityTests
{
    private readonly string _dataRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");

    [Fact]
    public void All_Weapon_SkillReferences_Should_Point_To_Valid_Skills()
    {
        // Arrange
        var weaponsPath = Path.Combine(_dataRoot, "items", "weapons", "catalog.json");
        var skillsPath = Path.Combine(_dataRoot, "skills", "catalog.json");

        var weaponsData = JObject.Parse(File.ReadAllText(weaponsPath));
        var skillsData = JObject.Parse(File.ReadAllText(skillsPath));

        // Get all valid weapon skill slugs
        var validSkills = new HashSet<string>();
        var weaponSkills = skillsData["skill_types"]?["weapon"]?["items"] as JArray;
        if (weaponSkills != null)
        {
            foreach (var skill in weaponSkills)
            {
                var slug = skill["slug"]?.ToString();
                if (!string.IsNullOrEmpty(slug))
                {
                    validSkills.Add(slug);
                }
            }
        }

        validSkills.Should().NotBeEmpty("weapon skills should exist in catalog");

        // Check all weapon type skillReferences
        var weaponTypes = weaponsData["weapon_types"] as JObject;
        var errors = new List<string>();

        foreach (var weaponType in weaponTypes!.Properties())
        {
            var properties = weaponType.Value["properties"];
            var skillRef = properties?["skillReference"]?["value"]?.ToString();

            if (string.IsNullOrEmpty(skillRef))
            {
                errors.Add($"Weapon type '{weaponType.Name}' has no skillReference");
                continue;
            }

            // Extract skill slug from reference (@skills/weapon:heavy-blades -> heavy-blades)
            if (skillRef.StartsWith("@skills/weapon:"))
            {
                var skillSlug = skillRef.Substring("@skills/weapon:".Length);
                if (!validSkills.Contains(skillSlug))
                {
                    errors.Add($"Weapon type '{weaponType.Name}' references invalid skill: '{skillSlug}' (reference: {skillRef})");
                }
            }
            else
            {
                errors.Add($"Weapon type '{weaponType.Name}' has invalid reference format: '{skillRef}'");
            }
        }

        // Assert
        errors.Should().BeEmpty($"all weapon skill references should be valid:\n{string.Join("\n", errors)}");
    }

    [Fact]
    public void All_Armor_SkillReferences_Should_Point_To_Valid_Skills()
    {
        // Arrange
        var armorPath = Path.Combine(_dataRoot, "items", "armor", "catalog.json");
        var skillsPath = Path.Combine(_dataRoot, "skills", "catalog.json");

        var armorData = JObject.Parse(File.ReadAllText(armorPath));
        var skillsData = JObject.Parse(File.ReadAllText(skillsPath));

        // Get all valid armor skill slugs
        var validSkills = new HashSet<string>();
        var armorSkills = skillsData["skill_types"]?["armor"]?["items"] as JArray;
        if (armorSkills != null)
        {
            foreach (var skill in armorSkills)
            {
                var slug = skill["slug"]?.ToString();
                if (!string.IsNullOrEmpty(slug))
                {
                    validSkills.Add(slug);
                }
            }
        }

        validSkills.Should().NotBeEmpty("armor skills should exist in catalog");

        // Check all armor type skillReferences
        var armorTypes = armorData["armor_types"] as JObject;
        var errors = new List<string>();

        foreach (var armorType in armorTypes!.Properties())
        {
            var properties = armorType.Value["properties"];
            var skillRef = properties?["skillReference"]?["value"]?.ToString();

            if (string.IsNullOrEmpty(skillRef))
            {
                errors.Add($"Armor type '{armorType.Name}' has no skillReference");
                continue;
            }

            // Extract skill slug from reference (@skills/armor:light-armor -> light-armor)
            if (skillRef.StartsWith("@skills/armor:"))
            {
                var skillSlug = skillRef.Substring("@skills/armor:".Length);
                if (!validSkills.Contains(skillSlug))
                {
                    errors.Add($"Armor type '{armorType.Name}' references invalid skill: '{skillSlug}' (reference: {skillRef})");
                }
            }
            else
            {
                errors.Add($"Armor type '{armorType.Name}' has invalid reference format: '{skillRef}'");
            }
        }

        // Assert
        errors.Should().BeEmpty($"all armor skill references should be valid:\n{string.Join("\n", errors)}");
    }

    [Fact]
    public void All_Weapon_Types_Should_Have_SkillReference()
    {
        // Arrange
        var weaponsPath = Path.Combine(_dataRoot, "items", "weapons", "catalog.json");
        var weaponsData = JObject.Parse(File.ReadAllText(weaponsPath));

        // Act
        var weaponTypes = weaponsData["weapon_types"] as JObject;
        var typesWithoutReference = new List<string>();

        foreach (var weaponType in weaponTypes!.Properties())
        {
            var properties = weaponType.Value["properties"];
            var skillRef = properties?["skillReference"]?["value"]?.ToString();

            if (string.IsNullOrEmpty(skillRef))
            {
                typesWithoutReference.Add(weaponType.Name);
            }
        }

        // Assert
        typesWithoutReference.Should().BeEmpty("all weapon types must have a skillReference trait");
    }

    [Fact]
    public void All_Armor_Types_Should_Have_SkillReference()
    {
        // Arrange
        var armorPath = Path.Combine(_dataRoot, "items", "armor", "catalog.json");
        var armorData = JObject.Parse(File.ReadAllText(armorPath));

        // Act
        var armorTypes = armorData["armor_types"] as JObject;
        var typesWithoutReference = new List<string>();

        foreach (var armorType in armorTypes!.Properties())
        {
            var properties = armorType.Value["properties"];
            var skillRef = properties?["skillReference"]?["value"]?.ToString();

            if (string.IsNullOrEmpty(skillRef))
            {
                typesWithoutReference.Add(armorType.Name);
            }
        }

        // Assert
        typesWithoutReference.Should().BeEmpty("all armor types must have a skillReference trait");
    }

    [Fact]
    public void SkillReference_Format_Should_Be_Valid_For_Weapons()
    {
        // Arrange
        var weaponsPath = Path.Combine(_dataRoot, "items", "weapons", "catalog.json");
        var weaponsData = JObject.Parse(File.ReadAllText(weaponsPath));

        // Act
        var weaponTypes = weaponsData["weapon_types"] as JObject;
        var invalidFormats = new List<string>();

        foreach (var weaponType in weaponTypes!.Properties())
        {
            var traits = weaponType.Value["traits"];
            var skillRef = traits?["skillReference"]?["value"]?.ToString();

            if (!string.IsNullOrEmpty(skillRef))
            {
                // Must match format: @skills/weapon:skill-slug
                if (!skillRef.StartsWith("@skills/weapon:") || !skillRef.Contains(":"))
                {
                    invalidFormats.Add($"{weaponType.Name}: '{skillRef}'");
                }
            }
        }

        // Assert
        invalidFormats.Should().BeEmpty($"all weapon skillReferences must follow format '@skills/weapon:skill-slug':\n{string.Join("\n", invalidFormats)}");
    }

    [Fact]
    public void SkillReference_Format_Should_Be_Valid_For_Armor()
    {
        // Arrange
        var armorPath = Path.Combine(_dataRoot, "items", "armor", "catalog.json");
        var armorData = JObject.Parse(File.ReadAllText(armorPath));

        // Act
        var armorTypes = armorData["armor_types"] as JObject;
        var invalidFormats = new List<string>();

        foreach (var armorType in armorTypes!.Properties())
        {
            var traits = armorType.Value["traits"];
            var skillRef = traits?["skillReference"]?["value"]?.ToString();

            if (!string.IsNullOrEmpty(skillRef))
            {
                // Must match format: @skills/armor:skill-slug
                if (!skillRef.StartsWith("@skills/armor:") || !skillRef.Contains(":"))
                {
                    invalidFormats.Add($"{armorType.Name}: '{skillRef}'");
                }
            }
        }

        // Assert
        invalidFormats.Should().BeEmpty($"all armor skillReferences must follow format '@skills/armor:skill-slug':\n{string.Join("\n", invalidFormats)}");
    }

    [Fact]
    public void All_Armor_Categories_Should_Match_Skill_System()
    {
        // Arrange
        var armorPath = Path.Combine(_dataRoot, "items", "armor", "catalog.json");
        var skillsPath = Path.Combine(_dataRoot, "skills", "catalog.json");

        var armorData = JObject.Parse(File.ReadAllText(armorPath));
        var skillsData = JObject.Parse(File.ReadAllText(skillsPath));

        // Get armor categories from armor catalog
        var armorCategories = ((JObject)armorData["armor_types"]!).Properties().Select(p => p.Name).ToHashSet();

        // Get armor skill slugs from skills catalog
        var armorSkills = new HashSet<string>();
        var armorSkillsArray = skillsData["skill_types"]?["armor"]?["items"] as JArray;
        if (armorSkillsArray != null)
        {
            foreach (var skill in armorSkillsArray)
            {
                var slug = skill["slug"]?.ToString();
                if (!string.IsNullOrEmpty(slug))
                {
                    armorSkills.Add(slug);
                }
            }
        }

        // Each armor category should have a corresponding skill
        var errors = new List<string>();
        foreach (var category in armorCategories)
        {
            if (!armorSkills.Contains(category))
            {
                errors.Add($"Armor category '{category}' has no matching skill in skills catalog");
            }
        }

        // Assert
        errors.Should().BeEmpty($"armor categories should match skill slugs:\n{string.Join("\n", errors)}");
    }
}
