using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

namespace WinCalendar.Application.Contracts;

public interface IPdfExportService
{
    Task ExportEventsAsync(
        IReadOnlyList<CalendarEvent> events,
        string outputFilePath,
        string documentTitle,
        CalendarViewType viewType,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken cancellationToken = default);
}
