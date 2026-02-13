using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class RecurrenceService : IRecurrenceService
{
    public IReadOnlyList<DateTimeOffset> ExpandOccurrences(
        CalendarEvent calendarEvent,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int maxOccurrences = 250)
    {
        if (rangeStart > rangeEnd || maxOccurrences <= 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(calendarEvent.RecurrenceRule))
        {
            IReadOnlyList<DateTimeOffset> single = IsInRange(calendarEvent.StartDateTime, rangeStart, rangeEnd)
                ? [calendarEvent.StartDateTime]
                : [];

            return FilterExceptions(single, calendarEvent.RecurrenceExceptions);
        }

        var rule = ParseRule(calendarEvent.RecurrenceRule);
        if (!rule.TryGetValue("FREQ", out var frequency))
        {
            IReadOnlyList<DateTimeOffset> single = IsInRange(calendarEvent.StartDateTime, rangeStart, rangeEnd)
                ? [calendarEvent.StartDateTime]
                : [];

            return FilterExceptions(single, calendarEvent.RecurrenceExceptions);
        }

        var countLimit = ParseInt(rule, "COUNT");
        var until = ParseDate(rule, "UNTIL");
        var interval = Math.Max(ParseInt(rule, "INTERVAL") ?? 1, 1);

        var expanded = frequency.ToUpperInvariant() switch
        {
            "WEEKLY" => ExpandWeekly(calendarEvent.StartDateTime, rangeStart, rangeEnd, interval, countLimit, until, rule, maxOccurrences),
            "MONTHLY" => ExpandFixedStep(calendarEvent.StartDateTime, rangeStart, rangeEnd, countLimit, until, maxOccurrences, value => value.AddMonths(interval)),
            "YEARLY" => ExpandFixedStep(calendarEvent.StartDateTime, rangeStart, rangeEnd, countLimit, until, maxOccurrences, value => value.AddYears(interval)),
            _ => ExpandFixedStep(calendarEvent.StartDateTime, rangeStart, rangeEnd, countLimit, until, maxOccurrences, value => value.AddDays(interval))
        };

        return FilterExceptions(expanded, calendarEvent.RecurrenceExceptions);
    }

    private static IReadOnlyList<DateTimeOffset> FilterExceptions(
        IReadOnlyList<DateTimeOffset> occurrences,
        IReadOnlyList<DateTimeOffset> recurrenceExceptions)
    {
        if (occurrences.Count == 0 || recurrenceExceptions.Count == 0)
        {
            return occurrences;
        }

        var filtered = occurrences
            .Where(occurrence => !recurrenceExceptions.Any(exception =>
                exception.UtcDateTime == occurrence.UtcDateTime ||
                exception.Date == occurrence.Date))
            .ToList();

        return filtered;
    }

    private static IReadOnlyList<DateTimeOffset> ExpandFixedStep(
        DateTimeOffset start,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int? countLimit,
        DateTimeOffset? until,
        int maxOccurrences,
        Func<DateTimeOffset, DateTimeOffset> next)
    {
        var results = new List<DateTimeOffset>();
        var current = start;
        var produced = 0;

        while (results.Count < maxOccurrences)
        {
            if (countLimit.HasValue && produced >= countLimit.Value)
            {
                break;
            }

            if (until.HasValue && current > until.Value)
            {
                break;
            }

            if (current > rangeEnd)
            {
                break;
            }

            if (IsInRange(current, rangeStart, rangeEnd))
            {
                results.Add(current);
            }

            produced++;
            current = next(current);
        }

        return results;
    }

    private static IReadOnlyList<DateTimeOffset> ExpandWeekly(
        DateTimeOffset start,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        int interval,
        int? countLimit,
        DateTimeOffset? until,
        IReadOnlyDictionary<string, string> rule,
        int maxOccurrences)
    {
        var byDay = ParseByDay(rule);
        if (byDay.Count == 0)
        {
            byDay.Add(start.DayOfWeek);
        }

        var results = new List<DateTimeOffset>();
        var produced = 0;
        var weekAnchor = start;

        while (results.Count < maxOccurrences)
        {
            foreach (var day in byDay.OrderBy(x => x))
            {
                var occurrence = AlignToDay(weekAnchor, day);
                if (occurrence < start)
                {
                    continue;
                }

                if (countLimit.HasValue && produced >= countLimit.Value)
                {
                    return results;
                }

                if (until.HasValue && occurrence > until.Value)
                {
                    return results;
                }

                if (occurrence > rangeEnd)
                {
                    return results;
                }

                if (IsInRange(occurrence, rangeStart, rangeEnd))
                {
                    results.Add(occurrence);
                }

                produced++;
            }

            weekAnchor = weekAnchor.AddDays(7 * interval);
        }

        return results;
    }

    private static DateTimeOffset AlignToDay(DateTimeOffset anchor, DayOfWeek day)
    {
        var offset = ((int)day - (int)anchor.DayOfWeek + 7) % 7;
        return anchor.AddDays(offset);
    }

    private static Dictionary<string, string> ParseRule(string rule)
    {
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parts = rule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length == 2)
            {
                dictionary[pair[0]] = pair[1];
            }
        }

        return dictionary;
    }

    private static int? ParseInt(IReadOnlyDictionary<string, string> rule, string key)
    {
        if (!rule.TryGetValue(key, out var value))
        {
            return null;
        }

        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static DateTimeOffset? ParseDate(IReadOnlyDictionary<string, string> rule, string key)
    {
        if (!rule.TryGetValue(key, out var value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;
    }

    private static List<DayOfWeek> ParseByDay(IReadOnlyDictionary<string, string> rule)
    {
        if (!rule.TryGetValue("BYDAY", out var byDay))
        {
            return [];
        }

        var list = new List<DayOfWeek>();
        var tokens = byDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var token in tokens)
        {
            if (token.Length < 2)
            {
                continue;
            }

            var cleaned = token[^2..].ToUpperInvariant();
            if (cleaned == "MO") list.Add(DayOfWeek.Monday);
            else if (cleaned == "TU") list.Add(DayOfWeek.Tuesday);
            else if (cleaned == "WE") list.Add(DayOfWeek.Wednesday);
            else if (cleaned == "TH") list.Add(DayOfWeek.Thursday);
            else if (cleaned == "FR") list.Add(DayOfWeek.Friday);
            else if (cleaned == "SA") list.Add(DayOfWeek.Saturday);
            else if (cleaned == "SU") list.Add(DayOfWeek.Sunday);
        }

        return list;
    }

    private static bool IsInRange(DateTimeOffset value, DateTimeOffset rangeStart, DateTimeOffset rangeEnd)
    {
        return value >= rangeStart && value <= rangeEnd;
    }
}
