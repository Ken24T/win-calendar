namespace WinCalendar.Import.Models;

internal sealed class RustEventRow
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string StartDateTime { get; set; } = string.Empty;
    public string EndDateTime { get; set; } = string.Empty;
    public long IsAllDay { get; set; }
    public string? Category { get; set; }
    public string? RecurrenceRule { get; set; }
    public string? RecurrenceExceptions { get; set; }
}

internal sealed class RustCategoryRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#001CAD";
}

internal sealed class RustTemplateRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long DurationMinutes { get; set; }
    public string? Category { get; set; }
}

internal sealed class RustThemeRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long IsDark { get; set; }
    public string AppBackground { get; set; } = string.Empty;
    public string CalendarBackground { get; set; } = string.Empty;
    public string WeekendBackground { get; set; } = string.Empty;
    public string TodayBackground { get; set; } = string.Empty;
    public string TodayBorder { get; set; } = string.Empty;
    public string DayBackground { get; set; } = string.Empty;
    public string DayBorder { get; set; } = string.Empty;
    public string TextPrimary { get; set; } = string.Empty;
    public string TextSecondary { get; set; } = string.Empty;
    public string? HeaderBackground { get; set; }
    public string? HeaderText { get; set; }
}
