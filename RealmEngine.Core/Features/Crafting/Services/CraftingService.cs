using RealmEngine.Shared.Models;
using RealmEngine.Core.Services;
using Serilog;

namespace RealmEngine.Core.Features.Crafting.Services;

/// <summary>
/// Service for managing crafting operations.
/// Validates recipes, checks materials, and determines crafting outcomes.
/// </summary>
public class CraftingService
{
    private readonly RecipeCatalogLoader _recipeCatalogLoader;

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftingService"/> class.
    /// </summary>
    /// <param name="recipeCatalogLoader">The recipe catalog loader.</param>
    public CraftingService(RecipeCatalogLoader recipeCatalogLoader)
    {
        _recipeCatalogLoader = recipeCatalogLoader ?? throw new ArgumentNullException(nameof(recipeCatalogLoader));
    }

    /// <summary>
    /// Gets all recipes available to a character based on their skill levels.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="stationId">Optional crafting station ID filter.</param>
    /// <returns>List of recipes the character can see (not necessarily craft).</returns>
    public List<Recipe> GetAvailableRecipes(Character character, string? stationId = null)
    {
        if (character == null)
            throw new ArgumentNullException(nameof(character));

        var allRecipes = _recipeCatalogLoader.LoadAllRecipes();
        var availableRecipes = new List<Recipe>();

        foreach (var recipe in allRecipes)
        {
            // Filter by station if specified
            if (!string.IsNullOrEmpty(stationId) && recipe.RequiredStation != stationId)
                continue;

            // Check if character has unlocked this recipe
            if (IsRecipeUnlocked(character, recipe))
            {
                availableRecipes.Add(recipe);
            }
        }

        return availableRecipes;
    }

    /// <summary>
    /// Checks if a character can craft a specific recipe.
    /// </summary>
    /// <param name="character">The character attempting to craft.</param>
    /// <param name="recipe">The recipe to craft.</param>
    /// <param name="failureReason">Output parameter with failure reason if crafting is not possible.</param>
    /// <returns>True if the character can craft the recipe, false otherwise.</returns>
    public bool CanCraftRecipe(Character character, Recipe recipe, out string failureReason)
    {
        if (character == null)
        {
            failureReason = "Character cannot be null";
            return false;
        }

        if (recipe == null)
        {
            failureReason = "Recipe cannot be null";
            return false;
        }

        // Check if recipe is unlocked
        if (!IsRecipeUnlocked(character, recipe))
        {
            failureReason = $"Recipe '{recipe.Name}' is not unlocked";
            return false;
        }

        // Check skill level requirement
        var craftingSkill = GetCraftingSkillForStation(recipe.RequiredStation);
        var skill = character.Skills?.GetValueOrDefault(craftingSkill);
        var characterSkillLevel = skill?.CurrentRank ?? 0;

        if (characterSkillLevel < recipe.RequiredSkillLevel)
        {
            failureReason = $"Requires {craftingSkill} skill level {recipe.RequiredSkillLevel} (current: {characterSkillLevel})";
            return false;
        }

        // Check materials
        if (!ValidateMaterials(character, recipe, out var materialFailure))
        {
            failureReason = materialFailure;
            return false;
        }

        failureReason = string.Empty;
        return true;
    }

    /// <summary>
    /// Validates that the character has all required materials for a recipe.
    /// </summary>
    /// <param name="character">The character to check.</param>
    /// <param name="recipe">The recipe requiring materials.</param>
    /// <param name="failureReason">Output parameter with missing material details.</param>
    /// <returns>True if all materials are available, false otherwise.</returns>
    public bool ValidateMaterials(Character character, Recipe recipe, out string failureReason)
    {
        if (character.Inventory == null)
        {
            failureReason = "Character has no inventory";
            return false;
        }

        foreach (var material in recipe.Materials)
        {
            var availableCount = CountMaterialInInventory(character, material.ItemReference);

            if (availableCount < material.Quantity)
            {
                failureReason = $"Missing {material.ItemReference}: need {material.Quantity}, have {availableCount}";
                return false;
            }
        }

        failureReason = string.Empty;
        return true;
    }

    /// <summary>
    /// Calculates the quality of a crafted item based on character skill and recipe parameters.
    /// </summary>
    /// <param name="character">The character crafting the item.</param>
    /// <param name="recipe">The recipe being crafted.</param>
    /// <returns>Quality value (0-100).</returns>
    public int CalculateQuality(Character character, Recipe recipe)
    {
        if (character == null || recipe == null)
            return (int)(recipe?.MinQuality ?? ItemRarity.Common);

        var craftingSkill = GetCraftingSkillForStation(recipe.RequiredStation);
        var skill = character.Skills?.GetValueOrDefault(craftingSkill);
        var characterSkillLevel = skill?.CurrentRank ?? 0;

        // Calculate rarity based on skill level
        var minRarity = (int)recipe.MinQuality;
        var maxRarity = (int)recipe.MaxQuality;
        var rarityRange = maxRarity - minRarity;

        // Add skill bonus (skill above requirement improves rarity chance)
        var skillOverRequirement = characterSkillLevel - recipe.RequiredSkillLevel;
        var rarityBonus = skillOverRequirement > 0 ? Math.Min(skillOverRequirement / 10, rarityRange) : 0;

        // Random variance within rarity range
        var random = new Random();
        var finalRarity = minRarity + rarityBonus + random.Next(0, Math.Max(1, rarityRange - rarityBonus + 1));

        // Clamp to valid rarity range
        return Math.Clamp(finalRarity, minRarity, maxRarity);
    }

    /// <summary>
    /// Checks if a recipe is unlocked for a character based on unlock method.
    /// </summary>
    private bool IsRecipeUnlocked(Character character, Recipe recipe)
    {
        switch (recipe.UnlockMethod)
        {
            case RecipeUnlockMethod.SkillLevel:
                // Recipe is unlocked when character reaches the required skill level
                var craftingSkill = GetCraftingSkillForStation(recipe.RequiredStation);
                var skill = character.Skills?.GetValueOrDefault(craftingSkill);
                var skillLevel = skill?.CurrentRank ?? 0;
                return skillLevel >= recipe.RequiredSkillLevel;

            case RecipeUnlockMethod.Trainer:
                // Recipe must be explicitly learned (check character's learned recipes)
                return character.LearnedRecipes?.Contains(recipe.Id) ?? false;

            case RecipeUnlockMethod.QuestReward:
                // Quest-unlocked recipes require quest completion
                return character.LearnedRecipes?.Contains(recipe.Id) ?? false;

            case RecipeUnlockMethod.Discovery:
                // Discovery recipes must be found through experimentation
                return character.LearnedRecipes?.Contains(recipe.Id) ?? false;

            default:
                return true;
        }
    }

    /// <summary>
    /// Gets the skill name associated with a crafting station.
    /// </summary>
    private string GetCraftingSkillForStation(string station)
    {
        return station.ToLowerInvariant() switch
        {
            "anvil" or "blacksmith_forge" => "Blacksmithing",
            "alchemytable" or "alchemy_table" => "Alchemy",
            "enchantingtable" or "enchanting_altar" => "Enchanting",
            "cookingfire" or "cooking_fire" => "Cooking",
            "workbench" => "Carpentry",
            "loom" => "Tailoring",
            "tanningrack" or "tanning_rack" => "Leatherworking",
            "jewelrybench" or "jewelry_bench" => "Jewelcrafting",
            _ => "Crafting"
        };
    }

    /// <summary>
    /// Counts how many of a specific material the character has in their inventory.
    /// </summary>
    private int CountMaterialInInventory(Character character, string itemId)
    {
        if (character.Inventory == null)
            return 0;

        return character.Inventory
            .Count(item => item.Id == itemId || item.Name == itemId);
    }
}
