using Game.Core.Models;
using Game.Core.Abstractions;
using Game.Console.UI;
using Game.Console.Services;
using Game.Core.Services;
using Game.Core.Features.SaveLoad;
using MediatR;
using Serilog;

namespace Game.Console.Orchestrators;

/// <summary>
/// Orchestrates the entire character creation flow including class selection,
/// attribute allocation, and character review.
/// </summary>
public class CharacterCreationOrchestrator
{
    private readonly IMediator _mediator;
    private readonly SaveGameService _saveGameService;
    private readonly ApocalypseTimer _apocalypseTimer;
    private readonly IConsoleUI _console;
    private readonly CharacterViewService _characterView;
    private readonly ICharacterClassRepository _classRepository;

    public CharacterCreationOrchestrator(
        IMediator mediator,
        SaveGameService saveGameService,
        ApocalypseTimer apocalypseTimer,
        IConsoleUI console,
        CharacterViewService characterView,
        ICharacterClassRepository classRepository)
    {
        _mediator = mediator;
        _saveGameService = saveGameService;
        _apocalypseTimer = apocalypseTimer;
        _console = console;
        _characterView = characterView;
        _classRepository = classRepository;
    }

    /// <summary>
    /// Handles the complete character creation process.
    /// Returns a tuple of (created character, save game ID, success flag).
    /// </summary>
    public async Task<(Character? Character, string? SaveId, bool Success)> CreateCharacterAsync()
    {
        _console.Clear();
        _console.ShowBanner("Character Creation", "Forge your legend");

        // Step 1: Enter name
        var playerName = _console.AskForInput("What is your name, brave adventurer?");

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
        // TODO: CharacterCreationService needs DI refactoring - temporarily create character manually
        var newCharacter = new Character
        {
            Name = playerName,
            ClassName = selectedClass.Name,
            Level = 1,
            Experience = 0,
            Health = 100,
            MaxHealth = 100,
            Mana = 50,
            MaxMana = 50,
            Strength = allocation.Strength,
            Dexterity = allocation.Dexterity,
            Constitution = allocation.Constitution,
            Intelligence = allocation.Intelligence,
            Wisdom = allocation.Wisdom,
            Charisma = allocation.Charisma,
            Gold = 100, // Starting gold
            Inventory = new List<Item>()
        };
        // var newCharacter = CharacterCreationService.CreateCharacter(playerName, selectedClass.Name, allocation);

        // Step 5: Review character
        ReviewCharacter(newCharacter, selectedClass);

        // Step 6: Select difficulty
        var difficulty = await SelectDifficultyAsync();
        if (difficulty == null)
        {
            // Player canceled difficulty selection - restart character creation
            return await CreateCharacterAsync();
        }

        _console.ShowSuccess($"Welcome, {newCharacter.Name} the {newCharacter.ClassName}!");
        await Task.Delay(500);

        // Publish character created event
        await _mediator.Publish(new CharacterCreated(newCharacter.Name));

        // Create save game with the new character and difficulty settings
        var saveGame = _saveGameService.CreateNewGame(newCharacter, difficulty);

        // Start apocalypse timer if applicable
        if (difficulty.IsApocalypse)
        {
            _apocalypseTimer.Start();

            _console.Clear();
            _console.ShowWarning("═══════════════════════════════════════");
            _console.ShowWarning("      APOCALYPSE MODE ACTIVE           ");
            _console.ShowWarning("═══════════════════════════════════════");
            _console.WriteText("  The world will end in 4 hours!");
            _console.WriteText("  Complete the main quest before time runs out.");
            _console.WriteText("  Completing quests will award bonus time.");
            _console.ShowWarning("═══════════════════════════════════════");
            await Task.Delay(2000);
        }

        Log.Information("Character created: {CharacterName} ({ClassName})", newCharacter.Name, newCharacter.ClassName);

        return (newCharacter, saveGame.Id, true);
    }

    /// <summary>
    /// Let the player select their character class.
    /// </summary>
    private async Task<CharacterClass?> SelectCharacterClassAsync()
    {
        _console.Clear();
        _console.ShowBanner("Choose Your Class", "Each class offers unique strengths");

        var classes = _classRepository.GetAllClasses();

        // Display all classes with details
        foreach (var cls in classes)
        {
            _console.WriteText("");
            _console.ShowPanel(
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
        var choice = _console.ShowMenu("Select your class:", classNames);

        if (choice == "Back to Menu")
        {
            return null;
        }

        var selected = classes.FirstOrDefault(c => c.Name == choice);

        if (selected != null)
        {
            _console.ShowSuccess($"You have chosen the path of the {selected.Name}!");
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
            _console.Clear();
            _console.ShowBanner("Attribute Allocation", $"Points Remaining: {allocation.GetRemainingPoints()}/27");

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

            _console.ShowPanel("Your Attributes", string.Join("\n", allocationLines), "green");

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

            var choice = _console.ShowMenu("Adjust attributes:", options.ToArray());

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
                _console.ShowSuccess("Attributes auto-allocated based on your class!");
                await Task.Delay(300);
            }
            else if (choice == "Reset All")
            {
                allocation = new AttributeAllocation();
                _console.ShowInfo("Attributes reset to base values.");
                await Task.Delay(200);
            }
            else if (choice == "Confirm & Continue")
            {
                if (allocation.GetRemainingPoints() > 0)
                {
                    if (_console.Confirm($"You have {allocation.GetRemainingPoints()} unspent points. Continue anyway?"))
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
        _characterView.ReviewCharacter(character, characterClass);
    }

    /// <summary>
    /// Let the player select game difficulty.
    /// </summary>
    private async Task<DifficultySettings?> SelectDifficultyAsync()
    {
        _console.Clear();
        _console.ShowBanner("Select Difficulty", "Choose your challenge level");

        var difficulties = DifficultySettings.GetAll();
        var options = difficulties.Select((d, i) =>
            $"{d.Name,-12} - {d.Description}"
        ).ToArray();

        var choiceText = _console.ShowMenu("Select difficulty level:", options);

        // Extract difficulty name from the choice (format: "Name - Description")
        var difficultyName = choiceText.Split(" - ")[0].Trim();
        var selected = difficulties.FirstOrDefault(d => d.Name == difficultyName);

        if (selected == null)
        {
            // Fallback to Normal if something goes wrong
            selected = DifficultySettings.Normal;
        }

        // Show confirmation for challenging modes
        if (selected.Name is "Ironman" or "Permadeath" or "Apocalypse")
        {
            _console.Clear();
            _console.ShowWarning($"⚠️  WARNING: {selected.Name.ToUpper()} MODE");
            System.Console.WriteLine();
            _console.WriteText("This mode features:");

            if (selected.AutoSaveOnly)
                _console.WriteText("  • Auto-save after every action - no manual saves");
            if (selected.IsPermadeath)
                _console.WriteText("  • Death PERMANENTLY deletes your save file");
            if (selected.IsApocalypse)
            {
                _console.WriteText($"  • {selected.ApocalypseTimeLimitMinutes / 60}-hour time limit to complete main quest");
                _console.WriteText("  • World ends when time runs out");
            }
            if (selected.DropAllInventoryOnDeath)
                _console.WriteText("  • Drop ALL items on death");

            System.Console.WriteLine();
            if (!_console.Confirm($"Are you absolutely sure you want {selected.Name} mode?"))
            {
                _console.ShowWarning("Returning to difficulty selection...");
                await Task.Delay(1000);
                return await SelectDifficultyAsync(); // Recursive call to let player re-select
            }
        }

        _console.ShowSuccess($"Difficulty set to: {selected.Name}");
        await Task.Delay(1000);

        return selected;
    }
}
