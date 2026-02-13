using WinCalendar.Import.Models;

namespace WinCalendar.Import.Contracts;

public interface IRustCalendarImporter
{
    Task<RustImportResult> ImportAsync(RustDbImportProfile profile, CancellationToken cancellationToken = default);
}
