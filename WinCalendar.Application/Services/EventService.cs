using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class EventService(IEventRepository eventRepository) : IEventService
{
    public Task<IReadOnlyList<CalendarEvent>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        return eventRepository.GetAllAsync(cancellationToken);
    }

    public async Task SeedSampleEventsAsync(CancellationToken cancellationToken = default)
    {
        var existingEvents = await eventRepository.GetAllAsync(cancellationToken);
        if (existingEvents.Count > 0)
        {
            return;
        }

        var now = DateTimeOffset.Now;

        var samples = new[]
        {
            new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = "Team stand-up",
                StartDateTime = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 0, 0, now.Offset),
                EndDateTime = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 30, 0, now.Offset),
                Category = "Work"
            },
            new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = "Body corporate reminder",
                StartDateTime = new DateTimeOffset(now.Year, now.Month, now.Day, 17, 0, 0, now.Offset),
                EndDateTime = new DateTimeOffset(now.Year, now.Month, now.Day, 18, 0, 0, now.Offset),
                Category = "Admin"
            }
        };

        foreach (var calendarEvent in samples)
        {
            await eventRepository.AddAsync(calendarEvent, cancellationToken);
        }
    }
}
