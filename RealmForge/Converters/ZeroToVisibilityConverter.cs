using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Game.ContentBuilder.Converters;

/// <summary>
/// Converts a count value to Visibility. Returns Visible when count is 0, Collapsed otherwise.
/// Used to show empty state messages when lists are empty.
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
