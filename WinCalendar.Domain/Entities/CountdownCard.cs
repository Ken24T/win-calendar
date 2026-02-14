namespace WinCalendar.Domain.Entities;

public sealed class CountdownCard
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTimeOffset TargetDateTime { get; init; }

    public string ColourHex { get; init; } = "#2D6CDF";

    public bool IsActive { get; init; } = true;

    public int SortOrder { get; init; }

    public string BuildRemainingLabel(DateTimeOffset now)
    {
        var delta = TargetDateTime - now;

        if (delta.TotalSeconds < 0)
        {
            var elapsed = now - TargetDateTime;
            if (elapsed.TotalDays >= 1)
            {
                return $"{Math.Floor(elapsed.TotalDays)} days ago";
            }

            if (elapsed.TotalHours >= 1)
            {
                return $"{Math.Floor(elapsed.TotalHours)} hours ago";
            }

            return "elapsed";
        }

        if (delta.TotalDays >= 1)
        {
            return $"{Math.Ceiling(delta.TotalDays)} days left";
        }

        if (delta.TotalHours >= 1)
        {
            return $"{Math.Ceiling(delta.TotalHours)} hours left";
        }

        if (delta.TotalMinutes >= 1)
        {
            return $"{Math.Ceiling(delta.TotalMinutes)} minutes left";
        }

        return "due now";
    }
}
