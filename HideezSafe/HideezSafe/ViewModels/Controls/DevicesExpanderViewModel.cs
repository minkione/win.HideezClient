using HideezSafe.Models;
using HideezSafe.Modules;
using HideezSafe.Modules.DeviceManager;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using HideezSafe.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HideezSafe.ViewModels
{
    class DevicesExpanderViewModel : ObservableObject
    {
        readonly IWindowsManager windowsManager;
        readonly IServiceProxy serviceProxy;
        readonly IDeviceManager deviceManager;
        readonly IMenuFactory menuFactory;

        public DevicesExpanderViewModel(IDeviceManager deviceManager, IWindowsManager windowsManager, IServiceProxy serviceProxy, IMenuFactory menuFactory)
        {
            this.windowsManager = windowsManager;
            this.serviceProxy = serviceProxy;
            this.deviceManager = deviceManager;
            this.menuFactory = menuFactory;
            deviceManager.Devices.CollectionChanged += Devices_CollectionChanged;
            Devices = new ObservableCollection<DeviceViewModel>(deviceManager.Devices.Select(d => new DeviceViewModel(d, windowsManager, serviceProxy, menuFactory)));
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (Device device in e.NewItems)
                    {
                        Devices.Add(new DeviceViewModel(device, windowsManager, serviceProxy, menuFactory));
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Device device in e.OldItems)
                    {
                        Devices.Remove(Devices.FirstOrDefault(d => d.Id == device.Id));
                    }
                }
            });
        }

        #region Properties

        public ObservableCollection<DeviceViewModel> Devices { get; }

        #endregion Properties
    }
}
