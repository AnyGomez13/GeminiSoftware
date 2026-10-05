using System.Globalization;
using System.Windows.Data;

namespace VetClinic.Presentation.Converters;

public class DateFormatConverter : IValueConverter
{
    public string DefaultFormat { get; set; } = "dd/MM/yyyy";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
        {
            var format = parameter is string p && !string.IsNullOrWhiteSpace(p) ? p : DefaultFormat;
            return dt.ToString(format, CultureInfo.CurrentCulture);
        }

        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && DateTime.TryParse(s, culture, DateTimeStyles.None, out var dt))
        {
            return dt;
        }

        return Binding.DoNothing;
    }
}
