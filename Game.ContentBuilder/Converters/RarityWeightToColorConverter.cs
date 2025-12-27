using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Game.ContentBuilder.Services;

namespace Game.ContentBuilder.Converters;

/// <summary>
/// Converts a numeric rarity weight to a color brush based on thresholds from rarity_config.json
/// </summary>
public class RarityWeightToColorConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value == null) return new SolidColorBrush(Colors.Gray);

    if (!int.TryParse(value.ToString(), out int weight))
      return new SolidColorBrush(Colors.Gray);

    var color = RarityConfigService.Instance.GetRarityColor(weight);
    return new SolidColorBrush(color);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
