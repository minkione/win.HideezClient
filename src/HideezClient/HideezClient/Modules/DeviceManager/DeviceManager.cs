using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using System.Linq;
using HideezClient.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using HideezClient.Controls;
using Hideez.SDK.Communication;
using HideezMiddleware.Threading;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;

namespace HideezClient.Modules.DeviceManager
{
    class DeviceManager : IDeviceManager
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(DeviceManager));
        readonly IMessenger _messenger;
        readonly IServiceProxy _serviceProxy;
        readonly IWindowsManager _windowsManager;
        readonly IMetaPubSub _metaMessenger;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        readonly SemaphoreQueue _semaphoreQueue = new SemaphoreQueue(1, 1);
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
            IWindowsManager windowsManager, IRemoteDeviceFactory remoteDeviceFactory, IMetaPubSub metaMessenger)
        {
            _messenger = messenger;
            _serviceProxy = serviceProxy;
            _windowsManager = windowsManager;
            _remoteDeviceFactory = remoteDeviceFactory;
            _metaMessenger = metaMessenger;

            _messenger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            _messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);

            _metaMessenger.Subscribe<ConnectedToServerEvent>(OnConnectedToService, null);
            _metaMessenger.Subscribe<DisconnectedFromServerEvent>(OnDisconnectedFromService, null);
        }

        public IEnumerable<Device> Devices => _devices.Values;

        async Task OnDisconnectedFromService(DisconnectedFromServerEvent arg)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                await ClearDevicesCollection();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async Task OnConnectedToService(ConnectedToServerEvent arg)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                await EnumerateDevices();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async void OnDevicesCollectionChanged(DevicesCollectionChangedMessage message)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                await EnumerateDevices(message.Devices);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async void OnDeviceConnectionStateChanged(DeviceConnectionStateChangedMessage message)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                await EnumerateDevices();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async Task ClearDevicesCollection()
        {
            foreach (var dvm in Devices.ToArray())
                await RemoveDevice(dvm);
        }

        async Task EnumerateDevices()
        {
            try
            {
                // TODO: FIX THIS LINE
                //var serviceDevices = await _serviceProxy.GetService().GetDevicesAsync();
                //await EnumerateDevices(serviceDevices);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }

        async Task EnumerateDevices(DeviceDTO[] serviceDevices)
        {
            try
            {
                // Ignore devices that are not connected
                serviceDevices = serviceDevices.Where(d => d.IsConnected).ToArray();

                // Create device if it does not exist in UI
                foreach (var deviceDto in serviceDevices)
                    AddDevice(deviceDto);

                // delete device from UI if its deleted from service
                Device[] missingDevices = _devices.Values.Where(d => serviceDevices.FirstOrDefault(dto => dto.SerialNo == d.SerialNo) == null).ToArray();
                await RemoveDevices(missingDevices);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }

        void AddDevice(DeviceDTO dto)
        {
            if (!_devices.ContainsKey(dto.Id))
            {
                var device = new Device(_serviceProxy, _remoteDeviceFactory, _messenger, _metaMessenger, dto);
                device.PropertyChanged += Device_PropertyChanged;

                if (_devices.TryAdd(device.Id, device))
                {
                    DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
                }
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
                await removedDevice.ShutdownRemoteDeviceAsync(HideezErrorCode.DeviceRemoved);
                removedDevice.Dispose();
                DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device));
            }
        }

        async Task RemoveDevices(Device[] devices)
        {
            foreach (var device in devices)
                await RemoveDevice(device);
        }
    }
}
