using System.Collections.ObjectModel;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Calendar;

public sealed class WeekDayColumnViewModel
{
    public string Header { get; init; } = string.Empty;

    public ObservableCollection<CalendarEvent> Events { get; init; } = [];
}
