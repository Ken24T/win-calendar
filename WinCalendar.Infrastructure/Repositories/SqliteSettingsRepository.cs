using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteSettingsRepository(SqliteConnectionFactory connectionFactory) : ISettingsRepository
{
    public async Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT key AS [Key], value AS [Value] FROM settings ORDER BY key;";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<AppSetting>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task UpsertAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO settings (key, value, updated_utc)
            VALUES (@Key, @Value, @UpdatedUtc);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                setting.Key,
                setting.Value,
                UpdatedUtc = DateTimeOffset.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken));
    }
}
