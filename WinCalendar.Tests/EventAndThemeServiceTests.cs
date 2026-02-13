using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application;
using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Tests;

public class EventAndThemeServiceTests
{
    [Fact]
    public async Task EventService_Should_Save_QueryRange_And_Delete()
    {
        var repository = new InMemoryEventRepository();

        var services = new ServiceCollection();
        services.AddSingleton<IEventRepository>(repository);
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var eventService = provider.GetRequiredService<IEventService>();
        var firstEvent = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = "Planning",
            StartDateTime = new DateTimeOffset(2026, 2, 18, 9, 0, 0, TimeSpan.FromHours(10)),
            EndDateTime = new DateTimeOffset(2026, 2, 18, 10, 0, 0, TimeSpan.FromHours(10)),
            Category = "Work"
        };

        await eventService.SaveEventAsync(firstEvent);

        var rangeItems = await eventService.GetEventsInRangeAsync(
            new DateTimeOffset(2026, 2, 18, 0, 0, 0, TimeSpan.FromHours(10)),
            new DateTimeOffset(2026, 2, 18, 23, 59, 0, TimeSpan.FromHours(10)));

        Assert.Single(rangeItems);

        await eventService.DeleteEventAsync(firstEvent.Id);
        var allItems = await eventService.GetEventsAsync();
        Assert.Empty(allItems);
    }

    [Fact]
    public async Task ThemeService_Should_Save_And_Select_Active_Theme()
    {
        var themeRepository = new InMemoryThemeRepository();
        var settingsRepository = new InMemorySettingsRepository();

        var services = new ServiceCollection();
        services.AddSingleton<IThemeRepository>(themeRepository);
        services.AddSingleton<ISettingsRepository>(settingsRepository);
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IThemeService>();

        await service.SaveThemeAsync(new AppTheme
        {
            Id = Guid.NewGuid(),
            Name = "Ocean",
            DefinitionJson = "{\"isDark\":false}"
        });

        await service.SetActiveThemeNameAsync("Ocean");

        var themes = await service.GetThemesAsync();
        var active = await service.GetActiveThemeNameAsync();

        Assert.Single(themes);
        Assert.Equal("Ocean", active);
    }

    private sealed class InMemoryEventRepository : IEventRepository
    {
        private readonly List<CalendarEvent> _items = [];

        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CalendarEvent>>(_items.OrderBy(x => x.StartDateTime).ToList());
        }

        public Task<IReadOnlyList<CalendarEvent>> GetInRangeAsync(DateTimeOffset rangeStart, DateTimeOffset rangeEnd, CancellationToken cancellationToken = default)
        {
            var rows = _items
                .Where(x => x.StartDateTime <= rangeEnd && x.EndDateTime >= rangeStart)
                .OrderBy(x => x.StartDateTime)
                .ToList();

            return Task.FromResult<IReadOnlyList<CalendarEvent>>(rows);
        }

        public Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            _items.Add(calendarEvent);
            return Task.CompletedTask;
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
    }

    private sealed class InMemoryThemeRepository : IThemeRepository
    {
        private readonly List<AppTheme> _themes = [];

        public Task<IReadOnlyList<AppTheme>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AppTheme>>(_themes.OrderBy(x => x.Name).ToList());
        }

        public Task UpsertAsync(AppTheme theme, CancellationToken cancellationToken = default)
        {
            _themes.RemoveAll(x => x.Id == theme.Id);
            _themes.Add(theme);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        private readonly List<AppSetting> _settings = [];

        public Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AppSetting>>(_settings.ToList());
        }

        public Task UpsertAsync(AppSetting setting, CancellationToken cancellationToken = default)
        {
            _settings.RemoveAll(x => string.Equals(x.Key, setting.Key, StringComparison.OrdinalIgnoreCase));
            _settings.Add(setting);
            return Task.CompletedTask;
        }
    }
}
