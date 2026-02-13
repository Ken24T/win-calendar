using Dapper;
using Microsoft.Data.Sqlite;
using WinCalendar.Import.Models;

namespace WinCalendar.Import.Services;

internal sealed class RustSourceReader
{
    public async Task<IReadOnlyList<RustEventRow>> ReadEventsAsync(SqliteConnection sourceConnection, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                id,
                title,
                description,
                location,
                start_datetime AS StartDateTime,
                end_datetime AS EndDateTime,
                is_all_day AS IsAllDay,
                category,
                recurrence_rule AS RecurrenceRule,
                recurrence_exceptions AS RecurrenceExceptions
            FROM events
            ORDER BY id;
            """;

        var rows = await sourceConnection.QueryAsync<RustEventRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<IReadOnlyList<RustCategoryRow>> ReadCategoriesAsync(SqliteConnection sourceConnection, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, color FROM categories ORDER BY id;";
        var rows = await sourceConnection.QueryAsync<RustCategoryRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<IReadOnlyList<RustTemplateRow>> ReadTemplatesAsync(SqliteConnection sourceConnection, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT id, name, title, description, duration_minutes AS DurationMinutes, category
            FROM event_templates
            ORDER BY id;
            """;

        var rows = await sourceConnection.QueryAsync<RustTemplateRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }

    public async Task<IDictionary<string, string>> ReadSettingsAsync(SqliteConnection sourceConnection, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT theme, first_day_of_week, time_format, date_format, current_view,
                   default_event_duration, first_day_of_work_week, last_day_of_work_week,
                   default_event_start_time, show_sidebar, use_system_theme,
                   show_week_numbers, sidebar_width
            FROM settings
            WHERE id = 1;
            """;

        var row = await sourceConnection.QuerySingleOrDefaultAsync(sql);
        if (row is null)
        {
            return new Dictionary<string, string>();
        }

        return new Dictionary<string, string>
        {
            ["theme"] = (string?)row.theme ?? "light",
            ["first_day_of_week"] = row.first_day_of_week?.ToString() ?? "0",
            ["time_format"] = (string?)row.time_format ?? "12h",
            ["date_format"] = (string?)row.date_format ?? "MM/DD/YYYY",
            ["current_view"] = (string?)row.current_view ?? "Month",
            ["default_event_duration"] = row.default_event_duration?.ToString() ?? "60",
            ["first_day_of_work_week"] = row.first_day_of_work_week?.ToString() ?? "1",
            ["last_day_of_work_week"] = row.last_day_of_work_week?.ToString() ?? "5",
            ["default_event_start_time"] = (string?)row.default_event_start_time ?? "08:00",
            ["show_sidebar"] = row.show_sidebar?.ToString() ?? "1",
            ["use_system_theme"] = row.use_system_theme?.ToString() ?? "0",
            ["show_week_numbers"] = row.show_week_numbers?.ToString() ?? "0",
            ["sidebar_width"] = row.sidebar_width?.ToString() ?? "180"
        };
    }

    public async Task<IReadOnlyList<RustThemeRow>> ReadThemesAsync(SqliteConnection sourceConnection, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                id,
                name,
                is_dark AS IsDark,
                app_background AS AppBackground,
                calendar_background AS CalendarBackground,
                weekend_background AS WeekendBackground,
                today_background AS TodayBackground,
                today_border AS TodayBorder,
                day_background AS DayBackground,
                day_border AS DayBorder,
                text_primary AS TextPrimary,
                text_secondary AS TextSecondary,
                header_background AS HeaderBackground,
                header_text AS HeaderText
            FROM custom_themes
            ORDER BY id;
            """;

        var rows = await sourceConnection.QueryAsync<RustThemeRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.ToList();
    }
}
