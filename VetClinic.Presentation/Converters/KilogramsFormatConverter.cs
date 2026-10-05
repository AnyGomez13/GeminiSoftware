using System.Globalization;
using System.Windows.Data;

namespace VetClinic.Presentation.Converters;

public class KilogramsFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return $"{d:F2} Kg";
        }

        if (value is float f)
        {
            return $"{f:F2} Kg";
        }

        if (value is decimal dec)
        {
            return $"{dec:F2} Kg";
        }

        return "0.00 Kg";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var limpio = s.Replace("Kg", "", StringComparison.OrdinalIgnoreCase).Trim();
            if (double.TryParse(limpio, NumberStyles.Any, culture, out var d))
            {
                return d;
            }
            if (double.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var dInv))
            {
                return dInv;
            }
        }

        return 0.0;
    }
}
