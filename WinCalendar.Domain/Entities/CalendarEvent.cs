namespace WinCalendar.Domain.Entities;

public sealed class CalendarEvent
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTimeOffset StartDateTime { get; init; }

    public DateTimeOffset EndDateTime { get; init; }

    public bool IsAllDay { get; init; }

    public string Category { get; init; } = "General";

    public string? Location { get; init; }

    public string? Notes { get; init; }

    public string? RecurrenceRule { get; init; }

    public IReadOnlyList<DateTimeOffset> RecurrenceExceptions { get; init; } = [];

    public bool IsMultiDay => EndDateTime.Date > StartDateTime.Date;

    public string TimeRangeLabel => IsAllDay
        ? "All day"
        : IsMultiDay
            ? $"{StartDateTime:dd MMM HH:mm} - {EndDateTime:dd MMM HH:mm}"
            : $"{StartDateTime:HH:mm} - {EndDateTime:HH:mm}";

    public string StartDisplayLabel => IsAllDay
        ? StartDateTime.ToString("ddd dd MMM yyyy")
        : StartDateTime.ToString("ddd dd MMM yyyy HH:mm");

    public string EndDisplayLabel => IsAllDay
        ? EndDateTime.ToString("ddd dd MMM yyyy")
        : EndDateTime.ToString("ddd dd MMM yyyy HH:mm");

    public string MonthCellLabel => IsAllDay
        ? $"All day: {Title}"
        : IsMultiDay
            ? $"Multi-day: {Title}"
            : Title;
}
