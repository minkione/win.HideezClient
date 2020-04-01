using MahApps.Metro.Controls;
using System.Collections.Generic;
using HideezClient.ViewModels;
using System.Windows;
using System.Windows.Input;
using HideezClient.Mvvm;
using System.ComponentModel;
using System;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for SimpleMainWindow.xaml
    /// </summary>
    public partial class SimpleMainView : MetroWindow
    {
        private WeakPropertyObserver bindingRaiseEventSelectedDevice;
        private readonly List<WeakPropertyObserver> bindings = new List<WeakPropertyObserver>();

        public SimpleMainView()
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
                bindings.Add(new WeakPropertyObserver(device, nameof(VaultViewModel.IsConnected)));
                bindings.Add(new WeakPropertyObserver(device, nameof(VaultViewModel.FinishedMainFlow)));
                bindings.Add(new WeakPropertyObserver(device, nameof(VaultViewModel.IsAuthorized)));
                bindings.Add(new WeakPropertyObserver(device, nameof(VaultViewModel.IsAuthorizingRemoteDevice)));
                bindings.Add(new WeakPropertyObserver(device, nameof(VaultViewModel.IsCreatingRemoteDevice)));

                bindings.ForEach(b => b.ValueChanged += DeviceValueChanged);
            };
        }

        private void DeviceValueChanged(string propName, object value)
        {
            this.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            // When changing state from minimized to normal, this case ensures that window is properly resized
            if (ActualHeight > 0)
                InvalidateVisual();
        }
    }
}
