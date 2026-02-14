using System.Windows;
using System.Windows.Threading;
using WinCalendar.App.ViewModels;

namespace WinCalendar.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _countdownRefreshTimer = new()
    {
        Interval = TimeSpan.FromMinutes(1)
    };

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        _countdownRefreshTimer.Tick += OnCountdownRefreshTick;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not ShellViewModel shellViewModel)
            {
                StartupDiagnostics.WriteInfo("MainWindow loaded without ShellViewModel DataContext.");
                return;
            }

            StartupDiagnostics.WriteInfo("MainWindow loaded. Initialising ShellViewModel.");
            await shellViewModel.InitialiseAsync();
            shellViewModel.RefreshCountdownLabels();
            _countdownRefreshTimer.Start();
            StartupDiagnostics.WriteInfo("ShellViewModel initialised.");
        }
        catch (Exception exception)
        {
            StartupDiagnostics.WriteError("MainWindow OnLoaded exception.", exception);
            MessageBox.Show(
                $"Failed to initialise the calendar view.\n\n{exception.Message}\n\nDiagnostics log:\n{StartupDiagnostics.LogPath}",
                "WinCalendar Initialisation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnCountdownRefreshTick(object? sender, EventArgs e)
    {
        try
        {
            if (DataContext is ShellViewModel shellViewModel)
            {
                shellViewModel.RefreshCountdownLabels();
            }
        }
        catch (Exception exception)
        {
            StartupDiagnostics.WriteError("Countdown refresh tick failed.", exception);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _countdownRefreshTimer.Stop();
        _countdownRefreshTimer.Tick -= OnCountdownRefreshTick;
        Loaded -= OnLoaded;
        Closed -= OnClosed;
    }
}