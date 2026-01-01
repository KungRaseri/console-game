using Game.Data.Services;
using Game.Shared.Models;
using Newtonsoft.Json.Linq;

namespace Game.Core.Generators.Modern;

/// <summary>
/// Generates dialogue lines (greetings, farewells, responses).
/// </summary>
public class DialogueGenerator
{
    private readonly GameDataCache _dataCache;
    private readonly ReferenceResolverService _referenceResolver;
    private readonly Random _random;

    public DialogueGenerator(GameDataCache dataCache, ReferenceResolverService referenceResolver)
    {
        _dataCache = dataCache ?? throw new ArgumentNullException(nameof(dataCache));
        _referenceResolver = referenceResolver ?? throw new ArgumentNullException(nameof(referenceResolver));
        _random = new Random();
    }

    /// <summary>
    /// Generates dialogue lines from a specific type (greetings, farewells, responses).
    /// </summary>
    public Task<List<DialogueLine>> GenerateDialogueAsync(string dialogueType, string style, int count = 5)
    {
        try
        {
            var catalogPath = $"social/{dialogueType}/catalog.json";
            var catalogFile = _dataCache.GetFile(catalogPath);
            
            if (catalogFile?.JsonData == null)
            {
                return Task.FromResult(new List<DialogueLine>());
            }

            var catalog = catalogFile.JsonData;
            var items = GetItemsFromCatalog(catalog);
            
            // Filter by style if specified
            if (!string.IsNullOrEmpty(style) && style != "*")
            {
                items = items.Where(i => i["style"]?.ToString() == style);
            }
            
            if (!items.Any())
            {
                return Task.FromResult(new List<DialogueLine>());
            }

            var result = new List<DialogueLine>();

            for (int i = 0; i < count; i++)
            {
                var randomDialogue = GetRandomWeightedItem(items);
                if (randomDialogue != null)
                {
                    var dialogue = ConvertToDialogueLine(randomDialogue, dialogueType);
                    if (dialogue != null)
                    {
                        result.Add(dialogue);
                    }
                }
            }

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating dialogue: {ex.Message}");
            return Task.FromResult(new List<DialogueLine>());
        }
    }

    /// <summary>
    /// Generates random greetings for NPCs.
    /// </summary>
    public async Task<string> GenerateGreetingAsync(string style = "casual")
    {
        var greetings = await GenerateDialogueAsync("greetings", style, 1);
        return greetings.FirstOrDefault()?.Text ?? "Hello there!";
    }

    /// <summary>
    /// Generates random farewells for NPCs.
    /// </summary>
    public async Task<string> GenerateFarewellAsync(string style = "casual")
    {
        var farewells = await GenerateDialogueAsync("farewells", style, 1);
        return farewells.FirstOrDefault()?.Text ?? "Goodbye!";
    }

    /// <summary>
    /// Generates random responses for NPCs.
    /// </summary>
    public async Task<string> GenerateResponseAsync(string context, string style = "neutral")
    {
        var responses = await GenerateDialogueAsync("responses", style, 1);
        return responses.FirstOrDefault()?.Text ?? "I understand.";
    }

    /// <summary>
    /// Generates a full conversation pattern.
    /// </summary>
    public async Task<Dictionary<string, string>> GenerateConversationAsync(string style = "casual")
    {
        var greeting = await GenerateGreetingAsync(style);
        var responses = await GenerateDialogueAsync("responses", style, 3);
        var farewell = await GenerateFarewellAsync(style);

        return new Dictionary<string, string>
        {
            ["greeting"] = greeting,
            ["response1"] = responses.ElementAtOrDefault(0)?.Text ?? "Yes?",
            ["response2"] = responses.ElementAtOrDefault(1)?.Text ?? "I see.",
            ["response3"] = responses.ElementAtOrDefault(2)?.Text ?? "Indeed.",
            ["farewell"] = farewell
        };
    }

    private DialogueLine? ConvertToDialogueLine(JToken dialogueData, string dialogueType)
    {
        try
        {
            var text = dialogueData["text"]?.ToString();
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var style = dialogueData["style"]?.ToString() ?? "neutral";
            
            var dialogue = new DialogueLine
            {
                Id = $"{dialogueType}:{style}:{Guid.NewGuid().ToString().Substring(0, 8)}",
                Text = text,
                Type = dialogueType,
                Style = style,
                Context = dialogueData["context"]?.ToString()
            };

            // Parse tags
            if (dialogueData["tags"] is JArray tags)
            {
                dialogue.Tags = tags.Select(t => t.ToString()).ToList();
            }

            return dialogue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting dialogue: {ex.Message}");
            return null;
        }
    }

    private IEnumerable<JToken> GetItemsFromCatalog(JToken catalog)
    {
        if (catalog["items"] is JArray itemsArray)
        {
            return itemsArray;
        }

        if (catalog is JArray rootArray)
        {
            return rootArray;
        }

        return Enumerable.Empty<JToken>();
    }

    private JToken? GetRandomWeightedItem(IEnumerable<JToken> items)
    {
        var itemsList = items.ToList();
        if (!itemsList.Any()) return null;

        var totalWeight = itemsList.Sum(i => i["rarityWeight"]?.Value<int>() ?? 1);
        var randomValue = _random.Next(totalWeight);
        var currentWeight = 0;

        foreach (var item in itemsList)
        {
            currentWeight += item["rarityWeight"]?.Value<int>() ?? 1;
            if (randomValue < currentWeight)
            {
                return item;
            }
        }

        return itemsList.Last();
    }
}
