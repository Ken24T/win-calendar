using WinCalendar.Application.Contracts;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Services;

internal sealed class DatabaseBackupService(SqliteConnectionFactory connectionFactory) : IBackupService
{
    private static readonly byte[] SqliteHeader = "SQLite format 3\0"u8.ToArray();

    public Task CreateBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            throw new ArgumentException("Backup file path is required.", nameof(backupFilePath));
        }

        var sourcePath = connectionFactory.DatabasePath;
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Database file not found for backup.", sourcePath);
        }

        var directory = Path.GetDirectoryName(backupFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourcePath, backupFilePath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupFilePath))
        {
            throw new ArgumentException("Backup file path is required.", nameof(backupFilePath));
        }

        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("Backup file does not exist.", backupFilePath);
        }

        ValidateSqliteHeader(backupFilePath);

        var targetPath = connectionFactory.DatabasePath;
        var targetDirectory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        File.Copy(backupFilePath, targetPath, overwrite: true);
        return Task.CompletedTask;
    }

    private static void ValidateSqliteHeader(string backupFilePath)
    {
        using var stream = File.OpenRead(backupFilePath);
        Span<byte> header = stackalloc byte[16];
        if (stream.Read(header) < header.Length)
        {
            throw new InvalidDataException("Backup file is not a valid SQLite database.");
        }

        if (!header.SequenceEqual(SqliteHeader))
        {
            throw new InvalidDataException("Backup file is not a valid SQLite database.");
        }
    }
}
