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
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProxy _serviceProxy;
        private readonly IWindowsManager _windowsManager;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;

        // Required for unit tests because during tests Application.Current is null
        Dispatcher Dispatcher
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

        public DeviceManager(IMessenger messenger, IServiceProxy serviceProxy, 
            IWindowsManager windowsManager, IRemoteDeviceFactory remoteDeviceFactory)
        {
            Devices = new ObservableCollection<DeviceViewModel>();
            _serviceProxy = serviceProxy;
            _windowsManager = windowsManager;
            _remoteDeviceFactory = remoteDeviceFactory;

            messenger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            messenger.Register<DeviceInitializedMessage>(this, OnDeviceInitialized);
            messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);

            _serviceProxy.Disconnected += ServiceProxy_ConnectionStateChanged;
            _serviceProxy.Connected += ServiceProxy_ConnectionStateChanged;
        }

        readonly object devicesLock = new object();
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();

        async void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (!_serviceProxy.IsConnected)
                ClearDevicesCollection();
            else
                await EnumerateDevices();
        }

        void OnDevicesCollectionChanged(DevicesCollectionChangedMessage message)
        {
            Task.Run(EnumerateDevices);
        }

        void OnDeviceConnectionStateChanged(DeviceConnectionStateChangedMessage message)
        {
            Task.Run(() =>
            {
                try
                {
                    var dvm = FindDevice(message.Device.Id);
                    if (dvm != null)
                        dvm.IsConnected = message.Device.IsConnected;
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }

        void OnDeviceInitialized(DeviceInitializedMessage message)
        {
            Task.Run(() =>
            {
                try
                {
                    var dvm = FindDevice(message.Device.Id);
                    if (dvm != null)
                    {
                        dvm.LoadFrom(message.Device);

                        if (dvm.IsConnected)
                            TryCreateRemoteDevice(dvm);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }

        DeviceViewModel FindDevice(string id)
        {
            lock (devicesLock)
            {
                return Devices.FirstOrDefault(d => d.Id == id);
            }
        }

        void ClearDevicesCollection()
        {
            lock (devicesLock)
            {
                foreach (var dvm in Devices.ToArray())
                    RemoveDevice(dvm);
            }
        }

        async Task EnumerateDevices()
        {
            try
            {
                var serviceDevices = await _serviceProxy.GetService().GetDevicesAsync();

                // Create device if it does not exist in UI
                foreach (var deviceDto in serviceDevices)
                {
                    var device = FindDevice(deviceDto.Id);
                    if (device == null)
                    {
                        Dispatcher.Invoke(() =>
                        {

                            DeviceViewModel dvm;

                            lock (devicesLock)
                            {
                                dvm = new DeviceViewModel(deviceDto, _windowsManager, _serviceProxy, _remoteDeviceFactory);
                                Devices.Add(dvm);
                            }

                            if (deviceDto.IsInitialized && dvm.IsConnected)
                                TryCreateRemoteDevice(dvm);
                        });
                    }
                }

                // delete device from UI if its deleted from service
                DeviceViewModel[] missingDevices;
                lock (devicesLock)
                {
                    missingDevices = Devices.Where(d => serviceDevices.FirstOrDefault(dto => dto.SerialNo == d.SerialNo) == null).ToArray();
                }
                RemoveDevices(missingDevices);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        async void TryCreateRemoteDevice(DeviceViewModel dvm)
        {
            if (!dvm.IsInitialized && !dvm.IsInitializing)
                await dvm.EstablishRemoteDeviceConnection();
        }

        /// <summary>
        /// Note: Must be executed on STA thread
        /// </summary>
        void RemoveDeviceSynchronously(DeviceViewModel dvm)
        {
            lock (devicesLock)
            {
                Devices.Remove(dvm);
            }
            // todo: close RemoveDevice connection and stop connection establishment that is in progress
        }

        void RemoveDevice(DeviceViewModel dvm)
        {
            Dispatcher.Invoke(() =>
            {
                RemoveDeviceSynchronously(dvm);
            });
        }

        void RemoveDevices(DeviceViewModel[] dvms)
        {
            Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < dvms.Length; i++)
                {
                    // Avoid invoking dispatcher from inside the dispatcher
                    RemoveDeviceSynchronously(dvms[i]);
                }
            });
        }
    }
}
