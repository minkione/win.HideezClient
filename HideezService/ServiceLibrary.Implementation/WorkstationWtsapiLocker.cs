using Hideez.SDK.Communication.Proximity;
using NLog;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ServiceLibrary.Implementation
{

    class WorkstationWtsapiLocker : IWorkstationLocker
    {
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSEnumerateSessions(IntPtr hServer, int Reserved, int Version, ref IntPtr ppSessionInfo, ref int pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
        System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);


        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        private enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        readonly Logger _log = LogManager.GetCurrentClassLogger();

        public void LockWorkstation()
        {
            IntPtr ppSessionInfo = IntPtr.Zero;
            IntPtr userPtr = IntPtr.Zero;
            IntPtr domainPtr = IntPtr.Zero;
            Int32 count = 0;
            Int32 retval = WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref ppSessionInfo, ref count);
            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            var currentSession = ppSessionInfo;
            uint bytes = 0;

            if (retval == 0)
                return;

            _log.Info("Query sessions");
            for (int i = 0; i < count; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                currentSession += dataSize;

                WTSQuerySessionInformation(IntPtr.Zero, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out bytes);
                WTSQuerySessionInformation(IntPtr.Zero, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out bytes);

                var domain = Marshal.PtrToStringAnsi(domainPtr);
                var userName = Marshal.PtrToStringAnsi(userPtr);
                var sessionFullName = domain + "\\" + userName;
                _log.Info("Session: " + sessionFullName);

                WTSFreeMemory(userPtr);
                WTSFreeMemory(domainPtr);

                // Note: it might be a good idea to limit session disconnects only to those activated by triggered device
                //if (sessionFullName == sessionTolock) 
                if (!string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(userName))
                {
                    _log.Info($"Session '{sessionFullName}' matches session to lock ' '");
                    if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                    {
                        _log.Info($"Session '{sessionFullName}' is active, disconnect initiated");
                        bool disconnected = WTSDisconnectSession(IntPtr.Zero, si.SessionID, false);
                        _log.Info($"Session disconnected: {disconnected}");
                    }
                }
            }
            WTSFreeMemory(ppSessionInfo);
        }

    }
}
