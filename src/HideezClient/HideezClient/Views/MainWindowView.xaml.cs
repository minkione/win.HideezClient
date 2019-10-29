using HideezClient.Modules;
using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HideezClient.Mvvm;
using System.ComponentModel;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        private BindingRaiseevent bindingRaiseeventSelectedDevice;
        private BindingRaiseevent bindingToDeviceProperties;

        public MainWindowView()
        {
            DataContextChanged += DeviceInfo_DataContextChanged;
            InitializeComponent();
        }

        private void DeviceInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bindingRaiseeventSelectedDevice = new BindingRaiseevent(e.NewValue, nameof(MainViewModel.SelectedDevice));
            bindingRaiseeventSelectedDevice.ValueChanged += device =>
            {
                bindingToDeviceProperties = new BindingRaiseevent(device, "");
                bindingToDeviceProperties.ValueChanged += value => this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
            };
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}
