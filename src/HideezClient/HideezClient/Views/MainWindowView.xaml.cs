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
        private WeakPropertyObserver bindingRaiseeventSelectedDevice;
        private readonly List<WeakPropertyObserver> bindings = new List<WeakPropertyObserver>();

        public MainWindowView()
        {
            DataContextChanged += DeviceInfo_DataContextChanged;
            InitializeComponent();
        }

        private void DeviceInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bindingRaiseeventSelectedDevice = new WeakPropertyObserver(e.NewValue, nameof(MainViewModel.SelectedDevice));
            bindingRaiseeventSelectedDevice.ValueChanged += (name, device) =>
            {
                this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

                bindings.Clear();

                // For all property delete commit from next line
                // bindings.Add(new BindingRaiseevent(device, string.Empty));
                bindings.Add(new WeakPropertyObserver(device, nameof(DeviceViewModel.IsConnected)));
                bindings.Add(new WeakPropertyObserver(device, nameof(DeviceViewModel.FinishedMainFlow)));
                bindings.Add(new WeakPropertyObserver(device, nameof(DeviceViewModel.IsAuthorized)));
                bindings.Add(new WeakPropertyObserver(device, nameof(DeviceViewModel.IsAuthorizingRemoteDevice)));
                bindings.Add(new WeakPropertyObserver(device, nameof(DeviceViewModel.IsCreatingRemoteDevice)));

                bindings.ForEach(b => b.ValueChanged += DeviceValueChanged);
            };
        }

        private void DeviceValueChanged(string propName, object value)
        {
            this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }
    }
}
