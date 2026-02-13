using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class EventEditorViewModel
{
    private static readonly IReadOnlyList<string> FrequencyOptions = ["None", "Daily", "Weekly", "Monthly", "Yearly"];

    [ObservableProperty]
    private string _recurrenceFrequency = "None";

    [ObservableProperty]
    private string _recurrenceIntervalText = "1";

    [ObservableProperty]
    private string _recurrenceCountText = string.Empty;

    [ObservableProperty]
    private DateTime? _recurrenceUntilDate;

    [ObservableProperty]
    private string _recurrenceMonthDaysText = string.Empty;

    [ObservableProperty]
    private bool _byMonday;

    [ObservableProperty]
    private bool _byTuesday;

    [ObservableProperty]
    private bool _byWednesday;

    [ObservableProperty]
    private bool _byThursday;

    [ObservableProperty]
    private bool _byFriday;

    [ObservableProperty]
    private bool _bySaturday;

    [ObservableProperty]
    private bool _bySunday;

    public ObservableCollection<string> RecurrenceFrequencyOptions { get; } = [.. FrequencyOptions];

    public bool ShowWeeklyBuilder => string.Equals(RecurrenceFrequency, "Weekly", StringComparison.OrdinalIgnoreCase);

    public bool ShowMonthlyBuilder => string.Equals(RecurrenceFrequency, "Monthly", StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private void ApplyRecurrenceBuilder()
    {
        var frequency = RecurrenceFrequency?.Trim();
        if (string.IsNullOrWhiteSpace(frequency) || string.Equals(frequency, "None", StringComparison.OrdinalIgnoreCase))
        {
            RecurrenceRule = null;
            return;
        }

        var parts = new List<string>
        {
            $"FREQ={frequency.ToUpperInvariant()}"
        };

        if (int.TryParse(RecurrenceIntervalText, out var interval) && interval > 1)
        {
            parts.Add($"INTERVAL={interval}");
        }

        if (int.TryParse(RecurrenceCountText, out var count) && count > 0)
        {
            parts.Add($"COUNT={count}");
        }
        else if (RecurrenceUntilDate.HasValue)
        {
            parts.Add($"UNTIL={RecurrenceUntilDate.Value:yyyy-MM-dd}");
        }

        if (string.Equals(frequency, "Weekly", StringComparison.OrdinalIgnoreCase))
        {
            var byDay = BuildByDayToken();
            if (!string.IsNullOrWhiteSpace(byDay))
            {
                parts.Add($"BYDAY={byDay}");
            }
        }

        if (string.Equals(frequency, "Monthly", StringComparison.OrdinalIgnoreCase))
        {
            var byMonthDay = BuildByMonthDayToken();
            if (!string.IsNullOrWhiteSpace(byMonthDay))
            {
                parts.Add($"BYMONTHDAY={byMonthDay}");
            }
        }

        RecurrenceRule = string.Join(';', parts);
    }

    [RelayCommand]
    private void ClearRecurrenceBuilder()
    {
        RecurrenceFrequency = "None";
        RecurrenceIntervalText = "1";
        RecurrenceCountText = string.Empty;
        RecurrenceUntilDate = null;
        RecurrenceMonthDaysText = string.Empty;
        ByMonday = false;
        ByTuesday = false;
        ByWednesday = false;
        ByThursday = false;
        ByFriday = false;
        BySaturday = false;
        BySunday = false;
        RecurrenceRule = null;
    }

    private void InitialiseRecurrenceBuilderFromRule()
    {
        if (string.IsNullOrWhiteSpace(RecurrenceRule))
        {
            ClearRecurrenceBuilder();
            return;
        }

        var parts = RecurrenceRule
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(item => item.Length == 2)
            .ToDictionary(item => item[0].ToUpperInvariant(), item => item[1], StringComparer.OrdinalIgnoreCase);

        RecurrenceFrequency = parts.TryGetValue("FREQ", out var freq)
            ? ToTitleCaseFrequency(freq)
            : "None";

        RecurrenceIntervalText = parts.TryGetValue("INTERVAL", out var interval)
            ? interval
            : "1";

        RecurrenceCountText = parts.TryGetValue("COUNT", out var count)
            ? count
            : string.Empty;

        RecurrenceUntilDate = parts.TryGetValue("UNTIL", out var until) && DateTime.TryParse(until, out var parsedUntil)
            ? parsedUntil.Date
            : null;

        RecurrenceMonthDaysText = parts.TryGetValue("BYMONTHDAY", out var monthDays)
            ? monthDays
            : string.Empty;

        ByMonday = false;
        ByTuesday = false;
        ByWednesday = false;
        ByThursday = false;
        ByFriday = false;
        BySaturday = false;
        BySunday = false;

        if (parts.TryGetValue("BYDAY", out var byDay))
        {
            var tokens = byDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => item.ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            ByMonday = tokens.Contains("MO");
            ByTuesday = tokens.Contains("TU");
            ByWednesday = tokens.Contains("WE");
            ByThursday = tokens.Contains("TH");
            ByFriday = tokens.Contains("FR");
            BySaturday = tokens.Contains("SA");
            BySunday = tokens.Contains("SU");
        }
    }

    private void ApplyRecurrenceBuilderIfActive()
    {
        if (string.IsNullOrWhiteSpace(RecurrenceFrequency) ||
            string.Equals(RecurrenceFrequency, "None", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ApplyRecurrenceBuilder();
    }

    private static string ToTitleCaseFrequency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "None";
        }

        var normalised = value.Trim().ToLowerInvariant();
        return char.ToUpperInvariant(normalised[0]) + normalised[1..];
    }

    private string BuildByDayToken()
    {
        var items = new List<string>();
        if (ByMonday) items.Add("MO");
        if (ByTuesday) items.Add("TU");
        if (ByWednesday) items.Add("WE");
        if (ByThursday) items.Add("TH");
        if (ByFriday) items.Add("FR");
        if (BySaturday) items.Add("SA");
        if (BySunday) items.Add("SU");

        return string.Join(',', items);
    }

    private string BuildByMonthDayToken()
    {
        if (string.IsNullOrWhiteSpace(RecurrenceMonthDaysText))
        {
            return string.Empty;
        }

        var validDays = RecurrenceMonthDaysText
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => int.TryParse(token, out var day) ? day : 0)
            .Where(day => day is >= 1 and <= 31)
            .Distinct()
            .OrderBy(day => day)
            .ToList();

        return validDays.Count == 0
            ? string.Empty
            : string.Join(',', validDays);
    }

    partial void OnRecurrenceFrequencyChanged(string value)
    {
        OnPropertyChanged(nameof(ShowWeeklyBuilder));
        OnPropertyChanged(nameof(ShowMonthlyBuilder));
    }
}
