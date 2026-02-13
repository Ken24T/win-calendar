using Microsoft.Data.Sqlite;

namespace WinCalendar.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory
{
    private readonly string _databasePath;

    public SqliteConnectionFactory(string? databasePath = null)
    {
        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _databasePath = databasePath;
            return;
        }

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = Path.Combine(appDataPath, "WinCalendar");
        Directory.CreateDirectory(dataDirectory);

        _databasePath = Path.Combine(dataDirectory, "wincalendar.db");
    }

    public SqliteConnection CreateConnection()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        return new SqliteConnection(connectionStringBuilder.ConnectionString);
    }
}