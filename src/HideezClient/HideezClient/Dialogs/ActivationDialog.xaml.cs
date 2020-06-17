using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for ActivationDialog.xaml
    /// </summary>
    public partial class ActivationDialog : BaseMetroDialog
    {
        //readonly Regex onlyDigitsRegex = new Regex("[0-9]+");

        public ActivationDialog(ActivationViewModel vm)
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
