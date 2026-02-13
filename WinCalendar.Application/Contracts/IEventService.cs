using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IEventService
{
    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarEvent>> GetEventsInRangeAsync(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken cancellationToken = default);

    Task SaveEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);

    Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task SeedSampleEventsAsync(CancellationToken cancellationToken = default);
}
