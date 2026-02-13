namespace WinCalendar.Domain.Entities;

public sealed class AppTheme
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string DefinitionJson { get; init; } = "{}";
}
