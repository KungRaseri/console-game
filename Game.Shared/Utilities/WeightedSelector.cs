using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Shared.Utilities;

/// <summary>
/// Utility for selecting items based on rarity weights.
/// Uses the formula: probability = 100 / rarityWeight
/// </summary>
public static class WeightedSelector
{
    private static readonly Random _random = Random.Shared;

    /// <summary>
    /// Select a random item from a collection based on rarity weights.
    /// Higher rarity weights = lower selection probability.
    /// Formula: probability = 100 / rarityWeight
    /// </summary>
    /// <typeparam name="T">Type that has a RarityWeight property</typeparam>
    /// <param name="items">Collection of items to select from</param>
    /// <returns>Selected item</returns>
    /// <exception cref="ArgumentException">If collection is empty</exception>
    public static T SelectByRarityWeight<T>(IEnumerable<T> items) where T : class
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            throw new ArgumentException("Cannot select from empty collection", nameof(items));
        }

        if (itemList.Count == 1)
        {
            return itemList[0];
        }

        // Get RarityWeight property using reflection
        var rarityWeightProperty = typeof(T).GetProperty("RarityWeight");
        if (rarityWeightProperty == null)
        {
            throw new ArgumentException($"Type {typeof(T).Name} does not have a RarityWeight property");
        }

        // Calculate total probability weight (sum of 100/rarityWeight for all items)
        double totalWeight = 0;
        foreach (var item in itemList)
        {
            var rarityWeight = (int)(rarityWeightProperty.GetValue(item) ?? 1);
            totalWeight += 100.0 / rarityWeight;
        }

        // Generate random value
        var randomValue = _random.NextDouble() * totalWeight;

        // Select item based on cumulative weight
        double cumulativeWeight = 0;
        foreach (var item in itemList)
        {
            var rarityWeight = (int)(rarityWeightProperty.GetValue(item) ?? 1);
            cumulativeWeight += 100.0 / rarityWeight;

            if (randomValue <= cumulativeWeight)
            {
                return item;
            }
        }

        // Fallback to last item (should never happen)
        return itemList[^1];
    }

    /// <summary>
    /// Calculate the selection probability for an item with given rarity weight.
    /// Formula: probability = 100 / rarityWeight
    /// </summary>
    /// <param name="rarityWeight">The rarity weight of the item</param>
    /// <returns>Probability as a percentage (0-100)</returns>
    public static double CalculateProbability(int rarityWeight)
    {
        return 100.0 / rarityWeight;
    }

    /// <summary>
    /// Get selection probabilities for all items in a collection.
    /// Useful for testing and debugging weighted selection.
    /// </summary>
    public static Dictionary<string, double> GetProbabilities<T>(IEnumerable<T> items) where T : class
    {
        var itemList = items.ToList();
        var result = new Dictionary<string, double>();

        if (itemList.Count == 0)
        {
            return result;
        }

        // Get properties
        var rarityWeightProperty = typeof(T).GetProperty("RarityWeight");
        var nameProperty = typeof(T).GetProperty("Name");

        if (rarityWeightProperty == null || nameProperty == null)
        {
            return result;
        }

        // Calculate total weight
        double totalWeight = 0;
        foreach (var item in itemList)
        {
            var rarityWeight = (int)(rarityWeightProperty.GetValue(item) ?? 1);
            totalWeight += 100.0 / rarityWeight;
        }

        // Calculate probability for each item
        foreach (var item in itemList)
        {
            var name = (string)(nameProperty.GetValue(item) ?? "Unknown");
            var rarityWeight = (int)(rarityWeightProperty.GetValue(item) ?? 1);
            var itemWeight = 100.0 / rarityWeight;
            var probability = (itemWeight / totalWeight) * 100.0;

            result[name] = probability;
        }

        return result;
    }
}
