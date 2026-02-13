using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteEventTemplateRepository(SqliteConnectionFactory connectionFactory) : IEventTemplateRepository
{
    public async Task<IReadOnlyList<EventTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT id, name, title, default_duration_minutes AS DefaultDurationMinutes, category
            FROM event_templates
            ORDER BY name;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<TemplateRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task UpsertAsync(EventTemplate template, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO event_templates
            (id, name, title, default_duration_minutes, category, notes, created_utc)
            VALUES
            (@Id, @Name, @Title, @DefaultDurationMinutes, @Category, @Notes, @CreatedUtc);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var id = template.Id == Guid.Empty ? Guid.NewGuid() : template.Id;
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id.ToString(),
                template.Name,
                template.Title,
                DefaultDurationMinutes = (int)template.DefaultDuration.TotalMinutes,
                template.Category,
                Notes = string.Empty,
                CreatedUtc = DateTimeOffset.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    private static EventTemplate Map(TemplateRow row)
    {
        return new EventTemplate
        {
            Id = Guid.Parse(row.Id),
            Name = row.Name,
            Title = row.Title,
            DefaultDuration = TimeSpan.FromMinutes(row.DefaultDurationMinutes),
            Category = row.Category
        };
    }

    private sealed class TemplateRow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int DefaultDurationMinutes { get; set; }
        public string Category { get; set; } = "General";
    }
}
