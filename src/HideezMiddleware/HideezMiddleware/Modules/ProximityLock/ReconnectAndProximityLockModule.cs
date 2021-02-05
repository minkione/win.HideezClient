using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.Hes.Messages;
using HideezMiddleware.ReconnectManager;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.ProximityLock
{
    public sealed class ReconnectAndProximityLockModule : ModuleBase
    {
        readonly DeviceReconnectManager _deviceReconnectManager;

        public ReconnectAndProximityLockModule(IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(ReconnectAndProximityLockModule), log)
        {
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            _messenger.Subscribe<HesAccessManager_AccessRetractedMessage>(HesAccessManager_AccessRetracted);
        }

        // Disable automatic reconnect when user is logged out or session is locked
        private void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            if (reason == SessionSwitchReason.SessionLogoff || reason == SessionSwitchReason.SessionLock)
            {
                _deviceReconnectManager.DisableAllDevicesReconnect();
            }
        }

        // Disable automatic reconnect when access is retracted
        private Task HesAccessManager_AccessRetracted(HesAccessManager_AccessRetractedMessage msg)
        {
            _deviceReconnectManager.DisableAllDevicesReconnect();

            return Task.CompletedTask;
        }
    }
}
