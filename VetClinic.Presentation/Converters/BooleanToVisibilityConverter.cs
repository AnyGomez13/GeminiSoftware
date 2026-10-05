using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VetClinic.Presentation.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;

        if (parameter is string paramStr && paramStr.Equals("Invert", StringComparison.OrdinalIgnoreCase))
        {
            boolValue = !boolValue;
        }
        else if (IsInverted)
        {
            boolValue = !boolValue;
        }

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isVisible = value is Visibility v && v == Visibility.Visible;
        return IsInverted ? !isVisible : isVisible;
    }
}
