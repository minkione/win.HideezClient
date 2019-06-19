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

            messanger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            //messanger.Register<DevicePropertiesUpdatedMessage>(this, OnDevicePropertiesUpdated);
            //messanger.Register<DeviceProximityChangedMessage>(this, OnProximityChanged);
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

        private void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            Task.Run(UpdateDevicesAsync);
        }

        void OnDevicesCollectionChanged(DevicesCollectionChangedMessage message)
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
                var devices = await serviceProxy.GetService().GetDevicesAsync();
                await UpdateDevicesAsync();
            }
        }

        private async Task UpdateDevicesAsync(DeviceDTO[] serviceDevices)
        {
            try
            {
                // update device's properties. If device does not exists, create it
                foreach (var deviceDto in serviceDevices)
                {
                    var device = FindDevice(deviceDto);
                    if (device != null)
                    {
                        device.LoadFrom(deviceDto);
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DeviceViewModel dvm = null;

                            lock (Devices)
                            {
                                device = FindDevice(deviceDto);

                                if (device == null)
                                {
                                    dvm = new DeviceViewModel(deviceDto, windowsManager, serviceProxy);
                                    Devices.Add(dvm);
                                }
                            }

                        });
                    }
                }

                // delete device from UI if its deleted from service
                foreach (var clientDevice in
                    Devices.Where(d => serviceDevices.FirstOrDefault(dto => dto.SerialNo == d.SerialNo) == null)
                    .ToArray())
                {
                    lock (Devices)
                    {
                        Dispatcher.Invoke(() => Devices.Remove(clientDevice));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public DeviceViewModel FindDevice(DeviceDTO deviceDto)
        {
            return FindDevice(deviceDto.SerialNo);
        }

        public DeviceViewModel FindDevice(string serialNo)
        {
            lock (Devices)
            {
                return Devices.FirstOrDefault(d => d.SerialNo == serialNo);
            }
        }
    }
}
