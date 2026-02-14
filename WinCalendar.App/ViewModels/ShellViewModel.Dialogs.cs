using System.Windows;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.App.ViewModels.Dialogs;
using WinCalendar.App.Views.Dialogs;
using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

namespace WinCalendar.App.ViewModels;

public partial class ShellViewModel
{
    [RelayCommand]
    private async Task OpenNewEventDialogAsync()
    {
        await OpenEventDialogAsync(null, null, null);
    }

    [RelayCommand]
    private async Task OpenNewEventOnDateDialogAsync(DateTime? date)
    {
        if (!date.HasValue)
        {
            return;
        }

        await OpenEventDialogAsync(null, null, date.Value);
    }

    [RelayCommand]
    private async Task OpenEditEventDialogAsync(CalendarEvent? calendarEvent)
    {
        if (calendarEvent is null)
        {
            return;
        }

        await OpenEventDialogAsync(calendarEvent, null, null);
    }

    [RelayCommand]
    private void OpenSearchDialog()
    {
        var viewModel = new SearchDialogViewModel(_eventSearchService);
        var window = new SearchWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        viewModel.EditEventRequested += async calendarEvent =>
        {
            await OpenEventDialogAsync(calendarEvent, window, null);
            await viewModel.SearchCommand.ExecuteAsync(null);
        };

        window.ShowDialog();
    }

    [RelayCommand]
    private async Task OpenCategoryManagerDialogAsync()
    {
        var viewModel = new CategoryManagerViewModel(_categoryService);
        await viewModel.InitialiseAsync();

        var window = new CategoryManagerWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        window.ShowDialog();
    }

    [RelayCommand]
    private async Task OpenTemplateManagerDialogAsync()
    {
        var viewModel = new TemplateManagerViewModel(_eventTemplateService);
        await viewModel.InitialiseAsync();

        var window = new TemplateManagerWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        window.ShowDialog();
    }

    [RelayCommand]
    private async Task OpenCountdownManagerDialogAsync()
    {
        var viewModel = new CountdownManagerViewModel(_countdownService);
        await viewModel.InitialiseAsync();

        var window = new CountdownManagerWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        window.ShowDialog();
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task ExportCurrentViewPdfAsync()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export Calendar PDF",
            Filter = "PDF files (*.pdf)|*.pdf",
            DefaultExt = "pdf",
            AddExtension = true,
            FileName = $"wincalendar-{DateTimeOffset.Now:yyyyMMdd-HHmm}.pdf"
        };

        if (saveFileDialog.ShowDialog(System.Windows.Application.Current.MainWindow) != true)
        {
            return;
        }

        var (rangeStart, rangeEnd) = GetCurrentRangeBounds();
        var eventsInRange = _allEvents
            .Where(item => item.StartDateTime <= rangeEnd && item.EndDateTime >= rangeStart)
            .OrderBy(item => item.StartDateTime)
            .ToList();

        var title = $"WinCalendar Export - {CurrentRangeLabel}";
        await _pdfExportService.ExportEventsAsync(eventsInRange, saveFileDialog.FileName, title);
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetCurrentRangeBounds()
    {
        var startOfDay = new DateTimeOffset(FocusDate.Date, FocusDate.Offset);

        return ActiveView switch
        {
            CalendarViewType.Day => (startOfDay, startOfDay.AddDays(1).AddTicks(-1)),
            CalendarViewType.Month =>
                (
                    new DateTimeOffset(new DateTime(FocusDate.Year, FocusDate.Month, 1), FocusDate.Offset),
                    new DateTimeOffset(new DateTime(FocusDate.Year, FocusDate.Month, 1), FocusDate.Offset).AddMonths(1).AddTicks(-1)
                ),
            CalendarViewType.WorkWeek =>
                (
                    startOfDay.AddDays(-((7 + (startOfDay.DayOfWeek - DayOfWeek.Monday)) % 7)),
                    startOfDay.AddDays(-((7 + (startOfDay.DayOfWeek - DayOfWeek.Monday)) % 7)).AddDays(5).AddTicks(-1)
                ),
            _ =>
                (
                    startOfDay.AddDays(-((7 + (startOfDay.DayOfWeek - DayOfWeek.Monday)) % 7)),
                    startOfDay.AddDays(-((7 + (startOfDay.DayOfWeek - DayOfWeek.Monday)) % 7)).AddDays(7).AddTicks(-1)
                )
        };
    }

    private async Task OpenEventDialogAsync(CalendarEvent? calendarEvent, Window? owner = null, DateTime? newEventDate = null)
    {
        var viewModel = new EventEditorViewModel(_eventService, _categoryService);

        if (calendarEvent is null)
        {
            if (newEventDate.HasValue)
            {
                await viewModel.InitialiseForNewOnDateAsync(newEventDate.Value);
            }
            else
            {
                await viewModel.InitialiseForNewAsync();
            }
        }
        else
        {
            await viewModel.InitialiseForEditAsync(calendarEvent);
        }

        var window = new EventEditorWindow
        {
            Owner = owner ?? System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        if (window.ShowDialog() == true)
        {
            await RefreshViewAsync();
        }
    }
}
