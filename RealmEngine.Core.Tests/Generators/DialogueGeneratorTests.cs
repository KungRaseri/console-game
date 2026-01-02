using FluentAssertions;
using RealmEngine.Core.Generators.Modern;
using RealmEngine.Data.Services;
using Xunit;

namespace RealmEngine.Core.Tests.Generators;

[Trait("Category", "Generator")]
public class DialogueGeneratorTests
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly DialogueGenerator _generator;

    public DialogueGeneratorTests()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataCache = new GameDataCache(basePath);
        _referenceResolver = new ReferenceResolverService(_dataCache);
        _generator = new DialogueGenerator(_dataCache, _referenceResolver);
    }

    [Fact]
    public async Task Should_Generate_Greetings()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var greetings = await _generator.GenerateDialogueAsync("greetings", "*", 5);

        // Assert
        greetings.Should().NotBeNull();
        greetings.Should().HaveCount(5);
        greetings.Should().AllSatisfy(greeting =>
        {
            greeting.Text.Should().NotBeNullOrEmpty();
            greeting.Type.Should().Be("greetings");
        });
    }

    [Fact]
    public async Task Should_Generate_Farewells()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var farewells = await _generator.GenerateDialogueAsync("farewells", "*", 5);

        // Assert
        farewells.Should().NotBeNull();
        farewells.Should().HaveCount(5);
        farewells.Should().AllSatisfy(farewell =>
        {
            farewell.Text.Should().NotBeNullOrEmpty();
            farewell.Type.Should().Be("farewells");
        });
    }

    [Fact]
    public async Task Should_Generate_Responses()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var responses = await _generator.GenerateDialogueAsync("responses", "*", 5);

        // Assert
        responses.Should().NotBeNull();
        responses.Should().HaveCount(5);
        responses.Should().AllSatisfy(response =>
        {
            response.Text.Should().NotBeNullOrEmpty();
            response.Type.Should().Be("responses");
        });
    }

    [Theory]
    [InlineData("greetings")]
    [InlineData("farewells")]
    [InlineData("responses")]
    public async Task Should_Generate_Dialogue_From_Different_Types(string dialogueType)
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var dialogues = await _generator.GenerateDialogueAsync(dialogueType, "*", 3);

        // Assert
        dialogues.Should().NotBeNull();
        dialogues.Should().HaveCountGreaterThan(0);
        dialogues.Should().AllSatisfy(dialogue =>
        {
            dialogue.Type.Should().Be(dialogueType);
        });
    }

    [Fact]
    public async Task Should_Generate_Greeting_Helper_Method()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var greeting = await _generator.GenerateGreetingAsync();

        // Assert
        greeting.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Generate_Farewell_Helper_Method()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var farewell = await _generator.GenerateFarewellAsync();

        // Assert
        farewell.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Generate_Response_Helper_Method()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var response = await _generator.GenerateResponseAsync("test context");

        // Assert
        response.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Generate_Full_Conversation()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var conversation = await _generator.GenerateConversationAsync();

        // Assert
        conversation.Should().NotBeNull();
        conversation.Should().ContainKey("greeting");
        conversation.Should().ContainKey("response1");
        conversation.Should().ContainKey("response2");
        conversation.Should().ContainKey("response3");
        conversation.Should().ContainKey("farewell");
        
        conversation.Values.Should().AllSatisfy(value =>
        {
            value.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Return_Empty_List_For_Invalid_Dialogue_Type()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var dialogues = await _generator.GenerateDialogueAsync("invalid_type", "*", 5);

        // Assert
        dialogues.Should().NotBeNull();
        dialogues.Should().BeEmpty();
    }

    [Fact]
    public async Task Generated_Dialogues_Should_Have_Required_Properties()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var dialogues = await _generator.GenerateDialogueAsync("greetings", "*", 3);

        // Assert
        dialogues.Should().AllSatisfy(dialogue =>
        {
            dialogue.Id.Should().NotBeNullOrEmpty();
            dialogue.Text.Should().NotBeNullOrEmpty();
            dialogue.Type.Should().Be("greetings");
            dialogue.Style.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task Should_Handle_Hierarchical_Catalog_Structure()
    {
        // Arrange - Tests that DialogueGenerator can parse nested structures like:
        // greeting_types -> religious -> items (with templates array)
        _dataCache.LoadAllData();

        // Act
        var greetings = await _generator.GenerateDialogueAsync("greetings", "*", 10);

        // Assert - Should successfully extract items from nested structure
        greetings.Should().NotBeNull();
        greetings.Should().HaveCount(10, "Generator should handle hierarchical catalog structure");
    }

    [Fact]
    public async Task Should_Extract_Text_From_Templates_Array()
    {
        // Arrange - Tests that DialogueGenerator picks random text from templates array
        _dataCache.LoadAllData();

        // Act
        var greetings = await _generator.GenerateDialogueAsync("greetings", "*", 5);

        // Assert - Each greeting should have text extracted from templates
        greetings.Should().AllSatisfy(greeting =>
        {
            greeting.Text.Should().NotBeNullOrEmpty("Templates array should be processed");
            greeting.Text.Should().NotContain("[", "Text should be actual string, not array");
        });
    }

    [Fact]
    public async Task Should_Use_Weighted_Random_Selection()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act - Generate many dialogues to check variety
        var dialogues = await _generator.GenerateDialogueAsync("greetings", "*", 50);

        // Assert - Should have variety due to weighted random selection
        var uniqueTexts = dialogues.Select(d => d.Text).Distinct().ToList();
        uniqueTexts.Should().HaveCountGreaterThan(5, "Weighted random selection should produce variety");
    }

    [Fact]
    public async Task Should_Support_Template_Variables()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var greetings = await _generator.GenerateDialogueAsync("greetings", "*", 10);

        // Assert - Some greetings may contain template variables like {player_name}, {time_of_day}
        // This test just ensures generation works - actual variable substitution is done by consumer
        greetings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Conversation_Should_Have_Natural_Flow()
    {
        // Arrange
        _dataCache.LoadAllData();

        // Act
        var conversation = await _generator.GenerateConversationAsync("casual");

        // Assert
        conversation["greeting"].Should().NotBeNullOrEmpty();
        conversation["farewell"].Should().NotBeNullOrEmpty();
        conversation["response1"].Should().NotBeNullOrEmpty();
    }
}
