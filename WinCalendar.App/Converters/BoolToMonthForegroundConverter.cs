using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WinCalendar.App.Converters;

public sealed class BoolToMonthForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var currentMonthBrush = System.Windows.Application.Current.TryFindResource("AppPrimaryTextBrush") as Brush ?? Brushes.Black;
        var otherMonthBrush = System.Windows.Application.Current.TryFindResource("AppSecondaryTextBrush") as Brush ?? Brushes.Gray;

        if (value is bool isCurrentMonth)
        {
            return isCurrentMonth ? currentMonthBrush : otherMonthBrush;
        }

        return otherMonthBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
