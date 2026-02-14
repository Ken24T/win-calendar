using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application;
using WinCalendar.Application.Contracts;
using WinCalendar.Infrastructure;
using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

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
                Category = "Work",
                Location = "Meeting Room 3",
                Notes = "Quarterly planning session with delivery milestones and dependency review."
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Review",
                StartDateTime = new DateTimeOffset(2026, 3, 2, 14, 0, 0, TimeSpan.FromHours(10)),
                EndDateTime = new DateTimeOffset(2026, 3, 2, 15, 0, 0, TimeSpan.FromHours(10)),
                Category = "Work",
                Notes = "Review follow-up actions and update schedule."
            }
        };

        await service.ExportEventsAsync(
            events,
            outputPath,
            "Test Export",
            CalendarViewType.Week,
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.FromHours(10)),
            new DateTimeOffset(2026, 3, 7, 23, 59, 0, TimeSpan.FromHours(10)));

        Assert.True(File.Exists(outputPath));
        var size = new FileInfo(outputPath).Length;
        Assert.True(size > 0);
    }

    [Fact]
    public async Task PdfExportService_Should_Generate_Pdf_File_For_Empty_Event_List()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "empty-calendar.pdf");

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IPdfExportService>();

        await service.ExportEventsAsync(
            [],
            outputPath,
            "Empty Export",
            CalendarViewType.Month,
            new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(10)),
            new DateTimeOffset(2026, 4, 30, 23, 59, 0, TimeSpan.FromHours(10)));

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }

    [Fact]
    public async Task PdfExportService_Should_Generate_Pdf_File_With_Long_Multiline_Notes()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "long-notes-calendar.pdf");

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IPdfExportService>();

        var longNotes = string.Join(
            Environment.NewLine,
            Enumerable.Repeat("This is a detailed planning note entry for export validation.", 8));

        var events = new List<CalendarEvent>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Long Notes Event",
                StartDateTime = new DateTimeOffset(2026, 5, 5, 8, 30, 0, TimeSpan.FromHours(10)),
                EndDateTime = new DateTimeOffset(2026, 5, 5, 9, 30, 0, TimeSpan.FromHours(10)),
                Category = "Work",
                Location = "Studio 2",
                Notes = longNotes
            }
        };

        await service.ExportEventsAsync(
            events,
            outputPath,
            "Long Notes Export",
            CalendarViewType.Day,
            new DateTimeOffset(2026, 5, 5, 0, 0, 0, TimeSpan.FromHours(10)),
            new DateTimeOffset(2026, 5, 5, 23, 59, 0, TimeSpan.FromHours(10)));

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }

    [Fact]
    public async Task PdfExportService_Should_Generate_Pdf_For_Dense_MultiDay_Schedule()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "dense-multiday.pdf");

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IPdfExportService>();

        var offset = TimeSpan.FromHours(10);
        var events = new List<CalendarEvent>();
        var categories = new[] { "Work", "Admin", "Personal", "Health" };

        for (var dayIndex = 0; dayIndex < 5; dayIndex++)
        {
            var day = new DateTimeOffset(2026, 6, 1 + dayIndex, 0, 0, 0, offset);

            events.Add(new CalendarEvent
            {
                Id = Guid.NewGuid(),
                Title = $"All-day {dayIndex + 1}",
                StartDateTime = day,
                EndDateTime = day.AddHours(23).AddMinutes(59),
                IsAllDay = true,
                Category = categories[dayIndex % categories.Length]
            });

            for (var slot = 0; slot < 4; slot++)
            {
                var start = day.AddHours(8 + (slot * 2));
                events.Add(new CalendarEvent
                {
                    Id = Guid.NewGuid(),
                    Title = $"Dense event {dayIndex + 1}-{slot + 1}",
                    StartDateTime = start,
                    EndDateTime = start.AddMinutes(75),
                    Category = categories[(dayIndex + slot) % categories.Length],
                    Location = $"Room {dayIndex + 1}{slot + 1}",
                    Notes = "Dense schedule regression case"
                });
            }
        }

        await service.ExportEventsAsync(
            events,
            outputPath,
            "Dense Multi-day Export",
            CalendarViewType.Week,
            new DateTimeOffset(2026, 6, 1, 0, 0, 0, offset),
            new DateTimeOffset(2026, 6, 7, 23, 59, 0, offset));

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }

    [Fact]
    public async Task PdfExportService_Should_Generate_Pdf_When_Start_And_Title_Tie()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), "wincalendar-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "tie-order.pdf");

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IPdfExportService>();

        var offset = TimeSpan.FromHours(10);
        var start = new DateTimeOffset(2026, 7, 2, 9, 0, 0, offset);
        var cardA = new CalendarEvent
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Same title",
            StartDateTime = start,
            EndDateTime = start.AddHours(1),
            Category = "Work"
        };
        var cardB = new CalendarEvent
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Same title",
            StartDateTime = start,
            EndDateTime = start.AddHours(1),
            Category = "Work"
        };

        await service.ExportEventsAsync(
            [cardB, cardA],
            outputPath,
            "Tie Export",
            CalendarViewType.Day,
            start.Date,
            start.Date.AddDays(1).AddTicks(-1));

        Assert.True(File.Exists(outputPath));
        Assert.True(new FileInfo(outputPath).Length > 0);
    }
}
