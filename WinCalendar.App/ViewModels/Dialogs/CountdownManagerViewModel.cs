using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class CountdownManagerViewModel(ICountdownService countdownService) : ObservableObject
{
    private static readonly Regex ColourHexPattern = new("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

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

    [ObservableProperty]
    private bool _showInactive;

    [ObservableProperty]
    private string? _validationMessage;

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessage);

    public ObservableCollection<CountdownCard> CountdownCards { get; } = [];

    public ObservableCollection<CountdownCard> VisibleCountdownCards { get; } = [];

    public async Task InitialiseAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!TryParseSortOrder(out var sortOrder))
        {
            return;
        }

        if (!ValidateInput(sortOrder))
        {
            return;
        }

        ValidationMessage = null;

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
        var cards = await countdownService.GetCountdownCardsForManagementAsync();

        CountdownCards.Clear();
        foreach (var card in cards)
        {
            CountdownCards.Add(card);
        }

        RefreshVisibleCards();
    }

    partial void OnSelectedCardChanged(CountdownCard? value)
    {
        if (value is null)
        {
            return;
        }

        ValidationMessage = null;
        Title = value.Title;
        TargetDateTime = value.TargetDateTime;
        ColourHex = value.ColourHex;
        IsActive = value.IsActive;
        SortOrderText = value.SortOrder.ToString();
    }

    private void ResetEditor()
    {
        ValidationMessage = null;
        Title = string.Empty;
        TargetDateTime = DateTimeOffset.Now.AddDays(7);
        ColourHex = "#2D6CDF";
        IsActive = true;
        SortOrderText = "0";
    }

    partial void OnValidationMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasValidationMessage));
    }

    partial void OnShowInactiveChanged(bool value)
    {
        RefreshVisibleCards();
    }

    private bool TryParseSortOrder(out int sortOrder)
    {
        if (!int.TryParse(SortOrderText, out sortOrder))
        {
            ValidationMessage = "Sort order must be a whole number.";
            return false;
        }

        return true;
    }

    private bool ValidateInput(int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ValidationMessage = "Title is required.";
            return false;
        }

        if (Title.Trim().Length > 100)
        {
            ValidationMessage = "Title must be 100 characters or fewer.";
            return false;
        }

        if (!ColourHexPattern.IsMatch(ColourHex.Trim()))
        {
            ValidationMessage = "Colour must be a hex value like #2D6CDF.";
            return false;
        }

        if (sortOrder < 0 || sortOrder > 999)
        {
            ValidationMessage = "Sort order must be between 0 and 999.";
            return false;
        }

        if (TargetDateTime < DateTimeOffset.Now.AddYears(-1))
        {
            ValidationMessage = "Target date is too far in the past.";
            return false;
        }

        return true;
    }

    private void RefreshVisibleCards()
    {
        var previousSelectionId = SelectedCard?.Id;

        var filtered = CountdownCards
            .Where(card => ShowInactive || card.IsActive)
            .OrderBy(card => card.SortOrder)
            .ThenBy(card => card.TargetDateTime)
            .ThenBy(card => card.Title)
            .ThenBy(card => card.Id)
            .ToList();

        VisibleCountdownCards.Clear();
        foreach (var card in filtered)
        {
            VisibleCountdownCards.Add(card);
        }

        if (previousSelectionId is null)
        {
            return;
        }

        SelectedCard = VisibleCountdownCards.FirstOrDefault(card => card.Id == previousSelectionId)
            ?? CountdownCards.FirstOrDefault(card => card.Id == previousSelectionId)
            ?? SelectedCard;
    }
}
