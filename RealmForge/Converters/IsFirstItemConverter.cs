using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace RealmForge.Converters;

/// <summary>
/// Converter that returns true if the item is the first item in a collection
/// </summary>
public class IsFirstItemConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] == null || values[1] == null)
            return false;

        var item = values[0];
        var collection = values[1] as IEnumerable;
        
        if (collection == null)
            return false;

        var firstItem = collection.Cast<object>().FirstOrDefault();
        return item.Equals(firstItem);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
