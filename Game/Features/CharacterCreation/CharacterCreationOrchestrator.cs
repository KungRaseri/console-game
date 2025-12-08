using Game.Shared.Data;
using Game.Models;
using Game.Shared.UI;
using Game.Shared.Services;
using Game.Services;
using Game.Features.SaveLoad;
using MediatR;
using Serilog;

namespace Game.Features.CharacterCreation;

/// <summary>
/// Orchestrates the entire character creation flow including class selection,
/// attribute allocation, and character review.
/// </summary>
public class CharacterCreationOrchestrator
{
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;

    public CharacterCreationOrchestrator(IMediator mediator, SaveGameService saveGameService)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
    }

    /// <summary>
    /// Handles the complete character creation process.
    /// Returns a tuple of (created character, save game ID, success flag).
    /// </summary>
    public async Task<(Character? Character, string? SaveId, bool Success)> CreateCharacterAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Character Creation", "Forge your legend");

        // Step 1: Enter name
        var playerName = ConsoleUI.AskForInput("What is your name, brave adventurer?");
        
        // Step 2: Choose class
        var selectedClass = await SelectCharacterClassAsync();
        if (selectedClass == null)
        {
            return (null, null, false);
        }
        
        // Step 3: Allocate attributes
        var allocation = await AllocateAttributesAsync(selectedClass);
        if (allocation == null)
        {
            return (null, null, false);
        }
        
        // Step 4: Create character
        var newCharacter = CharacterCreationService.CreateCharacter(playerName, selectedClass.Name, allocation);
        
        // Step 5: Review character
        ReviewCharacter(newCharacter, selectedClass);

        ConsoleUI.ShowSuccess($"Welcome, {newCharacter.Name} the {newCharacter.ClassName}!");
        await Task.Delay(500);

        // Publish character created event
        await _mediator.Publish(new CharacterCreated(newCharacter.Name));

        // Create save game with the new character
        var saveGame = _saveGameService.CreateNewGame(newCharacter);
        
        Log.Information("Character created: {CharacterName} ({ClassName})", newCharacter.Name, newCharacter.ClassName);

        return (newCharacter, saveGame.Id, true);
    }
    
    /// <summary>
    /// Let the player select their character class.
    /// </summary>
    private async Task<CharacterClass?> SelectCharacterClassAsync()
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner("Choose Your Class", "Each class offers unique strengths");
        
        var classes = CharacterClassRepository.GetAllClasses();
        
        // Display all classes with details
        foreach (var cls in classes)
        {
            ConsoleUI.WriteText("");
            ConsoleUI.ShowPanel(
                $"{cls.Name} ({string.Join(", ", cls.PrimaryAttributes)})",
                $"{cls.Description}\n\n{cls.FlavorText}\n\n" +
                $"[cyan]Attribute Bonuses:[/]\n" +
                (cls.BonusStrength > 0 ? $"  +{cls.BonusStrength} Strength\n" : "") +
                (cls.BonusDexterity > 0 ? $"  +{cls.BonusDexterity} Dexterity\n" : "") +
                (cls.BonusConstitution > 0 ? $"  +{cls.BonusConstitution} Constitution\n" : "") +
                (cls.BonusIntelligence > 0 ? $"  +{cls.BonusIntelligence} Intelligence\n" : "") +
                (cls.BonusWisdom > 0 ? $"  +{cls.BonusWisdom} Wisdom\n" : "") +
                (cls.BonusCharisma > 0 ? $"  +{cls.BonusCharisma} Charisma\n" : "") +
                $"\n[yellow]Health Bonus:[/] {(cls.StartingHealthBonus >= 0 ? "+" : "")}{cls.StartingHealthBonus}\n" +
                $"[blue]Mana Bonus:[/] {(cls.StartingManaBonus >= 0 ? "+" : "")}{cls.StartingManaBonus}",
                "yellow"
            );
        }
        
        var classNames = classes.Select(c => c.Name).Append("Back to Menu").ToArray();
        var choice = ConsoleUI.ShowMenu("Select your class:", classNames);
        
        if (choice == "Back to Menu")
        {
            return null;
        }
        
        var selected = classes.FirstOrDefault(c => c.Name == choice);
        
        if (selected != null)
        {
            ConsoleUI.ShowSuccess($"You have chosen the path of the {selected.Name}!");
            await Task.Delay(300);
        }
        
        return selected;
    }
    
    /// <summary>
    /// Let the player allocate attribute points.
    /// </summary>
    private async Task<AttributeAllocation?> AllocateAttributesAsync(CharacterClass selectedClass)
    {
        var allocation = new AttributeAllocation();
        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        
        while (true)
        {
            ConsoleUI.Clear();
            ConsoleUI.ShowBanner("Attribute Allocation", $"Points Remaining: {allocation.GetRemainingPoints()}/27");
            
            // Show current allocation
            var allocationLines = new List<string>();
            allocationLines.Add("[yellow]Current Attributes:[/]");
            allocationLines.Add("");
            
            foreach (var attr in attributes)
            {
                var current = allocation.GetAttributeValue(attr);
                var withBonus = current + GetClassBonus(selectedClass, attr);
                var isPrimary = selectedClass.PrimaryAttributes.Contains(attr);
                var primaryMark = isPrimary ? " [cyan]★[/]" : "";
                var color = isPrimary ? "cyan" : "white";
                
                allocationLines.Add($"  [{color}]{attr}:{primaryMark,-20}[/] {current,2} " +
                    $"(+{GetClassBonus(selectedClass, attr)} class) = [green]{withBonus}[/]");
            }
            
            allocationLines.Add("");
            allocationLines.Add("[grey]★ = Primary attribute for your class[/]");
            allocationLines.Add("");
            allocationLines.Add($"[yellow]Points Spent:[/] {allocation.GetPointsSpent()}/27");
            allocationLines.Add($"[cyan]Points Remaining:[/] {allocation.GetRemainingPoints()}");
            
            ConsoleUI.ShowPanel("Your Attributes", string.Join("\n", allocationLines), "green");
            
            // Menu options
            var options = new List<string>();
            foreach (var attr in attributes)
            {
                if (allocation.CanIncrease(attr))
                    options.Add($"Increase {attr}");
                if (allocation.CanDecrease(attr))
                    options.Add($"Decrease {attr}");
            }
            
            options.Add("Auto-Allocate (Recommended)");
            options.Add("Reset All");
            options.Add("Confirm & Continue");
            options.Add("Back to Class Selection");
            
            var choice = ConsoleUI.ShowMenu("Adjust attributes:", options.ToArray());
            
            if (choice.StartsWith("Increase "))
            {
                var attr = choice.Replace("Increase ", "");
                var current = allocation.GetAttributeValue(attr);
                allocation.SetAttributeValue(attr, current + 1);
            }
            else if (choice.StartsWith("Decrease "))
            {
                var attr = choice.Replace("Decrease ", "");
                var current = allocation.GetAttributeValue(attr);
                allocation.SetAttributeValue(attr, current - 1);
            }
            else if (choice == "Auto-Allocate (Recommended)")
            {
                // Auto-allocate based on class
                allocation = AutoAllocateAttributes(selectedClass);
                ConsoleUI.ShowSuccess("Attributes auto-allocated based on your class!");
                await Task.Delay(300);
            }
            else if (choice == "Reset All")
            {
                allocation = new AttributeAllocation();
                ConsoleUI.ShowInfo("Attributes reset to base values.");
                await Task.Delay(200);
            }
            else if (choice == "Confirm & Continue")
            {
                if (allocation.GetRemainingPoints() > 0)
                {
                    if (ConsoleUI.Confirm($"You have {allocation.GetRemainingPoints()} unspent points. Continue anyway?"))
                    {
                        return allocation;
                    }
                }
                else
                {
                    return allocation;
                }
            }
            else if (choice == "Back to Class Selection")
            {
                return null;
            }
        }
    }
    
    /// <summary>
    /// Auto-allocate attributes based on class recommendations.
    /// </summary>
    private AttributeAllocation AutoAllocateAttributes(CharacterClass characterClass)
    {
        var allocation = new AttributeAllocation();
        
        // Prioritize primary attributes
        foreach (var primary in characterClass.PrimaryAttributes)
        {
            allocation.SetAttributeValue(primary, 14); // High in primary
        }
        
        // Distribute remaining points to secondary stats
        var remaining = allocation.GetRemainingPoints();
        var secondaryAttrs = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" }
            .Where(a => !characterClass.PrimaryAttributes.Contains(a))
            .ToArray();
        
        // Bring all secondaries to 10 if possible
        foreach (var attr in secondaryAttrs)
        {
            if (remaining <= 0) break;
            var current = allocation.GetAttributeValue(attr);
            if (current < 10 && remaining >= 2)
            {
                allocation.SetAttributeValue(attr, 10);
                remaining = allocation.GetRemainingPoints();
            }
        }
        
        // Spend any leftover points on primary stats
        while (remaining > 0)
        {
            foreach (var primary in characterClass.PrimaryAttributes)
            {
                if (remaining <= 0) break;
                if (allocation.CanIncrease(primary))
                {
                    var current = allocation.GetAttributeValue(primary);
                    allocation.SetAttributeValue(primary, current + 1);
                    remaining = allocation.GetRemainingPoints();
                }
            }
            
            // Safety: if we can't allocate more, break
            if (!characterClass.PrimaryAttributes.Any(p => allocation.CanIncrease(p)))
                break;
        }
        
        return allocation;
    }
    
    /// <summary>
    /// Get class bonus for a specific attribute.
    /// </summary>
    private int GetClassBonus(CharacterClass characterClass, string attribute)
    {
        return attribute switch
        {
            "Strength" => characterClass.BonusStrength,
            "Dexterity" => characterClass.BonusDexterity,
            "Constitution" => characterClass.BonusConstitution,
            "Intelligence" => characterClass.BonusIntelligence,
            "Wisdom" => characterClass.BonusWisdom,
            "Charisma" => characterClass.BonusCharisma,
            _ => 0
        };
    }
    
    /// <summary>
    /// Review the final character before starting.
    /// </summary>
    private void ReviewCharacter(Character character, CharacterClass characterClass)
    {
        CharacterViewService.ReviewCharacter(character, characterClass);
    }
}
