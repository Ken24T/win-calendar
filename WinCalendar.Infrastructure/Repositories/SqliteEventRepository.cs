using System.Globalization;
using System.Text.Json;
using Dapper;
using WinCalendar.Application.Abstractions;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Infrastructure.Repositories;

internal sealed class SqliteEventRepository(SqliteConnectionFactory connectionFactory) : IEventRepository
{
    public async Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id,
                title,
                start_datetime AS StartDateTime,
                end_datetime AS EndDateTime,
                is_all_day AS IsAllDay,
                category,
                location,
                notes,
                recurrence_rule AS RecurrenceRule,
                recurrence_exceptions AS RecurrenceExceptions
            FROM events
            ORDER BY start_datetime;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<EventRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows
            .Select(Map)
            .ToList();
    }

    public async Task<IReadOnlyList<CalendarEvent>> GetInRangeAsync(
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id,
                title,
                start_datetime AS StartDateTime,
                end_datetime AS EndDateTime,
                is_all_day AS IsAllDay,
                category,
                location,
                notes,
                                recurrence_rule AS RecurrenceRule,
                                recurrence_exceptions AS RecurrenceExceptions
            FROM events
            WHERE start_datetime <= @RangeEnd
              AND end_datetime >= @RangeStart
            ORDER BY start_datetime;
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<EventRow>(
            new CommandDefinition(
                sql,
                new
                {
                    RangeStart = rangeStart.ToString("O"),
                    RangeEnd = rangeEnd.ToString("O")
                },
                cancellationToken: cancellationToken));

        return rows.Select(Map).ToList();
    }

    public async Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(calendarEvent, cancellationToken);
    }

    public async Task UpsertAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO events
            (
                id,
                title,
                start_datetime,
                end_datetime,
                is_all_day,
                category,
                location,
                notes,
                recurrence_rule,
                recurrence_exceptions,
                created_utc,
                updated_utc
            )
            VALUES
            (
                @Id,
                @Title,
                @StartDateTime,
                @EndDateTime,
                @IsAllDay,
                @Category,
                @Location,
                @Notes,
                @RecurrenceRule,
                @RecurrenceExceptions,
                @CreatedUtc,
                @UpdatedUtc
            );
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    calendarEvent.Id,
                    calendarEvent.Title,
                    StartDateTime = calendarEvent.StartDateTime.ToString("O"),
                    EndDateTime = calendarEvent.EndDateTime.ToString("O"),
                    IsAllDay = calendarEvent.IsAllDay ? 1 : 0,
                    calendarEvent.Category,
                    calendarEvent.Location,
                    calendarEvent.Notes,
                    calendarEvent.RecurrenceRule,
                    RecurrenceExceptions = SerializeRecurrenceExceptions(calendarEvent.RecurrenceExceptions),
                    CreatedUtc = now,
                    UpdatedUtc = now
                },
                cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM events WHERE id = @Id;";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = eventId.ToString() },
            cancellationToken: cancellationToken));
    }

    private static CalendarEvent Map(EventRow row)
    {
        return new CalendarEvent
        {
            Id = Guid.Parse(row.Id),
            Title = row.Title,
            StartDateTime = DateTimeOffset.Parse(row.StartDateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            EndDateTime = DateTimeOffset.Parse(row.EndDateTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            IsAllDay = row.IsAllDay == 1,
            Category = row.Category,
            Location = row.Location,
            Notes = row.Notes,
            RecurrenceRule = row.RecurrenceRule,
            RecurrenceExceptions = ParseRecurrenceExceptions(row.RecurrenceExceptions)
        };
    }

    private static IReadOnlyList<DateTimeOffset> ParseRecurrenceExceptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json) ?? [];
            return items
                .Select(item => DateTimeOffset.TryParse(item, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                    ? parsed
                    : (DateTimeOffset?)null)
                .Where(item => item.HasValue)
                .Select(item => item!.Value)
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private static string SerializeRecurrenceExceptions(IReadOnlyList<DateTimeOffset> recurrenceExceptions)
    {
        if (recurrenceExceptions.Count == 0)
        {
            return "[]";
        }

        var values = recurrenceExceptions
            .Select(item => item.ToString("O"))
            .ToList();

        return JsonSerializer.Serialize(values);
    }

    private sealed class EventRow
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string StartDateTime { get; set; } = string.Empty;

        public string EndDateTime { get; set; } = string.Empty;

        public long IsAllDay { get; set; }

        public string Category { get; set; } = string.Empty;

        public string? Location { get; set; }

        public string? Notes { get; set; }

        public string? RecurrenceRule { get; set; }

        public string? RecurrenceExceptions { get; set; }
    }
}