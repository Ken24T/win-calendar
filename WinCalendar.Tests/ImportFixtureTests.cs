using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Infrastructure;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Import;
using WinCalendar.Import.Contracts;

namespace WinCalendar.Tests;

public class ImportFixtureTests
{
    [Fact]
    public async Task Importer_Should_Import_Fixture_Counts_And_Warnings()
    {
        var paths = await CreateFixtureDatabasesAsync();

        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddRustImport();
        services.AddSingleton(new SqliteConnectionFactory(paths.TargetPath));

        using var provider = services.BuildServiceProvider();
        var importer = provider.GetRequiredService<IRustCalendarImporter>();

        var result = await importer.ImportAsync(new RustDbImportProfile
        {
            SourceDatabasePath = paths.SourcePath,
            ImportEvents = true,
            ImportCategories = true,
            ImportTemplates = true,
            ImportSettings = true,
            ImportCustomThemes = true
        });

        Assert.Equal(2, result.ImportedEvents);
        Assert.Equal(2, result.ImportedCategories);
        Assert.Equal(2, result.ImportedTemplates);
        Assert.Equal(1, result.ImportedThemes);
        Assert.True(result.ImportedSettings >= 4);
        Assert.Contains(result.Warnings.Items, item => item.Contains("invalid datetime format", StringComparison.OrdinalIgnoreCase));

        await using var target = new SqliteConnection($"Data Source={paths.TargetPath}");
        await target.OpenAsync();

        Assert.Equal(2, await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM events;"));
        Assert.Equal(2, await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM categories;"));
        Assert.Equal(2, await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM event_templates;"));
        Assert.Equal(1, await target.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM custom_themes;"));
    }

    [Fact]
    public async Task Importer_Should_Apply_Conversion_Rules_From_Fixture()
    {
        var paths = await CreateFixtureDatabasesAsync();

        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddRustImport();
        services.AddSingleton(new SqliteConnectionFactory(paths.TargetPath));

        using var provider = services.BuildServiceProvider();
        var importer = provider.GetRequiredService<IRustCalendarImporter>();

        await importer.ImportAsync(new RustDbImportProfile
        {
            SourceDatabasePath = paths.SourcePath,
            ImportEvents = true,
            ImportCategories = true,
            ImportTemplates = true,
            ImportSettings = true,
            ImportCustomThemes = true
        });

        await using var target = new SqliteConnection($"Data Source={paths.TargetPath}");
        await target.OpenAsync();

        var importedEvent = await target.QuerySingleAsync(
            "SELECT start_datetime AS StartDateTime, end_datetime AS EndDateTime, category AS Category, recurrence_exceptions AS RecurrenceExceptions FROM events WHERE title = 'Imported Event';");

        Assert.Equal(DateTimeOffset.Parse("2026-02-14T09:00:00+10:00").ToString("O"), (string)importedEvent.StartDateTime);
        Assert.Equal(DateTimeOffset.Parse("2026-02-14T10:00:00+10:00").ToString("O"), (string)importedEvent.EndDateTime);
        Assert.Equal("Work", (string)importedEvent.Category);
        Assert.Equal("[\"2026-02-21T09:00:00+10:00\"]", (string)importedEvent.RecurrenceExceptions);

        var fallbackEvent = await target.QuerySingleAsync(
            "SELECT category AS Category, recurrence_exceptions AS RecurrenceExceptions FROM events WHERE title = 'No Category Event';");

        Assert.Equal("General", (string)fallbackEvent.Category);
        Assert.Equal("[]", (string)fallbackEvent.RecurrenceExceptions);

        var fallbackTemplate = await target.QuerySingleAsync(
            "SELECT default_duration_minutes AS Duration, category AS Category FROM event_templates WHERE name = 'Quick Task';");

        Assert.Equal(60L, (long)fallbackTemplate.Duration);
        Assert.Equal("General", (string)fallbackTemplate.Category);

        var fallbackCategoryColour = await target.ExecuteScalarAsync<string>(
            "SELECT colour_hex FROM categories WHERE name = 'No Colour';");

        Assert.Equal("#001CAD", fallbackCategoryColour);

        var themeDefinition = await target.ExecuteScalarAsync<string>(
            "SELECT definition_json FROM custom_themes WHERE name = 'Custom Dark';");

        Assert.Contains("\"isDark\":true", themeDefinition, StringComparison.OrdinalIgnoreCase);

        var settingTheme = await target.QuerySingleOrDefaultAsync<string>(
            "SELECT value FROM settings WHERE key = 'theme';");

        Assert.Equal("light", settingTheme);
    }

    [Fact]
    public async Task Importer_Should_Persist_Settings_Without_Duplicate_Keys_On_Reimport()
    {
        var paths = await CreateFixtureDatabasesAsync();

        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddRustImport();
        services.AddSingleton(new SqliteConnectionFactory(paths.TargetPath));

        using var provider = services.BuildServiceProvider();
        var importer = provider.GetRequiredService<IRustCalendarImporter>();

        var profile = new RustDbImportProfile
        {
            SourceDatabasePath = paths.SourcePath,
            ImportEvents = false,
            ImportCategories = false,
            ImportTemplates = false,
            ImportSettings = true,
            ImportCustomThemes = false
        };

        await importer.ImportAsync(profile);

        await using var source = new SqliteConnection($"Data Source={paths.SourcePath}");
        await source.OpenAsync();
        await source.ExecuteAsync(
            "UPDATE settings SET theme = 'dark', default_event_duration = 30, show_week_numbers = 0 WHERE id = 1;");

        await importer.ImportAsync(profile);

        await using var target = new SqliteConnection($"Data Source={paths.TargetPath}");
        await target.OpenAsync();

        var themeValue = await target.ExecuteScalarAsync<string>(
            "SELECT value FROM settings WHERE key = 'theme';");
        var durationValue = await target.ExecuteScalarAsync<string>(
            "SELECT value FROM settings WHERE key = 'default_event_duration';");
        var weekNumberValue = await target.ExecuteScalarAsync<string>(
            "SELECT value FROM settings WHERE key = 'show_week_numbers';");

        Assert.Equal("dark", themeValue);
        Assert.Equal("30", durationValue);
        Assert.Equal("0", weekNumberValue);

        var duplicateThemeKeys = await target.ExecuteScalarAsync<long>(
            "SELECT COUNT(1) FROM settings WHERE key = 'theme';");

        Assert.Equal(1, duplicateThemeKeys);
    }

    private static async Task<(string SourcePath, string TargetPath)> CreateFixtureDatabasesAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var sourcePath = Path.Combine(root, "rust-calendar-fixture.db");
        var targetPath = Path.Combine(root, "win-calendar-fixture.db");

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
            "INSERT INTO categories VALUES (11, 'No Colour', '');");

        await source.ExecuteAsync(
            "INSERT INTO event_templates VALUES (20, 'Meeting', 'Team Meeting', 'Weekly sync', 60, 'Work');");
        await source.ExecuteAsync(
            "INSERT INTO event_templates VALUES (21, 'Quick Task', 'Quick Task', 'Fallback defaults', 0, NULL);");

        await source.ExecuteAsync(
            "INSERT INTO custom_themes VALUES (30, 'Custom Dark', 1, '0.1,0.1,0.1', '0.1,0.1,0.1', '0.12,0.12,0.12', '0.2,0.2,0.3', '0.5,0.5,0.8', '0.1,0.1,0.1', '0.2,0.2,0.2', '0.95,0.95,0.95', '0.75,0.75,0.75', NULL, NULL);");

        await source.ExecuteAsync(
            "INSERT INTO events VALUES (40, 'Imported Event', 'From rust', 'Office', '2026-02-14T09:00:00+10:00', '2026-02-14T10:00:00+10:00', 0, 'Work', 'FREQ=WEEKLY;BYDAY=FR', '[\"2026-02-21T09:00:00+10:00\"]');");
        await source.ExecuteAsync(
            "INSERT INTO events VALUES (41, 'No Category Event', 'Fallback category', 'Home', '2026-02-15T09:00:00+10:00', '2026-02-15T10:00:00+10:00', 0, NULL, NULL, NULL);");
        await source.ExecuteAsync(
            "INSERT INTO events VALUES (42, 'Invalid Datetime Event', 'Should warn', 'Nowhere', 'bad-datetime', '2026-02-16T10:00:00+10:00', 0, 'Work', NULL, '[]');");

        return (sourcePath, targetPath);
    }
}
