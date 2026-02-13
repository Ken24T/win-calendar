using System.Globalization;
using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteCountdownCardRepository(SqliteConnectionFactory connectionFactory) : ICountdownCardRepository
{
    public async Task<IReadOnlyList<CountdownCard>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id,
                title,
                target_datetime AS TargetDateTime,
                colour_hex AS ColourHex,
                is_active AS IsActive,
                sort_order AS SortOrder
            FROM countdown_cards
            ORDER BY sort_order, target_datetime;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<Row>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task UpsertAsync(CountdownCard countdownCard, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO countdown_cards
            (id, title, target_datetime, colour_hex, is_active, sort_order, created_utc, updated_utc)
            VALUES
            (@Id, @Title, @TargetDateTime, @ColourHex, @IsActive, @SortOrder, @CreatedUtc, @UpdatedUtc);
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                countdownCard.Id,
                countdownCard.Title,
                TargetDateTime = countdownCard.TargetDateTime.ToString("O"),
                countdownCard.ColourHex,
                IsActive = countdownCard.IsActive ? 1 : 0,
                countdownCard.SortOrder,
                CreatedUtc = now,
                UpdatedUtc = now
            },
            cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid countdownCardId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM countdown_cards WHERE id = @Id;";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = countdownCardId.ToString() },
            cancellationToken: cancellationToken));
    }

    private static CountdownCard Map(Row row)
    {
        return new CountdownCard
        {
            Id = Guid.Parse(row.Id),
            Title = row.Title,
            TargetDateTime = DateTimeOffset.Parse(row.TargetDateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            ColourHex = row.ColourHex,
            IsActive = row.IsActive == 1,
            SortOrder = (int)row.SortOrder
        };
    }

    private sealed class Row
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string TargetDateTime { get; set; } = string.Empty;

        public string ColourHex { get; set; } = "#2D6CDF";

        public long IsActive { get; set; }

        public long SortOrder { get; set; }
    }
}
