using HideezClient.ViewModels.Dialog;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;

namespace HideezClient.Dialogs
{
    public abstract class BaseDialog: BaseMetroDialog
    {
        public event EventHandler Closed;

        protected BaseDialog(IDialogViewModel dialogViewModel)
        {
            DataContext = dialogViewModel;
        }

        public virtual void Close()
        {
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
                Closed?.Invoke(this, EventArgs.Empty);
                (DataContext as IDialogViewModel).OnClose();
            }
        }
    }
}
