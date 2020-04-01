using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using System.Linq;
using HideezClient.Models;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using HideezClient.Controls;
using Hideez.SDK.Communication;
using HideezMiddleware.Threading;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Vaults.Software;

namespace HideezClient.Modules.VaultManager
{
    class VaultManager : Logger, IVaultManager
    {
        private readonly ILog _log;
        private readonly IMessenger _messenger;
        private readonly IServiceProxy _serviceProxy;
        private readonly IWindowsManager _windowsManager;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        readonly HesSoftwareVaultConnection _hesSoftwareVaultConnection;
        readonly SemaphoreQueue _semaphoreQueue = new SemaphoreQueue(1, 1);
        ConcurrentDictionary<string, HardwareVaultModel> _vaults { get; } = new ConcurrentDictionary<string, HardwareVaultModel>();

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

        public VaultManager(IMessenger messenger, IServiceProxy serviceProxy,
            IWindowsManager windowsManager, IRemoteDeviceFactory remoteDeviceFactory, HesSoftwareVaultConnection hesSoftwareVaultConnection, ILog log)
            : base(nameof(VaultManager), log)
        {
            _messenger = messenger;
            _serviceProxy = serviceProxy;
            _windowsManager = windowsManager;
            _remoteDeviceFactory = remoteDeviceFactory;
            _hesSoftwareVaultConnection = hesSoftwareVaultConnection;
            _log = log;

            _messenger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
            _messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);

            _serviceProxy.Disconnected += OnServiceProxyConnectionStateChanged;
            _serviceProxy.Connected += OnServiceProxyConnectionStateChanged;

            _hesSoftwareVaultConnection.HubConnectedSoftwareVaultsListChanged += OnSoftwareVaultsListChanged;
            _hesSoftwareVaultConnection.HubConnectionStateChanged += OnSoftwareVaultHubConnectionStateChanged;
        }

        public IEnumerable<HardwareVaultModel> Vaults => _vaults.Values;

        async void OnServiceProxyConnectionStateChanged(object sender, EventArgs e)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                if (!_serviceProxy.IsConnected)
                    await ClearDevicesCollection();
                else
                    await EnumerateDevices();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
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
                WriteLine(ex);
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
                WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async void OnSoftwareVaultsListChanged(object sender, IEnumerable<SoftwareVaultInfoDto> e)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async void OnSoftwareVaultHubConnectionStateChanged(object sender, EventArgs e)
        {
            await _semaphoreQueue.WaitAsync();
            try
            {
                // TODO: Remove all SoftwareVaults
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                _semaphoreQueue.Release();
            }
        }

        async Task ClearDevicesCollection()
        {
            foreach (var dvm in Vaults.ToArray())
                await RemoveDevice(dvm);
        }

        async Task EnumerateDevices()
        {
            try
            {
                var serviceDevices = await _serviceProxy.GetService().GetDevicesAsync();
                await EnumerateDevices(serviceDevices);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                WriteLine(ex.FormattedMessage(), LogErrorSeverity.Error);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
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
                HardwareVaultModel[] missingDevices = _vaults.Values.Where(d => serviceDevices.FirstOrDefault(dto => dto.SerialNo == d.SerialNo) == null).ToArray();
                await RemoveDevices(missingDevices);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                WriteLine(ex.FormattedMessage(), LogErrorSeverity.Error);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        void AddDevice(DeviceDTO dto)
        {
            if (!_vaults.ContainsKey(dto.Id))
            {
                var device = new HardwareVaultModel(_serviceProxy, _remoteDeviceFactory, _messenger, dto);
                device.PropertyChanged += Device_PropertyChanged;

                if (_vaults.TryAdd(device.Id, device))
                {
                    DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
                }
            }
        }

        void AddDevice(SoftwareVaultInfoDto dto)
        {
            throw new NotImplementedException();

            if (!_vaults.ContainsKey(dto.Id))
            {

                var vault = new SoftwareVault(dto, _hesSoftwareVaultConnection, _log);
                HardwareVaultModel device = null; // Todo: Create SoftwareVault
                device.PropertyChanged += Device_PropertyChanged;

                if (_vaults.TryAdd(device.Id, device))
                {
                    DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device));
                }
            }
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is HardwareVaultModel device && e.PropertyName == nameof(HardwareVaultModel.IsLoadingStorage) && device.IsLoadingStorage)
            {
                CredentialsLoadNotificationViewModel viewModal = new CredentialsLoadNotificationViewModel(device);
                _windowsManager.ShowCredentialsLoading(viewModal);
            }
        }

        async Task RemoveDevice(HardwareVaultModel device)
        {
            if (_vaults.TryRemove(device.Id, out HardwareVaultModel removedDevice))
            {
                removedDevice.PropertyChanged -= Device_PropertyChanged;
                await removedDevice.ShutdownRemoteDeviceAsync(HideezErrorCode.DeviceRemoved);
                removedDevice.Dispose();
                DevicesCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, device));
            }
        }

        async Task RemoveDevices(HardwareVaultModel[] devices)
        {
            foreach (var device in devices)
                await RemoveDevice(device);
        }
    }
}
