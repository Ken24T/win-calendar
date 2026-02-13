using Microsoft.Data.Sqlite;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Import.Contracts;
using WinCalendar.Import.Models;

namespace WinCalendar.Import.Services;

internal sealed class RustCalendarImporter(
    IDatabaseMigrator databaseMigrator,
    SqliteConnectionFactory connectionFactory,
    RustSourceReader sourceReader,
    TargetDataWriter targetWriter) : IRustCalendarImporter
{
    public async Task<RustImportResult> ImportAsync(RustDbImportProfile profile, CancellationToken cancellationToken = default)
    {
        ValidateProfile(profile);

        await databaseMigrator.MigrateAsync(cancellationToken);

        await using var sourceConnection = BuildSourceConnection(profile.SourceDatabasePath);
        await sourceConnection.OpenAsync(cancellationToken);

        await using var targetConnection = connectionFactory.CreateConnection();
        await targetConnection.OpenAsync(cancellationToken);

        var result = new RustImportResult();

        if (profile.ImportCategories)
        {
            var categories = await sourceReader.ReadCategoriesAsync(sourceConnection, cancellationToken);
            result.ImportedCategories = await targetWriter.WriteCategoriesAsync(targetConnection, categories, cancellationToken);
        }

        if (profile.ImportTemplates)
        {
            var templates = await sourceReader.ReadTemplatesAsync(sourceConnection, cancellationToken);
            result.ImportedTemplates = await targetWriter.WriteTemplatesAsync(targetConnection, templates, cancellationToken);
        }

        if (profile.ImportSettings)
        {
            var settings = await sourceReader.ReadSettingsAsync(sourceConnection, cancellationToken);
            result.ImportedSettings = await targetWriter.WriteSettingsAsync(targetConnection, settings, cancellationToken);
        }

        if (profile.ImportCustomThemes)
        {
            var themes = await sourceReader.ReadThemesAsync(sourceConnection, cancellationToken);
            result.ImportedThemes = await targetWriter.WriteThemesAsync(targetConnection, themes, cancellationToken);
        }

        if (profile.ImportEvents)
        {
            var events = await sourceReader.ReadEventsAsync(sourceConnection, cancellationToken);
            result.ImportedEvents = await targetWriter.WriteEventsAsync(targetConnection, events, result.Warnings, cancellationToken);
        }

        return result;
    }

    private static void ValidateProfile(RustDbImportProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.SourceDatabasePath))
        {
            throw new ArgumentException("Source database path is required.", nameof(profile));
        }

        if (!File.Exists(profile.SourceDatabasePath))
        {
            throw new FileNotFoundException("Source database file does not exist.", profile.SourceDatabasePath);
        }
    }

    private static SqliteConnection BuildSourceConnection(string sourceDatabasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDatabasePath,
            Mode = SqliteOpenMode.ReadOnly,
            ForeignKeys = true
        };

        return new SqliteConnection(builder.ConnectionString);
    }
}
