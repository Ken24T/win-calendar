using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteCategoryRepository(SqliteConnectionFactory connectionFactory) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT id, name, colour_hex AS ColourHex
            FROM categories
            ORDER BY name;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<CategoryRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task UpsertAsync(Category category, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO categories (id, name, colour_hex, created_utc)
            VALUES (@Id, @Name, @ColourHex, @CreatedUtc);
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var id = category.Id == Guid.Empty ? Guid.NewGuid() : category.Id;
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = id.ToString(),
                category.Name,
                category.ColourHex,
                CreatedUtc = DateTimeOffset.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken));
    }

    private static Category Map(CategoryRow row)
    {
        return new Category
        {
            Id = Guid.Parse(row.Id),
            Name = row.Name,
            ColourHex = row.ColourHex
        };
    }

    private sealed class CategoryRow
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ColourHex { get; set; } = string.Empty;
    }
}
