using CommunityToolkit.Mvvm.ComponentModel;
using WinCalendar.Domain.Entities;

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
        var presentation = new CountdownCard
        {
            TargetDateTime = TargetDateTime
        }.BuildPresentation(now);

        StatusLabel = presentation.StatusLabel;
        RemainingLabel = presentation.RemainingLabel;
        PriorityRank = presentation.PriorityRank;
    }
}
