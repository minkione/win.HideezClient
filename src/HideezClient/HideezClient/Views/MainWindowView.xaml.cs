using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using HideezClient.Mvvm;
using System.ComponentModel;
using HideezClient.Models;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        private WeakPropertyObserver bindingRaiseEventSelectedDevice;
        private readonly List<WeakPropertyObserver> bindings = new List<WeakPropertyObserver>();

        public MainWindowView()
        {
            // This is required to avoid issue where certain buttons are shown as inactive until user performs some input action
            // Calling InvalidateRequerySuggested performs recalculation of all dependencies immediatelly instead of waiting for input
            DataContextChanged += DeviceInfo_DataContextChanged;
            InitializeComponent();
        }

        private void DeviceInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bindingRaiseEventSelectedDevice = new WeakPropertyObserver(e.NewValue, nameof(MainViewModel.ActiveDevice));
            bindingRaiseEventSelectedDevice.ValueChanged += (name, device) =>
            {
                this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);

                bindings.Clear();

                // To subscribe to all properties, uncomment next line and remote the rest of weak property bindings
                // bindings.Add(new WeakPropertyObserver(device, string.Empty));
                bindings.Add(new WeakPropertyObserver(device, nameof(IVaultModel.IsConnected)));
                bindings.Add(new WeakPropertyObserver(device, nameof(IVaultModel.FinishedMainFlow)));
                bindings.Add(new WeakPropertyObserver(device, nameof(IVaultModel.IsAuthorized)));
                bindings.Add(new WeakPropertyObserver(device, nameof(IVaultModel.IsAuthorizingRemoteDevice)));
                bindings.Add(new WeakPropertyObserver(device, nameof(IVaultModel.IsCreatingRemoteDevice)));

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
