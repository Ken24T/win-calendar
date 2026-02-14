namespace WinCalendar.App.ViewModels.Calendar;

public sealed class DayTimeSegmentViewModel
{
    public double Top { get; init; }

    public string Label { get; init; } = string.Empty;

    public double LineHeight { get; init; }

    public double LineOpacity { get; init; }
}
