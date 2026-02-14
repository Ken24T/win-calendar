using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Infrastructure.Services;

internal sealed class PdfExportService : IPdfExportService
{
    private static bool _licenseConfigured;

    public Task ExportEventsAsync(
        IReadOnlyList<CalendarEvent> events,
        string outputFilePath,
        string documentTitle,
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
                        column.Item().Text($"Generated {DateTimeOffset.Now:dd MMM yyyy HH:mm}").FontColor(Colors.Grey.Darken2);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Title").SemiBold();
                            header.Cell().Element(CellStyle).Text("Start").SemiBold();
                            header.Cell().Element(CellStyle).Text("End").SemiBold();
                            header.Cell().Element(CellStyle).Text("Category").SemiBold();
                        });

                        foreach (var calendarEvent in events.OrderBy(item => item.StartDateTime))
                        {
                            table.Cell().Element(CellStyle).Text(calendarEvent.Title);
                            table.Cell().Element(CellStyle).Text(calendarEvent.StartDisplayLabel);
                            table.Cell().Element(CellStyle).Text(calendarEvent.EndDisplayLabel);
                            table.Cell().Element(CellStyle).Text(calendarEvent.Category);
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
