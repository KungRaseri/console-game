using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RealmForge.Converters;

/// <summary>
/// Converts null/non-null values to Visibility
/// Returns Visible if value is not null, Collapsed if null
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
