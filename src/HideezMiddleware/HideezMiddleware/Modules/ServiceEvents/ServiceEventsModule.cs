using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.ServiceEvents.Messages;
using HideezMiddleware.Threading;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;

namespace HideezMiddleware.Modules.ServiceEvents
{
    public sealed class ServiceEventsModule : ModuleBase
    {
        static bool _restoringFromSleep = false;
        
        static bool _alreadyRestored = false;

        public ServiceEventsModule(IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(ServiceEventsModule), log)
        {
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            PowerEventMonitor.SystemQuerySuspend += PowerEventMonitor_SystemQuerySuspend;
            PowerEventMonitor.SystemSuspending += PowerEventMonitor_SystemSuspending;
            PowerEventMonitor.SystemLeftSuspendedMode += PowerEventMonitor_SystemLeftSuspendedMode;
        }

        private async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            await SafePublish(new SessionSwitchMonitor_SessionSwitchMessage(sessionId, reason));
        }

        private async void PowerEventMonitor_SystemQuerySuspend()
        {
            await SafePublish(new PowerEventMonitor_SystemQuerySuspendMessage());
        }

        private async void PowerEventMonitor_SystemSuspending()
        {
            WriteLine("System going into suspended mode");
            canRaiseLeftSuspendedMode = true;
            await SafePublish(new PowerEventMonitor_SystemSuspendingMessage());
        }

        /* 
         * Prevents multiple messages from being sent when multiple resumes from suspend happen 
         * within a short frame of each other due to inconsistent behavior caused by SystemPowerEvent implementation
         */
        readonly SemaphoreQueue leftSuspendedModeQueue = new SemaphoreQueue(1, 1);
        bool canRaiseLeftSuspendedMode = true;
        private async void PowerEventMonitor_SystemLeftSuspendedMode()
        {
            try
            {
                await leftSuspendedModeQueue.WaitAsync();
                if (canRaiseLeftSuspendedMode)
                {
                    WriteLine("System left suspended mode");
                    canRaiseLeftSuspendedMode = false;
                    await SafePublish(new PowerEventMonitor_SystemLeftSuspendedModeMessage());
                }
            }
            finally
            {
                leftSuspendedModeQueue.Release();
            }
        }
    }
}
