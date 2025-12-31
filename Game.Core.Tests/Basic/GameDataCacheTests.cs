using FluentAssertions;
using Game.Data.Services;

namespace Game.Core.Tests.Basic;

public class GameDataCacheTests
{
    private readonly GameDataCache _dataCache;

    public GameDataCacheTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Game.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
    }

    [Fact]
    public void Should_Load_Game_Data_Cache()
    {
        // Act
        _dataCache.LoadAllData();
        var stats = _dataCache.GetStats();
        var totalFiles = _dataCache.TotalFilesLoaded;

        // Assert
        _dataCache.Should().NotBeNull();
        totalFiles.Should().BeGreaterThan(0, "should have loaded JSON files");
        stats.Should().NotBeNull();
    }

    [Fact]
    public void Should_Load_Character_Classes_Catalog()
    {
        // Act
        var classFile = _dataCache.GetFile("classes/catalog.json");

        // Assert
        classFile.Should().NotBeNull();
        classFile!.JsonData.Should().NotBeNull();
        
        // Check structure
        var json = classFile.JsonData;
        json["metadata"].Should().NotBeNull();
        json["class_types"].Should().NotBeNull();
    }

    [Fact]
    public void Should_Load_Abilities_Catalogs()
    {
        // Act
        var activeFile = _dataCache.GetFile("abilities/active/catalog.json");
        var passiveFile = _dataCache.GetFile("abilities/passive/catalog.json");

        // Assert
        activeFile.Should().NotBeNull("active abilities catalog should exist");
        activeFile!.JsonData.Should().NotBeNull();
        
        passiveFile.Should().NotBeNull("passive abilities catalog should exist");
        passiveFile!.JsonData.Should().NotBeNull();
    }

    [Fact]
    public void Should_Load_Items_Catalogs()
    {
        // Act
        var weaponsFile = _dataCache.GetFile("items/weapons/catalog.json");
        var armorFile = _dataCache.GetFile("items/armor/catalog.json");

        // Assert
        weaponsFile.Should().NotBeNull("weapons catalog should exist");
        weaponsFile!.JsonData.Should().NotBeNull();
        
        armorFile.Should().NotBeNull("armor catalog should exist");
        armorFile!.JsonData.Should().NotBeNull();
    }

    [Fact]
    public void Should_Load_Enemies_Catalogs()
    {
        // Act - Try a few enemy categories
        var beastsFile = _dataCache.GetFile("enemies/beasts/catalog.json");
        var humanoidFile = _dataCache.GetFile("enemies/humanoid/catalog.json");

        // Assert - At least one should exist
        var hasEnemies = (beastsFile?.JsonData != null) || (humanoidFile?.JsonData != null);
        hasEnemies.Should().BeTrue("At least one enemy catalog should exist");
    }

    [Fact]
    public void Should_Handle_Missing_Files_Gracefully()
    {
        // Act
        var missingFile = _dataCache.GetFile("non-existent/file.json");

        // Assert
        missingFile.Should().BeNull("non-existent file should return null");
    }
}