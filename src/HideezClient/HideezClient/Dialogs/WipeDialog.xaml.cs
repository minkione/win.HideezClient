using HideezClient.ViewModels;
using HideezClient.ViewModels.Dialog;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for WipeDialog.xaml
    /// </summary>
    public partial class WipeDialog : BaseMetroDialog
    {
        public WipeDialog(WipeViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        public event EventHandler Closed;

        public void Close()
        {
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
