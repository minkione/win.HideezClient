using Hideez.SDK.Communication;
using Microsoft.Win32;
using NLog;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class SessionSwitchManager : IDisposable
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static SessionSwitchSubject subject = SessionSwitchSubject.NonHideez;
        private static string _deviceId;

        public SessionSwitchManager()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        public event EventHandler<WorkstationEvent> SessionSwitch;

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                SessionSwitchReason reason = e.Reason;
                if (reason >= SessionSwitchReason.SessionLock && reason <= SessionSwitchReason.SessionUnlock)
                {
                    WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                    workstationEvent.Severity = WorkstationEventSeverity.Ok;
                    workstationEvent.Note = subject.ToString();
                    workstationEvent.DeviceId = _deviceId;
                    subject = SessionSwitchSubject.NonHideez;
                    _deviceId = null;

                    switch (reason)
                    {
                        case SessionSwitchReason.SessionLock:
                            workstationEvent.EventId = WorkstationEventId.ComputerLock;
                            break;
                        case SessionSwitchReason.SessionLogoff:
                            workstationEvent.EventId = WorkstationEventId.ComputerLogoff;
                            break;
                        case SessionSwitchReason.SessionUnlock:
                            workstationEvent.EventId = WorkstationEventId.ComputerUnlock;
                            break;
                        case SessionSwitchReason.SessionLogon:
                            workstationEvent.EventId = WorkstationEventId.ComputerLogon;
                            break;
                    }


                    if (SessionSwitch != null)
                    {
                        var @event = SessionSwitch;
                        @event.Invoke(this, workstationEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public static void SetEventSubject(SessionSwitchSubject sessionSwitchSubject, string deviceId)
        {
            subject = sessionSwitchSubject;
            _deviceId = deviceId;

            Task.Run(async () =>
            {
                await Task.Delay(2_000);
                subject = SessionSwitchSubject.NonHideez;
                _deviceId = null;
            });
        }

        public void Dispose()
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SessionSwitch = null;
        }
    }
}
