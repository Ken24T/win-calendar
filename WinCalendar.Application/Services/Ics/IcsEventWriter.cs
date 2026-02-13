using System.Text;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services.Ics;

internal sealed class IcsEventWriter
{
    public string Write(IReadOnlyList<CalendarEvent> events)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//WinCalendar//EN");

        foreach (var calendarEvent in events)
        {
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:{calendarEvent.Id}");
            builder.AppendLine($"DTSTAMP:{IcsLineCodec.ToIcsDateTime(DateTimeOffset.UtcNow)}");
            builder.AppendLine($"DTSTART:{IcsLineCodec.ToIcsDateTime(calendarEvent.StartDateTime)}");
            builder.AppendLine($"DTEND:{IcsLineCodec.ToIcsDateTime(calendarEvent.EndDateTime)}");
            builder.AppendLine($"SUMMARY:{IcsLineCodec.Escape(calendarEvent.Title)}");

            if (!string.IsNullOrWhiteSpace(calendarEvent.Notes))
            {
                builder.AppendLine($"DESCRIPTION:{IcsLineCodec.Escape(calendarEvent.Notes)}");
            }

            if (!string.IsNullOrWhiteSpace(calendarEvent.Location))
            {
                builder.AppendLine($"LOCATION:{IcsLineCodec.Escape(calendarEvent.Location)}");
            }

            if (!string.IsNullOrWhiteSpace(calendarEvent.Category))
            {
                builder.AppendLine($"CATEGORIES:{IcsLineCodec.Escape(calendarEvent.Category)}");
            }

            if (!string.IsNullOrWhiteSpace(calendarEvent.RecurrenceRule))
            {
                builder.AppendLine($"RRULE:{calendarEvent.RecurrenceRule}");
            }

            builder.AppendLine("END:VEVENT");
        }

        builder.AppendLine("END:VCALENDAR");
        return builder.ToString();
    }
}
