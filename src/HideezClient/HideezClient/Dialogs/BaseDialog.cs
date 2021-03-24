using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;

namespace HideezClient.Dialogs
{
    public abstract class BaseDialog: BaseMetroDialog
    {
        public event EventHandler Closed;

        public virtual void Close()
        {
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
