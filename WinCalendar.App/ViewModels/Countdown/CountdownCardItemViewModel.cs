using CommunityToolkit.Mvvm.ComponentModel;

namespace WinCalendar.App.ViewModels.Countdown;

public sealed partial class CountdownCardItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _targetDateTime;

    [ObservableProperty]
    private string _targetDateLabel = string.Empty;

    [ObservableProperty]
    private string _remainingLabel = string.Empty;

    [ObservableProperty]
    private string _colourHex = "#2D6CDF";

    [ObservableProperty]
    private int _sortOrder;

    [ObservableProperty]
    private string _statusLabel = string.Empty;

    [ObservableProperty]
    private int _priorityRank = 2;

    public void UpdateRemainingLabel(DateTimeOffset now)
    {
        var remaining = TargetDateTime - now;

        if (remaining <= TimeSpan.Zero)
        {
            var elapsed = now - TargetDateTime;

            StatusLabel = "Overdue";
            PriorityRank = 0;

            if (elapsed.TotalDays >= 1)
            {
                RemainingLabel = $"{Math.Floor(elapsed.TotalDays)}d overdue";
                return;
            }

            if (elapsed.TotalHours >= 1)
            {
                RemainingLabel = $"{Math.Floor(elapsed.TotalHours)}h overdue";
                return;
            }

            RemainingLabel = "Overdue";
            return;
        }

        if (remaining.TotalHours <= 48)
        {
            StatusLabel = "Due soon";
            PriorityRank = 1;
        }
        else
        {
            StatusLabel = "Upcoming";
            PriorityRank = 2;
        }

        if (remaining.TotalDays >= 1)
        {
            RemainingLabel = $"{Math.Floor(remaining.TotalDays)}d {remaining.Hours}h remaining";
            return;
        }

        if (remaining.TotalHours >= 1)
        {
            RemainingLabel = $"{Math.Floor(remaining.TotalHours)}h {remaining.Minutes}m remaining";
            return;
        }

        RemainingLabel = $"{Math.Max(1, remaining.Minutes)}m remaining";
    }
}
