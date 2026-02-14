using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IPdfExportService
{
    Task ExportEventsAsync(
        IReadOnlyList<CalendarEvent> events,
        string outputFilePath,
        string documentTitle,
        CancellationToken cancellationToken = default);
}
