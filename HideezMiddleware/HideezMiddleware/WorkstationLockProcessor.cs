using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using Hideez.SDK.Communication.WorkstationEvents;

namespace HideezMiddleware
{
    public class WorkstationLockProcessor : Logger
    {
        readonly ProximityMonitorManager _proximityMonitorManager;
        readonly IWorkstationLocker _workstationLocker;

        public WorkstationLockProcessor(ProximityMonitorManager proximityMonitorManager, IWorkstationLocker workstationLocker, ILog log)
            :base(nameof(WorkstationLockProcessor), log)
        {
            _proximityMonitorManager = proximityMonitorManager;
            _workstationLocker = workstationLocker;

            _proximityMonitorManager.DeviceConnectionLost += _proximityMonitorManager_DeviceConnectionLost;
            _proximityMonitorManager.DeviceBelowLockForToLong += _proximityMonitorManager_DeviceBelowLockForToLong;
            _proximityMonitorManager.DeviceProximityTimeout += _proximityMonitorManager_DeviceProximityTimeout;
        }

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
            SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
            _workstationLocker.LockWorkstation();
        }

        void _proximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            if (!IsEnabled)
                return;

            WriteLine($"Going to lock the workstation by 'DeviceBelowLockForToLong' reason. Device ID: {device.Id}");
            SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
            _workstationLocker.LockWorkstation();
        }

        void _proximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            if (!IsEnabled)
                return;

            WriteLine($"Going to lock the workstation by 'DeviceProximityTimeout' reason. Device ID: {device.Id}");
            SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
            _workstationLocker.LockWorkstation();
        }
    }
}
