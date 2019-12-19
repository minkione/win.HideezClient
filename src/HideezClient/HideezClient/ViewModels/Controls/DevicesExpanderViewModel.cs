using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.DeviceManager;
using HideezClient.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HideezClient.ViewModels
{
    class DevicesExpanderViewModel : ObservableObject
    {
        readonly IWindowsManager windowsManager;
        readonly IDeviceManager deviceManager;
        readonly IMenuFactory menuFactory;

        public DevicesExpanderViewModel(IDeviceManager deviceManager, IWindowsManager windowsManager, IMenuFactory menuFactory)
        {
            this.windowsManager = windowsManager;
            this.deviceManager = deviceManager;
            this.menuFactory = menuFactory;
            deviceManager.DevicesCollectionChanged += Devices_CollectionChanged;
            Devices = new ObservableCollection<DeviceForExpanderViewModel>(deviceManager.Devices.Select(d => new DeviceForExpanderViewModel(d, windowsManager, menuFactory)));
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (Device device in e.NewItems)
                    {
                        SubscribeToDevice(device);
                        if (device.IsConnected)
                            CreateViewModel(device);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Device device in e.OldItems)
                    {
                        UnsubscribeFromDevice(device);
                        RemoveViewModel(device);
                    }
                }
            }));
        }

        #region Properties

        public ObservableCollection<DeviceForExpanderViewModel> Devices { get; }

        #endregion Properties

        void SubscribeToDevice(Device device)
        {
            device.PropertyChanged += Device_PropertyChanged;
        }

        void UnsubscribeFromDevice(Device device)
        {
            device.PropertyChanged -= Device_PropertyChanged;
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Device device && e.PropertyName == nameof(Device.IsConnected))
            {
                if (device.IsConnected)
                    CreateViewModel(device);
                else
                    RemoveViewModel(device);
            }
        }

        void CreateViewModel(Device device)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!Devices.Any(d => d.Id == device.Id))
                    Devices.Add(new DeviceForExpanderViewModel(device, windowsManager, menuFactory));
            });
        }

        void RemoveViewModel(Device device)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Devices.Remove(Devices.FirstOrDefault((System.Func<DeviceForExpanderViewModel, bool>)(d => d.Id == device.Id)));
            });
        }
    }
}
