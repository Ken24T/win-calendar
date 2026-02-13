using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using WinCalendar.Import.Models;

namespace WinCalendar.Import.Services;

internal sealed class TargetDataWriter
{
    public async Task<int> WriteEventsAsync(SqliteConnection targetConnection, IEnumerable<RustEventRow> rows, ImportWarnings warnings, CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO events
            (id, title, start_datetime, end_datetime, is_all_day, category, location, notes,
             recurrence_rule, recurrence_exceptions, created_utc, updated_utc)
            VALUES
            (@Id, @Title, @StartDateTime, @EndDateTime, @IsAllDay, @Category, @Location, @Notes,
             @RecurrenceRule, @RecurrenceExceptions, @CreatedUtc, @UpdatedUtc);
            """;

        var imported = 0;
        foreach (var row in rows)
        {
            if (!TryParseDate(row.StartDateTime, out var start) || !TryParseDate(row.EndDateTime, out var end))
            {
                warnings.Add($"Skipped event id={row.Id} due to invalid datetime format.");
                continue;
            }

            var now = DateTimeOffset.UtcNow.ToString("O");
            await targetConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    Id = CreateStableGuid("event", row.Id),
                    row.Title,
                    StartDateTime = start,
                    EndDateTime = end,
                    IsAllDay = row.IsAllDay > 0 ? 1 : 0,
                    Category = string.IsNullOrWhiteSpace(row.Category) ? "General" : row.Category,
                    row.Location,
                    Notes = row.Description,
                    row.RecurrenceRule,
                    RecurrenceExceptions = string.IsNullOrWhiteSpace(row.RecurrenceExceptions) ? "[]" : row.RecurrenceExceptions,
                    CreatedUtc = now,
                    UpdatedUtc = now
                },
                cancellationToken: cancellationToken));

            imported++;
        }

        return imported;
    }

    public async Task<int> WriteCategoriesAsync(SqliteConnection targetConnection, IEnumerable<RustCategoryRow> rows, CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO categories (id, name, colour_hex, created_utc)
            VALUES (@Id, @Name, @ColourHex, @CreatedUtc);
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");
        var imported = 0;
        foreach (var row in rows)
        {
            await targetConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    Id = CreateStableGuid("category", row.Id),
                    row.Name,
                    ColourHex = string.IsNullOrWhiteSpace(row.Color) ? "#001CAD" : row.Color,
                    CreatedUtc = now
                },
                cancellationToken: cancellationToken));

            imported++;
        }

        return imported;
    }

    public async Task<int> WriteTemplatesAsync(SqliteConnection targetConnection, IEnumerable<RustTemplateRow> rows, CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO event_templates
            (id, name, title, default_duration_minutes, category, notes, created_utc)
            VALUES
            (@Id, @Name, @Title, @DefaultDurationMinutes, @Category, @Notes, @CreatedUtc);
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");
        var imported = 0;
        foreach (var row in rows)
        {
            await targetConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    Id = CreateStableGuid("template", row.Id),
                    row.Name,
                    row.Title,
                    DefaultDurationMinutes = row.DurationMinutes <= 0 ? 60 : row.DurationMinutes,
                    Category = string.IsNullOrWhiteSpace(row.Category) ? "General" : row.Category,
                    Notes = row.Description,
                    CreatedUtc = now
                },
                cancellationToken: cancellationToken));

            imported++;
        }

        return imported;
    }

    public async Task<int> WriteSettingsAsync(SqliteConnection targetConnection, IDictionary<string, string> values, CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO settings (key, value, updated_utc)
            VALUES (@Key, @Value, @UpdatedUtc);
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");
        foreach (var pair in values)
        {
            await targetConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new { Key = pair.Key, Value = pair.Value, UpdatedUtc = now },
                cancellationToken: cancellationToken));
        }

        return values.Count;
    }

    public async Task<int> WriteThemesAsync(SqliteConnection targetConnection, IEnumerable<RustThemeRow> rows, CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT OR REPLACE INTO custom_themes (id, name, definition_json, created_utc)
            VALUES (@Id, @Name, @DefinitionJson, @CreatedUtc);
            """;

        var now = DateTimeOffset.UtcNow.ToString("O");
        var imported = 0;

        foreach (var row in rows)
        {
            var definition = JsonSerializer.Serialize(new
            {
                isDark = row.IsDark > 0,
                row.AppBackground,
                row.CalendarBackground,
                row.WeekendBackground,
                row.TodayBackground,
                row.TodayBorder,
                row.DayBackground,
                row.DayBorder,
                row.TextPrimary,
                row.TextSecondary,
                row.HeaderBackground,
                row.HeaderText
            });

            await targetConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    Id = CreateStableGuid("theme", row.Id),
                    row.Name,
                    DefinitionJson = definition,
                    CreatedUtc = now
                },
                cancellationToken: cancellationToken));

            imported++;
        }

        return imported;
    }

    private static bool TryParseDate(string value, out string normalized)
    {
        if (DateTimeOffset.TryParse(value, out var parsed))
        {
            normalized = parsed.ToString("O");
            return true;
        }

        normalized = string.Empty;
        return false;
    }

    private static string CreateStableGuid(string prefix, long id)
    {
        var source = Encoding.UTF8.GetBytes($"{prefix}:{id}");
        var hash = MD5.HashData(source);
        return new Guid(hash).ToString();
    }
}
