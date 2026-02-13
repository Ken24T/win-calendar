using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteThemeRepository(SqliteConnectionFactory connectionFactory) : IThemeRepository
{
    public async Task<IReadOnlyList<AppTheme>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, name, definition_json AS DefinitionJson FROM custom_themes ORDER BY name;";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<ThemeRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task UpsertAsync(AppTheme theme, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO custom_themes (id, name, definition_json, created_utc)
            VALUES (@Id, @Name, @DefinitionJson, @CreatedUtc);
            """;

        var id = theme.Id == Guid.Empty ? Guid.NewGuid() : theme.Id;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id.ToString(),
                theme.Name,
                theme.DefinitionJson,
                CreatedUtc = DateTimeOffset.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    private static AppTheme Map(ThemeRow row)
    {
        return new AppTheme
        {
            Id = Guid.Parse(row.Id),
            Name = row.Name,
            DefinitionJson = row.DefinitionJson
        };
    }

    private sealed class ThemeRow
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string DefinitionJson { get; set; } = "{}";
    }
}
