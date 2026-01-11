using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RealmEngine.Shared.Models;
using Serilog;

namespace RealmEngine.Core.Services;

/// <summary>
/// Service for loading crafting recipe catalogs from JSON files.
/// Reads from recipes/catalog.json with unified recipe structure.
/// </summary>
public class RecipeCatalogLoader
{
    private readonly string _dataPath;
    private readonly Dictionary<string, List<Recipe>> _cache = new();
    private List<Recipe>? _allRecipesCache;

    /// <summary>
    /// Initializes a new instance of the RecipeCatalogLoader class.
    /// </summary>
    /// <param name="dataPath">The root path to the Data/Json folder (default: "Data/Json").</param>
    public RecipeCatalogLoader(string dataPath = "Data/Json")
    {
        _dataPath = dataPath;
    }

    /// <summary>
    /// Load all recipes from the unified catalog.
    /// </summary>
    /// <returns>List of all recipes across all categories.</returns>
    public List<Recipe> LoadAllRecipes()
    {
        if (_allRecipesCache != null)
        {
            return _allRecipesCache;
        }

        var recipesPath = Path.Combine(_dataPath, "recipes", "catalog.json");
        
        if (!File.Exists(recipesPath))
        {
            Log.Warning("Recipe catalog not found: {RecipesPath}", recipesPath);
            return new List<Recipe>();
        }

        try
        {
            var jsonContent = File.ReadAllText(recipesPath);
            var catalog = JObject.Parse(jsonContent);
            var allRecipes = new List<Recipe>();

            var recipeTypes = catalog["recipe_types"] as JObject;
            if (recipeTypes == null)
            {
                Log.Warning("Missing 'recipe_types' in recipe catalog");
                return new List<Recipe>();
            }

            // Iterate through each recipe type (weapons, consumables, etc.)
            foreach (var typeProperty in recipeTypes.Properties())
            {
                var categoryName = typeProperty.Name;
                var typeData = typeProperty.Value as JObject;
                var items = typeData?["items"] as JArray;

                if (items == null) continue;

                foreach (var recipeToken in items)
                {
                    var recipe = ParseRecipe(recipeToken, categoryName);
                    if (recipe != null)
                    {
                        allRecipes.Add(recipe);
                    }
                }
            }

            _allRecipesCache = allRecipes;
            Log.Information("Loaded {Count} recipes from catalog", allRecipes.Count);
            return allRecipes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading recipe catalog");
            return new List<Recipe>();
        }
    }

    /// <summary>
    /// Load recipes from a specific category.
    /// </summary>
    /// <param name="category">The recipe category (weapons, consumables, enchantments).</param>
    /// <returns>List of recipes in the specified category.</returns>
    public List<Recipe> LoadRecipesByCategory(string category)
    {
        if (_cache.ContainsKey(category))
        {
            return _cache[category];
        }

        var recipesPath = Path.Combine(_dataPath, "crafting", "recipes", $"{category}.json");
        
        if (!File.Exists(recipesPath))
        {
            Log.Warning("Recipe catalog not found: {RecipesPath}", recipesPath);
            return new List<Recipe>();
        }

        try
        {
            var jsonContent = File.ReadAllText(recipesPath);
            var catalog = JObject.Parse(jsonContent);
            var recipes = new List<Recipe>();

            var recipesArray = catalog["recipes"] as JArray;
            if (recipesArray == null)
            {
                Log.Warning("Missing 'recipes' array in catalog: {Category}", category);
                return new List<Recipe>();
            }

            foreach (var recipeToken in recipesArray)
            {
                var recipe = ParseRecipe(recipeToken);
                if (recipe != null)
                {
                    recipes.Add(recipe);
                }
            }

            _cache[category] = recipes;
            Log.Information("Loaded {Count} recipes from {Category} catalog", recipes.Count, category);
            return recipes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading recipe catalog {Category}", category);
            return new List<Recipe>();
        }
    }

    /// <summary>
    /// Get a specific recipe by its ID.
    /// </summary>
    /// <param name="recipeId">The unique recipe ID.</param>
    /// <returns>The recipe if found, otherwise null.</returns>
    public Recipe? GetRecipeById(string recipeId)
    {
        var allRecipes = LoadAllRecipes();
        return allRecipes.FirstOrDefault(r => r.Id == recipeId);
    }

    /// <summary>
    /// Get all recipes that a character can craft based on skill level.
    /// </summary>
    /// <param name="skillLevel">The character's crafting skill level.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>List of craftable recipes.</returns>
    public List<Recipe> GetAvailableRecipes(int skillLevel, string? category = null)
    {
        var recipes = category != null 
            ? LoadRecipesByCategory(category) 
            : LoadAllRecipes();

        return recipes.Where(r => r.RequiredSkillLevel <= skillLevel).ToList();
    }

    /// <summary>
    /// Parse a recipe from JSON token.
    /// </summary>
    private Recipe? ParseRecipe(JToken recipeToken)
    {
        try
        {
            var recipe = new Recipe
            {
                Id = recipeToken["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                Name = recipeToken["name"]?.ToString() ?? string.Empty,
                Description = recipeToken["description"]?.ToString() ?? string.Empty,
                Category = recipeToken["category"]?.ToString() ?? string.Empty,
                RequiredSkillLevel = recipeToken["requiredSkillLevel"]?.ToObject<int>() ?? 1,
                SkillGainOnCraft = recipeToken["skillGainOnCraft"]?.ToObject<int>() ?? 1,
                CraftingTime = recipeToken["craftingTime"]?.ToObject<int>() ?? 1,
                BaseQuality = recipeToken["baseQuality"]?.ToObject<int>() ?? 50,
                QualityRange = recipeToken["qualityRange"]?.ToObject<int>() ?? 20
            };

            // Parse required station
            if (recipeToken["requiredStation"] != null)
            {
                recipe.RequiredStation = Enum.TryParse<CraftingStationType>(
                    recipeToken["requiredStation"]!.ToString(), 
                    true, 
                    out var stationType) 
                    ? stationType 
                    : CraftingStationType.None;
            }

            // Parse unlock method
            if (recipeToken["unlockMethod"] != null)
            {
                recipe.UnlockMethod = Enum.TryParse<RecipeUnlockMethod>(
                    recipeToken["unlockMethod"]!.ToString(), 
                    true, 
                    out var unlockMethod) 
                    ? unlockMethod 
                    : RecipeUnlockMethod.SkillLevel;
            }

            // Parse materials
            var materialsArray = recipeToken["materials"] as JArray;
            if (materialsArray != null)
            {
                foreach (var materialToken in materialsArray)
                {
                    var material = new RecipeMaterial
                    {
                        ItemId = materialToken["itemId"]?.ToString() ?? string.Empty,
                        Quantity = materialToken["quantity"]?.ToObject<int>() ?? 1,
                        Category = materialToken["category"]?.ToString() ?? string.Empty
                    };
                    recipe.Materials.Add(material);
                }
            }

            // Parse output
            if (recipeToken["output"] != null)
            {
                recipe.OutputItemId = recipeToken["output"]!["itemId"]?.ToString() ?? string.Empty;
                recipe.OutputQuantity = recipeToken["output"]!["quantity"]?.ToObject<int>() ?? 1;
            }

            return recipe;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error parsing recipe from JSON");
            return null;
        }
    }

    /// <summary>
    /// Clear the internal cache.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }
}
