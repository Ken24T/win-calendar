using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class EventEditorViewModel : ObservableObject, IDialogRequestClose
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public event EventHandler<bool?>? RequestClose;

    public EventEditorViewModel(IEventService eventService, ICategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _startDateTime = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset _endDateTime = DateTimeOffset.Now.AddHours(1);

    [ObservableProperty]
    private bool _isAllDay;

    [ObservableProperty]
    private string _category = "General";

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _recurrenceRule;

    public ObservableCollection<string> Categories { get; } = [];

    public async Task InitialiseAsync()
    {
        Categories.Clear();
        var categories = await _categoryService.GetCategoriesAsync();
        foreach (var item in categories.OrderBy(x => x.Name))
        {
            Categories.Add(item.Name);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title) || EndDateTime < StartDateTime)
        {
            return;
        }

        await _eventService.SaveEventAsync(new CalendarEvent
        {
            Id = Guid.NewGuid(),
            Title = Title.Trim(),
            StartDateTime = StartDateTime,
            EndDateTime = EndDateTime,
            IsAllDay = IsAllDay,
            Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            RecurrenceRule = string.IsNullOrWhiteSpace(RecurrenceRule) ? null : RecurrenceRule.Trim()
        });

        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }
}
