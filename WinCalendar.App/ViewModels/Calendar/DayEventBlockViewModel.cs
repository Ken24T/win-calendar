using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Calendar;

public sealed class DayEventBlockViewModel
{
    public CalendarEvent SourceEvent { get; init; } = new();

    public string Title { get; init; } = string.Empty;

    public string TimeRangeLabel { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public double Top { get; init; }

    public double Height { get; init; }

    public double HeaderHeight { get; init; }

    public double BodyHeight { get; init; }

    public bool HasBody => BodyHeight > 0;

    public IReadOnlyList<double> SegmentSeparatorOffsets { get; init; } = [];
}
