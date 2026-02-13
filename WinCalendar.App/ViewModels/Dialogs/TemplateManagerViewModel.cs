using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class TemplateManagerViewModel(IEventTemplateService templateService) : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _durationMinutes = 60;

    [ObservableProperty]
    private string _category = "General";

    public ObservableCollection<EventTemplate> Templates { get; } = [];

    public async Task InitialiseAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task AddTemplateAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        await templateService.SaveTemplateAsync(new EventTemplate
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            Title = Title.Trim(),
            DefaultDuration = TimeSpan.FromMinutes(DurationMinutes <= 0 ? 60 : DurationMinutes),
            Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim()
        });

        Name = string.Empty;
        Title = string.Empty;
        DurationMinutes = 60;

        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var items = await templateService.GetTemplatesAsync();

        Templates.Clear();
        foreach (var item in items.OrderBy(x => x.Name))
        {
            Templates.Add(item);
        }
    }
}
