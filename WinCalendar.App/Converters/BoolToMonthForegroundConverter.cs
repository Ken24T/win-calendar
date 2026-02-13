using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WinCalendar.App.Converters;

public sealed class BoolToMonthForegroundConverter : IValueConverter
{
    private static readonly Brush CurrentMonthBrush = Brushes.Black;
    private static readonly Brush OtherMonthBrush = Brushes.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCurrentMonth)
        {
            return isCurrentMonth ? CurrentMonthBrush : OtherMonthBrush;
        }

        return OtherMonthBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
