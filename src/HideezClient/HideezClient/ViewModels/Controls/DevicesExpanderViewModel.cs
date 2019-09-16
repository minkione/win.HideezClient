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
        readonly ViewModelLocator viewModelLocator;

        public DevicesExpanderViewModel(IDeviceManager deviceManager, IWindowsManager windowsManager, IMenuFactory menuFactory, ViewModelLocator viewModelLocator)
        {
            this.windowsManager = windowsManager;
            this.deviceManager = deviceManager;
            this.menuFactory = menuFactory;
            this.viewModelLocator = viewModelLocator;
            deviceManager.DevicesCollectionChanged += Devices_CollectionChanged;
            Devices = new ObservableCollection<DeviceForExpanderViewModel>(deviceManager.Devices.Select(d => new DeviceForExpanderViewModel(d, windowsManager, menuFactory, viewModelLocator)));
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (Device device in e.NewItems)
                    {
                        Devices.Add(new DeviceForExpanderViewModel(device, windowsManager, menuFactory, viewModelLocator));
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Device device in e.OldItems)
                    {
                        Devices.Remove(Devices.FirstOrDefault((System.Func<DeviceForExpanderViewModel, bool>)(d => d.Id == device.Id)));
                    }
                }
            }));
        }

        #region Properties

        public ObservableCollection<DeviceForExpanderViewModel> Devices { get; }

        #endregion Properties
    }
}
