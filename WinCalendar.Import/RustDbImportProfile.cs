namespace WinCalendar.Import;

public sealed class RustDbImportProfile
{
    public string SourceDatabasePath { get; init; } = string.Empty;

    public bool ImportEvents { get; init; } = true;

    public bool ImportCategories { get; init; } = true;

    public bool ImportTemplates { get; init; } = true;

    public bool ImportSettings { get; init; } = true;

    public bool ImportCustomThemes { get; init; } = true;
}
