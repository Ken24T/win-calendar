namespace WinCalendar.Domain.Entities;

public sealed class EventTemplate
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public TimeSpan DefaultDuration { get; init; } = TimeSpan.FromHours(1);

    public string Category { get; init; } = "General";
}
