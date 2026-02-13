using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.App.ViewModels.Calendar;
using WinCalendar.App.ViewModels.Countdown;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;
using WinCalendar.Domain.Enums;

namespace WinCalendar.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IEventService _eventService;
    private readonly IEventSearchService _eventSearchService;
    private readonly ICategoryService _categoryService;
    private readonly IEventTemplateService _eventTemplateService;
    private readonly ICountdownService _countdownService;

    private IReadOnlyList<CalendarEvent> _allEvents = [];

    public ShellViewModel(
        IEventService eventService,
        IEventSearchService eventSearchService,
        ICategoryService categoryService,
        IEventTemplateService eventTemplateService,
        ICountdownService countdownService)
    {
        _eventService = eventService;
        _eventSearchService = eventSearchService;
        _categoryService = categoryService;
        _eventTemplateService = eventTemplateService;
        _countdownService = countdownService;
    }

    [ObservableProperty]
    private string _title = "WinCalendar";

    [ObservableProperty]
    private CalendarViewType _activeView = CalendarViewType.Month;

    [ObservableProperty]
    private DateTimeOffset _focusDate = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CalendarEvent> DayEvents { get; } = [];

    public ObservableCollection<WeekDayColumnViewModel> WeekColumns { get; } = [];

    public ObservableCollection<MonthDayCellViewModel> MonthCells { get; } = [];

    public ObservableCollection<CountdownCardItemViewModel> CountdownCards { get; } = [];

    public string ActiveViewLabel => $"View: {ActiveView}";

    public string CurrentRangeLabel => BuildCurrentRangeLabel();

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasVisibleEvents => ActiveView switch
    {
        CalendarViewType.Day => DayEvents.Count > 0,
        CalendarViewType.Month => MonthCells.Any(cell => cell.Events.Count > 0),
        _ => WeekColumns.Any(column => column.Events.Count > 0)
    };

    public bool ShowLoadingState => IsLoading;

    public bool ShowErrorState => HasError;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasVisibleEvents;

    public bool HasCountdownCards => CountdownCards.Count > 0;

    public string EmptyStateMessage => ActiveView switch
    {
        CalendarViewType.Day => "No events for this day.",
        CalendarViewType.Month => "No events for this month.",
        _ => "No events for this range."
    };

    [RelayCommand]
    private async Task PreviousRangeAsync()
    {
        FocusDate = ActiveView switch
        {
            CalendarViewType.Day => FocusDate.AddDays(-1),
            CalendarViewType.Month => FocusDate.AddMonths(-1),
            _ => FocusDate.AddDays(-7)
        };

        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task NextRangeAsync()
    {
        FocusDate = ActiveView switch
        {
            CalendarViewType.Day => FocusDate.AddDays(1),
            CalendarViewType.Month => FocusDate.AddMonths(1),
            _ => FocusDate.AddDays(7)
        };

        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        FocusDate = DateTimeOffset.Now;
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task SetDayViewAsync()
    {
        ActiveView = CalendarViewType.Day;
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task SetWeekViewAsync()
    {
        ActiveView = CalendarViewType.Week;
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task SetWorkWeekViewAsync()
    {
        ActiveView = CalendarViewType.WorkWeek;
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task SetMonthViewAsync()
    {
        ActiveView = CalendarViewType.Month;
        await RefreshViewAsync();
    }

    [RelayCommand]
    private async Task RefreshViewAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            _allEvents = await _eventService.GetEventsAsync();
            await LoadCountdownCardsAsync();

            BuildDayEvents();
            BuildWeekColumns();
            BuildMonthCells();
        }
        catch
        {
            _allEvents = [];
            CountdownCards.Clear();
            BuildDayEvents();
            BuildWeekColumns();
            BuildMonthCells();
            ErrorMessage = "Unable to load calendar events. Please try again.";
        }
        finally
        {
            IsLoading = false;
            NotifyViewStateChanged();
        }

        OnPropertyChanged(nameof(ActiveViewLabel));
        OnPropertyChanged(nameof(CurrentRangeLabel));
    }

    [RelayCommand]
    private async Task LoadSampleEventsAsync()
    {
        await _eventService.SeedSampleEventsAsync();
        await RefreshViewAsync();
    }

    private void BuildDayEvents()
    {
        DayEvents.Clear();

        var dayStart = FocusDate.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

        var items = _allEvents
            .Where(item => IsOverlapping(item, dayStart, dayEnd))
            .OrderByDescending(item => item.IsAllDay)
            .ThenBy(item => item.StartDateTime)
            .ThenBy(item => item.Title)
            .ToList();

        foreach (var item in items)
        {
            DayEvents.Add(item);
        }
    }

    private void BuildWeekColumns()
    {
        WeekColumns.Clear();

        var start = GetStartOfWeek(FocusDate.Date);
        var days = ActiveView == CalendarViewType.WorkWeek ? 5 : 7;

        for (var dayIndex = 0; dayIndex < days; dayIndex++)
        {
            var date = start.AddDays(dayIndex);
            var dayStart = new DateTimeOffset(date, FocusDate.Offset);
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var events = _allEvents
                .Where(item => IsOverlapping(item, dayStart, dayEnd))
                .OrderByDescending(item => item.IsAllDay)
                .ThenBy(item => item.StartDateTime)
                .ThenBy(item => item.Title)
                .ToList();

            WeekColumns.Add(new WeekDayColumnViewModel
            {
                Header = dayStart.ToString("ddd dd MMM"),
                Events = new ObservableCollection<CalendarEvent>(events)
            });
        }
    }

    private void BuildMonthCells()
    {
        MonthCells.Clear();

        var focusDay = FocusDate.Date;
        var firstDayOfMonth = new DateTime(focusDay.Year, focusDay.Month, 1);
        var gridStart = GetStartOfWeek(firstDayOfMonth);

        for (var cellIndex = 0; cellIndex < 42; cellIndex++)
        {
            var cellDate = gridStart.AddDays(cellIndex);
            var cellStart = new DateTimeOffset(cellDate, FocusDate.Offset);
            var cellEnd = cellStart.AddDays(1).AddTicks(-1);

            var events = _allEvents
                .Where(item => IsOverlapping(item, cellStart, cellEnd))
                .OrderByDescending(item => item.IsAllDay)
                .ThenBy(item => item.StartDateTime)
                .ThenBy(item => item.Title)
                .Take(3)
                .ToList();

            MonthCells.Add(new MonthDayCellViewModel
            {
                Date = cellDate,
                IsCurrentMonth = cellDate.Month == firstDayOfMonth.Month,
                IsToday = cellDate == DateTime.Today,
                Events = new ObservableCollection<CalendarEvent>(events)
            });
        }
    }

    private string BuildCurrentRangeLabel()
    {
        return ActiveView switch
        {
            CalendarViewType.Day => FocusDate.ToString("dddd, dd MMMM yyyy"),
            CalendarViewType.Month => FocusDate.ToString("MMMM yyyy"),
            CalendarViewType.WorkWeek => BuildWeekRangeLabel(5),
            _ => BuildWeekRangeLabel(7)
        };
    }

    private string BuildWeekRangeLabel(int length)
    {
        var start = new DateTimeOffset(GetStartOfWeek(FocusDate.Date), FocusDate.Offset);
        var end = start.AddDays(length - 1);
        return $"{start:dd MMM} - {end:dd MMM yyyy}";
    }

    private static bool IsOverlapping(CalendarEvent calendarEvent, DateTimeOffset start, DateTimeOffset end)
    {
        return calendarEvent.StartDateTime <= end && calendarEvent.EndDateTime >= start;
    }

    private static DateTime GetStartOfWeek(DateTime value)
    {
        var offset = (7 + (value.DayOfWeek - DayOfWeek.Monday)) % 7;
        return value.AddDays(-offset).Date;
    }

    partial void OnFocusDateChanged(DateTimeOffset value)
    {
        OnPropertyChanged(nameof(CurrentRangeLabel));
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    partial void OnActiveViewChanged(CalendarViewType value)
    {
        OnPropertyChanged(nameof(ActiveViewLabel));
        OnPropertyChanged(nameof(CurrentRangeLabel));
        OnPropertyChanged(nameof(EmptyStateMessage));
        NotifyViewStateChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        NotifyViewStateChanged();
    }

    partial void OnErrorMessageChanged(string? value)
    {
        NotifyViewStateChanged();
    }

    public async Task InitialiseAsync()
    {
        await RefreshViewAsync();
    }

    private void NotifyViewStateChanged()
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasVisibleEvents));
        OnPropertyChanged(nameof(HasCountdownCards));
        OnPropertyChanged(nameof(ShowLoadingState));
        OnPropertyChanged(nameof(ShowErrorState));
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    private async Task LoadCountdownCardsAsync()
    {
        CountdownCards.Clear();

        var items = await _countdownService.GetCountdownCardsAsync();
        var now = DateTimeOffset.Now;

        foreach (var item in items)
        {
            CountdownCards.Add(new CountdownCardItemViewModel
            {
                Title = item.Title,
                TargetDateLabel = item.TargetDateTime.ToString("ddd dd MMM yyyy HH:mm"),
                RemainingLabel = item.BuildRemainingLabel(now),
                ColourHex = item.ColourHex
            });
        }

        OnPropertyChanged(nameof(HasCountdownCards));
    }
}
