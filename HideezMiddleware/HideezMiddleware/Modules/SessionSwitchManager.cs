using Hideez.SDK.Communication;
using Microsoft.Win32;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;



namespace HideezMiddleware
{
    public static class SessionSwitchManager
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static SessionSwitchSubject subject = SessionSwitchSubject.NonHideez;
        private static string _deviceSerialNo;
        public static event Action<WorkstationEvent> SessionSwitch;

        static SessionSwitchManager()
        {
            try
            {
                UserSessionName = "SYSTEM";

                var explorer = Process.GetProcessesByName("explorer").FirstOrDefault();
                if (explorer != null)
                {
                    UserSessionName = GetUsername(explorer.SessionId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public static string UserSessionName { get; set; }

        public static void SystemSessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            try
            {
                if (reason == SessionSwitchReason.SessionLogon || reason == SessionSwitchReason.SessionUnlock)
                {
                    UserSessionName = GetUsername(sessionId);
                }

                if (reason >= SessionSwitchReason.SessionLogon && reason <= SessionSwitchReason.SessionUnlock)
                {
                    WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                    workstationEvent.Severity = WorkstationEventSeverity.Ok;
                    workstationEvent.Note = subject.ToString();
                    workstationEvent.DeviceId = _deviceSerialNo;
                    workstationEvent.UserSession = UserSessionName;
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

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private enum WtsInfoClass
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        private static string GetUsername(int sessionId)
        {
            IntPtr buffer;
            int strLen;
            string username = "SYSTEM";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
            }
            return username;
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
