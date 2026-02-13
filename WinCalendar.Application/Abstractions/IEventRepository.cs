using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface IEventRepository
{
    Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default);
}
