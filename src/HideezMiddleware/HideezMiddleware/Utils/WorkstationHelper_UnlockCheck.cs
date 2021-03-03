using System;
using System.Runtime.InteropServices;

namespace HideezMiddleware
{
    public partial class WorkstationHelper
    {
        const Int32 FALSE = 0;

        static readonly IntPtr WTS_CURRENT_SERVER = IntPtr.Zero;

        const Int32 WTS_SESSIONSTATE_LOCK = 0;
        const Int32 WTS_SESSIONSTATE_UNLOCK = 1;

        static bool _is_win7 = (Environment.OSVersion.Platform == PlatformID.Win32NT && 
            Environment.OSVersion.Version.Major == 6 && 
            Environment.OSVersion.Version.Minor == 1);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        static extern Int32 WTSQuerySessionInformation(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] uint SessionId,
            [MarshalAs(UnmanagedType.U4)] WTS_INFO_CLASS WTSInfoClass,
            out IntPtr ppBuffer,
            [MarshalAs(UnmanagedType.U4)] out uint pBytesReturned
        );

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        static extern void WTSFreeMemoryEx(
            WTS_TYPE_CLASS WTSTypeClass,
            IntPtr pMemory,
            uint NumberOfEntries
        );

        enum WTS_INFO_CLASS
        {
            WTSInitialProgram = 0,
            WTSApplicationName = 1,
            WTSWorkingDirectory = 2,
            WTSOEMId = 3,
            WTSSessionId = 4,
            WTSUserName = 5,
            WTSWinStationName = 6,
            WTSDomainName = 7,
            WTSConnectState = 8,
            WTSClientBuildNumber = 9,
            WTSClientName = 10,
            WTSClientDirectory = 11,
            WTSClientProductId = 12,
            WTSClientHardwareId = 13,
            WTSClientAddress = 14,
            WTSClientDisplay = 15,
            WTSClientProtocolType = 16,
            WTSIdleTime = 17,
            WTSLogonTime = 18,
            WTSIncomingBytes = 19,
            WTSOutgoingBytes = 20,
            WTSIncomingFrames = 21,
            WTSOutgoingFrames = 22,
            WTSClientInfo = 23,
            WTSSessionInfo = 24,
            WTSSessionInfoEx = 25,
            WTSConfigInfo = 26,
            WTSValidationInfo = 27,
            WTSSessionAddressV4 = 28,
            WTSIsRemoteSession = 29
        }

        enum WTS_TYPE_CLASS
        {
            WTSTypeProcessInfoLevel0,
            WTSTypeProcessInfoLevel1,
            WTSTypeSessionInfoLevel1
        }

        enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive, // A user is logged on to the WinStation. This state occurs when a user is signed in and actively connected to the device.
            WTSConnected, // The WinStation is connected to the client.
            WTSConnectQuery, // The WinStation is in the process of connecting to the client.
            WTSShadow, // The WinStation is shadowing another WinStation.
            WTSDisconnected, // The WinStation is active but the client is disconnected. This state occurs when a user is signed in but not actively connected to the device, such as when the user has chosen to exit to the lock screen.
            WTSIdle, // The WinStation is waiting for a client to connect.
            WTSListen, // The WinStation is listening for a connection. A listener session waits for requests for new client connections. No user is logged on a listener session. A listener session cannot be reset, shadowed, or changed to a regular client session.
            WTSReset, // The WinStation is being reset.
            WTSDown, // The WinStation is down due to an error.
            WTSInit // The WinStation is initializing.
        }

        public enum LockState
        {
            Unknown,
            Locked,
            Unlocked
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX
        {
            public uint Level;
            public WTSINFOEX_LEVEL Data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX_LEVEL
        {
            public WTSINFOEX_LEVEL1 WTSInfoExLevel1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX_LEVEL1
        {
            public uint SessionId;
            public WTS_CONNECTSTATE_CLASS SessionState;
            public Int32 SessionFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string WinStationName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string UserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
            public string DomainName;
            public UInt64 LogonTime;
            public UInt64 ConnectTime;
            public UInt64 DisconnectTime;
            public UInt64 LastInputTime;
            public UInt64 CurrentTime;
            public Int32 IncomingBytes;
            public Int32 OutgoingBytes;
            public Int32 IncomingFrames;
            public Int32 OutgoingFrames;
            public Int32 IncomingCompressedBytes;
            public Int32 OutgoingCompressedBytes;
        }

        public static LockState GetSessionLockState(uint session_id)
        {

            Int32 queryResult = WTSQuerySessionInformation(
                WTS_CURRENT_SERVER,
                session_id,
                WTS_INFO_CLASS.WTSSessionInfoEx,
                out IntPtr ppBuffer,
                out uint pBytesReturned
            );

            if (queryResult == FALSE)
                return LockState.Unknown;

            var session_info_ex = Marshal.PtrToStructure<WTSINFOEX>(ppBuffer);

            if (session_info_ex.Level != 1)
            {
                WTSFreeMemory(ppBuffer);
                return LockState.Unknown;
            }

            var lock_state = session_info_ex.Data.WTSInfoExLevel1.SessionFlags;
            WTSFreeMemory(ppBuffer);

            LockState parsedLockState = LockState.Unknown;
            if (_is_win7)
            {
                /* Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/ee621019(v=vs.85).aspx
                    * Windows Server 2008 R2 and Windows 7:  Due to a code defect, the usage of the WTS_SESSIONSTATE_LOCK
                    * and WTS_SESSIONSTATE_UNLOCK flags is reversed. That is, WTS_SESSIONSTATE_LOCK indicates that the
                    * session is unlocked, and WTS_SESSIONSTATE_UNLOCK indicates the session is locked.
                    * */
                switch (lock_state)
                {
                    case WTS_SESSIONSTATE_LOCK:
                        parsedLockState = LockState.Unlocked;
                        break;
                    case WTS_SESSIONSTATE_UNLOCK:
                        parsedLockState = LockState.Locked;
                        break;
                    default:
                        parsedLockState = LockState.Unknown;
                        break;
                }
            }
            else
            {
                switch (lock_state)
                {
                    case WTS_SESSIONSTATE_LOCK:
                        parsedLockState = LockState.Locked;
                        break;
                    case WTS_SESSIONSTATE_UNLOCK:
                        parsedLockState = LockState.Unlocked;
                        break;
                    default:
                        parsedLockState = LockState.Unknown;
                        break;
                }
            }

            var sessionState = session_info_ex.Data.WTSInfoExLevel1.SessionState;

            
            // known issue: parsedLockState is returned as Unlocked for all local sessions, regardless of which session exactly is unlocked
            // Yet only one session at a time has a state of WTSActive
            // WTSINFOEX_LEVEL1 reference: https://msdn.microsoft.com/zh-cn/vstudio/ee621019(v=vs.90)
            if (sessionState == WTS_CONNECTSTATE_CLASS.WTSActive && parsedLockState == LockState.Unlocked)
                return LockState.Unlocked;
            else if (parsedLockState == LockState.Unknown)
                return LockState.Unknown;
            else
                return LockState.Locked;
        }

        public static bool IsSessionLocked(uint session_id)
        {
            return GetSessionLockState(session_id) == LockState.Locked;
        }

        public static LockState GetActiveSessionLockState()
        {
            var sid = GetSessionId();
            return GetSessionLockState(sid);
        }

        public static bool IsActiveSessionLocked()
        {
            return GetActiveSessionLockState() == LockState.Locked;
        }

        public static LockState GetCurrentSessionLockState()
        {
            var sid = (uint)System.Diagnostics.Process.GetCurrentProcess().SessionId;
            return GetSessionLockState(sid);
        }

        public static bool IsCurrentSessionLocked()
        {
            return GetCurrentSessionLockState() == LockState.Locked;
        }
    }
}
