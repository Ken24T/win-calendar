using Microsoft.Data.Sqlite;

namespace WinCalendar.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory
{
    private readonly string _databasePath;

    public SqliteConnectionFactory()
    {
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