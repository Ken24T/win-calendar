using Dapper;

namespace WinCalendar.Infrastructure.Persistence;

internal sealed class SqliteDatabaseMigrator(SqliteConnectionFactory connectionFactory) : IDatabaseMigrator
{
    private const string EnsureMigrationsTableSql =
        """
        CREATE TABLE IF NOT EXISTS schema_migrations (
            id TEXT PRIMARY KEY,
            applied_utc TEXT NOT NULL
        );
        """;

    private static readonly IReadOnlyList<(string Id, string Sql)> Migrations =
    [
        (
            "001_core_tables",
            """
            CREATE TABLE IF NOT EXISTS events (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                start_datetime TEXT NOT NULL,
                end_datetime TEXT NOT NULL,
                is_all_day INTEGER NOT NULL DEFAULT 0,
                category TEXT NOT NULL DEFAULT 'General',
                location TEXT NULL,
                notes TEXT NULL,
                recurrence_rule TEXT NULL,
                recurrence_exceptions TEXT NOT NULL DEFAULT '[]',
                created_utc TEXT NOT NULL,
                updated_utc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_events_start_datetime ON events(start_datetime);
            CREATE INDEX IF NOT EXISTS idx_events_end_datetime ON events(end_datetime);
            CREATE INDEX IF NOT EXISTS idx_events_category ON events(category);

            CREATE TABLE IF NOT EXISTS categories (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL UNIQUE,
                colour_hex TEXT NOT NULL,
                created_utc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS event_templates (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                title TEXT NOT NULL,
                default_duration_minutes INTEGER NOT NULL,
                category TEXT NOT NULL,
                notes TEXT NULL,
                created_utc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL,
                updated_utc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS custom_themes (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                definition_json TEXT NOT NULL,
                created_utc TEXT NOT NULL
            );
            """
        )
    ];

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            EnsureMigrationsTableSql,
            cancellationToken: cancellationToken));

        foreach (var migration in Migrations)
        {
            var alreadyApplied = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    "SELECT COUNT(1) FROM schema_migrations WHERE id = @id;",
                    new { id = migration.Id },
                    cancellationToken: cancellationToken));

            if (alreadyApplied > 0)
            {
                continue;
            }

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                migration.Sql,
                transaction: transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO schema_migrations (id, applied_utc) VALUES (@id, @appliedUtc);",
                new
                {
                    id = migration.Id,
                    appliedUtc = DateTimeOffset.UtcNow.ToString("O")
                },
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
    }
}