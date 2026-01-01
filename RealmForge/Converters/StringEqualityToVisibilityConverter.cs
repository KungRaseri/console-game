using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RealmForge.Converters;

/// <summary>
/// Converter that compares a string value to a comparison value and returns Visibility.
/// Used to hide/show UI elements based on string matching.
/// </summary>
public class StringEqualityToVisibilityConverter : IValueConverter
{
  /// <summary>
  /// The value to compare against
  /// </summary>
  public string ComparisonValue { get; set; } = string.Empty;

  /// <summary>
  /// If true, returns Visible when strings DON'T match (inverted logic)
  /// </summary>
  public bool InvertMatch { get; set; } = false;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is not string stringValue)
      return Visibility.Visible;

    bool matches = stringValue.Equals(ComparisonValue, StringComparison.OrdinalIgnoreCase);

    // If InvertMatch is true, we want Visible when it DOESN'T match
    // If InvertMatch is false, we want Visible when it DOES match
    bool shouldBeVisible = InvertMatch ? !matches : matches;

    return shouldBeVisible ? Visibility.Visible : Visibility.Collapsed;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
