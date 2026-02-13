using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Infrastructure;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Import;
using WinCalendar.Import.Contracts;

namespace WinCalendar.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Importer_Should_Copy_Core_Data_From_Rust_Database()
    {
        var root = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var sourcePath = Path.Combine(root, "rust-calendar.db");
        var targetPath = Path.Combine(root, "win-calendar.db");

        await CreateRustSourceDatabaseAsync(sourcePath);

        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddRustImport();
        services.AddSingleton(new SqliteConnectionFactory(targetPath));

        var provider = services.BuildServiceProvider();
        var importer = provider.GetRequiredService<IRustCalendarImporter>();

        var result = await importer.ImportAsync(new RustDbImportProfile
        {
            SourceDatabasePath = sourcePath,
            ImportEvents = true,
            ImportCategories = true,
            ImportTemplates = true,
            ImportSettings = true,
            ImportCustomThemes = true
        });

        Assert.Equal(1, result.ImportedEvents);
        Assert.Equal(1, result.ImportedCategories);
        Assert.Equal(1, result.ImportedTemplates);
        Assert.True(result.ImportedSettings >= 4);
        Assert.Equal(1, result.ImportedThemes);

        await using var target = new SqliteConnection($"Data Source={targetPath}");
        await target.OpenAsync();

        var eventCount = await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM events;");
        var categoryCount = await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM categories;");
        var templateCount = await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM event_templates;");

        Assert.Equal(1, eventCount);
        Assert.Equal(1, categoryCount);
        Assert.Equal(1, templateCount);
    }

    private static async Task CreateRustSourceDatabaseAsync(string sourcePath)
    {
        await using var source = new SqliteConnection($"Data Source={sourcePath}");
        await source.OpenAsync();

        var schemaSql =
            """
            CREATE TABLE settings (
                id INTEGER PRIMARY KEY,
                theme TEXT,
                first_day_of_week INTEGER,
                time_format TEXT,
                date_format TEXT,
                current_view TEXT,
                default_event_duration INTEGER,
                first_day_of_work_week INTEGER,
                last_day_of_work_week INTEGER,
                default_event_start_time TEXT,
                show_sidebar INTEGER,
                use_system_theme INTEGER,
                show_week_numbers INTEGER,
                sidebar_width REAL
            );

            CREATE TABLE events (
                id INTEGER PRIMARY KEY,
                title TEXT NOT NULL,
                description TEXT,
                location TEXT,
                start_datetime TEXT NOT NULL,
                end_datetime TEXT NOT NULL,
                is_all_day INTEGER NOT NULL,
                category TEXT,
                recurrence_rule TEXT,
                recurrence_exceptions TEXT
            );

            CREATE TABLE categories (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                color TEXT NOT NULL
            );

            CREATE TABLE event_templates (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                title TEXT NOT NULL,
                description TEXT,
                duration_minutes INTEGER NOT NULL,
                category TEXT
            );

            CREATE TABLE custom_themes (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                is_dark INTEGER NOT NULL,
                app_background TEXT NOT NULL,
                calendar_background TEXT NOT NULL,
                weekend_background TEXT NOT NULL,
                today_background TEXT NOT NULL,
                today_border TEXT NOT NULL,
                day_background TEXT NOT NULL,
                day_border TEXT NOT NULL,
                text_primary TEXT NOT NULL,
                text_secondary TEXT NOT NULL,
                header_background TEXT,
                header_text TEXT
            );
            """;

        await source.ExecuteAsync(schemaSql);

        await source.ExecuteAsync(
            "INSERT INTO settings VALUES (1, 'light', 1, '24h', 'DD/MM/YYYY', 'Month', 45, 1, 5, '08:00', 1, 0, 1, 180.0);");
        await source.ExecuteAsync(
            "INSERT INTO categories VALUES (10, 'Work', '#001CAD');");
        await source.ExecuteAsync(
            "INSERT INTO event_templates VALUES (20, 'Meeting', 'Team Meeting', 'Weekly sync', 60, 'Work');");
        await source.ExecuteAsync(
            "INSERT INTO custom_themes VALUES (30, 'Custom Light', 0, '1,1,1', '1,1,1', '0.9,0.9,0.9', '0.8,0.9,1', '0.3,0.5,0.9', '1,1,1', '0.8,0.8,0.8', '0.1,0.1,0.1', '0.4,0.4,0.4', NULL, NULL);");
        await source.ExecuteAsync(
            "INSERT INTO events VALUES (40, 'Imported Event', 'From rust', 'Office', '2026-02-14T09:00:00+10:00', '2026-02-14T10:00:00+10:00', 0, 'Work', NULL, '[]');");
    }
}