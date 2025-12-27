using System;
using System.Globalization;
using System.Windows.Data;
using Game.ContentBuilder.Services;

namespace Game.ContentBuilder.Converters;

/// <summary>
/// Converts a numeric rarity weight to a text rarity name based on thresholds from rarity_config.json
/// </summary>
public class RarityWeightToNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return "Unknown";
        
        if (!int.TryParse(value.ToString(), out int weight))
            return "Unknown";

        return RarityConfigService.Instance.GetRarityName(weight);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
