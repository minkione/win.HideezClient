using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Proximity;
using HideezMiddleware;
using NLog;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    class WorkstationLocker : IWorkstationLocker
    {
        Logger _log = LogManager.GetCurrentClassLogger();
        readonly ServiceClientSessionManager _sessionManager;
        readonly ProximityMonitorManager _proximityMonitorManager;

        public WorkstationLocker(ServiceClientSessionManager sessionManager, ProximityMonitorManager proximityMonitorManager)
        {
            _sessionManager = sessionManager;
            _proximityMonitorManager = proximityMonitorManager;

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
            if (IsEnabled)
            {
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                LockWorkstationAsync();
            }
        }

        void _proximityMonitorManager_DeviceBelowLockForToLong(object sender, IDevice device)
        {
            if (IsEnabled)
            {
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                LockWorkstationAsync();
            }
        }

        void _proximityMonitorManager_DeviceProximityTimeout(object sender, IDevice device)
        {
            if (IsEnabled)
            {
                SessionSwitchManager.SetEventSubject(SessionSwitchSubject.Proximity, device.SerialNo);
                LockWorkstationAsync();
            }
        }

        public void LockWorkstation()
        {
            LockWorkstationAsync();
        }

        void LockWorkstationAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var client in _sessionManager.Sessions)
                        client.Callbacks.LockWorkstationRequest();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }
    }
}
