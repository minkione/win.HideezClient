using System;
using System.ComponentModel;
using System.Windows;
using DeviceMaintenance.ViewModel;
using MahApps.Metro.Controls;

namespace DeviceMaintenance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;

            if (vm.IsUpdateInProgress)
            {
                var mb = MessageBox.Show(
                    "Firmware update in progress!" +
                    Environment.NewLine +
                    "Are you sure you want to exit?",
                    "Exit application",
                    MessageBoxButton.YesNoCancel, 
                    MessageBoxImage.Exclamation);

                if (mb == MessageBoxResult.Yes)
                {
                    vm.Close();
                    base.OnClosing(e);
                }
                else
                    e.Cancel = true;
            }
            else
            {
                vm.Close();
                base.OnClosing(e);
            }

        }

    }
}
