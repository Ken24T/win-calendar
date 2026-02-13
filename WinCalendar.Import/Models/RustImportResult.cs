namespace WinCalendar.Import.Models;

public sealed class RustImportResult
{
    public int ImportedEvents { get; set; }

    public int ImportedCategories { get; set; }

    public int ImportedTemplates { get; set; }

    public int ImportedSettings { get; set; }

    public int ImportedThemes { get; set; }

    public ImportWarnings Warnings { get; } = new();
}
