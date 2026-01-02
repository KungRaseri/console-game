namespace RealmEngine.Shared.Models;

/// <summary>
/// Represents a quest that can be given to the player.
/// Implements ITraitable to support quest-specific traits from templates.
/// </summary>
public class Quest : ITraitable
{
    /// <summary>
    /// Gets or sets the unique identifier for this quest.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the quest title displayed to the player.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the descriptive text explaining the quest objectives.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quest type (e.g., "kill", "fetch", "escort", "investigate", "delivery").
    /// </summary>
    public string QuestType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the difficulty rating (e.g., "easy", "medium", "hard").
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quest category ("main", "side", "legendary").
    /// </summary>
    public string Type { get; set; } = "side";

    /// <summary>
    /// Gets or sets the unique identifier of the NPC who gives this quest.
    /// </summary>
    public string QuestGiverId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of the quest giver NPC.
    /// </summary>
    public string QuestGiverName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of target for the objective (e.g., "beast", "undead", "demon").
    /// </summary>
    public string TargetType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the specific name of the target (e.g., "Ancient Red Dragon").
    /// </summary>
    public string TargetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the required quantity to complete the objective.
    /// </summary>
    public int Quantity { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the location where the quest objective must be completed.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of prerequisite quest IDs that must be completed before this quest becomes available.
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary mapping objective names to their required completion counts.
    /// </summary>
    public Dictionary<string, int> Objectives { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary tracking current progress towards each objective.
    /// </summary>
    public Dictionary<string, int> ObjectiveProgress { get; set; } = new();

    /// <summary>
    /// Gets or sets the gold reward for completing this quest.
    /// </summary>
    public int GoldReward { get; set; }
    
    /// <summary>
    /// Gets or sets the experience points reward for completing this quest.
    /// </summary>
    public int XpReward { get; set; }
    
    /// <summary>
    /// Gets or sets the bonus time (in minutes) awarded in Apocalypse mode for completing this quest.
    /// </summary>
    public int ApocalypseBonusMinutes { get; set; } = 0;
    
    /// <summary>
    /// Collection of item reward IDs given to player upon quest completion.
    /// These are resolved from @items JSON references when quest is completed.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Award items when quest completes
    /// var rewardItems = await itemRepository.GetByIdsAsync(quest.ItemRewardIds);
    /// character.Inventory.AddRange(rewardItems);
    /// ShowQuestRewards(rewardItems, quest.GoldReward, quest.XpReward);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Complete quest and give rewards
    /// for item_id in quest.ItemRewardIds:
    ///     var item = await item_service.get_by_id(item_id)
    ///     player.inventory.add_item(item)
    /// player.add_gold(quest.GoldReward)
    /// player.gain_experience(quest.XpReward)
    /// </code>
    /// <para><strong>Why IDs instead of objects?</strong></para>
    /// <list type="bullet">
    /// <item><description>Items created fresh when quest completes (not pre-instantiated)</description></item>
    /// <item><description>Save file optimization - store IDs instead of full items</description></item>
    /// <item><description>Lazy loading - only resolve when quest is completed</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@items/weapons/swords:magic-longsword", "@items/consumables/potions:health-potion"]
    /// </example>
    public List<string> ItemRewardIds { get; set; } = new();
    
    /// <summary>
    /// Collection of ability reward IDs granted to player upon quest completion.
    /// These are resolved from @abilities JSON references when quest is completed.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Grant new abilities when quest completes
    /// var rewardAbilities = await abilityRepository.GetByIdsAsync(quest.AbilityRewardIds);
    /// character.LearnedSkills.AddRange(rewardAbilities);
    /// ShowNewAbilitiesUnlocked(rewardAbilities);
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Learn new abilities from quest
    /// for ability_id in quest.AbilityRewardIds:
    ///     var ability = await ability_service.get_by_id(ability_id)
    ///     player.learn_ability(ability)
    ///     show_notification("New ability unlocked: " + ability.DisplayName)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["power-strike", "heroic-leap"]
    /// </example>
    public List<string> AbilityRewardIds { get; set; } = new();
    
    /// <summary>
    /// Collection of location IDs where quest objectives must be completed.
    /// These are resolved from @locations JSON references when displaying quest info.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Show quest objective locations on map
    /// var locations = await locationRepository.GetByIdsAsync(quest.ObjectiveLocationIds);
    /// foreach (var location in locations)
    /// {
    ///     AddQuestMarkerToMap(location.Name, location.Coordinates);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Mark quest locations on map
    /// for location_id in quest.ObjectiveLocationIds:
    ///     var location = await location_service.get_by_id(location_id)
    ///     add_quest_marker(location.name, location.coordinates)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@locations/dungeons:dark-cavern", "@locations/towns:riverside"]
    /// </example>
    public List<string> ObjectiveLocationIds { get; set; } = new();
    
    /// <summary>
    /// Collection of NPC IDs involved in quest objectives (talk to, escort, etc.).
    /// These are resolved from @npcs JSON references when quest becomes active.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Load quest NPCs for objectives
    /// var questNpcs = await npcRepository.GetByIdsAsync(quest.ObjectiveNpcIds);
    /// foreach (var npc in questNpcs)
    /// {
    ///     npc.HasQuestMarker = true;
    ///     UpdateNpcDialogue(npc, quest.Id);
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Mark quest NPCs with indicators
    /// for npc_id in quest.ObjectiveNpcIds:
    ///     var npc = await npc_service.get_by_id(npc_id)
    ///     npc.show_quest_marker = true
    ///     world.update_npc(npc)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@npcs/merchants:blacksmith-john", "@npcs/quest-givers:elder-sage"]
    /// </example>
    public List<string> ObjectiveNpcIds { get; set; } = new();
    
    /// <summary>
    /// Collection of enemy IDs that must be defeated for quest objectives.
    /// These are resolved from @enemies JSON references when tracking quest progress.
    /// </summary>
    /// <remarks>
    /// <para><strong>Resolution Pattern (C#):</strong></para>
    /// <code>
    /// // Track enemy kills for quest
    /// public void OnEnemyDefeated(Enemy defeated)
    /// {
    ///     if (quest.ObjectiveEnemyIds.Contains(defeated.Id))
    ///     {
    ///         quest.Progress++;
    ///         UpdateQuestLog(quest);
    ///     }
    /// }
    /// </code>
    /// <para><strong>Resolution Pattern (GDScript/Godot):</strong></para>
    /// <code>
    /// # Check if enemy kill counts for quest
    /// func on_enemy_defeated(enemy):
    ///     if enemy.id in quest.ObjectiveEnemyIds:
    ///         quest.progress += 1
    ///         update_quest_tracker(quest)
    /// </code>
    /// </remarks>
    /// <example>
    /// Example IDs: ["@enemies/beasts:dire-wolf", "@enemies/undead:skeleton-warrior"]
    /// </example>
    public List<string> ObjectiveEnemyIds { get; set; } = new();

    /// <summary>
    /// Gets or sets whether this quest is currently active in the player's quest log.
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Gets or sets whether this quest has been completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the current progress count towards the quest objective.
    /// </summary>
    public int Progress { get; set; } = 0;

    /// <summary>
    /// Gets or sets the time limit for this quest in hours (0 = no time limit).
    /// </summary>
    public int TimeLimit { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the timestamp when this quest was started.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the trait system dictionary for dynamic quest properties.
    /// Implements ITraitable interface.
    /// </summary>
    public Dictionary<string, TraitValue> Traits { get; set; } = new();

    /// <summary>
    /// Check if the quest has expired (time limit reached).
    /// </summary>
    public bool IsExpired()
    {
        if (TimeLimit <= 0 || !StartTime.HasValue)
            return false;

        var elapsed = DateTime.Now - StartTime.Value;
        return elapsed.TotalHours >= TimeLimit;
    }

    /// <summary>
    /// Get the formatted time remaining.
    /// </summary>
    public string GetTimeRemaining()
    {
        if (TimeLimit <= 0 || !StartTime.HasValue)
            return "No time limit";

        var elapsed = DateTime.Now - StartTime.Value;
        var remaining = TimeSpan.FromHours(TimeLimit) - elapsed;

        if (remaining.TotalHours < 0)
            return "Expired";

        if (remaining.TotalHours < 1)
            return $"{remaining.Minutes} minutes";

        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays} days, {remaining.Hours} hours";

        return $"{(int)remaining.TotalHours} hours";
    }

    /// <summary>
    /// Check if all quest objectives are complete (Phase 4 enhancement).
    /// </summary>
    public bool IsObjectivesComplete()
    {
        // If no objectives defined, fall back to legacy Progress check
        if (!Objectives.Any())
        {
            return Progress >= Quantity;
        }

        // Check all objectives are met
        foreach (var objective in Objectives)
        {
            if (!ObjectiveProgress.ContainsKey(objective.Key) ||
                ObjectiveProgress[objective.Key] < objective.Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Update progress for a specific objective (Phase 4 enhancement).
    /// </summary>
    public void UpdateObjectiveProgress(string objectiveName, int progressIncrement = 1)
    {
        if (!Objectives.ContainsKey(objectiveName))
        {
            return;
        }

        if (!ObjectiveProgress.ContainsKey(objectiveName))
        {
            ObjectiveProgress[objectiveName] = 0;
        }

        ObjectiveProgress[objectiveName] += progressIncrement;

        // Also update legacy Progress field for backwards compatibility
        Progress = ObjectiveProgress.Values.Sum();
    }
}
