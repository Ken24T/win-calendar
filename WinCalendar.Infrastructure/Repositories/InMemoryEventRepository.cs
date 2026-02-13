using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<CalendarEvent> _events = [];

    public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CalendarEvent> snapshot = _events
            .OrderBy(x => x.StartDateTime)
            .ToList();

        return Task.FromResult(snapshot);
    }

    public Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _events.Add(calendarEvent);
        return Task.CompletedTask;
    }
}
