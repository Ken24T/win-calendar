using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

namespace WinCalendar.App.ViewModels;

public partial class ShellViewModel(IEventService eventService) : ObservableObject
{
    [ObservableProperty]
    private string _title = "WinCalendar";

    [ObservableProperty]
    private CalendarViewType _activeView = CalendarViewType.Month;

    public ObservableCollection<CalendarEvent> Events { get; } = [];

    public string ActiveViewLabel => $"View: {ActiveView}";

    [RelayCommand]
    private void SetDayView()
    {
        ActiveView = CalendarViewType.Day;
        OnPropertyChanged(nameof(ActiveViewLabel));
    }

    [RelayCommand]
    private void SetWeekView()
    {
        ActiveView = CalendarViewType.Week;
        OnPropertyChanged(nameof(ActiveViewLabel));
    }

    [RelayCommand]
    private void SetWorkWeekView()
    {
        ActiveView = CalendarViewType.WorkWeek;
        OnPropertyChanged(nameof(ActiveViewLabel));
    }

    [RelayCommand]
    private void SetMonthView()
    {
        ActiveView = CalendarViewType.Month;
        OnPropertyChanged(nameof(ActiveViewLabel));
    }

    [RelayCommand]
    private async Task LoadSampleEventsAsync()
    {
        await eventService.SeedSampleEventsAsync();
        var items = await eventService.GetEventsAsync();

        Events.Clear();
        foreach (var item in items)
        {
            Events.Add(item);
        }
    }
}
