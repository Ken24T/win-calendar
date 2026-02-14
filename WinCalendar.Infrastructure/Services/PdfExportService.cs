using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

namespace WinCalendar.Infrastructure.Services;

internal sealed class PdfExportService : IPdfExportService
{
    private static bool _licenseConfigured;

    public Task ExportEventsAsync(
        IReadOnlyList<CalendarEvent> events,
        string outputFilePath,
        string documentTitle,
        CalendarViewType viewType,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException("Output file path is required.", nameof(outputFilePath));
        }

        var directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        ConfigureLicense();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text(documentTitle).FontSize(18).SemiBold();
                        column.Item().Text($"View: {GetViewLabel(viewType)}").FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"Range: {rangeStart:dd MMM yyyy} - {rangeEnd:dd MMM yyyy}").FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"Generated {DateTimeOffset.Now:dd MMM yyyy HH:mm}").FontColor(Colors.Grey.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        var sortedEvents = events
                            .OrderBy(item => item.StartDateTime)
                            .ThenBy(item => item.Title)
                            .ToList();

                        if (sortedEvents.Count == 0)
                        {
                            column.Item().Text("No events in this range.").FontColor(Colors.Grey.Darken1);
                            return;
                        }

                        var groupedByDay = sortedEvents
                            .GroupBy(item => item.StartDateTime.Date)
                            .OrderBy(group => group.Key)
                            .ToList();

                        foreach (var dayGroup in groupedByDay)
                        {
                            column.Item().PaddingTop(10).Text(dayGroup.Key.ToString("dddd, dd MMM yyyy")).FontSize(13).SemiBold();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.8f);
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(2.2f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Time").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Title").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Category").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Type").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Details").SemiBold();
                                });

                                foreach (var calendarEvent in dayGroup)
                                {
                                    table.Cell().Element(CellStyle).Text(BuildTimeLabel(calendarEvent));
                                    table.Cell().Element(CellStyle).Text(calendarEvent.Title);
                                    table.Cell().Element(CellStyle).Text(string.IsNullOrWhiteSpace(calendarEvent.Category) ? "Uncategorised" : calendarEvent.Category);
                                    table.Cell().Element(CellStyle).Text(calendarEvent.IsAllDay ? "All day" : "Timed");
                                    table.Cell().Element(CellStyle).Text(BuildDetailsLabel(calendarEvent));
                                }
                            });
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });
            });
        }).GeneratePdf(outputFilePath);

        return Task.CompletedTask;
    }

    private static string GetViewLabel(CalendarViewType viewType)
    {
        return viewType switch
        {
            CalendarViewType.WorkWeek => "Work Week",
            _ => viewType.ToString()
        };
    }

    private static string BuildTimeLabel(CalendarEvent calendarEvent)
    {
        if (calendarEvent.IsAllDay)
        {
            return "All day";
        }

        return $"{calendarEvent.StartDateTime:HH:mm} - {calendarEvent.EndDateTime:HH:mm}";
    }

    private static string BuildDetailsLabel(CalendarEvent calendarEvent)
    {
        var detailParts = new List<string>(2);

        if (!string.IsNullOrWhiteSpace(calendarEvent.Location))
        {
            detailParts.Add($"Location: {calendarEvent.Location.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(calendarEvent.Notes))
        {
            var notes = calendarEvent.Notes.Trim().Replace("\r\n", " ").Replace('\n', ' ');
            if (notes.Length > 120)
            {
                notes = $"{notes[..117]}...";
            }

            detailParts.Add($"Notes: {notes}");
        }

        return detailParts.Count == 0 ? "-" : string.Join(" | ", detailParts);
    }

    private static void ConfigureLicense()
    {
        if (_licenseConfigured)
        {
            return;
        }

        QuestPDF.Settings.License = LicenseType.Community;
        _licenseConfigured = true;
    }
}
