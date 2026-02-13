namespace WinCalendar.App.ViewModels.Dialogs;

public interface IDialogRequestClose
{
    event EventHandler<bool?>? RequestClose;
}
