using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IEventService
{
    Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(CancellationToken cancellationToken = default);

    Task SeedSampleEventsAsync(CancellationToken cancellationToken = default);
}
