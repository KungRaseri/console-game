using Game.Models;
using Game.Shared.UI;
using Spectre.Console;

namespace Game.Services;

/// <summary>
/// Service for managing level-up attribute allocation and skill selection.
/// </summary>
public static class LevelUpService
{
    /// <summary>
    /// Process all pending level-ups for a character.
    /// </summary>
    public static async Task ProcessPendingLevelUpsAsync(Character character)
    {
        var unprocessedLevelUps = character.PendingLevelUps
            .Where(l => !l.IsProcessed)
            .OrderBy(l => l.NewLevel)
            .ToList();

        if (!unprocessedLevelUps.Any())
        {
            return;
        }

        foreach (var levelUp in unprocessedLevelUps)
        {
            await ProcessSingleLevelUpAsync(character, levelUp);
            levelUp.IsProcessed = true;
        }

        // Clean up old processed level-ups (keep last 5 for history)
        var processedCount = character.PendingLevelUps.Count(l => l.IsProcessed);
        if (processedCount > 5)
        {
            character.PendingLevelUps.RemoveAll(l => 
                l.IsProcessed && 
                l.NewLevel < character.Level - 5);
        }
    }

    /// <summary>
    /// Process a single level-up with player choices.
    /// </summary>
    private static async Task ProcessSingleLevelUpAsync(Character character, LevelUpInfo levelUp)
    {
        ConsoleUI.Clear();
        ConsoleUI.ShowBanner(
            $"🌟 LEVEL {levelUp.NewLevel} 🌟",
            $"Congratulations! You have reached level {levelUp.NewLevel}!"
        );

        // Show what was gained
        ConsoleUI.ShowPanel(
            "Level-Up Rewards",
            $"[green]+{levelUp.AttributePointsGained} Attribute Points[/]\n" +
            $"[cyan]+{levelUp.SkillPointsGained} Skill Point(s)[/]\n" +
            $"[yellow]Health & Mana fully restored![/]",
            "green"
        );

        Console.WriteLine();
        ConsoleUI.PressAnyKey("Press any key to allocate your points...");

        // Allocate attribute points
        if (character.UnspentAttributePoints > 0)
        {
            await AllocateAttributePointsAsync(character);
        }

        // Select skills (if available)
        if (character.UnspentSkillPoints > 0)
        {
            await SelectSkillsAsync(character);
        }

        ConsoleUI.Clear();
        ConsoleUI.ShowSuccess($"Level {levelUp.NewLevel} complete! You are now more powerful!");
        await Task.Delay(300);
    }

    /// <summary>
    /// Interactive attribute point allocation.
    /// </summary>
    private static async Task AllocateAttributePointsAsync(Character character)
    {
        ConsoleUI.Clear();
        
        var allocation = new AttributePointAllocation();
        bool done = false;

        while (!done)
        {
            ConsoleUI.ShowBanner(
                "Attribute Point Allocation",
                $"You have {character.UnspentAttributePoints - allocation.TotalPointsAllocated} points remaining"
            );

            // Show current attributes and preview
            DisplayAttributeAllocationPreview(character, allocation);

            Console.WriteLine();

            var choices = new List<string>
            {
                $"Strength ({character.Strength} → {character.Strength + allocation.StrengthPoints})",
                $"Dexterity ({character.Dexterity} → {character.Dexterity + allocation.DexterityPoints})",
                $"Constitution ({character.Constitution} → {character.Constitution + allocation.ConstitutionPoints})",
                $"Intelligence ({character.Intelligence} → {character.Intelligence + allocation.IntelligencePoints})",
                $"Wisdom ({character.Wisdom} → {character.Wisdom + allocation.WisdomPoints})",
                $"Charisma ({character.Charisma} → {character.Charisma + allocation.CharismaPoints})",
                "[dim]─────────────[/]",
                "[yellow]Reset Allocation[/]"
            };

            if (allocation.TotalPointsAllocated == character.UnspentAttributePoints)
            {
                choices.Add("[green bold]✓ Confirm Allocation[/]");
            }

            var choice = ConsoleUI.ShowMenu("Allocate points to:", choices.ToArray());

            if (choice.StartsWith("Strength"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.StrengthPoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.StartsWith("Dexterity"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.DexterityPoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.StartsWith("Constitution"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.ConstitutionPoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.StartsWith("Intelligence"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.IntelligencePoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.StartsWith("Wisdom"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.WisdomPoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.StartsWith("Charisma"))
            {
                if (allocation.TotalPointsAllocated < character.UnspentAttributePoints)
                {
                    allocation.CharismaPoints++;
                }
                else
                {
                    ConsoleUI.ShowWarning("No points remaining!");
                    await Task.Delay(200);
                }
            }
            else if (choice.Contains("Reset"))
            {
                allocation.Reset();
                ConsoleUI.ShowInfo("Allocation reset.");
                await Task.Delay(200);
            }
            else if (choice.Contains("Confirm"))
            {
                // Apply the allocation
                character.Strength += allocation.StrengthPoints;
                character.Dexterity += allocation.DexterityPoints;
                character.Constitution += allocation.ConstitutionPoints;
                character.Intelligence += allocation.IntelligencePoints;
                character.Wisdom += allocation.WisdomPoints;
                character.Charisma += allocation.CharismaPoints;
                
                character.UnspentAttributePoints -= allocation.TotalPointsAllocated;
                
                // Recalculate vitals with new CON/WIS
                var oldMaxHealth = character.MaxHealth;
                var oldMaxMana = character.MaxMana;
                
                character.MaxHealth = character.GetMaxHealth();
                character.MaxMana = character.GetMaxMana();
                
                // Heal for the increased amount
                character.Health += (character.MaxHealth - oldMaxHealth);
                character.Mana += (character.MaxMana - oldMaxMana);
                
                ConsoleUI.ShowSuccess("Attributes increased!");
                await Task.Delay(300);
                done = true;
            }

            ConsoleUI.Clear();
        }
    }

    /// <summary>
    /// Display attribute allocation preview.
    /// </summary>
    private static void DisplayAttributeAllocationPreview(Character character, AttributePointAllocation allocation)
    {
        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("[yellow]Attribute[/]");
        table.AddColumn("[cyan]Current[/]");
        table.AddColumn("[green]Change[/]");
        table.AddColumn("[bold]New Value[/]");
        table.AddColumn("[dim]Effect[/]");

        table.AddRow(
            "Strength (STR)",
            character.Strength.ToString(),
            allocation.StrengthPoints > 0 ? $"+{allocation.StrengthPoints}" : "-",
            (character.Strength + allocation.StrengthPoints).ToString(),
            "Melee damage"
        );

        table.AddRow(
            "Dexterity (DEX)",
            character.Dexterity.ToString(),
            allocation.DexterityPoints > 0 ? $"+{allocation.DexterityPoints}" : "-",
            (character.Dexterity + allocation.DexterityPoints).ToString(),
            "Dodge, crit chance"
        );

        table.AddRow(
            "Constitution (CON)",
            character.Constitution.ToString(),
            allocation.ConstitutionPoints > 0 ? $"+{allocation.ConstitutionPoints}" : "-",
            (character.Constitution + allocation.ConstitutionPoints).ToString(),
            $"HP, defense"
        );

        table.AddRow(
            "Intelligence (INT)",
            character.Intelligence.ToString(),
            allocation.IntelligencePoints > 0 ? $"+{allocation.IntelligencePoints}" : "-",
            (character.Intelligence + allocation.IntelligencePoints).ToString(),
            "Magic damage"
        );

        table.AddRow(
            "Wisdom (WIS)",
            character.Wisdom.ToString(),
            allocation.WisdomPoints > 0 ? $"+{allocation.WisdomPoints}" : "-",
            (character.Wisdom + allocation.WisdomPoints).ToString(),
            "Mana, magic resist"
        );

        table.AddRow(
            "Charisma (CHA)",
            character.Charisma.ToString(),
            allocation.CharismaPoints > 0 ? $"+{allocation.CharismaPoints}" : "-",
            (character.Charisma + allocation.CharismaPoints).ToString(),
            "Prices, rare loot"
        );

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Interactive skill selection.
    /// </summary>
    private static async Task SelectSkillsAsync(Character character)
    {
        ConsoleUI.Clear();

        bool done = false;
        int pointsUsed = 0;

        while (!done && pointsUsed < character.UnspentSkillPoints)
        {
            ConsoleUI.ShowBanner(
                "Skill Selection",
                $"You have {character.UnspentSkillPoints - pointsUsed} skill point(s) remaining"
            );

            // Get available skills for this character
            var availableSkills = GetAvailableSkills(character);

            if (!availableSkills.Any())
            {
                ConsoleUI.ShowWarning("No skills available at your current level.");
                await Task.Delay(300);
                break;
            }

            var skillChoices = availableSkills
                .Select(s => $"{s.Name} (Rank {s.CurrentRank}/{s.MaxRank}) - {s.Description}")
                .ToList();
            skillChoices.Add("[dim]Skip for now[/]");

            var choice = ConsoleUI.ShowMenu("Select a skill to improve:", skillChoices.ToArray());

            if (choice.Contains("Skip"))
            {
                done = true;
            }
            else
            {
                var selectedSkill = availableSkills[skillChoices.IndexOf(choice)];
                
                // Upgrade or learn the skill
                var existingSkill = character.LearnedSkills.FirstOrDefault(s => s.Name == selectedSkill.Name);
                
                if (existingSkill != null)
                {
                    existingSkill.CurrentRank++;
                    ConsoleUI.ShowSuccess($"{selectedSkill.Name} increased to rank {existingSkill.CurrentRank}!");
                }
                else
                {
                    selectedSkill.CurrentRank = 1;
                    character.LearnedSkills.Add(selectedSkill);
                    ConsoleUI.ShowSuccess($"Learned {selectedSkill.Name} (Rank 1)!");
                }

                pointsUsed++;
                await Task.Delay(300);
                ConsoleUI.Clear();
            }
        }

        character.UnspentSkillPoints -= pointsUsed;
    }

    /// <summary>
    /// Get skills available for the character based on level and class.
    /// </summary>
    private static List<Skill> GetAvailableSkills(Character character)
    {
        var allSkills = new List<Skill>
        {
            new Skill
            {
                Name = "Power Attack",
                Description = "+10% melee damage per rank",
                RequiredLevel = 2,
                MaxRank = 5,
                Type = SkillType.Combat,
                Effect = "Increases physical damage"
            },
            new Skill
            {
                Name = "Critical Strike",
                Description = "+2% critical chance per rank",
                RequiredLevel = 3,
                MaxRank = 5,
                Type = SkillType.Combat,
                Effect = "Increases critical hit chance"
            },
            new Skill
            {
                Name = "Iron Skin",
                Description = "+5% physical defense per rank",
                RequiredLevel = 2,
                MaxRank = 5,
                Type = SkillType.Defense,
                Effect = "Reduces physical damage taken"
            },
            new Skill
            {
                Name = "Arcane Knowledge",
                Description = "+10% magic damage per rank",
                RequiredLevel = 3,
                MaxRank = 5,
                Type = SkillType.Magic,
                Effect = "Increases magical damage"
            },
            new Skill
            {
                Name = "Quick Reflexes",
                Description = "+3% dodge chance per rank",
                RequiredLevel = 4,
                MaxRank = 5,
                Type = SkillType.Defense,
                Effect = "Increases dodge chance"
            },
            new Skill
            {
                Name = "Treasure Hunter",
                Description = "+10% rare item find per rank",
                RequiredLevel = 5,
                MaxRank = 3,
                Type = SkillType.Utility,
                Effect = "Increases chance to find rare items"
            },
            new Skill
            {
                Name = "Regeneration",
                Description = "+2 HP regen per turn per rank",
                RequiredLevel = 6,
                MaxRank = 3,
                Type = SkillType.Passive,
                Effect = "Slowly regenerates health"
            },
            new Skill
            {
                Name = "Mana Efficiency",
                Description = "+10% mana pool per rank",
                RequiredLevel = 4,
                MaxRank = 5,
                Type = SkillType.Magic,
                Effect = "Increases maximum mana"
            }
        };

        // Filter skills based on level requirement and current ranks
        return allSkills
            .Where(s => s.RequiredLevel <= character.Level)
            .Where(s =>
            {
                var learned = character.LearnedSkills.FirstOrDefault(ls => ls.Name == s.Name);
                return learned == null || learned.CurrentRank < s.MaxRank;
            })
            .ToList();
    }
}
