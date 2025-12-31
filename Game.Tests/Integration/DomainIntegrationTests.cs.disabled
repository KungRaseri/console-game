using FluentAssertions;

namespace Game.Tests.Integration;

/// <summary>
/// Integration tests for cross-domain data generation
/// Tests real-world scenarios using existing generators
/// </summary>
public class DomainIntegrationTests
{
    #region NPC Generation Tests

    [Fact]
    public void Should_Generate_NPC_With_Valid_Data()
    {
        // Act
        var npc = NpcGenerator.Generate();

        // Assert
        npc.Should().NotBeNull();
        npc.Id.Should().NotBeNullOrEmpty();
        npc.Name.Should().NotBeNullOrEmpty();
        npc.Occupation.Should().NotBeNullOrEmpty();
        npc.Age.Should().BeInRange(18, 100);
        npc.Gold.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Should_Generate_Multiple_Unique_NPCs()
    {
        // Act
        var npcs = NpcGenerator.Generate(20);

        // Assert
        npcs.Should().HaveCount(20);
        npcs.Should().OnlyHaveUniqueItems(n => n.Id, "All NPCs should have unique IDs");
        
        // Should have diversity in occupations
        var occupations = npcs.Select(n => n.Occupation).Distinct().ToList();
        occupations.Should().HaveCountGreaterThan(5, "Should generate diverse occupations");
    }

    [Fact]
    public void Generated_NPCs_Should_Have_Realistic_Ages()
    {
        // Act
        var npcs = NpcGenerator.Generate(50);

        // Assert
        var ages = npcs.Select(n => n.Age).ToList();
        
        // Should have age distribution
        ages.Should().Contain(a => a < 30, "Should have young NPCs");
        ages.Should().Contain(a => a >= 30 && a < 60, "Should have middle-aged NPCs");
        ages.Should().Contain(a => a >= 60, "Should have elder NPCs");
    }

    #endregion

    #region Item Generation Tests

    [Fact]
    public void Should_Generate_Item_With_Valid_Properties()
    {
        // Act
        var item = ItemGenerator.Generate();

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBeNullOrEmpty();
        item.Name.Should().NotBeNullOrEmpty();
        item.ItemType.Should().NotBeNullOrEmpty();
        item.Value.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Should_Generate_Items_With_Rarity_Distribution()
    {
        // Act
        var items = ItemGenerator.Generate(100);

        // Assert
        items.Should().HaveCount(100);
        
        var rarityGroups = items.GroupBy(i => i.Rarity).ToList();
        rarityGroups.Should().NotBeEmpty("Should have items with rarity values");
        
        // Common items should be more frequent than rare
        var commonCount = items.Count(i => i.Rarity == "Common");
        var rareCount = items.Count(i => i.Rarity == "Rare" || i.Rarity == "Epic");
        
        if (commonCount > 0 && rareCount > 0)
        {
            commonCount.Should().BeGreaterThan(rareCount, "Common items should be more frequent");
        }
    }

    [Fact]
    public void Should_Generate_Different_Item_Types()
    {
        // Act
        var items = ItemGenerator.Generate(50);

        // Assert
        var itemTypes = items.Select(i => i.ItemType).Distinct().ToList();
        itemTypes.Should().HaveCountGreaterThan(3, "Should generate multiple item types");
    }

    #endregion

    #region Quest Generation Tests

    [Fact]
    public void Should_Generate_Quest_With_Valid_Structure()
    {
        // Act
        var quest = QuestGenerator.Generate();

        // Assert
        quest.Should().NotBeNull();
        quest.Id.Should().NotBeNullOrEmpty();
        quest.Title.Should().NotBeNullOrEmpty();
        quest.Description.Should().NotBeNullOrEmpty();
        quest.RewardGold.Should().BeGreaterThanOrEqualTo(0);
        quest.RewardExperience.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Should_Generate_Quests_With_Different_Types()
    {
        // Act
        var quests = QuestGenerator.Generate(20);

        // Assert
        quests.Should().HaveCount(20);
        
        var questTypes = quests.Select(q => q.Type).Distinct().ToList();
        questTypes.Should().HaveCountGreaterThan(2, "Should generate different quest types");
    }

    [Fact]
    public void Quest_Rewards_Should_Scale_With_Difficulty()
    {
        // Act
        var quests = QuestGenerator.Generate(30);

        // Assert
        var easyQuests = quests.Where(q => q.Difficulty == "Easy").ToList();
        var hardQuests = quests.Where(q => q.Difficulty == "Hard").ToList();
        
        if (easyQuests.Any() && hardQuests.Any())
        {
            var avgEasyReward = easyQuests.Average(q => q.RewardGold);
            var avgHardReward = hardQuests.Average(q => q.RewardGold);
            
            avgHardReward.Should().BeGreaterThan(avgEasyReward, 
                "Hard quests should reward more gold than easy quests");
        }
    }

    #endregion

    #region Enemy Generation Tests

    [Fact]
    public void Should_Generate_Enemy_With_Valid_Stats()
    {
        // Act
        var enemy = EnemyGenerator.Generate();

        // Assert
        enemy.Should().NotBeNull();
        enemy.Id.Should().NotBeNullOrEmpty();
        enemy.Name.Should().NotBeNullOrEmpty();
        enemy.Health.Should().BeGreaterThan(0);
        enemy.Level.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Should_Generate_Enemies_With_Different_Types()
    {
        // Act
        var enemies = EnemyGenerator.Generate(25);

        // Assert
        enemies.Should().HaveCount(25);
        
        var enemyTypes = enemies.Select(e => e.EnemyType).Distinct().ToList();
        enemyTypes.Should().HaveCountGreaterThan(3, "Should generate different enemy types");
    }

    [Fact]
    public void Enemy_Stats_Should_Scale_With_Level()
    {
        // Act
        var enemies = EnemyGenerator.Generate(50);

        // Assert
        var lowLevelEnemies = enemies.Where(e => e.Level <= 3).ToList();
        var highLevelEnemies = enemies.Where(e => e.Level >= 8).ToList();
        
        if (lowLevelEnemies.Any() && highLevelEnemies.Any())
        {
            var avgLowHealth = lowLevelEnemies.Average(e => e.Health);
            var avgHighHealth = highLevelEnemies.Average(e => e.Health);
            
            avgHighHealth.Should().BeGreaterThan(avgLowHealth,
                "Higher level enemies should have more health");
        }
    }

    #endregion

    #region Cross-Generator Integration Tests

    [Fact]
    public void Should_Generate_Complete_Game_Session_Data()
    {
        // Act - Generate a complete game world
        var npcs = NpcGenerator.Generate(10);
        var items = ItemGenerator.Generate(20);
        var quests = QuestGenerator.Generate(5);
        var enemies = EnemyGenerator.Generate(15);

        // Assert
        npcs.Should().HaveCount(10).And.OnlyHaveUniqueItems(n => n.Id);
        items.Should().HaveCount(20).And.OnlyHaveUniqueItems(i => i.Id);
        quests.Should().HaveCount(5).And.OnlyHaveUniqueItems(q => q.Id);
        enemies.Should().HaveCount(15).And.OnlyHaveUniqueItems(e => e.Id);
        
        // All IDs should be globally unique
        var allIds = npcs.Select(n => n.Id)
            .Concat(items.Select(i => i.Id))
            .Concat(quests.Select(q => q.Id))
            .Concat(enemies.Select(e => e.Id))
            .ToList();
        
        allIds.Should().OnlyHaveUniqueItems("All generated IDs should be unique across all generators");
    }

    [Fact]
    public void Generators_Should_Produce_Consistent_Results()
    {
        // Act - Generate multiple times
        var npcs1 = NpcGenerator.Generate(10);
        var npcs2 = NpcGenerator.Generate(10);

        // Assert - Different results but same structure
        npcs1.Should().HaveCount(10);
        npcs2.Should().HaveCount(10);
        
        // Should have different IDs (not deterministic)
        var ids1 = npcs1.Select(n => n.Id).ToList();
        var ids2 = npcs2.Select(n => n.Id).ToList();
        ids1.Should().NotBeEquivalentTo(ids2, "Multiple generations should produce different IDs");
    }

    [Fact]
    public void Generators_Should_Handle_Large_Batch_Generation()
    {
        // Act - Generate large batches
        var npcs = NpcGenerator.Generate(100);
        var items = ItemGenerator.Generate(200);
        var quests = QuestGenerator.Generate(50);
        var enemies = EnemyGenerator.Generate(150);

        // Assert
        npcs.Should().HaveCount(100);
        items.Should().HaveCount(200);
        quests.Should().HaveCount(50);
        enemies.Should().HaveCount(150);
        
        // All should have valid, unique data
        npcs.Should().OnlyHaveUniqueItems(n => n.Id);
        npcs.Should().OnlyContain(n => !string.IsNullOrEmpty(n.Name));
    }

    #endregion
}
