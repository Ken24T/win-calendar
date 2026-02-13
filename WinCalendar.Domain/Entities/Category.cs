namespace WinCalendar.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ColourHex { get; init; } = "#001CAD";
}
