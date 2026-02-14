using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application;
using WinCalendar.Application.Contracts;
using WinCalendar.Infrastructure;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Tests;

public class PdfExportServiceTests
{
    [Fact]
    public async Task PdfExportService_Should_Generate_Pdf_File()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "calendar.pdf");

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IPdfExportService>();

        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Planning",
                StartDateTime = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(10)),
                EndDateTime = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.FromHours(10)),
                Category = "Work"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Review",
                StartDateTime = new DateTimeOffset(2026, 3, 2, 14, 0, 0, TimeSpan.FromHours(10)),
                EndDateTime = new DateTimeOffset(2026, 3, 2, 15, 0, 0, TimeSpan.FromHours(10)),
                Category = "Work"
            }
        };

        await service.ExportEventsAsync(events, outputPath, "Test Export");

        Assert.True(File.Exists(outputPath));
        var size = new FileInfo(outputPath).Length;
        Assert.True(size > 0);
    }
}
