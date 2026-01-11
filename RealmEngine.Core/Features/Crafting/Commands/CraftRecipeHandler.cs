using MediatR;
using RealmEngine.Core.Features.Crafting.Services;
using RealmEngine.Shared.Models;
using Serilog;

namespace RealmEngine.Core.Features.Crafting.Commands;

/// <summary>
/// Handles crafting an item from a recipe.
/// Validates materials, consumes them, creates the item, and awards skill XP.
/// </summary>
public class CraftRecipeHandler : IRequestHandler<CraftRecipeCommand, CraftRecipeResult>
{
    private readonly CraftingService _craftingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftRecipeHandler"/> class.
    /// </summary>
    /// <param name="craftingService">The crafting service.</param>
    public CraftRecipeHandler(CraftingService craftingService)
    {
        _craftingService = craftingService ?? throw new ArgumentNullException(nameof(craftingService));
    }

    /// <summary>
    /// Handles the craft recipe command and returns the result.
    /// </summary>
    /// <param name="request">The craft recipe command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the crafting result.</returns>
    public async Task<CraftRecipeResult> Handle(CraftRecipeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the character can craft this recipe
            if (!_craftingService.CanCraftRecipe(request.Character, request.Recipe, out var failureReason))
            {
                Log.Warning("Crafting failed for {CharacterName}: {Reason}", 
                    request.Character.Name, failureReason);
                
                return new CraftRecipeResult
                {
                    Success = false,
                    Message = failureReason
                };
            }

            // Validate station matches recipe requirements
            if (request.Station.Id != request.Recipe.RequiredStation && 
                request.Station.Slug != request.Recipe.RequiredStation)
            {
                return new CraftRecipeResult
                {
                    Success = false,
                    Message = $"Recipe requires {request.Recipe.RequiredStation}, but using {request.Station.Name}"
                };
            }

            // Calculate item quality
            var quality = _craftingService.CalculateQuality(request.Character, request.Recipe);
            
            Log.Information("Crafting {RecipeName} for {CharacterName} with quality {Quality}", 
                request.Recipe.Name, request.Character.Name, quality);

            // Consume materials
            var materialsConsumed = ConsumeMaterials(request.Character, request.Recipe);

            // Create the crafted item
            var craftedItem = CreateCraftedItem(request.Recipe, quality);

            // Add the item to the character's inventory
            request.Character.Inventory ??= new List<Item>();
            request.Character.Inventory.Add(craftedItem);

            // Award skill XP
            var skillName = GetCraftingSkillForStation(request.Recipe.RequiredStation);
            var xpGained = AwardSkillXp(request.Character, skillName, request.Recipe.SkillGainOnCraft);

            Log.Information("Successfully crafted {ItemName} (quality {Quality}) for {CharacterName}. {SkillName} +{XpGained} XP",
                craftedItem.Name, quality, request.Character.Name, skillName, xpGained);

            return await Task.FromResult(new CraftRecipeResult
            {
                Success = true,
                Message = $"Successfully crafted {craftedItem.Name} (quality {quality}%)",
                CraftedItem = craftedItem,
                Quality = quality,
                SkillXpGained = xpGained,
                MaterialsConsumed = materialsConsumed
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error crafting recipe {RecipeName} for {CharacterName}", 
                request.Recipe.Name, request.Character.Name);
            
            return new CraftRecipeResult
            {
                Success = false,
                Message = $"Failed to craft item: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Consumes materials from the character's inventory.
    /// </summary>
    private List<string> ConsumeMaterials(Character character, Recipe recipe)
    {
        var consumed = new List<string>();

        foreach (var material in recipe.Materials)
        {
            var remaining = material.Quantity;
            
            // Find and remove materials from inventory
            for (int i = character.Inventory!.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var item = character.Inventory[i];
                
                if (item.Id == material.ItemId || item.Name == material.ItemId)
                {
                    var removeCount = Math.Min(remaining, item.StackSize);
                    
                    if (removeCount >= item.StackSize)
                    {
                        // Remove entire stack
                        character.Inventory.RemoveAt(i);
                    }
                    else
                    {
                        // Reduce stack size
                        item.StackSize -= removeCount;
                    }
                    
                    remaining -= removeCount;
                    consumed.Add($"{removeCount}x {material.ItemId}");
                }
            }
        }

        return consumed;
    }

    /// <summary>
    /// Creates a new item instance from the recipe with the specified quality.
    /// </summary>
    private Item CreateCraftedItem(Recipe recipe, int quality)
    {
        // TODO: In a real implementation, this would load the item template from the catalog
        // and apply quality modifiers. For now, create a basic item.
        
        var item = new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = recipe.Output.ItemName ?? recipe.Name.Replace("Recipe: ", ""),
            Description = $"A crafted item of {quality}% quality",
            Type = DetermineItemType(recipe),
            Rarity = DetermineRarity(quality),
            StackSize = recipe.Output.Quantity,
            IsCraftable = false,  // The output itself is not a recipe
            Traits = new Dictionary<string, int>()
        };

        // Apply quality-based stat bonuses
        ApplyQualityBonuses(item, quality);

        return item;
    }

    /// <summary>
    /// Determines the item type based on the recipe's output and required station.
    /// </summary>
    private ItemType DetermineItemType(Recipe recipe)
    {
        return recipe.RequiredStation.ToLowerInvariant() switch
        {
            "anvil" or "blacksmith_forge" => ItemType.Weapon,
            "alchemytable" or "alchemy_table" => ItemType.Consumable,
            "enchantingtable" or "enchanting_altar" => ItemType.EnchantmentScroll,
            "cookingfire" or "cooking_fire" => ItemType.Consumable,
            "workbench" => ItemType.Material,
            "loom" => ItemType.Armor,
            "tanningrack" or "tanning_rack" => ItemType.Armor,
            "jewelrybench" or "jewelry_bench" => ItemType.Accessory,
            _ => ItemType.Material
        };
    }

    /// <summary>
    /// Determines the item rarity based on quality (0-100).
    /// </summary>
    private ItemRarity DetermineRarity(int quality)
    {
        return quality switch
        {
            >= 90 => ItemRarity.Legendary,
            >= 75 => ItemRarity.Epic,
            >= 60 => ItemRarity.Rare,
            >= 40 => ItemRarity.Uncommon,
            _ => ItemRarity.Common
        };
    }

    /// <summary>
    /// Applies stat bonuses to the item based on quality.
    /// Higher quality items get better stats.
    /// </summary>
    private void ApplyQualityBonuses(Item item, int quality)
    {
        // Quality affects base stats
        var qualityMultiplier = quality / 100.0;
        
        // Add bonuses based on item type
        switch (item.Type)
        {
            case ItemType.Weapon:
                item.Traits["Strength"] = (int)(5 * qualityMultiplier);
                item.Traits["Dexterity"] = (int)(3 * qualityMultiplier);
                break;
            
            case ItemType.Armor:
                item.Traits["Constitution"] = (int)(5 * qualityMultiplier);
                break;
            
            case ItemType.Accessory:
                item.Traits["Charisma"] = (int)(3 * qualityMultiplier);
                break;
        }
    }

    /// <summary>
    /// Awards skill XP to the character and handles level-ups.
    /// </summary>
    private int AwardSkillXp(Character character, string skillName, int baseXp)
    {
        character.Skills ??= new Dictionary<string, CharacterSkill>();

        if (!character.Skills.ContainsKey(skillName))
        {
            character.Skills[skillName] = new CharacterSkill
            {
                SkillId = skillName,
                CurrentLevel = 0,
                CurrentXp = 0
            };
        }

        var skill = character.Skills[skillName];
        skill.CurrentXp += baseXp;

        // Check for level-ups (simplified: 100 XP per level)
        var xpPerLevel = 100;
        var levelUps = 0;
        
        while (skill.CurrentXp >= xpPerLevel && skill.CurrentLevel < 100)
        {
            skill.CurrentXp -= xpPerLevel;
            skill.CurrentLevel++;
            levelUps++;
        }

        if (levelUps > 0)
        {
            Log.Information("{CharacterName} gained {LevelUps} {SkillName} level(s)! Now level {NewLevel}",
                character.Name, levelUps, skillName, skill.CurrentLevel);
        }

        return baseXp;
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
}
