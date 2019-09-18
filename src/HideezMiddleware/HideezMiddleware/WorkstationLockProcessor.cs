using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using System;

namespace HideezMiddleware
{
    public class WorkstationLockProcessor : Logger, IDisposable
    {
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly IWorkstationLocker _workstationLocker;

        public event EventHandler<WorkstationLockingEventArgs> WorkstationLocking;

        public WorkstationLockProcessor(ProximityMonitorManager proximityMonitorManager, IWorkstationLocker workstationLocker, ILog log)
            :base(nameof(WorkstationLockProcessor), log)
        {
            _proximityMonitorManager = proximityMonitorManager;
            _workstationLocker = workstationLocker;

            _proximityMonitorManager.DeviceConnectionLost += _proximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceBelowLockForToLong += _proximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceProximityTimeout += _proximityMonitorManager_DeviceProximityTimeout;
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
                _proximityMonitorManager.DeviceConnectionLost -= _proximityMonitorManager_DeviceConnectionLost;
                _proximityMonitorManager.DeviceBelowLockForToLong -= _proximityMonitorManager_DeviceBelowLockForToLong;
                _proximityMonitorManager.DeviceProximityTimeout -= _proximityMonitorManager_DeviceProximityTimeout;
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

        void _proximityMonitorManager_DeviceConnectionLost(object sender, IDevice device)
        {
            if (!IsEnabled)
                return;

            WriteLine($"Going to lock the workstation by 'DeviceConnectionLost' reason. Device ID: {device.Id}");
            WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceConnectionLost));
            _workstationLocker.LockWorkstation();
        }

        void _proximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            if (!IsEnabled)
                return;

            WriteLine($"Going to lock the workstation by 'DeviceBelowLockForToLong' reason. Device ID: {device.Id}");
            WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.DeviceBelowThreshold));
            _workstationLocker.LockWorkstation();
        }

        void _proximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            if (!IsEnabled)
                return;

            WriteLine($"Going to lock the workstation by 'DeviceProximityTimeout' reason. Device ID: {device.Id}");
            WorkstationLocking?.Invoke(this, new WorkstationLockingEventArgs(device, WorkstationLockingReason.ProximityTimeout));
            _workstationLocker.LockWorkstation();
        }
    }
}
