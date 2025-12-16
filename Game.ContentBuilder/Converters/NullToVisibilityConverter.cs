using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Game.ContentBuilder.Converters;

/// <summary>
/// Converts null/non-null values to Visibility
/// Returns Visible if value is null, Collapsed if not null (opposite of NotNullToVisibilityConverter)
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
