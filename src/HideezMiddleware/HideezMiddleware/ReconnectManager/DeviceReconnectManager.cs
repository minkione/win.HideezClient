using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using HideezMiddleware.DeviceConnection.Workflow;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.ReconnectManager
{
    public class DeviceReconnectManager : Logger, IDisposable
    {
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly DeviceManager _deviceManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        readonly object _reconnectListLock = new object();

        HashSet<string> _reconnectAllowedList = new HashSet<string>();
        HashSet<string> _reconnectInProgressList = new HashSet<string>(); // prevents start of multiple reconnects of same device

        public event EventHandler<IDevice> DeviceReconnected;
        public event EventHandler<IDevice> DeviceDisconnected;

        public DeviceReconnectManager(ProximityMonitorManager proximityMonitorManager, 
            DeviceManager deviceManager, 
            ConnectionFlowProcessor connectionFlowProcessor, 
            ILog log)
            : base(nameof(DeviceReconnectManager), log)
        {
            _proximityMonitorManager = proximityMonitorManager;
            _deviceManager = deviceManager;
            _connectionFlowProcessor = connectionFlowProcessor;

            _connectionFlowProcessor.DeviceFinilizingMainFlow += ConnectionFlowProcessor_DeviceFinalizingMainFlow;

            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;

            _proximityMonitorManager.DeviceConnectionLost += ProximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceProximityTimeout += ProximityMonitorManager_DeviceProximityTimeout;
            _proximityMonitorManager.DeviceBelowLockForToLong += ProximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceBelowUnlockWarning += ProximityMonitorManager_DeviceBelowUnlockWarning;
            _proximityMonitorManager.DeviceProximityNormalized += ProximityMonitorManager_DeviceProximityNormalized;
        }

        private void ConnectionFlowProcessor_DeviceFinalizingMainFlow(object sender, IDevice e)
        {
            EnableDeviceReconnect(e);
        }

        void DeviceManager_DeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            DisableDeviceReconnect(e.Device);
        }

        async void ProximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            DisableDeviceReconnect(device);
            await _deviceManager.Disconnect(device.DeviceConnection);
        }

        async void ProximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            DisableDeviceReconnect(device);
            await _deviceManager.Disconnect(device.DeviceConnection);
        }

        void ProximityMonitorManager_DeviceConnectionLost(object sender, IDevice device)
        {
            if (IsEnabled)
            {
                // Reconnect is performed only if manager is enabled and we are in unlocked windows session
                // Certain operations explicitly deny device reconnect and so CanReconnect will be FALSE during them
                if (!WorkstationHelper.IsActiveSessionLocked() && CanReconnect(device.Id))
                {
                    Task.Run(() => Reconnect(device));
                }
                else
                {
                    SafeInvoke(DeviceDisconnected, device);
                }
            }

        }

        void ProximityMonitorManager_DeviceProximityNormalized(object sender, IDevice device)
        {
            lock (_reconnectListLock)
            {
                if (device.GetUserProperty<HwVaultConnectionState>(CustomProperties.HW_CONNECTION_STATE_PROP) >= HwVaultConnectionState.Finalizing)
                    EnableDeviceReconnect(device);
            }
        }

        void ProximityMonitorManager_DeviceBelowUnlockWarning(object sender, IDevice device)
        {
            lock (_reconnectListLock)
            {
                if (device.GetUserProperty<HwVaultConnectionState>(CustomProperties.HW_CONNECTION_STATE_PROP) >= HwVaultConnectionState.Finalizing)
                    DisableDeviceReconnect(device);
            }
        }

        internal async Task Reconnect(IDevice device)
        {
            try
            {
                WriteLine($"Starting reconnect procedure for {device.Id}");
                _reconnectInProgressList.Add(device.Id);

                await Task.Delay(SdkConfig.ReconnectDelay); // Small delay before reconnecting

                var proc = new ReconnectProc(device, _connectionFlowProcessor);
                var reconnectSuccessful = await proc.Run();


                if (reconnectSuccessful)
                {
                    WriteLine($"{device.Id} reconnected successfully");
                    SafeInvoke(DeviceReconnected, device);
                }
                else
                {
                    WriteLine($"{device.Id} reconnect failed");
                    SafeInvoke(DeviceDisconnected, device);
                }
            }
            finally
            {
                _reconnectInProgressList.Remove(device.Id);
            }
        }

        #region IDisposable
        bool disposed = false; // To detect redundant calls
        protected virtual void Dispose(bool dispose)
        {
            if (!disposed)
            {
                if (dispose)
                {
                    _connectionFlowProcessor.DeviceFinishedMainFlow -= ConnectionFlowProcessor_DeviceFinalizingMainFlow;

                    _deviceManager.DeviceRemoved -= DeviceManager_DeviceRemoved;

                    _proximityMonitorManager.DeviceConnectionLost -= ProximityMonitorManager_DeviceConnectionLost;
                    _proximityMonitorManager.DeviceProximityTimeout -= ProximityMonitorManager_DeviceProximityTimeout;
                    _proximityMonitorManager.DeviceBelowLockForToLong -= ProximityMonitorManager_DeviceBelowLockForToLong;
                    _proximityMonitorManager.DeviceBelowUnlockWarning -= ProximityMonitorManager_DeviceBelowUnlockWarning;
                    _proximityMonitorManager.DeviceProximityNormalized -= ProximityMonitorManager_DeviceProximityNormalized;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposed = true;
            }
        }

        ~DeviceReconnectManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public bool IsEnabled { get; private set; }

        public void Start()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;
        }

        public void EnableDeviceReconnect(IDevice device)
        {
            lock (_reconnectListLock)
            {
                if (!device.IsBoot && !(device is IRemoteDeviceProxy))
                {
                    WriteLine($"{device.Id} reconnect re-enabled");
                    _reconnectAllowedList.Add(device.Id);
                }
            }
        }

        public void DisableDeviceReconnect(IDevice device)
        {
            lock (_reconnectListLock)
            {
                if (!device.IsBoot && !(device is IRemoteDeviceProxy))
                {
                    WriteLine($"{device.Id} reconnect disabled");
                    _reconnectAllowedList.Remove(device.Id);
                }
            }
        }

        public void DisableAllDevicesReconnect()
        {
            lock (_reconnectListLock)
            {
                _reconnectAllowedList.Clear();
            }
        }

        bool CanReconnect(string id)
        {
            lock (_reconnectListLock)
            {
                return _reconnectAllowedList.Contains(id) && !_reconnectInProgressList.Contains(id);
            }
        }
    }
}
