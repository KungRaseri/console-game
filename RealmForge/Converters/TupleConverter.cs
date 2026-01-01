using System.Globalization;
using System.Windows.Data;

namespace RealmForge.Converters;

/// <summary>
/// Converts multiple values into a Tuple for command parameters.
/// Used to pass both a string parameter and the current DataContext pattern to commands.
/// </summary>
public class TupleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is string str && values[1] != null)
        {
            return Tuple.Create(str, values[1]);
        }
        return null!;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
