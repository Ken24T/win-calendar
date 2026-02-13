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
        if (DataContext is not ShellViewModel shellViewModel)
        {
            return;
        }

        await shellViewModel.InitialiseAsync();
    }
}