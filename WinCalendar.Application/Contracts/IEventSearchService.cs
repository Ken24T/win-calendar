using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IEventSearchService
{
    Task<IReadOnlyList<CalendarEvent>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
