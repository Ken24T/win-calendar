using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class CountdownManagerViewModel(ICountdownService countdownService) : ObservableObject
{
    [ObservableProperty]
    private CountdownCard? _selectedCard;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _targetDateTime = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty]
    private string _colourHex = "#2D6CDF";

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string _sortOrderText = "0";

    public ObservableCollection<CountdownCard> CountdownCards { get; } = [];

    public async Task InitialiseAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            return;
        }

        var sortOrder = int.TryParse(SortOrderText, out var parsedSortOrder)
            ? parsedSortOrder
            : 0;

        var card = new CountdownCard
        {
            Id = SelectedCard?.Id ?? Guid.NewGuid(),
            Title = Title.Trim(),
            TargetDateTime = TargetDateTime,
            ColourHex = string.IsNullOrWhiteSpace(ColourHex) ? "#2D6CDF" : ColourHex.Trim(),
            IsActive = IsActive,
            SortOrder = sortOrder
        };

        await countdownService.SaveCountdownCardAsync(card);
        await ReloadAsync();

        SelectedCard = CountdownCards.FirstOrDefault(item => item.Id == card.Id);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedCard is null)
        {
            return;
        }

        await countdownService.DeleteCountdownCardAsync(SelectedCard.Id);
        await ReloadAsync();
        ResetEditor();
    }

    [RelayCommand]
    private void NewCard()
    {
        SelectedCard = null;
        ResetEditor();
    }

    private async Task ReloadAsync()
    {
        var cards = await countdownService.GetCountdownCardsAsync();

        CountdownCards.Clear();
        foreach (var card in cards)
        {
            CountdownCards.Add(card);
        }
    }

    partial void OnSelectedCardChanged(CountdownCard? value)
    {
        if (value is null)
        {
            return;
        }

        Title = value.Title;
        TargetDateTime = value.TargetDateTime;
        ColourHex = value.ColourHex;
        IsActive = value.IsActive;
        SortOrderText = value.SortOrder.ToString();
    }

    private void ResetEditor()
    {
        Title = string.Empty;
        TargetDateTime = DateTimeOffset.Now.AddDays(7);
        ColourHex = "#2D6CDF";
        IsActive = true;
        SortOrderText = "0";
    }
}
