using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class EventSearchService(IEventRepository eventRepository) : IEventSearchService
{
    public async Task<IReadOnlyList<CalendarEvent>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var events = await eventRepository.GetAllAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(query))
        {
            return events;
        }

        var trimmed = query.Trim();
        return events
            .Where(item => Matches(item, trimmed))
            .OrderBy(item => item.StartDateTime)
            .ToList();
    }

    private static bool Matches(CalendarEvent calendarEvent, string query)
    {
        return Contains(calendarEvent.Title, query)
            || Contains(calendarEvent.Category, query)
            || Contains(calendarEvent.Location, query)
            || Contains(calendarEvent.Notes, query);
    }

    private static bool Contains(string? value, string query)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
