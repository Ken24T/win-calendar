namespace WinCalendar.App.ViewModels.Countdown;

public sealed class CountdownCardItemViewModel
{
    public string Title { get; init; } = string.Empty;

    public string TargetDateLabel { get; init; } = string.Empty;

    public string RemainingLabel { get; init; } = string.Empty;

    public string ColourHex { get; init; } = "#2D6CDF";
}
