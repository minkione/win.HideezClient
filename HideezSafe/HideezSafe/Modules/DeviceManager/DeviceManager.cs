using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.ViewModels;
using NLog;
using System.Linq;
using Hideez.SDK.Communication.Remote;

namespace HideezSafe.Modules.DeviceManager
{
    class DeviceManager : IDeviceManager
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProxy serviceProxy;
        private readonly IWindowsManager windowsManager;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;

        public DeviceManager(IMessenger messanger, IServiceProxy serviceProxy, 
            IWindowsManager windowsManager, IRemoteDeviceFactory remoteDeviceFactory)
        {
            Devices = new ObservableCollection<DeviceViewModel>();
            this.serviceProxy = serviceProxy;
            this.windowsManager = windowsManager;
            _remoteDeviceFactory = remoteDeviceFactory;
            windowsManager.MainWindowVisibleChanged += WindowsManager_ActivatedStateMainWindowChanged;

            messanger.Register<PairedDevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            messanger.Register<DevicePropertiesUpdatedMessage>(this, OnDevicePropertiesUpdated);
            messanger.Register<DeviceProximityChangedMessage>(this, OnProximityChanged);
            serviceProxy.Disconnected += ServiceProxy_ConnectionStateChanged;
            serviceProxy.Connected += ServiceProxy_ConnectionStateChanged;

            Task.Run(UpdateDevicesAsync);
        }

        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        public ObservableCollection<RemoteDevice> RemoteDevices { get; } = new ObservableCollection<RemoteDevice>();

        private void WindowsManager_ActivatedStateMainWindowChanged(object sender, bool isVisible)
        {
            Task.Run(UpdateDevicesAsync);
        }

        public async Task SwitchMonitoringDeviceProximityAsync(bool enable)
        {
            foreach (var device in Devices)
            {
                if (enable)
                {
                    await serviceProxy.GetService().EnableMonitoringProximityAsync(device.Id);
                }
                else
                {
                    await serviceProxy.GetService().DisableMonitoringProximityAsync(device.Id);
                }
            }
        }

        public async Task SwitchMonitoringDevicePropertiesAsync(bool enable)
        {
            foreach (var device in Devices)
            {
                if (enable)
                {
                    await serviceProxy.GetService().EnableMonitoringDevicePropertiesAsync(device.Id);
                }
                else
                {
                    await serviceProxy.GetService().DisableMonitoringDevicePropertiesAsync(device.Id);
                }
            }
        }

        private void OnProximityChanged(DeviceProximityChangedMessage obj)
        {
            var device = FindDevice(obj.DeviceId);
            if (device != null)
            {
                device.Proximity = obj.Proximity;
            }
        }

        private Dispatcher Dispatcher
        {
            get
            {
                if (Application.Current != null)
                {
                    return Application.Current.Dispatcher;
                }

                return Dispatcher.CurrentDispatcher;
            }
        }

        private void OnDevicePropertiesUpdated(DevicePropertiesUpdatedMessage obj)
        {
            FindDevice(obj.Device)?.LoadFrom(obj.Device);
        }

        private void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            Task.Run(UpdateDevicesAsync);
        }

        void OnDevicesCollectionChanged(PairedDevicesCollectionChangedMessage message)
        {
            Task.Run(()=> UpdateDevicesAsync(message.Devices));
        }

        void ClearDevicesCollection()
        {
            lock (Devices)
            {
                if (Devices.Count > 0)
                {
                    try
                    {
                        Dispatcher.Invoke(Devices.Clear);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        private void OnServiceDisconnected()
        {
            foreach (var device in Devices)
            {
                device.IsConnected = false;
                device.Proximity = 0;
            }
        }

        private async Task UpdateDevicesAsync()
        {
            if (!serviceProxy.IsConnected)
            {
                OnServiceDisconnected();
                // ClearDevicesCollection();
            }
            else
            {
                await UpdateDevicesAsync(await serviceProxy.GetService().GetPairedDevicesAsync());
            }
        }

        private async Task UpdateDevicesAsync(DeviceDTO[] serverDevices)
        {
            try
            {
                // update device's properties. If device does not exists, create it
                foreach (var item in serverDevices)
                {
                    var device = FindDevice(item);
                    if (device != null)
                    {
                        device.LoadFrom(item);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DeviceViewModel dvm = null;

                            lock (Devices)
                            {
                                device = FindDevice(item);

                                if (device == null)
                                {
                                    dvm = new DeviceViewModel(item, windowsManager, serviceProxy);
                                    Devices.Add(dvm);
                                }
                            }

                        });

                        if (device == null)
                        {
                            var remDev = await _remoteDeviceFactory.CreateRemoteDevice(item.Mac, 4);
                            RemoteDevices.Add(remDev);
                        }
                    }
                }

                // delete device from UI if its deleted from service
                foreach (var clientDevice in
                    Devices.Where(d => serverDevices.FirstOrDefault(dto => dto.Id == d.Id) == null)
                    .ToArray())
                {
                    lock (Devices)
                    {
                        Dispatcher.Invoke(() => Devices.Remove(clientDevice));
                    }
                }

                await SwitchMonitoringDeviceProximityAsync(windowsManager.IsMainWindowVisible);
                await SwitchMonitoringDevicePropertiesAsync(windowsManager.IsMainWindowVisible);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public DeviceViewModel FindDevice(DeviceDTO deviceDto)
        {
            return FindDevice(deviceDto.Id);
        }

        public DeviceViewModel FindDevice(string deviceId)
        {
            lock (Devices)
            {
                return Devices.FirstOrDefault(d => d.Id == deviceId);
            }
        }
    }
}
