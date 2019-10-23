using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using NLog;
using System.Linq;
using HideezClient.Models;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using HideezClient.Controls;
using Hideez.SDK.Communication;

namespace HideezClient.Modules.DeviceManager
{
    class DeviceManager : IDeviceManager
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly IMessenger _messenger;
        private readonly IServiceProxy _serviceProxy;
        private readonly IWindowsManager _windowsManager;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        ConcurrentDictionary<string, Device> _devices { get; } = new ConcurrentDictionary<string, Device>();

        // Custom dispatcher is required for unit tests because during test 
        // runs the Application.Current property is null
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

        public event NotifyCollectionChangedEventHandler DevicesCollectionChanged;

        public DeviceManager(IMessenger messenger, IServiceProxy serviceProxy,
            IWindowsManager windowsManager, IRemoteDeviceFactory remoteDeviceFactory)
        {
            _messenger = messenger;
            _serviceProxy = serviceProxy;
            _windowsManager = windowsManager;
            _remoteDeviceFactory = remoteDeviceFactory;

            _messenger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            _messenger.Register<DeviceFinishedMainFlowMessage>(this, OnDeviceFinishedMainFlow);

            _serviceProxy.Disconnected += OnServiceProxyConnectionStateChanged;
            _serviceProxy.Connected += OnServiceProxyConnectionStateChanged;
        }

        public IEnumerable<Device> Devices => _devices.Values;

        async void OnServiceProxyConnectionStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_serviceProxy.IsConnected)
                    await ClearDevicesCollection();
                else
                    await EnumerateDevices();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        async void OnDevicesCollectionChanged(DevicesCollectionChangedMessage message)
        {
            try
            {
                await EnumerateDevices(message.Devices);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        void OnDeviceFinishedMainFlow(DeviceFinishedMainFlowMessage message)
        {
            Task.Run(() =>
            {
                try
                {
                    if (_devices.TryGetValue(message.Device.Id, out Device dvm))
                    {
                        if (dvm.IsConnected && message.Device.FinishedMainFlow)
                            _ = TryCreateRemoteDeviceAsync(dvm);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }

        async Task ClearDevicesCollection()
        {
            foreach (var dvm in Devices.ToArray())
                await RemoveDevice(dvm);
        }
        // TODO: Add thread safety
        async Task EnumerateDevices()
        {
            try
            {
                var serviceDevices = await _serviceProxy.GetService().GetDevicesAsync();
                await EnumerateDevices(serviceDevices);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.Error(ex.FormattedMessage());
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        async Task EnumerateDevices(DeviceDTO[] serviceDevices)
        {
            try
            {
                // Create device if it does not exist in UI
                foreach (var deviceDto in serviceDevices)
                    AddDevice(deviceDto);

                // delete device from UI if its deleted from service
                Device[] missingDevices = _devices.Values.Where(d => serviceDevices.FirstOrDefault(dto => dto.SerialNo == d.SerialNo) == null).ToArray();
                await RemoveDevices(missingDevices);

                // Create remote devices if they are not already created
                foreach (var dvm in Devices.ToList().Where(d => d.IsConnected))
                {
                    if (serviceDevices.Any(d => d.Id == dvm.Id && d.IsConnected && d.FinishedMainFlow))
                        _ = TryCreateRemoteDeviceAsync(dvm);
                }
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.Error(ex.FormattedMessage());
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        void AddDevice(DeviceDTO dto)
        {
            var device = new Device(_serviceProxy, _remoteDeviceFactory, _messenger, dto);
            device.PropertyChanged += Device_PropertyChanged;

            if (_devices.TryAdd(device.Id, device))
            {
                DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
            }
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is Device device && e.PropertyName == nameof(Device.IsLoadingStorage) && device.IsLoadingStorage)
            {
                CredentialsLoadNotificationViewModel viewModal = new CredentialsLoadNotificationViewModel(device);
                _windowsManager.ShowCredentialsLoading(viewModal);
            }
        }

        async Task RemoveDevice(Device device)
        {
            if (_devices.TryRemove(device.Id, out Device removedDevice))
            {
                removedDevice.PropertyChanged -= Device_PropertyChanged;
                await removedDevice.CloseRemoteDeviceConnection(HideezErrorCode.DeviceRemoved);
                DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device));
            }
        }

        async Task RemoveDevices(Device[] devices)
        {
            foreach (var device in devices)
                await RemoveDevice(device);
        }

        async Task TryCreateRemoteDeviceAsync(Device device)
        {
            try
            {
                if (device.IsAuthorized && !device.IsStorageLoaded && !device.IsLoadingStorage)
                {
                    await device.LoadStorage();
                }
                else if (!device.IsInitialized && !device.IsInitializing && !device.IsAuthorized && !device.IsAuthorizing)
                {
                    await device.InitializeRemoteDevice();

                    // With certain configuration, the device will be authorized without pin code or button confirmation
                    // In that case we can immediatelly begin loading storage
                    if (device.IsAuthorized && !device.IsStorageLoaded && !device.IsLoadingStorage)
                        await device.LoadStorage();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}
