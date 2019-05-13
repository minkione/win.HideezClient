using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.ViewModels;
using NLog;

namespace HideezSafe.Modules.DeviceManager
{
    class DeviceManager : IDeviceManager
    {
        readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IServiceProxy serviceProxy;
        private int deviceUpdateLoopRunning = 0;

        public DeviceManager(IMessenger messanger, IServiceProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;

            messanger.Register<PairedDevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            // Reminder for the future
            //messanger.Register<DevicePropertiesUpdatedMessage>(this, );
            //messanger.Register<DeviceProximityUpdatedMessage>(this, );
            serviceProxy.Disconnected += ServiceProxy_ConnectionStateChanged;
            serviceProxy.Connected += ServiceProxy_ConnectionStateChanged;

            Task.Run(() => UpdateDevicesProc());
        }

        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        private void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    await UpdateDevices();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            });
        }

        void OnDevicesCollectionChanged(PairedDevicesCollectionChangedMessage message)
        {
            Task.Run(async () =>
            {
                try
                {
                    await UpdateDevices();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            });
        }

        void ClearDevicesCollection()
        {
            lock (Devices)
            {
                if (Devices.Count > 0)
                {
                    try
                    {
                        if (Application.Current != null)
                            Application.Current.Dispatcher.Invoke(() => Devices.Clear());
                        else
                            Devices.Clear();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        async Task UpdateDevices()
        {
            if (!serviceProxy.IsConnected)
            {
                foreach (var device in Devices)
                {
                    device.IsConnected = false;
                    device.Proximity = 0;
                }

                return;
            }
            try
            {
                BleDeviceDTO[] serverDevices = await serviceProxy.GetService().GetPairedDevicesAsync();

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
                        Application.Current?.Dispatcher.Invoke(async () =>
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
                if (serverDevices.Length != Devices.Count)
                {
                    foreach (var clientDevice in Devices)
                    {
                        bool exists = false;
                        foreach (var serviceDevice in serverDevices)
                        {
                            if (serviceDevice.Id == clientDevice.Id)
                                exists = true;
                        }

                        if (!exists)
                        {
                            lock (Devices)
                            {
                                Application.Current?.Dispatcher.Invoke(() => Devices.Remove(clientDevice));
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        void UpdateDevice(BleDeviceDTO deviceDto)
        {
            try
            {
                var device = FindDevice(deviceDto);
                if (device != null)
                    device.LoadFrom(deviceDto);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        async void UpdateDevicesProc()
        {
            while (true)
            {
                try
                {
                    if (deviceUpdateLoopRunning > 0)
                        await UpdateDevices();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    ClearDevicesCollection();
                }
                finally
                {
                    await Task.Delay(1000);
                }
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
                foreach (var item in Devices)
                {
                    if (item.Id == deviceId)
                        return item;
                }
            }

            return null;
        }

        public void StartDeviceUpdateLoop()
        {
            Interlocked.Increment(ref deviceUpdateLoopRunning);
        }

        public void StopDeviceUpdateLoop()
        {
            Interlocked.Decrement(ref deviceUpdateLoopRunning);
        }
    }
}
