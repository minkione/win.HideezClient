using Hideez.SDK.Communication;
using Microsoft.Win32;
using NLog;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public static class SessionSwitchManager
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static SessionSwitchSubject subject = SessionSwitchSubject.NonHideez;
        private static string _deviceSerialNo;
        public static event Action<WorkstationEvent> SessionSwitch;

        public static void SystemSessionSwitch(SessionSwitchReason reason)
        {
            try
            {
                if (reason >= SessionSwitchReason.SessionLogon && reason <= SessionSwitchReason.SessionUnlock)
                {
                    WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                    workstationEvent.Severity = WorkstationEventSeverity.Ok;
                    workstationEvent.Note = subject.ToString();
                    workstationEvent.DeviceId = _deviceSerialNo;
                    subject = SessionSwitchSubject.NonHideez;
                    _deviceSerialNo = null;

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
                        @event.Invoke(workstationEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public static void SetEventSubject(SessionSwitchSubject sessionSwitchSubject, string deviceSerialNo)
        {
            subject = sessionSwitchSubject;
            _deviceSerialNo = deviceSerialNo;

            Task.Run(async () =>
            {
                await Task.Delay(2_000);
                subject = SessionSwitchSubject.NonHideez;
                _deviceSerialNo = null;
            });
        }
    }
}
