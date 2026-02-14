using System.IO;
using System.Text;

namespace WinCalendar.App;

internal static class StartupDiagnostics
{
    private static readonly object Sync = new();
    private const long MaxLogFileBytes = 1024 * 1024;
    private const int MaxArchivedLogs = 5;
    private const int MaxLogAgeDays = 30;

    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinCalendar",
        "logs");

    private static readonly string LogFilePath = Path.Combine(LogDirectory, "startup.log");
    private static bool _isInfoLoggingEnabled;
    private static bool _isInitialised;

    public static string LogPath => LogFilePath;

    public static void Initialise(bool enableInfoLogging)
    {
        try
        {
            lock (Sync)
            {
                _isInfoLoggingEnabled = enableInfoLogging;
                Directory.CreateDirectory(LogDirectory);
                RotateIfNeeded();
                PruneOldLogs();
                _isInitialised = true;
            }
        }
        catch
        {
            // Avoid throwing from diagnostics initialisation.
        }
    }

    public static void WriteInfo(string message)
    {
        if (!_isInfoLoggingEnabled)
        {
            return;
        }

        Write("INFO", message);
    }

    public static void WriteError(string context, Exception exception)
    {
        var builder = new StringBuilder();
        builder.AppendLine(context);
        builder.AppendLine(exception.ToString());
        Write("ERROR", builder.ToString());
    }

    private static void Write(string level, string message)
    {
        try
        {
            if (!_isInitialised)
            {
                Initialise(enableInfoLogging: false);
            }

            lock (Sync)
            {
                RotateIfNeeded();
                var entry = $"[{DateTimeOffset.Now:O}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, entry);
            }
        }
        catch
        {
            // Avoid throwing from diagnostics.
        }
    }

    private static void RotateIfNeeded()
    {
        try
        {
            if (!File.Exists(LogFilePath))
            {
                return;
            }

            var info = new FileInfo(LogFilePath);
            if (info.Length < MaxLogFileBytes)
            {
                return;
            }

            var archiveFilePath = Path.Combine(
                LogDirectory,
                $"startup-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.log");

            if (File.Exists(archiveFilePath))
            {
                archiveFilePath = Path.Combine(
                    LogDirectory,
                    $"startup-{DateTimeOffset.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.log");
            }

            File.Move(LogFilePath, archiveFilePath);
        }
        catch
        {
            // Avoid throwing from diagnostics.
        }
    }

    private static void PruneOldLogs()
    {
        try
        {
            var retentionCutoff = DateTimeOffset.Now.AddDays(-MaxLogAgeDays);

            var archiveFiles = Directory
                .GetFiles(LogDirectory, "startup-*.log")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            var filesToDeleteByCount = archiveFiles.Skip(MaxArchivedLogs).ToList();
            foreach (var file in filesToDeleteByCount)
            {
                file.Delete();
            }

            foreach (var file in archiveFiles.Take(MaxArchivedLogs))
            {
                var lastWrite = new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero);

                if (lastWrite < retentionCutoff)
                {
                    file.Delete();
                }
            }
        }
        catch
        {
            // Avoid throwing from diagnostics.
        }
    }
}
