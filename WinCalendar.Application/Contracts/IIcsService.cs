namespace WinCalendar.Application.Contracts;

public interface IIcsService
{
    Task<int> ImportAsync(string filePath, CancellationToken cancellationToken = default);

    Task ExportAsync(string filePath, CancellationToken cancellationToken = default);
}
