namespace WinCalendar.Application.Contracts;

public interface IBackupService
{
    Task CreateBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);

    Task RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
}
