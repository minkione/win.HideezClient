using HideezSafe.Models;
using HideezSafe.Modules;
using HideezSafe.Modules.DeviceManager;
using HideezSafe.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace HideezSafe.ViewModels
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
                        Devices.Add(new DeviceForExpanderViewModel(device, windowsManager, menuFactory));
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
