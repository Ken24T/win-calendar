using System.Text;
using System.IO;

namespace WinCalendar.App;

internal static class StartupDiagnostics
{
    private static readonly object Sync = new();

    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinCalendar",
        "logs");

    private static readonly string LogFilePath = Path.Combine(LogDirectory, "startup.log");

    public static string LogPath => LogFilePath;

    public static void WriteInfo(string message)
    {
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
            Directory.CreateDirectory(LogDirectory);

            var entry = $"[{DateTimeOffset.Now:O}] [{level}] {message}{Environment.NewLine}";
            lock (Sync)
            {
                File.AppendAllText(LogFilePath, entry);
            }
        }
        catch
        {
            // Avoid throwing from diagnostics.
        }
    }
}
