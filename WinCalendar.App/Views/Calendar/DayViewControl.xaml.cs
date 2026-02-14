using System;
using System.Windows.Controls;
using System.Windows.Threading;
using WinCalendar.App.ViewModels;

namespace WinCalendar.App.Views.Calendar;

public partial class DayViewControl : UserControl
{
    private readonly DispatcherTimer _currentTimeTimer;

    public DayViewControl()
    {
        InitializeComponent();

        _currentTimeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };

        _currentTimeTimer.Tick += (_, _) =>
        {
            if (DataContext is ShellViewModel viewModel)
            {
                viewModel.UpdateCurrentTimeIndicator();
            }
        };
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel)
        {
            viewModel.UpdateCurrentTimeIndicator();
        }

        _currentTimeTimer.Start();
    }

    private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _currentTimeTimer.Stop();
    }
}
