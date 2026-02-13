using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Application.Services.Ics;

namespace WinCalendar.Application.Services;

internal sealed class IcsService(
    IEventRepository eventRepository,
    IcsEventParser parser,
    IcsEventWriter writer) : IIcsService
{
    public async Task<int> ImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("ICS file path is required.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("ICS file does not exist.", filePath);
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var parsedEvents = parser.Parse(content);

        var existing = await eventRepository.GetAllAsync(cancellationToken);
        var existingKeys = existing
            .Select(CreateFingerprint)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var imported = 0;
        foreach (var calendarEvent in parsedEvents)
        {
            if (!existingKeys.Add(CreateFingerprint(calendarEvent)))
            {
                continue;
            }

            await eventRepository.AddAsync(calendarEvent, cancellationToken);
            imported++;
        }

        return imported;
    }

    public async Task ExportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("ICS file path is required.", nameof(filePath));
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var events = await eventRepository.GetAllAsync(cancellationToken);
        var content = writer.Write(events);
        await File.WriteAllTextAsync(filePath, content, cancellationToken);
    }

    private static string CreateFingerprint(WinCalendar.Domain.Entities.CalendarEvent calendarEvent)
    {
        return $"{calendarEvent.Title}|{calendarEvent.StartDateTime:O}|{calendarEvent.EndDateTime:O}";
    }
}
