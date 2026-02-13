using System.Windows;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.App.ViewModels.Dialogs;
using WinCalendar.App.Views.Dialogs;

namespace WinCalendar.App.ViewModels;

public partial class ShellViewModel
{
    [RelayCommand]
    private async Task OpenNewEventDialogAsync()
    {
        var viewModel = new EventEditorViewModel(_eventService, _categoryService);
        await viewModel.InitialiseAsync();

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
}
