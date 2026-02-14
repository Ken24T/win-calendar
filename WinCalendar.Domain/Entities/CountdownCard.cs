namespace WinCalendar.Domain.Entities;

public sealed class CountdownCard
{
    private const int DueSoonThresholdHours = 48;

    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTimeOffset TargetDateTime { get; init; }

    public string ColourHex { get; init; } = "#2D6CDF";

    public bool IsActive { get; init; } = true;

    public int SortOrder { get; init; }

    public string BuildRemainingLabel(DateTimeOffset now)
    {
        return BuildPresentation(now).RemainingLabel;
    }

    public CountdownCardPresentation BuildPresentation(DateTimeOffset now)
    {
        var remaining = TargetDateTime - now;

        if (remaining <= TimeSpan.Zero)
        {
            var elapsed = now - TargetDateTime;
            if (elapsed.TotalDays >= 1)
            {
                return new CountdownCardPresentation("Overdue", $"{Math.Floor(elapsed.TotalDays)}d overdue", 0);
            }

            if (elapsed.TotalHours >= 1)
            {
                return new CountdownCardPresentation("Overdue", $"{Math.Floor(elapsed.TotalHours)}h overdue", 0);
            }

            return new CountdownCardPresentation("Overdue", "Overdue", 0);
        }

        if (remaining.TotalDays >= 1)
        {
            var status = remaining.TotalHours <= DueSoonThresholdHours ? "Due soon" : "Upcoming";
            var priority = remaining.TotalHours <= DueSoonThresholdHours ? 1 : 2;
            return new CountdownCardPresentation(status, $"{Math.Floor(remaining.TotalDays)}d {remaining.Hours}h remaining", priority);
        }

        if (remaining.TotalHours >= 1)
        {
            return new CountdownCardPresentation("Due soon", $"{Math.Floor(remaining.TotalHours)}h {remaining.Minutes}m remaining", 1);
        }

        return new CountdownCardPresentation("Due soon", $"{Math.Max(1, remaining.Minutes)}m remaining", 1);
    }
}

public readonly record struct CountdownCardPresentation(
    string StatusLabel,
    string RemainingLabel,
    int PriorityRank);
