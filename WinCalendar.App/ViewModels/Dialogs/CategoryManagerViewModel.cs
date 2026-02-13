using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class CategoryManagerViewModel(ICategoryService categoryService) : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _colourHex = "#001CAD";

    public ObservableCollection<Category> Categories { get; } = [];

    public async Task InitialiseAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return;
        }

        await categoryService.SaveCategoryAsync(new Category
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            ColourHex = string.IsNullOrWhiteSpace(ColourHex) ? "#001CAD" : ColourHex.Trim()
        });

        Name = string.Empty;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var items = await categoryService.GetCategoriesAsync();

        Categories.Clear();
        foreach (var item in items.OrderBy(x => x.Name))
        {
            Categories.Add(item);
        }
    }
}
