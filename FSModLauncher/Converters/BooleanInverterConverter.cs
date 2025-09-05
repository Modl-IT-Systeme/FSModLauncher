using System.Globalization;
using System.Windows.Data;

namespace FSModLauncher;

public class BooleanInverterConverter : IValueConverter
{
    public static readonly BooleanInverterConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}