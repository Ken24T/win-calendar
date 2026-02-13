using System.Windows;
using WinCalendar.App.ViewModels.Dialogs;

namespace WinCalendar.App.Views.Dialogs;

public partial class EventEditorWindow : Window
{
    public EventEditorWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is IDialogRequestClose oldHandler)
        {
            oldHandler.RequestClose -= OnRequestClose;
        }

        if (e.NewValue is IDialogRequestClose newHandler)
        {
            newHandler.RequestClose += OnRequestClose;
        }
    }

    private void OnRequestClose(object? sender, bool? dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
}
