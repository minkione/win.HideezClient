using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationLockProcessor : Logger, IDisposable
    {
        readonly ConnectionFlowProcessor _flowProcessor;
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly IWorkstationLocker _workstationLocker;
        readonly BleDeviceManager _deviceManager;

        List<IDevice> _authorizedDevicesList = new List<IDevice>();

        readonly object _deviceListsLock = new object();

        public event EventHandler<WorkstationLockingEventArgs> WorkstationLocking;

        public WorkstationLockProcessor(ConnectionFlowProcessor flowProcessor, ProximityMonitorManager proximityMonitorManager, BleDeviceManager deviceManager, IWorkstationLocker workstationLocker, ILog log)
            :base(nameof(WorkstationLockProcessor), log)
        {
            _flowProcessor = flowProcessor;
            _proximityMonitorManager = proximityMonitorManager;
            _workstationLocker = workstationLocker;
            _deviceManager = deviceManager;

            _flowProcessor.DeviceFinishedMainFlow += FlowProcessor_DeviceFinishedMainFlow;
            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;

            _proximityMonitorManager.DeviceConnectionLost += ProximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceBelowLockForToLong += ProximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceProximityTimeout += ProximityMonitorManager_DeviceProximityTimeout;
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
                    if (!_authorizedDevicesList.Contains(device) && _authorizedDevicesList.Count == 0)
                    {
                        WriteLine($"Device ({device.Id}) added as valid to trigger workstation lock");
                        device.Disconnected += Device_Disconnected;
                        _authorizedDevicesList.Add(device);
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

                foreach (var device in _authorizedDevicesList)
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
                _authorizedDevicesList.RemoveAll(d => d == e.RemovedDevice);
                e.RemovedDevice.Disconnected -= Device_Disconnected;
            }
        }

        void Device_Disconnected(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                lock (_deviceListsLock)
                {
                    if (_authorizedDevicesList.Contains(device))
                    {
                        WriteLine($"Device ({device.Id}) is no longer a valid trigger for workstation lock");
                        _authorizedDevicesList.Remove(device);
                    }
                }
            }
        }

        void ProximityMonitorManager_DeviceConnectionLost(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!CanLock(device))
                    return;

                WriteLine($"Going to lock the workstation by 'DeviceConnectionLost' reason. Device ID: {device.Id}");
                WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceConnectionLost));
                _workstationLocker.LockWorkstation();
            }
        }

        void ProximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!CanLock(device))
                { 
                    Task.Run(async () => { await _deviceManager.DisconnectDevice(device); });
                    return;
                }

                WriteLine($"Going to lock the workstation by 'DeviceBelowLockForToLong' reason. Device ID: {device.Id}");
                WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceBelowThreshold));
                _workstationLocker.LockWorkstation();
            }
        }

        void ProximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!CanLock(device))
                {
                    Task.Run(async () => { await _deviceManager.DisconnectDevice(device); });
                    return;
                }

                WriteLine($"Going to lock the workstation by 'DeviceProximityTimeout' reason. Device ID: {device.Id}");
                WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.ProximityTimeout));
                _workstationLocker.LockWorkstation();
            }
        }

        bool CanLock(IDevice device)
        {
            return IsEnabled && _authorizedDevicesList.Contains(device);
        }
    }
}
