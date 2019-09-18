using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using System;
using System.Collections.Generic;

namespace HideezMiddleware
{
    public class WorkstationLockProcessor : Logger, IDisposable
    {
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly IWorkstationLocker _workstationLocker;
        readonly BleDeviceManager _deviceManager;

        List<IDevice> _subscribedDevicesList = new List<IDevice>();
        List<IDevice> _authorizedDevicesList = new List<IDevice>();

        readonly object _deviceListsLock = new object();

        public WorkstationLockProcessor(ProximityMonitorManager proximityMonitorManager, BleDeviceManager deviceManager, IWorkstationLocker workstationLocker, ILog log)
            :base(nameof(WorkstationLockProcessor), log)
        {
            _proximityMonitorManager = proximityMonitorManager;
            _workstationLocker = workstationLocker;
            _deviceManager = deviceManager;

            _deviceManager.DeviceAdded += DeviceManager_DeviceAdded;
            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;

            _proximityMonitorManager.DeviceConnectionLost += ProximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceBelowLockForToLong += ProximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceProximityTimeout += ProximityMonitorManager_DeviceProximityTimeout;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _deviceManager.DeviceAdded -= DeviceManager_DeviceAdded;
                _deviceManager.DeviceRemoved -= DeviceManager_DeviceRemoved;

                _proximityMonitorManager.DeviceConnectionLost -= ProximityMonitorManager_DeviceConnectionLost;
                _proximityMonitorManager.DeviceBelowLockForToLong -= ProximityMonitorManager_DeviceBelowLockForToLong;
                _proximityMonitorManager.DeviceProximityTimeout -= ProximityMonitorManager_DeviceProximityTimeout;

                foreach (var device in _subscribedDevicesList)
                    device.Authorized -= Device_Authorized;
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
                e.RemovedDevice.Authorized -= Device_Authorized;
                _subscribedDevicesList.Remove(e.RemovedDevice);
                _authorizedDevicesList.Remove(e.RemovedDevice);
            }
        }

        void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            if (e.AddedDevice == null)
                return;

            lock (_deviceListsLock)
            {
                if (!e.AddedDevice.IsRemote && !e.AddedDevice.IsBoot)
                {
                    e.AddedDevice.Authorized += Device_Authorized;
                    e.AddedDevice.ConnectionStateChanged += Device_ConnectionStateChanged;
                    _subscribedDevicesList.Add(e.AddedDevice);
                }
            }
        }

        private void Device_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                lock (_deviceListsLock)
                {
                    if (!device.IsConnected)
                        _authorizedDevicesList.Remove(device);
                }
            }
        }

        void Device_Authorized(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                lock (_deviceListsLock)
                {
                    WriteLine($"Device ({device.Id}) added as valid to trigger workstation lock");
                    _authorizedDevicesList.Add(device);
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
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                _workstationLocker.LockWorkstation();
            }
        }

        void ProximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!CanLock(device))
                    return;

                WriteLine($"Going to lock the workstation by 'DeviceBelowLockForToLong' reason. Device ID: {device.Id}");
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                _workstationLocker.LockWorkstation();
            }
        }

        void ProximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            lock (_deviceListsLock)
            {
                if (!CanLock(device))
                    return;

                WriteLine($"Going to lock the workstation by 'DeviceProximityTimeout' reason. Device ID: {device.Id}");
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                _workstationLocker.LockWorkstation();
            }
        }

        bool CanLock(IDevice device)
        {
            return IsEnabled && _authorizedDevicesList.Contains(device);
        }
    }
}
