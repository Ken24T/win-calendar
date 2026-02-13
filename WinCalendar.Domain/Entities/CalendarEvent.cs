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
}
