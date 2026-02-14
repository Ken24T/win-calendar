using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.App.Theming;
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
    private readonly IPdfExportService _pdfExportService;
    private readonly ISettingsService _settingsService;

    private const string ThemeKey = "theme";
    private const string UseSystemThemeKey = "use_system_theme";
    private const string ShowSidebarKey = "show_sidebar";

    private IReadOnlyList<CalendarEvent> _allEvents = [];

    public ShellViewModel(
        IEventService eventService,
        IEventSearchService eventSearchService,
        ICategoryService categoryService,
        IEventTemplateService eventTemplateService,
        ICountdownService countdownService,
        IPdfExportService pdfExportService,
        ISettingsService settingsService)
    {
        _eventService = eventService;
        _eventSearchService = eventSearchService;
        _categoryService = categoryService;
        _eventTemplateService = eventTemplateService;
        _countdownService = countdownService;
        _pdfExportService = pdfExportService;
        _settingsService = settingsService;
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

    [ObservableProperty]
    private AppThemeMode _themeMode = AppThemeMode.System;

    [ObservableProperty]
    private bool _isSidebarVisible = true;

    public ObservableCollection<CalendarEvent> DayEvents { get; } = [];

    public ObservableCollection<WeekDayColumnViewModel> WeekColumns { get; } = [];

    public ObservableCollection<MonthDayCellViewModel> MonthCells { get; } = [];

    public ObservableCollection<CountdownCardItemViewModel> CountdownCards { get; } = [];

    public ObservableCollection<SidebarEventItemViewModel> SidebarTodayAgenda { get; } = [];

    public ObservableCollection<SidebarEventItemViewModel> SidebarUpcomingEvents { get; } = [];

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

    public bool IsLightThemeSelected => ThemeMode == AppThemeMode.Light;

    public bool IsDarkThemeSelected => ThemeMode == AppThemeMode.Dark;

    public bool IsSystemThemeSelected => ThemeMode == AppThemeMode.System;

    public string EmptyStateMessage => ActiveView switch
    {
        CalendarViewType.Day => "No events for this day.",
        CalendarViewType.Month => "No events for this month.",
        _ => "No events for this range."
    };

    public DateTime SidebarDisplayDate => FocusDate.Date;

    public DateTime SidebarDisplayMonth => new(FocusDate.Year, FocusDate.Month, 1);

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
    private async Task SetThemeLightAsync()
    {
        await SetThemeModeAsync(AppThemeMode.Light);
    }

    [RelayCommand]
    private async Task SetThemeDarkAsync()
    {
        await SetThemeModeAsync(AppThemeMode.Dark);
    }

    [RelayCommand]
    private async Task SetThemeSystemAsync()
    {
        await SetThemeModeAsync(AppThemeMode.System);
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
            BuildSidebarSections();
        }
        catch
        {
            _allEvents = [];
            CountdownCards.Clear();
            SidebarTodayAgenda.Clear();
            SidebarUpcomingEvents.Clear();
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

    [RelayCommand]
    private async Task LoadPhase3VerificationDataAsync()
    {
        var now = DateTimeOffset.Now;

        foreach (var calendarEvent in BuildPhase3VerificationEvents(now))
        {
            await _eventService.SaveEventAsync(calendarEvent);
        }

        foreach (var countdownCard in BuildPhase3VerificationCountdownCards(now))
        {
            await _countdownService.SaveCountdownCardAsync(countdownCard);
        }

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
                .ToList();

            var visibleEvents = events
                .Take(3)
                .ToList();

            var hiddenCount = Math.Max(0, events.Count - visibleEvents.Count);

            MonthCells.Add(new MonthDayCellViewModel
            {
                Date = cellDate,
                IsCurrentMonth = cellDate.Month == firstDayOfMonth.Month,
                IsToday = cellDate == DateTime.Today,
                Events = new ObservableCollection<CalendarEvent>(visibleEvents),
                HiddenEventsLabel = hiddenCount > 0 ? $"+{hiddenCount} more" : string.Empty
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

    private static IReadOnlyList<CalendarEvent> BuildPhase3VerificationEvents(DateTimeOffset now)
    {
        var startOfWeek = GetStartOfWeek(now.Date);
        var monday = new DateTimeOffset(startOfWeek, now.Offset);

        var events = new List<CalendarEvent>();
        var categories = new[] { "Work", "Admin", "Personal", "Health" };

        for (var dayIndex = 0; dayIndex < 5; dayIndex++)
        {
            var day = monday.AddDays(dayIndex);

            events.Add(new CalendarEvent
            {
                Id = Guid.Parse($"10000000-0000-0000-0000-0000000000{dayIndex + 1:00}"),
                Title = $"All-day milestone {day:ddd}",
                StartDateTime = new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, day.Offset),
                EndDateTime = new DateTimeOffset(day.Year, day.Month, day.Day, 23, 59, 0, day.Offset),
                IsAllDay = true,
                Category = categories[dayIndex % categories.Length]
            });

            for (var slot = 0; slot < 4; slot++)
            {
                var startHour = 8 + (slot * 2);
                var start = new DateTimeOffset(day.Year, day.Month, day.Day, startHour, 0, 0, day.Offset);
                var end = start.AddMinutes(75);

                events.Add(new CalendarEvent
                {
                    Id = Guid.Parse($"20000000-0000-0000-0000-00000000{dayIndex + 1:00}{slot + 1:00}"),
                    Title = $"Dense schedule {day:ddd} #{slot + 1}",
                    StartDateTime = start,
                    EndDateTime = end,
                    Category = categories[(dayIndex + slot) % categories.Length],
                    Location = $"Room {(dayIndex + 1) * 10 + slot}",
                    Notes = "Verification event for PDF dense-layout parity checks."
                });
            }
        }

        events.Add(new CalendarEvent
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Title = "Long title parity event - this title is intentionally very long to validate column wrapping and readability in exported PDF files",
            StartDateTime = monday.AddDays(2).AddHours(15),
            EndDateTime = monday.AddDays(2).AddHours(16),
            Category = "Work",
            Location = "Conference Room Long-Form",
            Notes = string.Join("\n", Enumerable.Repeat("Long notes content for Phase 3 parity validation against Rust PDF wrapping behaviour.", 8))
        });

        return events;
    }

    private static IReadOnlyList<CountdownCard> BuildPhase3VerificationCountdownCards(DateTimeOffset now)
    {
        return
        [
            new CountdownCard
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
                Title = "P3 Near-due 50h",
                TargetDateTime = now.AddHours(50),
                ColourHex = "#2D6CDF",
                IsActive = true,
                SortOrder = 10
            },
            new CountdownCard
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000002"),
                Title = "P3 Near-due 26h",
                TargetDateTime = now.AddHours(26),
                ColourHex = "#0C8A43",
                IsActive = true,
                SortOrder = 10
            },
            new CountdownCard
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000003"),
                Title = "P3 Due soon 45m",
                TargetDateTime = now.AddMinutes(45),
                ColourHex = "#A07000",
                IsActive = true,
                SortOrder = 10
            },
            new CountdownCard
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000004"),
                Title = "P3 Overdue 20m",
                TargetDateTime = now.AddMinutes(-20),
                ColourHex = "#B42318",
                IsActive = true,
                SortOrder = 10
            },
            new CountdownCard
            {
                Id = Guid.Parse("40000000-0000-0000-0000-000000000005"),
                Title = "P3 Inactive tie",
                TargetDateTime = now.AddHours(26),
                ColourHex = "#6941C6",
                IsActive = false,
                SortOrder = 10
            }
        ];
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
        OnPropertyChanged(nameof(SidebarDisplayDate));
        OnPropertyChanged(nameof(SidebarDisplayMonth));
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

    partial void OnThemeModeChanged(AppThemeMode value)
    {
        OnPropertyChanged(nameof(IsLightThemeSelected));
        OnPropertyChanged(nameof(IsDarkThemeSelected));
        OnPropertyChanged(nameof(IsSystemThemeSelected));
    }

    partial void OnIsSidebarVisibleChanged(bool value)
    {
        _ = PersistSidebarVisibilityAsync(value);
    }

    public async Task InitialiseAsync()
    {
        await LoadAndApplyDisplaySettingsAsync();
        await RefreshViewAsync();
    }

    private async Task SetThemeModeAsync(AppThemeMode mode)
    {
        ThemeMode = mode;
        ThemeRuntime.Apply(mode);
        await PersistThemeModeAsync(mode);
    }

    private async Task LoadAndApplyDisplaySettingsAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();

        var useSystemValue = settings
            .FirstOrDefault(item => string.Equals(item.Key, UseSystemThemeKey, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        var useSystemTheme = string.Equals(useSystemValue, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(useSystemValue, "true", StringComparison.OrdinalIgnoreCase);

        var mode = AppThemeMode.System;
        if (!useSystemTheme)
        {
            var persistedTheme = settings
                .FirstOrDefault(item => string.Equals(item.Key, ThemeKey, StringComparison.OrdinalIgnoreCase))
                ?.Value;

            mode = ThemeRuntime.Parse(persistedTheme);
            if (mode == AppThemeMode.System)
            {
                mode = AppThemeMode.Light;
            }
        }

        ThemeMode = mode;
        ThemeRuntime.Apply(mode);

        var sidebarValue = settings
            .FirstOrDefault(item => string.Equals(item.Key, ShowSidebarKey, StringComparison.OrdinalIgnoreCase))
            ?.Value;

        IsSidebarVisible = !string.Equals(sidebarValue, "0", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sidebarValue, "false", StringComparison.OrdinalIgnoreCase);
    }

    private async Task PersistThemeModeAsync(AppThemeMode mode)
    {
        await _settingsService.SaveSettingAsync(new AppSetting
        {
            Key = UseSystemThemeKey,
            Value = mode == AppThemeMode.System ? "1" : "0"
        });

        if (mode == AppThemeMode.System)
        {
            return;
        }

        await _settingsService.SaveSettingAsync(new AppSetting
        {
            Key = ThemeKey,
            Value = ThemeRuntime.ToPersistedValue(mode)
        });
    }

    private async Task PersistSidebarVisibilityAsync(bool isVisible)
    {
        await _settingsService.SaveSettingAsync(new AppSetting
        {
            Key = ShowSidebarKey,
            Value = isVisible ? "1" : "0"
        });
    }

    private void BuildSidebarSections()
    {
        SidebarTodayAgenda.Clear();
        SidebarUpcomingEvents.Clear();

        var now = DateTimeOffset.Now;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);

        var todayItems = _allEvents
            .Where(item => IsOverlapping(item, todayStart, todayEnd))
            .OrderBy(item => item.StartDateTime)
            .ThenBy(item => item.Title)
            .Take(8)
            .ToList();

        foreach (var item in todayItems)
        {
            SidebarTodayAgenda.Add(new SidebarEventItemViewModel
            {
                Title = item.Title,
                Category = item.Category,
                TimeLabel = item.IsAllDay ? "All day" : item.StartDateTime.ToString("HH:mm")
            });
        }

        var upcomingItems = _allEvents
            .Where(item => item.StartDateTime > now)
            .OrderBy(item => item.StartDateTime)
            .ThenBy(item => item.Title)
            .Take(10)
            .ToList();

        foreach (var item in upcomingItems)
        {
            SidebarUpcomingEvents.Add(new SidebarEventItemViewModel
            {
                Title = item.Title,
                Category = item.Category,
                TimeLabel = item.IsAllDay
                    ? item.StartDateTime.ToString("ddd dd MMM")
                    : item.StartDateTime.ToString("ddd dd MMM HH:mm")
            });
        }
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

        foreach (var item in items)
        {
            var viewModel = new CountdownCardItemViewModel
            {
                CardId = item.Id,
                Title = item.Title,
                TargetDateTime = item.TargetDateTime,
                TargetDateLabel = item.TargetDateTime.ToString("ddd dd MMM yyyy HH:mm"),
                ColourHex = item.ColourHex,
                SortOrder = item.SortOrder
            };

            viewModel.UpdateRemainingLabel(DateTimeOffset.Now);
            CountdownCards.Add(viewModel);
        }

        ApplyCountdownOrdering();

        OnPropertyChanged(nameof(HasCountdownCards));
    }

    public void RefreshCountdownLabels()
    {
        var now = DateTimeOffset.Now;
        foreach (var card in CountdownCards)
        {
            card.UpdateRemainingLabel(now);
        }

        ApplyCountdownOrdering();
    }

    private void ApplyCountdownOrdering()
    {
        var ordered = CountdownCards
            .OrderBy(item => item.PriorityRank)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.TargetDateTime)
            .ThenBy(item => item.Title)
            .ThenBy(item => item.CardId)
            .ToList();

        if (ordered.Count == CountdownCards.Count && ordered.SequenceEqual(CountdownCards))
        {
            return;
        }

        CountdownCards.Clear();
        foreach (var item in ordered)
        {
            CountdownCards.Add(item);
        }
    }
}
