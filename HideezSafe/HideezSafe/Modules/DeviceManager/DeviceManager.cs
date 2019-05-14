using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Utilities;
using HideezSafe.ViewModels;
using NLog;
using System.Linq;

namespace HideezSafe.Modules.DeviceManager
{
    public class DeviceManager : IDeviceManager
    {
        readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IServiceProxy serviceProxy;

        public DeviceManager(IMessenger messanger, IServiceProxy serviceProxy)
        {
            Devices = new ObservableCollection<DeviceViewModel>();
            this.serviceProxy = serviceProxy;

            messanger.Register<PairedDevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            messanger.Register<DevicePropertiesUpdatedMessage>(this, OnDevicePropertiesUpdated);
            messanger.Register<DeviceProximityUpdatedMessage>(this, OnDeviceProximityUpdated);
            serviceProxy.Disconnected += ServiceProxy_ConnectionStateChanged;
            serviceProxy.Connected += ServiceProxy_ConnectionStateChanged;

            Task.Run(UpdateDevices);

            //Devices.Add(new DeviceViewModel("user1@hideez.com", "Hideez key", "HedeezKeySimpleIMG", "8989"));
            //Devices.Add(new DeviceViewModel("user2@hideez.com", "Hideez key", "HedeezKeySimpleIMG", "7777") { IsConnected = true, Proximity = 50 });
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

        private void OnDeviceProximityUpdated(DeviceProximityUpdatedMessage obj)
        {
            DeviceViewModel deviceVM = FindDevice(obj.Device);
            if (deviceVM != null)
            {
                // TODO: Set proximity
                // deviceVM.Proximity = obj.Device.Proximity
            }
        }

        private void OnDevicePropertiesUpdated(DevicePropertiesUpdatedMessage obj)
        {
            FindDevice(obj.Device)?.LoadFrom(obj.Device);
        }

        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        private void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            UpdateDevices().Wait();
        }

        void OnDevicesCollectionChanged(PairedDevicesCollectionChangedMessage message)
        {
            UpdateDevices(message.Devices);
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

        private void ServiceDisconnected()
        {
            foreach (var device in Devices)
            {
                device.IsConnected = false;
                device.Proximity = 0;
            }
        }

        private async Task UpdateDevices()
        {
            if (!serviceProxy.IsConnected)
            {
                ServiceDisconnected();
            }
            else
            {
                UpdateDevices(await serviceProxy.GetService().GetPairedDevicesAsync());
            }
        }

        private void UpdateDevices(BleDeviceDTO[] serverDevices)
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
                                    dvm = new DeviceViewModel(item);
                                    Devices.Add(dvm);
                                }
                            }
                        });
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
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public DeviceViewModel FindDevice(BleDeviceDTO deviceDto)
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
