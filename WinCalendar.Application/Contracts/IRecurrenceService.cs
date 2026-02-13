using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IRecurrenceService
{
    IReadOnlyList<DateTimeOffset> ExpandOccurrences(
        CalendarEvent calendarEvent,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 250);
}
