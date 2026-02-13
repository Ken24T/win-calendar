using System.Windows;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.App.ViewModels.Dialogs;
using WinCalendar.App.Views.Dialogs;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels;

public partial class ShellViewModel
{
    [RelayCommand]
    private async Task OpenNewEventDialogAsync()
    {
        await OpenEventDialogAsync(null);
    }

    [RelayCommand]
    private async Task OpenEditEventDialogAsync(CalendarEvent? calendarEvent)
    {
        if (calendarEvent is null)
        {
            return;
        }

        await OpenEventDialogAsync(calendarEvent);
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

    private async Task OpenEventDialogAsync(CalendarEvent? calendarEvent)
    {
        var viewModel = new EventEditorViewModel(_eventService, _categoryService);

        if (calendarEvent is null)
        {
            await viewModel.InitialiseForNewAsync();
        }
        else
        {
            await viewModel.InitialiseForEditAsync(calendarEvent);
        }

        var window = new EventEditorWindow
        {
            Owner = System.Windows.Application.Current.MainWindow,
            DataContext = viewModel
        };

        if (window.ShowDialog() == true)
        {
            await RefreshViewAsync();
        }
    }
}
