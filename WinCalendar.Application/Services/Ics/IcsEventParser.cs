using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services.Ics;

internal sealed class IcsEventParser
{
    public IReadOnlyList<CalendarEvent> Parse(string content)
    {
        var lines = UnfoldLines(content);
        var results = new List<CalendarEvent>();

        Dictionary<string, string>? current = null;

        foreach (var line in lines)
        {
            if (line == "BEGIN:VEVENT")
            {
                current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            if (line == "END:VEVENT")
            {
                if (current is not null && TryMapEvent(current, out var calendarEvent))
                {
                    results.Add(calendarEvent);
                }

                current = null;
                continue;
            }

            if (current is null)
            {
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var rawKey = line[..separator];
            var key = rawKey.Split(';', 2)[0];
            var value = line[(separator + 1)..];
            current[key] = value;
        }

        return results;
    }

    private static bool TryMapEvent(IReadOnlyDictionary<string, string> fields, out CalendarEvent calendarEvent)
    {
        calendarEvent = null!;

        if (!fields.TryGetValue("DTSTART", out var startRaw) || !IcsLineCodec.TryParseDateTime(startRaw, out var start))
        {
            return false;
        }

        if (!fields.TryGetValue("DTEND", out var endRaw) || !IcsLineCodec.TryParseDateTime(endRaw, out var end))
        {
            end = start.AddHours(1);
        }

        fields.TryGetValue("UID", out var uidRaw);
        fields.TryGetValue("SUMMARY", out var summaryRaw);
        fields.TryGetValue("DESCRIPTION", out var descriptionRaw);
        fields.TryGetValue("LOCATION", out var locationRaw);
        fields.TryGetValue("CATEGORIES", out var categoriesRaw);
        fields.TryGetValue("RRULE", out var ruleRaw);

        calendarEvent = new CalendarEvent
        {
            Id = TryParseGuid(uidRaw),
            Title = IcsLineCodec.Unescape(summaryRaw),
            Notes = IcsLineCodec.Unescape(descriptionRaw),
            Location = IcsLineCodec.Unescape(locationRaw),
            Category = string.IsNullOrWhiteSpace(categoriesRaw) ? "General" : IcsLineCodec.Unescape(categoriesRaw),
            StartDateTime = start,
            EndDateTime = end,
            IsAllDay = startRaw.Length == 8,
            RecurrenceRule = string.IsNullOrWhiteSpace(ruleRaw) ? null : ruleRaw
        };

        return true;
    }

    private static IReadOnlyList<string> UnfoldLines(string content)
    {
        var sourceLines = content
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.None);

        var unfolded = new List<string>();
        foreach (var line in sourceLines)
        {
            if (line.StartsWith(' ') || line.StartsWith('\t'))
            {
                if (unfolded.Count > 0)
                {
                    unfolded[^1] += line[1..];
                }

                continue;
            }

            unfolded.Add(line);
        }

        return unfolded;
    }

    private static Guid TryParseGuid(string? raw)
    {
        return Guid.TryParse(raw, out var parsed) ? parsed : Guid.NewGuid();
    }
}
