using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Abstractions;
using WinCalendar.Application;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Tests;

public class CoreServicesTests
{
    [Fact]
    public void RecurrenceService_Should_Expand_Daily_Rule()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IRecurrenceService>();

        var start = new DateTimeOffset(2026, 2, 14, 9, 0, 0, TimeSpan.FromHours(10));

        var calendarEvent = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = "Daily stand-up",
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            RecurrenceRule = "FREQ=DAILY;COUNT=3"
        };

        var occurrences = service.ExpandOccurrences(
            calendarEvent,
            start.AddDays(-1),
            start.AddDays(10));

        Assert.Equal(3, occurrences.Count);
    }

    [Fact]
    public async Task EventSearchService_Should_Filter_By_Title_And_Category()
    {
        var repository = new FakeEventRepository(
        [
            new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = "Team stand-up",
                Category = "Work",
                StartDateTime = DateTimeOffset.UtcNow,
                EndDateTime = DateTimeOffset.UtcNow.AddMinutes(30)
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = "Dentist",
                Category = "Personal",
                StartDateTime = DateTimeOffset.UtcNow,
                EndDateTime = DateTimeOffset.UtcNow.AddMinutes(45)
            }
        ]);

        var services = new ServiceCollection();
        services.AddSingleton<IEventRepository>(repository);
        services.AddApplication();
        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IEventSearchService>();

        var workResults = await service.SearchAsync("work");
        var dentistResults = await service.SearchAsync("dent");

        Assert.Single(workResults);
        Assert.Single(dentistResults);
        Assert.Equal("Team stand-up", workResults[0].Title);
        Assert.Equal("Dentist", dentistResults[0].Title);
    }

    private sealed class FakeEventRepository(IReadOnlyList<CalendarEvent> items) : IEventRepository
    {
        public Task<IReadOnlyList<CalendarEvent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(items);
        }

        public Task AddAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
