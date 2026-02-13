using System.Collections.ObjectModel;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Calendar;

public sealed class MonthDayCellViewModel
{
    public DateTime Date { get; init; }

    public bool IsCurrentMonth { get; init; }

    public bool IsToday { get; init; }

    public ObservableCollection<CalendarEvent> Events { get; init; } = [];
}
