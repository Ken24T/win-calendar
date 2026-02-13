using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.App.ViewModels.Dialogs;

public partial class SearchDialogViewModel(IEventSearchService eventSearchService) : ObservableObject
{
    public delegate Task EditEventRequestedHandler(CalendarEvent calendarEvent);

    public event EditEventRequestedHandler? EditEventRequested;

    [ObservableProperty]
    private string _query = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<CalendarEvent> Results { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasResults => Results.Count > 0;

    public bool ShowErrorState => HasError;

    public bool ShowEmptyState => !IsLoading && !HasError && !HasResults && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var items = await eventSearchService.SearchAsync(Query);

            Results.Clear();
            foreach (var item in items)
            {
                Results.Add(item);
            }
        }
        catch
        {
            Results.Clear();
            ErrorMessage = "Search failed. Please try again.";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    [RelayCommand]
    private async Task EditEventAsync(CalendarEvent? calendarEvent)
    {
        if (calendarEvent is null || EditEventRequested is null)
        {
            return;
        }

        await EditEventRequested(calendarEvent);
    }

    partial void OnQueryChanged(string value)
    {
        NotifyStateChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        NotifyStateChanged();
    }

    partial void OnErrorMessageChanged(string? value)
    {
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ShowErrorState));
        OnPropertyChanged(nameof(ShowEmptyState));
    }
}
