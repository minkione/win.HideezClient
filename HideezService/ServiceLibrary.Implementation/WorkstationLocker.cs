using Hideez.SDK.Communication.Proximity;
using NLog;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    class WorkstationLocker
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

        void _proximityMonitorManager_DeviceConnectionLost(object sender, string deviceId)
        {
            LockWorkstationAsync();
        }

        void _proximityMonitorManager_DeviceBelowLockForToLong(object sender, string deviceId)
        {
            LockWorkstationAsync();
        }

        void _proximityMonitorManager_DeviceProximityTimeout(object sender, string deviceId)
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
