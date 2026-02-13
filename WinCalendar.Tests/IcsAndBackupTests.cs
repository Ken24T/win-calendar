using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application;
using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;
using WinCalendar.Infrastructure;
using WinCalendar.Infrastructure.Persistence;

namespace WinCalendar.Tests;

public class IcsAndBackupTests
{
    [Fact]
    public async Task IcsService_Should_Export_And_Import_Events()
    {
        var repo = new FakeEventRepository(
        [
            new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = "Planning",
                StartDateTime = new DateTimeOffset(2026, 2, 15, 9, 0, 0, TimeSpan.FromHours(10)),
                EndDateTime = new DateTimeOffset(2026, 2, 15, 10, 0, 0, TimeSpan.FromHours(10)),
                Category = "Work",
                Notes = "Sprint planning"
            }
        ]);

        var services = new ServiceCollection();
        services.AddSingleton<IEventRepository>(repo);
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var icsService = provider.GetRequiredService<IIcsService>();

        var root = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var icsPath = Path.Combine(root, "events.ics");

        await icsService.ExportAsync(icsPath);

        repo.Reset([]);
        var imported = await icsService.ImportAsync(icsPath);

        Assert.Equal(1, imported);
        Assert.Single(repo.Items);
        Assert.Equal("Planning", repo.Items[0].Title);
    }

    [Fact]
    public async Task BackupService_Should_Create_And_Restore_Database_File()
    {
        var root = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var databasePath = Path.Combine(root, "calendar.db");
        var backupPath = Path.Combine(root, "calendar-backup.db");

        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddSingleton(new SqliteConnectionFactory(databasePath));
        using var provider = services.BuildServiceProvider();

        var migrator = provider.GetRequiredService<IDatabaseMigrator>();
        var backupService = provider.GetRequiredService<IBackupService>();

        await migrator.MigrateAsync();

        await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("INSERT INTO settings (key, value, updated_utc) VALUES ('theme', 'dark', @UpdatedUtc);", new { UpdatedUtc = DateTimeOffset.UtcNow.ToString("O") });
        }

        await backupService.CreateBackupAsync(backupPath);

        await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
        {
            await connection.OpenAsync();
            await connection.ExecuteAsync("DELETE FROM settings WHERE key = 'theme';");
        }

        await backupService.RestoreBackupAsync(backupPath);

        await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
        {
            await connection.OpenAsync();
            var count = await connection.ExecuteScalarAsync<long>("SELECT COUNT(1) FROM settings WHERE key = 'theme' AND value = 'dark';");
            Assert.Equal(1, count);
        }
    }

    private sealed class FakeEventRepository(IReadOnlyList<CalendarEvent> seed) : IEventRepository
    {
        private readonly List<CalendarEvent> _items = [.. seed];

        public IReadOnlyList<CalendarEvent> Items => _items;

        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CalendarEvent>>(_items.OrderBy(x => x.StartDateTime).ToList());
        }

        public Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            _items.Add(calendarEvent);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<CalendarEvent>> GetInRangeAsync(
            DateTimeOffset rangeStart,
            DateTimeOffset rangeEnd,
            CancellationToken cancellationToken = default)
        {
            var rows = _items
                .Where(x => x.StartDateTime <= rangeEnd && x.EndDateTime >= rangeStart)
                .OrderBy(x => x.StartDateTime)
                .ToList();

            return Task.FromResult<IReadOnlyList<CalendarEvent>>(rows);
        }

        public Task UpsertAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(x => x.Id == calendarEvent.Id);
            _items.Add(calendarEvent);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(x => x.Id == eventId);
            return Task.CompletedTask;
        }

        public void Reset(IEnumerable<CalendarEvent> newItems)
        {
            _items.Clear();
            _items.AddRange(newItems);
        }
    }
}
