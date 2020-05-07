using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using HideezMiddleware.ReconnectManager;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationLockProcessor : Logger, IDisposable
    {
        public const string PROX_LOCK_ENABLED_PROP = "ProximityLockEnabled";

        readonly ConnectionFlowProcessor _flowProcessor;
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly BleDeviceManager _deviceManager;
        readonly IWorkstationLocker _workstationLocker;
        readonly DeviceReconnectManager _deviceReconnectManager;

        readonly object _deviceListsLock = new object();

        public event EventHandler<IDevice> DeviceProxLockEnabled;
        public event EventHandler<WorkstationLockingEventArgs> WorkstationLocking;

        public WorkstationLockProcessor(
            ConnectionFlowProcessor flowProcessor, 
            ProximityMonitorManager proximityMonitorManager, 
            BleDeviceManager deviceManager, 
            IWorkstationLocker workstationLocker, 
            DeviceReconnectManager deviceReconnectManager,
            ILog log)
            :base(nameof(WorkstationLockProcessor), log)
        {
            _flowProcessor = flowProcessor;
            _proximityMonitorManager = proximityMonitorManager;
            _deviceManager = deviceManager;
            _workstationLocker = workstationLocker;
            _deviceReconnectManager = deviceReconnectManager;

            _flowProcessor.DeviceFinishedMainFlow += FlowProcessor_DeviceFinishedMainFlow;
            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;

            _proximityMonitorManager.DeviceBelowLockForToLong += ProximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceProximityTimeout += ProximityMonitorManager_DeviceProximityTimeout;
            _proximityMonitorManager.DeviceConnectionLost += ProximityMonitorManager_DeviceConnectionLost;

            _deviceReconnectManager.DeviceDisconnected += DeviceReconnectManager_DeviceDisconnected;
        }

        void FlowProcessor_DeviceFinishedMainFlow(object sender, IDevice device)
        {
            if (device == null)
                return;

            lock (_deviceListsLock)
            {
                if (!device.IsRemote && !device.IsBoot)
                {
                    // Limit of one device that may be authorized for workstation lock
                    if (!_deviceManager.Devices.Any(d => d.GetUserProperty<bool>(PROX_LOCK_ENABLED_PROP)))
                    {
                        WriteLine($"Device ({device.Id}) added as valid to trigger workstation lock");
                        device.Disconnected += Device_Disconnected;
                        device.SetUserProperty(PROX_LOCK_ENABLED_PROP, true);
                        SafeInvoke(DeviceProxLockEnabled, device);
                    }
                }
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _flowProcessor.DeviceFinishedMainFlow -= FlowProcessor_DeviceFinishedMainFlow;
                _deviceManager.DeviceRemoved -= DeviceManager_DeviceRemoved;

                _proximityMonitorManager.DeviceConnectionLost -= ProximityMonitorManager_DeviceConnectionLost;
                _proximityMonitorManager.DeviceBelowLockForToLong -= ProximityMonitorManager_DeviceBelowLockForToLong;
                _proximityMonitorManager.DeviceProximityTimeout -= ProximityMonitorManager_DeviceProximityTimeout;

                foreach (var device in _deviceManager.Devices.ToList())
                    device.Disconnected -= Device_Disconnected;
            }

            disposed = true;
        }

        ~WorkstationLockProcessor()
        {
            Dispose(false);
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

        void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            if (e.RemovedDevice == null)
                return;

            lock (_deviceListsLock)
            {
                e.RemovedDevice.Disconnected -= Device_Disconnected;
            }
        }

        void Device_Disconnected(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                lock (_deviceListsLock)
                {
                    if (device.GetUserProperty<bool>(PROX_LOCK_ENABLED_PROP))
                    {
                        WriteLine($"Device ({device.Id}) is no longer a valid trigger for workstation lock");
                        device.SetUserProperty(PROX_LOCK_ENABLED_PROP, false);
                    }
                }
            }
        }

        void ProximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (CanLock(device))
                {
                    WriteLine($"Going to lock the workstation by 'DeviceBelowLockForToLong' reason. Device ID: {device.Id}");
                    WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceBelowThreshold));
                    _workstationLocker.LockWorkstation();
                }
            }
        }

        void ProximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (CanLock(device))
                {
                    WriteLine($"Going to lock the workstation by 'DeviceProximityTimeout' reason. Device ID: {device.Id}");
                    WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.ProximityTimeout));
                    _workstationLocker.LockWorkstation();
                }
            }
        }

        void ProximityMonitorManager_DeviceConnectionLost(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!_deviceReconnectManager.IsEnabled && CanLock(device))
                {
                    WriteLine($"Going to lock the workstation by 'DeviceConnectionLost' reason. Device ID: {device.Id}");
                    WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceConnectionLost));
                    _workstationLocker.LockWorkstation();
                }
            }
        }

        void DeviceReconnectManager_DeviceDisconnected(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (_deviceReconnectManager.IsEnabled && CanLock(device))
                {
                    WriteLine($"Going to lock the workstation by 'DeviceConnectionLost' reason. Device ID: {device.Id}");
                    WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceConnectionLost));
                    _workstationLocker.LockWorkstation();
                }
            }
        }

        bool CanLock(IDevice device)
        {
            return IsEnabled && device.GetUserProperty<bool>(PROX_LOCK_ENABLED_PROP);
        }
    }
}
