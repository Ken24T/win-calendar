using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface IEventRepository
{
    Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalendarEvent>> GetInRangeAsync(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken cancellationToken = default);

    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);

    Task UpsertAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default);
}
