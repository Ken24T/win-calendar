using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class EventEditorViewModel : ObservableObject, IDialogRequestClose
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private Guid _eventId;

    public event EventHandler<bool?>? RequestClose;

    public EventEditorViewModel(IEventService eventService, ICategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _startDateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private string _startTimeText = DateTimeOffset.Now.ToString("HH:mm");

    [ObservableProperty]
    private DateTimeOffset _endDateTime = DateTimeOffset.Now.AddHours(1);

    [ObservableProperty]
    private string _endTimeText = DateTimeOffset.Now.AddHours(1).ToString("HH:mm");

    [ObservableProperty]
    private bool _isAllDay;

    [ObservableProperty]
    private string _category = "General";

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _recurrenceRule;

    [ObservableProperty]
    private string _recurrenceExceptionsText = string.Empty;

    [ObservableProperty]
    private bool _canDelete;

    public ObservableCollection<string> Categories { get; } = [];

    public async Task InitialiseForNewAsync()
    {
        _eventId = Guid.NewGuid();
        CanDelete = false;

        Title = string.Empty;
        StartDateTime = DateTimeOffset.Now;
        EndDateTime = DateTimeOffset.Now.AddHours(1);
        IsAllDay = false;
        Category = "General";
        Location = null;
        Notes = null;
        RecurrenceRule = null;
        RecurrenceExceptionsText = string.Empty;
        StartTimeText = StartDateTime.ToString("HH:mm");
        EndTimeText = EndDateTime.ToString("HH:mm");

        await LoadCategoriesAsync();
    }

    public async Task InitialiseForNewOnDateAsync(DateTime date)
    {
        _eventId = Guid.NewGuid();
        CanDelete = false;

        var offset = DateTimeOffset.Now.Offset;
        StartDateTime = new DateTimeOffset(date.Year, date.Month, date.Day, 9, 0, 0, offset);
        EndDateTime = StartDateTime.AddHours(1);
        Title = string.Empty;
        IsAllDay = false;
        Category = "General";
        Location = null;
        Notes = null;
        RecurrenceRule = null;
        RecurrenceExceptionsText = string.Empty;
        StartTimeText = StartDateTime.ToString("HH:mm");
        EndTimeText = EndDateTime.ToString("HH:mm");

        await LoadCategoriesAsync();
    }

    public async Task InitialiseForEditAsync(CalendarEvent calendarEvent)
    {
        _eventId = calendarEvent.Id;
        CanDelete = true;

        Title = calendarEvent.Title;
        StartDateTime = calendarEvent.StartDateTime;
        EndDateTime = calendarEvent.EndDateTime;
        IsAllDay = calendarEvent.IsAllDay;
        Category = calendarEvent.Category;
        Location = calendarEvent.Location;
        Notes = calendarEvent.Notes;
        RecurrenceRule = calendarEvent.RecurrenceRule;
        RecurrenceExceptionsText = FormatRecurrenceExceptions(calendarEvent.RecurrenceExceptions);
        StartTimeText = StartDateTime.ToString("HH:mm");
        EndTimeText = EndDateTime.ToString("HH:mm");

        await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories.Clear();
        var categories = await _categoryService.GetCategoriesAsync();
        foreach (var item in categories.OrderBy(x => x.Name))
        {
            Categories.Add(item.Name);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        if (!TryParseTime(StartTimeText, out var startTime) || !TryParseTime(EndTimeText, out var endTime))
        {
            return;
        }

        var start = IsAllDay
            ? new DateTimeOffset(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, 0, 0, 0, StartDateTime.Offset)
            : new DateTimeOffset(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, startTime.Hours, startTime.Minutes, 0, StartDateTime.Offset);

        var end = IsAllDay
            ? new DateTimeOffset(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, 23, 59, 0, EndDateTime.Offset)
            : new DateTimeOffset(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, endTime.Hours, endTime.Minutes, 0, EndDateTime.Offset);

        if (end < start)
        {
            return;
        }

        await _eventService.SaveEventAsync(new CalendarEvent
        {
            Id = _eventId,
            Title = Title.Trim(),
            StartDateTime = start,
            EndDateTime = end,
            IsAllDay = IsAllDay,
            Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            RecurrenceRule = string.IsNullOrWhiteSpace(RecurrenceRule) ? null : RecurrenceRule.Trim(),
            RecurrenceExceptions = ParseRecurrenceExceptions(RecurrenceExceptionsText)
        });

        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!CanDelete)
        {
            return;
        }

        await _eventService.DeleteEventAsync(_eventId);
        RequestClose?.Invoke(this, true);
    }

    private IReadOnlyList<DateTimeOffset> ParseRecurrenceExceptions(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var lines = value
            .Split(['\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var timeOfDay = TimeOnly.FromDateTime(StartDateTime.DateTime);
        var items = new List<DateTimeOffset>();

        foreach (var line in lines)
        {
            if (DateTimeOffset.TryParse(line, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
            {
                items.Add(dateTimeOffset);
                continue;
            }

            if (DateOnly.TryParse(line, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
            {
                var localDateTime = dateOnly.ToDateTime(timeOfDay, DateTimeKind.Unspecified);
                items.Add(new DateTimeOffset(localDateTime, StartDateTime.Offset));
            }
        }

        return items
            .Distinct()
            .OrderBy(item => item)
            .ToList();
    }

    private static string FormatRecurrenceExceptions(IReadOnlyList<DateTimeOffset> recurrenceExceptions)
    {
        if (recurrenceExceptions.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            recurrenceExceptions
                .OrderBy(item => item)
                .Select(item => item.ToString("yyyy-MM-dd"))
                .Distinct(StringComparer.Ordinal));
    }

    private static bool TryParseTime(string? value, out TimeSpan time)
    {
        var formats = new[] { "H:mm", "HH:mm", "h:mm tt", "hh:mm tt" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            time = parsed.TimeOfDay;
            return true;
        }

        time = default;
        return false;
    }

    partial void OnIsAllDayChanged(bool value)
    {
        if (value)
        {
            StartTimeText = "00:00";
            EndTimeText = "23:59";
            return;
        }

        if (string.IsNullOrWhiteSpace(StartTimeText))
        {
            StartTimeText = "09:00";
        }

        if (string.IsNullOrWhiteSpace(EndTimeText))
        {
            EndTimeText = "10:00";
        }
    }
}
