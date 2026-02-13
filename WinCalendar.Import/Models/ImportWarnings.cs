namespace WinCalendar.Import.Models;

public sealed class ImportWarnings
{
    private readonly List<string> _items = [];

    public IReadOnlyList<string> Items => _items;

    public void Add(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _items.Add(message);
    }
}
