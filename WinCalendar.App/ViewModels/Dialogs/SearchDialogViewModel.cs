using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class SearchDialogViewModel(IEventSearchService eventSearchService) : ObservableObject
{
    [ObservableProperty]
    private string _query = string.Empty;

    public ObservableCollection<CalendarEvent> Results { get; } = [];

    [RelayCommand]
    private async Task SearchAsync()
    {
        var items = await eventSearchService.SearchAsync(Query);

        Results.Clear();
        foreach (var item in items)
        {
            Results.Add(item);
        }
    }
}
