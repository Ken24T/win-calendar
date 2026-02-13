using System.Windows;
using WinCalendar.App.ViewModels;

namespace WinCalendar.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
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
}