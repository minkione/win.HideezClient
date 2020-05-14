using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.ReconnectManager
{
    public class DeviceReconnectManager : Logger, IDisposable
    {
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly BleDeviceManager _deviceManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        readonly object _reconnectListLock = new object();

        HashSet<string> _reconnectAllowedList = new HashSet<string>();
        HashSet<string> _reconnectInProgressList = new HashSet<string>(); // prevents start of multiple reconnects of same device

        public event EventHandler<IDevice> DeviceReconnected;
        public event EventHandler<IDevice> DeviceDisconnected;

        public DeviceReconnectManager(ProximityMonitorManager proximityMonitorManager, 
            BleDeviceManager deviceManager, 
            ConnectionFlowProcessor connectionFlowProcessor, 
            ILog log)
            : base(nameof(DeviceReconnectManager), log)
        {
            _proximityMonitorManager = proximityMonitorManager;
            _deviceManager = deviceManager;
            _connectionFlowProcessor = connectionFlowProcessor;

            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;

            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;

            _proximityMonitorManager.DeviceConnectionLost += ProximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceProximityTimeout += ProximityMonitorManager_DeviceProximityTimeout;
            _proximityMonitorManager.DeviceBelowLockForToLong += ProximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceBelowLockWarning += ProximityMonitorManager_DeviceBelowLockWarning;
            _proximityMonitorManager.DeviceProximityNormalized += ProximityMonitorManager_DeviceProximityNormalized;
        }

        private void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice e)
        {
            EnableDeviceReconnect(e);
        }

        void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            DisableDeviceReconnect(e.RemovedDevice);
        }

        async void ProximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            DisableDeviceReconnect(device);
            await _deviceManager.DisconnectDevice(device);
        }

        async void ProximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            DisableDeviceReconnect(device);
            await _deviceManager.DisconnectDevice(device);
        }

        void ProximityMonitorManager_DeviceConnectionLost(object sender, IDevice device)
        {
            if (IsEnabled)
            {
                // Reconnect is performed only if manager is enabled and we are in unlocked windows session
                // Certain operations explicitly deny device reconnect and so CanReconnect will be FALSE during them
                if (!WorkstationHelper.IsActiveSessionLocked() && CanReconnect(device.SerialNo))
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
                EnableDeviceReconnect(device.Id);
            }
        }

        void ProximityMonitorManager_DeviceBelowLockWarning(object sender, IDevice device)
        {
            lock (_reconnectListLock)
            {
                DisableDeviceReconnect(device);
            }
        }

        internal async Task Reconnect(IDevice device)
        {
            try
            {
                WriteLine($"Starting reconnect procedure for {device.SerialNo}");
                _reconnectInProgressList.Add(device.SerialNo);
                using (var cts = new CancellationTokenSource())
                {
                    await Task.Delay(SdkConfig.ReconnectDelay); // Small delay before reconnecting

                    var proc = new ReconnectProc(device, _connectionFlowProcessor);
                    var reconnectSuccessful = await proc.Run(SdkConfig.ReconnectWorkflowTimeout, cts.Token);
                    cts.Dispose();

                    if (reconnectSuccessful)
                    {
                        WriteLine($"{device.SerialNo} reconnected successfully");
                        SafeInvoke(DeviceReconnected, device);
                    }
                    else
                    {
                        WriteLine($"{device.SerialNo} reconnect failed");
                        SafeInvoke(DeviceDisconnected, device);
                    }
                }
            }
            finally
            {
                _reconnectInProgressList.Remove(device.SerialNo);
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
                    _connectionFlowProcessor.DeviceFinishedMainFlow -= ConnectionFlowProcessor_DeviceFinishedMainFlow;

                    _deviceManager.DeviceRemoved -= DeviceManager_DeviceRemoved;

                    _proximityMonitorManager.DeviceConnectionLost -= ProximityMonitorManager_DeviceConnectionLost;
                    _proximityMonitorManager.DeviceProximityTimeout -= ProximityMonitorManager_DeviceProximityTimeout;
                    _proximityMonitorManager.DeviceBelowLockForToLong -= ProximityMonitorManager_DeviceBelowLockForToLong;
                    _proximityMonitorManager.DeviceBelowLockWarning -= ProximityMonitorManager_DeviceBelowLockWarning;
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
                if (!device.IsBoot && !device.IsRemote)
                {
                    WriteLine($"{device.SerialNo} reconnect re-enabled");
                    _reconnectAllowedList.Add(device.SerialNo);
                }
            }
        }

        public void EnableDeviceReconnect(string deviceId)
        {
            lock (_reconnectListLock)
            {
                var device = _deviceManager.Find(deviceId);
                if (device != null)
                    EnableDeviceReconnect(device);
            }
        }

        public void DisableDeviceReconnect(IDevice device)
        {
            lock (_reconnectListLock)
            {
                if (!device.IsBoot && !device.IsRemote)
                {
                    WriteLine($"{device.SerialNo} reconnect disabled");
                    _reconnectAllowedList.Remove(device.SerialNo);
                }
            }
        }

        public void DisableDeviceReconnect(string deviceId)
        {
            lock (_reconnectListLock)
            {
                var device = _deviceManager.Find(deviceId);
                if (device != null)
                    DisableDeviceReconnect(device);
            }
        }

        public void DisableAllDevicesReconnect()
        {
            lock (_reconnectListLock)
            {
                _reconnectAllowedList.Clear();
            }
        }

        bool CanReconnect(string deviceSerialNo)
        {
            lock (_reconnectListLock)
            {
                return _reconnectAllowedList.Contains(deviceSerialNo) && !_reconnectInProgressList.Contains(deviceSerialNo);
            }
        }
    }
}
